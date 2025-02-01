# LuxImport Technical Documentation

## Overview
LuxImport is a modular system designed for importing, indexing, and managing digital collections efficiently. It integrates with the Luxoria ecosystem and provides functionalities for handling structured asset collections.

## Features
- **Event-driven architecture** with `IEventBus` integration
- **Asynchronous operations** for improved performance
- **Modular and scalable** design
- **Progress tracking** through event notifications
- **Parallel processing** for asset loading

---

## Architecture

### Core Components

#### `LuxImport` Module
- Implements `IModule`
- Handles initialization, execution, and shutdown
- Listens to `OpenCollectionEvent` and triggers import process

#### `ImportService`
- Manages the actual import process
- Tracks progress and sends updates via event notifications
- Loads assets into memory

#### `ManifestRepository`
- Handles metadata storage and retrieval
- Ensures collection initialization and integrity

#### `LuxConfigRepository`
- Stores per-asset configuration
- Manages LuxCfg models in `.lux/assets`

#### `ImageDataHelper`
- Loads image assets efficiently
- Uses SkiaSharp for image decoding

---

## How to Use

### Prerequisites
- Ensure that the **Luxoria ecosystem** is installed and configured properly.
- Your project should reference the necessary **LuxImport libraries**.
- Implement and register `IEventBus` for event-driven interactions.

### Installation
1. Clone or download the **LuxImport** module.
2. Add the project reference in your Luxoria-based application.
3. Implement `IModule` interface in your module system.

### Running the Module
1. Initialize the module using `Initialize()`.
2. Subscribe to the `OpenCollectionEvent`.
3. When an import request is triggered, LuxImport processes the collection.
4. Once completed, `CollectionUpdatedEvent` is published.

### Example Usage
```csharp
var luxImport = new LuxImport();
luxImport.Initialize(eventBus, context, logger);
luxImport.Execute();
```

---

## Initialization

```csharp
public void Initialize(IEventBus eventBus, IModuleContext context, ILoggerService logger)
```
- Subscribes to `OpenCollectionEvent`
- Initializes logger and context

## Import Process

### **1. Collection Validation**
- Checks if the collection path is valid and initialized

### **2. Indexing & Asset Processing**
- Reads metadata from `manifest.json`
- Hashes and stores asset references
- Uses parallel processing for efficiency

### **3. Asset Loading**
- Loads assets into memory
- Uses `ImageDataHelper` for efficient decoding

### **4. Completion & Event Publishing**
- Publishes `CollectionUpdatedEvent` upon successful import

---

## Event Handling

### **`OpenCollectionEvent` Listener**
```csharp
_eventBus.Subscribe<OpenCollectionEvent>(HandleOnOpenCollectionAsync);
```
- Triggers collection import
- Sends progress updates

### **`CollectionUpdatedEvent` Publisher**
```csharp
_eventBus?.Publish(new CollectionUpdatedEvent(collectionName, collectionPath, assets));
```
- Notifies the system of import completion

---

## Error Handling & Logging
- Uses `ILoggerService` for structured logs
- Exception handling ensures robust error reporting

---

## Performance Optimizations
- Uses **parallel processing** in `LoadAssets`
- Minimizes **blocking I/O operations**
- Implements **batch logging** to prevent log spam

---

## Future Enhancements
- **Cache system** for faster re-imports
- **Support for more file formats**
- **UI integration** for progress monitoring

---

## Conclusion
LuxImport is designed for efficient, modular asset importation and indexing within the Luxoria ecosystem. Its event-driven design and asynchronous processing make it highly scalable and reliable.