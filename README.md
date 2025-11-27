<img width="4498" height="1252" alt="suprabullion_logo" src="https://github.com/user-attachments/assets/aa41883e-8c97-4137-815d-8ceb9cac2795" />

## Overview
SuprabullionApp is a C# console application implementing a peer-to-peer blockchain with PayPal integration, user registration, block locking, and querying. It features an interactive command-line interface (CLI), peer networking with persistence, config file support, and manual UPnP port forwarding for NAT traversal. The app uses SHA256 hashing for security, stores data in JSON format, and supports RSA encryption for user data.

## TODO
- class out Program.cs 
- Unit test classes 
- Multi-user network test 

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
dotnet add package NJsonSchema
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

---------------------------------------------------------------------------------------
Steps for Upgraded Database querying and Block Lock Extension
---------------------------------------------------------------------------------------

## Blockchain Application with Database and Lock Constraints - Documentation

## Overview
This is an upgraded version of a C# blockchain application that simulates a cryptocurrency system with peer-to-peer networking, PayPal integration, and blockchain management. Key upgrades include:
- **Database Functionality**: A new `Database` class (in `DataBaseApp` namespace) that supports SQL-like queries (CREATE, SELECT, INSERT, UPDATE, DELETE) on JSON-based "tables" stored in the `./database/` directory.
- **Lock Constraints**: A new `Constraint` class (in `LockApp` namespace) that manages locks with JSON schemas. Schemas are appended to receipts as "formData" and enforced during transactions (e.g., transfers or sells) to ensure constraints are met before fulfillment.
- **Enhanced CLI**: The `ExecuteCommand` switch in `Program.cs` now supports quoted strings for queries, locks, and schemas, enabling dynamic database interactions and constraint enforcement.
- **Integration**: Transactions (buy, sell, transfer) now check for locks and schemas, temporarily storing matching receipt IDs in the `LockChain` until constraints are fulfilled. Errors are logged to the console.

The application uses `Newtonsoft.Json` for JSON manipulation (e.g., reading/writing files, deserializing objects). It runs as a console CLI with commands for blockchain operations, database queries, and lock management.

### Prerequisites
- .NET environment with `Newtonsoft.Json` NuGet package installed.
- Directories: `./receipts/`, `./blocks/`, `./database/`, `./peers/`, `./database/locks/` (auto-created).
- PayPal sandbox credentials for buy/sell operations.

## Architecture
- **Program.cs**: Entry point with CLI loop. Instantiates `Blockchain`, `PeerNetwork`, `Database`, and `Constraint` classes. Handles user commands via `ExecuteCommand`.
- **Blockchain.cs**: Core blockchain logic (blocks, transactions, mining, locks). Writes receipts and blocks as JSON files.
- **PeerNetwork.cs**: Manages peer connections and notifications.
- **Database.cs** (`DataBaseApp` namespace): Parses and executes SQL-like queries on JSON files. Tables are subdirectories under `./database/`, with `schema.json` defining structure.
- **Locks.cs** (`LockApp` namespace): Manages `Lock` objects with schemas. Enforces constraints by checking receipts for matching "formData".
- **Interactions**:
  - CLI commands trigger blockchain operations or delegate to `Database`/`Constraint`.
  - Transactions check `Constraint.LockChain` for schemas and validate against receipts.
  - JSON files are used for persistence (e.g., receipts with appended schemas).

## New Features
1. **Database Queries**:
   - Supports basic SQL-like syntax for JSON-based tables.
   - Queries return JSON responses (status or data).
   - Example: `query "SELECT * FROM receipts WHERE ID=1"` executes a SELECT and prints results.

2. **Lock Schemas and Constraints**:
   - Locks can have JSON schemas appended to receipts.
   - During transfers/sells, the system checks for matching receipts and enforces schemas (e.g., required fields).
   - Constraints must be fulfilled before transactions complete; otherwise, errors are logged, and the transaction is pending.

3. **Enhanced Commands**:
   - `query`: Accepts quoted SQL strings or integer BlockIDs.
   - `addlock`: Accepts quoted schemas or integer BlockIDs.
   - `updatelock`: New command for updating lock schemas.
   - `buy`: Now accepts an optional schema parameter for receipts.

