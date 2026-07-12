// OptiScaler Client - A frontend for managing OptiScaler installations
// Copyright (C) 2026 Agustín Montaña (Agustinm28)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using OptiscalerClient.Helpers;
using OptiscalerClient.Models;
using OptiscalerClient.Services;

namespace OptiscalerClient.Views
{
    /// <summary>
    /// Which user-supplied FSR DLL this dialog imports.
    /// </summary>
    public enum CustomDllKind
    {
        /// <summary>amdxcffx64.dll — the driver-side DLL containing the FSR 4 ML models.</summary>
        Fsr4ModelDll,
        /// <summary>amd_fidelityfx_upscaler_dx12.dll — the FSR SDK upscaler DLL.</summary>
        FsrSdkDll,
    }

    /// <summary>
    /// "Bring your own DLL" import dialog for the custom FSR DLLs (amdxcffx64.dll
    /// or amd_fidelityfx_upscaler_dx12.dll). The app never downloads or links to
    /// these DLLs — the user must browse to a local file they already possess. The
    /// file is validated as a 64-bit PE, its version resource and SHA-256 are shown,
    /// and Authenticode signature presence is reported as info (never blocking).
    /// </summary>
    public partial class ImportFsr4DllDialog : Window
    {
        private readonly ComponentManagementService _componentService;
        private readonly CustomDllKind _kind;
        private string? _selectedPath;
        private PeFileInfo? _selectedPeInfo;
        private ComponentManagementService.FsrSdkScanResult? _sdkScan;
        private bool _isAnimatingClose;

        private string ExpectedDllName => _kind == CustomDllKind.FsrSdkDll
            ? ComponentManagementService.CustomFsrSdkDllName
            : ComponentManagementService.CustomFsr4DllName;

        /// <summary>Metadata of the imported DLL, set when the import succeeded.</summary>
        public CustomFsr4DllInfo? ImportedInfo { get; private set; }

        public ImportFsr4DllDialog() : this(new ComponentManagementService()) { }

        public ImportFsr4DllDialog(ComponentManagementService componentService, CustomDllKind kind = CustomDllKind.Fsr4ModelDll)
        {
            InitializeComponent();
            DialogDimHelper.Register(this);
            _componentService = componentService;
            _kind = kind;
            ApplyKindTexts();

            this.Opacity = 0;

            var titleBar = this.FindControl<Border>("TitleBar");
            if (titleBar != null)
                titleBar.PointerPressed += (s, e) => this.BeginMoveDrag(e);

            this.Opened += (s, e) =>
            {
                this.Opacity = 1;
                var rootPanel = this.FindControl<Panel>("RootPanel");
                if (rootPanel != null)
                {
                    AnimationHelper.SetupPanelTransition(rootPanel);
                    rootPanel.Opacity = 1;
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>Adjusts titles and instructions to the DLL kind being imported.</summary>
        private void ApplyKindTexts()
        {
            if (_kind != CustomDllKind.FsrSdkDll)
                return;

            var title = this.FindControl<TextBlock>("TxtDialogTitle");
            var subtitle = this.FindControl<TextBlock>("TxtDialogSubtitle");
            var instruction = this.FindControl<TextBlock>("TxtInstruction");
            var legal = this.FindControl<TextBlock>("TxtLegalNotice");

            if (legal != null)
                legal.Text = "This tool does not provide the DLLs. You must supply files you obtained yourself. " +
                             "The FSR SDK DLLs are AMD software and are never downloaded, bundled, or linked by this application.";
            if (title != null) title.Text = "Import Custom FSR SDK";
            if (subtitle != null) subtitle.Text = "FSR SDK package (upscaler + companion DLLs)";
            if (instruction != null)
                instruction.Text = "Point to an extracted FSR SDK folder, the SDK archive (.zip/.7z), or a single DLL. " +
                                   "All known FSR SDK DLLs found inside (upscaler, frame generation, and companions — " +
                                   "subfolders included) are imported as one package and installed together, replacing " +
                                   "the copies bundled with your OptiScaler release. amd_fidelityfx_upscaler_dx12.dll " +
                                   "is required; the rest are optional. Note: the downloadable \"FSR4 INT8\" component " +
                                   "installs the same upscaler file — use one or the other per game, not both.";

            var btnFolder = this.FindControl<Button>("BtnBrowseFolder");
            if (btnFolder != null) btnFolder.IsVisible = true;
        }

        private async void BtnBrowse_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var filters = _kind == CustomDllKind.FsrSdkDll
                    ? new[]
                    {
                        new FilePickerFileType("FSR SDK (archive or DLL)") { Patterns = new[] { "*.zip", "*.7z", "*.dll" } },
                        new FilePickerFileType("Archives") { Patterns = new[] { "*.zip", "*.7z", "*.rar" } },
                        new FilePickerFileType("DLL files") { Patterns = new[] { "*.dll" } },
                        new FilePickerFileType("All files") { Patterns = new[] { "*.*" } }
                    }
                    : new[]
                    {
                        new FilePickerFileType(ExpectedDllName) { Patterns = new[] { ExpectedDllName } },
                        new FilePickerFileType("DLL files") { Patterns = new[] { "*.dll" } },
                        new FilePickerFileType("All files") { Patterns = new[] { "*.*" } }
                    };

                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = _kind == CustomDllKind.FsrSdkDll ? "Select FSR SDK archive or DLL" : $"Select {ExpectedDllName}",
                    AllowMultiple = false,
                    FileTypeFilter = filters
                });

                if (files == null || files.Count == 0) return;

                var path = files[0].Path.IsAbsoluteUri ? files[0].Path.LocalPath : files[0].TryGetLocalPath();
                if (string.IsNullOrEmpty(path)) return;

                if (_kind == CustomDllKind.FsrSdkDll)
                    await InspectSdkSourceAsync(path);
                else
                    await InspectSelectionAsync(path);
            }
            catch (Exception ex)
            {
                DebugWindow.Log($"[CustomFsr4] Browse failed: {ex.Message}");
            }
        }

