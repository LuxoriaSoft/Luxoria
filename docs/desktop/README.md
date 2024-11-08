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
- **Framework**: WinUI 3 with .NET 8
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
- **Technologies**: WinUI 3 with .NET 8
- **Target SDK Version**: net8.0-windows10.0.19041.0
- **Minimum Platform Version**: 10.0.17763.0
- **Using Windows SDL Package Version**: 10.0.19041.38
- **App.xaml/App.xaml.cs**: Entry point of the application

Application is divided into modules, each module is a separate project in the solution. Each module is responsible for a specific feature or a group of features. Each module is a separate project in the solution.

### Luxoria.Core
- **Technologies**: .NET 8.0
- **Target Framework**: net8.0
- **Luxoria.Core**: Contains fundamental services, repositories, and models

This project contains the core services, repositories, and models used across the application.

### Luxoria.Modules
- **Technologies**: .NET 8.0
- **Target Framework**: net8.0
- **Luxoria.Modules**: Module management for the app contains (Models, Interfaces, Services)

This project contains the module management system for the application. Each module is a separate project in the solution.

## How does communication work inside Luxoria ?
The communication between the modules and core program works by using `EventBus`.

### What is `EventBus` ?
`EventBus` is an implementation of the `Mediator` pattern. It allows the modules to communicate with each other without having to know about each other. This makes the application more modular and easier to maintain.

With `EventBus`, modules can send messages to each other without having to know about each other. This makes the application more modular and easier to maintain.  
The purpose of the `EventBus` is to decouple the modules from each other, so that they can be developed and tested independently.

### How does it work ?
`EventBus` works by registering `EventHandlers` for specific `Events`. When an `Event` is raised, the `EventBus` will call all the `EventHandlers` that are registered for that `Event`.  
So, when a module wants to send a message to another module, it raises an `Event` and the `EventBus` will call the `EventHandlers` that are registered for that `Event`.

### How to use it ?
`EventBus` is implemented as a singleton, so you can access it from anywhere in the application (Luxoria.App).

To use `EventBus`, you need to:
1. Create an `Event` class.
```csharp
public class MyEvent
{
    public required string Message { get; set; }
}
```

2. Create an `EventHandler` function.
```csharp
void OnMyEvent(MyEvent e)
{
    Console.WriteLine(e.Message);
}
```

2. (b) If you want to use async `EventHandler` function.
```csharp
async Task OnMyEvent(MyEvent e)
{
    await Task.Delay(1000);
    Console.WriteLine(e.Message);
}
```

3. Register the `EventHandler` for the `Event`.
```csharp
EventBus.Subscribe<MyEvent>(OnMyEvent);
```

4. Raise the `Event`.
```csharp
await EventBus.Publish(new MyEvent { Message = "Hello, World!" });
```

For more information, go to  : [EventBus implementation](../../Luxoria.App/Luxoria.Modules/Interfaces/IEventBus.cs)

For more information about `Event` : please refer to [LuxEvents]()