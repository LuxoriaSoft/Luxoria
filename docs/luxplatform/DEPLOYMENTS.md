# LuxStudio Web Platform Deployments & CI/CD

## Overview
This document describes how the LuxStudio Web Platform (LuxAPI + Portal) is built, validated, and deployed to Kubernetes environments using CircleCI. It covers:
- Web platform deployment targets (LuxAPI, Portal, PostgreSQL, MinIO)
- Kubernetes environments (Pluto dev/staging, Saturn production)
- CircleCI CI/CD workflows and container builds
- Code quality and release gates
- Security, secrets management, and Kubernetes operations

> **Note**: For Luxoria Desktop (WinUI 3) deployments using GitHub Actions, see [Desktop Deployments](../desktop/DEPLOYMENTS.md).

---

## LuxStudio Web Platform Deployment Flow

```
┌──────────────────────────────────────────────────────────────────┐
│  Developer Creates Feature Branch                                │
│  (feat/*, fix/*, or chore/*)                                     │
└─────────────────────────┬────────────────────────────────────────┘
                          │
                          ↓
        ┌─────────────────────────────────┐
        │  Push to Remote & Create PR     │
        │  ✓ CircleCI Build & Test        │
        │  ✓ Unit Tests (xUnit, Jest)     │
        │  ✓ Code Quality (SonarCloud)    │
        │  ✓ Kustomize Validation         │
        │  ✗ NO DEPLOYMENT                │
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
    ┌─────────────────────────────┐
    │  Build & Push Containers    │
    │  ├─ LuxAPI image            │
    │  └─ LuxStudio Portal image  │
    │  Tags: SHA + version        │
    └──────────┬──────────────────┘
               │
               ↓
    ┌─────────────────────────────┐
    │  AUTO DEPLOY to PLUTO       │
    │  (Dev/Staging Kubernetes)   │
    │  ✓ Health checks            │
    │  ✓ Smoke tests              │
    └──────────┬──────────────────┘
               │ Release Management
               ↓
    ┌────────────────────────────────────┐
    │  Create Release PR (develop→main)  │
    │  Update version in package.json    │
    └──────────┬─────────────────────────┘
               │ PR Review & Approve
               ↓
    ┌──────────────────────────────────┐
    │  Merge to MAIN Branch            │
    │  ✓ Semantic Release (stable)     │
    │  ✓ Generate release notes        │
    │  ✓ Create git tag (v1.2.0)       │
    └──────────┬───────────────────────┘
               │
               ↓
    ┌─────────────────────────────┐
    │  Build & Push Containers    │
    │  ├─ LuxAPI image            │
    │  └─ LuxStudio Portal image  │
    │  Tags: SHA + version        │
    └──────────┬──────────────────┘
               │
               ↓
    ┌────────────────────────────────┐
    │    MANUAL APPROVAL REQUIRED    │
    │  Admin authorization needed    │
    │  Security review checkpoint    │
    └──────────┬─────────────────────┘
               │ Approved ✓
               ↓
    ┌────────────────────────────────┐
    │  DEPLOY to SATURN              │
    │  (Production Kubernetes)       │
    │  ✓ Rolling update              │
    │  ✓ Health verification         │
    │  ✓ Smoke tests                 │
    │  ✓ Monitor for 24h             │
    └────────────────────────────────┘
```

---

## Environments

### Environment Architecture
```
LOCAL (Developer Machine)
└── Docker Compose (LuxStudio development)
    ├── LuxAPI (ASP.NET Core 8)
    ├── LuxStudio Portal (Next.js/React)
    ├── PostgreSQL 17.2
    └── MinIO (S3-compatible storage)

PLUTO (Dev/Staging Kubernetes Namespace)
├── Branch: develop
├── Releases: Versions marked as pre-release (v1.2.0)
├── Image Tags: commit-SHA + version
├── Deployments:
│   ├── LuxAPI Service
│   ├── LuxStudio Portal Service
│   ├── PostgreSQL 17.2
│   └── MinIO storage
├── Access: Team-wide, no approval required
└── Purpose: Validate features before production

SATURN (Production Kubernetes Namespace)
├── Branch: main
├── Releases: Stable versions (v1.2.0)
├── Image Tags: commit-SHA + release
├── Deployments:
│   ├── LuxAPI Service (replicated)
│   ├── LuxStudio Portal Service (replicated)
│   ├── PostgreSQL 17.2 (high-availability)
│   └── MinIO storage (replicated)
├── Access: Restricted, admin approval required
└── Purpose: Live customer traffic, high availability
```

### Environment Details
- **Local**: Developer workstation using `docker-compose` (LuxStudio) or local builds (Desktop).
- **Pluto (Dev/Staging)**: Kubernetes namespace `luxstudio-pluto`; non-production validation.
- **Saturn (Production)**: Kubernetes namespace `luxstudio-saturn`; live customer traffic.

---

## Branching & Release Model

### Git Workflow Diagram
```
┌─────────────────────────────────────────────────────────┐
│  Feature/Fix Branch                                     │
│  (feat/*, fix/*, chore/*)                               │
│  ↓ Conventional Commits (commitlint +  Husky)           │ 
│  ↓ Push to origin/ feat-xyz                             │
└────────────────┬────────────────────────────────────────┘
                 │ Create Pull Request
                 ↓
        ┌────────────────────────┐
        │  CI Checks             │
        │  ✓ Linting             │
        │  ✓ Tests               │
        │  ✓ SonarCloud          │
        │  ✓ kustomize dry-run   │
        └────────────────────────┘
                 │ Approved by reviewers
                 ↓
        ┌────────────────────────────────┐
        │  Merge to DEVELOP Branch       │
        │  (--squash or fast-forward)    │
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
    │  • Publish to registry                 │
    └────────┬───────────────────────────────┘
             │ Scheduled release (e.g., Sprint end)
             ↓
    ┌────────────────────────────────────────┐
    │  Create Release PR (develop → main)    │
    │  • Update CHANGELOG.md                 │
    │  • Update version in package.json      │
    │  • Merge with fast-forward             │
    └────────┬───────────────────────────────┘
             │ Push to origin/main
             ↓
    ┌────────────────────────────────────────┐
    │  Semantic Release: STABLE RELEASE      │
    │  • Detect conventional commits         │
    │  • Calculate release version           │
    │  • Generate CHANGELOG                  │
    │  • Create git tag (v1.2.0)             │
    │  • Publish release to registry         │
    │  • Create GitHub Release with assets   │
    └────────────────────────────────────────┘
```

- **Branches**: Short-lived `feat/*` and `fix/*` merge into `develop`; releases are promoted from `develop` to `main`.
- **Semantic Release**: Runs on both `develop` (marked as pre-release) and `main` (marked as stable), using same version format (e.g., `1.2.0`).
- **Environment Mapping**: `develop` → Pluto (dev/staging); `main` → Saturn (production).
- **CI System**: CircleCI handles all web/API build, test, containerization, and Kubernetes deployments.

---

## Deployment Targets

### Luxoria Desktop (WinUI 3) - Distribution Pipeline

