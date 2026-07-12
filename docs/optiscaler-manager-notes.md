# Notes toward `optiscaler-manager`

Input for a future, simpler, AMD-focused frontend. Written from a review of
OptiScaler Client Next as of v1.2.1.

## What the current app does well (reuse as-is)

The service layer is cleanly separated from the Avalonia UI and is the real
asset. A new frontend can reuse these `Services/` classes unchanged:

- **`ComponentManagementService`** — downloads/imports every component into a
  versioned cache (`Cache/<Component>/<version>/`), tracks release metadata,
  and owns the bring-your-own-DLL import + validation (`PeFileInspector`).
- **`GameInstallationService`** — the install/uninstall engine. Copies files
  next to the game exe, edits `OptiScaler.ini` (section-aware
  `ModifyOptiScalerIniKey`), and records every mutation in a manifest.
- **`BackupStoreService`** — external, per-game backup store keyed by a path
  slug, outside the game folder. This is what makes uninstall trustworthy.
- **Scanners** (`SteamScanner`, `EpicScanner`, …) and `GameAnalyzerService`.

## Design gaps found during review (fixed in v1.2.1)

- In-app updater pointed at the upstream repo and expected an asset name this
  fork never produces → retargeted to the fork + per-platform RID asset match,
  with a one-time config migration for existing installs.
- Quick Install ignored the two custom (bring-your-own) component defaults →
  now installs them, with SDK-vs-Extras mutual exclusion honoured.
- `UpdateStatus` re-scanned the game folder recursively on every refresh →
  now a single manifest read.

## Known limitations to carry into the redesign

- **Bulk Install** does not offer the custom (bring-your-own) components.
- The FSR4 selectors' mutual exclusion is enforced in the game manager, not in
  a shared model — a redesign should model "components that target the same
  file" as data, not per-screen glue.
- Component state on a `Game` is a growing set of `*Version` string fields.
  A `Dictionary<ComponentId, InstalledComponent>` would scale better.

## Proposed shape for `optiscaler-manager`

Guidelines: simple, lean, transparent, tooltip-rich, no black boxes.

- **One screen per game**: detected GPU + game → a single "Enable FSR 4" flow
  with sensible AMD defaults (OptiScaler latest + the user's custom DLL/SDK if
  imported). Advanced options collapsed by default.
- **A "what will happen" preview** before any install: list the exact files to
  be written and the exact `OptiScaler.ini` keys to be set, so the action is
  never a black box and is manually replicable.
- **Reuse the service layer verbatim**; the new project is a thin UI + a small
  "component registry" describing each component (target files, ini keys,
  conflicts) as data. That registry replaces the per-screen conditionals here.
- Keep the bring-your-own-DLL philosophy central: the app fills gaps OptiScaler
  leaves (slow updates, no custom DLLs in the official repo) without ever
  shipping or linking to proprietary binaries.
