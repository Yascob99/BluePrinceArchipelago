using Archipelago.MultiClient.Net.Models;
using BepInEx;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using static BluePrinceArchipelago.Archipelago.ItemQueue;

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
        public const string ItemQueueFile = "ItemQueue.json";
        public const string LocationQueueFile = "LocationQueue.json";
        public const string ItemsByDayFile = "ItemsByDay.json";

        public static string RecievedItemsPath => Path.Combine(ModFolder, SessionFolder, RecievedItemsFile);
        public static string SentLocationsPath => Path.Combine(ModFolder, SessionFolder, SentLocationsFile);
        public static string SessionDataPath => Path.Combine(ModFolder, SessionFolder, SessionDataFile);
        public static string ServerDetailsPath => Path.Combine(ModFolder, SessionFolder, ServerDetailsFile);
        public static string ServerOptionsPath => Path.Combine(ModFolder, SessionFolder, ServerOptionsFile);
        public static string TrunkCountsPath => Path.Combine(ModFolder, SessionFolder, TrunkCountsFile);
        public static string ItemQueuePath => Path.Combine(ModFolder, SessionFolder, ItemQueueFile);
        public static string LocationQueuePath => Path.Combine(ModFolder, SessionFolder, LocationQueueFile);
        public static string ItemsByDayPath => Path.Combine(ModFolder, SessionFolder, ItemsByDayFile);

        public static List<ItemInfo> TodaysItems = new();

        public static List<ItemInfo> TempTodaysItems = new();

        public static bool TodayLoaded = false;

        public static int CurrentDayNum = 1;

        public static void Initialize()
        {
            if (!Directory.Exists(Path.Combine(ModFolder, SessionFolder))) {
                Directory.CreateDirectory(Path.Combine(ModFolder, SessionFolder));
            }
            InitializeReceivedItems();
            InitializeSentLocations();
            InitializeSessionData();
            InitializeServerDetails();
            //InitializeItemQueue();
            //InitializeLocationQueue();
        }

        public static void UpdateAll() {
            UpdateItems(ArchipelagoClient.ServerData.ReceivedItems);
            UpdateLocations(ArchipelagoClient.ServerData.CheckedLocations);
            UpdateItemsByDay(ArchipelagoClient.ServerData.ItemsByDay);
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
        public static void UpdateItemQueue(List<ItemInfo> itemQueue) {
            using (var writer = new StreamWriter(ItemQueuePath, false))
            {
                writer.Write(JsonConvert.SerializeObject(itemQueue));
                writer.Flush();
            }
        }
        public static void UpdateLocationQueue(List<string> locationQueue)
        {
            using (var writer = new StreamWriter(LocationQueuePath, false))
            {
                writer.Write(JsonConvert.SerializeObject(locationQueue));
                writer.Flush();
            }
        }

        public static void UpdateItemsByDay(ItemInfo item) {
            ArchipelagoClient.ServerData.ItemsByDay[CurrentDayNum] = TodaysItems;
            UpdateItemsByDay(ArchipelagoClient.ServerData.ItemsByDay);
        }
        private static void UpdateItemsByDay(Dictionary<int, List<ItemInfo>> itemsByDay) {
            using (var writer = new StreamWriter(ItemsByDayPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(itemsByDay));
                writer.Flush();
            }
        }
        public static void FirstLoad() {
            // If it is a reconnect from a crash
            if (CurrentDayNum > 0 && ArchipelagoClient.Reconnected) {
                InitializeItemsByDay();
                int LatestDay = ArchipelagoClient.ServerData.ItemsByDay.HighestKey<int, List<ItemInfo>>();
                if (LatestDay > 0) {

                    // If the current day is the Latest day in data, there was likely a crash mid day and any recieved items need to be re-given (Except for rooms);
                    if (CurrentDayNum == LatestDay) {
                        // Re-recieve all items for the current day
                        foreach (ItemInfo item in ArchipelagoClient.ServerData.ItemsByDay[LatestDay]) {
                            if (!ModInstance.QueueManager.ReceiveItem(item, false))
                            {
                                ModInstance.QueueManager.AddItemToQueue(item);
                            }
                        }
                    }
                    else if (LatestDay > CurrentDayNum) {
                        Logging.LogWarning($"Current Day {CurrentDayNum} does not match latest day in data {LatestDay}. It's very likely an incorrect file was loaded.");
                    }
                }
            }
            // If it's not a reconnect, nothing needs to be done.
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
        private static void InitializeItemQueue() 
        {
            if (File.Exists(ItemQueuePath))
            {
                string jsonData = "";
                using (var reader = new StreamReader(ItemQueuePath))
                {
                    jsonData = reader.ReadToEnd();
                }
                if (jsonData.Trim().Length > 0)
                {
                    try
                    {
                        ModInstance.QueueManager.SetItemQueue(JsonConvert.DeserializeObject<List<ItemInfo>>(jsonData));
                    }
                    catch (Exception ex)
                    {
                        Logging.LogWarning($"Error loading Item Queue: \n{ex.Message}");
                    }
                }
            }
            else {
                using (var writer = new StreamWriter(ItemQueuePath, false))
                {
                    writer.Write(JsonConvert.SerializeObject(new List<ItemInfo>()));
                    writer.Flush();
                }
            }
        }
        private static void InitializeLocationQueue() {
            if (File.Exists(LocationQueuePath))
            {
                string jsonData = "";
                using (var reader = new StreamReader(LocationQueuePath))
                {
                    jsonData = reader.ReadToEnd();
                }
                if (jsonData.Trim().Length > 0)
                {
                    try
                    {
                        ModInstance.QueueManager.SetLocationQueue(JsonConvert.DeserializeObject<List<string>>(jsonData));
                    }
                    catch (Exception ex)
                    {
                        Logging.LogWarning($"Error loading Location Queue: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(LocationQueuePath, false))
                {
                    writer.Write(JsonConvert.SerializeObject(new List<String>()));
                    writer.Flush();
                }
            }
        }

        private static void InitializeItemsByDay() {
            if (File.Exists(ItemsByDayPath))
            {
                string jsonData = "";
                using (var reader = new StreamReader(ItemsByDayPath))
                {
                    jsonData = reader.ReadToEnd();
                }
                if (jsonData.Trim().Length > 0)
                {
                    try
                    {
                        ArchipelagoClient.ServerData.ItemsByDay = JsonConvert.DeserializeObject<Dictionary<int,List<ItemInfo>>>(jsonData);
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Error loading Items By Day data: \n{ex.Message}");
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(ItemsByDayPath, false))
                {
                    Dictionary<int, List<ItemInfo>> tempDict = new();
                    tempDict[1] = new List<ItemInfo>(); // Blank Day 1
                    writer.Write(JsonConvert.SerializeObject(tempDict));
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
                using (var writer = new StreamWriter(ServerOptionsPath, false))
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
                using (var writer = new StreamWriter(TrunkCountsPath, false))
                {
                    writer.Write(JsonConvert.SerializeObject(new Dictionary<string, int>()));
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
            // Server Options
            using (var writer = new StreamWriter(ServerOptionsPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(new SlotData()));
                writer.Flush();
            }
            // Trunk Counts
            using (var writer = new StreamWriter(TrunkCountsPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(new Dictionary<string, int>()));
                writer.Flush();
            }
            // Item Queue
            using (var writer = new StreamWriter(ItemQueuePath, false))
            {
                writer.Write(JsonConvert.SerializeObject(new List<ItemInfo>()));
                writer.Flush();
            }
            // Location Queue
            using (var writer = new StreamWriter(LocationQueuePath, false))
            {
                writer.Write(JsonConvert.SerializeObject(new List<String>()));
                writer.Flush();
            }
            // 
            using (var writer = new StreamWriter(ItemsByDayPath, false))
            {
                Dictionary<int, List<ItemInfo>> tempDict = new();
                tempDict[1] = new List<ItemInfo>(); // Blank Day 1
                writer.Write(JsonConvert.SerializeObject(tempDict));
                writer.Flush();
            }
            //Don't reset the connection details since they might be useful.
        }
    }
}