```
Source Code
│
├─ Build per Runtime Identifier (RID)
│  ├─ win-x86 (32-bit)
│  ├─ win-x64 (64-bit) ← Primary
│  └─ win-arm64 (ARM processors)
│
├─ dotnet publish
│  ├─ Framework: net9.0-windows10.0.26100.0
│  ├─ Configuration: Release
│  └─ Output: Self-contained, single-file executable
│
├─ Code Signing
│  ├─ Certificate: LuxoriaSoft Code Signing Cert
│  ├─ Signature: SmartScreen-compliant
│  └─ Tools: signtool.exe (Windows SDK)
│
├─ Package as Installer
│  ├─ Tool: Inno Setup
│  ├─ Format: .exe installer
│  ├─ Portable: .zip (optional)
│  └─ Output: Distribution-ready artifacts
│
└─ Distribution Channels
   ├─ GitHub Releases (automatic)
   │  └─ luxoria.bluepelicansoft.com (mirror)
   └─ Windows Package Manager (WinGet)
```

**Artifact Details:**
- **Artifact**: Inno Setup installer (and optional portable build).
- **Build**: `dotnet publish` targeting `net9.0-windows10.0.26100.0` (x86, x64, ARM64).
- **Signing**: Code signing via LuxoriaSoft certificate (SmartScreen-friendly).
- **Distribution**: Download from [luxoria.bluepelicansoft.com](https://luxoria.bluepelicansoft.com) or GitHub Releases.

### LuxStudio Web Platform - Container Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                      LuxStudio Platform                          │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────┐        ┌──────────────────────────┐      │
│  │   Next.js Portal   │        │    LuxAPI Backend        │      │
│  │   (React)          │        │    (ASP.NET Core 8)      │      │
│  │                    │        │                          │      │
│  │ • User Interface   │        │ • REST API Endpoints     │      │
│  │ • Client-side      │        │ • Business Logic         │      │
│  │   routing          │        │ • Database Operations    │      │
│  │ • Authentication   │        │ • File Processing        │      │
│  │ • Asset serving    │        │ • Module Management      │      │
│  └────────────────────┘        └──────────────────────────┘      │
│           ↓                              ↓                       │
│  ┌────────────────────────────────────────────────────────┐      │
│  │          Shared Infrastructure                         │      │
│  │                                                        │      │
│  │  ┌─────────────────┐  ┌──────────────────────────┐     │      │
│  │  │   PostgreSQL    │  │      MinIO Storage       │     │      │
│  │  │   Database      │  │  (S3-compatible)         │     │      │
│  │  │                 │  │                          │     │      │
│  │  │ • Tables        │  │ • Project files          │     │      │
│  │  │ • Indexes       │  │ • User uploads           │     │      │
│  │  │ • Init scripts  │  │ • Generated exports      │     │      │
│  │  │   (ConfigMap)   │  │ • Console UI             │     │      │
│  │  └─────────────────┘  └──────────────────────────┘     │      │
│  │                                                        │      │
│  └────────────────────────────────────────────────────────┘      │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
             ↑                                           ↑
             │ Ingress Traffic (TLS)                    │
             │ (Traefik + Let's Encrypt)                │ Config & Secrets
             │                                           │
        ┌────────────────────────────────────────────────────┐
        │  Kubernetes Service Mesh & Networking              │
        │  • Service routing                                 │
        │  • Load balancing                                  │
        │  • Certificate management                          │
        └────────────────────────────────────────────────────┘
```

**Services:**
- **LuxAPI** (ASP.NET Core 8): RESTful API backend with business logic
- **LuxStudio Portal** (Next.js/React): Web user interface
- **PostgreSQL 17.2**: Relational database (Alpine container)
- **MinIO**: S3-compatible object storage for files

**Container Images:**
- Built per service and tagged per environment (e.g., `dev`, `latest`, semantic version)
- Pushed to private container registry
- Image naming convention: `$REGISTRY/luxapi:<tag>` and `$REGISTRY/luxstudio:<tag>`

**Orchestration & Networking:**
- **Orchestration**: Kubernetes with Kustomize overlays (`depl/base`, `depl/overlays/pluto`, `depl/overlays/saturn`)
- **Ingress**: Traefik with Let's Encrypt (cert-manager) for TLS termination
- **Namespaces**: `luxstudio-pluto` (dev) and `luxstudio-saturn` (production)

### Supporting Infrastructure

```
┌──────────────────────────────────────────┐
│      Kubernetes Infrastructure           │
├──────────────────────────────────────────┤
│                                          │
│  Database Layer:                         │
│  • PostgreSQL 17.2 (Alpine)              │
│  • Init scripts from ConfigMap           │
│  • Environment-specific credentials      │
│  • Automated backups (Saturn only)       │
│                                          │
│  Storage Layer:                          │
│  • MinIO (S3-compatible)                 │
│  • Console endpoint for browsing         │
│  • Access keys from Kubernetes Secret    │
│  • Versioning enabled (Saturn)           │
│                                          │
│  Secrets Management:                     │
│  • luxapi-secrets                        │
│  • minio-secret                          │
│  • jwt-keys                              │
│  • Never committed to git                │
│  • Created out-of-band by ops team       │
│                                          │
│  ConfigMaps:                             │
│  • Database initialization scripts       │
│  • Application configuration             │
│  • Environment variables                 │
│                                          │
│  PersistentVolumes (Saturn):             │
│  • PostgreSQL data directory             │
│  • MinIO data directory                  │
│  • Backup storage                        │
│                                          │
└──────────────────────────────────────────┘
```

---

## CI/CD Workflows

### CircleCI Pipeline (Web/API and Container Deployment)

```
┌─────────────────────────────────────────────────────────────────┐
│                    CircleCI Workflow                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  JOB 1: Checkout & Setup                                        │
│  ├─ Clone repository                                            │
│  ├─ Restore build cache                                         │
│  └─ Install dependencies                                        │
│       │                                                         │
│       ↓                                                         │
│  JOB 2: .NET Backend Build & Test                               │
│  ├─ dotnet restore (LuxStudio/LuxAPI/Luxoria.App)               │
│  ├─ dotnet build (Release configuration)                        │
│  ├─ dotnet test (run xUnit test suites)                         │
│  │  └─ Luxoria.Core.Tests                                       │
│  │  └─ Luxoria.Modules.Tests                                    │
│  │  └─ Luxoria.SDK.Tests                                        │
│  ├─ Run .NET analyzers & code quality checks                    │
│  └─ Save cache for next runs                                    │
│       │                                                         │
│       ↓                                                         │
│  JOB 3: Frontend Build & Test                                   │
│  ├─ npm/yarn install dependencies                               │
│  ├─ ESLint (code style)                                         │
│  ├─ Prettier (code formatting)                                  │
│  ├─ npm run build (Next.js/React compilation)                   │
│  ├─ npm run test (Jest unit tests)                              │
│  └─ Tailwind CSS validation                                     │
│       │                                                         │
│       ↓                                                         │
│  JOB 4: Code Quality Gate (SonarCloud)                          │
│  ├─ SonarQube scanner analysis                                  │
│  ├─ Check coverage thresholds                                   │
│  ├─ Verify security hotspots                                    │
│  ├─ Evaluate code smells                                        │
│  └─ Block if quality gate fails                                 │
│       │                                                         │
│       ↓                                                         │
│  JOB 5: Kustomize Validation                                    │
│  ├─ kustomize build depl/base                                   │
│  ├─ kustomize build depl/overlays/pluto                         │
│  ├─ kustomize build depl/overlays/saturn                        │
│  ├─ Validate YAML output                                        │
│  └─ Dry-run (no actual deployment)                              │
│       │                                                         │
│  ┌────┴─────────────────────────────────────────┐               │
│  │  Branch Specific Steps                       │               │
│  │                                              │               │
│  │  IF: Pull Request                            │               │
│  │  └─ STOP here (no deployment)                │               │
│  │     └─ All checks passed                     │               │
│  │                                              │               │
│  │  IF: develop branch (push)                   │               │
│  │  └─ Continue to steps below ↓                │               │
│  └────┬─────────────────────────────────────────┘               │
│       │                                                         │
│       ↓                                                         │
│  JOB 6: Semantic Release (Pre-release)                          │
│  ├─ Analyze commits since last tag              │               │
│  ├─ Calculate next version (patch/minor/major)  │               │
│  ├─ Generate CHANGELOG.md                       │               │
│  ├─ Create annotated git tag (v1.2.0)           │               │
│  ├─ Mark as pre-release in GitHub               │               │
│  ├─ Push tag to origin                          │               │
│  └─ Update release metadata                     │               │
│       │                                                         │
│       ↓                                                         │
│  JOB 7: Build Container Images (develop)                        │
│  ├─ docker build -t $REGISTRY/luxapi:<SHA>      │               │
│  │  ├─ FROM mcr.microsoft.com/dotnet/aspnet:8   │               │
│  │  ├─ COPY LuxStudio/LuxAPI/ /app              │               │
│  │  └─ ENTRYPOINT ["dotnet", "LuxAPI.dll"]      │               │
│  ├─ docker build -t $REGISTRY/luxstudio:<SHA>   │               │
│  │  ├─ FROM node:20-alpine                      │               │
│  │  ├─ COPY LuxStudio/portal/ /app              │               │
│  │  └─ ENTRYPOINT ["npm", "start"]              │               │
│  ├─ Tag images: $REGISTRY/luxapi:1.2.0          │               │
│  └─ Tag images: $REGISTRY/luxstudio:1.2.0       │               │
│       │                                                         │
│       ↓                                                         │
│  JOB 8: Push Images to Registry                 │               │
│  ├─ docker login $REGISTRY                      │               │
│  ├─ docker push $REGISTRY/luxapi:<SHA>          │               │
│  ├─ docker push $REGISTRY/luxapi:1.2.0          │               │
│  ├─ docker push $REGISTRY/luxstudio:<SHA>       │               │
│  └─ docker push $REGISTRY/luxstudio:1.2.0       │               │
│       │                                                         │
│       ↓                                                         │
│  JOB 9: Deploy to PLUTO (Auto)                  │               │
│  ├─ kubectl config use-context pluto-kubeconfig                 │
│  ├─ kubectl set image deployment/luxapi-deployment \            │
│  │  luxapi=$REGISTRY/luxapi:1.2.0                               │
│  ├─ kubectl set image deployment/luxstudio-deployment \         │
│  │  luxstudio=$REGISTRY/luxstudio:1.2.0                         │
│  ├─ kubectl rollout status deployment/luxapi-deployment         │
│  ├─ kubectl rollout status deployment/luxstudio-deployment      │
│  ├─ Run smoke tests (health checks, API endpoints)              │
│  └─ Verify ingress routing                                      │
│       │                                                         │
│       ↓ [PIPELINE SUCCESS ✓]                                    │
│                                                                 │
│  ┌────────────────────────────────────────────────────────┐     │
│  │  IF: main branch (release tag)                         │     │
│  │  └─ Steps 6-9 repeated but:                            │     │
│  │     • Step 6: Semantic Release (STABLE v1.2.0)         │     │
│  │     • Step 9: REQUIRES MANUAL APPROVAL before Saturn   │     │
│  └────────────────────────────────────────────────────────┘     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Pull Request from Feature Branch:**
- Build, test, lint, kustomize dry-run
- **NO deployment** to any environment
- Fails if quality gates not met

**Develop Branch Workflow:**
1. Semantic-release (v1.2.0, marked as pre-release)
2. Build & push images tagged with commit SHA and version
3. **Automatically deploy to Pluto (dev/staging)**
4. Run smoke tests

**Main Branch Workflow (Production Release):**
1. Semantic-release stable (v1.2.0)
2. Build & push images tagged with commit SHA and release tag
3. **Requires manual approval** (admin authorization)
4. Deploy to Saturn (production)
5. GitHub Actions triggered for desktop release build

### GitHub Actions Pipeline (Desktop Release & Signing)

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
│  │ STEP 1: Checkout                             │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • Git clone with tag history                 │               │
│  │ • Set working directory to Luxoria.App       │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 2: Setup .NET Runtime                   │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • Install .NET 9.0.x SDK                     │               │
│  │ • Verify dotnet --version                    │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 3: Restore Dependencies                 │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • dotnet restore Luxoria.App.sln             │               │
│  │ • Fetch NuGet packages                       │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 4: Compile Release Build                │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • dotnet build -c Release --no-restore       │               │
│  │ • Compile all projects in solution           │               │
│  │ • Generate optimized binaries                │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 5: Run Unit Tests                       │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • dotnet test -c Release --no-build          │               │
│  │ • Run Luxoria.Core.Tests                     │               │
│  │ • Run Luxoria.Modules.Tests                  │               │
│  │ • Run Luxoria.SDK.Tests                      │               │
│  │ • Fail if any test fails                     │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 6: Publish for Each RID                 │               │
│  ├──────────────────────────────────────────────┤               │
│  │ For win-x64 (Primary):                       │               │
│  │  dotnet publish \                            │               │
│  │    -c Release \                              │               │
│  │    -p:RuntimeIdentifier=win-x64 \            │               │
│  │    -p:PublishSingleFile=true \               │               │
│  │    -p:SelfContained=true \                   │               │
│  │    --output ./publish/x64                    │               │
│  │                                              │               │
│  │ For win-x86:                                 │               │
│  │  dotnet publish ... -p:RuntimeIdentifier=win-x86             │
│  │                                              │               │
│  │ For win-arm64:                               │               │
│  │  dotnet publish ... -p:RuntimeIdentifier=win-arm64           │
│  │                                              │               │
│  │ Output: Self-contained, single-file exes     │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 7: Sign Binaries & Installer            │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • Load cert from $CODE_SIGN_CERT (base64)    │               │
│  │ • Run signtool.exe (Windows SDK)             │               │
│  │ • Sign: publish/x64/Luxoria.App.exe          │               │
│  │ • Sign: publish/x86/Luxoria.App.exe          │               │
│  │ • Sign: publish/arm64/Luxoria.App.exe        │               │
│  │ • Verify signatures with SmartScreen         │               │
│  │ • Timestamp authority: Digicert              │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 8: Build Inno Setup Installer           │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • iscc.exe installer.iss                     │               │
│  │ • Package signed binaries                    │               │
│  │ • Generate: LuxoriaSetup-1.2.0.exe           │               │
│  │ • Include all architectures (x86/x64/ARM64)  │               │
│  │ • Post-install hooks: shortcuts, registry    │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 9: Create Portable ZIP                  │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • Zip all signed binaries                    │               │
│  │ • Generate: Luxoria-Portable-1.2.0.zip       │               │
│  │ • No installer required (extract & run)      │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 10: Create GitHub Release               │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • Use softprops/action-gh-release@v2         │               │
│  │ • Upload artifacts:                          │               │
│  │   └─ LuxoriaSetup-1.2.0.exe (installer)      │               │
│  │   └─ Luxoria-Portable-1.2.0.zip              │               │
│  │ • Create release notes (auto-generated)      │               │
│  │ • Mark as "Latest Release"                   │               │
│  │ • Publicly available for download            │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓                                                     │
│  ┌──────────────────────────────────────────────┐               │
│  │ STEP 11: Optional - Upload to CDN            │               │
│  ├──────────────────────────────────────────────┤               │
│  │ • Mirror to luxoria.bluepelicansoft.com      │               │
│  │ • Update download links                      │               │
│  │ • Invalidate CDN cache                       │               │
│  └────────┬─────────────────────────────────────┘               │
│           │                                                     │
│           ↓ [WORKFLOW COMPLETE ✓]                               │
│                                                                 │
│  Release is now publicly available for download                 │
│  Users can install via installer or portable zip                │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Quality Gates - Enforcement Points

```
┌──────────────────────────────────────────────────────────────┐
│             Quality Gate Checkpoints                         │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  GATE 1: Commit-level (Local + CI)                           │
│  ├─ Husky pre-commit hooks                                   │
│  ├─ commitlint: Conventional Commits validation              │
│  └─ Blocks commit if message invalid                         │
│                                                              │
│  GATE 2: Code-level (CI Pipeline)                            │
│  ├─ Unit Tests (xUnit, Jest)                                 │
│  │  └─ All test projects must pass                           │
│  ├─ Linting (ESLint, Prettier, .NET analyzers)               │
│  │  └─ Code style violations fail build                      │
│  ├─ Type Checking (Pylance, TypeScript)                      │
│  │  └─ Type errors fail build                                │
│  └─ Build Verification                                       │
│     └─ Project compilation succeeds                          │
│                                                              │
│  GATE 3: Quality Analysis (SonarCloud)                       │
│  ├─ Code Coverage Threshold (e.g., >80%)                     │
│  ├─ Bug Detection                                            │
│  │  └─ Blocks if bugs > threshold                            │
│  ├─ Security Vulnerabilities                                 │
│  │  └─ Fails if critical/high severity found                 │
│  ├─ Code Smells                                              │
│  │  └─ Tracks technical debt                                 │
│  ├─ Maintainability Rating                                   │
│  │  └─ Must be A or B                                        │
│  └─ Reliability Rating                                       │
│     └─ Must be A or B                                        │
│                                                              │
│  GATE 4: Infrastructure Validation (Kustomize)               │
│  ├─ YAML syntax validation                                   │
│  ├─ Kubernetes manifest validation                           │
│  ├─ Image tag resolution                                     │
│  ├─ Dry-run without actual deployment                        │
│  └─ Fails if invalid manifests                               │
│                                                              │
│  GATE 5: Approval Gates                                      │
│  ├─ PR Review (2+ approvals recommended)                     │
│  │  └─ Required before merge to develop                      │
│  ├─ Manual Approval (Production Only)                        │
│  │  └─ Admin sign-off required before Saturn deploy          │
│  └─ Change Advisory Board (CAB) for critical                 │
│     └─ Optional for high-risk changes                        │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

**Unit Tests**: All test projects must pass.
**Commitlint/Husky**: Conventional commits enforce semantic-release versioning.
**SonarCloud**: Quality gate (bugs, coverage, code smells) enforced on PRs and protected branches.
**Formatting/Linting**: Frontend linters (ESLint/Tailwind/Prettier) and .NET analyzers must pass before deploy.

---

## Release Promotion & Deployment Flow

### Complete Release Lifecycle

```
┌────────────────────────────────────────────────────────────────────┐
│  PHASE 1: DEVELOPMENT & INTEGRATION                                │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  Developer Activity                                                │
│  ├─ Create feature branch (feat/add-new-module)                    │
│  ├─ Write code & tests                                             │
│  ├─ Commit with conventional messages                              │
│  │  └─ Types: feat|fix|refactor|docs|test|chore|perf               │
│  ├─ Push to origin                                                 │
│  └─ Create Pull Request                                            │
│                                                                    │
│  CI/CD Validation (CircleCI)                                       │
│  ├─ Build: ✓ Code compiles                                         │
│  ├─ Kustomize: ✓ Manifests valid                                   │
│  └─ Status: READY FOR REVIEW ✓                                     │
│                                                                    │
│  Code Review                                                       │
│  ├─ Peer review (>= 2 team members)                                │
│  ├─ Comments & suggestions                                         │
│  ├─ Approve or Request Changes                                     │
│  └─ Once approved → Ready to merge                                 │
│                                                                    │
│  Merge to develop                                                  │
│  └─ PR merged with fast-forward or squash                          │
│                                                                    │
└────────────────┬───────────────────────────────────────────────────┘
                 │
                 ↓
┌────────────────────────────────────────────────────────────────────┐
│  PHASE 2: STAGING & PRE-RELEASE VALIDATION (Pluto)                 │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  Develop Branch Processing                                         │
│  ├─ Semantic Release Automation                                    │
│  │  ├─ Scan conventional commits since last tag                    │
│  │  ├─ Determine version bump (patch/minor/major)                  │
│  │  │  └─ feat: → minor | fix: → patch | BREAKING: → major         │
│  │  ├─ Generate or update CHANGELOG.md                             │
│  │  ├─ Create annotated git tag (v1.2.0)                           │
│  │  ├─ Mark as pre-release in GitHub                               │
│  │  └─ Push tag to origin                                          │
│  │                                                                 │
│  └─ Container Image Build                                          │
│     ├─ Build luxapi image                                          │
│     │  └─ docker build -t $REGISTRY/luxapi:abc123 ...              │
│     ├─ Build luxstudio image                                       │
│     │  └─ docker build -t $REGISTRY/luxstudio:abc123 ...           │
│     ├─ Tag with version                                            │
│     │  └─ docker tag ... luxapi:1.2.0                              │
│     └─ Push to registry                                            │
│        └─ docker push $REGISTRY/luxapi:1.2.0                       │
│                                                                    │
│  Deployment to PLUTO (Automatic)                                   │
│  ├─ kubectl set image deployment/luxapi-deployment \               │
│  │  luxapi=$REGISTRY/luxapi:1.2.0                                  │
│  ├─ kubectl set image deployment/luxstudio-deployment \            │
│  │  luxstudio=$REGISTRY/luxstudio:1.2.0                            │
│  ├─ kubectl rollout status deployment/luxapi-deployment            │
│  ├─ Watch for pod readiness & liveness                             │
│  └─ Auto-rollback if readiness fails                               │
│                                                                    │
│  Post-Deploy Verification                                          │
│  ├─ Health Check: GET /api/health → 200 OK ✓                       │
│  ├─ Database Connectivity: SELECT * FROM health ✓                  │
│  ├─ Smoke Tests: CRUD operations on test data                      │
│  ├─ Ingress Routing: https://api.pluto.local working               │
│  └─ Certificate Validation: TLS cert valid                         │
│                                                                    │
│  Monitoring & QA                                                   │
│  ├─ Monitor logs: kubectl logs deployment/luxapi-deployment        │
│  ├─ Check metrics (CPU, memory, request latency)                   │
│  ├─ QA runs test suite against Pluto environment                   │
│  ├─ Performance benchmarks recorded                                │
│  └─ Any issues found → Bugfix PR to develop                        │
│                                                                    │
└────────────────┬───────────────────────────────────────────────────┘
                 │ Release Sign-Off (Scheduled)
                 │ (e.g., Sprint end, every Friday)
                 ↓
┌────────────────────────────────────────────────────────────────────┐
│  PHASE 3: RELEASE PREPARATION (develop → main)                     │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  Release Manager Creates Release PR                                │
│  ├─ Source branch: develop                                         │
│  ├─ Target branch: main                                            │
│  ├─ Title: "Release v1.2.0"                                        │
│  ├─ Update CHANGELOG.md (stable section)                           │
│  ├─ Update package.json version field                              │
│  ├─ Add release notes                                              │
│  └─ Push for review                                                │
│                                                                    │
│  Release Review Checklist                                          │
│  ├─ ✓ CHANGELOG updated                                            │
│  ├─ ✓ Version correctly bumped                                     │
│  ├─ ✓ All commits included from develop                            │
│  ├─ ✓ No security issues in SonarCloud                             │
│  ├─ ✓ Desktop tests passing                                        │
│  ├─ ✓ API/Portal tests passing                                     │
│  ├─ ✓ No performance regressions                                   │
│  └─ Approved by release owner & tech lead                          │
│                                                                    │
│  Merge to Main                                                     │
│  └─ PR merged with fast-forward to main branch                     │
│                                                                    │
└────────────────┬───────────────────────────────────────────────────┘
                 │
                 ↓
┌────────────────────────────────────────────────────────────────────┐
│  PHASE 4: PRODUCTION RELEASE WORKFLOW (Main Branch)                │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  Semantic Release Activation (Stable)                              │
│  ├─ Scan commits since last release                                │
│  ├─ Detect version bump (same as Pluto)                            │
│  ├─ Generate final CHANGELOG.md                                    │
│  ├─ Create annotated git tag (v1.2.0) ← STABLE                     │
│  ├─ Push tag to origin                                             │
│  └─ Create GitHub Release (body: changelog)                        │
│                                                                    │
│  Container Image Build (Production)                                │
│  ├─ Build luxapi with production settings                          │
│  │  └─ docker build --build-arg ENV=production ...                 │
│  ├─ Build luxstudio with production settings                       │
│  ├─ Tag with commit SHA: luxapi:abc123                             │
│  ├─ Tag with version: luxapi:1.2.0 (latest)                        │
│  └─ Push both tags to registry                                     │
│                                                                    │
│  Authorization & Approval (CRITICAL GATE)                          │
│  ├─ Deployment paused (no auto-deploy to production)               │
│  ├─ Slack notification: "Awaiting production approval"             │
│  ├─ Only authorized personnel can approve                          │
│  │  └─ DevOps lead, Release manager, CTO                           │
│  ├─ Approval token required: $APPROVAL_TOKEN                       │
│  ├─ Generate time-limited approval signature                       │
│  ├─ Store in CircleCI context (temporary)                          │
│  └─ Timestamp for audit trail                                      │
│                                                                    │
│  Production Deployment (Saturn) - After Approval ✓                 │
│  ├─ kubectl config use-context saturn-kubeconfig                   │
│  ├─ kubectl set image deployment/luxapi-deployment \               │
│  │  luxapi=$REGISTRY/luxapi:1.2.0                                  │
│  ├─ kubectl set image deployment/luxstudio-deployment \            │
│  │  luxstudio=$REGISTRY/luxstudio:1.2.0                            │
│  ├─ kubectl rollout status (wait for new pods)                     │
│  │  └─ Default: 5 minute timeout                                   │
│  ├─ Verify desired replicas reached                                │
│  └─ Auto-rollback if pod fails to start                            │
│                                                                    │
│  Production Health Verification                                    │
│  ├─ Wait 30 seconds (pod startup time)                             │
│  ├─ API Health Check: GET https://api.saturn.local/health ✓        │
│  ├─ Database Query: SELECT version() ✓                             │
│  ├─ Canary Request: Small % traffic to new version                 │
│  ├─ Error Rate Check: < 0.1% (auto-rollback if exceeded)           │
│  ├─ Latency Check: P95 < baseline (auto-rollback if exceeded)      │
│  ├─ Memory/CPU: Under thresholds                                   │
│  └─ TLS Certificate: Valid, not expiring soon                      │
│                                                                    │
│  Desktop Release Trigger (GitHub Actions)                          │
│  ├─ New version tag (v1.2.0) triggers GitHub Actions               │
│  ├─ Build Windows installer                                        │
│  ├─ Sign binaries (SmartScreen)                                    │
│  ├─ Create GitHub Release with artifacts                           │
│  └─ Users can download from:                                       │
│     ├─ GitHub Releases page                                        │
│     ├─ luxoria.bluepelicansoft.com                                 │
│     └─ Windows Package Manager (WinGet)                            │
│                                                                    │
└────────────────┬───────────────────────────────────────────────────┘
                 │ Post-Deployment (24 hours monitoring)
                 ↓
┌────────────────────────────────────────────────────────────────────┐
│  PHASE 5: PRODUCTION MONITORING & STABILITY                        │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  Continuous Monitoring (1st hour - Critical)                       │
│  ├─ Watch error rates in real-time                                 │
│  ├─ Monitor API latency (P50, P95, P99)                            │
│  ├─ Check database connection pool health                          │
│  ├─ Verify MinIO object storage operations                         │
│  ├─ Monitor ingress traffic distribution                           │
│  └─ Alert threshold: > 1% error rate triggers rollback             │
│                                                                    │
│  Extended Monitoring (1-24 hours)                                  │
│  ├─ Memory leaks: Watch memory growth trend                        │
│  ├─ CPU saturation: Monitor CPU usage pattern                      │
│  ├─ Database queries: Slow query log analysis                      │
│  ├─ User-reported issues: Triage support tickets                   │
│  └─ Performance baselines: Compare with previous release           │
│                                                                    │
│  Success Criteria (After 24 hours)                                 │
│  ├─ Error rate: < 0.05%                                            │
│  ├─ Latency P95: Within 10% of baseline                            │
│  ├─ Zero critical issues reported                                  │
│  ├─ All health checks: Passing ✓                                   │
│  └─ Release marked as "STABLE" → Production lockdown lifted        │
│                                                                    │
│  Fallback Strategy (If Issues Found)                               │
│  ├─ Automatic Rollback ↓                                           │
│  │  └─ kubectl rollout undo deployment/luxapi-deployment           │
│  │     -n luxstudio-saturn                                         │
│  ├─ Manual Rollback ↓                                              │
│  │  └─ DevOps initiates kubectl command                            │
│  │     └─ Revert to previous version within 2 minutes              │
│  ├─ Incident Response ↓                                            │
│  │  ├─ Page on-call engineer                                       │
│  │  ├─ Create incident ticket                                      │
│  │  ├─ Investigate root cause                                      │
│  │  ├─ Document findings                                           │
│  │  └─ Create hotfix branch                                        │
│  └─ Post-Incident Review ↓                                         │
│     ├─ Team retrospective                                          │
│     ├─ Process improvements                                        │
│     └─ Updated runbooks                                            │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

### Step-by-Step Release Summary
1. **Feature Branch → PR**: Validate in CircleCI (tests, lint, kustomize dry-run); no deploy.
2. **Merge to `develop`**: Semantic-release pre-release; images tagged with SHA and pre-release; auto-deploy to **Pluto (dev/staging)**.
3. **Merge to `main`**: Semantic-release stable; images tagged with SHA and release; manual approval then deploy to **Saturn (production)**; desktop tag triggers GitHub Actions release build.
4. **Post-Deploy Checks**: Smoke tests, health endpoints, ingress verification; roll back with `kubectl rollout undo` if needed.

---

## Deployment Commands & Operations

### Dev/Staging (Pluto) Deployment
```bash
# Build overlays for Pluto
kustomize build depl/overlays/pluto > pluto.yaml

# Apply to Pluto namespace
kubectl apply -k depl/overlays/pluto -n luxstudio-pluto

# Check rollout status (wait for completion)
kubectl rollout status deployment/luxapi-deployment -n luxstudio-pluto
kubectl rollout status deployment/luxstudio-deployment -n luxstudio-pluto

# View pod status
kubectl get pods -n luxstudio-pluto

# Check logs from LuxAPI
kubectl logs -n luxstudio-pluto -l app=luxapi -f

# Verify service endpoints
kubectl get svc -n luxstudio-pluto
```

### Production (Saturn) Deployment
```bash
# Build overlays for Saturn
kustomize build depl/overlays/saturn > saturn.yaml

# Review manifest before applying (dry-run)
kubectl apply -k depl/overlays/saturn -n luxstudio-saturn --dry-run=client

# Apply to Saturn namespace (WITH APPROVAL)
kubectl apply -k depl/overlays/saturn -n luxstudio-saturn

# Check rollout status (wait for completion)
kubectl rollout status deployment/luxapi-deployment -n luxstudio-saturn
kubectl rollout status deployment/luxstudio-deployment -n luxstudio-saturn

# Monitor pod events during rollout
kubectl describe pods -n luxstudio-saturn | grep -A 5 "Events:"

# Verify all pods are running
kubectl get pods -n luxstudio-saturn

# Check service ingress routes
kubectl get ingress -n luxstudio-saturn
```

### Rollback Procedure (Emergency)
```bash
# Check rollout history
kubectl rollout history deployment/luxapi-deployment -n luxstudio-saturn

# View previous revision details
kubectl rollout history deployment/luxapi-deployment -n luxstudio-saturn --revision=2

# Perform rollback to previous revision
kubectl rollout undo deployment/luxapi-deployment -n luxstudio-saturn

# Wait for rollback to complete
kubectl rollout status deployment/luxapi-deployment -n luxstudio-saturn

# Verify old version is running
kubectl get deployment/luxapi-deployment -n luxstudio-saturn -o wide
```

### Desktop Release Build (Local)
```bash
# Restore dependencies
dotnet restore Luxoria.App/Luxoria.App.sln

# Build release configuration
dotnet build Luxoria.App/Luxoria.App.sln -c Release --no-restore

# Run tests
dotnet test Luxoria.App/Luxoria.App.sln -c Release --no-build

# Publish for x64 architecture (primary)
dotnet publish Luxoria.App/Luxoria.App/Luxoria.App.csproj \
  -c Release \
  -p:RuntimeIdentifier=win-x64 \
  -p:PublishSingleFile=true \
  -p:SelfContained=true \
  --output ./publish/x64

# Sign binaries
pwsh ./scripts/sign.ps1

# Build Inno Setup installer
iscc installer.iss

# Result: LuxoriaSetup-1.2.0.exe ready for distribution
```

---

## Code Quality Standards & Testing Strategy

### Testing Pyramid
```
   Luxoria Test Coverage:
   
   Unit Tests (Base): 70-80% coverage
   ├─ Luxoria.Core.Tests
   ├─ Luxoria.Modules.Tests
   ├─ Luxoria.SDK.Tests
   └─ LuxAPI integration tests
   
   Integration Tests (Middle): 20-25% coverage
   ├─ Database operations
   ├─ API endpoint flows
   └─ Module interactions
   
   E2E Tests (Top): 5-10% coverage
   ├─ Portal UI workflows
   ├─ File upload/download
   └─ Production deployment smoke tests
```

**Unit Tests**: xUnit test suites across Core, Modules, SDK, and LuxAPI where applicable.
**Static Analysis**: SonarQube badges and gates for reliability, security, maintainability, coverage.
**API/Frontend Tests**: Run as part of CI when defined (lint + unit for Next.js/Vue portals).
**Manual Smoke Tests**: Post-deploy verification of health endpoints and ingress routing.

---

## Security, Secrets & Access Control

### Secrets Management Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                  Secrets Storage Strategy                    │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Development (Local)                                         │
│  ├─ .env.local file (Git-ignored)                            │
│  ├─ Docker Compose secrets volume                            │
│  ├─ Contains: JWT keys, DB password, API keys                │
│  └─   NEVER commit to repository                             │
│                                                              │
│  CI/CD Secrets                                               │
│  ├─ CircleCI Contexts                                        │
│  │  ├─ luxstudio-dev-secrets                                 │
│  │  ├─ luxstudio-prod-secrets                                │
│  │  └─ Vars: REGISTRY_USER, REGISTRY_TOKEN, etc.             │
│  │                                                           │
│  ├─ GitHub Actions Secrets                                   │
│  │  ├─ CODE_SIGN_CERT (base64 PFX)                           │
│  │  ├─ CODE_SIGN_PASSWORD                                    │
│  │  └─ Accessed via: ${{ secrets.SECRET_NAME }}              │
│  │                                                           │
│  ├─ Environment Variables (Workflow)                         │
│  │  ├─ Set per job/step                                      │
│  │  ├─ Scoped to branch/tag                                  │
│  │  └─ Masked in logs (sensitive values hidden)              │
│  │                                                           │
│  └─ Rotation Policy                                          │
│     ├─ Monthly for development                               │
│     ├─ Quarterly for production                              │
│     └─ Immediate upon leak detection                         │
│                                                              │
│  Runtime Secrets (Kubernetes)                                │
│  ├─ Kubernetes Secrets (etcd-backed)                         │
│  │  ├─ base64 encoded (not encrypted by default)             │
│  │  ├─ Types: Opaque, TLS, Docker, ServiceAccount            │
│  │  └─ Should use encryption-at-rest plugin                  │
│  │                                                           │
│  ├─ Secret Management:                                       │
│  │  ├─ luxapi-secrets                                        │
│  │  │  ├─ JWT_SECRET                                         │
│  │  │  ├─ DATABASE_PASSWORD                                  │
│  │  │  ├─ SMTP_PASSWORD                                      │
│  │  │  └─ API_KEYS (external services)                       │
│  │  │                                                        │
│  │  ├─ minio-secret                                          │
│  │  │  ├─ MINIO_ROOT_USER                                    │
│  │  │  └─ MINIO_ROOT_PASSWORD                                │
│  │  │                                                        │
│  │  └─ database-secrets                                      │
│  │     ├─ POSTGRES_PASSWORD                                  │
│  │     └─ POSTGRES_USER                                      │
│  │                                                           │
│  ├─ Mounted via:                                             │
│  │  ├─ volumeMounts (files in pod)                           │
│  │  └─ Environment variables (via valueFrom.secretKeyRef)    │
│  │                                                           │
│  └─ Access Control                                           │
│     ├─ Scoped to namespace (Pluto vs Saturn)                 │
│     ├─ RBAC: Only authorized service accounts                │
│     └─ Audit: All secret access logged                       │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

**Sensitive Environment Variables**:
- JWT keys, SMTP credentials, MinIO credentials set via secrets
- Referenced in overlays patches
- Never logged or exposed in CI output

**TLS Security**:
- Automatic certificates via cert-manager + Let's Encrypt on Traefik ingress
- HTTPS enforced (redirect HTTP → HTTPS)
- Certificate auto-renewal 30 days before expiry

**Least Privilege Access**:
- Service accounts and namespaces isolate Pluto/Saturn
- Database users scoped per environment
- Role-Based Access Control (RBAC) enforced

**Code Signing**:
- Desktop binaries and installers signed to satisfy SmartScreen
- Using LuxoriaSoft Code Signing Certificate
- Timestamp authority: DigiCert (ensures validity after cert expiry)

---

## Production Rollback & Recovery

### Rollout Monitoring & Automatic Rollback

```
┌──────────────────────────────────────────────────────┐
│         Deployment Rollout Process                   │
├──────────────────────────────────────────────────────┤
│                                                      │
│  T+0 sec: New image deployment triggered             │
│  ├─ kubectl set image deployment/luxapi-deployment   │
│  └─ Old pods: Running (desired replicas: 3)          │
│                                                      │
│  T+5 sec: New pod creation initiated                 │
│  ├─ Kubernetes creates 1 new pod (1 old running)     │
│  ├─ Image pull in progress                           │
│  └─ Container initialization                         │
│                                                      │
│  T+15 sec: Readiness probe check                     │
│  ├─ New pod: GET /api/health                         │
│  ├─ Result: 200 OK ✓                                 │
│  ├─ Pod added to service load balancer               │
│  └─ Traffic begins flowing (small %)                 │
│                                                      │
│  T+20 sec: Canary traffic phase                      │
│  ├─ 33% traffic to new version                       │
│  ├─ Monitor error rate & latency                     │
│  ├─ 2nd new pod created if no errors                 │
│  └─ 1 old pod terminated                             │
│                                                      │
│  ┌──────────────────────────────────────────────┐    │
│  │ ERROR DETECTION TRIGGERS ROLLBACK:           │    │
│  │ • Error rate > 1%                            │    │
│  │ • P95 latency > baseline + 50%               │    │
│  │ • Memory leak detected                       │    │
│  │ • Pod crashes/restarts                       │    │
│  └──────────────────────────────────────────────┘    │
│          ↓                                           │
│    AUTOMATIC ROLLBACK INITIATED                      │
│    └─ kubectl rollout undo (< 30 seconds)            │
│                                                      │
│  Success Path (No Errors):                           │
│  ├─ T+40 sec: 3rd new pod created                    │
│  ├─ T+45 sec: Last old pod terminated                │
│  ├─ T+50 sec: Rollout complete                       │
│  ├─ All 3 pods running new version                   │
│  └─ Release marked as "STABLE" after 24h             │
│                                                      │
└──────────────────────────────────────────────────────┘
```

**Health Probes**:
- Liveness probes: Restart unhealthy pods
- Readiness probes: Remove from load balancer if not ready

**Rollout Monitoring**:
- `kubectl rollout status` monitors deployment progress
- `kubectl logs` captures application startup errors

**Rollback Procedure**:
- `kubectl rollout undo deployment/<name> -n <namespace>` for API/portal
- GitHub Releases versioned for desktop rollback (users can download previous installer)

**Ingress Verification**:
- Confirm Traefik routes and TLS for Pluto/Saturn hosts after deploy

---

## Operational Best Practices & Runbooks

### Best Practices Summary

```
┌──────────────────────────────────────────────────────────────┐
│           Operational Guidelines                             │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  CONTAINER IMAGE MANAGEMENT                                  │
│  ├─ Tags: dev/latest for continuous delivery                 │
│  ├─ Semantic versions for releases (v1.2.3)                  │
│  ├─ Commit SHA always included (abc1234)                     │
│  ├─ Never use 'latest' in production k8s specs               │
│  └─ Clean up old images monthly (garbage collection)         │
│                                                              │
│  DATA PERSISTENCE & BACKUPS                                  │
│  ├─ Base: emptyDir (for development only)                    │
│  ├─ Production: Bind PersistentVolumeClaims                  │
│  │  ├─ PostgreSQL data → 100GB volume                        │
│  │  └─ MinIO data → 500GB volume                             │
│  ├─ Backup Strategy:                                         │
│  │  ├─ Daily snapshots (Saturn only)                         │
│  │  ├─ 7-day retention (local)                               │
│  │  ├─ 30-day retention (offsite)                            │
│  │  └─ Tested recovery annually                              │
│  └─ Disaster Recovery: RTO 1hr, RPO 15min                    │
│                                                              │
│  APPROVAL & CHANGE MANAGEMENT                                │
│  ├─ Development: Auto-deploy (Pluto)                         │
│  ├─ Staging: Auto-deploy (Pluto)                             │
│  ├─ Production: Manual approval (Saturn)                     │
│  ├─ Approvers: Release manager, DevOps lead, CTO             │
│  ├─ Approval window: 24h (can extend for holidays)           │
│  └─ CAB reviews: For critical changes                        │
│                                                              │
│  ENVIRONMENT CONSISTENCY                                     │
│  ├─ Kustomize overlays enforce consistency                   │
│  ├─ Avoid hand-edits to live clusters                        │
│  ├─ All config in git (except secrets)                       │
│  ├─ Regular drift detection audits                           │
│  └─ Synchronization: Git → Cluster via CI/CD                 │
│                                                              │
│  SECURITY HARDENING                                          │
│  ├─ Pod Security Policy enforcement                          │
│  ├─ Network policies (ingress/egress rules)                  │
│  ├─ RBAC: Least privilege service accounts                   │
│  ├─ Secrets encryption-at-rest (etcd)                        │
│  ├─ Image scanning for vulnerabilities                       │
│  ├─ Regular penetration testing                              │
│  └─ Security patches: Apply within 24h                       │
│                                                              │
│  MONITORING & ALERTING                                       │
│  ├─ Prometheus metrics collection                            │
│  ├─ Grafana dashboards (per environment)                     │
│  ├─ Alert rules: Pod restarts, high CPU/memory               │
│  ├─ PagerDuty integration for on-call rotation               │
│  ├─ SLO targets:                                             │
│  │  ├─ Availability: 99.9%                                   │
│  │  ├─ Latency P99: < 500ms                                  │ 
│  │  └─ Error rate: < 0.05%                                   │
│  └─ Regular incident drills                                  │
│                                                              │
│  CAPACITY PLANNING                                           │
│  ├─ Right-sizing: CPU & memory requests/limits               │
│  ├─ Horizontal Pod Autoscaler (HPA)                          │
│  │  ├─ Min replicas: 3 (production)                          │
│  │  ├─ Max replicas: 10 (based on load)                      │
│  │  └─ Target CPU: 70%                                       │
│  ├─ Quarterly capacity reviews                               │
│  └─ Growth forecasting (6-12 months)                         │
│                                                              │
│  DOCUMENTATION & RUNBOOKS                                    │
│  ├─ Updated after every incident                             │
│  ├─ Playbooks for common scenarios:                          │
│  │  ├─ Pod crash troubleshooting                             │
│  │  ├─ Database connection issues                            │
│  │  ├─ High latency diagnosis                                │
│  │  ├─ Storage full recovery                                 │
│  │  └─ Emergency rollback procedure                          │
│  ├─ Owner assigned to each runbook                           │
│  └─ Annual review & update                                   │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

## Troubleshooting & Common Issues

### Pod Crash - Diagnosis & Recovery
```bash
# Check pod status
kubectl get pods -n luxstudio-saturn -o wide

# View recent pod events
kubectl describe pod <pod-name> -n luxstudio-saturn

# Check logs (current attempt)
kubectl logs <pod-name> -n luxstudio-saturn

# Check logs (previous/crashed container)
kubectl logs <pod-name> -n luxstudio-saturn --previous

# Get deployment events
kubectl describe deployment luxapi-deployment -n luxstudio-saturn

# Check resource limits
kubectl get pods -n luxstudio-saturn -o jsonpath='{.items[0].spec.containers[0].resources}'

# Emergency: Trigger immediate rollback
kubectl rollout undo deployment/luxapi-deployment -n luxstudio-saturn
```

### High Latency - Investigation Checklist
```bash
# 1. Check pod resource utilization
kubectl top pods -n luxstudio-saturn

# 2. View node status & capacity
kubectl top nodes
kubectl describe nodes

# 3. Check database connection pool
kubectl logs <luxapi-pod> | grep -i "connection pool"

# 4. Query database performance
kubectl exec -it <postgres-pod> -n luxstudio-saturn -- psql -U postgres
>>> EXPLAIN ANALYZE SELECT ...;

# 5. Check ingress/network metrics
kubectl get ingress -n luxstudio-saturn -o wide

# 6. Restart problematic pod (if needed)
kubectl delete pod <pod-name> -n luxstudio-saturn
# (Deployment will recreate immediately)
```

### Storage/Disk Full - Emergency Cleanup
```bash
# Check PVC usage
kubectl get pvc -n luxstudio-saturn
kubectl exec -it <postgres-pod> -- df -h

# Clean old images from registry (if local)
docker system prune -a

# Resize PVC (if supported by StorageClass)
kubectl patch pvc postgres-data-pvc -n luxstudio-saturn \
  -p '{"spec":{"resources":{"requests":{"storage":"200Gi"}}}}'

# Check database size
kubectl exec -it <postgres-pod> -- psql -U postgres -c "SELECT pg_size_pretty(pg_database_size('luxstudio'));"
```

---

## Example GitHub Actions Workflows

### Desktop Release Workflow (Windows Runner)
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
         - uses: actions/checkout@v4
         
         - uses: actions/setup-dotnet@v4
            with:
               dotnet-version: '9.0.x'
         
         - name: Restore
            run: dotnet restore Luxoria.App/Luxoria.App.sln
         
         - name: Build
            run: dotnet build Luxoria.App/Luxoria.App.sln -c Release --no-restore
         
         - name: Test
            run: dotnet test Luxoria.App/Luxoria.App.sln -c Release --no-build
         
         - name: Publish for x64
            run: >
               dotnet publish Luxoria.App/Luxoria.App/Luxoria.App.csproj -c Release
               -p:RuntimeIdentifier=win-x64 -p:PublishSingleFile=true -p:SelfContained=true
               --output ./publish/x64
         
         - name: Publish for x86
            run: >
               dotnet publish Luxoria.App/Luxoria.App/Luxoria.App.csproj -c Release
               -p:RuntimeIdentifier=win-x86 -p:PublishSingleFile=true -p:SelfContained=true
               --output ./publish/x86
         
         - name: Publish for ARM64
            run: >
               dotnet publish Luxoria.App/Luxoria.App/Luxoria.App.csproj -c Release
               -p:RuntimeIdentifier=win-arm64 -p:PublishSingleFile=true -p:SelfContained=true
               --output ./publish/arm64
         
         - name: Sign binaries
            env:
               PFX_BASE64: ${{ secrets.CODE_SIGN_CERT }}
               PFX_PASSWORD: ${{ secrets.CODE_SIGN_PASSWORD }}
            run: pwsh ./scripts/sign.ps1
         
         - name: Build installer
            run: iscc installer.iss
         
         - name: Create portable ZIP
            run: |
               7z a Luxoria-Portable-${{ github.ref_name }}.zip ./publish/x64/Luxoria.App.exe
         
         - name: Release assets
            uses: softprops/action-gh-release@v2
            with:
               files: |
                  LuxoriaSetup-${{ github.ref_name }}.exe
                  Luxoria-Portable-${{ github.ref_name }}.zip
               body_path: CHANGELOG.md
               draft: false
               prerelease: false
```

---

## Container Image Build Reference

### Building and Pushing Images

**LuxAPI (ASP.NET Core Backend):**
```bash
docker build \
  -t luxoria-registry.azurecr.io/luxapi:abc1234 \
  -t luxoria-registry.azurecr.io/luxapi:1.2.0 \
  -f LuxStudio/LuxAPI/Dockerfile \
  LuxStudio/LuxAPI

docker push luxoria-registry.azurecr.io/luxapi:abc1234
docker push luxoria-registry.azurecr.io/luxapi:1.2.0
```

**LuxStudio Portal (Next.js Frontend):**
```bash
docker build \
  -t luxoria-registry.azurecr.io/luxstudio:abc1234 \
  -t luxoria-registry.azurecr.io/luxstudio:1.2.0 \
  -f LuxStudio/portal/Dockerfile \
  LuxStudio/portal

docker push luxoria-registry.azurecr.io/luxstudio:abc1234
docker push luxoria-registry.azurecr.io/luxstudio:1.2.0
```

**Image Tagging Strategy:**
- Commit SHA for traceability (abc1234)
- Semantic version for both environments (1.2.0)
- Pre-release vs stable determined by GitHub release flag
- Never use `latest` in production manifests

---

## Required Secrets and Configuration

### CircleCI Contexts & Secrets
```
luxstudio-dev-secrets:
├─ REGISTRY_USER: Container registry username
├─ REGISTRY_TOKEN: Container registry password/token
├─ REGISTRY_URL: Registry endpoint (e.g., luxoria-registry.azurecr.io)
└─ KUBECONFIG_PLUTO: Base64-encoded kubeconfig for dev cluster

luxstudio-prod-secrets:
├─ REGISTRY_USER: Container registry username
├─ REGISTRY_TOKEN: Container registry password/token
├─ REGISTRY_URL: Registry endpoint
├─ KUBECONFIG_SATURN: Base64-encoded kubeconfig for production
└─ APPROVAL_TOKEN: Token for manual production approval gate
```

### GitHub Actions Secrets (Desktop Release)
```
CODE_SIGN_CERT: Base64-encoded .pfx certificate
CODE_SIGN_PASSWORD: Password for code signing certificate
```

### Kubernetes Secrets (Runtime Configuration)
```bash
# Create LuxAPI secrets
kubectl create secret generic luxapi-secrets \
  --from-literal=JWT_SECRET='your-secret-key' \
  --from-literal=DATABASE_PASSWORD='secure-password' \
  --from-literal=SMTP_PASSWORD='email-password' \
  -n luxstudio-saturn

# Create MinIO secrets
kubectl create secret generic minio-secret \
  --from-literal=MINIO_ROOT_USER='minioadmin' \
  --from-literal=MINIO_ROOT_PASSWORD='minioadmin-password' \
  -n luxstudio-saturn

# Create database secrets
kubectl create secret generic database-secrets \
  --from-literal=POSTGRES_PASSWORD='db-password' \
  --from-literal=POSTGRES_USER='postgres' \
  -n luxstudio-saturn
```

---

## Deployment Quick Reference

| Operation | Command | Environment |
|-----------|---------|-------------|
| Deploy Dev | `kubectl apply -k depl/overlays/pluto` | Pluto (auto) |
| Deploy Prod | `kubectl apply -k depl/overlays/saturn` | Saturn (manual approval) |
| Rollback API | `kubectl rollout undo deployment/luxapi-deployment` | Current namespace |
| Check Status | `kubectl rollout status deployment/luxapi-deployment` | Current namespace |
| View Logs | `kubectl logs deployment/luxapi-deployment -f` | Current namespace |
| Restart Pods | `kubectl rollout restart deployment/luxapi-deployment` | Current namespace |
| Scale Pods | `kubectl scale deployment/luxapi-deployment --replicas=5` | Current namespace |
| Build Desktop | `dotnet publish ... -p:RuntimeIdentifier=win-x64` | Local |
| Sign Installer | `pwsh ./scripts/sign.ps1` | Local (Windows) |
| Create Release | `iscc installer.iss` | Local (Windows) |

---

**Document Version**: 1.0  
**Last Updated**: January 2026  
**Owner**: DevOps Team  
**Status**: Production Ready
