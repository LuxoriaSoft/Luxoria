# Luxoria Desktop Deployments & CI/CD

## Overview
This document describes how the Luxoria Desktop application (WinUI 3) is built, validated, signed, and released through GitHub Actions. It covers:
- Desktop application build process
- GitHub Actions CI/CD workflow
- Code signing for Windows SmartScreen
- Release creation and distribution
- Quality gates and testing

---

## Desktop Deployment Flow

```
┌──────────────────────────────────────────────────────────────────┐
│  Developer Creates Feature Branch                                │
│  (feat/*, fix/*, or chore/*)                                     │
└─────────────────────────┬────────────────────────────────────────┘
                          │
                          ↓
        ┌─────────────────────────────────┐
        │  Push to Remote & Create PR     │
        │  ✓ Commit Linting               │
        │  ✓ Unit Tests (xUnit)           │
        │  ✓ Code Quality (SonarCloud)    │
        │  ✓ Build Verification           │
        │  ✗ NO RELEASE                   │
        └──────────┬──────────────────────┘
                   │ PR Approved
                   ↓
    ┌──────────────────────────────────┐
    │  Merge to DEVELOP Branch         │
    │  ✓ Semantic Release              │
    │  ✓ Generate changelog            │
    │  ✓ Create git tag (v1.2.0)       │
    │  ✓ Mark as pre-release           │
    └──────────┬───────────────────────┘
               │
               ↓
    ┌─────────────────────────────────────┐
    │  Manual Testing on Windows          │
    │  ✓ QA validation                    │
    │  ✓ User acceptance testing          │
    │  ✓ Performance benchmarks           │
    └──────────┬──────────────────────────┘
               │ Release Sign-Off
               ↓
    ┌────────────────────────────────────┐
    │  Create Release PR (develop→main)  │
    │  Update version & changelog        │
    └──────────┬─────────────────────────┘
               │ PR Review & Approve
               ↓
    ┌──────────────────────────────────┐
    │  Merge to MAIN Branch            │
    │  ✓ Semantic Release (stable)     │
    │  ✓ Generate release notes        │
    │  ✓ Create git tag (v1.2.0)       │
    └──────────┬───────────────────────┘
               │ Tag triggers GitHub Actions
               ↓
    ┌────────────────────────────────────┐
    │  GitHub Actions Workflow           │
    │  ✓ Build for x86, x64, ARM64       │
    │  ✓ Run all unit tests              │
    │  ✓ Sign binaries with certificate  │
    │  ✓ Create Inno Setup installer     │
    │  ✓ Create portable ZIP             │
    │  ✓ Upload to GitHub Release        │
    └──────────┬─────────────────────────┘
               │
               ↓
    ┌────────────────────────────────────┐
    │  Release Published                 │
    │  ✓ GitHub Release with assets      │
    │  ✓ luxoria.bluepelicansoft.com     │
    │  ✓ WinGet package manager          │
    └────────────────────────────────────┘
```

---

## Branching & Release Model

### Git Workflow for Desktop

```
┌─────────────────────────────────────────────────────────┐
│  Feature/Fix Branch (Desktop-specific)                  │
│  (feat/*, fix/*, chore/*)                               │
│  ↓ Conventional Commits (commitlint + Husky)            │
│  ↓ Push to origin/feat-xyz                              │
└────────────────┬────────────────────────────────────────┘
                 │ Create Pull Request
                 ↓
        ┌────────────────────────┐
        │  CI Checks             │
        │  ✓ .NET Build          │
        │  ✓ xUnit Tests         │
        │  ✓ SonarCloud          │
        │  ✓ Code analyzers      │
        └────────────────────────┘
                 │ Approved by reviewers
                 ↓
        ┌────────────────────────────────┐
        │  Merge to DEVELOP Branch       │
        └────────┬───────────────────────┘
                 │ Push to origin/develop
                 ↓
    ┌────────────────────────────────────────┐
    │  Semantic Release: PRE-RELEASE         │
    │  • Detect conventional commits         │
    │  • Calculate next version              │
    │  • Generate CHANGELOG                  │
    │  • Create git tag (v1.2.0)             │
    │  • Mark as pre-release in GitHub       │
    └────────┬───────────────────────────────┘
             │ Manual QA & Testing Period
             ↓
    ┌────────────────────────────────────────┐
    │  Create Release PR (develop → main)    │
    │  • Update CHANGELOG.md                 │
    │  • Update version in .csproj           │
    └────────┬───────────────────────────────┘
             │ PR Review & Approve
             ↓
    ┌────────────────────────────────────────┐
    │  Merge to MAIN Branch                  │
    │  ✓ Semantic Release (stable)           │
    │  ✓ Create git tag (v1.2.0)             │
    │  ✓ Trigger GitHub Actions              │
    └────────────────────────────────────────┘
```

