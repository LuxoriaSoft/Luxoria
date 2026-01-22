# Luxoria Platform Architecture

## Platform Overview

**Luxoria** is a complete, open-source photography ecosystem consisting of three integrated components:

1. **Luxoria Desktop** - Native Windows application for image management, editing, and local operations
2. **LuxStudio Web Platform** - Cloud-based web application for client collaboration, gallery sharing, and remote operations
3. **Infrastructure & Deployment** - Kubernetes-based cloud infrastructure for hosting and scaling LuxStudio

Together, these components create a unified workflow: photographers work locally with Luxoria Desktop, then seamlessly publish and collaborate with clients via LuxStudio Web Platform, all powered by transparent, open-source infrastructure.

**Official Website & Downloads**: [luxoria.bluepelicansoft.com](https://luxoria.bluepelicansoft.com)  
**Documentation**: [docs.luxoria.bluepelicansoft.com](https://docs.luxoria.bluepelicansoft.com)  
**Repository**: [github.com/LuxoriaSoft/Luxoria](https://github.com/LuxoriaSoft/Luxoria)  
**Marketplace**: [github.com/LuxoriaSoft/marketplace](https://github.com/LuxoriaSoft/marketplace)

---

## Platform Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Luxoria Platform                             │
├─────────────────────┬──────────────────────────┬─────────────────────┤
│                     │                          │                     │
│  Luxoria Desktop    │   LuxStudio Web Platform │   Infrastructure    │
│  (Local Windows)    │   (Cloud Services)       │   (Kubernetes)      │
│                     │                          │                     │
│ • Image Import      │  • Gallery Publishing    │  • Base Config      │
│ • Organization      │  • Client Collaboration  │  • Pluto Overlay    │
│ • Editing           │  • Feedback System       │  • Saturn Overlay   │
│ • Modules           │  • Analytics             │  • Ingress/TLS      │
│ • Local Export      │  • Asset Management      │  • Load Balancing   │
│                     │                          │                     │
│  Tech: C#, WinUI 3, │  Tech: ASP.NET, Next.js, │ Tech: Kustomize,    │
│  .NET 9, Modular    │  PostgreSQL, MinIO       │ Kubernetes, Traefik │
│  Architecture       │                          │                     │
└─────────────────────┴──────────────────────────┴─────────────────────┘
         ↓                        ↓                       ↓
    Desktop-to-Web         Web-to-Infrastructure    Infrastructure
    Integration            API Calls, DB Access      Orchestration
```

---

## How Luxoria is Split

### 1. **Luxoria Desktop** (Standalone Application)
- **Purpose**: Local image processing and management on Windows
- **Technology**: C# with WinUI 3 and .NET 9.0
- **Architecture**: Modular, event-driven with plugin system
- **Key Features**:
  - Image import and organization
  - Advanced editing tools
  - Module-based extensibility
  - Direct integration with LuxStudio for uploads
  - Completely functional offline
- **Distribution**: Inno Setup installer or portable executable
- **No Cloud Required**: Can function entirely locally

### 2. **LuxStudio Web Platform** (Cloud Services)
- **Purpose**: Collaborative photography galleries and client management
- **Technology Stack**:
  - Backend: ASP.NET Core 8.0, PostgreSQL, MinIO
  - Frontend: Next.js 14, React 18, TypeScript
  - Real-Time: SignalR for live updates
- **Key Features**:
  - Publish galleries online
  - Client feedback and collaboration
  - Image analytics and engagement tracking
  - Secure asset storage and delivery
  - Multi-user administration
- **Deployment**: Containerized microservices
- **Requires Cloud**: Hosted environment with persistent storage

### 3. **Infrastructure & Deployment** (Kubernetes Cluster)
- **Purpose**: Reliable, scalable hosting of LuxStudio platform
- **Technology**: Kubernetes with Kustomize, Traefik, Let's Encrypt
- **Architecture**: Base configuration + environment overlays
- **Key Features**:
  - Production environment (Pluto)
  - Staging environment (Saturn)
  - Automatic SSL/TLS certificates
  - Load balancing and failover
  - Namespace isolation
  - Zero-configuration environment creation
- **Maintenance**: Minimal operational overhead

---

## Integration Points

### Desktop → Web
- Photographers upload images from Desktop directly to LuxStudio
- JWT authentication for secure API access
- SignalR real-time updates of gallery status
- Metadata and EXIF preservation

### Web → Infrastructure
- LuxStudio API runs on Kubernetes
- Database (PostgreSQL) persisted on cloud storage
- Object storage (MinIO) for image assets
- Automatic scaling based on demand

### Desktop ↔ Marketplace
- Built-in module discovery from GitHub releases
- One-click module installation
- Automatic updates from marketplace repository

---

## Table of Contents

### Part 1: Luxoria Desktop
1. [Desktop Technology Stack](#luxoria-desktop-technology-stack)
2. [Desktop Architectural Pattern](#desktop-architectural-pattern)
3. [Desktop Project Structure](#desktop-project-structure)
4. [Module System Architecture](#module-system-architecture)
5. [Standard Modules](#luxoria-standard-modules-luxstd)
6. [Marketplace System](#luxoria-marketplace-open--transparent-module-distribution)

### Part 2: LuxStudio Web Platform
7. [LuxStudio Technology Stack](#luxstudio-technology-stack)
8. [LuxStudio Architectural Pattern](#luxstudio-architectural-pattern)
9. [LuxAPI Backend](#luxapi-architecture)
10. [Portal Frontend](#portal-architecture)
11. [Desktop-Web Integration](#integration-with-luxoria-desktop)

### Part 3: Infrastructure & Deployment
12. [Kubernetes Cluster Architecture](#kubernetes-cluster-architecture)
13. [Kustomize Pattern](#the-kustomize-pattern-base--overlays)
14. [Base Configuration](#base-configuration)
15. [Overlay Configuration](#overlay-configuration)
16. [Building & Deploying](#building-and-deploying)

### Part 4: Cross-Platform Considerations
17. [Security & Best Practices](#security--best-practices)
18. [Performance Considerations](#performance-considerations)
19. [Key Architectural Decisions](#key-architectural-decisions)
20. [Future Enhancements](#future-architecture-considerations)
21. [Conclusion](#overall-conclusion)

---

# Part 1: Luxoria Desktop

## Luxoria Desktop Technology Stack

### Primary Language & Framework
- **Programming Language**: C# 
- **Framework**: .NET 9.0 (net9.0-windows10.0.26100.0)
- **UI Framework**: WinUI 3 (Windows App SDK 1.6)
- **Target Platform**: Windows 10 version 1809 (10.0.17763.0) and later
- **Windows SDK**: 10.0.26100.57

### Supported Architectures
- **x86** (32-bit Intel/AMD)
- **x64** (64-bit Intel/AMD)
- **ARM64** (64-bit ARM processors)

### Core Technologies
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection 9.0.0
- **Hosting**: Microsoft.Extensions.Hosting 9.0.0
- **Graphics Rendering**: SkiaSharp 3.116.1
- **UI Controls**: CommunityToolkit.WinUI 8.1

### Development Tools
- **IDEs**: Visual Studio 2022 / JetBrains Rider
- **Version Control**: Git / GitHub
- **CI/CD**: GitHub Actions
- **Testing Framework**: xUnit
- **Code Quality**: SonarQube Cloud
- **Deployment Format**: Inno Setup Wizard

---

## Desktop Architectural Pattern

Luxoria Desktop follows a **modular, event-driven architecture** with clear separation of concerns. The system is built on several key architectural principles:

### 1. Modular Architecture
The application is designed as a plugin-based system where functionality is organized into independent, reusable modules. This approach provides:
- **Extensibility**: New features can be added as modules without modifying core code
- **Maintainability**: Each module has a well-defined responsibility
- **Scalability**: Modules can be developed, tested, and deployed independently
- **Flexibility**: Features can be enabled/disabled dynamically

### 2. Event-Driven Communication
Modules communicate through an event bus system (`IEventBus`), enabling:
- **Loose Coupling**: Modules don't need direct references to each other
- **Asynchronous Operations**: Non-blocking event processing
- **Real-time Updates**: Components respond to changes across the application
- **Extensible Interactions**: New event types can be added without breaking existing code

### 3. Dependency Injection
The application uses Microsoft's DI container for:
- **Service Lifetime Management**: Singleton, Scoped, and Transient services
- **Testability**: Easy mocking and unit testing
- **Configuration**: Centralized service registration in `Startup.cs`
- **Decoupling**: Components depend on abstractions, not concrete implementations

### 4. Layered Architecture
The solution is organized into distinct layers, each with specific responsibilities:

```
┌─────────────────────────────────────────────────────────────┐
│                     Luxoria.App (Presentation)              │
│         WinUI 3 UI, Views, Components, Event Handlers       │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                  Luxoria.GModules (UI Components)           │
│          Shared Graphical Components & Controls             │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│              Luxoria.Modules (Module Management)            │
│      ModuleLoader, EventBus, ModuleContext, Services        │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                  Luxoria.Core (Business Logic)              │
│        Services, Repositories, Models, Interfaces           │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    Luxoria.SDK (Foundation)                 │
│     Shared Utilities, Interfaces, Logger, FileHasher        │
└─────────────────────────────────────────────────────────────┘
```

---

## Desktop Project Structure


The Luxoria Desktop solution consists of several interconnected projects:

### Core Projects

#### **Luxoria.App**
- **Purpose**: Main application entry point and presentation layer
- **Technology**: WinUI 3 with .NET 9
- **Key Components**:
  - `App.xaml.cs`: Application initialization and module loading
  - `MainWindow.xaml`: Primary application window with split-panel layout
  - `Views/`: Feature-specific views (ModuleManager, Marketplace, etc.)
  - `Components/`: Reusable UI components (MenuBar, Panels)
  - `EventHandlers/`: Event processing logic
  - `Services/`: UI-specific services
- **Responsibilities**:
  - Application lifecycle management
  - User interface rendering
  - Module coordination
  - Event routing

#### **Luxoria.Core**
- **Purpose**: Core business logic and services
- **Technology**: .NET 9.0
- **Key Components**:
  - `Services/`: Business logic implementations
  - `Models/`: Domain entities and data structures
  - `Interfaces/`: Core service contracts (IMarketplaceService, etc.)
  - `Helpers/`: Utility functions
- **Responsibilities**:
  - Business rule enforcement
  - Data management
  - Marketplace integration
  - Storage operations

#### **Luxoria.Modules**
- **Purpose**: Module system implementation
- **Technology**: .NET 9.0
- **Key Components**:
  - `ModuleLoader.cs`: Dynamic assembly loading and module discovery
  - `EventBus.cs`: Publish-subscribe event system
  - `ModuleContext.cs`: Module execution context
  - `Services/ModuleService.cs`: Module lifecycle management
  - `Interfaces/IModule.cs`: Module contract
- **Responsibilities**:
  - Loading external module assemblies
  - Module initialization and lifecycle
  - Inter-module communication via events
  - Module isolation and sandboxing

#### **Luxoria.GModules**
- **Purpose**: Graphical module components
- **Technology**: WinUI 3 with .NET 9
- **Key Components**:
  - `SmartButton.cs`: Advanced button control
  - `LuxMenuBarItem.cs`: Menu bar items
  - Custom UI controls for modules
- **Responsibilities**:
  - Shared UI components across modules
  - Consistent visual design
  - Reusable interactive elements

#### **Luxoria.SDK**
- **Purpose**: Software Development Kit for module developers
- **Technology**: .NET 9.0
- **Key Components**:
  - `Interfaces/ILoggerService.cs`: Logging contract
  - `Interfaces/IFileHasherService.cs`: File hashing contract
  - `Services/LoggerService.cs`: Logging implementation
  - `Services/FileHashService.cs`: File hashing with multiple algorithms
  - `Models/LogLevel.cs`: Structured logging levels
- **Responsibilities**:
  - Standardized logging across modules
  - File integrity verification
  - Common utilities for module development
  - Consistent API for third-party modules

### Test Projects

#### **Luxoria.Core.Tests**
- Unit tests for core business logic
- Tests: `ModuleServiceTests.cs`, `VaultAndStorageTests.cs`, `StartupTests.cs`

#### **Luxoria.Modules.Tests**
- Module system and event bus tests
- Tests: `ModuleLoaderTests.cs`, `BusTests.cs`, `ManifestTests.cs`, etc.

#### **Luxoria.SDK.Tests**
- SDK functionality tests
- Tests: `LoggerServiceTests.cs`

---

## Module System Architecture

### Module Interface (`IModule`)

All Luxoria modules must implement the `IModule` interface:

```csharp
public interface IModule
{
    string Name { get; }
    string Description { get; }
    string Version { get; }
    
    void Initialize(IEventBus eventBus, IModuleContext context, ILoggerService logger);
    void Execute();
    void Shutdown();
}
```

### Module Lifecycle

1. **Discovery**: `ModuleLoader` scans designated directories for `.dll` assemblies
2. **Loading**: Assemblies are loaded via reflection (`Assembly.LoadFrom`)
3. **Instantiation**: Types implementing `IModule` are instantiated
4. **Initialization**: `Initialize()` is called with event bus, context, and logger
5. **Execution**: `Execute()` begins module operation
6. **Shutdown**: `Shutdown()` is called during application termination

### Module Communication

Modules communicate exclusively through the `IEventBus` system:

```csharp
// Publishing events
eventBus.Publish(new ImageUpdateEvent(imageData));

// Subscribing to events
eventBus.Subscribe<OpenCollectionEvent>(OnCollectionOpened);
```

This decoupled approach ensures modules remain independent and testable.

---

## Standard Modules (LuxStd)

Luxoria includes several built-in standard modules:

### **LuxImport**
- **Purpose**: Asset importation and indexing
- **Features**:
  - Bulk image import from cameras/storage
  - Automatic metadata extraction
  - Collection organization
  - Asynchronous processing with progress tracking

### **LuxFilter**
- **Purpose**: Advanced asset filtering and scoring
- **Features**:
  - Multi-criteria filtering pipeline
  - Weighted scoring algorithms
  - Real-time filter application
  - Extensible filter framework

### **LuxEditor**
- **Purpose**: Image editing and manipulation
- **Features**:
  - Non-destructive editing
  - Layer-based operations
  - AI-powered enhancements
  - Color correction and retouching

### **LuxExport**
- **Purpose**: Asset export and delivery
- **Features**:
  - Multi-format export (JPEG, PNG, TIFF, PDF)
  - Batch processing
  - Quality presets
  - Watermarking

### **LuxStudio** (Desktop Module)
- **Purpose**: Web platform integration module for publishing to LuxStudio
- **Features**:
  - Gallery publishing
  - Client collaboration
  - Feedback collection
  - Remote asset sharing

---

## Luxoria Marketplace: Open & Transparent Module Distribution

### Overview

The **Luxoria Marketplace** is a revolutionary, fully open-source module distribution system that enables users to discover, download, and install modules seamlessly within the Luxoria Desktop application. Unlike traditional app stores with hidden infrastructure and proprietary systems, the Luxoria Marketplace operates with complete transparency, leveraging GitHub's public infrastructure and automated CI/CD pipelines.

**Marketplace Repository**: [github.com/LuxoriaSoft/marketplace](https://github.com/LuxoriaSoft/marketplace)

### Architecture & Design Philosophy

#### **Fully Open Source**
The entire marketplace infrastructure is open source, meaning:
- **Complete Visibility**: Every aspect of module distribution is public and auditable
- **No Hidden Systems**: No proprietary databases, secret APIs, or closed-source components
- **Community Trust**: Users can verify exactly what happens when they download a module
- **Educational Resource**: Developers can learn from and improve the system

#### **GitHub-Native Infrastructure**
The marketplace leverages GitHub's existing infrastructure:
- **GitHub Releases**: Modules are distributed as GitHub Release assets
- **Public Repository**: All modules, metadata, and documentation are version-controlled
- **GitHub API**: Module discovery uses the official GitHub REST API via Octokit
- **Asset Hosting**: GitHub's CDN handles all downloads at no cost

#### **CI/CD Automation**
The entire process is automated through GitHub Actions:
- **Automated Builds**: Module compilation happens in public CI workflows
- **Automated Testing**: Quality checks run before modules are published
- **Automated Releases**: Publishing is triggered by git tags or workflow dispatch
- **Transparent Pipeline**: Every build step is logged and visible to the community

### How It Works

#### **From the User Perspective**

1. **Browse Modules**
   - Open Luxoria Desktop and navigate to the Marketplace View
   - Browse available modules with descriptions, versions, and download counts
   - View module README files directly in the application

2. **Select Architecture**
   - Modules are built for x86, x64, and ARM64 architectures
   - The application automatically selects the appropriate build for your system
   - Multi-architecture support ensures compatibility across all Windows devices

3. **Download & Install**
   - Click to download a module directly from GitHub Releases
   - The application handles extraction and installation automatically
   - Modules are loaded dynamically without requiring an application restart

4. **Automatic Updates**
   - The marketplace checks for new module versions
   - Users are notified when updates are available
   - One-click updates keep modules current

#### **From the Developer Perspective**

1. **Submit Module**
   - Fork the marketplace repository
   - Add your module metadata and assets
   - Submit a pull request with your module

2. **Automated Validation**
   - CI pipeline validates module manifest format
   - Security checks ensure no malicious code
   - Build verification confirms cross-platform compatibility

3. **Publication**
   - Once approved, the module is automatically built for all architectures
   - GitHub Actions creates a new release with your module assets
   - Module appears immediately in all Luxoria Desktop installations worldwide

4. **Version Management**
   - Semantic versioning (SemVer) ensures predictable updates
   - Each version is tagged and permanently archived
   - Users can download previous versions if needed

### Technical Implementation

#### **MarketplaceService Architecture**

The marketplace integration is implemented via `IMarketplaceService`:

```csharp
public interface IMarketplaceService
{
    Task<ICollection<LuxRelease>> GetReleases();
    Task<ICollection<LuxRelease.LuxMod>> GetRelease(long releaseId);
}
```

**Key Implementation Details**:
- Uses **Octokit** (official GitHub API client) for reliable GitHub integration
- Asynchronous operations ensure UI responsiveness
- Built-in retry logic handles network interruptions
- Caches results to minimize API calls

#### **Module Discovery Process**

1. **Fetch Releases**: Query GitHub API for all releases in `LuxoriaSoft/marketplace`
2. **Parse Assets**: Each release contains architecture-specific module builds
3. **README Identification**: Modules with `.readme.md` assets include documentation
4. **Architecture Matching**: Filter assets by `.x86.dll`, `.x64.dll`, `.arm64.dll` suffixes
5. **Metadata Extraction**: Parse `luxmod.json` manifest files for module information

#### **Download & Installation Flow**

```
User Clicks "Install" 
    ↓
IMarketplaceService.GetRelease(releaseId)
    ↓
GitHub API returns release assets
    ↓
Filter assets by current architecture
    ↓
Download .dll from GitHub CDN
    ↓
Validate file hash (SHA256)
    ↓
Extract to %APPDATA%\Luxoria\Modules
    ↓
ModuleLoader discovers new module
    ↓
Module is initialized and activated
```

### Revolutionary Advantages

#### **1. Complete Transparency**
- **Open Build Process**: Every module build is logged in public GitHub Actions runs
- **Source Code Visibility**: Developers can inspect the entire pipeline
- **Audit Trail**: All changes are tracked in git history with full attribution
- **No Black Boxes**: Users know exactly where modules come from and how they're built

#### **2. Zero Infrastructure Costs**
- **No Databases**: Module metadata lives in git, not proprietary databases
- **No Servers**: GitHub hosts everything via its global CDN
- **No APIs to Maintain**: The GitHub REST API is our infrastructure
- **Free for All**: Both developers and users benefit from zero hosting costs
- **No Scaling Concerns**: GitHub automatically handles traffic and storage
- **Zero Maintenance Overhead**: No architecture to maintain, upgrade, or monitor
- **No Capacity Planning**: Storage scales infinitely without intervention
- **No DevOps Required**: No servers to patch, backup, or secure

#### **3. Community Empowerment**
- **Anyone Can Contribute**: Fork, modify, and improve the marketplace itself
- **Democratized Distribution**: No gatekeepers or approval committees
- **Feature Requests Welcome**: Want a new feature? Submit a PR to the marketplace repo
- **Collaborative Governance**: The community decides marketplace evolution

#### **4. Decentralized & Resilient**
- **No Single Point of Failure**: GitHub's infrastructure is globally distributed
- **Git-Based**: Even if GitHub goes down, modules can be distributed via git mirrors
- **Forkable**: Anyone can create their own marketplace fork for specialized modules
- **Censorship-Resistant**: No central authority can remove modules unilaterally

#### **5. Developer-Friendly**
- **Simple Workflow**: Tag a release, and CI does the rest
- **Multi-Architecture by Default**: Automatic builds for x86, x64, ARM64
- **Version Management**: Git tags provide natural versioning
- **Documentation Included**: README files travel with modules

#### **6. Security Through Transparency**
- **Public Scrutiny**: Malicious code would be immediately visible
- **Reproducible Builds**: Anyone can verify module integrity
- **Hash Verification**: SHA256 hashes prevent tampering
- **Community Review**: Pull requests are reviewed before merging

### How Collaborators Can Modify the Marketplace

The marketplace's open nature allows anyone to contribute improvements:

#### **Adding New Features**
1. **Fork** `github.com/LuxoriaSoft/marketplace`
2. **Clone** the repository locally
3. **Create** a feature branch (e.g., `feature/enhanced-search`)
4. **Implement** your improvement (e.g., better search, categories, ratings)
5. **Test** your changes with the CI pipeline
6. **Submit** a pull request with clear documentation
7. **Collaborate** with maintainers to refine and merge

#### **Example Contributions**
- **Enhanced Metadata**: Add module categories, tags, screenshots
- **Rating System**: Implement user reviews and ratings
- **Dependency Management**: Automatic resolution of module dependencies
- **Security Scanning**: Automated vulnerability checks for modules
- **Localization**: Multi-language support for module descriptions
- **Analytics**: Download statistics and popularity metrics

#### **Custom Marketplace Forks**
Organizations can create their own private or specialized marketplaces:
- Fork the repository for your organization
- Configure `MarketplaceService` to point to your fork
- Maintain internal modules with the same transparent process
- Merge upstream improvements as they're released

### Marketplace View in Luxoria Desktop

The application includes a dedicated **MarketplaceView** component:

**Features**:
- **Module Catalog**: Grid or list view of available modules
- **Search & Filter**: Find modules by name, category, or version
- **Module Details**: View README, version history, download counts
- **One-Click Install**: Download and install with a single button
- **Update Notifications**: Visual indicators for available updates
- **Installed Modules**: Manage currently installed modules

**Navigation**: `Main Menu → Modules → Marketplace`

### Module Manifest Format

Each module includes a `luxmod.json` manifest:

```json
{
  "name": "LuxEditor",
  "version": "1.2.0",
  "author": "LuxoriaSoft",
  "description": "Advanced image editing and manipulation",
  "url": "https://docs.luxoria.bluepelicansoft.com/modules/luxeditor",
  "dependencies": [],
  "architectures": ["x86", "x64", "arm64"]
}
```

This metadata is parsed by the marketplace service to populate the UI.

### Future Marketplace Enhancements

The open architecture enables exciting future possibilities:

- **Paid Modules**: Integration with GitHub Sponsors or payment gateways
- **Module Dependencies**: Automatic installation of required modules
- **Preview System**: Try modules without full installation
- **Rollback Mechanism**: Revert to previous module versions
- **Beta Channel**: Opt-in to pre-release module versions
- **Community Ratings**: User reviews and star ratings
- **Module Analytics**: Usage statistics for developers
- **Automated Updates**: Background module updates
- **Smart Recommendations**: AI-powered module suggestions based on usage

### Comparison to Traditional App Stores

| Feature | Luxoria Marketplace | Traditional App Store |
|---------|-------------------|---------------------|
| **Infrastructure** | GitHub (free, public) | Proprietary servers |
| **Source Code** | Fully open | Closed source |
| **Build Process** | Public CI (GitHub Actions) | Hidden build systems |
| **Costs** | $0 for hosting | Hosting fees + revenue share |
| **Approval Process** | Community-driven | Centralized review |
| **Modification** | Anyone can fork/improve | Vendor lock-in |
| **Transparency** | Complete visibility | Black box |
| **Censorship** | Decentralized (forkable) | Single authority |

### Getting Started with the Marketplace

**For Users**:
1. Open Luxoria Desktop
2. Navigate to `Modules → Marketplace`
3. Browse available modules
4. Click "Install" on any module
5. Start using new features immediately

**For Developers**:
1. Visit [github.com/LuxoriaSoft/marketplace](https://github.com/LuxoriaSoft/marketplace)
2. Read the contribution guidelines
3. Fork the repository
4. Submit your module via pull request
5. Watch your module become available to thousands of users

**For Contributors**:
1. Identify an improvement opportunity
2. Fork the marketplace repository
3. Implement and test your feature
4. Submit a PR with clear documentation
5. Collaborate with the community to merge

---

## Application Flow

### Startup Sequence

1. **Application Launch** (`App.xaml.cs::OnLaunched`)
   - Initialize dependency injection container
   - Configure services via `Startup.cs`
   - Display splash screen with version info

2. **Module Discovery** (`LoadModulesAsync`)
   - Scan module directories
   - Load module assemblies
   - Instantiate module instances
   - Update splash screen with progress

3. **Service Initialization**
   - Resolve required services from DI container
   - Initialize `EventBus`, `LoggerService`, `ModuleService`
   - Configure module UI service

4. **Main Window Creation**
   - Close splash screen
   - Create `MainWindow` with split-panel layout
   - Initialize UI components (menu bar, panels)
   - Activate window

5. **Module Initialization**
   - Call `Initialize()` on each module
   - Provide event bus, context, and logger
   - Subscribe to application events

6. **Module Execution**
   - Call `Execute()` on each module
   - Modules begin processing events
   - Application ready for user interaction

### Runtime Behavior

- **Event-Driven**: User actions trigger events that modules respond to
- **Asynchronous**: Long-running operations use async/await patterns
- **Responsive**: UI remains interactive during background processing
- **Extensible**: New modules can be added without recompilation

---

## UI Architecture

### Main Window Layout

The main window uses a **split-panel layout** with five primary areas:

```
┌─────────────────────────────────────────────────────────────┐
│                      Main Menu Bar                          │
├──────────┬───────────────────────────────┬──────────────────┤
│          │                               │                  │
│   Left   │        Center Canvas          │    Right Panel   │
│  Panel   │      (Main Content)           │    (Properties)  │
│(Explorer)│                               │                  │
│          │                               │                  │
├──────────┴───────────────────────────────┴──────────────────┤
│                    Bottom Panel (Timeline/Info)             │
└─────────────────────────────────────────────────────────────┘
```

- **Main Menu Bar**: Context-aware actions and module controls
- **Left Panel**: File explorer, collections, module navigation
- **Center Canvas**: Primary workspace (image viewing, editing, etc.)
- **Right Panel**: Properties, metadata, adjustment controls
- **Bottom Panel**: Timeline, progress indicators, status information

### Resizable & Adaptive
- Panels are resizable with **GridSplitter** controls
- Minimum/maximum width constraints prevent unusable layouts
- State is preserved across sessions

---

## Deployment & Distribution

### Packaging Format
- **Inno Setup**: Built application and associated DLLs packaged into Inno Stup using ISS wizard
- **Self-Contained**: Includes .NET 9 runtime (no external dependencies)
- **Windows App SDK**: Self-contained (no framework dependencies)

### Installation Options
- **Inno Setup**: Built application and associated DLLs packaged into Inno Stup using ISS wizard
- **Portable Version**: Unpackaged executable for advanced users & option for all-in-one updater system

### Update Strategy
- **Module Updates**: Modules can be updated independently
- **Rollback Support**: Previous versions preserved for recovery

### Download Location
- **Primary**: [luxoria.bluepelicansoft.com](https://luxoria.bluepelicansoft.com)
- **GitHub Releases**: [github.com/LuxoriaSoft/Luxoria/releases](https://github.com/LuxoriaSoft/Luxoria/releases)
- **Documentation**: [docs.luxoria.bluepelicansoft.com](https://docs.luxoria.bluepelicansoft.com)

---

## Key Architectural Decisions

### Why WinUI 3?
- **Modern UI**: Fluent Design System with native Windows 11 styling
- **Performance**: Hardware-accelerated rendering via DirectX
- **Future-Proof**: Microsoft's current UI framework for Windows
- **Rich Controls**: Comprehensive control library with accessibility support

### Why Modular Architecture?
- **Extensibility**: Third-party developers can create custom modules
- **Maintainability**: Changes to one module don't affect others
- **Performance**: Unused modules can be disabled to reduce memory usage
- **Distribution**: Modules can be sold/distributed separately

### Why Event Bus Pattern?
- **Decoupling**: Modules communicate without direct dependencies
- **Scalability**: New event types can be added without breaking changes
- **Testability**: Events can be mocked and verified in unit tests
- **Flexibility**: Multiple modules can respond to the same event

### Why .NET 9?
- **Modern Features**: Latest C# language features (records, pattern matching, etc.)
- **Performance**: Significant performance improvements over previous versions
- **Long-Term Support**: Enterprise-grade stability and security
- **Cross-Platform SDK**: Shared codebase potential for future Linux/macOS ports

---

# Part 2: LuxStudio Web Platform

## LuxStudio Technology Stack

### Code Signing
- All binaries are signed with LuxoriaSoft certificate
- MSIX packages are signed for Windows SmartScreen compatibility

### Sandboxing
- Modules run in the same process but with restricted capabilities
- File system access controlled through `IModuleContext`

### Logging & Diagnostics
- Comprehensive logging via `ILoggerService`
- Error telemetry (opt-in) for crash reporting
- Diagnostic logs written to `%APPDATA%\Luxoria\Logs`

### Data Protection
- User data stored in isolated application directories
- Sensitive data encrypted using Windows Data Protection API (DPAPI)

---

## Performance Considerations

### Asynchronous Operations
- All I/O operations use async/await
- Image processing offloaded to background threads
- UI remains responsive during long-running tasks

### Memory Management
- Lazy loading of large assets
- Image thumbnails cached for quick display
- Unused modules unloaded to free memory

### Rendering Optimization
- SkiaSharp for hardware-accelerated graphics
- Virtual scrolling for large collections
- Progressive image loading

---

## Future Architecture Considerations

### Potential Enhancements
- **Cross-Platform**: .NET MAUI or Avalonia for Linux/macOS support
- **Cloud Integration**: Direct upload to cloud storage providers
- **AI Services**: Cloud-based AI enhancement services
- **Collaborative Editing**: Real-time multi-user editing
- **WebAssembly Modules**: Browser-based module execution

### Scalability Plans
- **Database Backend**: SQLite/PostgreSQL for large collections
- **Microservices**: Split processing into separate services
- **Container Support**: Docker deployment for server components

---

## Security & Best Practices (Desktop)

### Code Signing
- All binaries are signed with LuxoriaSoft certificate
- MSIX packages are signed for Windows SmartScreen compatibility

### Sandboxing
- Modules run in the same process but with restricted capabilities
- File system access controlled through `IModuleContext`

### Logging & Diagnostics
- Comprehensive logging via `ILoggerService`
- Error telemetry (opt-in) for crash reporting
- Diagnostic logs written to `%APPDATA%\Luxoria\Logs`

### Data Protection
- User data stored in isolated application directories
- Sensitive data encrypted using Windows Data Protection API (DPAPI)

---

## Performance Considerations (Desktop)

### Asynchronous Operations
- All I/O operations use async/await
- Image processing offloaded to background threads
- UI remains responsive during long-running tasks

### Memory Management
- Lazy loading of large assets
- Image thumbnails cached for quick display
- Unused modules unloaded to free memory

### Rendering Optimization
- SkiaSharp for hardware-accelerated graphics
- Virtual scrolling for large collections
- Progressive image loading

---

---

## LuxStudio Technology Stack

**LuxStudio** is the cloud-based companion platform to Luxoria Desktop, providing photographers with a powerful web interface for client collaboration, gallery sharing, and remote asset management. Built as a modern, containerized microservices architecture, LuxStudio enables photographers to publish galleries online, collect client feedback, and manage projects from anywhere.

**Live Platforms**:
- **Pluto Environment**: [studio.pluto.luxoria.bluepelicansoft.com](https://studio.pluto.luxoria.bluepelicansoft.com)
- **Saturn Environment**: [studio.saturn.luxoria.bluepelicansoft.com](https://studio.saturn.luxoria.bluepelicansoft.com)

**Repository**: Located in `LuxStudio/` directory within the main Luxoria repository

---

### Backend (LuxAPI)

#### **Core Technologies**
- **Language**: C#
- **Framework**: ASP.NET Core 8.0 (net8.0)
- **Runtime**: .NET 8.0
- **API Architecture**: RESTful API with SignalR for real-time communication
- **Containerization**: Docker with Linux containers

#### **Database & Storage**
- **Primary Database**: PostgreSQL 17.2 (Alpine Linux 3.21)
  - Entity Framework Core 9.0.1 for ORM
  - Npgsql.EntityFrameworkCore.PostgreSQL 9.0.3 for PostgreSQL integration
- **Object Storage**: MinIO (S3-compatible)
  - Image asset storage and delivery
  - Bucket-based organization
  - CDN-style content distribution

#### **Key Libraries & Services**
- **Authentication**: JWT Bearer Tokens (Microsoft.AspNetCore.Authentication.JwtBearer 8.0.12)
- **Password Hashing**: BCrypt.Net-Next 4.0.3
- **Real-Time Communication**: SignalR (Microsoft.AspNetCore.SignalR 1.2.0)
- **Email Services**: SendGrid 9.29.3
- **API Documentation**: Swashbuckle (Swagger) 6.6.2

#### **Architecture Patterns**
- **Repository Pattern**: DAL (Data Access Layer) abstractions
- **Service Layer**: Business logic separation
- **Dependency Injection**: Built-in ASP.NET Core DI container
- **Background Services**: Hosted services for cleanup tasks

### Frontend

#### **Portal v1 (Legacy - Vue.js)**
- **Framework**: Vue 3.5.13 with TypeScript
- **Build Tool**: Vite 6.2.0
- **Routing**: Vue Router 4.5.0
- **Styling**: 
  - TailwindCSS 3.4.17
  - DaisyUI 4.12.23 (component library)
- **HTTP Client**: Axios 1.7.9
- **Real-Time**: SignalR Client 8.0.7

#### **Portal v2 (Next.js - Current)**
- **Framework**: Next.js 14.2.3 (React 18)
- **Language**: TypeScript 5
- **Styling**: 
  - TailwindCSS 4.1.7
  - Headless UI 2.2.4 (accessible components)
  - Heroicons 2.1.3 (icon library)
- **Animations**: Framer Motion 11.2.6
- **HTTP Client**: Axios 1.10.0
- **Real-Time**: SignalR Client 8.0.7
- **File Processing**: 
  - JSZip 3.10.1 (bulk downloads)
  - FileSaver 2.0.5 (client-side downloads)
- **Code Quality**: 
  - ESLint with Next.js config
  - Prettier 3.3.2 with plugins

### Infrastructure & DevOps

#### **Containerization**
- **Container Runtime**: Docker 27.4.0+
- **Orchestration**: Docker Compose (development) / Kubernetes (production)
- **Base Images**:
  - PostgreSQL: `postgres:17.2-alpine3.21`
  - .NET API: Custom .NET 8.0 image
  - Frontend: Nginx-based static hosting

---

## Kubernetes Cluster Architecture

### Overview

LuxStudio uses a **declarative, template-based Kubernetes architecture** that leverages **Kustomize** for configuration management. This approach enables multiple isolated environments (Pluto and Saturn) to be deployed from a single base configuration with environment-specific overlays, eliminating duplication and ensuring consistency across all deployments.

**Key Philosophy**: Define once, deploy everywhere with minimal configuration drift.

### The Kustomize Pattern: Base + Overlays

#### **What is Kustomize?**

Kustomize is a native Kubernetes templating system that uses a **composition-based approach** rather than templates. It's built into `kubectl` (no external dependencies needed) and allows you to:

- Define a **base** configuration with all standard components
- Create **overlays** that override specific values for different environments
- Compose manifests declaratively without variable substitution

**Why Kustomize Instead of Alternatives?**

| Aspect | Kustomize | Helm | Templating (Envsubst) |
|--------|-----------|------|----------------------|
| **Learning Curve** | Gentle (native YAML) | Steep (Go templates) | Very steep (scripting) |
| **Native Support** | Built into kubectl | External tool | Not a standard |
| **Configuration Reuse** | Strategic merge | Values files | Environment variables |
| **Version Control** | Git-friendly YAML | Complex charts | Hard to track changes |
| **Debugging** | `kustomize build` | `helm template` | Manual expansion |
| **Dependency Management** | Built-in composition | Package management | Manual dependencies |
| **Production Ready** | Yes | Yes | Not recommended |

We chose **Kustomize** because:
1. **No Learning Curve**: Uses standard Kubernetes YAML
2. **Native Integration**: Part of kubectl, no additional tools
3. **Git-Friendly**: All configurations version-controlled, easily reviewable
4. **Composition Over Templating**: Declarative approach prevents template complexity
5. **Simple Overlays**: Easy to understand environment differences

#### **Directory Structure**

```
LuxStudio/depl/
│
├── base/                              # Common base configuration
│   ├── kustomization.yaml             # Kustomize base definition
│   ├── namespace.yaml                 # Base namespace
│   ├── deployment-luxapi.yaml         # API deployment
│   ├── deployment-luxstudio.yaml      # Frontend deployment
│   ├── deployment-luxdb.yaml          # Database deployment
│   ├── deployment-minio.yaml          # Object storage deployment
│   ├── service.yaml                   # Kubernetes services
│   ├── luxdb-init.sql                 # Database initialization SQL
│   └── minio-passwdgen.sh            # MinIO password generation
│
└── overlays/                          # Environment-specific customizations
    ├── pluto/                         # Production environment
    │   ├── kustomization.yaml         # Pluto overlay definition
    │   ├── namespace.yaml             # Pluto namespace
    │   ├── ingress.yaml               # Production ingress (TLS, domains)
    │   ├── patch-deployment-luxapi.yaml   # Production API patches
    │   ├── patch-deployment-luxstudio.yaml # Production frontend patches
    │   ├── configmap-luxdb-init.yaml  # Production DB config
    │   └── luxapi-secrets.sh          # Secret generation
    │
    └── saturn/                        # Staging environment
        ├── kustomization.yaml         # Saturn overlay definition
        ├── namespace.yaml             # Saturn namespace
        ├── ingress.yaml               # Staging ingress
        ├── patch-deployment-luxapi.yaml   # Staging API patches
        ├── patch-deployment-luxstudio.yaml # Staging frontend patches
        ├── configmap-luxdb-init.yaml  # Staging DB config
        └── luxapi-secrets.sh          # Secret generation
```

### Base Configuration

#### **What Goes in Base?**

The `base/` directory contains the **common foundation** shared across all environments:

**1. Namespace (`namespace.yaml`)**
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: default  # Generic namespace (overridden by overlays)
```

**2. Service Definitions (`service.yaml`)**
- ClusterIP service for LuxAPI (internal only)
- ClusterIP service for LuxDB (internal only)
- ClusterIP service for MinIO (internal only)
- NodePort service for LuxStudio (exposed via ingress)

**3. Deployments (4 core services)**

**a) LuxAPI Deployment** (`deployment-luxapi.yaml`)
- Base image: `luxoria/luxapi:latest`
- Replicas: 1 (customized per environment)
- CPU/Memory: Baseline requests/limits
- Init container: Waits for PostgreSQL readiness
- Liveness/Readiness probes: Health checks
- Environment variables: Base values (overridden by overlays)

**b) LuxStudio Frontend** (`deployment-luxstudio.yaml`)
- Base image: `luxoria/luxstudio:latest`
- Replicas: 1 (customized per environment)
- Port: 3000 (Next.js default)
- No init containers: Stateless, can start immediately

**c) PostgreSQL Database** (`deployment-luxdb.yaml`)
- Image: `postgres:17.2-alpine3.21`
- Replicas: 1 (not meant for HA in base)
- Storage: `emptyDir` in base (replaced by PersistentVolume in overlays)
- Init scripts: Loaded from ConfigMap
- Readiness probe: `pg_isready` command

**d) MinIO Object Storage** (`deployment-minio.yaml`)
- Image: `quay.io/minio/minio:latest`
- Replicas: 1 (single-node, not distributed)
- Storage: `emptyDir` in base (replaced by persistent storage in overlays)
- Console ports: 9000 (API), 9001 (Web UI)
- Arguments: `server /data --console-address 0.0.0.0:9001`

