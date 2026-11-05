# Crux brand

<img src="logo.png" alt="Crux logo" width="128" height="128">

## Name

- Product name: **Crux**. Always written with a capital C and lowercase rest. Never "CRUX" or "crux" in running text.
- Crux is Latin for "cross" and also means "the core of something".
- Use "Crux Crosshair" only where the name alone is ambiguous, for example in repository names, store listings or search keywords.
- Debug builds are called "Crux Dev".
- The app was called ND Crosshair up to version 1.2. Mention the old name only where it helps users find their way, for example in the migration notes.

## Tagline

- English: "Crosshair overlay for Windows"
- German: "Fadenkreuz-Overlay für Windows"

## Identifiers

| Where | Value |
|---|---|
| Code namespaces and projects | `Crux.App`, `Crux.Core`, `Crux.Core.Tests` |
| Executable | `Crux.exe` |
| AppData folder, autostart value, mutex, Velopack pack id | `CruxCrosshair` (debug: `CruxCrosshair.Dev`) |
| Repository | `nowaaak/crux-crosshair` |
| Share codes | `CRX3-` (classic), `CRX4-` (layers). `NDX1-` to `NDX4-` are read for compatibility. |

## Logo

Four arms around an open center. Each arm ends in a point that aims at the center.

- Source of truth: `LogoGeometry` in `src/Crux.App/Theme.xaml`, drawn on a 64 by 64 grid.
- The app icon puts the geometry on a rounded square (corner radius 14 on the 64 grid).
- Keep the open center. Do not add a center dot. It disappears at 16 px.
- Do not rotate, outline or recolor the arms outside the colors below.

## Colors

| Use | Color |
|---|---|
| Logo arms (release) | `#60CDFF` (same as the app accent) |
| Logo arms (debug) | `#16A34A` |
| Logo background | `#171717` (same as the sidebar) |

## Regenerating the icons

Run this from the repository root after changing `LogoGeometry`:

```powershell
dotnet run tools/IconGen.cs
```

It writes `src/Crux.App/Assets/app.ico`, `src/Crux.App/Assets/app-dev.ico` (16 to 256 px) and `docs/logo.png` (256 px).
