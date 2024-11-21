# Luxoria Desktop

## Summary
- [Technical Stack](#technical-stack)
- [Project Structure](#project-structure)
- [Development](#development)
- [Testing](#testing)
- [Deployment](#deployment)
- [CI/CD](#ci/cd)
- [Contributing](#contributing)

## Technical Stack
- **Language**: C#
- **Framework**: WinUI 3 with .NET 9
- **IDE**: Visual Studio 2022 / Rider
- **Version Control**: Git
- **CI/CD**: GitHub Actions
- **Deployment**: .MSIX
- **Testing**: xUnit

## Project Structure
**Luxoria.App** is composed of the following projects:
- **Luxoria.App**: Main project
- **Luxoria.Desktop.Tests**: Unit tests
- **Luxoria.Core**: Fundamental services, repositories, and models
- **Luxoria.Modules**: Module management for the app contains (Models, Interfaces, Services)

### Luxoria.App
- **Technologies**: WinUI 3 with .NET 9
- **Target SDK Version**: net9.0-windows10.0.26100.0
- **Minimum Platform Version**: 10.0.17763.0
- **Using Windows SDK Package Version**: 10.0.26100.57
- **App.xaml/App.xaml.cs**: Entry point of the application

Application is divided into modules, each module is a separate project in the solution. Each module is responsible for a specific feature or a group of features. Each module is a separate project in the solution.

### Luxoria.Core
- **Technologies**: .NET 9.0
- **Target Framework**: net9.0
- **Luxoria.Core**: Contains fundamental services, repositories, and models

This project contains the core services, repositories, and models used across the application.

### Luxoria.Modules
- **Technologies**: .NET 9.0
- **Target Framework**: net9.0
- **Luxoria.Modules**: Module management for the app contains (Models, Interfaces, Services)

This project contains the module management system for the application. Each module is a separate project in the solution.

