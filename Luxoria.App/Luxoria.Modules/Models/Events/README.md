# Luxoria Modules - Event Documentation

This document describes the various events used in the **Luxoria.Modules.Models.Events** namespace, their purpose, and technical details.

---

## List of Events
1. **FilterCatalogEvent** - Requests and retrieves a list of available filters.
2. **CollectionUpdatedEvent** - Notifies when a collection has been updated.
3. **LogEvent** - Sends a log message to the logging system.
4. **RequestWindowHandleEvent** - Requests the native window handle.
5. **TextInputEvent** - Sends a text input event.

---

## 1. FilterCatalogEvent
### Purpose
The `FilterCatalogEvent` is used to request and retrieve a list of available filters asynchronously.

### Technical Details
- **Namespace:** `Luxoria.Modules.Models.Events`
- **Implements:** `IEvent`
- **Response Handling:** Uses `TaskCompletionSource<List<(string Name, string Description, string Version)>>` to handle asynchronous responses.

### Event Flow
1. A component publishes the event.
2. A subscriber (e.g., a filter manager) listens and provides the filter list.
3. The event’s `TaskCompletionSource` is completed with the filter list.
4. The publisher receives the list.

### Code Example
```csharp
var filterEvent = new FilterCatalogEvent();
await _eventBus.Publish(filterEvent);
var receivedFilters = await filterEvent.Response.Task;
```

---

## 2. CollectionUpdatedEvent
### Purpose
The `CollectionUpdatedEvent` notifies subscribers when a collection has been updated.

### Technical Details
- **Namespace:** `Luxoria.Modules.Models.Events`
- **Implements:** `IEvent`
- **Properties:**
  - `CollectionName` (string) - The name of the updated collection.
  - `CollectionPath` (string) - The file path of the collection.
  - `Assets` (ICollection<LuxAsset>) - The updated assets in the collection.

### Event Flow
1. A module publishes this event after updating a collection.
2. Subscribers (e.g., UI components) react to the update.

### Code Example
```csharp
var updateEvent = new CollectionUpdatedEvent("MyCollection", "/path/to/collection", assets);
_eventBus.Publish(updateEvent);
```

---

## 3. LogEvent
### Purpose
The `LogEvent` is used to send log messages for debugging or monitoring purposes.

### Technical Details
- **Namespace:** `Luxoria.Modules.Models.Events`
- **Implements:** `IEvent`
- **Properties:**
  - `Message` (string) - The log message.

### Event Flow
1. Any component can publish this event to log information.
2. A logger service listens for this event and processes the message.

### Code Example
```csharp
_eventBus.Publish(new LogEvent("This is a log message."));
```

---

## 4. RequestWindowHandleEvent
### Purpose
The `RequestWindowHandleEvent` allows a component to request the native window handle.

### Technical Details
- **Namespace:** `Luxoria.Modules.Models.Events`
- **Implements:** `IEvent`
- **Properties:**
  - `OnHandleReceived` (Action<nint>) - Callback to return the window handle.

### Event Flow
1. A component publishes this event with a callback.
2. A subscriber (e.g., the main window) calls the callback with the window handle.

### Code Example
```csharp
var requestEvent = new RequestWindowHandleEvent(handle => Debug.WriteLine($"Handle: {handle}"));
_eventBus.Publish(requestEvent);
```

---

## 5. TextInputEvent
### Purpose
The `TextInputEvent` is used to transmit a text input from one component to another.

### Technical Details
- **Namespace:** `Luxoria.Modules.Models.Events`
- **Implements:** `IEvent`
- **Properties:**
  - `Text` (string) - The text input.
- **Validation:** Ensures the text is not `null` or empty.

### Event Flow
1. A component publishes this event with a text message.
2. A subscriber processes the text input.

### Code Example
```csharp
_eventBus.Publish(new TextInputEvent("User input text"));
```

---

## Conclusion
This document outlines the core events used in Luxoria's event-driven architecture. Each event plays a crucial role in communication between components, improving modularity and scalability.