**4. Kustomization Definition** (`kustomization.yaml`)
```yaml
resources:
  - deployment-minio.yaml
  - deployment-luxstudio.yaml
  - deployment-luxapi.yaml
  - deployment-luxdb.yaml
  - service.yaml
```

### Overlay Configuration

#### **Pluto Overlay (Production)**

**Purpose**: Production-grade deployment with security, performance, and reliability focus.

**File: `overlays/pluto/kustomization.yaml`**
```yaml
resources:
  - ../../base                  # Include all base manifests
  - namespace.yaml              # Production namespace
  - configmap-luxdb-init.yaml   # Production DB configuration
  - ingress.yaml                # Production ingress with TLS

namespace: luxstudio-pluto     # All resources in pluto namespace

patches:
  - path: patch-deployment-luxstudio.yaml  # Frontend customizations
  - path: patch-deployment-luxapi.yaml     # API customizations

images:
  - name: luxoria/luxapi
    newTag: latest              # Use stable release tag
  - name: luxoria/luxstudio
    newTag: latest              # Use stable release tag
```

**Key Customizations:**

1. **Namespace Isolation**: `luxstudio-pluto`
   - All resources isolated in dedicated namespace
   - Prevents accidental interference with other deployments
   - Enables role-based access control (RBAC) per environment

