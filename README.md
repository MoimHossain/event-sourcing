# SuperNova Storage

A lightweight **CQRS** supporting library with **Event Store** based on __Azure Table Storage__.

## Quick start guide

### Install

Install the [**SuperNova.Storage** Nuget](https://www.nuget.org/packages/SuperNova.Storage/) package into the project.

```
Install-Package SuperNova.Storage -Version 1.0.0
```

The dependencies of the package are:

 - .NETCoreApp 2.0
 - Microsoft.Azure.DocumentDB.Core (>= 1.7.1)
 - Microsoft.Extensions.Logging.Debug (>= 2.0.0)
 - SuperNova.Shared (>= 1.0.0)
 - WindowsAzure.Storage (>= 8.5.0)


# Implemention guide

## Write Side - Event Sourcing 

Once the package is installed, we can start sourcing events in an application. For example, let's start with a canonical example of ``` UserController ``` in a **Web API** project.

We can use the dependency injection to make **EventStore** available in our controller. 

Here's an example where we register an instance of Event Store with DI framework in our _Startup.cs_

```
// Config object encapsulates the table storage connection string
services.AddSingleton<IEventStore>(new EventStore( ... provide config )); 
```
Now the controller:

```
[Produces("application/json")]
[Route("users")]
public class UsersController : Controller
{   
    public UsersController(IEventStore eventStore)
    {
        this.eventStore = eventStore; // Here capture the event store handle
    }   
    
    ... other methods skipped here
}
```

#### Aggregate
Implementing event sourcing becomes way much handier, when it's fostered with  **Domain Driven Design** (aka DDD). We are going to assume that we are familiar with DDD concepts (especially **Aggregate Roots**).

An aggregate is our consistency boundary (read as __transactional boundary__) in Event Sourcing. (Technically, Aggregate ID's are our **partition keys** on Event Store table - therefore, we can only apply an **atomic** operation on a single aggregate root level.)

Let's create an Aggregate for our User domain entity:

```
using SuperNova.Shared.Messaging.Events.Users;
using SuperNova.Shared.Supports;

public class UserAggregate : AggregateRoot
{
    private string _userName;
    private string _emailAddress;
    private Guid _userId;
    private bool _blocked;

```

Once we have the aggregate class written, we should come up with the events that are relevant to this aggregate. We can use **Event storming** to come up with the relevant events.

Here are the events that we will use for our example scenario:

```
public class UserAggregate : AggregateRoot
{

    ... skipped other codes

    #region Apply events
    private void Apply(UserRegistered e)
    {
        this._userId = e.AggregateId;
        this._userName = e.UserName;
        this._emailAddress = e.Email;            
    }

    private void Apply(UserBlocked e)
    {
        this._blocked = true;
    }

    private void Apply(UserNameChanged e)
    {
        this._userName = e.NewName;
    }
    #endregion

    ... skipped other codes
}

```

Now that we have our business events defined, we will define our **commands** for the aggregate:

```
public class UserAggregate : AggregateRoot
{
    #region Accept commands
    public void RegisterNew(string userName, string emailAddress)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(userName, nameof(userName));
        Ensure.ArgumentNotNullOrWhiteSpace(emailAddress, nameof(emailAddress));

        ApplyChange(new UserRegistered
        {
            AggregateId = Guid.NewGuid(),
            Email = emailAddress,
            UserName = userName                
        });
    }

    public void BlockUser(Guid userId)
    {            
        ApplyChange(new UserBlocked
        {
            AggregateId = userId
        });
    }

    public void RenameUser(Guid userId, string name)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(name, nameof(name));

        ApplyChange(new UserNameChanged
        {
            AggregateId = userId,
            NewName = name
        });
    }
    #endregion


    ... skipped other codes
}
```


So far so good!

Now we will modify the web api controller to send the correct command to the aggregate.

```
public class UserPayload 
{  
    public string UserName { get; set; } 
    public string Email { get; set; } 
}

// POST: User
[HttpPost]
public async Task<JsonResult> Post(Guid projectId, [FromBody]UserPayload user)
{
    Ensure.ArgumentNotNull(user, nameof(user));

    var userId = Guid.NewGuid();    

    await eventStore.ExecuteNewAsync(
        Tenant, "user_event_stream", userId, async () => {

        var aggregate = new UserAggregate();

        aggregate.RegisterNew(user.UserName, user.Email);

        return await Task.FromResult(aggregate);
    });

    return new JsonResult(new { id = userId });
}

```

And another API to modify existing users into the system:
```
//PUT: User
[HttpPut("{userId}")]
public async Task<JsonResult> Put(Guid projectId, Guid userId, [FromBody]string name)
{
    Ensure.ArgumentNotNullOrWhiteSpace(name, nameof(name));

    await eventStore.ExecuteEditAsync<UserAggregate>(
        Tenant, "user_event_stream", userId,
        async (aggregate) =>
        {
            aggregate.RenameUser(userId, name);

            await Task.CompletedTask;
        }).ConfigureAwait(false);

    return new JsonResult(new { id = userId });
}
```

That's it! We have our **WRITE** side completed. The event store is now contains the events for user event stream. 

![EventStore](https://i.imgur.com/cU3HT4l.png "EventStore with events")


## Read Side - Materialized Views

We can consume the events in a seperate console worker process and generate the _materialized_ views for **READ** side.

The readers (the console application - **Azure Web Worker** for instance) are like feed processor and have their own **lease** collection that makes them fault tolerant and resilient. If crashes, it catches up form the last event version that was materialized successfully. It's doing a polling - instead of a message broker (**Service Bus** for instance) on purpose, to speed up and avoid latencies during event propagation. Scalabilities are ensured by means of dedicating lease per tenants and event streams - which provides pretty high scalability.

### How to listen for events?

In a **worker application** (typically a console application) we will listen for events:

```
private static async Task Run()
{
    var eventConsumer = new EventStreamConsumer(        
        ... skipped for simplicity
        "user-event-stream", 
        "user-event-stream-lease");
    
    await eventConsumer.RunAndBlock((evts) =>
    {
        foreach (var @evt in evts)
        {
            if (evt is UserRegistered userAddedEvent)
            {
                readModel.AddUserAsync(new UserDto
                {
                    UserId = userAddedEvent.AggregateId,
                    Name = userAddedEvent.UserName,
                    Email = userAddedEvent.Email
                }, evt.Version);
            }

            else if (evt is UserNameChanged userChangedEvent)
            {
                readModel.UpdateUserAsync(new UserDto
                {
                    UserId = userChangedEvent.AggregateId,
                    Name = userChangedEvent.NewName
                }, evt.Version);
            }
        }

    }, CancellationToken.None);
}

static void Main(string[] args)
{
    Run().Wait();
}
```


Now we have a document collection (we are using Cosmos Document DB in this example for materialization but it could be any database essentially) that is being updated as we store events in event stream.

## Conclusion

The library is very light weight and havily influenced by Greg's event store model and aggreagate model. Feel free to use/contribute.

Thank you!
