<<<<<<< HEAD
<<<<<<< HEAD
## Overview
SuprabullionApp is a C# console application implementing a peer-to-peer blockchain with PayPal integration, user registration, block locking, and querying. It features an interactive command-line interface (CLI), peer networking with persistence, config file support, and manual UPnP port forwarding for NAT traversal. The app uses SHA256 hashing for security, stores data in JSON format, and supports RSA encryption for user data.
=======
=======
>>>>>>> df785bdbcbf60602559fbb36c4a9d39db6d636fb
## Version 1.0.5: Suprabullion C#
***
Suprabullion has gone through changes from the original database as a crypto currency. 
In order to keep the vision around SupraSage has decided to implement a C# version, 
this is for The Suns!
***
<<<<<<< HEAD
>>>>>>> df785bdbcbf60602559fbb36c4a9d39db6d636fb

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
=======

### Documentation for the Upgraded PayPal REST API Blockchain Program

This document provides comprehensive usage instructions for the upgraded C# console application, which implements a peer-to-peer blockchain with PayPal integration, receipts, block storage, lock functionality, user registration, and query capabilities. The program uses TCP sockets for peer communication, stores blocks and receipts in JSON format, and supports encrypted user registration.

#### Overview
- **Core Features**: Blockchain with mining, transactions (buy/sell/transfer), PayPal payments, peer networking, block locking/unlocking, user registration with RSA encryption, and block querying.
- **Directories**: 
  - `./receipts/`: Stores receipt files (e.g., `block_1.json`) for each mined block.
  - `./blocks/`: Stores block files (e.g., `block_1.json`) for querying by `BlockId`.
- **Networking**: Each instance runs as a peer, listening on a configurable port and notifying known peers via TCP.
- **Security**: RSA encryption for user data; PayPal sandbox/live environments; block integrity validation.
- **Dependencies**: .NET Core/6+, `Newtonsoft.Json` (install via `dotnet add package Newtonsoft.Json`).

#### Setup
1. **Build the Project**: Run `dotnet build` in the project directory.
2. **Configure Credentials**: Update `PayPalConfig` in the code with real PayPal client ID and secret for sandbox or live environments.
3. **Run Multiple Instances**: Start peers on different ports (e.g., port 8080 for Peer1, 8081 for Peer2). Modify `peerNetwork.AddPeer()` in `Main()` to connect peers.
4. **Directories**: `./receipts/` and `./blocks/` are auto-created. Ensure write permissions.
5. **RSA Keys**: For registration, generate RSA key pairs (public/private XML strings) using tools like `RSACryptoServiceProvider` or online generators.

#### CLI Commands
Run commands from the project directory using `dotnet run <command> [arguments]`. Outputs are logged to the console, and files are written to `./receipts/` and `./blocks/`.

- **buy <buyer> <amount>**  
  Initiates a PayPal payment from the user to the Reserve account. Creates an order, simulates capture, adds a transaction, mines a block, writes receipt/block, and notifies peers. If the block is locked, sends receipts to origination and receivers.  
  - Arguments: `<buyer>` (string, e.g., "Alice"), `<amount>` (decimal, e.g., 50.00).  
  - Example: `dotnet run buy Alice 50.00`  
  - Output: PayPal order details, capture status, transaction confirmation, receipt/block file paths, peer notifications.  
  - Notes: Requires valid PayPal credentials. Block gets a unique `BlockId` and default `Lock=false`.

- **sell <seller> <amount>**  
  Simulates selling to the Reserve account (adds transaction, mines block, writes receipt/block, notifies peers; real payouts require PayPal Payouts API). If locked, sends receipts.  
  - Arguments: `<seller>` (string, e.g., "Bob"), `<amount>` (decimal, e.g., 30.00).  
  - Example: `dotnet run sell Bob 30.00`  
  - Output: Transaction confirmation, receipt/block file paths, peer notifications.  
  - Notes: No actual PayPal payout; integrate Payouts API for production.

- **transfer <sender> <receiver> <amount>**  
  Performs a peer-to-peer blockchain transfer (adds transaction to pending list; no PayPal or mining).  
  - Arguments: `<sender>` (string, e.g., "Alice"), `<receiver>` (string, e.g., "Bob"), `<amount>` (decimal, e.g., 20.00).  
  - Example: `dotnet run transfer Alice Bob 20.00`  
  - Output: Transaction confirmation.  
  - Notes: Mine separately with `mine` to finalize.

- **mine**  
  Mines all pending transactions into a new block, writes receipt/block, and notifies peers with the updated chain. If locked, sends receipts.  
  - Arguments: None.  
  - Example: `dotnet run mine`  
  - Output: Mining progress, block hash, receipt/block file paths, peer notification status.  
  - Notes: Assigns `BlockId`; checks lock conditions.

- **validate**  
  Checks the blockchain's integrity (valid hashes and links) by reading chain data.  
  - Arguments: None.  
  - Example: `dotnet run validate`  
  - Output: "Blockchain valid: True" or "Blockchain valid: False" with details.  
  - Notes: Reads all receipts in `./receipts/` for verification.