2. **Image Tags**: Production-specific versions
   - Base uses `latest` (development)
   - Pluto overrides to stable release versions
   - Enables rollback to known-good versions

3. **API Patches** (`patch-deployment-luxapi.yaml`)
   ```yaml
   apiVersion: apps/v1
   kind: Deployment
   metadata:
     name: luxapi-deployment
   spec:
     replicas: 3  # High availability
     template:
       spec:
         containers:
           - name: luxapi-container
             resources:
               requests:
                 cpu: "500m"
                 memory: "512Mi"
               limits:
                 cpu: "1000m"
                 memory: "1Gi"
             env:
               - name: ENVIRONMENT
                 value: PRODUCTION
               - name: URI__FrontEnd
                 value: https://studio.pluto.luxoria.bluepelicansoft.com
               - name: URI__Backend
                 value: https://api.studio.pluto.luxoria.bluepelicansoft.com
               - name: Jwt__Key
                 valueFrom:
                   secretKeyRef:
                     name: luxapi-secrets
                     key: JWT_KEY
               - name: Smtp__Host
                 value: smtp.gmail.com
               - name: Smtp__Port
                 value: "587"
               - name: Smtp__Username
                 valueFrom:
                   secretKeyRef:
                     name: luxapi-secrets
                     key: SMTP_USER
               - name: Smtp__Password
                 valueFrom:
                   secretKeyRef:
                     name: luxapi-secrets
                     key: SMTP_PASSWORD
   ```