        private async void BtnBrowseFolder_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select extracted FSR SDK folder",
                    AllowMultiple = false
                });

                if (folders == null || folders.Count == 0) return;

                var path = folders[0].Path.IsAbsoluteUri ? folders[0].Path.LocalPath : folders[0].TryGetLocalPath();
                if (string.IsNullOrEmpty(path)) return;

                await InspectSdkSourceAsync(path);
            }
            catch (Exception ex)
            {
                DebugWindow.Log($"[CustomFsrSdk] Folder browse failed: {ex.Message}");
            }
        }

        /// <summary>
        /// SDK mode: scans the selected folder/archive/DLL for the FSR SDK DLL set
        /// and previews what would be imported.
        /// </summary>
        private async Task InspectSdkSourceAsync(string path)
        {
            var txtPath = this.FindControl<TextBox>("TxtSelectedPath");
            var pnlInfo = this.FindControl<Border>("PnlDllInfo");
            var pnlError = this.FindControl<Border>("PnlError");
            var pnlRename = this.FindControl<Border>("PnlRenameWarning");
            var btnImport = this.FindControl<Button>("BtnImport");

            if (txtPath != null) txtPath.Text = path;
            if (pnlInfo != null) pnlInfo.IsVisible = false;
            if (pnlError != null) pnlError.IsVisible = false;
            if (pnlRename != null) pnlRename.IsVisible = false;
            if (btnImport != null) btnImport.IsEnabled = false;

            _sdkScan?.Cleanup();
            _sdkScan = null;
            _selectedPath = null;
            _selectedPeInfo = null;

            ComponentManagementService.FsrSdkScanResult scan;
            try
            {
                scan = await _componentService.ScanFsrSdkSourceAsync(path);
            }
            catch (Exception ex)
            {
                ShowError($"Could not read the selected source: {ex.Message}");
                return;
            }

            if (!scan.HasUpscaler)
            {
                scan.Cleanup();
                ShowError($"No 64-bit {ComponentManagementService.CustomFsrSdkDllName} was found in the selected source. " +
                          "The upscaler DLL is required — make sure you picked the right folder or archive.");
                return;
            }

            // Rename note for a single DLL whose name didn't match the upscaler
            if (File.Exists(path) && path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                var fileName = Path.GetFileName(path);
                if (!fileName.Equals(ComponentManagementService.CustomFsrSdkDllName, StringComparison.OrdinalIgnoreCase) &&
                    !Array.Exists(ComponentManagementService.FsrSdkDllNames, n => n.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (pnlRename != null) pnlRename.IsVisible = true;
                    var txtRename = this.FindControl<TextBlock>("TxtRenameWarning");
                    if (txtRename != null)
                        txtRename.Text = $"The selected file is named '{fileName}'. It will be imported as " +
                                         $"{ComponentManagementService.CustomFsrSdkDllName} — make sure this really is the FSR SDK upscaler.";
                }
            }

            var pe = scan.UpscalerPe!;
            var txtFileVer = this.FindControl<TextBlock>("TxtFileVersion");
            var txtProdVer = this.FindControl<TextBlock>("TxtProductVersion");
            var txtSig = this.FindControl<TextBlock>("TxtSignature");
            var txtSha = this.FindControl<TextBlock>("TxtSha256");
            var txtHint = this.FindControl<TextBlock>("TxtModelHint");
            var txtIncluded = this.FindControl<TextBlock>("TxtIncludedFiles");

            if (txtFileVer != null) txtFileVer.Text = pe.FileVersion ?? "unknown (no version resource)";
            if (txtProdVer != null) txtProdVer.Text = pe.ProductVersion ?? "unknown";
            if (txtSig != null)
                txtSig.Text = pe.HasAuthenticodeSignature
                    ? "Signed (Authenticode signature present — not validated)"
                    : "Unsigned (no Authenticode signature found)";

            string sha256 = "";
            try
            {
                var upscalerPath = scan.FoundFiles[ComponentManagementService.CustomFsrSdkDllName];
                sha256 = await Task.Run(() =>
                {
                    using var sha = System.Security.Cryptography.SHA256.Create();
                    using var stream = File.OpenRead(upscalerPath);
                    return Convert.ToHexString(sha.ComputeHash(stream));
                });
            }
            catch (Exception ex)
            {
                DebugWindow.Log($"[CustomFsrSdk] SHA-256 failed: {ex.Message}");
            }
            if (txtSha != null) txtSha.Text = string.IsNullOrEmpty(sha256) ? "unavailable" : sha256;

            if (txtIncluded != null)
            {
                txtIncluded.Text = $"DLLs found ({scan.FoundFiles.Count}): " + string.Join(", ", scan.FoundFiles.Keys);
                txtIncluded.IsVisible = true;
            }

            if (txtHint != null)
            {
                if (Version.TryParse(pe.FileVersion ?? "", out var v))
                {
                    txtHint.Text = v >= new Version(4, 1, 1, 0)
                        ? "Upscaler file version 4.1.1.0 or newer — OptiScaler will treat this SDK as FSR 4.1.x (INT8-capable)."
                        : "Upscaler file version below 4.1.1.0 — OptiScaler will treat this as an older FSR 4.0.x-era SDK.";
                }
                else
                {
                    txtHint.Text = "Upscaler version could not be detected; OptiScaler decides INT8 support from it at runtime.";
                }
            }

            if (pnlInfo != null) pnlInfo.IsVisible = true;

            _sdkScan = scan;
            _selectedPath = path;
            if (btnImport != null) btnImport.IsEnabled = true;
        }

        private async Task InspectSelectionAsync(string path)
        {
            var txtPath = this.FindControl<TextBox>("TxtSelectedPath");
            var pnlInfo = this.FindControl<Border>("PnlDllInfo");
            var pnlError = this.FindControl<Border>("PnlError");
            var pnlRename = this.FindControl<Border>("PnlRenameWarning");
            var btnImport = this.FindControl<Button>("BtnImport");

            if (txtPath != null) txtPath.Text = path;
            if (pnlInfo != null) pnlInfo.IsVisible = false;
            if (pnlError != null) pnlError.IsVisible = false;
            if (pnlRename != null) pnlRename.IsVisible = false;
            if (btnImport != null) btnImport.IsEnabled = false;

            _selectedPath = null;
            _selectedPeInfo = null;

            PeFileInfo pe;
            try
            {
                pe = await Task.Run(() => PeFileInspector.Inspect(path));
            }
            catch (Exception ex)
            {
                ShowError($"Could not read the selected file: {ex.Message}");
                return;
            }

            if (!pe.IsValidPe)
            {
                ShowError("The selected file is not a valid Windows DLL (missing PE header).");
                return;
            }

            if (!pe.Is64Bit)
            {
                ShowError($"The selected DLL is not a 64-bit (x64) binary. OptiScaler requires the 64-bit {ExpectedDllName}.");
                return;
            }

            // Filename check: warn but allow — the file is renamed on import.
            var fileName = Path.GetFileName(path);
            if (!fileName.Equals(ExpectedDllName, StringComparison.OrdinalIgnoreCase))
            {
                if (pnlRename != null) pnlRename.IsVisible = true;
                var txtRename = this.FindControl<TextBlock>("TxtRenameWarning");
                if (txtRename != null)
                    txtRename.Text = $"The selected file is named '{fileName}', not '{ExpectedDllName}'. " +
                                     $"It will be renamed to {ExpectedDllName} when imported. Make sure this really is the right DLL.";
            }

            // Populate the info card
            var txtFileVer = this.FindControl<TextBlock>("TxtFileVersion");
            var txtProdVer = this.FindControl<TextBlock>("TxtProductVersion");
            var txtSig = this.FindControl<TextBlock>("TxtSignature");
            var txtSha = this.FindControl<TextBlock>("TxtSha256");
            var txtHint = this.FindControl<TextBlock>("TxtModelHint");

            if (txtFileVer != null) txtFileVer.Text = pe.FileVersion ?? "unknown (no version resource)";
            if (txtProdVer != null) txtProdVer.Text = pe.ProductVersion ?? "unknown";
            if (txtSig != null)
                txtSig.Text = pe.HasAuthenticodeSignature
                    ? "Signed (Authenticode signature present — not validated)"
                    : "Unsigned (no Authenticode signature found)";

            string sha256 = "";
            try
            {
                sha256 = await Task.Run(() =>
                {
                    using var sha = System.Security.Cryptography.SHA256.Create();
                    using var stream = File.OpenRead(path);
                    return Convert.ToHexString(sha.ComputeHash(stream));
                });
            }
            catch (Exception ex)
            {
                DebugWindow.Log($"[CustomFsr4] SHA-256 failed: {ex.Message}");
            }
            if (txtSha != null) txtSha.Text = string.IsNullOrEmpty(sha256) ? "unavailable" : sha256;

            // INT8 (FSR 4.1.1) detection thresholds used by OptiScaler:
            //  - amdxcffx64.dll: FileVersion >= 2.3.0.0 (4.0.2c-era DLLs report 2.2.x)
            //  - amd_fidelityfx_upscaler_dx12.dll (SDK): FileVersion >= 4.1.1.0
            if (txtHint != null)
            {
                var threshold = _kind == CustomDllKind.FsrSdkDll ? new Version(4, 1, 1, 0) : new Version(2, 3, 0, 0);
                if (Version.TryParse(pe.FileVersion ?? "", out var v))
                {
                    txtHint.Text = v >= threshold
                        ? $"File version {threshold} or newer — OptiScaler will treat this as an FSR 4.1.x (INT8-capable) DLL."
                        : $"File version below {threshold} — OptiScaler will treat this as an older FSR 4.0.x-era DLL.";
                }
                else
                {
                    txtHint.Text = "Version could not be detected; OptiScaler decides INT8 support from the DLL's file version at runtime.";
                }
            }

            if (pnlInfo != null) pnlInfo.IsVisible = true;

            _selectedPath = path;
            _selectedPeInfo = pe;
            if (btnImport != null) btnImport.IsEnabled = true;
        }

        private void ShowError(string message)
        {
            var pnlError = this.FindControl<Border>("PnlError");
            var txtError = this.FindControl<TextBlock>("TxtError");
            if (txtError != null) txtError.Text = message;
            if (pnlError != null) pnlError.IsVisible = true;
        }

        private async void BtnImport_Click(object? sender, RoutedEventArgs e)
        {
            bool ready = _kind == CustomDllKind.FsrSdkDll
                ? _sdkScan is { HasUpscaler: true }
                : !string.IsNullOrEmpty(_selectedPath) && _selectedPeInfo != null;
            if (!ready)
                return;

            var btnImport = this.FindControl<Button>("BtnImport");
            if (btnImport != null) btnImport.IsEnabled = false;

            try
            {
                ImportedInfo = _kind == CustomDllKind.FsrSdkDll
                    ? await _componentService.ImportCustomFsrSdkPackageAsync(_sdkScan!)
                    : await _componentService.ImportCustomFsr4DllAsync(_selectedPath!);
                _sdkScan?.Cleanup();
                _sdkScan = null;
                _ = CloseAnimated();
            }
            catch (Exception ex)
            {
                ShowError($"Import failed: {ex.Message}");
                if (btnImport != null) btnImport.IsEnabled = true;
            }
        }

        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
        {
            ImportedInfo = null;
            _sdkScan?.Cleanup();
            _sdkScan = null;
            _ = CloseAnimated();
        }

        private async Task CloseAnimated()
        {
            if (_isAnimatingClose) return;
            _isAnimatingClose = true;
            DialogDimHelper.HideDimNow(this);
            var rootPanel = this.FindControl<Panel>("RootPanel");
            if (rootPanel != null) rootPanel.Opacity = 0;
            await Task.Delay(220);
            Close(ImportedInfo);
        }
    }
}
