# Layers Organization and Dependency Rules

NopCommerce follows a **N-Tier Layered Architecture**, where the codebase is logically separated into distinct layers. The fundamental dependency rule of this architecture is that **dependencies only flow downwards**. Upper layers depend on lower layers, but lower layers have no knowledge of the layers above them.

The system is organized into the following primary layers, from top to bottom:

### 1. Presentation Layer (`Nop.Web`)
* **Responsibility:** Handles UI rendering, HTTP request routing, and API endpoints. It acts as the entry point for users interacting with the store.
* **Dependency Rule:** Depends on all lower layers (primarily `Nop.Services` and `Nop.Core`).

### 2. Business Logic Layer (`Nop.Services`)
* **Responsibility:** The "brain" of the application where core business rules, calculations (like taxes and discounts), and workflows are executed. It orchestrates actions between the database and the domain entities.
* **Dependency Rule:** Depends heavily on `Nop.Core` (for domain models) and `Nop.Data` (for database access/repositories). It is completely decoupled from the Presentation Layer.

### 3. Data Access Layer (`Nop.Data`)
* **Responsibility:** Acts as the translation layer between C# code and the SQL database using Entity Framework Core. It defines the mapping configurations (using the Fluent API) to convert domain entities into database tables and provides the Repository implementation.
* **Dependency Rule:** Depends entirely on `Nop.Core` to know which entities it needs to persist.

### 4. Domain Layer (`Nop.Core`)
* **Responsibility:** The foundational layer of the system. It contains the core domain entities (e.g., `Customer`, `Order`, `Product`), common helper classes, and crucial abstract interfaces (like `IEngine` for Dependency Injection and `IEventPublisher` for internal messaging).
* **Dependency Rule:** This layer sits at the very bottom and **depends on no other layers** within the NopCommerce solution.


# Internal Event Handling and `IEventPublisher`

NopCommerce handles events internally using an **In-Process Publisher/Subscriber** pattern. It does not use external message brokers like RabbitMQ or Kafka. Instead, the events are handled synchronously within the application itself.

### The Role of `IEventPublisher`

The `IEventPublisher` interface, defined in the `Nop.Core` layer, is the main tool used for this event system. Its primary purpose is to keep the code **decoupled** (a practice known as Separation of Concerns).

Without this event publisher, a core service like the one that processes orders would need to directly interact with every other service dealing with the side-effects of an order (such as sending emails, updating inventory, or giving reward points). This would make the code messy and highly dependent. By using `IEventPublisher`, the order service simply announces that an order was placed and finishes its job. Specialized "consumer" classes listen for this announcement and handle their own specific tasks independently.

### Event Processing Lifecycle

The event mechanism works in four structured steps:

1. **Publishing:** A service triggers an event by calling the `PublishAsync` method (e.g., `await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));`).
2. **Finding the Listeners:** The concrete `EventPublisher` (inside `Nop.Services`) receives this call. It then dynamically asks the Dependency Injection container (`IServiceProvider`) to find all the classes in the application that are registered to listen to this specific event (i.e., classes implementing the `IConsumer` interface).
3. **Execution:** The publisher loops through every listener it found, executing their logic one by one.
4. **Conclusion:** Because this happens synchronously in the same application, the service that published the event simply waits for the loop to finish. There is no need for complex network confirmations.