4. **Frontend Patches** (`patch-deployment-luxstudio.yaml`)
   ```yaml
   apiVersion: apps/v1
   kind: Deployment
   metadata:
     name: luxstudio-deployment
   spec:
     replicas: 2  # High availability
     template:
       spec:
         containers:
           - name: luxstudio-container
             resources:
               requests:
                 cpu: "250m"
                 memory: "256Mi"
               limits:
                 cpu: "500m"
                 memory: "512Mi"
             env:
               - name: API_URL
                 value: https://api.studio.pluto.luxoria.bluepelicansoft.com
               - name: NEXT_PUBLIC_API_URL
                 value: https://api.studio.pluto.luxoria.bluepelicansoft.com
   ```

5. **Production Ingress** (`ingress.yaml`)
   ```yaml
   apiVersion: networking.k8s.io/v1
   kind: Ingress
   metadata:
     name: pluto-ingress
     namespace: luxstudio-pluto
     annotations:
       kubernetes.io/ingress.class: traefik
       cert-manager.io/cluster-issuer: letsencrypt-production
   spec:
     rules:
       - host: studio.pluto.luxoria.bluepelicansoft.com
         http:
           paths:
             - path: /
               pathType: Prefix
               backend:
                 service:
                   name: luxstudio-service
                   port:
                     number: 80
       - host: api.studio.pluto.luxoria.bluepelicansoft.com
         http:
           paths:
             - path: /
               pathType: Prefix
               backend:
                 service:
                   name: luxapi-service
                   port:
                     number: 8080
       - host: bucket.pluto.luxoria.bluepelicansoft.com
         http:
           paths:
             - path: /
               pathType: Prefix
               backend:
                 service:
                   name: minio-service
                   port:
                     number: 9000
       - host: console.bucket.pluto.luxoria.bluepelicansoft.com
         http:
           paths:
             - path: /
               pathType: Prefix
               backend:
                 service:
                   name: minio-service
                   port:
                     number: 9001
     tls:
       - hosts:
           - studio.pluto.luxoria.bluepelicansoft.com
           - api.studio.pluto.luxoria.bluepelicansoft.com
           - bucket.pluto.luxoria.bluepelicansoft.com
           - console.bucket.pluto.luxoria.bluepelicansoft.com
         secretName: pluto-tls-cert
   ```

