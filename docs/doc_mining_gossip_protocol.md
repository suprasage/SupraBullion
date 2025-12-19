### Documentation for Blockchain Mining and Gossip Protocol Updates

This documentation outlines the changes made to the Blockchain program (specifically in `Program.cs`) to implement a gossip protocol for mining consensus. The updates enhance the mining process by incorporating peer-to-peer validation, duplicate transaction prevention, and consensus-based finalization. These changes ensure that blocks are only added to the blockchain after agreement from multiple peers, improving decentralization and security.

#### Overview of Changes
The core updates focus on transforming the mining process from a local operation to a distributed consensus mechanism using a gossip protocol. Key components include:
- **Asynchronous Mining**: Mining now waits for peer responses, making it non-blocking.
- **Duplicate Checks**: Transactions are validated for uniqueness before mining.
- **Consensus Mechanism**: Blocks are proposed to peers; finalization requires at least 3 agreements (or local acceptance in small networks).
- **Gossip Protocol**: Peers exchange proposals and finalized chains via TCP sockets.
- **Peer Integration**: Uses the existing peer list from `./peers/` for communication.

These changes address build errors (e.g., accessibility and context issues) and warnings (e.g., async handling) from the previous version.

#### Detailed Feature Descriptions
- **Asynchronous Mining with Peer Reference**:
  - `MinePendingTransactions` is now an async method that accepts an optional `PeerNetwork?` parameter.
  - It filters out duplicate transactions from pending ones using `IsTransactionDuplicate`.
  - Mines the block locally, then proposes it to peers for consensus.
  - If consensus is reached, the block is added to the chain, receipts/blocks are written, and peers are notified of the finalized chain.
  - If consensus fails, the block is discarded.

- **Duplicate Transaction Prevention**:
  - `IsTransactionDuplicate` (now public) checks if a transaction (based on sender, receiver, amount, and PayPalOrderId) already exists in any block.
  - In `AddTransaction`, duplicates are skipped with a warning.
  - During proposal validation, peers also check for duplicates in the proposed block.

- **Consensus and Gossip Protocol**:
  - `ProposeBlockAsync` handles the proposal: sends the block to all known peers and collects agreements.
  - Requires at least 3 agreements; if fewer than 3 peers are known, defaults to local acceptance.
  - `SendProposalAndCollectAgreementsAsync` in `PeerNetwork` sends "PROPOSAL:" messages and waits for "AGREE:" responses (with a 10-second timeout per peer).
  - Peers validate proposals (hash, previous hash, duplicates) and respond accordingly.
  - Finalized chains are broadcast via "FINALIZED:" messages, updating peer blockchains if valid and longer.

- **Peer Networking Enhancements**:
  - `HandleClientAsync` now processes "PROPOSAL:" (validates and agrees/disagrees) and "FINALIZED:" (updates chain if valid).
  - `GetPeerCount` returns the number of known peers for consensus logic.
  - Peers are loaded/saved from `./peers/` as before.

- **Integration with Existing Features**:
  - Lock management, database queries, and PayPal integration remain unchanged.
  - `BuyFromReserve` and `SellToReserve` now await `MinePendingTransactions` to support async mining.
  - Commands like "mine" in the CLI now pass `peerNetwork` and await the result.

#### Prerequisites and Setup
- **Dependencies**: Ensure .NET 10.0, Newtonsoft.Json, and NJsonSchema are installed. The code uses TCP for peer communication, so ensure ports (default 8080) are open and not firewalled.
- **Peer Configuration**: Edit `config.json` to enable port forwarding if needed. Add peers via the CLI or manually in `./peers/`.
- **Network Setup**: Run multiple instances on different machines/ports for testing. Peers must be reachable via IP:port.
- **Security Note**: This is a demo implementation. In production, add encryption, digital signatures, and robust error handling for proposals.

#### Usage Guide
1. **Building and Running**:
   - Run `dotnet build` (should now succeed without errors).
   - Start the app: `dotnet run`.
   - Use the CLI for commands (e.g., `> mine` to trigger mining with consensus).

2. **Testing Consensus**:
   - Add peers: Manually edit `./peers/peers_1.json` or use networking to discover.
   - Simulate transactions: Use `buy`, `sell`, or `transfer`.
   - Mine: The system will propose the block and wait for agreements. Check logs for "consensus reached" or "failed".
   - Monitor: Peers will update their chains on finalization.

3. **Example Workflow**:
   - User A runs `buy Alice 50.00`.
   - System adds transaction, mines block, proposes to peers.
   - If 3+ peers agree, block is finalized and broadcast.
   - All peers update their blockchains.

4. **Troubleshooting**:
   - If consensus fails: Check peer connectivity, timeouts, or duplicate transactions.
   - Logs: Use `PrettyPrint` for info/errors.
   - Small Networks: With <3 peers, blocks finalize locally.

#### Potential Improvements
- Add retry logic for failed peer connections.
- Implement digital signatures for proposals to prevent tampering.
- Increase timeout or make it configurable for slower networks.
- Add metrics for consensus success rates.