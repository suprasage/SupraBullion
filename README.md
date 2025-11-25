## Overview
SuprabullionApp is a C# console application implementing a peer-to-peer blockchain with PayPal integration, user registration, block locking, and querying. It features an interactive command-line interface (CLI), peer networking with persistence, config file support, and manual UPnP port forwarding for NAT traversal. The app uses SHA256 hashing for security, stores data in JSON format, and supports RSA encryption for user data.

### Core Features
- **Blockchain**: Mining, transactions (buy/sell/transfer), validation, and integrity checks.
- **PayPal Integration**: Sandbox/live environments for payments.
- **User Registration**: RSA key generation with fallback; encrypted data storage.
- **Peer Networking**: TCP-based communication with peer persistence and UPnP port forwarding.
- **Block Management**: Locking/unlocking blocks, querying by ID, and file-based storage.
- **Interactive CLI**: Command loop with Ctrl+C exit.
- **Config File**: JSON-based settings for port forwarding, port, and peer ID.
- **File Storage**: Blocks in `./blocks/`, receipts in `./receipts/`, peers in `./peers/`.

### Dependencies
- .NET 10.0+
- Newtonsoft.Json (install via `dotnet add package Newtonsoft.Json`)
```bash 
dotnet add package Newtonsoft.Json
dotnet add package PayPalServerSDK
dotnet add package Open.Nat
```

## Setup
1. **Build the Project**: Run `dotnet build` in the project directory.
2. **Config File**: `config.json` is auto-created on first run. Edit it to enable features:
   ```json
   {
     "EnablePortForwarding": false,
     "Port": 8080,
     "PeerId": "Peer1"
   }
   ```
3. **Directories**: `./blocks/`, `./receipts/`, `./peers/` are auto-created.
4. **Run**: Use `dotnet run` for interactive mode or `dotnet run <command>` for single commands.
5. **Peers**: Add peers via CLI or edit `./peers/peers_1.json`.

## CLI Commands
Run commands in interactive mode (`dotnet run`) or as args. All commands are case-insensitive.

- **buy <buyer> <amount>**  
  Initiates PayPal payment from buyer to Reserve. Requires valid PayPal credentials.  
  Example: `buy Alice 50.00`  
  Output: PayPal URL, capture status, transaction confirmation.

- **sell <seller> <amount>**  
  Simulates selling to Reserve (no real payout).  
  Example: `sell Bob 30.00`  
  Output: Transaction confirmation.

- **transfer <sender> <receiver> <amount>**  
  Peer-to-peer transfer (no PayPal).  
  Example: `transfer Alice Bob 20.00`  
  Output: Transaction confirmation.

- **mine**  
  Mines pending transactions into a block.  
  Example: `mine`  
  Output: Mining progress, block hash.

- **validate**  
  Checks blockchain integrity.  
  Example: `validate`  
  Output: "Blockchain valid: True/False".

- **register <user> <ip> <peersHash> <privateKey> <publicKey>**  
  Registers user with RSA keys (auto-generates if invalid).  
  Example: `register Alice 192.168.1.1 abc123 "" ""` (generates keys).  
  Output: Registration confirmation, generated keys if applicable.

- **addlock <blockid>**  
  Locks a block.  
  Example: `addlock 1`  
  Output: Lock confirmation.

- **removelock <blockid>**  
  Unlocks a block.  
  Example: `removelock 1`  
  Output: Unlock confirmation.

- **query <blockid>**  
  Prints block data from `./blocks/`.  
  Example: `query 1`  
  Output: Full block JSON or "not found".

- **addpeer <ip:port>**  
  Adds a peer (persists to `./peers/`).  
  Example: `addpeer 127.0.0.1:8081`  
  Output: Peer added.

## Networking
- **Peer Communication**: TCP on configurable port (default 8080). Messages: "CHAIN:" (blockchain data), "RECEIPT:" (receipts).
- **Persistence**: Peers saved in `./peers/peers_*.json` (100 peers per file).
- **Port Forwarding**: Manual UPnP if enabled in config. Requires UPnP router.
- **Startup**: Loads peers and config on launch.

## Security Notes
- **RSA**: Keys auto-generated; private keys printed securely.
- **Encryption**: User data hashed (not encrypted for demo).
- **PayPal**: Use sandbox for testing; live requires compliance.
- **Blockchain**: SHA256 hashing; validate integrity regularly.
- **Networking**: Plain TCP; add SSL for production.

## Troubleshooting
- **Build Errors**: Ensure .NET 10.0 and dependencies.
- **PayPal Failures**: Check credentials in code.
- **Peer Issues**: Verify ports and UPnP.
- **Config**: Edit `config.json` and restart.
- **Runtime Errors**: Check console logs; share for debugging.

## Example Usage Session
1. Register a user: `dotnet run register Bob 192.168.1.1 abc123 "<privateKey>" "<publicKey>"`
2. Buy from Reserve: `dotnet run buy Bob 50.00`
3. Transfer: `dotnet run transfer Alice Bob 20.00`
4. Mine: `dotnet run mine`
5. Lock a block: `dotnet run addlock 1`
6. Query a block: `dotnet run query 1`
7. Validate: `dotnet run validate`