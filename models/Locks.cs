using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerApp;

namespace LockApp
{
    // Class meant for JSON schema storage and manipulation
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

        // Default constructor (as per original)
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

        // Method to add schema to a receipt's JSON as form data
        public void AddSchemaToReceipt(int blockId, string schema)
        {
            string receiptPath = Path.Combine("./receipts/", $"block_{blockId}.json");
            if (!File.Exists(receiptPath))
            {
                Console.WriteLine($"Receipt for block {blockId} not found.");
                return;
            }

            try
            {
                var receiptJson = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(receiptPath)) ?? new JObject();
                var formData = JsonConvert.DeserializeObject<JObject>(schema) ?? new JObject();
                receiptJson["formData"] = formData; // Append schema as formData
                File.WriteAllText(receiptPath, JsonConvert.SerializeObject(receiptJson, Formatting.Indented));
                Console.WriteLine($"Schema added to receipt for block {blockId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding schema to receipt: {ex.Message}");
            }
        }

        // Method to update lock schema (for updatelock command)
        public void UpdateLockSchema(string schema)
        {
            // Assume updating the last lock or a specific one; for simplicity, update all or add a new one
            // Here, we'll add a new lock with the schema
            var newLock = new Lock(nextLockId++, schema);
            LockChain.Add(newLock);
            // Optionally save to file
            string lockFile = Path.Combine(LocksPath, $"lock_{newLock.BlockId}.json");
            File.WriteAllText(lockFile, JsonConvert.SerializeObject(newLock, Formatting.Indented));
            Console.WriteLine($"Lock schema updated for lock {newLock.BlockId}.");
        }

        // Method to enforce constraints during Transfer or Sell
        public void EnforceConstraints(int blockId, string sender, string receiver, decimal amount)
        {
            // Find the lock for this blockId
            var lockEntry = LockChain.FirstOrDefault(l => l.BlockId == blockId);
            if (lockEntry == null)
            {
                Console.WriteLine($"No lock found for block {blockId}. Proceeding without constraints.");
                return;
            }

            // Check receipts directory for matching receipts
            string receiptsPath = "./receipts/";
            var receiptFiles = Directory.GetFiles(receiptsPath, "*.json");
            var matchingReceipts = new List<int>();

            foreach (var file in receiptFiles)
            {
                try
                {
                    var receiptJson = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(file)) ?? new JObject();
                    var block = receiptJson.ToObject<Block>(); // Assuming Block class is accessible or deserialize manually
                    if (block != null && block.BlockId == blockId && receiptJson["formData"] != null)
                    {
                        // Check if formData matches the schema (basic check: if all schema keys are present)
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
                            matchingReceipts.Add(blockId); // Store the blockId as receipt ID
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking receipt {file}: {ex.Message}");
                }
            }

            if (matchingReceipts.Count == 0)
            {
                Console.WriteLine($"Constraints not fulfilled for block {blockId}. Required schema: {lockEntry.Schema}. Transaction pending.");
                // Do not proceed; log errors
                return;
            }

            // Store matching receipts in LockChain temporarily
            foreach (var receiptId in matchingReceipts)
            {
                // Assuming we add to LockChain or a sublist; for simplicity, log
                Console.WriteLine($"Matching receipt ID {receiptId} stored for constraints.");
            }

            // Once constraints are met, proceed (this would be called after checks)
            Console.WriteLine($"Constraints fulfilled for block {blockId}. Proceeding with transaction.");
            // In a real implementation, send receipts to origination and recipient here
        }

        // Placeholder for LockChain method (as per original, but renamed to avoid conflict)
        public void InitializeLockChain()
        {
            // Load existing locks from files
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
                    Console.WriteLine($"Error loading lock from {file}: {ex.Message}");
                }
            }
        }

        // Placeholder for SchemaParser (as per original)
        public void SchemaParser(Lock locked)
        {
            // Parse the schema JSON; for now, just log
            Console.WriteLine($"Parsing schema for lock {locked.BlockId}: {locked.Schema}");
            // Could validate or manipulate the JObject here
        }
    }
}