**Key Points:**
- **Branches**: Short-lived `feat/*` and `fix/*` merge into `develop`
- **Semantic Release**: Runs on `develop` (marked as pre-release) and on `main` (marked as stable), both use same version format (e.g., `1.2.0`)
- **Release Trigger**: GitHub Actions workflow triggers on tag push (v*)
- **Testing**: Manual QA and automated tests before stable release

---

## Desktop Application Architecture

### Luxoria Desktop (WinUI 3) - Build Pipeline

```
Source Code (.NET 9, WinUI 3)
│
├─ Projects Structure
│  ├─ Luxoria.App (Main WinUI application)
│  ├─ Luxoria.Core (Business logic)
│  ├─ Luxoria.Modules (Module system)
│  ├─ Luxoria.GModules (UI components)
│  └─ Luxoria.SDK (SDK for extensions)
│
├─ Test Projects
│  ├─ Luxoria.Core.Tests
│  ├─ Luxoria.Modules.Tests
│  └─ Luxoria.SDK.Tests
│
├─ Build per Runtime Identifier (RID)
│  ├─ win-x86 (32-bit Windows)
│  ├─ win-x64 (64-bit Windows) ← Primary
│  └─ win-arm64 (ARM processors)
│
├─ dotnet publish
│  ├─ Framework: net9.0-windows10.0.26100.0
│  ├─ Configuration: Release
│  ├─ PublishSingleFile: true
│  ├─ SelfContained: true
│  └─ Output: Self-contained, single-file executable
│
├─ Code Signing
│  ├─ Certificate: LuxoriaSoft Code Signing Cert
│  ├─ Signature: SmartScreen-compliant
│  ├─ Tools: signtool.exe (Windows SDK)
│  └─ Timestamp: DigiCert authority
│
├─ Package as Installer
│  ├─ Tool: Inno Setup (iscc.exe)
│  ├─ Format: .exe installer
│  ├─ Portable: .zip (optional)
│  ├─ Registry keys: Uninstall info
│  └─ Shortcuts: Desktop & Start Menu
│
└─ Distribution Channels
   ├─ GitHub Releases (automatic)
   ├─ luxoria.bluepelicansoft.com (mirror)
   └─ Windows Package Manager (WinGet)
```

**Build Details:**
- **Target Framework**: `net9.0-windows10.0.26100.0` (Windows 11 SDK)
- **Self-Contained**: Includes .NET runtime (no installation required)
- **Single File**: All dependencies bundled into one executable
- **Signing**: Required for Windows SmartScreen compatibility

---

## GitHub Actions Workflow

### Complete Desktop Release Pipeline

