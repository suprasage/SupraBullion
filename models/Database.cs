using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataBaseApp
{
    public enum QueryType { CREATE, SELECT, INSERT, UPDATE, DELETE }

    public class Query
    {
        public string QueryText { get; set; }
        public QueryType Type { get; set; }

        public Query(string text, QueryType qtype)
        {
            QueryText = text;
            Type = qtype;
        }
    }

    public class Database
    {
        public Query UserQuery { get; set; }
        public string DatabasePath { get; set; } = "./database";

        public Database(Query qry)
        {
            UserQuery = qry;
            Directory.CreateDirectory(DatabasePath);
        }

        public string StringParser(Query qry)
        {
            string[] tokens = qry.QueryText.Split(new[] { ' ', ',', '(', ')', '=', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) return JsonConvert.SerializeObject(new { status = "error", message = "Invalid query" });

            string command = tokens[0].ToUpper();
            switch (command)
            {
                case "CREATE":
                    qry.Type = QueryType.CREATE;
                    return DbCreateParse(tokens);
                case "SELECT":
                    qry.Type = QueryType.SELECT;
                    return DbSelectParse(tokens);
                case "INSERT":
                    qry.Type = QueryType.INSERT;
                    return DbInsertParse(tokens);
                case "UPDATE":
                    qry.Type = QueryType.UPDATE;
                    return DbUpdateParse(tokens);
                case "DELETE":
                    qry.Type = QueryType.DELETE;
                    return DbDeleteParse(tokens);
                default:
                    return JsonConvert.SerializeObject(new { status = "error", message = "Unsupported query type" });
            }
        }

        public string DbCreateParse(string[] tokens)
        {
            if (tokens.Length < 4 || tokens[1].ToUpper() != "TABLE") return JsonConvert.SerializeObject(new { status = "error", message = "Invalid CREATE syntax" });
            string tableName = tokens[2];
            string schemaStr = string.Join(" ", tokens.Skip(3));
            // Create subdirectory and schema.json
            string tablePath = Path.Combine(DatabasePath, tableName);
            Directory.CreateDirectory(tablePath);
            var schema = new JObject();
            // Basic parsing: assume key-value pairs
            var pairs = schemaStr.Split(',');
            foreach (var pair in pairs)
            {
                var kv = pair.Trim().Split(' ');
                if (kv.Length == 2) schema[kv[0]] = kv[1];
            }
            File.WriteAllText(Path.Combine(tablePath, "schema.json"), JsonConvert.SerializeObject(schema, Formatting.Indented));
            return JsonConvert.SerializeObject(new { status = "success", message = $"Table {tableName} created" });
        }

        public string DbSelectParse(string[] tokens)
        {
            if (tokens.Length < 4 || tokens[1] != "*" || tokens[2].ToUpper() != "FROM") return JsonConvert.SerializeObject(new { status = "error", message = "Invalid SELECT syntax" });
            string tableName = tokens[3];
            string whereClause = tokens.Length > 4 ? string.Join(" ", tokens.Skip(4)) : "";
            return DbSelectExec(tableName, whereClause);
        }

        public string DbInsertParse(string[] tokens)
        {
            // Simplified: INSERT INTO table VALUES (val1, val2)
            if (tokens.Length < 5 || tokens[1].ToUpper() != "INTO" || tokens[3].ToUpper() != "VALUES") return JsonConvert.SerializeObject(new { status = "error", message = "Invalid INSERT syntax" });
            string tableName = tokens[2];
            string valuesStr = string.Join(" ", tokens.Skip(4)).Trim('(', ')');
            return DbInsertExec(tableName, valuesStr);
        }

        public string DbUpdateParse(string[] tokens)
        {
            // Simplified: UPDATE table SET col=val WHERE condition
            if (tokens.Length < 6 || tokens[2].ToUpper() != "SET") return JsonConvert.SerializeObject(new { status = "error", message = "Invalid UPDATE syntax" });
            string tableName = tokens[1];
            string setClause = string.Join(" ", tokens.Skip(3).TakeWhile(t => t.ToUpper() != "WHERE"));
            string whereClause = tokens.Contains("WHERE") ? string.Join(" ", tokens.Skip(Array.IndexOf(tokens, "WHERE") + 1)) : "";
            return DbUpdateExec(tableName, setClause, whereClause);
        }

        public string DbDeleteParse(string[] tokens)
        {
            // Simplified: DELETE FROM table WHERE condition
            if (tokens.Length < 4 || tokens[1].ToUpper() != "FROM") return JsonConvert.SerializeObject(new { status = "error", message = "Invalid DELETE syntax" });
            string tableName = tokens[2];
            string whereClause = string.Join(" ", tokens.Skip(3));
            return DbDeleteExec(tableName, whereClause);
        }

        public string DbSelectExec(string tableName, string whereClause)
        {
            string tablePath = Path.Combine(DatabasePath, tableName);
            if (!Directory.Exists(tablePath)) return JsonConvert.SerializeObject(new { status = "error", message = "Table not found" });

            var results = new List<JObject>();
            var files = Directory.GetFiles(tablePath, "*.json").Where(f => !f.EndsWith("schema.json"));
            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var obj = JsonConvert.DeserializeObject<JObject>(json);
                if (obj != null && MatchesWhere(obj, whereClause))
                {
                    results.Add(obj);
                }
            }
            return JsonConvert.SerializeObject(new { status = "success", data = results }, Formatting.Indented);
        }

        public string DbInsertExec(string tableName, string valuesStr)
        {
            string tablePath = Path.Combine(DatabasePath, tableName);
            if (!Directory.Exists(tablePath)) return JsonConvert.SerializeObject(new { status = "error", message = "Table not found" });

            var values = valuesStr.Split(',');
            var obj = new JObject();
            var schema = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path.Combine(tablePath, "schema.json")));
            int i = 0;
            if (schema != null)
			{
	            foreach (var prop in schema.Properties())
	            {
	                if (i < values.Length) obj[prop.Name] = values[i].Trim();
	                i++;
	            }
	        } else 
	        {
           		Console.WriteLine("Error: schema variable null.");
           	}
           string fileName = $"record_{Guid.NewGuid()}.json";
           File.WriteAllText(Path.Combine(tablePath, fileName), JsonConvert.SerializeObject(obj, Formatting.Indented));
           return JsonConvert.SerializeObject(new { status = "success", message = "Inserted" });
        }

        public string DbUpdateExec(string tableName, string setClause, string whereClause)
        {
            string tablePath = Path.Combine(DatabasePath, tableName);
            if (!Directory.Exists(tablePath)) return JsonConvert.SerializeObject(new { status = "error", message = "Table not found" });

            var files = Directory.GetFiles(tablePath, "*.json").Where(f => !f.EndsWith("schema.json"));
            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var obj = JsonConvert.DeserializeObject<JObject>(json);
                if (obj != null && MatchesWhere(obj, whereClause))
                {
                    // Apply SET
                    var sets = setClause.Split(',');
                    foreach (var set in sets)
                    {
                        var kv = set.Trim().Split('=');
                        if (kv.Length == 2) obj[kv[0].Trim()] = kv[1].Trim().Trim('"');
                    }
                    File.WriteAllText(file, JsonConvert.SerializeObject(obj, Formatting.Indented));
                }
            }
            return JsonConvert.SerializeObject(new { status = "success", message = "Updated" });
        }

        public string DbDeleteExec(string tableName, string whereClause)
        {
            string tablePath = Path.Combine(DatabasePath, tableName);
            if (!Directory.Exists(tablePath)) return JsonConvert.SerializeObject(new { status = "error", message = "Table not found" });

            var files = Directory.GetFiles(tablePath, "*.json").Where(f => !f.EndsWith("schema.json"));
            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var obj = JsonConvert.DeserializeObject<JObject>(json);
                if (obj != null && MatchesWhere(obj, whereClause))
                {
                    File.Delete(file);
                }
            }
            return JsonConvert.SerializeObject(new { status = "success", message = "Deleted" });
        }

        private bool MatchesWhere(JObject obj, string whereClause)
        {
            if (string.IsNullOrEmpty(whereClause)) return true;
            var parts = whereClause.Split('=');
            if (parts.Length == 2)
            {
                string key = parts[0].Trim();
                string value = parts[1].Trim().Trim('"');
                return obj[key]?.ToString() == value;
            }
            return false;
        }
    }
}