#### **Saturn Overlay (Staging)**

**Purpose**: Testing environment that mirrors production but with lower resource requirements.

**Key Differences from Pluto:**
- Namespace: `luxstudio-saturn`
- Replicas: 1 (cost optimization)
- Resource limits: Lower than production
- ENVIRONMENT: `STAGING`
- Domains: `*.saturn.luxoria.bluepelicansoft.com`
- Image tags: `dev` or `staging` (development versions)

### Technical Implementation Details

#### **1. Strategic Merge Patches**

Kustomize uses **Strategic Merge Patches** (SMP) to intelligently merge configurations:

```yaml
# Base deployment has:
spec:
  replicas: 1
  template:
    spec:
      containers:
        - name: luxapi-container
          image: luxoria/luxapi:latest
          env:
            - name: PORT
              value: "8080"

# Overlay patch adds/modifies:
spec:
  replicas: 3
  template:
    spec:
      containers:
        - name: luxapi-container
          resources:
            requests:
              cpu: "500m"

# Result: Replicas = 3, PORT remains "8080", resources added
```

**Why this matters**:
- Changes don't overwrite base values
- Additive approach prevents accidental deletions
- Predictable, debuggable merging behavior

#### **2. Image Tag Substitution**

```yaml
# Overlay kustomization.yaml
images:
  - name: luxoria/luxapi
    newTag: v1.2.3         # Replace all luxoria/luxapi:* with :v1.2.3
  - name: luxoria/luxstudio
    newTag: v2.1.0         # Replace all luxoria/luxstudio:* with :v2.1.0
```