```
┌─────────────────────────────────────────────────────────────────┐
│                  GitHub Actions: Desktop Release                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  TRIGGER: New Release Tag (v*.*.*)                              │
│           (automatically created by semantic-release on main)   │
│                                                                 │
│  RUNNER: windows-latest (Microsoft-hosted)                      │
│                                                                 │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 1: Checkout Repository                  │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • actions/checkout@v4                        │               │
│  │ • Fetch full git history with tags           │               │
│  │ • Set working directory to Luxoria.App       │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 2: Setup .NET Runtime                   │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • actions/setup-dotnet@v4                    │               │
│  │ • Install .NET 9.0.x SDK                     │               │
│  │ • Verify: dotnet --version                   │               │
│  │ • Add Windows 11 SDK (26100.0)               │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 3: Restore Dependencies                 │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • dotnet restore Luxoria.App.sln             │               │
│  │ • Fetch all NuGet packages                   │               │
│  │ • Restore project references                 │               │
│  │ • Cache for faster subsequent builds         │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 4: Compile Release Build                │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • dotnet build -c Release --no-restore       │               │
│  │ • Compile all projects in solution           │               │
│  │ • Generate optimized binaries                │               │
│  │ • Apply .NET analyzers                       │               │
│  │ • Fail on warnings (TreatWarningsAsErrors)   │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 5: Run Unit Tests                       │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • dotnet test -c Release --no-build          │               │
│  │ • Run Luxoria.Core.Tests (xUnit)             │               │
│  │ • Run Luxoria.Modules.Tests (xUnit)          │               │
│  │ • Run Luxoria.SDK.Tests (xUnit)              │               │
│  │ • Generate test results (TRX format)         │               │
│  │ • Fail build if any test fails               │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 6: Publish for Each RID                 │               │
│  ├──────────────────────────────────────────────┤               │
│  │ For win-x64 (Primary):                       │               │
│  │  dotnet publish \                            │               │
│  │    Luxoria.App/Luxoria.App.csproj \          │               │
│  │    -c Release \                              │               │
│  │    -r win-x64 \                              │               │
│  │    -p:PublishSingleFile=true \               │               │
│  │    -p:SelfContained=true \                   │               │
│  │    -p:PublishTrimmed=false \                 │               │
│  │    --output ./publish/x64                    │               │
│  │                                              │               │
│  │ For win-x86 (32-bit):                        │               │
│  │  dotnet publish ... -r win-x86 \             │               │
│  │    --output ./publish/x86                    │               │
│  │                                              │               │
│  │ For win-arm64 (ARM):                         │               │
│  │  dotnet publish ... -r win-arm64 \           │               │
│  │    --output ./publish/arm64                  │               │
│  │                                              │               │
│  │ Output: Self-contained, single-file exes     │               │
│  │  • Luxoria.App.exe (each architecture)       │               │
│  │  • All dependencies embedded                 │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 7: Sign Binaries & Installer            │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • Load cert from $CODE_SIGN_CERT (base64)    │               │
│  │ • Decode to .pfx file                        │               │
│  │ • Run signtool.exe (Windows SDK)             │               │
│  │   └─ Sign: publish/x64/Luxoria.App.exe       │               │
│  │   └─ Sign: publish/x86/Luxoria.App.exe       │               │
│  │   └─ Sign: publish/arm64/Luxoria.App.exe     │               │
│  │ • Timestamp authority: DigiCert              │               │
│  │   └─ Ensures validity after cert expiry      │               │
│  │ • Verify signatures: signtool verify /pa     │               │
│  │ • Check SmartScreen compatibility            │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 8: Build Inno Setup Installer           │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • iscc.exe installer/installer.iss           │               │
│  │ • Package signed binaries                    │               │
│  │ • Include all architectures (x86/x64/ARM64)  │               │
│  │ • Auto-detect architecture at install time   │               │
│  │ • Generate installer:                        │               │
│  │   └─ LuxoriaSetup-{version}.exe              │               │
│  │ • Add uninstall registry entries             │               │
│  │ • Create Start Menu shortcuts                │               │
│  │ • Create Desktop shortcut (optional)         │               │
│  │ • Sign installer executable                  │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 9: Create Portable ZIP                  │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • 7z a (7-Zip archiver)                      │               │
│  │ • Compress all signed binaries:              │               │
│  │   └─ Luxoria-Portable-{version}.zip          │               │
│  │ • Include:                                   │               │
│  │   └─ x64/Luxoria.App.exe                     │               │
│  │   └─ x86/Luxoria.App.exe                     │               │
│  │   └─ arm64/Luxoria.App.exe                   │               │
│  │   └─ README.txt (usage instructions)         │               │
│  │ • No installer required (extract & run)      │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 10: Create GitHub Release               │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • Use softprops/action-gh-release@v2         │               │
│  │ • Upload artifacts:                          │               │
│  │   └─ LuxoriaSetup-{version}.exe (installer)  │               │
│  │   └─ Luxoria-Portable-{version}.zip          │               │
│  │ • Auto-generate release notes:               │               │
│  │   └─ Pull from CHANGELOG.md                  │               │
│  │   └─ List all commits since last release     │               │
│  │ • Mark as "Latest Release"                   │               │
│  │ • Publicly available for download            │               │
│  │ • Tag: v{version}                            │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 11: Optional - Publish to Distribution  │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • Mirror to luxoria.bluepelicansoft.com      │               │
│  │ • Update download links on website           │               │
│  │ • Invalidate CDN cache                       │               │
│  │ • Notify WinGet package manager              │               │
│  │ • Update WinGet manifest repository          │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓ [WORKFLOW COMPLETE ✓]                               │
│                                                                 │
│  Release is now publicly available:                             │
│  • GitHub Releases page                                         │
│  • luxoria.bluepelicansoft.com                                  │
│  • Windows Package Manager (WinGet)                             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## GitHub Actions Workflow YAML

### Complete Desktop Release Workflow

```yaml
name: desktop-release
on:
   push:
      tags:
         - 'v*'

