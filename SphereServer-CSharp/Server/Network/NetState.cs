using System.Net;
using System.Net.Sockets;
using SphereServer.Mobiles;

namespace SphereServer.Network;

/// <summary>
/// Represents a client connection to the server.
/// Based on Source-X CClient.
/// </summary>
public class NetState : IDisposable
{
    private readonly Socket _socket;
    private readonly byte[] _receiveBuffer;
    private readonly Queue<byte[]> _sendQueue;
    private bool _disposed;

    public NetState(Socket socket)
    {
        _socket = socket;
        _receiveBuffer = new byte[4096];
        _sendQueue = new Queue<byte[]>();
        _disposed = false;

        IPEndPoint? endpoint = _socket.RemoteEndPoint as IPEndPoint;
        Address = endpoint?.Address ?? IPAddress.None;
        ConnectedOn = DateTime.UtcNow;
    }

    public IPAddress Address { get; }
    public DateTime ConnectedOn { get; }
    public Mobile? Mobile { get; set; }
    public bool IsConnected => _socket.Connected && !_disposed;
    public ClientVersion Version { get; set; }

    public void BeginReceive()
    {
        if (!IsConnected)
            return;

        try
        {
            _socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, OnReceive, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting receive: {ex.Message}");
            Disconnect();
        }
    }

    private void OnReceive(IAsyncResult ar)
    {
        try
        {
            int bytesRead = _socket.EndReceive(ar);

            if (bytesRead <= 0)
            {
                Disconnect();
                return;
            }

            ProcessReceived(_receiveBuffer, bytesRead);
            BeginReceive();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving data: {ex.Message}");
            Disconnect();
        }
    }

    private void ProcessReceived(byte[] buffer, int length)
    {
        if (length == 0)
            return;

        byte packetId = buffer[0];
        PacketHandler.Handle(this, packetId, buffer, length);
    }

    public void Send(byte[] data)
    {
        if (!IsConnected || data == null || data.Length == 0)
            return;

        lock (_sendQueue)
        {
            _sendQueue.Enqueue(data);
        }

        ProcessSendQueue();
    }

    private void ProcessSendQueue()
    {
        if (!IsConnected)
            return;

        lock (_sendQueue)
        {
            while (_sendQueue.Count > 0)
            {
                byte[] data = _sendQueue.Dequeue();

                try
                {
                    _socket.Send(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending data: {ex.Message}");
                    Disconnect();
                    return;
                }
            }
        }
    }

    public void Disconnect()
    {
        if (_disposed)
            return;

        try
        {
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch { }

        try
        {
            _socket.Close();
        }
        catch { }

        _disposed = true;
        Console.WriteLine($"Client {Address} disconnected.");
    }

    public void Dispose()
    {
        Disconnect();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Client version information.
/// </summary>
public struct ClientVersion
{
    public byte Major { get; set; }
    public byte Minor { get; set; }
    public byte Revision { get; set; }
    public byte Patch { get; set; }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Revision}.{Patch}";
    }
}

/// <summary>
/// Packet handler registry.
/// </summary>
public static class PacketHandler
{
    private static readonly Dictionary<byte, Action<NetState, byte[], int>> _handlers = new();

    static PacketHandler()
    {
        RegisterHandlers();
    }

    private static void RegisterHandlers()
    {
        // Register packet handlers
        Register(0x80, HandleLoginRequest);          // Login Request
        Register(0x91, HandleGameServerLogin);       // Game Server Login
        Register(0x00, HandleCreateCharacter);       // Create Character (simplified)
        Register(0x02, HandleMovementRequest);       // Movement Request
        Register(0x06, HandleDoubleClick);           // Double Click
        Register(0x73, HandlePing);                  // Ping
    }

    public static void Register(byte packetId, Action<NetState, byte[], int> handler)
    {
        _handlers[packetId] = handler;
    }

    public static void Handle(NetState client, byte packetId, byte[] buffer, int length)
    {
        if (_handlers.TryGetValue(packetId, out var handler))
        {
            try
            {
                handler(client, buffer, length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling packet 0x{packetId:X2}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Unhandled packet: 0x{packetId:X2}");
        }
    }

    private static void HandleLoginRequest(NetState client, byte[] data, int length)
    {
        Console.WriteLine($"Login request from {client.Address}");
        // Send login response (simplified)
        // In real implementation, validate credentials and send appropriate response
    }

    private static void HandleGameServerLogin(NetState client, byte[] data, int length)
    {
        Console.WriteLine($"Game server login from {client.Address}");
        // Send character list
    }

    private static void HandleCreateCharacter(NetState client, byte[] data, int length)
    {
        Console.WriteLine($"Create character request from {client.Address}");
        // Create character and send response
    }

    private static void HandleMovementRequest(NetState client, byte[] data, int length)
    {
        if (client.Mobile == null)
            return;

        // Parse movement direction from packet
        // Update mobile position
        Console.WriteLine($"Movement request from {client.Address}");
    }

    private static void HandleDoubleClick(NetState client, byte[] data, int length)
    {
        Console.WriteLine($"Double click from {client.Address}");
        // Handle object interaction
    }

    private static void HandlePing(NetState client, byte[] data, int length)
    {
        // Send ping response
        client.Send(data); // Echo back
    }
}
