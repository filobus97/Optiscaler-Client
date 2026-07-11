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
    /// "Bring your own DLL" import dialog for the custom FSR 4.x amdxcffx64.dll.
    /// The app never downloads or links to this DLL — the user must browse to a
    /// local file they already possess. The file is validated as a 64-bit PE, its
    /// version resource and SHA-256 are shown, and Authenticode signature presence
    /// is reported as info (never blocking).
    /// </summary>
    public partial class ImportFsr4DllDialog : Window
    {
        private readonly ComponentManagementService _componentService;
        private string? _selectedPath;
        private PeFileInfo? _selectedPeInfo;
        private bool _isAnimatingClose;

        /// <summary>Metadata of the imported DLL, set when the import succeeded.</summary>
        public CustomFsr4DllInfo? ImportedInfo { get; private set; }

        public ImportFsr4DllDialog() : this(new ComponentManagementService()) { }

        public ImportFsr4DllDialog(ComponentManagementService componentService)
        {
            InitializeComponent();
            DialogDimHelper.Register(this);
            _componentService = componentService;

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

        private async void BtnBrowse_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select amdxcffx64.dll",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("amdxcffx64.dll") { Patterns = new[] { ComponentManagementService.CustomFsr4DllName } },
                        new FilePickerFileType("DLL files") { Patterns = new[] { "*.dll" } },
                        new FilePickerFileType("All files") { Patterns = new[] { "*.*" } }
                    }
                });

                if (files == null || files.Count == 0) return;

                var path = files[0].Path.IsAbsoluteUri ? files[0].Path.LocalPath : files[0].TryGetLocalPath();
                if (string.IsNullOrEmpty(path)) return;

                await InspectSelectionAsync(path);
            }
            catch (Exception ex)
            {
                DebugWindow.Log($"[CustomFsr4] Browse failed: {ex.Message}");
            }
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
                ShowError("The selected DLL is not a 64-bit (x64) binary. OptiScaler requires the 64-bit amdxcffx64.dll.");
                return;
            }

            // Filename check: warn but allow — the file is renamed to amdxcffx64.dll on import.
            var fileName = Path.GetFileName(path);
            if (!fileName.Equals(ComponentManagementService.CustomFsr4DllName, StringComparison.OrdinalIgnoreCase))
            {
                if (pnlRename != null) pnlRename.IsVisible = true;
                var txtRename = this.FindControl<TextBlock>("TxtRenameWarning");
                if (txtRename != null)
                    txtRename.Text = $"The selected file is named '{fileName}', not '{ComponentManagementService.CustomFsr4DllName}'. " +
                                     "It will be renamed to amdxcffx64.dll when imported. Make sure this really is the FSR 4 driver DLL.";
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

            // FileVersion >= 2.3.0.0 is how OptiScaler detects INT8 (FSR 4.1.1) support;
            // the 4.0.2c-era DLLs report 2.2.x and lower.
            if (txtHint != null)
            {
                if (Version.TryParse(pe.FileVersion ?? "", out var v))
                {
                    txtHint.Text = v >= new Version(2, 3, 0, 0)
                        ? "File version 2.3.0.0 or newer — OptiScaler will treat this as an FSR 4.1.x (INT8-capable) DLL."
                        : "File version below 2.3.0.0 — OptiScaler will treat this as an older FSR 4.0.x DLL.";
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
            if (string.IsNullOrEmpty(_selectedPath) || _selectedPeInfo == null)
                return;

            var btnImport = this.FindControl<Button>("BtnImport");
            if (btnImport != null) btnImport.IsEnabled = false;

            try
            {
                ImportedInfo = await _componentService.ImportCustomFsr4DllAsync(_selectedPath);
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