**Advantages**:
- Single source of truth for image versions
- No manual manifest editing
- Easy promotion through environments
- Audit trail in git history

#### **3. Namespace Isolation**

```yaml
namespace: luxstudio-pluto  # Automatically applies to all resources
```

**Benefits**:
- Resource quota per environment
- Network policies per environment
- RBAC policies per environment
- Clean separation for cleanup/deletion

#### **4. Secret Management**

Secrets are stored **outside** version control (in cluster or external manager):

```bash
# Generate secrets in cluster
kubectl create namespace luxstudio-pluto
kubectl -n luxstudio-pluto create secret generic luxapi-secrets \
  --from-literal=JWT_KEY=$(openssl rand -base64 32) \
  --from-literal=SMTP_USER="your-email@gmail.com" \
  --from-literal=SMTP_PASSWORD="your-app-password"

kubectl -n luxstudio-pluto create secret generic minio-secret \
  --from-literal=MINIO_ROOT_USER="minioadmin" \
  --from-literal=MINIO_ROOT_PASSWORD=$(openssl rand -base64 32)
```

### Building and Deploying

#### **Building Manifests**

```bash
# Build production manifests
kustomize build overlays/pluto > pluto-manifests.yaml

# Build staging manifests
kustomize build overlays/saturn > saturn-manifests.yaml

# Verify before applying
kubectl apply -f pluto-manifests.yaml --dry-run=client
```

#### **Deploying**

```bash
# Deploy to production (Pluto)
kubectl apply -k overlays/pluto

# Deploy to staging (Saturn)
kubectl apply -k overlays/saturn

# Update images
kubectl set image deployment/luxapi-deployment \
  luxapi-container=luxoria/luxapi:v1.2.3 \
  -n luxstudio-pluto

# Rollback to previous version
kubectl rollout undo deployment/luxapi-deployment -n luxstudio-pluto
```

#### **Monitoring Deployment**

```bash
# Watch deployment progress
kubectl rollout status deployment/luxapi-deployment -n luxstudio-pluto

# View pod status
kubectl get pods -n luxstudio-pluto

# View deployment details
kubectl describe deployment luxapi-deployment -n luxstudio-pluto

# View logs
kubectl logs -f deployment/luxapi-deployment -n luxstudio-pluto
```

### Why This Architecture?

#### **1. Single Source of Truth**

All configurations derive from the base. Changes to common features happen once, propagate everywhere.

```
base/ (1 copy of shared config)
  ↓
overlays/pluto/ (small delta file: replicas, image tags, resources)
overlays/saturn/ (small delta file: replicas, image tags, resources)
```

**Alternative Rejected**: Separate manifests for each environment
- ❌ Duplication: 100+ lines duplicated per environment
- ❌ Maintenance nightmare: Bug fix must be applied 3 times
- ❌ Drift risk: Environments diverge accidentally

#### **2. Git-Friendly**

All configurations are standard YAML. Diffs show exactly what changes between environments.

```bash
git diff overlays/pluto/kustomization.yaml overlays/saturn/kustomization.yaml
- newTag: latest     (pluto uses stable)
+ newTag: dev        (saturn uses development)
```

**Alternative Rejected**: Helm charts with Go templates
- ❌ Templates are hard to read and debug
- ❌ Requires Helm tool (external dependency)
- ❌ Diffs show template variables, not actual values

#### **3. Environment Parity**

Pluto and Saturn run identical configurations except for:
- Image tags
- Resource limits
- Number of replicas
- Domain names
- Secrets

This ensures staging is a true production replica.

**Alternative Rejected**: Different Kubernetes clusters for each environment
- ❌ Expensive (2+ clusters, 2+ control planes)
- ❌ Difficult to test infrastructure changes
- ❌ Overkill for our scale

#### **4. Easy Onboarding**

New developers see the directory structure and immediately understand:
- What's common (base/)
- What's different (overlays/)
- How to add a new environment (copy overlay, modify names)

**Alternative Rejected**: Terraform + Helm + Kubectl
- ❌ Requires learning 3 tools
- ❌ State management complexity
- ❌ Longer deployment feedback loop

#### **5. Namespace-per-Environment**

Each environment is completely isolated:

```bash
kubectl get namespaces
NAME                  STATUS
luxstudio-pluto       Active    # Production
luxstudio-saturn      Active    # Staging
kube-system           Active
```

**Benefits**:
- Can delete entire environment: `kubectl delete namespace luxstudio-saturn`
- Network policies can restrict cross-namespace traffic
- RBAC can grant different permissions per namespace
- Resource quotas prevent one environment starving another

#### **6. Multi-Region Ready**

To add a third environment (e.g., `asia` region):

```bash
cp -r overlays/pluto overlays/asia
# Edit overlays/asia/kustomization.yaml
#   - Change namespace to luxstudio-asia
#   - Change domain names to asia.luxoria.bluepelicansoft.com
#   - Set image tags to asia-specific version

kubectl apply -k overlays/asia
```

### Deployment Workflow