jobs:
   build:
      runs-on: windows-latest
      
      steps:
         - name: Checkout repository
           uses: actions/checkout@v4
           with:
              fetch-depth: 0  # Full history for changelog
         
         - name: Setup .NET
           uses: actions/setup-dotnet@v4
           with:
              dotnet-version: '9.0.x'
         
         - name: Restore dependencies
           run: dotnet restore Luxoria.App/Luxoria.App.sln
         
         - name: Build Release
           run: dotnet build Luxoria.App/Luxoria.App.sln -c Release --no-restore
         
         - name: Run Unit Tests
           run: dotnet test Luxoria.App/Luxoria.App.sln -c Release --no-build --verbosity normal
         
         - name: Publish for x64
           run: >
              dotnet publish Luxoria.App/Luxoria.App/Luxoria.App.csproj 
              -c Release
              -r win-x64
              -p:PublishSingleFile=true
              -p:SelfContained=true
              -p:PublishTrimmed=false
              --output ./publish/x64
         
         - name: Publish for x86
           run: >
              dotnet publish Luxoria.App/Luxoria.App/Luxoria.App.csproj
              -c Release
              -r win-x86
              -p:PublishSingleFile=true
              -p:SelfContained=true
              -p:PublishTrimmed=false
              --output ./publish/x86
         
         - name: Publish for ARM64
           run: >
              dotnet publish Luxoria.App/Luxoria.App/Luxoria.App.csproj
              -c Release
              -r win-arm64
              -p:PublishSingleFile=true
              -p:SelfContained=true
              -p:PublishTrimmed=false
              --output ./publish/arm64
         
         - name: Sign binaries
           env:
              PFX_BASE64: ${{ secrets.CODE_SIGN_CERT }}
              PFX_PASSWORD: ${{ secrets.CODE_SIGN_PASSWORD }}
           run: |
              # Decode certificate
              $pfxBytes = [System.Convert]::FromBase64String($env:PFX_BASE64)
              [IO.File]::WriteAllBytes("cert.pfx", $pfxBytes)
              
              # Sign executables
              & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe" sign `
                 /f cert.pfx `
                 /p $env:PFX_PASSWORD `
                 /tr http://timestamp.digicert.com `
                 /td sha256 `
                 /fd sha256 `
                 .\publish\x64\Luxoria.App.exe
              
              & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe" sign `
                 /f cert.pfx `
                 /p $env:PFX_PASSWORD `
                 /tr http://timestamp.digicert.com `
                 /td sha256 `
                 /fd sha256 `
                 .\publish\x86\Luxoria.App.exe
              
              & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe" sign `
                 /f cert.pfx `
                 /p $env:PFX_PASSWORD `
                 /tr http://timestamp.digicert.com `
                 /td sha256 `
                 /fd sha256 `
                 .\publish\arm64\Luxoria.App.exe
              
              # Verify signatures
              & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe" verify /pa .\publish\x64\Luxoria.App.exe
              
              # Clean up certificate
              Remove-Item cert.pfx
         
         - name: Build Inno Setup Installer
           run: |
              iscc installer\installer.iss /DVERSION=${{ github.ref_name }}
         
         - name: Sign Installer
           env:
              PFX_BASE64: ${{ secrets.CODE_SIGN_CERT }}
              PFX_PASSWORD: ${{ secrets.CODE_SIGN_PASSWORD }}
           run: |
              # Decode certificate
              $pfxBytes = [System.Convert]::FromBase64String($env:PFX_BASE64)
              [IO.File]::WriteAllBytes("cert.pfx", $pfxBytes)
              
              # Sign installer
              & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe" sign `
                 /f cert.pfx `
                 /p $env:PFX_PASSWORD `
                 /tr http://timestamp.digicert.com `
                 /td sha256 `
                 /fd sha256 `
                 .\installer\Output\LuxoriaSetup-${{ github.ref_name }}.exe
              
              # Clean up certificate
              Remove-Item cert.pfx
         
         - name: Create Portable ZIP
           run: |
              7z a Luxoria-Portable-${{ github.ref_name }}.zip `
                 .\publish\x64\Luxoria.App.exe `
                 .\publish\x86\Luxoria.App.exe `
                 .\publish\arm64\Luxoria.App.exe `
                 README.md
         
         - name: Create GitHub Release
           uses: softprops/action-gh-release@v2
           with:
              files: |
                 installer\Output\LuxoriaSetup-${{ github.ref_name }}.exe
                 Luxoria-Portable-${{ github.ref_name }}.zip
              body_path: CHANGELOG.md
              draft: false
              prerelease: false
              generate_release_notes: true