4. **JSON Manipulation with Newtonsoft.Json**:
   - Reading: `JsonConvert.DeserializeObject<T>(File.ReadAllText(path))`.
   - Writing: `File.WriteAllText(path, JsonConvert.SerializeObject(obj, Formatting.Indented))`.
   - Manipulation: Deserialize to `JObject`, modify properties (e.g., add "formData"), then serialize.

## API/Command Reference
Commands are entered in the CLI (e.g., `> buy Alice 50.00 "{\"key\": \"value\"}"`). All commands are case-insensitive.

| Command | Syntax | Description | Example |
|---------|--------|-------------|---------|
| `buy` | `buy <buyer> <amount> [schema]` | Buys from reserve via PayPal. Optional schema adds to receipt. | `buy Alice 50.00 "{\"requiredField\": \"value\"}"` |
| `sell` | `sell <seller> <amount>` | Sells to reserve. Checks constraints. | `sell Bob 25.00` |
| `transfer` | `transfer <sender> <receiver> <amount>` | P2P transfer. Checks constraints. | `transfer Alice Bob 10.00` |
| `mine` | `mine` | Mines pending transactions. | `mine` |
| `validate` | `validate` | Validates blockchain. | `validate` |
| `register` | `register <user> <ip> <peersHash> <privateKey> <publicKey>` | Registers a user. | `register Alice 127.0.0.1 hash priv pub` |
| `addlock` | `addlock <blockid>` or `addlock "<schema>" [blockid]` | Adds lock by ID or with schema. | `addlock 1` or `addlock "{\"key\": \"value\"}" 1` |
| `removelock` | `removelock <blockid>` | Removes lock by ID. | `removelock 1` |
| `query` | `query <blockid>` or `query "<sql_query>"` | Queries block by ID or executes SQL. | `query 1` or `query "SELECT * FROM receipts WHERE ID=1"` |
| `updatelock` | `updatelock "<schema>"` | Updates lock schema. | `updatelock "{\"newKey\": \"newValue\"}"` |

## Classes and Methods Summary
### Program.cs
- **ExecuteCommand(string action, string[] args)**: Routes commands to methods or classes.
- **BuyFromReserve(string buyer, decimal amount, string schema)**: Handles buys, adds schema to receipt.
- **SellToReserve(string seller, decimal amount)**: Handles sells, enforces constraints.
- **PeerToPeerTransfer(string sender, string receiver, decimal amount)**: Handles transfers, enforces constraints.

### Database.cs (DataBaseApp)
- **Database(Query qry)**: Constructor; sets up `./database/`.
- **StringParser(Query qry)**: Parses query string and routes to exec methods.
- **DbCreateParse/DbSelectParse/etc.**: Parse SQL tokens.
- **DbSelectExec/DbInsertExec/etc.**: Execute operations on JSON files.

### Locks.cs (LockApp)
- **Lock(int blockId, string schema)**: Constructor for locks with schema.
- **Constraint()**: Constructor; loads existing locks.
- **AddSchemaToReceipt(int blockId, string schema)**: Appends schema to receipt JSON.
- **UpdateLockSchema(string schema)**: Adds/updates lock with schema.
- **EnforceConstraints(int blockId, string sender, string receiver, decimal amount)**: Checks receipts for schema matches; logs errors if not fulfilled.

## Testing Steps
Follow these steps to test the upgraded features. Run the application (`dotnet run`) and use the CLI. Ensure directories exist and PayPal is configured for buy/sell tests. Expected outputs are noted.

### 1. **Setup and Basic Blockchain Test**
   - Start the app: `> validate` (should output "Blockchain valid: True" if no blocks exist).
   - Mine a block: `> mine` (outputs mining details).
   - Register a user: `> register Alice 127.0.0.1 hash priv pub` (outputs registration details).

### 2. **Test Database Queries**
   - Create a table: `> query "CREATE TABLE receipts (ID int, Name varchar)"` (Expected: JSON status "success"; creates `./database/receipts/schema.json`).
   - Insert data: `> query "INSERT INTO receipts VALUES (1, 'TestReceipt')"` (Expected: JSON status "success"; creates a JSON file in `./database/receipts/`).
   - Select data: `> query "SELECT * FROM receipts WHERE ID=1"` (Expected: JSON with data array containing the inserted record).
   - Update data: `> query "UPDATE receipts SET Name='Updated' WHERE ID=1"` (Expected: JSON status "success"; check file for changes).
   - Delete data: `> query "DELETE FROM receipts WHERE ID=1"` (Expected: JSON status "success"; file should be deleted).

