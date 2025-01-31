# Luxoria Desktop

## Table of Contents
- [Technical Stack](#technical-stack)
- [Project Structure](#project-structure)
- [Development](#development)
- [Testing](#testing)
- [Deployment](#deployment)
- [CI/CD](#ci-cd)
- [Contributing](#contributing)
- [Luxoria Standard Modules](#luxoria-standard-modules)

---

## Technical Stack
- **Language**: C#
- **Framework**: WinUI 3 with .NET 9
- **IDE**: Visual Studio 2022 / Rider
- **Version Control**: Git
- **CI/CD**: GitHub Actions
- **Deployment**: .MSIX
- **Testing**: xUnit

---

## Project Structure

Luxoria Desktop is structured as follows:

- **Luxoria.App**: Main project containing the primary application logic.
- **Luxoria.Desktop.Tests**: Unit tests for ensuring application stability and correctness.
- **Luxoria.Core**: Provides fundamental services, repositories, and models essential for the application.
- **Luxoria.Modules**: Manages modular components of the application, including models, interfaces, and services.
- **Luxoria.SDK**: Houses shared utilities, services, and interfaces to ensure consistency across modules.

### Luxoria.App
- **Technologies**: WinUI 3 with .NET 9
- **Target SDK Version**: `net9.0-windows10.0.26100.0`
- **Minimum Platform Version**: `10.0.17763.0`
- **Windows SDK Package Version**: `10.0.26100.57`
- **Main Entry Point**: `App.xaml` / `App.xaml.cs`

The application follows a modular architecture where each module is a separate project within the solution, responsible for specific features or functionality.

### Luxoria.Core
- **Technologies**: .NET 9.0
- **Target Framework**: `net9.0`
- **Functionality**: Contains core services, repositories, and models used across the application.

### Luxoria.Modules
- **Technologies**: .NET 9.0
- **Target Framework**: `net9.0`
- **Functionality**: Manages modules within the application, ensuring modularization and scalability.

Each module is developed as a separate project to provide distinct functionalities while maintaining interoperability.

### Luxoria.SDK

#### Overview
Luxoria.SDK provides essential utilities, interfaces, and services to ensure consistency across Luxoria modules. It standardizes logging, file hashing, and module communication, promoting reusability and efficiency.

#### Core Components

**Interfaces**
- `IFileHasherService`: Defines contract for file hashing operations.
- `ILoggerService`: Provides structured logging capabilities.
- `ILogTarget`: Specifies logging targets for various outputs.

**Models**
- `LogLevel`: Defines log severity levels for structured logging.

**Services**
- `FileHashService`: Implements file hashing with multiple algorithms.
- `LoggerService`: Provides system-wide and module-specific logging.

#### Benefits
- **Code Reusability**: Centralizes common functionalities across modules.
- **Scalability**: Easily extensible for new services and features.
- **Consistency**: Ensures uniformity in logging, file management, and cross-module communication.

---

## Luxoria Standard Modules (LuxStd Modules)

### Overview
Luxoria Standard Modules (LuxStd Modules) provide core functionalities within the Luxoria ecosystem. They follow a standardized architecture, ensuring seamless integration, extensibility, and interoperability.

### Key Features
- **Modular Design**: Follows a plug-and-play structure.
- **Event-Driven**: Utilizes `IEventBus` for module communication.
- **Extensible**: Allows enhancement without breaking compatibility.
- **Asynchronous Processing**: Optimized for high-performance and non-blocking operations.
- **Structured Logging & Error Handling**: Implements `ILoggerService` for debugging and diagnostics.

### List of LuxStd Modules
#### 1. `LuxImport`
- Manages importation, indexing, and organization of digital asset collections.
- Processes large datasets with asynchronous operations and real-time progress tracking.

#### 2. `LuxFilter`
- Provides an advanced filtering pipeline for processing and scoring digital assets.
- Implements a modular filtering framework with weighted execution for improved accuracy.

### Read More
- [LuxStd Modules Overview](luxstd/README.md)
- [LuxImport Technical Documentation](luxstd/LUXIMPORT.md)
- [LuxFilter Technical Documentation](luxstd/LUXFILTER.md)

---

## Conclusion
Luxoria Desktop follows a modular architecture, ensuring scalability, maintainability, and high performance. LuxStd Modules serve as the foundation of the ecosystem, providing essential functionalities in an event-driven and extensible manner. The inclusion of Luxoria.SDK ensures consistency across different modules, making development more efficient and streamlined.
