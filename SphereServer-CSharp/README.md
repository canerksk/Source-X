# SphereServer C# - Ultima Online Emulator

Modern .NET 9.0 port of [Source-X](https://github.com/Sphereserver/Source-X) Ultima Online server emulator.

## 🎯 Project Goals

This is an MVP (Minimum Viable Product) implementation with the following core features:

- ✅ Login to the game
- ✅ Character creation
- ✅ Spawn in the world and move around
- ✅ Basic entity system (items and mobiles)
- ✅ Scripting support for game data

## 🏗️ Architecture

```
SphereServer-CSharp/
├── Server/
│   ├── Core/
│   │   ├── Serial.cs       - Unique identifiers (ported from CUID)
│   │   ├── Point3D.cs      - 3D coordinates (ported from CPointMap)
│   │   ├── Entity.cs       - Base entity class (ported from CObjBase)
│   │   ├── World.cs        - World manager
│   │   └── Server.cs       - Main server class
│   ├── Items/
│   │   └── Item.cs         - Item class (ported from CItem)
│   ├── Mobiles/
│   │   └── Mobile.cs       - Character/NPC class (ported from CChar)
│   ├── Scripting/
│   │   └── ScriptParser.cs - Script parser (ported from CScript)
│   ├── Network/
│   │   └── NetState.cs     - Client connection (ported from CClient)
│   └── Program.cs          - Entry point
└── README.md
```

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Any OS (Windows, Linux, macOS)

### Building

```bash
cd SphereServer-CSharp/Server
dotnet build
```

### Running

```bash
dotnet run
```

The server will start on port **2593** (default UO port).

### Commands

Once the server is running, you can use these console commands:

- `status` - Show server status (clients, entities, etc.)
- `save` - Save world (not yet implemented)
- `quit` - Stop server and exit

## 🎮 Connecting

Use any Ultima Online client (Classic or Enhanced) and configure it to connect to:
- **Host:** localhost (or server IP)
- **Port:** 2593

## 📋 Implementation Status

### ✅ Completed

- **Core System**
  - Serial (CUID) system for unique entity identifiers
  - 3D point system with distance calculations
  - Base entity class with properties and tags
  - World entity management

- **Items**
  - Basic item class with type, amount, weight
  - Item attributes (blessed, newbie, cursed, etc.)
  - Container support

- **Mobiles**
  - Character/NPC class
  - Stats system (Str/Hits, Int/Mana, Dex/Stam)
  - Skills system (58 skills)
  - Equipment system by layer
  - Combat flags and states

- **Scripting**
  - Script file parser (.scp format)
  - Block-based configuration reading
  - Property parsing (string, int, bool, hex)

- **Networking**
  - TCP socket server
  - Client connection management
  - Packet handler registry
  - Basic packet handlers (login, movement, etc.)

### 🚧 In Progress / Not Yet Implemented

- Full UO protocol implementation
- Encryption/compression support
- World persistence (saving/loading)
- Complete packet handlers
- Combat system
- Magic system
- AI for NPCs
- Map/multi support
- Item containers
- Guild/party systems

## 🔄 Ported from Source-X

This project is a C# port of the following Source-X components:

| Source-X (C++) | SphereServer-CSharp (C#) |
|----------------|--------------------------|
| CUID / dword   | Serial                   |
| CPointMap      | Point3D                  |
| CObjBase       | Entity                   |
| CItem          | Item                     |
| CChar          | Mobile                   |
| CScript        | ScriptParser             |
| CClient        | NetState                 |

## 🛠️ Development

### Code Style

- Modern C# with nullable reference types enabled
- Record types for immutable data structures
- LINQ for collections
- Async/await for I/O operations (where applicable)

### Adding Features

1. Core game entities go in `Core/`
2. Item-specific code in `Items/`
3. Character/NPC code in `Mobiles/`
4. Network packets in `Network/`
5. Script definitions in `Scripting/`

## 📝 License

This is a port of Source-X, which is licensed under Apache 2.0.

## 🙏 Credits

- Original **Source-X** team for the C++ implementation
- **SphereServer** community for decades of UO emulation knowledge
- **Ultima Online** by Origin Systems / Electronic Arts

## ⚠️ Disclaimer

This is an educational project. Ultima Online is a registered trademark of Electronic Arts Inc.

## 📞 Contact

This is a MVP demonstration project showing how to port Source-X to C#.
