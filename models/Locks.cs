using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema; // New library for schema validation
using ServerApp;

namespace LockApp
{
    public class Lock
    {
        public DateTime Timestamp { get; set; }
        public int BlockId { get; set; }
        public string Schema { get; set; }
        public JObject Json { get; set; }

        public Lock(int blockId, string schema)
        {
            Timestamp = DateTime.Now;
            BlockId = blockId;
            Schema = schema;
            Json = JsonConvert.DeserializeObject<JObject>(schema) ?? new JObject();
        }

        public Lock()
        {
            Timestamp = DateTime.Now;
            BlockId = 0;
            Schema = "";
            Json = new JObject();
        }
    }

    public class Constraint
    {
        public List<Lock> LockChain { get; set; } = new List<Lock>();
        public string LocksPath { get; set; } = "./database/locks";
        public static int nextLockId = 1;

        public Constraint()
        {
            Directory.CreateDirectory(LocksPath);
        }

        public void AddSchemaToReceipt(int blockId, string schema)
        {
            string receiptPath = Path.Combine("./receipts/", $"block_{blockId}.json");
            if (!File.Exists(receiptPath))
            {
                PrettyPrint.PrintError($"Receipt for block {blockId} not found.");
                return;
            }

            try
            {
                var receiptJson = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(receiptPath)) ?? new JObject();
                var formData = JsonConvert.DeserializeObject<JObject>(schema) ?? new JObject();
                receiptJson["formData"] = formData;
                File.WriteAllText(receiptPath, JsonConvert.SerializeObject(receiptJson, Formatting.Indented));
                PrettyPrint.PrintSuccess($"Schema added to receipt for block {blockId}.");
            }
            catch (Exception ex)
            {
                PrettyPrint.PrintError($"Error adding schema to receipt: {ex.Message}");
            }
        }

        public void UpdateLockSchema(string schema)
        {
            var newLock = new Lock(nextLockId++, schema);
            LockChain.Add(newLock);
            string lockFile = Path.Combine(LocksPath, $"lock_{newLock.BlockId}.json");
            File.WriteAllText(lockFile, JsonConvert.SerializeObject(newLock, Formatting.Indented));
            PrettyPrint.PrintSuccess($"Lock schema updated for lock {newLock.BlockId}.");
        }

        public void EnforceConstraints(int blockId, string sender, string receiver, decimal amount)
        {
            var lockEntry = LockChain.FirstOrDefault(l => l.BlockId == blockId);
            if (lockEntry == null)
            {
                PrettyPrint.PrintError($"No lock found for block {blockId}. Proceeding without constraints.");
                return;
            }

            string receiptsPath = "./receipts/";
            var receiptFiles = Directory.GetFiles(receiptsPath, "*.json");
            var matchingReceipts = new List<int>();

            foreach (var file in receiptFiles)
            {
                try
                {
                    var receiptJson = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(file)) ?? new JObject();
                    var block = receiptJson.ToObject<Block>(); // Assuming Block class is accessible; adjust if needed
                    if (block != null && block.BlockId == blockId && receiptJson["formData"] != null)
                    {
                        var formData = receiptJson["formData"] as JObject;
                        bool matches = true;
                        foreach (var prop in lockEntry.Json.Properties())
                        {
                            if (formData?[prop.Name] == null)
                            {
                                matches = false;
                                break;
                            }
                        }
                        if (matches)
                        {
                            matchingReceipts.Add(blockId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    PrettyPrint.PrintError($"Error checking receipt {file}: {ex.Message}");
                }
            }

            if (matchingReceipts.Count == 0)
            {
                PrettyPrint.PrintError($"Constraints not fulfilled for block {blockId}. Required schema: {lockEntry.Schema}. Transaction pending.");
                return;
            }

            foreach (var receiptId in matchingReceipts)
            {
                PrettyPrint.PrintSuccess($"Matching receipt ID {receiptId} stored for constraints.");
            }

            PrettyPrint.PrintSuccess($"Constraints fulfilled for block {blockId}. Proceeding with transaction.");
        }

        public void InitializeLockChain()
        {
            var lockFiles = Directory.GetFiles(LocksPath, "*.json");
            foreach (var file in lockFiles)
            {
                try
                {
                    var lockJson = JsonConvert.DeserializeObject<Lock>(File.ReadAllText(file));
                    if (lockJson != null)
                    {
                        LockChain.Add(lockJson);
                    }
                }
                catch (Exception ex)
                {
                    PrettyPrint.PrintError($"Error loading lock from {file}: {ex.Message}");
                }
            }
        }

        public bool ValidateAgainstSchema(string tableName, JObject data)
        {
            string schemaPath = Path.Combine("./database", tableName, "schema.json");
            if (!File.Exists(schemaPath))
            {
                PrettyPrint.PrintWarning($"Warning: No schema found for table {tableName}. Skipping validation.");
                return true; // Skip if no schema
            }

            try
            {
                var schemaJson = File.ReadAllText(schemaPath);
                var schema = JsonSchema.FromJsonAsync(schemaJson).Result; // Async load; use .Result for sync
                var dataToken = JToken.FromObject(data);
                var errors = schema.Validate(dataToken);
                if (errors.Any())
                {
                    PrettyPrint.PrintError($"Schema validation errors for {tableName}: {string.Join(", ", errors.Select(e => e.ToString()))}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                PrettyPrint.PrintError($"Error validating schema for {tableName}: {ex.Message}");
                return false;
            }
        }

        public void SchemaParser(Lock locked)
        {
            PrettyPrint.PrintInfo($"Parsing schema for lock {locked.BlockId}: {locked.Schema}");
            // Could validate or manipulate the JObject here
        }
    }
}