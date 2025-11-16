# Developer Documentation

## Windows Edge Light - Developer Guide

This document provides technical information for developers who want to build, modify, or contribute to Windows Edge Light.

---

## Table of Contents

- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Building Locally](#building-locally)
- [Architecture](#architecture)
- [Publishing](#publishing)
- [GitHub Actions CI/CD](#github-actions-cicd)
- [Version Management](#version-management)
- [Technical Limitations](#technical-limitations)
- [Contributing](#contributing)

---

## Prerequisites

### Required
- **Windows 10 or later**
- **.NET 10.0 SDK** - [Download from Microsoft](https://dotnet.microsoft.com/download/dotnet/10.0)
- **PowerShell 5.1+** (for build scripts)

### Optional
- **Visual Studio 2022** (with .NET desktop development workload)
- **Git** for version control
- **GitHub CLI** (`gh`) for release management

---

## Project Structure

```
WindowsEdgeLight/
├── .github/
│   └── workflows/
│       └── build.yml           # GitHub Actions workflow
├── WindowsEdgeLight/
│   ├── App.xaml                # Application entry point
│   ├── App.xaml.cs
│   ├── MainWindow.xaml         # Main UI layout
│   ├── MainWindow.xaml.cs      # Application logic
│   ├── AssemblyInfo.cs
│   ├── ringlight_cropped.ico   # Application icon
│   └── WindowsEdgeLight.csproj # Project file
├── .gitignore
├── build.ps1                   # Local build script
├── README.md                   # User documentation
└── DEVELOPER.md               # This file
```

---

## Building Locally

### Quick Build (Debug)

```bash
cd WindowsEdgeLight
dotnet build
dotnet run
```

### Production Build (Single Platform)

For x64:
```bash
dotnet publish -c Release /p:DebugType=None /p:DebugSymbols=false -r win-x64
```

For ARM64:
```bash
dotnet publish -c Release /p:DebugType=None /p:DebugSymbols=false -r win-arm64
```

### Build Both Platforms with Script

Use the included build script to create both x64 and ARM64 versions:

```powershell
.\build.ps1
```

**Options:**
```powershell
.\build.ps1 -Configuration Release -Version "0.5"
```

**Output:**
- `publish/WindowsEdgeLight-v0.4-win-x64.exe` (~72 MB)
- `publish/WindowsEdgeLight-v0.4-win-arm64.exe` (~68 MB)

The script will:
1. Clean previous builds
2. Build x64 version
3. Build ARM64 version
4. Copy executables to `./publish/` with version naming
5. Display file sizes and summary

---

## Architecture

### Technology Stack

- **Framework**: .NET 10.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Additional APIs**: Windows Forms (for NotifyIcon and Screen detection)
- **Language**: C# 12

### Key Components

#### 1. Main Window (`MainWindow.xaml/.cs`)
- Transparent, click-through overlay window
- Uses Win32 `WS_EX_TRANSPARENT` flag for click-through
- Positioned and sized using `Screen.PrimaryScreen` API
- DPI-aware scaling using `PresentationSource`
- Temperature-to-RGB color conversion using scientific formulas
- Settings persistence via `Properties.Settings`

#### 2. Control Window (`ControlWindow.xaml/.cs`)
- Separate clickable window for user controls
- Temperature slider (2700K - 6500K range)
- Three preset buttons with save/load functionality
- Brightness controls
- Toggle and exit buttons
- Positioned at bottom center of main window

#### 3. Global Hotkeys
- Registered using Win32 `RegisterHotKey` API
- Hooks into Windows message pump via `HwndSource`
- Supported hotkeys:
  - `Ctrl+Shift+L` - Toggle light
  - `Ctrl+Shift+Up` - Increase brightness
  - `Ctrl+Shift+Down` - Decrease brightness

#### 4. System Tray Icon
- Uses Windows Forms `NotifyIcon`
- Right-click context menu for all operations including presets
- Double-click shows help dialog
- Icon loaded from file or falls back to system icon

#### 5. Edge Light Effect
- XAML Path with LinearGradientBrush
- DropShadowEffect for glow appearance
- Adjustable opacity (20% - 100%)
- Adjustable color temperature (2700K - 6500K)
- Dynamic gradient based on temperature setting

#### 6. Settings Management
- Uses `Properties.Settings` for persistence
- Stores temperature, brightness, and three presets
- Automatically saves on change
- Automatically loads on startup
- XML-based configuration in user's AppData folder

### Design Patterns

- **Event-driven architecture**: Responds to hotkeys and user interactions
- **Separation of concerns**: XAML for UI, C# for logic
- **Defensive programming**: Try-catch blocks, null checks, fallbacks
- **Settings pattern**: Centralized configuration management

---

## Publishing

### Project Configuration

The project is configured for single-file, self-contained publishing:

```xml
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>true</SelfContained>
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
<EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
```

### Why Single-File?

- **User convenience**: One file to download and run
- **No dependencies**: Includes .NET runtime
- **Portable**: Can be run from any location

### Build Artifacts

| Platform | File Size | Description |
|----------|-----------|-------------|
| win-x64  | ~72 MB    | Intel/AMD 64-bit Windows |
| win-arm64| ~68 MB    | ARM64 Windows (Surface Pro X, etc.) |

---

## GitHub Actions CI/CD

### Workflow File

Located at `.github/workflows/build.yml`

### Triggers

1. **Tag Push**: Automatically builds and releases when you push a version tag
   ```bash
   git tag v0.5
   git push origin v0.5
   ```

2. **Manual Dispatch**: Can be triggered manually from GitHub Actions tab with custom version

### Workflow Steps

1. **Checkout** - Clone the repository
2. **Setup .NET** - Install .NET 10.0 SDK
3. **Restore** - Restore NuGet packages
4. **Build x64** - Compile and publish x64 version
5. **Build ARM64** - Compile and publish ARM64 version
6. **Prepare Artifacts** - Copy executables with version naming
7. **Upload Artifacts** - Make available as workflow artifacts
8. **Create Release** - Automatically create GitHub release (only on tag push)

### Release Automation

When a tag is pushed, the workflow:
- Builds both platforms
- Creates a GitHub release with the tag name
- Uploads both executables as release assets
- Generates release notes with features and download links

---

## Version Management

### Updating Version

Edit `WindowsEdgeLight.csproj`:

```xml
<AssemblyVersion>0.5.0.0</AssemblyVersion>
<FileVersion>0.5.0.0</FileVersion>
<Version>0.5</Version>
```

### Creating a Release

1. **Update version** in `.csproj` file
2. **Commit changes**:
   ```bash
   git add -A
   git commit -m "Bump version to 0.5"
   git push
   ```

3. **Create and push tag**:
   ```bash
   git tag v0.5
   git push origin v0.5
   ```

4. **GitHub Actions** will automatically build and create the release

### Manual Release (Alternative)

If you prefer to build locally and create releases manually:

```bash
# Build both versions
.\build.ps1 -Version "0.5"

# Create release with GitHub CLI
gh release create v0.5 `
  --title "Windows Edge Light v0.5" `
  --notes "Release notes here..." `
  publish\WindowsEdgeLight-v0.5-win-x64.exe `
  publish\WindowsEdgeLight-v0.5-win-arm64.exe
```

---

## Technical Limitations

### WPF Constraints

#### No Native AOT
WPF applications **cannot** use Native AOT compilation because:
- Heavy use of reflection
- XAML parsing at runtime
- Dynamic resource loading

#### Limited Trimming
WPF and Windows Forms don't support aggressive IL trimming:
- Windows Forms: Errors with trimming enabled
- WPF: Requires many assemblies at runtime

**Result**: Single-file executables are ~70MB (includes full .NET runtime)

### Alternative Considered

A pure Win32/WinUI3 implementation could achieve:
- Native AOT compilation
- ~5-10MB executable size
- Faster startup time

However, WPF was chosen for:
- Rapid development
- Rich XAML support
- Mature ecosystem
- Built-in transparency and effects

---

## Code Guidelines

### Key Files to Modify

#### Adding Features
- **MainWindow.xaml.cs** - Add logic and event handlers
- **MainWindow.xaml** - Modify UI layout (rarely needed with separate control window)
- **ControlWindow.xaml** - Modify control panel layout
- **ControlWindow.xaml.cs** - Add control panel event handlers
- **Settings.settings** - Add new configuration properties

#### Changing Appearance
- **MainWindow.xaml** - Modify gradient, colors, blur
- Look for `EdgeLightBorder` Path element
- Temperature-to-color conversion in `MainWindow.xaml.cs`

#### Updating Hotkeys
- **MainWindow.xaml.cs** - Modify `RegisterHotKey` calls and `HwndHook` switch statement

#### Settings Management
- **Settings.settings** - Add new settings (auto-generates Settings.settings.cs)
- Use `Settings.Default.PropertyName` to access/modify
- Call `Settings.Default.Save()` to persist changes

#### Icon Changes
- Replace `ringlight_cropped.ico`
- Update `ApplicationIcon` in `.csproj`

### Coding Standards

- Use C# 12 features where appropriate
- Follow Microsoft naming conventions
- Add XML documentation for public APIs
- Keep methods focused and small
- Use nullable reference types
- Handle errors gracefully with fallbacks

### Testing

Currently no automated tests. Manual testing checklist:

- [ ] Edge light displays on primary monitor
- [ ] Respects taskbar area
- [ ] All hotkeys work
- [ ] System tray menu works
- [ ] Brightness controls function
- [ ] Temperature slider adjusts color correctly
- [ ] Temperature range (2700K - 6500K) works
- [ ] Preset save functionality (Ctrl+Click)
- [ ] Preset load functionality (Click)
- [ ] Settings persist after restart
- [ ] Toggle on/off works
- [ ] Can close from tray/taskbar
- [ ] Works on different DPI settings
- [ ] Works in multi-monitor setup

---

## Debugging

### Visual Studio
1. Open `WindowsEdgeLight.sln` in Visual Studio
2. Set breakpoints as needed
3. Press F5 to debug

### VS Code
1. Install C# extension
2. Open folder in VS Code
3. Use Debug panel with .NET Core Launch configuration

### Common Issues

#### Icon Not Loading
- Check `ringlight_cropped.ico` exists
- Verify `CopyToOutputDirectory` is set to `PreserveNewest`
- Check fallback to `SystemIcons.Application` works

#### Window Not Visible
- Check `ShowInTaskbar` property
- Verify `NotifyIcon.Visible = true`
- Look in hidden tray icons (click ^ arrow)

#### Hotkeys Not Working
- Check if another app uses same combination
- Verify `RegisterHotKey` returns true
- Check Windows message handling in `HwndHook`

#### DPI Scaling Issues
- Verify `PresentationSource.FromVisual(this)` is not null
- Called in `Window_Loaded`, not constructor
- Check `AppContext.BaseDirectory` for file paths

---

## Contributing

### Pull Request Process

1. **Fork** the repository
2. **Create a feature branch**: `git checkout -b feature/my-feature`
3. **Make changes** and test thoroughly
4. **Commit** with clear messages: `git commit -m "Add feature: description"`
5. **Push** to your fork: `git push origin feature/my-feature`
6. **Create Pull Request** on GitHub

### What to Include

- Clear description of changes
- Screenshots/GIFs for UI changes
- Update README.md if needed
- Update version in DEVELOPER.md
- Test on both x64 and ARM64 if possible

### Code Review

- Maintainer will review within a few days
- Address any feedback
- Once approved, will be merged to main
- New release will be created as needed

---

## Resources

### Documentation
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [.NET 10 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)

### Tools
- [.NET SDK Download](https://dotnet.microsoft.com/download)
- [Visual Studio Download](https://visualstudio.microsoft.com/)
- [GitHub CLI](https://cli.github.com/)

### Related Projects
- [Hue Sync](https://www.philips-hue.com/en-us/entertainment/hue-sync) - Commercial alternative
- [Ambilight DIY Projects](https://github.com/topics/ambilight) - Hardware-based solutions

---

## Support

### Issues
Report bugs or request features on [GitHub Issues](https://github.com/shanselman/WindowsEdgeLight/issues)

### Discussions
Ask questions in [GitHub Discussions](https://github.com/shanselman/WindowsEdgeLight/discussions)

### Contact
Created by [Scott Hanselman](https://github.com/shanselman)

---

## License

This project is provided as-is for personal and educational use.

---

*Last Updated: November 14, 2025*
