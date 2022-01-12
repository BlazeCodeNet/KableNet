# KableNet
 A C# Multiplayer VideoGame Networking Library

---
 ## Quick Start
 To start using KableNet, first import it as a reference in your project.
 The next steps depend on if you're doing the Server or Client(because they are handled seperately).

 **NOTICE:** Subscribe to events *BEFORE* starting the connection!

#### ServerSide
A KableNet server is rather simple to setup.
An example snippet of a KableNet server is below.
```cs
int port = 6886;
KableServer server = new KableServer( port );

// The NewConnectionEvent is raised when a new client connects over TCP
server.NewConnectionEvent += OnNewConnection;
// The NewConnectionErroredEvent is called when a new client connection has a error/exception
server.NewConnectionErroredEvent += OnNewConnectionError;

// Starts the TCP server to accept new clients
server.StartListening();
```

The ``NewConnectionEvent`` event will provide a KableConnection that represents the connection to the client. You may then subscribe to the clients ``PacketReadyEvent`` to recieve ``KablePacket``'s from the client.

#### ClientSide
ClientSide is equally simple.
An example snippet of a KableNet client is below.
```cs
string address = "127.0.0.1";
int port = 6886;
KableConnection connection = new KableConnection( address, port );

// PacketReadyEvent is raised when a new packet has been read and is ready to be processed.
// It will be in the form of a KablePacket which will be discussed later on.
connection.PacketReadyEvent += OnPacketReady;
// ConnectedEvent is raised when the connection has been established.
// If there is an error, ConnectErroredEvent will instead of raised.
connection.ConnectedEvent += OnConnected;
connection.ConnectErroredEvent += OnConnectionFailed;

// ConnectionErroredEvent is called when there is a general error with the connection after a connection has been previously established
connection.ConnectionErroredEvent += OnConnectionError;

// Starts the connection
connection.Connect();
```

#### KablePacket
A ``KablePacket`` is the class that is used to wrap data being sent to and from ``KableConnection`` classes. An example of its usage to send a TCP message through a ``KableConnection`` is given below.
```cs
KablePacket packet = new KablePacket();
packet.Write("Hello World!");
// connection is a instance of KableConnection
await connection.SendPacketTCPAsync(packet);
```
And to read, you'd do the following inside of your method that handles ``PacketReadyEvent``
```cs
private void OnPacketReady(KablePacket packet)
{
    string message = packet.ReadString();
    Console.WriteLine( $"Recieved Message '{ message }'" );
}
```
When done properly, this will send the string ``"Hello World"`` through the KableConnection. You can Write and Read many different data types.

---

### Packet Processing
In order for the ``PacketReadyEvent`` to be called, you must call ``ProcessBuffer`` routinely on all of your ``KableConnection`` instances. This will process the current network stream into ``KablePacket``'s on the current thread. 

An alternative is to call ``EnableBackgroundProcessing`` on your ``KableConnection`` instances. This will handle the processing on a background thread.

**WARNING:** Your project **MUST** support multi-threading to use ``EnableBackgroundProcessing``. This is especially true with the Unity3D Game Engine, where you should instead call ``ProcessBuffer`` inside of a game script in the ``Update`` method.

---
## Other Included Stuff
##### Identifier
An identifier is similar to MC's implementation of namespace:value style identifiaction of different objects.

##### Vec3f
Vec3f is a barebones class consisting of three floats. Literally meaning Vector-3-Float. This can be sent through included KablePacket methods allowing for slightly quicker implementations.

##### NetId
NetId is a barebones implimentation of identifying different objects between clients. It is literally just a random string generator that is wrapped in a fancy class. This is inherently slower than needed(Because of strings) so I plan to rework this class later on while mainting backwards compatability.