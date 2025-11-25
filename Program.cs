using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServerApp
{
    public enum Environment { SANDBOX, LIVE }

    public class Credential
    {
        public string? UserName { get; set; } // Made nullable
        public string? Password { get; set; } // Made nullable
    }

    public static class PayPalConfig
    {
        public static Credential Sandbox = new Credential { UserName = "YourLiveClientId", Password = "YourLiveClientSecret" };
        public static Credential Live = new Credential { UserName = "YourLiveClientId", Password = "YourLiveClientSecret" };
    }

    // Simple Transaction class for the blockchain
    public class Transaction
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public decimal Amount { get; set; }
        public string? PayPalOrderId { get; set; } // Made nullable

        public Transaction(string sender, string receiver, decimal amount)
        {
            Sender = sender;
            Receiver = receiver;
            Amount = amount;
        }
    }

    // Registration Block class
    public class RegistrationBlock
    {
        public string User { get; set; }
        public string EncryptedData { get; set; } // Encrypted: [IP] [PeersHash] [PrivateKey]
        public string PublicKey { get; set; } // Unencrypted for verification
        public string Hash { get; set; }

        public RegistrationBlock(string user, string ip, string peersHash, string privateKey, string publicKey)
        {
            User = user;
            PublicKey = publicKey;
            EncryptedData = EncryptData($"{ip}|{peersHash}|{privateKey}", publicKey);
            Hash = CalculateHash();
        }

        private string EncryptData(string data, string publicKey)
        {
            // For demo purposes, skip encryption to avoid RSA size limits.
            // In production, use AES for data and RSA for the AES key.
            // Return a hash for basic privacy instead.
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes); // Return hashed data
            }
            // Original encryption code (commented out):
            // using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            // {
            //     rsa.FromXmlString(publicKey);
            //     byte[] dataToEncrypt = Encoding.UTF8.GetBytes(data);
            //     byte[] encryptedData = rsa.Encrypt(dataToEncrypt, false);
            //     return Convert.ToBase64String(encryptedData);
            // }
        }

        private string CalculateHash()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string input = $"{User}{EncryptedData}{PublicKey}";
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }

    // Simple Block class with new fields
    public class Block
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public int Nonce { get; set; }
        public string PeerReference { get; set; }
        public int BlockId { get; set; } // New: Unique block ID
        public bool Lock { get; set; } // New: Lock status
        public string Origination { get; set; } // New: Origination public key hash

        public Block(int index, List<Transaction> transactions, string previousHash, string peerReference = "", int blockId = 0, bool lockStatus = false, string origination = "")
        {
            Index = index;
            Timestamp = DateTime.Now;
            Transactions = transactions;
            PreviousHash = previousHash;
            PeerReference = peerReference;
            BlockId = blockId;
            Lock = lockStatus;
            Origination = origination;
            Nonce = 0;
            Hash = CalculateHash();
        }

        public string CalculateHash()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string input = $"{Index}{Timestamp}{string.Join("", Transactions)}{PreviousHash}{Nonce}{PeerReference}{BlockId}{Lock}{Origination}";
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public void MineBlock(int difficulty)
        {
            string target = new string('0', difficulty);
            while (!Hash.StartsWith(target))
            {
                Nonce++;
                Hash = CalculateHash();
            }
            Console.WriteLine($"Block {Index} mined: {Hash}");
        }
    }

    // Simple Blockchain class
    public class Blockchain
    {
        public List<Block> Chain { get; set; }
        public List<RegistrationBlock> Registrations { get; set; }
        public List<Transaction> PendingTransactions { get; set; }
        public int Difficulty { get; set; }
        public const string ReserveAccount = "Reserve";
        public string ReceiptsPath { get; set; } = "./receipts/";
        public string BlocksPath { get; set; } = "./blocks/"; // New: Path for block files
        private static int nextBlockId = 1; // Auto-increment block IDs

        public Blockchain()
        {
            Chain = new List<Block>();
            Registrations = new List<RegistrationBlock>();
            PendingTransactions = new List<Transaction>();
            Difficulty = 2;
            Directory.CreateDirectory(ReceiptsPath);
            Directory.CreateDirectory(BlocksPath); // Create blocks directory
            Chain.Add(new Block(0, new List<Transaction>(), "0", blockId: 0));
        }

        public void AddTransaction(Transaction transaction)
        {
            PendingTransactions.Add(transaction);
        }

        public void MinePendingTransactions(string peerReference = "")
        {
            int blockId = nextBlockId++;
            Block newBlock = new Block(Chain.Count, PendingTransactions, Chain[Chain.Count - 1].Hash, peerReference, blockId);
            newBlock.MineBlock(Difficulty);
            Chain.Add(newBlock);
            PendingTransactions = new List<Transaction>();
            WriteReceipt(newBlock);
            WriteBlock(newBlock); // New: Write block to BlocksPath
        }

        private void WriteReceipt(Block block)
        {
            string filePath = Path.Combine(ReceiptsPath, $"block_{block.Index}.json");
            File.WriteAllText(filePath, JsonConvert.SerializeObject(block, Formatting.Indented));
            Console.WriteLine($"Receipt written: {filePath}");
        }

        private void WriteBlock(Block block) // New: Write block to BlocksPath
        {
            string filePath = Path.Combine(BlocksPath, $"block_{block.BlockId}.json");
            File.WriteAllText(filePath, JsonConvert.SerializeObject(block, Formatting.Indented));
            Console.WriteLine($"Block written: {filePath}");
        }

        public bool IsChainValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                Block current = Chain[i];
                Block previous = Chain[i - 1];

                if (current.Hash != current.CalculateHash())
                    return false;

                if (current.PreviousHash != previous.Hash)
                    return false;
            }
            return true;
        }

        public void RegisterUser(string user, string ip, string peersHash, string privateKey, string publicKey)
        {
            var regBlock = new RegistrationBlock(user, ip, peersHash, privateKey, publicKey);
            Registrations.Add(regBlock);
            Console.WriteLine($"User {user} registered with block hash: {regBlock.Hash}");
        }

        // New: AddLock function
        public void AddLock(int blockId)
        {
            // Check receipts for existing lock on this blockId
            var receipts = Directory.GetFiles(ReceiptsPath, "*.json");
            foreach (var receipt in receipts)
            {
                var json = File.ReadAllText(receipt);
                var block = JsonConvert.DeserializeObject<Block>(json);
                if (block != null && block.Lock && block.BlockId == blockId) // Null check
                {
                    Console.WriteLine($"Block {blockId} is already locked.");
                    return;
                }
            }

            // Find the block in chain and get origination
            var targetBlock = Chain.FirstOrDefault(b => b.BlockId == blockId);
            if (targetBlock == null || !targetBlock.Lock) // Only lock if not already locked
            {
                Console.WriteLine($"Block {blockId} not found or already locked.");
                return;
            }

            // Create lock block
            var lockBlock = new Block(Chain.Count, new List<Transaction> { new Transaction("System", "Lock", 0) }, Chain[Chain.Count - 1].Hash, blockId: nextBlockId++, lockStatus: true, origination: targetBlock.Origination);
            lockBlock.MineBlock(Difficulty);
            Chain.Add(lockBlock);
            WriteReceipt(lockBlock);
            WriteBlock(lockBlock);
            Console.WriteLine($"Lock added for block {blockId}, new block: {lockBlock.Index}");
        }

        // New: RemoveLock function
        public void RemoveLock(int blockId)
        {
            // Check receipts for lock on this blockId
            var receipts = Directory.GetFiles(ReceiptsPath, "*.json");
            foreach (var receipt in receipts)
            {
                var json = File.ReadAllText(receipt);
                var block = JsonConvert.DeserializeObject<Block>(json);
                if (block != null && block.Lock && block.BlockId == blockId) // Null check
                {
                    // Create unlock block
                    var unlockBlock = new Block(Chain.Count, new List<Transaction> { new Transaction("System", "Unlock", 0) }, Chain[Chain.Count - 1].Hash, blockId: nextBlockId++, lockStatus: false, origination: block.Origination);
                    unlockBlock.MineBlock(Difficulty);
                    Chain.Add(unlockBlock);
                    WriteReceipt(unlockBlock);
                    WriteBlock(unlockBlock);
                    Console.WriteLine($"Lock removed for block {blockId}, new block: {unlockBlock.Index}");
                    return;
                }
            }
            Console.WriteLine($"No lock found for block {blockId}.");
        }

        // New: Get origination public key for locked block
        public string? GetOriginationForLockedBlock(int blockId) // Nullable return
        {
            var receipts = Directory.GetFiles(ReceiptsPath, "*.json");
            foreach (var receipt in receipts)
            {
                var json = File.ReadAllText(receipt);
                var block = JsonConvert.DeserializeObject<Block>(json);
                if (block != null && block.Lock && block.BlockId == blockId && block.BlockId > 0) // Null check
                {
                    // Check blockchain for the block
                    var chainBlock = Chain.FirstOrDefault(b => b.BlockId == blockId && b.Lock);
                    if (chainBlock != null)
                    {
                        return chainBlock.Origination;
                    }
                }
            }
            return null;
        }

        // New: Query block by blockId
        public void QueryBlock(int blockId)
        {
            var blockFiles = Directory.GetFiles(BlocksPath, "*.json");
            foreach (var file in blockFiles)
            {
                var json = File.ReadAllText(file);
                var block = JsonConvert.DeserializeObject<Block>(json);
                if (block != null && block.BlockId == blockId) // Null check
                {
                    Console.WriteLine($"Block Data for BlockId {blockId}:");
                    Console.WriteLine(JsonConvert.SerializeObject(block, Formatting.Indented));
                    return;
                }
            }
            Console.WriteLine($"Block with BlockId {blockId} not found.");
        }
    }

    // PayPal API helper class
    public class PayPalAPI
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<string?> AuthenticateAsync(Environment environment) // Already nullable
        {
            var credential = PayPalConfig.Sandbox;
            if (environment == Environment.LIVE) credential = PayPalConfig.Live;
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{credential.UserName ?? ""}:{credential.Password ?? ""}")); // Null check
            var apiAuthEndpoint = "https://api-m.sandbox.paypal.com/v1/oauth2/token";
            if (environment == Environment.LIVE)
            {
                apiAuthEndpoint = "https://api-m.paypal.com/v1/oauth2/token";
            }

            var content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);
            var response = await httpClient.PostAsync(apiAuthEndpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var jAuthResponse = JObject.Parse(responseString);
            return jAuthResponse["access_token"]?.ToString(); // Nullable
        }


        public static async Task<(string paypalId, string url)> CreateOrderAsync(string token, decimal amount, string currency, Environment environment, string returnUrl, string cancelUrl)
        {
            var intent = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = currency,
                            value = amount.ToString("F2")
                        }
                    }
                },
                payment_source = new
                {
                    paypal = new
                    {
                        experience_context = new
                        {
                            payment_method_preference = "UNRESTRICTED",
                            landing_page = "LOGIN",
                            user_action = "PAY_NOW",
                            return_url = returnUrl,
                            cancel_url = cancelUrl
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(intent);
            var apiEndpoint = "https://api-m.sandbox.paypal.com/v2/checkout/orders";
            if (environment == Environment.LIVE)
            {
                apiEndpoint = "https://api-m.paypal.com/v2/checkout/orders";
            }

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PostAsync(apiEndpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var jOrderSetup = JObject.Parse(responseString);
            var paypalId = jOrderSetup["id"]?.ToString() ?? ""; // Null check
            var linksArray = jOrderSetup["links"] as JArray;
            var url = "";
            if (linksArray != null) // Null check
            {
                foreach (var link in linksArray)
                {
                    if (link["rel"]?.ToString() == "payer-action")
                    {
                        url = link["href"]?.ToString() ?? ""; // Null check
                        break;
                    }
                }
            }
            return (paypalId, url);
        }

        public static async Task<string> CaptureOrderAsync(string token, string paypalOrderId, Environment environment)
        {
            var apiCaptureEndpoint = "https://api-m.sandbox.paypal.com/v2/checkout/orders/{0}/capture";
            if (environment == Environment.LIVE)
            {
                apiCaptureEndpoint = "https://api-m.paypal.com/v2/checkout/orders/{0}/capture";
            }
            apiCaptureEndpoint = string.Format(apiCaptureEndpoint, paypalOrderId);

            var content = new StringContent("", Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PostAsync(apiCaptureEndpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var jCaptureResponse = JObject.Parse(responseString);
            return jCaptureResponse["status"]?.ToString() ?? ""; // Null check
        }
    }

    // New: Config class for app settings
    public class AppConfig
    {
        public bool EnablePortForwarding { get; set; } = false; // Default: disabled
        public int Port { get; set; } = 8080; // Default port
        public string PeerId { get; set; } = "Peer1"; // Default peer ID

        public static AppConfig Load(string configPath = "config.json")
        {
            if (File.Exists(configPath))
            {
                try
                {
                    var json = File.ReadAllText(configPath);
                    return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading config: {ex.Message}. Using defaults.");
                    return new AppConfig();
                }
            }
            else
            {
                // Create default config
                var defaultConfig = new AppConfig();
                File.WriteAllText(configPath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
                Console.WriteLine($"Created default config at {configPath}. Edit and restart.");
                return defaultConfig;
            }
        }
    }


    // Peer Networking class with enhanced receipt sending and peer persistence
    public class PeerNetwork
    {
        private TcpListener listener;
        private List<string> knownPeers = new List<string>();
        private Blockchain blockchain;
        private string myPeerId;
        private const string PeersPath = "./peers/";
        private const int PeersPerFile = 100;
        private AppConfig config;

        public PeerNetwork(Blockchain bc, AppConfig appConfig)
        {
            blockchain = bc;
            config = appConfig;
            myPeerId = config.PeerId;
            Directory.CreateDirectory(PeersPath);
            LoadPeers();
            listener = new TcpListener(IPAddress.Any, config.Port);
        }

        public void AddPeer(string peerAddress)
        {
            if (!knownPeers.Contains(peerAddress))
            {
                knownPeers.Add(peerAddress);
                SavePeers();
            }
        }

        public async Task StartAsync()
        {
            if (config.EnablePortForwarding)
            {
                await ForwardPort(config.Port);
            }

            listener.Start();
            Console.WriteLine($"Peer {myPeerId} listening on port {((IPEndPoint)listener.LocalEndpoint).Port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        // New: Manual UPnP port forwarding (no external library)
        private async Task ForwardPort(int port)
        {
            try
            {
                // Discover UPnP device (basic SSDP search)
                var ssdpRequest = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 3\r\nST: urn:schemas-upnp-org:device:InternetGatewayDevice:1\r\n\r\n";
                using (var udpClient = new UdpClient())
                {
                    udpClient.Send(Encoding.UTF8.GetBytes(ssdpRequest), ssdpRequest.Length, "239.255.255.250", 1900);
                    udpClient.Client.ReceiveTimeout = 5000;
                    var result = await udpClient.ReceiveAsync();
                    var response = Encoding.UTF8.GetString(result.Buffer);

                    // Extract control URL from response
                    var locationMatch = System.Text.RegularExpressions.Regex.Match(response, @"LOCATION: (.*?)\r\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (!locationMatch.Success) throw new Exception("UPnP device not found.");

                    string controlUrl = locationMatch.Groups[1].Value;
                    var uri = new Uri(controlUrl);
                    string serviceUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}/upnp/control/WANIPConnection1"; // Common service

                    // SOAP request to add port mapping
                    string soapBody = $@"<?xml version=""1.0""?>
    <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
    <s:Body>
    <u:AddPortMapping xmlns:u=""urn:schemas-upnp-org:service:WANIPConnection:1"">
    <NewRemoteHost></NewRemoteHost>
    <NewExternalPort>{port}</NewExternalPort>
    <NewProtocol>TCP</NewProtocol>
    <NewInternalPort>{port}</NewInternalPort>
    <NewInternalClient>{GetLocalIPAddress()}</NewInternalClient>
    <NewEnabled>1</NewEnabled>
    <NewPortMappingDescription>BlockchainAppPortForward</NewPortMappingDescription>
    <NewLeaseDuration>0</NewLeaseDuration>
    </u:AddPortMapping>
    </s:Body>
    </s:Envelope>";

                    using (var httpClient = new HttpClient())
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);
                        request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:WANIPConnection:1#AddPortMapping\"");
                        request.Content = new StringContent(soapBody, Encoding.UTF8, "text/xml");

                        var responseMsg = await httpClient.SendAsync(request);
                        if (responseMsg.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Port {port} forwarded successfully via UPnP.");
                        }
                        else
                        {
                            Console.WriteLine($"Port forwarding failed: {responseMsg.StatusCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Port forwarding failed: {ex.Message}");
            }
        }

        // Helper: Get local IP address
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }


        private async Task HandleClientAsync(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream))
            using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
            {
                string? _message = await reader.ReadLineAsync();
                string message;
                if (_message != null)
                {
                    message = _message;
                }
                else
                {
                    message = "Empty Response";
                    Console.WriteLine("ReadLine for client returns Null response.");
                }
                if (message != null)
                {
                    if (message.StartsWith("CHAIN:"))
                    {
                        string chainData = message.Substring(6);
                        File.WriteAllText(Path.Combine(blockchain.ReceiptsPath, "received_chain.json"), chainData);
                        Console.WriteLine("Received and saved chain data.");
                    }
                    else if (message.StartsWith("RECEIPT:"))
                    {
                        string receiptData = message.Substring(8);
                        File.WriteAllText(Path.Combine(blockchain.ReceiptsPath, "received_receipt.json"), receiptData);
                        Console.WriteLine("Received and saved receipt data.");
                    }
                }
            }
        }

        public async Task NotifyPeersAsync(string message, Block? block = null)
        {
            foreach (var peer in knownPeers)
            {
                try
                {
                    var parts = peer.Split(':');
                    TcpClient client = new TcpClient();
                    await client.ConnectAsync(parts[0], int.Parse(parts[1]));
                    using (NetworkStream stream = client.GetStream())
                    using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
                    {
                        await writer.WriteLineAsync(message);
                        if (block != null && block.Lock)
                        {
                            string? originationHash = blockchain.GetOriginationForLockedBlock(block.BlockId);
                            if (!string.IsNullOrEmpty(originationHash))
                            {
                                await writer.WriteLineAsync($"RECEIPT:{JsonConvert.SerializeObject(block)} to {originationHash}");
                            }
                            await writer.WriteLineAsync($"RECEIPT:{JsonConvert.SerializeObject(block)} to receivers");
                        }
                    }
                    client.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to notify peer {peer}: {ex.Message}");
                }
            }
        }

        // New: Save peers to files with limit
        private void SavePeers()
        {
            // Clear existing files
            var existingFiles = Directory.GetFiles(PeersPath, "peers_*.json");
            foreach (var file in existingFiles)
            {
                File.Delete(file);
            }

            // Split into chunks and save
            for (int i = 0; i < knownPeers.Count; i += PeersPerFile)
            {
                var chunk = knownPeers.Skip(i).Take(PeersPerFile).ToList();
                string fileName = $"peers_{i / PeersPerFile + 1}.json";
                string filePath = Path.Combine(PeersPath, fileName);
                File.WriteAllText(filePath, JsonConvert.SerializeObject(chunk, Formatting.Indented));
            }
        }

        // New: Load peers from files
        private void LoadPeers()
        {
            var peerFiles = Directory.GetFiles(PeersPath, "peers_*.json").OrderBy(f => f);
            foreach (var file in peerFiles)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var peers = JsonConvert.DeserializeObject<List<string>>(json);
                    if (peers != null)
                    {
                        knownPeers.AddRange(peers);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading peers from {file}: {ex.Message}");
                }
            }
        }
    }

    class Program
    {
        private const Environment environment = Environment.SANDBOX;
        private const string domain = "http://localhost:5000";
        private static Blockchain blockchain = new Blockchain();
        private static PeerNetwork? peerNetwork;

        static async Task Main(string[] args)
        {
            // New: Load config
            var config = AppConfig.Load();

            // Initialize peer network with config
            peerNetwork = new PeerNetwork(blockchain, config);
            peerNetwork.AddPeer("127.0.0.1:8081"); // Example peer
            _ = peerNetwork.StartAsync();

            // Handle Ctrl+C to exit gracefully
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent abrupt exit
                Console.WriteLine("\nExiting... Goodbye!");
                System.Environment.Exit(0); // Fixed: Qualify with System
            };

            Console.WriteLine("Blockchain CLI started. Type commands (e.g., 'buy Alice 50.00') or Ctrl+C to exit.");

            // Interactive command loop
            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;

                string[] commandArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (commandArgs.Length == 0) continue;

                string action = commandArgs[0].ToLower();
                await ExecuteCommand(action, commandArgs);
            }
        }


        private static async Task ExecuteCommand(string action, string[] args)
        {
            switch (action)
            {
                case "buy":
                    if (args.Length < 3 || args[1] == null || args[2] == null) { Console.WriteLine("Usage: buy <buyer> <amount>"); return; }
                    await BuyFromReserve(args[1], decimal.Parse(args[2]));
                    break;
                case "sell":
                    if (args.Length < 3 || args[1] == null || args[2] == null) { Console.WriteLine("Usage: sell <seller> <amount>"); return; }
                    await SellToReserve(args[1], decimal.Parse(args[2]));
                    break;
                case "transfer":
                    if (args.Length < 4 || args[1] == null || args[2] == null || args[3] == null) { Console.WriteLine("Usage: transfer <sender> <receiver> <amount>"); return; }
                    PeerToPeerTransfer(args[1], args[2], decimal.Parse(args[3]));
                    break;
                case "mine":
                    blockchain.MinePendingTransactions();
                    if (peerNetwork != null)
                    {
                        await peerNetwork!.NotifyPeersAsync($"CHAIN:{JsonConvert.SerializeObject(blockchain.Chain)}");
                    }
                    break;
                case "validate":
                    Console.WriteLine($"Blockchain valid: {blockchain.IsChainValid()}");
                    break;
                case "register":
                    if (args.Length < 6 || args[1] == null || args[2] == null || args[3] == null || args[4] == null || args[5] == null) { Console.WriteLine("Usage: register <user> <ip> <peersHash> <privateKey> <publicKey>"); return; }
                    RegisterUser(args[1], args[2], args[3], args[4], args[5]);
                    break;
                case "addlock":
                    if (args.Length < 2) { Console.WriteLine("Usage: addlock <blockid>"); return; }
                    blockchain.AddLock(int.Parse(args[1]));
                    if (peerNetwork != null)
                    {
                        await peerNetwork!.NotifyPeersAsync($"CHAIN:{JsonConvert.SerializeObject(blockchain.Chain)}");
                    }
                    break;
                case "removelock":
                    if (args.Length < 2) { Console.WriteLine("Usage: removelock <blockid>"); return; }
                    blockchain.RemoveLock(int.Parse(args[1]));
                    if (peerNetwork != null)
                    {
                        await peerNetwork!.NotifyPeersAsync($"CHAIN:{JsonConvert.SerializeObject(blockchain.Chain)}");
                    }
                    break;
                case "query":
                    if (args.Length < 2) { Console.WriteLine("Usage: query <blockid>"); return; }
                    blockchain.QueryBlock(int.Parse(args[1]));
                    break;
                default:
                    Console.WriteLine("Unknown action. Type 'help' for commands.");
                    break;
            }
        }

        // Updated functions to handle lock conditions
        public static async Task BuyFromReserve(string buyer, decimal amount)
        {
            try
            {
                var token = await PayPalAPI.AuthenticateAsync(environment);
                if (string.IsNullOrEmpty(token)) { Console.WriteLine("Authentication failed."); return; }
                // Proceed
                var (paypalId, url) = await PayPalAPI.CreateOrderAsync(token, amount, "USD", environment, domain + "/payments/paypalOrderComplete", domain);
                Console.WriteLine($"Redirect user to: {url}");
                var status = await PayPalAPI.CaptureOrderAsync(token, paypalId, environment);
                if (status == "COMPLETED")
                {
                    var tx = new Transaction(Blockchain.ReserveAccount, buyer, amount) { PayPalOrderId = paypalId };
                    blockchain.AddTransaction(tx);
                    blockchain.MinePendingTransactions($"PeerRef:{peerNetwork}");
                    var newBlock = blockchain.Chain.Last();
                    if (newBlock.Lock)
                    {
                        if (peerNetwork != null)
                        {
                            await peerNetwork!.NotifyPeersAsync($"CHAIN:{JsonConvert.SerializeObject(blockchain.Chain)}", newBlock); // Null check
                        } else 
                        {
                            Console.WriteLine("Error: PeerNetwork returns Null Response while adding locked block for reserve.");
                        }
                    }
                    else
                    {
                        if (peerNetwork != null)
                        {
                            await peerNetwork!.NotifyPeersAsync($"CHAIN:{JsonConvert.SerializeObject(blockchain.Chain)}"); // Null check
                        } else 
                        {
                            Console.WriteLine("Error: PeerNetwork returns Null Response while adding unlock block for reserve.");
                        }
                    }
                    Console.WriteLine($"Buy from Reserve: {buyer} bought {amount} USD. Transaction added.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: in BuyFromReserve: {ex.Message}");
            }
        }

        public static async Task SellToReserve(string seller, decimal amount)
        {
            var tx = new Transaction(seller, Blockchain.ReserveAccount, amount);
            blockchain.AddTransaction(tx);
            blockchain.MinePendingTransactions($"PeerRef:{peerNetwork}");
            var newBlock = blockchain.Chain.Last();
            if (newBlock.Lock)
            {
                if (peerNetwork != null)
                {
                    await peerNetwork!.NotifyPeersAsync($"CHAIN:{JsonConvert.SerializeObject(blockchain.Chain)}", newBlock); // Null check
                } else 
                { Console.WriteLine("Error: PeerNetwork returns Null Response while adding locked block for reserve."); }
            }
            else
            {
                if (peerNetwork != null)
                {
                    await peerNetwork!.NotifyPeersAsync($"CHAIN:{JsonConvert.SerializeObject(blockchain.Chain)}"); // Null check
                } else 
                { Console.WriteLine("Error: PeerNetwork returns Null Response while adding unlocked block for reserve."); }
            }
            Console.WriteLine($"Sell to Reserve: {seller} sold {amount} USD. Transaction added.");
        }

        public static void PeerToPeerTransfer(string sender, string receiver, decimal amount)
        {
            var tx = new Transaction(sender, receiver, amount);
            blockchain.AddTransaction(tx);
            Console.WriteLine($"P2P Transfer: {sender} sent {amount} USD to {receiver}. Transaction added.");
        }

        public static void RegisterUser(string user, string ip, string peersHash, string privateKey, string publicKey)
        {
            try
            {
                // Attempt to use provided keys
                var regBlock = new RegistrationBlock(user, ip, peersHash, privateKey, publicKey);
                blockchain.RegisterUser(user, ip, peersHash, privateKey, publicKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with provided keys: {ex.Message}. Generating new RSA keys...");
                
                // Fallback: Generate new RSA keys (2048-bit)
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    string? newPrivateKey = rsa.ToXmlString(true); // Nullable
                    string? newPublicKey = rsa.ToXmlString(false); // Nullable
                    
                    if (string.IsNullOrEmpty(newPrivateKey) || string.IsNullOrEmpty(newPublicKey))
                    {
                        Console.WriteLine("Failed to generate RSA keys.");
                        return;
                    }
                    
                    Console.WriteLine("Generated Private Key (use securely):");
                    Console.WriteLine(newPrivateKey);
                    Console.WriteLine("Generated Public Key:");
                    Console.WriteLine(newPublicKey);
                    
                    // Register with generated keys
                    var regBlock = new RegistrationBlock(user, ip, peersHash, newPrivateKey, newPublicKey);
                    blockchain.RegisterUser(user, ip, peersHash, newPrivateKey, newPublicKey);
                }
            }
        }

    }
}
