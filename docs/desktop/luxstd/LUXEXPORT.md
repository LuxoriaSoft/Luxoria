# LuxExport Technical Documentation

## Overview
LuxExport is a modular export system integrated into the Luxoria ecosystem. It allows users to export collections of images in various formats with customizable file naming, export locations, and conflict handling strategies.

## Features
- **Multi-format export support** using SkiaSharp
- **Customizable file naming** with pattern-based presets
- **Conflict resolution options**: overwrite, rename, skip
- **Support for subfolder creation** and base path selection
- **Live progress display** during export operations
- **Modular design** and integration with `IEventBus`

---

## Architecture

### Core Components

#### `LuxExport` Module
- Implements `IModule` and `IModuleUI`
- Handles initialization and shutdown
- Subscribes to `CollectionUpdatedEvent`
- Instantiates and controls the main `Export` dialog

#### `ExportViewModel`
- ViewModel for binding export settings to the UI
- Manages file naming, export paths, format settings, and presets

#### `ExporterFactory`
- Static factory that returns an `IExporter` implementation based on `ExportFormat`
- Supports JPEG, PNG, BMP, GIF, WEBP, AVIF, and many others

#### `ExportProgressWindow`
- UI window that displays export progress
- Handles exporting on a background thread
- Provides preview and pause/cancel support

#### `TagResolverManager`
- Resolves tags in naming patterns like `{name}`, `{date}`, `{counter}`, and `meta:` keys
- Allows dynamic generation of file names

---

## How to Use

### Prerequisites
- Ensure that the **Luxoria ecosystem** is installed and configured
- Register the module via your main application’s module loader
- Implement and provide `IEventBus`, `IModuleContext`, and `ILoggerService`

### Installation
1. Add the **LuxExport** module to your Luxoria project.
2. Connect the module to the UI via smart button configuration.
3. Load file naming presets from a JSON file.

### Running the Module
1. Call `Initialize()` with required dependencies.
2. Trigger `Export` via UI interaction.
3. Choose export location, format, and file naming options.
4. Launch export via the main button.
5. View progress in the export window.

### Example Usage
```csharp
var luxExport = new LuxExport();
luxExport.Initialize(eventBus, context, logger);
luxExport.Execute();
```

---

## Initialization

```csharp
public void Initialize(IEventBus eventBus, IModuleContext context, ILoggerService logger)
```
- Subscribes to `CollectionUpdatedEvent`
- Registers UI components
- Sets up `Export` dialog

## Export Process

### **1. Image Collection Setup**
- Receives image data and metadata from `CollectionUpdatedEvent`
- Calls `SetBitmaps(...)` on the export dialog

### **2. Export Configuration**
- Users select format, color space, quality
- Optionally define size limit and subfolder behavior

### **3. File Naming**
- Supports dynamic patterns like `{name}_{date}`
- Applies extension casing and resolves conflicts

### **4. Export Execution**
- Uses `ExporterFactory` to get exporter instance
- Applies settings and writes to disk (or simulated web)
- Displays progress bar and image preview

---

## Event Handling

### **`CollectionUpdatedEvent` Listener**
```csharp
_eventBus.Subscribe<CollectionUpdatedEvent>(OnCollectionUpdated);
```
- Receives the updated list of assets
- Extracts `SKBitmap` and metadata
- Passes them to `Export` dialog

---

## Error Handling & Logging
- Uses `ILoggerService` for structured logging
- Exceptions during export are caught and logged
- Displays error messages in the progress window if needed

---

## Performance Optimizations
- Export runs on a background thread using `Task.Run`
- UI updates dispatched via `DispatcherQueue`
- SkiaSharp is used for efficient image encoding

---

## Future Enhancements
- **Export presets** based on project type
- **Metadata filtering** before export
- **Batch export reports** generation

---

## Conclusion
LuxExport provides a flexible and user-friendly way to export images in the Luxoria ecosystem. Its modular structure and advanced export options make it a powerful tool for end-users and developers alike.

---

**Last updated**: April 2025

