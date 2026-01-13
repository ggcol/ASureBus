# ASureBus

ASureBus is a lightweight .NET messaging framework built on top of Azure Service Bus (ASB). It provides a set of
abstractions and helper classes for sending and receiving commands, events and timeouts, handling messages in plain
classes or sagas, caching ASB resources, off‑loading heavy properties to Azure Blob Storage, and persisting long‑running
workflows.

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/e75f90253491454cbf0dfb25c9c7085b)](https://app.codacy.com/gh/ggcol/ASureBus/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)[![Codacy Badge](https://app.codacy.com/project/badge/Coverage/e75f90253491454cbf0dfb25c9c7085b)](https://app.codacy.com/gh/ggcol/ASureBus/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_coverage)

[![NuGet version (ASureBus)](https://img.shields.io/nuget/v/ASureBus.svg?style=flat-square)](https://www.nuget.org/packages/ASureBus/)
ASureBus
[![NuGet version (ASureBus.Abstractions)](https://img.shields.io/nuget/v/ASureBus.Abstractions.svg?style=flat-square)](https://www.nuget.org/packages/ASureBus.Abstractions/)
ASureBus.Abstractions

## 1. Getting started

ASureBus integrates into an `ASP.NET Core` or generic host via extension methods. The minimal setup registers the
service bus client, message processors and supporting services:

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<TConfig>()                          // registers Azure Service Bus and message processing
    .RunConsoleAsync();
```

Only a connection string is required. You can supply the service bus settings via an options class that implements
`IConfigureAzureServiceBus` or by passing a configuration object directly. The most important property is the connection
string; other properties (transport type, retry policy, delays, max concurrency) have sensible defaults.

You can also configure additional features such as caching, heavy property off‑loading and saga persistence. Each has a
fluent extension method and a corresponding options class:

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb(new ServiceBusConfig
    {
        ConnectionString = "<connection-string>",
        TransportType = "AmqpWebSocket", // optional
        MaxRetries = 3                   // optional, defaults to 3
    })
    .ConfigureAsbCache(new AsbCacheConfig
    {
        Expiration = TimeSpan.FromMinutes(5),
        TopicConfigPrefix = "topicConfig",
        ServiceBusSenderCachePrefix = "sender"
    })
    .UseHeavyProps(new HeavyPropertiesConfig
    {
        ConnectionString = "<storage-connection>",
        Container = "heavies"
    })
    .UseSqlServerSagaPersistence(new SqlServerSagaPersistenceConfig
    {
        ConnectionString = "<sql-connection>"
    })
    .RunConsoleAsync();
```

These extension methods register the necessary services for caching senders, off‑loading heavy properties and persisting
sagas. See Configuration for detailed descriptions of each option.

### 1.1 NuGet packages

To use ASureBus in your project, install the following packages:

- **ASureBus** – the main runtime package providing messaging, sagas, heavy property support and persistence. It pulls
  in Azure.Messaging.ServiceBus, Azure.Storage.Blobs, Microsoft.Extensions.Hosting and Microsoft.Data.SqlClient as
  transitive dependencies.
- **ASureBus.Abstractions** – defines the contracts, marker interfaces and option classes. Reference this package in
  shared projects or other microservices that only need to define or consume messages; it has no external dependencies.

## 2. Messages and message handlers

ASureBus distinguishes between commands, events and timeouts. All messages implement the marker interface `IAmAMessage`.
Commands implement `IAmACommand`, events implement `IAmAnEvent`, and timeout requests implement `IAmATimeout`. A simple
message type might look like the following:

```csharp
public record CreateOrder : IAmACommand
{
    public Guid OrderId { get; init; }
    public decimal Total  { get; init; }
}

public record OrderCreated : IAmAnEvent
{
    public Guid OrderId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
```

### 2.1 Handlers

Handlers are plain classes that implement `IHandleMessage<TMessage>`. The interface defines a `Handle` method that
receives the message and an `IMessagingContext` for sending further messages or publishing events. There is an optional
`HandleError` method that can override default error handling. Below is a simple command handler:

```csharp
public class CreateOrderHandler : IHandleMessage<CreateOrder>
{
    public async Task Handle(CreateOrder message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        // perform work…
        // publish an event after the order is created
        await context.Publish(new OrderCreated
        {
            OrderId   = message.OrderId,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken).ConfigureAwait(false);
    }

    // optional error hook
    public Task HandleError(Exception ex, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        /*
        * log and swallow, 
        * or rethrow to let ASureBus dead‑letter the message (this is the default behavior if HandleError() is omitted)
        */
        return Task.CompletedTask;
    }
}
```

Handlers are automatically discovered at startup. ASureBus scans the entry assembly via TypesLoader and registers every
type that implements `IHandleMessage<T>`. When a command is sent it is delivered to a single handler (queue semantics),
whereas an event is published to a topic and delivered to every subscriber.

### 2.2 Sagas

A saga represents a long‑running workflow and tracks state across multiple messages. It derives from the abstract class
`Saga<TSagaData>` where TSagaData is a state class. The saga implements interfaces to specify which messages start it (
`IAmStartedBy<TInit>`), which messages it handles (`IHandleMessage<TMessage>`) and which timeouts it reacts to (
`IHandleTimeout<TTimeout>`). The base class exposes:

- SagaData – a strongly typed data object that is automatically persisted.
- CorrelationId – a Guid used to group messages belonging to the same instance.
- IAmComplete() – call this method when your saga has finished; it triggers the Completed event causing ASureBus to
  remove it from the cache and persistence store.
- RequestTimeout<TTimeout> – schedule a timeout message to be delivered after a delay or at an absolute time; the
  timeout message type must implement `IAmATimeout`.

Example saga:

```csharp
public class OrderSagaData : SagaData
{
    public Guid OrderId { get; set; }
    public bool PaymentReceived { get; set; }
}

public class OrderSaga : Saga<OrderSagaData>,
    IAmStartedBy<CreateOrder>,
    IHandleMessage<PaymentReceived>,
    IHandleTimeout<OrderTimeout>
{
    public async Task Handle(CreateOrder message, IMessagingContext context,
        CancellationToken ct)
    {
        // initialize state
        SagaData.OrderId = message.OrderId;
        // maybe send command to payment service
        await context.Send(new RequestPayment { OrderId = message.OrderId }, ct);
        // request timeout if payment isn’t received
        await RequestTimeout(new OrderTimeout(), TimeSpan.FromMinutes(30), context);
    }

    public Task Handle(PaymentReceived message, IMessagingContext context,
        CancellationToken ct)
    {
        SagaData.PaymentReceived = true;
        IAmComplete();            // mark saga as completed
        return Task.CompletedTask;
    }

    public async Task HandleTimeout(OrderTimeout timeout, IMessagingContext context,
        CancellationToken ct)
    {
        if (!SagaData.PaymentReceived)
        {
            // compensate or notify
            await context.Send(new CancelOrder { OrderId = SagaData.OrderId }, ct);
            IAmComplete();
        }
    }
}
```

Sagas are automatically discovered by TypesLoader just like handlers. Persistence is described in the Saga persistence
section.

### 2.3 Typed Messages and Routing

ASureBus supports generic (typed) messages, allowing you to define message contracts with type parameters. This enables
strong typing and flexible message handling. For example:

```csharp
public record AGenericMessage<T>(T Data) : IAmACommand;

public class AMessageStringFlavourHandler : IHandleMessage<AGenericMessage<string>>
{
    public Task Handle(AGenericMessage<string> genericMessage, IMessagingContext context, CancellationToken cancellationToken)
    {
        // Handle string-typed message
        return Task.CompletedTask;
    }
}

public class AMessageIntFlavourHandler : IHandleMessage<AGenericMessage<int>>
{
    public Task Handle(AGenericMessage<int> genericMessage, IMessagingContext context, CancellationToken cancellationToken)
    {
        // Handle int-typed message
        return Task.CompletedTask;
    }
}
```

When you send a message such as `AGenericMessage<string>` or `AGenericMessage<int>`, ASureBus will automatically route
it to the handler that matches the exact type argument. If you do not configure custom routing, the framework ensures
that each message is delivered to the handler whose generic type matches the message type precisely. This allows for
clear separation of logic and type-safe message processing.

See `Playground/Samples/12-GenericTypeMessages` for a practical example.

## 3. Configuration

ASureBus exposes several option classes. You can implement the corresponding `IConfigure…` interface and bind it from
configuration providers, or instantiate the class explicitly. All optional properties have fallback values from Defaults
to minimize boilerplate.

### 3.1 Service Bus (basic)

The following table summarizes the properties of `ServiceBusConfig` (which implements `IConfigureAzureServiceBus`) and
their default values:

| Property            | Description                                        | Default       |
|---------------------|----------------------------------------------------|---------------|
| ConnectionString    | Required. The Azure Service Bus connection string. | —             |
| TransportType       | Transport protocol: AmqpTcp or AmqpWebSockets.     | AmqpWebSocket |
| MaxRetries          | Number of message receive retries.                 | 3             |
| DelayInSeconds      | Delay between retry attempts (seconds).            | 0.8           |
| MaxDelayInSeconds   | Maximum delay between retries.                     | 60            |
| TryTimeoutInSeconds | Timeout for each try (seconds).                    | 300           |
| ServiceBusRetryMode | fixed or exponential.                              | fixed         |

These values configure the ServiceBusClientOptions and the concurrency of the message processors. They are used
internally by the AzureServiceBusService when it creates queue and topic processors.

### 3.2 Caching (AsbCache)

ASureBus caches ServiceBusSender instances and topic configurations in an in‑memory cache to avoid repeated network
calls. The cache can be tuned via `AsbCacheConfig` (implements `IConfigureAsbCache`):

- Expiration – optional TimeSpan specifying how long entries stay in the cache (default 5 minutes).
- TopicConfigPrefix – prefix used for cache keys storing topic/subscription names.
- ServiceBusSenderCachePrefix – prefix used for cache keys storing ServiceBusSender instances.

Call `.ConfigureAsbCache<T>()` to use settings bound from configuration, or pass an `AsbCacheConfig` object directly.
Internally, AsbCache exposes methods to set, upsert and remove entries and is used by AzureServiceBusService to reuse
senders and topic subscriptions.

### 3.3 Heavy properties

When messages contain large payloads (for example, file contents or binary data), sending them over Service Bus can
exceed the size limits (this strictly depends on selected Azure Service Bus tier). ASureBus introduces the `Heavy<T>`
wrapper to transparently off‑load such properties to Azure Blob Storage. To enable this feature, call `.UseHeavyProps()`
and provide `HeavyPropertiesConfig` (implements `IConfigureHeavyProperties`):

- ConnectionString – connection string to the storage account.
- Container – name of the blob container for storing heavies.

A property of type `Heavy<byte[]>` or `Heavy<string>` is off‑loaded when a message is sent; the Service Bus Message
header contains a pointer to the blob, and the actual payload is deleted from the message body. On the receiving side,
heavy properties are automatically loaded back from storage. Here is an example message with a heavy property:

```csharp
public record UploadDocument : IAmACommand
{
    public Heavy<byte[]> Content { get; init; } = new();
    public string FileName { get; init; } = string.Empty;
}

// usage
await context.Send(new UploadDocument
{
    Content  = new Heavy<byte[]>(File.ReadAllBytes(path)),
    FileName = Path.GetFileName(path)
});
```

### 3.4 Saga persistence

ASureBus stores saga state in an in‑memory cache by default. **This is sufficient for testing but unsuitable for
production because data is lost on restart**. You can persist sagas in either SQL Server or Azure Blob Storage.
Configuration classes implement `IConfigureSqlServerSagaPersistence` and `IConfigureDataStorageSagaPersistence`
respectively. Provide the connection string (and container for data storage) to persist saga snapshots. Use one of the
following extension methods:

- `.UseSqlServerSagaPersistence()` – persists saga data into a table named `<SagaType>` under a default schema (
  configurable). ASureBus creates the table if it doesn’t exist and uses JSON to serialize the saga data. You only need
  to supply the connection string.
- `.UseDataStorageSagaPersistence()` – uses Azure Blob Storage to store saga state. A blob is created for each saga
  instance; supply a connection string and container name.

Both providers work together with the in‑memory cache (AsbCache) to speed up saga retrieval. When a saga completes,
ASureBus deletes its persisted record.

### 3.5 Message lock auto‑renewal

Azure Service Bus locks a message for a short period (typically 30 seconds) when it is received. If your handler or saga
takes longer than this to process the message, the lock can expire and the message becomes visible to other consumers,
potentially leading to duplicate processing. ASureBus can automatically renew the message lock just before it expires.
The auto‑renewal feature is disabled by default to avoid unnecessary network calls. To enable it call the
`.ConfigureMessageLockHandling()` extension method. You can also set the
`MessageLockRenewalPreemptiveThresholdInSeconds` to control how many seconds before the lock expiry the renewal should
occur. The default threshold is 10 seconds.

You can also set the `MaxAutoLockRenewalDuration` property to control the maximum total time (in seconds) that ASureBus
will attempt to auto-renew the lock for a message. The default is 5 minutes (which is also Azure Service Bus default).
If message processing exceeds this duration, the lock will not be renewed further and the message may become visible to
other consumers.

```csharp
await Host.CreateDefaultBuilder()
    .UseAsb()
    .ConfigureMessageLockHandling(opt =>
    {
        opt.EnableMessageLockAutoRenewal = true;
        opt.MessageLockRenewalPreemptiveThresholdInSeconds = 5;
        opt.MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10);
    })
    .RunConsoleAsync();
```

When auto‑renewal is enabled, ASureBus attaches a lock observer to each incoming message. This observer starts a timer
based on the remaining lock duration minus the preemptive threshold. When the timer fires, it calls
`RenewMessageLockAsync` on the message to extend the lock and then removes itself.
Adjusting the threshold lets you control how aggressively the renewal happens: a smaller threshold renews the lock
earlier but may result in more renewals on short‑running handlers, while a larger threshold risks missing the renewal
window if message processing slows down.

## 4. Messaging API

The `IMessagingContext` interface is injected into handlers, sagas and any other class via DI. It exposes methods to
send commands and publish events, with optional delays and scheduling. These methods immediately send messages to the
service bus (or the appropriate topic). When used inside sagas and handlers, the context also maintains a correlation ID
so that all messages sent within the same handler share the same correlation context.

### 4.1 Sending commands and publishing events

- `Task Send<TCommand>(TCommand message, CancellationToken ct = default)` – sends a command to its queue.
- `Task Publish<TEvent>(TEvent message, CancellationToken ct = default)` – publishes an event to a topic.

Within handlers or sagas you can simply call `context.Send()` or `context.Publish()` to emit new messages.

### 4.2 Delayed and scheduled messages

ASureBus allows you to delay or schedule messages either via dedicated methods or via options objects:

- Delayed messages – call `SendAfter(message, TimeSpan delay)` or `PublishAfter(message, TimeSpan delay)` to enqueue the
  message after a delay. Alternatively, set the `Delay` property on `SendOptions` or `PublishOptions`.
- Scheduled messages – call `SendScheduled(message, DateTimeOffset scheduledTime)` or
  `PublishScheduled(message, DateTimeOffset scheduledTime)` to deliver the message at a specific time. Alternatively,
  set the `ScheduledTime` property on the options object.

Example:

```csharp
// Delay a command by 20 seconds
await context.SendAfter(new ExpireCache(), TimeSpan.FromSeconds(20), ct);

// Equivalent using SendOptions
await context.Send(new ExpireCache(), new SendOptions
{
    Delay = TimeSpan.FromSeconds(20)
}, ct);

// Schedule a message for New Year’s Day
await context.SendScheduled(new WishHappyNewYear(), new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), ct);
```

### 4.3 Send/publish options

The `SendOptions` and `PublishOptions` classes derive from a common `EmitOptions` base. They allow you to override the
destination queue or topic via the `Destination` property, or specify `Delay` and `ScheduledTime` manually. When both
`Delay` and `ScheduledTime` are set, the scheduled time takes precedence. These options are optional and only required
when customizing behaviour beyond the defaults.

### 4.4 Correlation binding (Bind)

The `IMessagingContext` exposes a `Bind(Guid correlationId)` method that sets the context’s `CorrelationId` and returns
the same context. When messages are enqueued, this correlation identifier is written into the message header so that
downstream consumers can associate subsequent messages with the same workflow. Use this method when you need to
correlate messages outside of handlers or sagas—e.g. when a background job sends a follow‑up command to an existing
saga. Inside handlers and sagas the correlation id is set automatically by the framework; **manually calling `Bind()`
within those contexts overrides the existing correlation and can break saga correlation.**

Example:

```csharp
// correlate a message with an existing saga from a background job
var correlationId = existingSagaId;
await messagingContext.Bind(correlationId).Send(new CheckStatus());
```

## 5. Heavy property usage

The `Heavy<T>` wrapper is central to how ASureBus handles large payloads. When a message is serialized, ASureBus detects
`Heavy<T>` properties and uses `HeavyIo.Unload` to off‑load them into Azure Blob Storage. The message header stores a
reference to the blob so the heavy payload isn’t transmitted over Service Bus. On the receiving side, `HeavyIo.Load`
automatically loads the payload back into the property.

Because heavies rely on Azure Storage, you must enable them via `.UseHeavyProps()` and provide connection and container
settings. Only properties of type `Heavy<T>` are off‑loaded; other properties remain in the message body. You can wrap
any serializable type, including strings, byte arrays or custom objects.

### 5.1. Common constraints & considerations

In an event-driven architecture, messages should ideally be small and focused on conveying intent rather than large data
blobs. However, when large payloads are unavoidable, using `Heavy<T>` provides a practical solution. Here are some
considerations:

#### 5.1.1. Performance:

off‑loading and loading heavies involves additional network calls to Azure Blob Storage, which can introduce latency.
Use heavies judiciously for truly large payloads. Moreover, serialization and deserialization of heavies can add CPU
overhead. Monitor performance and optimize as needed.

#### 5.1.2. Storage costs:

storing large payloads in Azure Blob Storage incurs costs based on storage size and access patterns. Monitor your usage
to avoid unexpected charges. Blobs are not automatically deleted; implement a cleanup strategy if needed.

**Why blobs are not deleted automatically:** When a message containing a heavy property is sent, ASureBus off‑loads the
payload to Azure Blob Storage. However, the blob is not deleted automatically after the message is processed because
multiple consumers might need to access the heavy property.
Automatic deletion could lead to data loss if other consumers attempt to access the blob after it has been removed.
Instead, consider implementing a cleanup strategy based on your application's requirements, such as deleting blobs after
a certain retention period or when they are no longer needed.
Also, offloaded payload may be involved in auditing processes or compliance requirements, necessitating retention beyond
immediate processing.

## 6. Saga persistence

By default, sagas are cached in memory. To persist them, configure either SQL Server or Azure Storage as described in
Section 3.4. Internally, `SagaFactory` attempts to load saga data from the cache first; if it isn’t present it will call
the configured persistence service to load the saga from storage. The saga is then stored in both cache and persistence
storage for subsequent calls. When the saga calls `IAmComplete()` the data is removed from both the cache and the
persistence provider.

- SQL Server persistence – `SqlServerService` creates a table on first use and inserts or updates rows using the saga’s
  correlation ID as the primary key. The JSON structure includes both the saga data and its correlation ID, so the saga
  can be reconstructed via `SagaConverter`.
- Azure Storage persistence – `AzureDataStorageService` stores saga data as blobs. Each blob is named using the saga
  type and correlation ID; it is deleted when the saga completes.

Using persistence ensures that sagas survive application restarts and that long‑running workflows are durable. For unit
tests or simple demos you can omit these calls and rely on the in‑memory cache.

## 7. Error handling and dead‑lettering

ASureBus provides sensible defaults for error handling but also allows developers to influence behaviour:

- Retry and dead‑letter policy – when a handler or saga throws an exception, the message is retried until the maximum
  delivery count (`MaxRetries`) is reached. After that the message is dead‑lettered. You can adjust this behaviour via
  `ServiceBusConfig.MaxRetries`.
- Fail‑fast exceptions – throw a `FailFastException` (implements `IFailFast`) to immediately dead‑letter the message,
  bypassing retries. This is useful when the error is permanent (e.g. invalid input).
- HandleError – override `HandleError` in your handler or saga to perform custom logic on errors. By default, the
  exception is re‑thrown so that ASureBus performs retries and dead‑letters.
- Dead‑letter content – when messages are dead‑lettered, ASureBus preserves the original message and adds exception
  details.

## 8. Internal architecture (optional reading)

While most users do not need to know the internal workings, understanding the components can aid debugging and
extension:

- TypesLoader – scans the entry assembly to find all handlers and sagas and builds metadata used to route messages. It
  distinguishes commands versus events and records which messages start or continue sagas.
- Message processors – there are two concrete processors derived from `MessageProcessor`: `HandlerMessagesProcessor`
  processes plain handlers and `SagaMessagesProcessor` processes sagas. They deserialize messages, load heavy properties
  via `HeavyIo`, invoke handlers via `BrokerFactory` and `SagaBroker`, and handle completion and errors.
- Message emitter – `MessageEmitter` flushes pending messages collected in a context. It uses `ServiceBusSender` to send
  to queues for commands or to topics for events, scheduling messages when required.
- Caching – `AsbCache` stores `ServiceBusSender` instances and topic subscription names with a configurable expiration.
  This reduces the overhead of repeatedly creating senders.
- Configuration – global options are stored in `AsbConfiguration` and updated by the extension methods. Flags such as
  `UseHeavyProperties`, `OffloadSagas` and others determine which services are enabled.
- Serialization – messages and saga data are serialized using System.Text.Json via the static `Serializer` class. *
  *There is no pluggable serializer at the moment.**

## 9. Summary

ASureBus brings structure and convenience to Azure Service Bus–based messaging by providing:

- Fluent configuration for Service Bus, caching, heavy properties and saga persistence.
- Simple message and handler abstractions for commands, events and timeouts.
- Built‑in support for sagas with durable persistence and timeout scheduling.
- Transparent heavy property off‑loading to Azure Blob Storage.
- Automatic discovery of handlers and sagas and concurrency control.
- Sensible defaults with extensive configuration options.

By following the patterns described above, developers can build robust, event‑driven systems that leverage Azure Service
Bus with minimal boilerplate while still having fine‑grained control over transport settings, caching and persistence.

## 10. Strengths, weaknesses and common constraints

### Strengths

- Minimal setup – a single `UseAsb()` call registers the service bus, message processors and all required services.
- Lightweight abstractions – commands, events, timeouts and sagas are simple classes or records; handler and saga
  discovery is automatic.
- Heavy property off‑loading allowing large payloads to be stored in Azure Blob Storage.
- Durable sagas – built‑in persistence for sagas via SQL Server or Azure Storage.

### Weaknesses

- Single transport – ASureBus only supports Azure Service Bus. Other frameworks (e.g., NServiceBus, MassTransit, ReBus)
  support multiple brokers, making them more flexible. <br> This is intended so far => *Not actively worked.*
- Reflection‑based wiring – handler and saga discovery and saga handlers invocation rely on reflection, deferring
  certain errors to runtime and adding a small overhead. <br> Keep in mind for huge solutions/projects. => *Not actively
  worked.*
- Limited customization – there are few hooks for custom serialization, outbox patterns, advanced error handling or
  per‑queue policies => *Actively worked.*

### Common constraints and worries

- Message size limits – Azure Service Bus imposes a limit on message size; use `Heavy<T>` to off‑load large payloads.
  Failing to enable heavy property support will result in exceptions when sending large messages.
- In‑memory cache persistence – without configuring saga persistence the state lives only in memory; restarting the
  service will lose all saga data.
- Correlation misuse – calling `Bind()` within a saga or handler will override the correlation id, decoupling messages
  from the current saga and potentially starting new instances. Reserve `Bind()` for external contexts.

## 11. Playground Samples

The Playground project provides a rich set of samples demonstrating usage of ASureBus features. Each sample is
self-contained and focuses on a specific pattern or capability. You can find these samples in the `/Playground/Samples/`
directory:

- [01-OneCommand](Playground/Samples/01-OneCommand): Basic command sending and handling
- [02-OneEvent](Playground/Samples/02-OneEvent): Event publishing and subscription
- [03-TwoMessagesSameHandlerClass](Playground/Samples/03-TwoMessagesSameHandlerClass): Handling multiple message types
  in a single handler
- [04-ASaga](Playground/Samples/04-ASaga): Basic saga workflow
- [05-Heavy](Playground/Samples/05-Heavy): Heavy property off-loading to Azure Blob Storage
- [06-SagaPersistence](Playground/Samples/06-SagaPersistence): Saga persistence with SQL Server/Azure Storage
- [07-DelayedAndScheduled](Playground/Samples/07-DelayedAndScheduled): Delayed and scheduled message delivery
- [08-ABrokenSaga](Playground/Samples/08-ABrokenSaga): Error handling in sagas
- [09-LongerSaga](Playground/Samples/09-LongerSaga): Extended saga workflow
- [10-SagaWithTimeout](Playground/Samples/10-SagaWithTimeout): Saga with timeout handling
- [11-SagaTimeoutTriggeredAfterCompleting](Playground/Samples/11-SagaTimeoutTriggeredAfterCompleting): Timeout after
  saga completion
- [12-GenericTypeMessages](Playground/Samples/12-GenericTypeMessages): Typed/generic messages and routing

Refer to these samples for practical code examples and inspiration. Throughout this documentation, you will find direct
links to relevant samples for each feature:

- **Getting Started**: See [01-OneCommand](Playground/Samples/01-OneCommand)
- **Events**: See [02-OneEvent](Playground/Samples/02-OneEvent)
- **Multiple Message Types**: See [03-TwoMessagesSameHandlerClass](Playground/Samples/03-TwoMessagesSameHandlerClass)
- **Sagas**:
  See [04-ASaga](Playground/Samples/04-ASaga), [09-LongerSaga](Playground/Samples/09-LongerSaga), [10-SagaWithTimeout](Playground/Samples/10-SagaWithTimeout)
- **Heavy Properties**: See [05-Heavy](Playground/Samples/05-Heavy)
- **Saga Persistence**: See [06-SagaPersistence](Playground/Samples/06-SagaPersistence)
- **Delayed/Scheduled Messages**: See [07-DelayedAndScheduled](Playground/Samples/07-DelayedAndScheduled)
- **Typed Messages**: See [12-GenericTypeMessages](Playground/Samples/12-GenericTypeMessages)
- **Error Handling**:
  See [08-ABrokenSaga](Playground/Samples/08-ABrokenSaga), [11-SagaTimeoutTriggeredAfterCompleting](Playground/Samples/11-SagaTimeoutTriggeredAfterCompleting)

For more details, browse the code in each sample folder.

## 12. License

ASureBus is licensed under the [LGPL-3.0-or-later](LICENSE) license.

This means:

- ✅ You can freely use this library in proprietary or open-source projects.
- ✅ You can distribute software that uses this library under any license.
- ✅ If you modify ASureBus itself and distribute it, you must publish those changes under LGPL.