```

---

## Quality Gates & Testing

### Desktop Testing Strategy

```
   Desktop Test Coverage:
   
   Unit Tests (Base): 75-85% coverage
   ├─ Luxoria.Core.Tests
   │  ├─ Business logic validation
   │  ├─ Data models & serialization
   │  └─ Service implementations
   ├─ Luxoria.Modules.Tests
   │  ├─ Module loader functionality
   │  ├─ Event bus communication
   │  └─ Module context management
   └─ Luxoria.SDK.Tests
      ├─ Logger service validation
      └─ SDK interface contracts
   
   Integration Tests (Middle): 15-20% coverage
   ├─ Module system integration
   ├─ File system operations
   └─ Inter-module communication
   
   Manual QA (Top): 5-10% coverage
   ├─ UI/UX validation
   ├─ Installation testing
   ├─ Update mechanisms
   └─ Performance benchmarks
```

**Test Execution:**
```bash
# Run all tests
dotnet test Luxoria.App/Luxoria.App.sln -c Release

# Run specific test project
dotnet test Luxoria.App/Luxoria.Core.Tests/Luxoria.Core.Tests.csproj

# Generate code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

---

## Local Build Instructions

### Building Desktop Locally

```bash
# 1. Clone repository
git clone https://github.com/LuxoriaSoft/Luxoria.git
cd Luxoria

# 2. Restore dependencies
dotnet restore Luxoria.App/Luxoria.App.sln

# 3. Build solution
dotnet build Luxoria.App/Luxoria.App.sln -c Release

# 4. Run tests
dotnet test Luxoria.App/Luxoria.App.sln -c Release --no-build

# 5. Publish for x64 (primary)
dotnet publish Luxoria.App/Luxoria.App/Luxoria.App.csproj \
  -c Release \
  -r win-x64 \
  -p:PublishSingleFile=true \
  -p:SelfContained=true \
  --output ./publish/x64

# 6. Run the application
./publish/x64/Luxoria.App.exe
```

### Creating Installer Locally

