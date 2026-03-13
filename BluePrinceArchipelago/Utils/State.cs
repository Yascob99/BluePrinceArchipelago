using AsmResolver.PE.DotNet.ReadyToRun;
using BepInEx;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using static Il2CppSystem.Globalization.CultureInfo;

namespace BluePrinceArchipelago.Utils
{
    public static class State
    {
        public static string PluginPath => Paths.PluginPath;
        public static string ModFolder => Path.Combine(PluginPath, Plugin.PluginName);

        public const string RecievedItemsFile = "RecievedItems.json";
        public const string SentLocationsFile = "SentLocations.json";
        public const string SessionDataFile = "SessionData.json";
        public const string ServerDetailsFile = "ServerDetails.json";
        public const string ServerOptionsFile = "ServerOptions.json";
        public const string TrunkCountsFile = "TrunkCounts.json";

        public static string RecievedItemsPath => Path.Combine(ModFolder, RecievedItemsFile);
        public static string SentLocationsPath => Path.Combine(ModFolder, SentLocationsFile);
        public static string SessionDataPath => Path.Combine(ModFolder, SessionDataFile);
        public static string ServerDetailsPath => Path.Combine(ModFolder, ServerDetailsFile);
        public static string ServerOptionsPath => Path.Combine(ModFolder, ServerOptionsFile);
        public static string TrunkCountsPath => Path.Combine(ModFolder, TrunkCountsFile);

        public static void Initialize()
        {
            InitializeReceivedItems();
            InitializeSentLocations();
            InitializeSessionData();
            InitializeServerDetails();
            InitializeServerOptions();
        }

        public static void UpdateAll() {
            UpdateItems(ArchipelagoClient.ServerData.ReceivedItems);
            UpdateLocations(ArchipelagoClient.ServerData.CheckedLocations);
            SessionData session = new SessionData();
            session.SaveSlot = ModInstance.SaveSlot;
            session.Seed = ArchipelagoClient.ServerData.Seed;
            UpdateSession(session);
        }

        public static void UpdateLocations(List<long> data) { 
            using (var writer = new StreamWriter(SentLocationsPath))
            {
                writer.Write(JsonConvert.SerializeObject(data));
                writer.Flush();
            }
        }
        public static void UpdateItems(List<string> data)
        {
            using (var writer = new StreamWriter(RecievedItemsPath))
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
            using (var writer = new StreamWriter(ServerDetailsPath))
            {
                writer.Write(JsonConvert.SerializeObject(data));
                writer.Flush();
            }
        }
        public static void UpdateSession(SessionData data)
        {
            using (var writer = new StreamWriter(SessionDataPath))
            {
                writer.Write(JsonConvert.SerializeObject(data));
                writer.Flush();
            }
        }
        internal static void UpdateTrunkCounts()
        {
            using (var writer = new StreamWriter(TrunkCountsPath))
            {
                writer.Write(JsonConvert.SerializeObject(ModInstance.TrunkManager.TrunkCounts));
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
                        Logging.Log($"Error loading ServerData: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(ServerDetailsPath))
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
                        Logging.Log($"Error loading Received items: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(SessionDataPath))
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
                        Logging.Log($"Error loading Received items: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(SentLocationsPath))
                {
                    writer.Write(JsonConvert.SerializeObject(new List<long>()));
                    writer.Flush();
                }
            }
        }

        private static void InitializeReceivedItems() {
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
                        Logging.Log($"Error loading Received items: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(RecievedItemsPath))
                {
                    writer.Write(JsonConvert.SerializeObject(new List<string>()));
                    writer.Flush();
                }
            }
        }
        private static void InitializeServerOptions()
        {
            if (File.Exists(ServerOptionsPath))
            {
                string jsonData = "";
                using (var reader = new StreamReader(ServerOptionsPath))
                {
                    jsonData = reader.ReadToEnd();
                }
                if (jsonData.Trim().Length > 0)
                {
                    try
                    {
                        ArchipelagoClient.ServerData.Options = JsonConvert.DeserializeObject<SlotData>(jsonData);
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Error loading Received items: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(ServerOptionsPath))
                {
                    writer.Write(JsonConvert.SerializeObject(new SlotData()));
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
                using (var writer = new StreamWriter(TrunkCountsPath))
                {
                    writer.Write(JsonConvert.SerializeObject(new Dictionary<string, int>()));
                    writer.Flush();
                }
            }
        }
        public static void Reset() {
            using (var writer = new StreamWriter(RecievedItemsPath))
            {
                writer.Write(JsonConvert.SerializeObject(new List<string>()));
                writer.Flush();
            }
            using (var writer = new StreamWriter(SentLocationsPath))
            {
                writer.Write(JsonConvert.SerializeObject(new List<long>()));
                writer.Flush();
            }
            using (var writer = new StreamWriter(SessionDataPath))
            {
                SessionData defaultData = new SessionData();
                defaultData.Seed = "";
                defaultData.SaveSlot = 5;
                writer.Write(JsonConvert.SerializeObject(defaultData));
                writer.Flush();
            }
            using (var writer = new StreamWriter(ServerOptionsPath))
            {
                writer.Write(JsonConvert.SerializeObject(new SlotData()));
                writer.Flush();
            }
            using (var writer = new StreamWriter(TrunkCountsPath))
            {
                writer.Write(JsonConvert.SerializeObject(new Dictionary<string, int>()));
                writer.Flush();
            }
            //Don't reset the connection details since they might be useful.
        }
    }
}