### 3. **Test Locks and Schemas**
   - Add a lock with schema: `> addlock "{\"requiredField\": \"value\"}" 1` (Expected: Schema added to receipt `./receipts/block_1.json` under "formData").
   - Update lock schema: `> updatelock "{\"newField\": \"newValue\"}"` (Expected: New lock file in `./database/locks/`; console logs update).
   - Query a block: `> query 1` (Expected: JSON block data, including any schema in receipts).

### 4. **Test Transactions with Constraints**
   - Buy with schema: `> buy Alice 50.00 "{\"constraint\": \"met\"}"` (Expected: PayPal flow (mocked); schema added to new receipt).
   - Sell with constraints: First, ensure a lock exists. `> sell Alice 25.00` (If constraints not met, console logs errors like "Constraints not fulfilled"; otherwise, proceeds).
   - Transfer with constraints: `> transfer Alice Bob 10.00` (Similar to sell; checks receipts for matching "formData". If fulfilled, logs "Constraints fulfilled"; else, pending with errors).
   - Verify receipts: Check `./receipts/block_*.json` for "formData" appended schemas.

### 5. **Edge Cases and Validation**
   - Invalid query: `> query "INVALID"` (Expected: JSON error message).
   - Lock without schema: `> addlock 2` (Expected: Standard lock added without schema).
   - Constraint failure: Attempt transfer without matching receipts (Expected: Console errors; transaction doesn't complete).
   - Peer notification: After mining/locking, check for network logs (requires peers configured).

### Troubleshooting
- **JSON Errors**: Ensure schemas are valid JSON strings (e.g., no unescaped quotes).
- **File Not Found**: Run commands in sequence (e.g., create table before inserting).
- **PayPal Issues**: Use sandbox; check credentials in `PayPalConfig`.
- **Logs**: All operations log to console; check for exceptions.

## Contribution Git steps 
```bash
git remote add origin 'https://github.com/suprasage/SupraBullion.git'
git pull origin master --allow-unrelated-histories  
git add .
git commit -m "message"
git push -u origin master 
```

## Key Changes Confirmed in Locks.cs
- **PrettyPrint Integration**: All logging now uses the `PrettyPrint` class from `ServerApp` (added `using ServerApp;`). This provides:
  - Red timestamps for errors.
  - Green for successes.
  - Yellow for warnings.
  - Cyan for info.
  - White bold messages.
- **Schema Validation**: Uses `NJsonSchema` for robust JSON schema validation in `ValidateAgainstSchema`.
- **Error Handling**: Improved with try-catch and PrettyPrint for better user feedback.
- **No Breaking Changes**: The file remains compatible with `Database.cs` and the overall application.

## Final Project Summary
With all updates applied:
- **Program.cs**: Enhanced CLI with quoted string parsing, PrettyPrint, and command handling (e.g., `help`, schema support for `buy`).
- **Database.cs**: SQL-like queries with JSON schema generation and validation via NJsonSchema.
- **Locks.cs**: Constraint enforcement with schema validation and PrettyPrint logging.
- **Dependencies**: Switched to `NJsonSchema` for schema handling (install via `dotnet add package NJsonSchema`).

## Testing Steps for the Complete System
1. **Build and Run**: `dotnet build` (should have no warnings) then `dotnet run`.
2. **Test CLI Commands**:
   - `> help` – Lists all commands with descriptions.
   - `> query "CREATE TABLE receipts (ID int, Name varchar, email array)"` – Creates table with schema.
   - `> query "INSERT INTO receipts VALUES (1, 'Test', '[\"a@example.com\"]')"` – Inserts valid data.
   - `> query "INSERT INTO receipts VALUES ('invalid', 'Test', '[\"a@example.com\"]')"` – Should fail with red error log.
   - `> addlock "{\"key\": \"value\"}" 1` – Adds schema to receipt.
   - `> buy Alice 50.00 "{\"constraint\": \"met\"}"` – Buys with schema, checks constraints.
3. **Verify Output**: Logs should be colored and timestamped (e.g., green for success, red for errors).
4. **Edge Cases**: Test invalid schemas, missing files, or network operations.

If everything works, your blockchain app now supports advanced database operations with schema validation and pretty logging! If you encounter issues or need further tweaks, share the error logs or code snippets.