```
Developer commits code to GitHub
         ↓
GitHub Actions builds Docker images
         ↓
Images pushed to registry (luxoria/luxapi:commit-hash)
         ↓
Developer runs: kubectl set image deployment/luxapi-deployment \
                  luxapi-container=luxoria/luxapi:commit-hash \
                  -n luxstudio-saturn
         ↓
Kustomize reads saturn/kustomization.yaml
         ↓
Loads base/ manifests
         ↓
Applies saturn/ patches and overrides
         ↓
Kubernetes creates/updates resources in luxstudio-saturn namespace
         ↓
Pods are scheduled and started
         ↓
Init containers run (wait for dependencies)
         ↓
Containers start
         ↓
Liveness/readiness probes verify health
         ↓
Ingress routes traffic to new pods
```

### Storage & Persistence

#### **Current Approach (Development)**

```yaml
volumes:
  - name: pgdata
    emptyDir: {}        # Pod-local, ephemeral storage
```

**Characteristics**:
- ✅ Simple, no storage provisioning needed
- ✅ Good for testing and development
- ❌ Data lost when pod restarts
- ❌ Can't survive node failures

#### **Production Improvements (Future)**

**Option 1: PersistentVolume Claims (Recommended)**
```yaml
volumes:
  - name: pgdata
    persistentVolumeClaim:
      claimName: pg-pvc

---
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: pg-pvc
  namespace: luxstudio-pluto
spec:
  accessModes: [ "ReadWriteOnce" ]
  resources:
    requests:
      storage: 100Gi
```

**Option 2: StatefulSet + Persistent Volumes**
```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: luxdb
spec:
  volumeClaimTemplates:
    - metadata:
        name: pgdata
      spec:
        accessModes: [ "ReadWriteOnce" ]
        resources:
          requests:
            storage: 100Gi
```

**Option 3: Managed Database Service**
- AWS RDS PostgreSQL
- Azure Database for PostgreSQL
- Kubernetes Operator (CloudNativePG)

### Resource Management

#### **Current Setup**

```yaml
# Base: No resource limits (development)
# Overlay: Adds production limits
containers:
  - name: luxapi-container
    resources:
      requests:
        cpu: "500m"
        memory: "512Mi"
      limits:
        cpu: "1000m"
        memory: "1Gi"
```

**Requests**: Kubernetes reserves this amount
**Limits**: Pod killed if it exceeds this

#### **Scaling Strategy**

```yaml
# Future: Horizontal Pod Autoscaler
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: luxapi-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: luxapi-deployment
  minReplicas: 3
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
```

### Monitoring & Observability

#### **Health Checks**

```yaml
containers:
  - name: luxapi-container
    livenessProbe:
      httpGet:
        path: /health
        port: 8080
      initialDelaySeconds: 30
      periodSeconds: 10
    readinessProbe:
      httpGet:
        path: /ready
        port: 8080
      initialDelaySeconds: 10
      periodSeconds: 5
```

#### **Logging**

```bash
# View logs from pod
kubectl logs -n luxstudio-pluto deployment/luxapi-deployment

# Follow logs in real-time
kubectl logs -f -n luxstudio-pluto deployment/luxapi-deployment

# See logs from all replicas
kubectl logs -n luxstudio-pluto -l app=luxapi-app
```

---

## Kubernetes Ingress Configuration

