using Archipelago.MultiClient.Net.Models;
using BepInEx;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BluePrinceArchipelago.Utils
{
    public static class State
    {
        public static string PluginPath => Paths.PluginPath;
        public static string ModFolder => Path.Combine(PluginPath, Plugin.PluginName);

        public const string SessionFolder = "SessionData";

        public const string RecievedItemsFile = "RecievedItems.json";
        public const string SentLocationsFile = "SentLocations.json";
        public const string SessionDataFile = "SessionData.json";
        public const string ServerDetailsFile = "ServerDetails.json";
        public const string ServerOptionsFile = "ServerOptions.json";
        public const string TrunkCountsFile = "TrunkCounts.json";
        public const string LocationDictFile = "LocationDict.json";
        public const string DeathLinkTotalsFile = "DeathLinkTotals.json";

        public static string RecievedItemsPath => Path.Combine(ModFolder, SessionFolder, RecievedItemsFile);
        public static string SentLocationsPath => Path.Combine(ModFolder, SessionFolder, SentLocationsFile);
        public static string SessionDataPath => Path.Combine(ModFolder, SessionFolder, SessionDataFile);
        public static string ServerDetailsPath => Path.Combine(ModFolder, SessionFolder, ServerDetailsFile);
        public static string ServerOptionsPath => Path.Combine(ModFolder, SessionFolder, ServerOptionsFile);
        public static string TrunkCountsPath => Path.Combine(ModFolder, SessionFolder, TrunkCountsFile);
        public static string LocationDictPath => Path.Combine(ModFolder, SessionFolder, LocationDictFile);
        public static string DeathLinkTotalsPath => Path.Combine(ModFolder, SessionFolder, DeathLinkTotalsFile);

        public static void Initialize()
        {
            if (!Directory.Exists(Path.Combine(ModFolder, SessionFolder))) {
                Directory.CreateDirectory(Path.Combine(ModFolder, SessionFolder));
            }
            InitializeSentLocations();
            InitializeSessionData();
            InitializeServerDetails();
            InitializeLocationDict();
        }

        public static void UpdateAll() {
            UpdateLocations(ArchipelagoClient.ServerData.CheckedLocations);
            SessionData session = new SessionData();
            session.SaveSlot = ModInstance.SaveSlot;
            session.Seed = ArchipelagoClient.ServerData.Seed;
            UpdateSession(session);
        }

        public static void UpdateLocations(List<long> data) { 
            using (var writer = new StreamWriter(SentLocationsPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(data));
                writer.Flush();
            }
        }
        public static void UpdateItems(List<string> data)
        {
            using (var writer = new StreamWriter(RecievedItemsPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(data));
                writer.Flush();
            }
        }
        public static void UpdateServerDetails(List<string> data)
        {
            ConnectionData connData = new ConnectionData();
            connData.Uri = data[0];
            connData.SlotName = data[1];
            connData.Password = data[2];
            UpdateServerDetails(connData);
        }
        public static void UpdateServerDetails(ConnectionData data)
        {
            using (var writer = new StreamWriter(ServerDetailsPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(data));
                writer.Flush();
            }
        }
        public static void UpdateSession(SessionData data)
        {
            using (var writer = new StreamWriter(SessionDataPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(data));
                writer.Flush();
            }
        }
        public static void UpdateTrunkCounts()
        {
            using (var writer = new StreamWriter(TrunkCountsPath))
            {
                writer.Write(JsonConvert.SerializeObject(ModInstance.TrunkManager.TrunkCounts));
                writer.Flush();
            }
        }
        public static void UpdateLocationDict() {
            using (var writer = new StreamWriter(LocationDictPath))
            {
                writer.Write(JsonConvert.SerializeObject(ArchipelagoClient.ServerData.LocationDict));
                writer.Flush();
            }
        }

        public static void UpdateDeathLinkData() {
            using (var writer = new StreamWriter(DeathLinkTotalsPath, false))
            {
                DeathLinkData data = new DeathLinkData();
                data.DeathLinkEnabled = DeathLinkHandler.deathLinkEnabled;
                data.DeathLinkCount = DeathLinkHandler.DeathLinkCount;
                data.TotalDeathLinksSent = DeathLinkHandler.TotalDeathLinksSent;
                data.BlockedDeaths = DeathLinkHandler.BlockedDeathLinks;
                writer.Write(JsonConvert.SerializeObject(data));
                writer.Flush();
            }
        }
        private static void InitializeServerDetails()
        {
            if (File.Exists(ServerDetailsPath))
            {
                string jsonData = "";
                using (var reader = new StreamReader(ServerDetailsPath))
                {
                    jsonData = reader.ReadToEnd();
                }
                if (jsonData.Trim().Length > 0)
                {
                    try
                    {
                        ConnectionData data = JsonConvert.DeserializeObject<ConnectionData>(jsonData);
                        ArchipelagoClient.ServerData.Uri = data.Uri;
                        ArchipelagoClient.ServerData.SlotName = data.SlotName;
                        ArchipelagoClient.ServerData.Password = data.Password;
                    }
                    catch (Exception ex)
                    {
                        Logging.LogWarning($"Error loading ServerData: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(ServerDetailsPath, false))
                {
                    ConnectionData data = new ConnectionData();
                    data.Uri = ArchipelagoClient.ServerData.Uri;
                    data.SlotName = ArchipelagoClient.ServerData.SlotName;
                    data.Password = ArchipelagoClient.ServerData.Password;
                    writer.Write(JsonConvert.SerializeObject(data));
                    writer.Flush();
                }
            }
        }

        private static void InitializeSessionData()
        {
            if (File.Exists(SessionDataPath))
            {
                string jsonData = "";
                using (var reader = new StreamReader(SessionDataPath))
                {
                    jsonData = reader.ReadToEnd();
                }
                if (jsonData.Trim().Length > 0)
                {
                    try
                    {
                        SessionData data = JsonConvert.DeserializeObject<SessionData>(jsonData);
                        ArchipelagoClient.ServerData.Seed = data.Seed;
                        ModInstance.SaveSlot = data.SaveSlot;
                    }
                    catch (Exception ex)
                    {
                        Logging.LogWarning($"Error loading Session Data: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(SessionDataPath, false))
                {
                    SessionData defaultData = new SessionData();
                    defaultData.Seed = "";
                    defaultData.SaveSlot = 5;
                    writer.Write(JsonConvert.SerializeObject(defaultData));
                    writer.Flush();
                }
            }
        }

        private static void InitializeSentLocations()
        {
            if (File.Exists(SentLocationsPath))
            {
                string jsonData = "";
                using (var reader = new StreamReader(SentLocationsPath))
                {
                    jsonData = reader.ReadToEnd();
                }
                if (jsonData.Trim().Length > 0)
                {
                    try
                    {
                        ArchipelagoClient.ServerData.CheckedLocations = JsonConvert.DeserializeObject<List<long>>(jsonData);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogWarning($"Error loading Sent Locations: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(SentLocationsPath, false))
                {
                    writer.Write(JsonConvert.SerializeObject(new List<long>()));
                    writer.Flush();
                }
            }
        }

        public static void InitializeReceivedItems() {
            if (File.Exists(RecievedItemsPath))
            {
                string jsonData = "";
                using (var reader = new StreamReader(RecievedItemsPath))
                {
                    jsonData = reader.ReadToEnd();
                }
                if (jsonData.Trim().Length > 0)
                {
                    try
                    {
                        ArchipelagoClient.ServerData.ReceivedItems = JsonConvert.DeserializeObject<List<string>>(jsonData);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogWarning($"Error loading Received Items: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(RecievedItemsPath, false))
                {
                    writer.Write(JsonConvert.SerializeObject(new List<string>()));
                    writer.Flush();
                }
            }
        }


        public static void InitializeTrunkCounts()
        {
            if (File.Exists(TrunkCountsPath))
            {
                string jsonData = "";
                using (var reader = new StreamReader(TrunkCountsPath))
                {
                    jsonData = reader.ReadToEnd();
                }
                if (jsonData.Trim().Length > 0)
                {
                    try
                    {
                        ModInstance.TrunkManager.TrunkCounts = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonData);
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Error loading Received items: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(TrunkCountsPath, false))
                {
                    writer.Write(JsonConvert.SerializeObject(new Dictionary<string, int>()));
                    writer.Flush();
                }
            }
        }
        public static void InitializeLocationDict()
        {
            if (File.Exists(LocationDictPath))
            {
                string jsonData = "";
                using (var reader = new StreamReader(LocationDictPath))
                {
                    jsonData = reader.ReadToEnd();
                }
                if (jsonData.Trim().Length > 0)
                {
                    try
                    {
                        ArchipelagoClient.ServerData.LocationDict = JsonConvert.DeserializeObject<Dictionary<long, string>>(jsonData);
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Error loading Received items: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(LocationDictPath, false))
                {
                    writer.Write(JsonConvert.SerializeObject(new Dictionary<long, string>()));
                    writer.Flush();
                }
            }
        }
        public static void InitializeDeathLinkTotals()
        {
            if (File.Exists(DeathLinkTotalsPath))
            {
                string jsonData = "";
                using (var reader = new StreamReader(DeathLinkTotalsPath))
                {
                    jsonData = reader.ReadToEnd();
                }
                if (jsonData.Trim().Length > 0)
                {
                    try
                    {
                        DeathLinkData data = JsonConvert.DeserializeObject<DeathLinkData>(jsonData);
                        DeathLinkHandler.deathLinkEnabled = data.DeathLinkEnabled;
                        DeathLinkHandler.DeathLinkCount = data.DeathLinkCount;
                        DeathLinkHandler.TotalDeathLinksSent = data.TotalDeathLinksSent;
                        DeathLinkHandler.BlockedDeathLinks = data.BlockedDeaths;

                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Error loading Received items: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(DeathLinkTotalsPath, false))
                {
                    DeathLinkData data = new DeathLinkData();
                    data.DeathLinkEnabled = false;
                    data.DeathLinkCount = 0;
                    data.TotalDeathLinksSent = 0;
                    data.BlockedDeaths = 0;
                    writer.Write(JsonConvert.SerializeObject(data));
                    writer.Flush();
                }
            }
        }


        public static void Reset() {
            // Received Items
            using (var writer = new StreamWriter(RecievedItemsPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(new List<string>()));
                writer.Flush();
            }
            // Sent Locations
            using (var writer = new StreamWriter(SentLocationsPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(new List<long>()));
                writer.Flush();
            }
            // Session Data
            using (var writer = new StreamWriter(SessionDataPath, false))
            {
                SessionData defaultData = new SessionData();
                defaultData.Seed = "";
                defaultData.SaveSlot = 5;
                writer.Write(JsonConvert.SerializeObject(defaultData));
                writer.Flush();
            }
            using (var writer = new StreamWriter(TrunkCountsPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(new Dictionary<string, int>()));
                writer.Flush();
            }

            //Location Dict
            using (var writer = new StreamWriter(LocationDictPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(new Dictionary<long, string>()));
                writer.Flush();
            }

            //DeathLinkData
            using (var writer = new StreamWriter(DeathLinkTotalsPath, false))
            {
                DeathLinkData data = new DeathLinkData();
                data.DeathLinkEnabled = false;
                data.DeathLinkCount = 0;
                data.TotalDeathLinksSent = 0;
                data.BlockedDeaths = 0;
                writer.Write(JsonConvert.SerializeObject(data));
                writer.Flush();
            }
            //Don't reset the connection details since they might be useful.
        }
    }
}