**Prerequisites:**
- Inno Setup installed
- Code signing certificate (for signed builds)
- Windows 10/11 with .NET 9 SDK

**Build Steps:**
```bash
# 1. Build all architectures
pwsh ./build.ps1

# 2. Sign binaries (if you have certificate)
pwsh ./scripts/sign.ps1

# 3. Build installer
iscc installer/installer.iss

# Output: installer/Output/LuxoriaSetup.exe
```

---

## Distribution & Release

### Release Channels

**Installation Methods:**

1. **GitHub Releases (Manual Download)**
   ```
   Visit: https://github.com/LuxoriaSoft/Luxoria/releases/latest
   Download: LuxoriaSetup-v{version}.exe
   Run installer
   ```

2. **Windows Package Manager (WinGet)**
   ```powershell
   winget install LuxoriaSoft.Luxoria
   ```

3. **Portable Version**
   ```
   Download: Luxoria-Portable-v{version}.zip
   Extract to any folder
   Run: Luxoria.App.exe
   ```

---

## Troubleshooting & Common Issues

### Build Failures

**Issue: .NET SDK Not Found**
```bash
# Check installed SDKs
dotnet --list-sdks

# Install .NET 9 SDK
winget install Microsoft.DotNet.SDK.9
```

**Issue: Missing Windows SDK**
```bash
# Install Windows 11 SDK
winget install Microsoft.WindowsSDK.10.0.26100
```

**Issue: Test Failures**
```bash
# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~ModuleLoaderTests"
```

### Signing Issues

**Issue: Certificate Not Found**
- Verify `CODE_SIGN_CERT` secret is set in GitHub
- Ensure certificate is base64-encoded
- Check certificate expiration date

**Issue: Timestamp Server Unreachable**
- Use alternative timestamp server: `http://timestamp.comodoca.com`
- Add retry logic in signing script
- Check network connectivity

**Issue: SmartScreen Still Shows Warning**
- EV certificates provide immediate reputation
- OV certificates require reputation building (time + downloads)
- Submit binary to Microsoft for analysis

---

## Required Secrets & Configuration

### GitHub Actions Secrets

```
Desktop Release Secrets:
├─ CODE_SIGN_CERT
│  └─ Base64-encoded .pfx certificate
│     Example: [Convert]::ToBase64String([IO.File]::ReadAllBytes("cert.pfx"))
│
└─ CODE_SIGN_PASSWORD
   └─ Password for code signing certificate
      Security: Stored encrypted in GitHub Secrets
```

---

## Version Management

### Semantic Versioning

```
Version Format: vMAJOR.MINOR.PATCH

Examples:
├─ v1.0.0  (Initial release)
├─ v1.1.0  (New features)
├─ v1.1.1  (Bug fixes)
├─ v1.2.0  (Pre-release - marked in GitHub)
└─ v2.0.0  (Breaking changes)

Note: Pre-release vs Stable is determined by GitHub release flags,
not by version format. Both use the same semantic versioning.

Automatic Bumping (Conventional Commits):
├─ feat:     → Minor version bump
├─ fix:      → Patch version bump
├─ BREAKING: → Major version bump
└─ chore:    → No version change
```

**Version Sources:**
- Git tags (v*.*.*)
- AssemblyInfo.cs (auto-updated by semantic-release)
- Package manifests (.csproj)

---

## Release Checklist

### Pre-Release Validation

- [ ] All unit tests passing
- [ ] No compiler warnings
- [ ] SonarCloud quality gate passed
- [ ] Manual QA completed
- [ ] CHANGELOG.md updated
- [ ] Version number correct
- [ ] Code signing certificate valid
- [ ] GitHub Actions workflow tested

### Post-Release Verification

- [ ] GitHub Release created successfully
- [ ] Installer downloads and installs
- [ ] Application launches without errors
- [ ] Update mechanism works
- [ ] Website download links updated
- [ ] WinGet manifest submitted
- [ ] Release notes published

---

**Document Version**: 1.0  
**Last Updated**: January 2026  
**Owner**: Desktop Team  
**Status**: Production Ready