#### **Ingress Controller**: Traefik
- **Why Traefik?**
  - Built-in TLS termination
  - Automatic certificate management (Let's Encrypt)
  - Middleware support (rate limiting, authentication)
  - Simple, declarative routing
  - Lower memory footprint than Nginx Ingress

#### **TLS/SSL**: Let's Encrypt with cert-manager
- **Automatic Certificate Issuance**: Certificates created automatically
- **Renewal**: Auto-renewed 30 days before expiration
- **Multiple Certificates**: One per domain, or wildcard

#### **Domain Routing**

```yaml
Pluto (Production):
  - studio.pluto.luxoria.bluepelicansoft.com    → LuxStudio Portal
  - api.studio.pluto.luxoria.bluepelicansoft.com → LuxAPI Backend
  - bucket.pluto.luxoria.bluepelicansoft.com     → MinIO API
  - console.bucket.pluto.luxoria.bluepelicansoft.com → MinIO Console

Saturn (Staging):
  - studio.saturn.luxoria.bluepelicansoft.com    → LuxStudio Portal
  - api.studio.saturn.luxoria.bluepelicansoft.com → LuxAPI Backend
  - bucket.saturn.luxoria.bluepelicansoft.com     → MinIO API
  - console.bucket.saturn.luxoria.bluepelicansoft.com → MinIO Console
```

---

## Comparison: What We Chose vs. Alternatives

| Aspect | Kustomize (Chosen) | Helm | Terraform |
|--------|-------------------|------|-----------|
| **Templating** | Native YAML + patches | Go templates | HCL templating |
| **Learning** | Minimal (YAML knowledge) | Steep (charts, values) | Medium (HCL) |
| **Reusability** | Overlays | Chart packages | Modules |
| **Version Control** | Excellent (git diffs) | Good | Good |
| **Secrets** | External or sealed-secrets | Values/Helm Secrets | tfstate files |
| **Integration** | kubectl built-in | Separate tool | Separate tool |
| **Multi-env** | Simple overlays | Helm values override | Workspaces/modules |
| **Debugging** | `kustomize build` | `helm template` | `terraform plan` |
| **State Management** | None (immutable) | None (stateless) | tfstate (stateful) |

---

## Current Kubernetes Setup

### Cluster Configuration

**Deployment Environments**:
1. **Pluto**: Production environment
   - Fully replicated and highly available
   - 3 API replicas, 2 frontend replicas
   - SSL/TLS with Let's Encrypt
   - Production secrets and credentials

2. **Saturn**: Staging environment
   - Minimal replication (1 replica each)
   - Development image tags
   - Staging secrets
   - Used for testing before production

**Scaling**:
- API: 1 → 3 replicas (CPU/memory-based HPA in future)
- Frontend: 1 → 2 replicas
- Database: 1 replica (single point currently)
- MinIO: 1 replica (single-node currently)

**Next Steps for Production Readiness**:
1. Add persistent volumes for database and object storage
2. Implement database replication/failover
3. Add horizontal pod autoscaling (HPA)
4. Implement pod disruption budgets (PDB)
5. Add network policies for pod-to-pod communication
6. Implement distributed tracing (Jaeger)
7. Add Prometheus monitoring and Grafana dashboards

---

## Kubernetes Deployment

---

## Portal Architecture

### Next.js Frontend (v2)

#### **Routing Structure**
- **Server-Side Rendering (SSR)**: Initial page loads for SEO
- **Client-Side Navigation**: Instant page transitions
- **Dynamic Routes**: Gallery-specific URLs
- **API Routes**: Next.js API endpoints for backend proxying

#### **Key Components**

**Public Pages**:
- **Gallery Viewer**: Browse shared galleries without authentication
- **Image Lightbox**: Full-screen image viewing with navigation
- **Client Feedback**: Comment and rating system

**Authenticated Pages**:
- **Dashboard**: Photographer workspace overview
- **Gallery Management**: Create, edit, delete galleries
- **Upload Interface**: Bulk image upload with progress
- **Client Management**: Invite and manage client access
- **Analytics**: View statistics and engagement metrics

#### **State Management**
- React Context API for global state
- Local component state for UI interactions
- SignalR connection state management

#### **Styling Architecture**
- **TailwindCSS Utility Classes**: Rapid UI development
- **Headless UI Components**: Accessible, unstyled primitives
- **Framer Motion**: Smooth animations and transitions
- **Responsive Design**: Mobile-first approach

#### **Performance Optimizations**
- **Next.js Image Optimization**: Automatic image resizing and WebP conversion
- **Code Splitting**: Route-based lazy loading
- **Static Generation**: Pre-render static pages at build time
- **Incremental Static Regeneration**: Update static pages without rebuilding

---

## Deployment Architecture

### Docker Compose (Development)

```yaml
services:
  luxdb:        # PostgreSQL database
  luxapi:       # ASP.NET Core API
  luxportal:    # Next.js frontend
  minio:        # Object storage
```

**Features**:
- Single command startup: `docker-compose up`
- Automatic service dependencies
- Volume persistence for database data
- Health checks for service readiness

### Kubernetes (Production)

#### **Base Manifests** (`depl/base/`)
- Deployment configurations for all services
- Service definitions (ClusterIP, LoadBalancer)
- ConfigMaps for application configuration
- PersistentVolumeClaims for data storage

#### **Environment Overlays** (`depl/overlays/`)
- **Pluto**: Production configuration
  - High availability (multiple replicas)
  - Resource limits (CPU/memory)
  - Production database credentials (secrets)
  - MinIO production configuration
- **Saturn**: Staging configuration
  - Lower resource allocation
  - Test data and debugging enabled

#### **Ingress Configuration**
```yaml
Hosts:
  - studio.pluto.luxoria.bluepelicansoft.com    # Portal frontend
  - bucket.pluto.luxoria.bluepelicansoft.com    # MinIO API
  - console.bucket.pluto.luxoria.bluepelicansoft.com  # MinIO Console
```

**Features**:
- Automatic SSL/TLS via Let's Encrypt
- Path-based routing to services
- WebSocket support for SignalR
- Rate limiting and DDoS protection (Traefik)

#### **Deployment Strategy**
- **Rolling Updates**: Zero-downtime deployments
- **Health Checks**: Kubernetes monitors service health
- **Auto-Scaling**: Horizontal pod autoscaling based on load
- **Secrets Management**: Kubernetes secrets for sensitive data

---

## Key Features

### For Photographers

#### **Gallery Management**
- Create unlimited galleries with custom names and descriptions
- Organize images into collections within galleries
- Set gallery visibility (public, private, password-protected)
- Bulk upload with drag-and-drop interface

#### **Client Collaboration**
- Share gallery links with clients
- Allow clients to favorite/flag images
- Collect comments and feedback on specific images
- Download selected images in bulk (ZIP)

#### **Analytics & Insights**
- View gallery access statistics
- Track client engagement (views, downloads, favorites)
- Monitor upload progress and storage usage

### For Clients

#### **Gallery Viewing**
- Browse shared galleries without account creation
- Full-screen lightbox image viewer
- Zoom and pan for image details
- Mobile-responsive interface

#### **Feedback System**
- Flag favorite images for photographer
- Leave comments on specific images
- Rate images (if enabled)
- Download approved images

---

## Integration with Luxoria Desktop

### Desktop-to-Web Workflow

1. **Export from Desktop**
   - Photographer selects images in Luxoria Desktop
   - Uses LuxStudio module to upload to web platform
   - Assigns images to specific gallery

2. **Authentication**
   - Desktop module uses JWT tokens for API authentication
   - Tokens stored securely in Windows Credential Manager

3. **Upload Process**
   - Desktop generates thumbnails locally
   - Uploads full-resolution images to MinIO
   - Registers images with LuxAPI database
   - Real-time progress via SignalR

4. **Metadata Sync**
   - EXIF data preserved during upload
   - Tags and collections synchronized
   - Editing history maintained

### Module Communication

The **LuxStudio Desktop Module** acts as a bridge:
- Consumes LuxAPI REST endpoints
- Maintains SignalR connection for live updates
- Handles authentication token refresh
- Provides UI within Luxoria Desktop for web operations

---

## Security Architecture

### API Security

#### **Authentication Flow**
1. User logs in with email/password
2. Password verified with BCrypt
3. JWT token generated with user claims
4. Token returned to client
5. Client includes token in `Authorization` header for subsequent requests

#### **Password Security**
- BCrypt hashing with salt
- No plain-text password storage
- Configurable hash cost factor

#### **CORS Protection**
- Whitelist frontend origins
- Credential support for cookies/auth
- Preflight request handling

### Data Security

#### **Database Security**
- Connection string stored in environment variables
- PostgreSQL SSL connections (production)
- Database user with minimal privileges
- Regular automated backups

#### **Object Storage Security**
- Pre-signed URLs with expiration
- Bucket-level access policies
- Per-object access control
- Encryption at rest (MinIO)

#### **Network Security**
- TLS 1.3 for all external connections
- Internal services communicate over private network
- Kubernetes NetworkPolicies for pod isolation

---

## Scalability & Performance

### Horizontal Scaling

#### **API Scaling**
- Stateless API design enables multiple replicas
- Kubernetes Horizontal Pod Autoscaler (HPA)
- Load balancing across API instances
- Session state stored in database (not memory)

#### **Frontend Scaling**
- Static assets served via CDN (Next.js automatic optimization)
- Multiple portal replicas behind load balancer
- Client-side caching with service workers

#### **Database Scaling**
- PostgreSQL read replicas for query load distribution
- Connection pooling (Npgsql)
- Query optimization and indexing

#### **Storage Scaling**
- MinIO distributed mode for multi-node storage
- Automatic bucket sharding
- CDN integration for global content delivery

### Performance Optimizations

#### **Backend**
- Asynchronous I/O throughout (async/await)
- Database query optimization (EF Core eager loading)
- Response caching for frequently accessed data

#### **Frontend**
- Image lazy loading
- Virtual scrolling for large galleries
- Progressive image loading (blur-up technique)
- Code splitting and tree shaking

---

## Monitoring & Observability

### Application Logging
- Structured logging via ASP.NET Core ILogger
- Log aggregation to centralized system
- Error tracking and alerting
- Performance metrics collection

### Health Checks
- Kubernetes liveness probes
- Kubernetes readiness probes
- Database connectivity checks
- MinIO availability monitoring

### Analytics
- User activity tracking (ActivityLogService)
- API endpoint performance metrics
- Gallery engagement statistics
- Storage usage monitoring

---

## Key Architectural Decisions

### Why ASP.NET Core?
- **Performance**: High-throughput HTTP server
- **Cross-Platform**: Runs on Linux containers
- **Ecosystem**: Rich library ecosystem and tooling
- **Integration**: Seamless .NET desktop integration

### Why Next.js for Frontend?
- **SEO**: Server-side rendering for search engines
- **Performance**: Automatic code splitting and optimization
- **Developer Experience**: Hot reloading, TypeScript support
- **Deployment**: Edge-ready with Vercel-style deployments

### Why PostgreSQL?
- **Reliability**: ACID compliance and data integrity
- **Performance**: Advanced query optimization
- **Features**: JSON support, full-text search, extensions
- **Open Source**: No licensing costs

### Why MinIO?
- **S3 Compatibility**: Standard API for object storage
- **Self-Hosted**: Complete control over data
- **Performance**: High-throughput object storage
- **Cost-Effective**: No per-request charges like AWS S3

### Why Kubernetes?
- **Orchestration**: Automated deployment and scaling
- **Resilience**: Self-healing and automatic restarts
- **Portability**: Cloud-agnostic infrastructure
- **Ecosystem**: Rich tooling (Helm, Kustomize, etc.)

---

## Environment Configuration

### Pluto (Production)
- **Domain**: `studio.pluto.luxoria.bluepelicansoft.com`
- **Purpose**: Live production environment for end-users
- **Characteristics**:
  - High availability configuration
  - Multi-replica deployments
  - Production-grade SSL certificates
  - Comprehensive monitoring and alerting

### Saturn (Staging)
- **Domain**: `studio.saturn.luxoria.bluepelicansoft.com`
- **Purpose**: Pre-production testing and validation
- **Characteristics**:
  - Mirror of production architecture
  - Test data and debugging enabled
  - Canary deployments for new features
  - Performance testing environment

---

## Future Enhancements

### Planned Features
- **WebRTC Video Calling**: Real-time client consultations
- **AI-Powered Selection**: Automatic best-photo recommendations
- **Mobile Apps**: Native iOS/Android applications
- **Collaborative Editing**: Real-time multi-user gallery curation
- **Payment Integration**: Direct client billing for services
- **White-Label Solution**: Custom branding for photographers

### Technical Improvements
- **GraphQL API**: Flexible data fetching
- **Redis Caching**: Distributed cache for improved performance
- **Elasticsearch**: Advanced search and filtering
- **Prometheus/Grafana**: Enhanced observability
- **Service Mesh (Istio)**: Advanced traffic management

---

## Conclusion

LuxStudio represents a modern, cloud-native web platform that seamlessly complements Luxoria Desktop. Built on a robust microservices architecture with ASP.NET Core, Next.js, PostgreSQL, and Kubernetes, LuxStudio provides photographers with enterprise-grade tools for client collaboration and gallery management.

The platform's containerized design ensures scalability, portability, and resilience, while its integration with Luxoria Desktop creates a unified workflow from capture to delivery. With multiple deployment environments (Pluto and Saturn) and comprehensive security measures, LuxStudio is production-ready for photographers of all scales.

**Live Platforms**:
- **Pluto**: [studio.pluto.luxoria.bluepelicansoft.com](https://studio.pluto.luxoria.bluepelicansoft.com)
- **Saturn**: [studio.saturn.luxoria.bluepelicansoft.com](https://studio.saturn.luxoria.bluepelicansoft.com)

---

# Overall Conclusion

The **Luxoria Platform** (Desktop + Studio) represents a complete photography ecosystem built on modern, open-source technologies. From the modular WinUI 3 desktop application to the containerized Next.js web platform, every component is designed for extensibility, transparency, and community collaboration.

Luxoria Desktop represents a sophisticated, modular photography management platform built on modern .NET technologies. Its event-driven architecture, combined with a powerful module system, provides photographers with a flexible, extensible tool that can adapt to diverse workflows. The use of WinUI 3 ensures a polished, native Windows experience while maintaining high performance through careful architectural decisions.

The platform is available for download at **luxoria.bluepelicansoft.com** and continues to evolve with community feedback and contributions.

---

**License**: Apache 2.0  
**Repository**: [github.com/LuxoriaSoft/Luxoria](https://github.com/LuxoriaSoft/Luxoria)  
**Documentation**: [docs.luxoria.bluepelicansoft.com](https://docs.luxoria.bluepelicansoft.com)
