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