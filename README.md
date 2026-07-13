# ✦ OptiScaler Client Next

[![GitHub Release](https://img.shields.io/github/v/release/filobus97/Optiscaler-Client?style=flat-square&color=8A2BE2)](https://github.com/filobus97/Optiscaler-Client/releases)
[![License: GPL-3.0-or-later](https://img.shields.io/badge/License-GPL--3.0--or--later-yellow.svg?style=flat-square)](LICENSE)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows-0078D4?style=flat-square&logo=windows)](https://www.microsoft.com/windows)
[![Platform: Linux](https://img.shields.io/badge/Platform-Linux-E95420?style=flat-square&logo=linux)](https://www.linux.org)

> **⚠️ Disclaimer:** This is **not** an official OptiScaler project. I am not affiliated with the OptiScaler team. This is a personal project developed without any commercial purpose. Anyone is free to try and use this software at their own risk.

> **🔱 Fork note:** **OptiScaler Client Next** is a fork of [Agustinm28/Optiscaler-Client](https://github.com/Agustinm28/Optiscaler-Client) (GPL-3.0-or-later) — the "Next" in the name marks it as unofficial and distinct from the original. It adds two **bring-your-own-DLL** features: the app can install a **user-supplied** `amdxcffx64.dll` (FSR 4.x driver/model DLL) and a **user-supplied** `amd_fidelityfx_upscaler_dx12.dll` (FSR SDK) into games. This repository contains **no AMD binaries and no download links to them** — you must source the DLLs yourself. It also adds GitHub Actions CI and cross-platform release builds. All upstream credit belongs to the original author and the OptiScaler team.

**OptiScaler Client Next** is a modern, high-performance desktop utility designed to simplify the installation, management, and update of the **OptiScaler** mod across your entire game library. Built with **C#** and **Avalonia UI**.

---

## Screenshots

* Main window

<img width="1920" height="1032" alt="1 0 4_A" src="https://github.com/user-attachments/assets/f39de984-a055-41ef-8900-3ee4e4317a68" />

* Game management

<img width="1140" height="600" alt="oc_01" src="https://github.com/user-attachments/assets/f8608451-d18e-410f-9df5-5e63a27e0e02" />

* Game management after installation

<img width="1140" height="621" alt="oc_02" src="https://github.com/user-attachments/assets/cd8e41fc-a6e0-4ca3-a035-ebc8fd4d32bb" />

---

## 🚀 Key Features

### Game Discovery

- **Multi-Platform Auto-Scanner** — Scans **Steam, Epic Games, GOG, EA, Ubisoft, Battle.net, and Xbox/Microsoft Store** libraries in parallel. On Linux, only Steam is scanned automatically.
- **Custom Folder Scanning** — Add any folder as a scan source for DRM-free or standalone games.
- **Manual Game Addition** — Add games by selecting the executable directly.
- **Drive Root Filtering** — Limit scanning to specific drives.
- **Smart Exclusions** — Pre-configured exclusions for non-game entries (e.g., Wallpaper Engine, Steamworks Redistributables).
- **Cover Art Fetching** — Automatically fetches game cover art from Steam API and SteamGridDB with local caching.

### Installation & Uninstallation

- **Quick Install / Uninstall** — One-click toggle per game directly from the main view. Automatically downloads components if not cached.
- **Auto Install** — Detects game directory structure automatically, including **UE5/Phoenix** game layouts.
- **Manual Install** — Select the target executable manually for non-standard game structures.
- **Bulk Install** — Install OptiScaler across multiple games at once with platform filtering, component selection, and profile application.
- **Injection Method Selection** — Choose the DLL injection method: `dxgi.dll`, `winmm.dll`, `d3d12.dll`, `dbghelp.dll`, `version.dll`, `wininet.dll`, `winhttp.dll`.
- **Backup & Restore** — Original game files are backed up before installation and restored on uninstall.

### Component Management

- **OptiScaler** — Core upscaling mod with stable and beta version channels.
- **Fakenvapi** — Compatibility layer for **AMD/Intel GPUs**, installed alongside OptiScaler when needed.
- **Nukem's DLSSG-to-FSR3** — Frame generation bridge that converts DLSS Frame Gen to FSR3.
- **FSR 4 INT8 Extras** — INT8 shader injection for non-RDNA 4 GPUs.
- **FSR 4.x Custom DLL (bring your own)** — *fork addition:* installs a **user-supplied** `amdxcffx64.dll` (e.g. a newer FSR 4.1.x INT8 build) next to the game executable, where OptiScaler loads it before the driver store. See [Custom FSR 4 DLLs](#-custom-fsr-4-dlls-bring-your-own) below.
- **FSR SDK Custom Package (bring your own)** — *fork addition:* imports a **user-supplied** FSR SDK from an extracted folder or archive (`amd_fidelityfx_upscaler_dx12.dll` + frame generation + companion DLLs) and installs the whole set, replacing the FSR SDK bundled with the installed OptiScaler release — run a newer FSR 4 SDK without waiting for an OptiScaler update. Mutually exclusive per game with the FSR 4 INT8 Extras (same upscaler file).
- **OptiPatcher** — ASI plugin loader, automatically configured with `LoadAsiPlugins=true` in OptiScaler.ini.

### Profiles

- **OptiScaler Profiles** — Create, edit, clone, and manage INI-based configuration profiles.
- **Easy Mode Editor** — Simple toggle-based interface for common settings.
- **Advanced Mode Editor** — Full section-based settings editor with search and sidebar navigation.
- **Default Profile** — Set a default profile that is applied automatically during Quick Install and Bulk Install.
- **Built-in Default** — "OptiScaler Standard" profile ships out-of-the-box with sensible defaults.

## Network & Proxy

- Supports system proxy settings and `HTTP_PROXY` / `HTTPS_PROXY` environment variables.
- Also supports explicit proxy configuration from app settings (including auth when required).
- Network settings are persisted in app configuration.

### Settings & Customization

- **Default Versions** — Configure default OptiScaler, Extras, and OptiPatcher versions for Quick Install.
- **Beta Channel Toggle** — Show or hide beta versions in all version selectors.
- **GPU Detection** — Automatically detects installed GPUs with platform-specific providers and discrete GPU preference logic.
- **Preferred GPU Selection** — Choose which GPU is used for installation decisions.
- **Scan Source Management** — Enable/disable per-platform scanners and configure custom folders.
- **Cache Management** — View and delete cached OptiScaler and Extras versions to free storage.
- **SteamGridDB Integration** — Optional API key for improved cover art fetching.
- **Clear Application Cache** — Full reset: delete all stored data (games, covers, config, analysis cache).

### UI & UX

- **List & Grid Views** — Switch between compact list and card-based grid layouts (preference saved).
- **Real-Time Search** — Filter games by name as you type.
- **Edit Mode** — Reorder games via drag-and-drop or arrow buttons; hide/show games.
- **Technology Badges** — Visual indicators showing detected DLSS, FSR, XeSS, DLSS Frame Gen versions.
- **Platform Badges** — Icons for each supported game platform.
- **Toast Notifications** — Non-blocking notifications with progress bars for downloads and operations.
- **Status Bar** — Footer with real-time operation feedback and GPU info.
- **Loading Overlays** — Animated indicators during scanning and startup checks.
- **Window State Persistence** — Window size, position, and maximized state are saved across sessions.
- **Configurable Animations** — UI transitions can be disabled in Settings for performance.

### Localization

Full interface translation in **14 languages**:

| Language | Language |
|---|---|
| 🇬🇧 English | 🇯🇵 Japanese |
| 🇪🇸 Spanish | 🇰🇷 Korean |
| 🇩🇪 German | 🇳🇱 Dutch |
| 🇫🇷 French | 🇵🇱 Polish |
| 🇮🇹 Italian | 🇷🇺 Russian |
| 🇧🇷 Portuguese (Brazil) | 🇹🇷 Turkish |
| 🇨🇳 Chinese (Simplified) | 🇹🇼 Chinese (Traditional) |

---

## 📖 Usage Guide

### Getting Started

1. **Find your games** — Click **"Scan Games"** to automatically detect installed titles from all supported platforms. You can manage scan sources or add custom folders in **Settings**. For standalone games, use **"Add Manually"**.
2. **Select a Game** — Click **"Manage"** next to any game, or use **Quick Install** for a one-click experience.
3. **Install OptiScaler** — From the Manage window, choose version, injection method, components, and profile, then click **"Auto Install"**. Or just hit **Quick Install** from the main view to install with your configured defaults.
4. **Bulk Install** — Use the **"Bulk Install"** button to install OptiScaler on multiple games simultaneously.
5. **Launch & Tweak** — Start your game normally. Press **`Insert`** to open the OptiScaler in-game menu and adjust upscaling settings in real-time.

### Profiles

1. Navigate to the **Profiles** tab in the sidebar.
2. Click **"New Profile"** to create a custom configuration.
3. Use **Easy Mode** for quick toggles or **Advanced Mode** for full INI control.
4. Set a default profile in **Settings → Manage Default Versions** so it's applied automatically during Quick Install.

### Uninstalling

- **Quick Uninstall** — Click the Quick Install button on any game that already has OptiScaler installed.
- **Manage → Uninstall** — Open the game management window and click **Uninstall**.
- Both methods will restore original game files from backup and clean up all OptiScaler artifacts.

---

## 🧩 Custom FSR 4 DLLs (bring your own)

> **This fork adds "bring-your-own-DLL" features for FSR 4.x.** The repository contains **no AMD binaries and no links to any**, and the app **never downloads** these DLLs. You must supply files you obtained yourself (for example from an AMD driver installation you own). They are AMD software; sourcing them is entirely your responsibility.

This fork lets you swap two independent pieces of the FSR 4 stack per game:

| Component | What it is | What you supply |
|---|---|---|
| **FSR4 Custom DLL** | The driver-side DLL holding the FSR 4 **ML models**. File version `2.3.x` = FSR 4.1.x INT8-capable (runs on RDNA 2/3). | A single `amdxcffx64.dll`. |
| **FSR4 Custom SDK** | The FSR **SDK** OptiScaler calls into — a *set* of DLLs, not just the upscaler. Each OptiScaler release bundles a copy of this SDK; importing your own overrides it so you can run a newer FSR release immediately, on any GPU. | The whole SDK, imported from its **`.zip`/`.7z` archive or an extracted folder**. |

An FSR SDK is a **package of DLLs**. The importer looks for all of these (the upscaler is required; the rest are imported when present):

`amd_fidelityfx_upscaler_dx12.dll` (required) · `amd_fidelityfx_framegeneration_dx12.dll` (frame generation) · `amd_fidelityfx_dx12.dll` · `amd_fidelityfx_loader_dx12.dll` · `amd_fidelityfx_denoiser_dx12.dll` · `amd_fidelityfx_radiancecache_dx12.dll` · `amd_fidelityfx_vk.dll`

Subfolders are searched, 32-bit copies are skipped, and if the same DLL appears more than once a copy from a `signed` path wins. Everything found is imported as **one versioned package** (labelled by the upscaler's file version) and installed together.

AMD ships FSR 4.1 officially for RDNA 3 (driver 26.6.2+), while RDNA 2 support is not expected before ~early 2027. OptiScaler, however, checks the **game folder first** for `amdxcffx64.dll` (since v0.7.7-pre9 / stable v0.7.8) before falling back to the driver store — so a newer INT8-capable DLL placed next to the game executable can enable FSR 4.1.x on older GPUs.

How to use them:

1. Open **Settings → Manage Cache**. For the model DLL choose **FSR4 Custom DLL** → **Import DLL…**; for the SDK choose **FSR4 Custom SDK** → **Import SDK (folder or archive)…**.
2. Select your source. The **Custom DLL** takes a single `amdxcffx64.dll`. The **Custom SDK** dialog offers two pickers — **Select archive…** (the SDK `.zip`/`.7z`) and **Select folder…** (an already-extracted SDK) — and imports every recognised DLL it finds (see the list above). The app validates each DLL, shows the upscaler's **file version**, **SHA-256**, and **Authenticode signature** presence plus the full list of DLLs it will import, then stores everything in the local component cache. Re-import newer builds at any time — versions are kept side by side.
3. In a game's **Manage** window, pick the imported version in the matching selector and install. The app backs up any existing files, copies the imported ones next to the game executable, and sets `[FSR] UpscalerIndex=0` / `Fsr4Update=true` in `OptiScaler.ini` so the FSR 4 backend is engaged on non-RDNA4 GPUs.
4. Uninstalling (or re-installing with the selector on **None**) restores backups — including putting back the SDK DLLs bundled with the installed OptiScaler release — and reverts the ini keys.

Notes:

- The **FSR4 Custom SDK** selector and the classic **FSR4 INT8** (Extras) selector install the *same file* — the game manager keeps them mutually exclusive and deselects one when you pick the other. With a 4.1.x `amdxcffx64.dll` the old INT8 Extras are normally unnecessary.
- If the selected OptiScaler version is older than **v0.7.8** (or nightly **0.7.7-pre9**), the app warns you to update OptiScaler first, since older builds do not load `amdxcffx64.dll` from the game folder.

---

## 🔍 Under the hood (no black boxes)

Everything this app does to a game folder is plain file operations you can replicate — or undo — by hand. Per component:

| Component | Files placed next to the game .exe | OptiScaler.ini keys set | On removal |
|---|---|---|---|
| OptiScaler | The OptiScaler package (main DLL renamed to your chosen injection name, e.g. `dxgi.dll`, plus its support files) | Written from your selected profile | Originals restored from backup, created files deleted |
| Fakenvapi | `nvapi64.dll`, `fakenvapi.ini` | — | Restored/deleted via manifest |
| NukemFG | `dlssg_to_fsr3_amd_is_better.dll` | `[General] FGType=nukems` | Restored/deleted via manifest |
| FSR4 INT8 (Extras) | `amd_fidelityfx_upscaler_dx12.dll` (community 4.0.2c INT8 build) | — | Cleaned on uninstall |
| FSR4 Custom DLL | your `amdxcffx64.dll` | `[FSR] UpscalerIndex=0`, `Fsr4Update=true` | Backup restored, keys reverted to `auto` |
| FSR4 Custom SDK | every DLL of your imported SDK package (upscaler + frame generation + companions) | `[FSR] UpscalerIndex=0`, `Fsr4Update=true` | Backups restored, OptiScaler's bundled DLLs put back, keys reverted |
| OptiPatcher | `plugins/OptiPatcher.asi` | `LoadAsiPlugins=true` | Removed on uninstall |

Bookkeeping lives outside the game folder, in the app's data directory (`%APPDATA%/OptiscalerClient` on Windows, `~/.config/OptiscalerClient` on Linux):

- `Backups/<game-slug>/manifest.json` — exactly which files were created vs overwritten, with SHA-256 hashes; `Backups/<game-slug>/files/` holds the pre-install originals.
- `Cache/<Component>/<version>/` — every downloaded or imported component version; imported DLL packages carry a `dll_info.json` with per-file versions, hashes, and signature info.

Every selector in the game manager has a "?" tooltip stating exactly what it will do. The two custom (bring-your-own) components are available everywhere OptiScaler is: per-game **Manage**, **Quick Install** (via configured defaults), and **Bulk Install**.

---

## 📦 Releases & CI (this fork)

All builds happen on GitHub Actions — nothing is built locally:

- **CI** (`.github/workflows/ci.yml`): every push/PR to `main` restores, builds (win-x64 + linux-x64), and runs the xUnit test suite (`tests/OptiscalerClient.Tests`).
- **Releases** (`.github/workflows/release.yml`): pushing a tag matching `v*` publishes self-contained single-file builds for `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64`, zips them as `OptiscalerClient-<version>-<rid>.zip`, and attaches them to an auto-created GitHub Release.

To cut a release:

```bash
git tag v1.0.6
git push origin v1.0.6
```

Then download the zips from the repository's **Releases** page. The Linux/macOS builds run the full UI; game-install paths remain Windows-oriented and are guarded on other platforms.

---

## 🛠️ Installation & Requirements

### Platform Support

- Windows
- Linux

### Instructions

1. Download the latest release asset from [Releases](https://github.com/Agustinm28/Optiscaler-Client/releases).
2. Extract the package.
3. Run `OptiscalerClient.exe`.

### Notes

- The app is self-contained, so no external .NET runtime installation is required.
- On Linux, automatic scanner sources are focused on Steam libraries.
- Manual add/install flows currently target executable files (`.exe`) for game selection.

---

## 🛡️ Security & Antivirus False Positives

**Is this software safe?**

Yes, OptiScaler Client is completely safe and open-source. However, some antivirus programs may flag it as suspicious due to **false positive detections**.

### Why does this happen?

- **File Downloads**: The app downloads `.zip` and `.dll` files from GitHub (OptiScaler, Fakenvapi, NukemFG)
- **Heuristic Detection**: Antivirus software may flag download behavior as "potentially unwanted"
- **Unsigned Binary**: The executable is not digitally signed (code signing certificates cost $100-300/year)

### Common False Positives

- **Zillya**: `Downloader.MLoki.Win64.10` — Known for aggressive heuristics
- **Other AVs**: May show generic "downloader" or "trojan" warnings

### What you can do

1. **Verify the Source**: Download only from official [GitHub Releases](https://github.com/Agustinm28/Optiscaler-Client/releases)
2. **Check VirusTotal**: Upload the file to [VirusTotal.com](https://www.virustotal.com) — most reputable AVs will show clean
3. **Review the Code**: This is open-source — you can inspect all code before running
4. **Add Exception**: Whitelist `OptiscalerClient.exe` in your antivirus settings

### Transparency

All downloads are from official sources:
- OptiScaler: `github.com/optiscaler/OptiScaler`
- Fakenvapi: `github.com/optiscaler/fakenvapi`
- NukemFG: `github.com/Nukem9/dlssg-to-fsr3`

The application **never** collects personal data, connects to third-party servers, or performs any malicious actions. All source code is available for audit.

---

## 🤝 Contributing

We welcome contributions! If you'd like to improve OptiScaler Client:

1. **Fork** the project.
2. Create your **Feature Branch** (`git checkout -b feature/AmazingFeature`).
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`).
4. **Push** to the branch (`git push origin feature/AmazingFeature`).
5. Open a **Pull Request**.

---

## 📄 License & Acknowledgments

### License

**OptiScaler Client** is free software: you can redistribute it and/or modify it under the terms of the **GNU General Public License** as published by the Free Software Foundation, either **version 3** of the License, or (at your option) **any later version**.

This program is distributed in the hope that it will be useful, but **WITHOUT ANY WARRANTY**; without even the implied warranty of **MERCHANTABILITY** or **FITNESS FOR A PARTICULAR PURPOSE**. See the [GNU General Public License](LICENSE) for more details.

**Copyright (C) 2026 Agustín Montaña (Agustinm28)**

### Acknowledgments & Third-Party Software

- **Special thanks and deep respect to the OptiScaler development team** for creating and maintaining this incredible software that enhances gaming experiences for countless users worldwide.
- **[OptiScaler](https://github.com/optiscaler/OptiScaler)**: The core upscaling technology that makes this possible.
- **[fakenvapi](https://github.com/optiscaler/fakenvapi)**: Essential compatibility layer developed by the OptiScaler team.
- **[OptiPatcher](https://github.com/optiscaler/OptiPatcher)**: ASI plugin loader by the OptiScaler team.
- **[NukemFG (DLSSG-to-FSR3)](https://github.com/Nukem9/dlssg-to-fsr3)**: Frame Generation bridge by Nukem.

This client application is merely a frontend interface to help users more easily manage and install the amazing work done by the OptiScaler team and other contributors. While OptiScaler Client itself is licensed under GPL-3.0-or-later, the third-party components it downloads and manages may be subject to their own respective licenses.

---

<p align="center">
  Developed with ❤️
</p>
