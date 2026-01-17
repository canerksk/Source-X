using SphereServer.Core;
using SphereServer.Mobiles;

namespace SphereServer;

class Program
{
    static void Main(string[] args)
    {
        Console.Title = "SphereServer C# - Ultima Online Emulator";

        Server server = new(port: 2593);

        // Handle Ctrl+C gracefully
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            server.Stop();
        };

        // Start server
        server.Start();

        // Create some test entities
        CreateTestWorld();

        // Print initial status
        server.PrintStatus();

        // Command loop
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  status  - Show server status");
        Console.WriteLine("  save    - Save world (not implemented)");
        Console.WriteLine("  quit    - Stop server and exit");
        Console.WriteLine();

        bool running = true;
        while (running)
        {
            Console.Write("> ");
            string? command = Console.ReadLine()?.Trim().ToLower();

            switch (command)
            {
                case "status":
                    server.PrintStatus();
                    break;

                case "save":
                    Console.WriteLine("World save is not implemented yet.");
                    break;

                case "quit":
                case "exit":
                    server.Stop();
                    running = false;
                    break;

                case "help":
                    Console.WriteLine("Commands: status, save, quit");
                    break;

                case "":
                    // Ignore empty input
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }

        Console.WriteLine("Goodbye!");
    }

    static void CreateTestWorld()
    {
        Console.WriteLine("Creating test world...");

        // Create a test player character at Britain bank
        Mobile testPlayer = World.Instance.CreateMobile(
            bodyId: 0x0190, // Human male body
            location: new Point3D(1438, 1695, 0, 0), // Britain bank location
            name: "Test Player"
        );

        testPlayer.Hits.Base = 100;
        testPlayer.Hits.Current = 100;
        testPlayer.Hits.Max = 100;

        testPlayer.Stam.Base = 100;
        testPlayer.Stam.Current = 100;
        testPlayer.Stam.Max = 100;

        testPlayer.Mana.Base = 100;
        testPlayer.Mana.Current = 100;
        testPlayer.Mana.Max = 100;

        // Create some test items nearby
        World.Instance.CreateItem(
            itemId: 0x0EED, // Gold pile
            location: new Point3D(1440, 1695, 0, 0),
            amount: 1000
        );

        World.Instance.CreateItem(
            itemId: 0x0F3F, // Dagger
            location: new Point3D(1441, 1695, 0, 0)
        );

        Console.WriteLine($"Created {World.Instance.MobileCount} mobiles and {World.Instance.ItemCount} items");
        Console.WriteLine();
    }
}
