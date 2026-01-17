using System.Net;
using System.Net.Sockets;
using SphereServer.Network;

namespace SphereServer.Core;

/// <summary>
/// Main server class that handles client connections and game loop.
/// </summary>
public class Server
{
    private readonly int _port;
    private Socket? _listener;
    private readonly List<NetState> _clients;
    private bool _running;
    private Thread? _gameLoopThread;

    public Server(int port = 2593)
    {
        _port = port;
        _clients = new List<NetState>();
        _running = false;
    }

    public bool IsRunning => _running;
    public int ClientCount => _clients.Count;

    public void Start()
    {
        if (_running)
        {
            Console.WriteLine("Server is already running!");
            return;
        }

        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║   SphereServer C# - Ultima Online       ║");
        Console.WriteLine("║   Ported from Source-X                   ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(IPAddress.Any, _port));
            _listener.Listen(10);

            _running = true;

            Console.WriteLine($"Server listening on port {_port}");
            Console.WriteLine("Waiting for connections...");
            Console.WriteLine();

            // Start game loop thread
            _gameLoopThread = new Thread(GameLoop)
            {
                IsBackground = true,
                Name = "GameLoop"
            };
            _gameLoopThread.Start();

            // Start accepting connections
            BeginAcceptConnection();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting server: {ex.Message}");
            _running = false;
        }
    }

    private void BeginAcceptConnection()
    {
        if (_listener == null || !_running)
            return;

        try
        {
            _listener.BeginAccept(OnAcceptConnection, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accepting connection: {ex.Message}");
        }
    }

    private void OnAcceptConnection(IAsyncResult ar)
    {
        if (_listener == null || !_running)
            return;

        try
        {
            Socket socket = _listener.EndAccept(ar);
            NetState client = new(socket);

            lock (_clients)
            {
                _clients.Add(client);
            }

            Console.WriteLine($"New connection from {client.Address} (Total clients: {_clients.Count})");

            client.BeginReceive();
            BeginAcceptConnection();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling new connection: {ex.Message}");
            BeginAcceptConnection();
        }
    }

    private void GameLoop()
    {
        DateTime lastTick = DateTime.UtcNow;
        const int TickInterval = 100; // 100ms = 10 ticks per second

        while (_running)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan elapsed = now - lastTick;

            if (elapsed.TotalMilliseconds >= TickInterval)
            {
                lastTick = now;
                Tick();
            }

            Thread.Sleep(10); // Sleep to avoid 100% CPU usage
        }
    }

    private void Tick()
    {
        try
        {
            // Update world
            World.Instance.Tick();

            // Remove disconnected clients
            lock (_clients)
            {
                _clients.RemoveAll(c => !c.IsConnected);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in game loop: {ex.Message}");
        }
    }

    public void Stop()
    {
        if (!_running)
            return;

        Console.WriteLine("Shutting down server...");
        _running = false;

        // Disconnect all clients
        lock (_clients)
        {
            foreach (NetState client in _clients)
            {
                client.Disconnect();
            }
            _clients.Clear();
        }

        // Stop listener
        _listener?.Close();
        _listener = null;

        // Wait for game loop to finish
        _gameLoopThread?.Join(1000);

        Console.WriteLine("Server stopped.");
    }

    public void BroadcastMessage(string message)
    {
        Console.WriteLine($"Broadcast: {message}");

        lock (_clients)
        {
            foreach (NetState client in _clients.Where(c => c.IsConnected))
            {
                // Send message packet to client
                // Implementation depends on UO protocol
            }
        }
    }

    public void PrintStatus()
    {
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine($"Server Status:");
        Console.WriteLine($"  Running: {_running}");
        Console.WriteLine($"  Clients: {_clients.Count}");
        Console.WriteLine($"  Entities: {World.Instance.EntityCount}");
        Console.WriteLine($"    - Mobiles: {World.Instance.MobileCount}");
        Console.WriteLine($"    - Items: {World.Instance.ItemCount}");
        Console.WriteLine("═══════════════════════════════════════");
    }
}