- **register <user> <ip> <peersHash> <privateKey> <publicKey>**  
  Registers a user by creating an encrypted registration block (IP, peers hash, private key encrypted with public key; public key unencrypted). Sets `Origination` to the public key hash.  
  - Arguments: `<user>` (string, e.g., "Alice"), `<ip>` (string, e.g., "192.168.1.1"), `<peersHash>` (string, e.g., "abc123"), `<privateKey>` (string, RSA private key XML), `<publicKey>` (string, RSA public key XML).  
  - Example: `dotnet run register Alice 192.168.1.1 abc123 "<RSAKeyValue>...</RSAKeyValue>" "<RSAKeyValue>...</RSAKeyValue>"`  
  - Output: Registration confirmation with block hash.  
  - Notes: Use RSA key generation tools for keys; `Origination` is used for locks.

- **addlock <blockid>**  
  Adds a lock to the specified block ID by creating a new block logging the change, writing receipt/block, and notifying peers. Checks receipts for existing locks and validates the block was originally lockable.  
  - Arguments: `<blockid>` (int, e.g., 1).  
  - Example: `dotnet run addlock 1`  
  - Output: Lock confirmation, new block details, receipt/block file paths.  
  - Notes: Only works if the block exists and isn't already locked. Sets `Lock=true` and uses `Origination` from the target block.

- **removelock <blockid>**  
  Removes a lock from the specified block ID by creating a new block logging the change, writing receipt/block, and notifying peers. Checks receipts for the lock.  
  - Arguments: `<blockid>` (int, e.g., 1).  
  - Example: `dotnet run removelock 1`  
  - Output: Unlock confirmation, new block details, receipt/block file paths.  
  - Notes: Only works if a lock exists for the block ID.

- **query <blockid>**  
  Queries and prints all data for the block with the specified `BlockId` by walking `./blocks/` and reading the matching JSON file.  
  - Arguments: `<blockid>` (int, e.g., 1).  
  - Example: `dotnet run query 1`  
  - Output: Full block data in JSON format, or "Block with BlockId X not found."  
  - Notes: Searches `./blocks/` for `block_<blockid>.json`.

#### Networking
- **Peer Setup**: Each instance listens on a port (e.g., 8080) and connects to known peers. Modify `PeerNetwork` constructor and `AddPeer()` for your setup.
- **Messages**: Peers exchange "CHAIN:" (blockchain data) and "RECEIPT:" (receipt data) messages. Locked blocks trigger extra receipts to origination and receivers.
- **Handling**: Incoming messages save data to `./receipts/received_chain.json` or `./receipts/received_receipt.json`.
- **Notes**: Use firewalls to secure ports; messages are plain text (add encryption for production).

#### Security Notes
- **PayPal**: Use sandbox for testing; live requires real credentials and compliance.
- **Encryption**: User registration encrypts sensitive data with RSA; public keys are unencrypted for verification.
- **Blockchain Integrity**: `validate` ensures no tampering; blocks are hashed and linked.
- **Keys**: Store RSA keys securely; never hardcode in production.
- **Networking**: TCP sockets are unencrypted; implement SSL/TLS for secure communication.
- **Locks**: Ensure only authorized users can lock/unlock blocks; current implementation is basic.

#### Example Usage Session
1. Register a user: `dotnet run register Bob 192.168.1.1 abc123 "<privateKey>" "<publicKey>"`
2. Buy from Reserve: `dotnet run buy 3b71b9def0a044a1aabf2f94dcdce5c21d6889bd5554bfe91d066efc2399bf21 50.00`
>>>>>>> df785bdbcbf60602559fbb36c4a9d39db6d636fb
3. Transfer: `dotnet run transfer Alice Bob 20.00`
4. Mine: `dotnet run mine`
5. Lock a block: `dotnet run addlock 1`
6. Query a block: `dotnet run query 1`
<<<<<<< HEAD
<<<<<<< HEAD
7. Validate: `dotnet run validate`
=======
=======
>>>>>>> df785bdbcbf60602559fbb36c4a9d39db6d636fb
7. Validate: `dotnet run validate`

#### Troubleshooting
- **Build Errors**: Ensure no duplicate files; remove `backup/Program.v2.cs`.
- **PayPal Failures**: Check credentials and network.
- **Peer Connections**: Verify ports are open and peers are running.
- **File Access**: Ensure `./receipts/` and `./blocks/` are writable.

### Quick Recap of Final Features
- **Interactive Mode**: Run `dotnet run` and type commands at the prompt.
- **Commands**: buy, sell, transfer, mine, validate, register, addlock, removelock, query.
- **Peer Networking**: Runs in background on port 8080.
- **RSA Fallback**: Auto-generates keys if invalid ones are provided.
- **File Storage**: Blocks and receipts saved to `./blocks/` and `./receipts/`.
<<<<<<< HEAD
>>>>>>> df785bdbcbf60602559fbb36c4a9d39db6d636fb
=======
>>>>>>> df785bdbcbf60602559fbb36c4a9d39db6d636fb
