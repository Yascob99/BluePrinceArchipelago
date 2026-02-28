using BepInEx;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BluePrinceArchipelago.Utils
{
    public class ObjectStore {
        public string Name = "";
        public string SerializedObject = "";
        public Type SerializedObjectType;
        public ObjectStore(string name, string serializedObject, Type type)
        {
            Name = name;
            SerializedObject = serializedObject;
            SerializedObjectType = type;
        }
    }
    public class StateStore {
        public Dictionary<string, ObjectStore> Objects = [];

        public StateStore() { 
        }
        //Adds an indexer to allow writing to the dictionary.
        public ObjectStore this[string key] { 
            get { return Objects[key]; }
            set {
                if (value.GetType() == typeof(ObjectStore)) {
                    Objects[key] = value;
                }
            }
        }
        public bool ContainsKey(string key) { 
            return Objects.ContainsKey(key);
        }
    }
    public static class State
    {
        public static string PluginPath => Paths.PluginPath;
        public static string ModFolder => Path.Combine(PluginPath, Plugin.PluginName);

        public const string StateFile = "State.json";

        public static string StateFilePath => Path.Combine(ModFolder, StateFile);

        private static StateStore _Data = new();

        public static void Initialize(){
            _Data = LoadData(); // load data from existing state on start.
            ModInstance.Instance.ModEventHandler.QueueEvent += OnQueueEvent;
        }
        public static void Update(string key, object data) {
            _Data[key] = new ObjectStore(key, JsonConvert.SerializeObject(data), data.GetType());
            WriteToState();
        }
        public static void Update(string key, object data, Type type)
        {
            _Data[key] = new ObjectStore(key, JsonConvert.SerializeObject(data), type);
            WriteToState();
        }
        public static void UpdateNoSave(string key, object data)
        {
            _Data[key] = new ObjectStore(key, JsonConvert.SerializeObject(data), data.GetType());
        }
        public static void UpdateNoSave(string key, object data, Type type) {
            _Data[key] = new ObjectStore(key, JsonConvert.SerializeObject(data), type);
        }
        private static void OnQueueEvent(string senderName, ItemQueueEventArgs e) {
            Update(senderName, e.Data, e.Type); //Sending with type just in case.
        }

        public static void Update(string[] keys, object[] data)
        {
            // Check that the number of keys and the number of objects is equal.
            if (keys.Length == data.GetLength(0)) {
                for (int i = 0; i < keys.Length; i++)
                {
                    _Data[keys[i]] = new ObjectStore(keys[i], JsonConvert.SerializeObject(data[i]), data[i].GetType());
                }
                WriteToState();
                return;
            }
            Plugin.BepinLogger.LogMessage("Unable to add data to the State, number of keys and objects do not match.");
        }
        public static bool ContainsKey(string key)
        {
            return _Data.ContainsKey(key);
        }
        // Attempts to get data of a type from the data, if unable to returns the default value of that type of object.
        public static T GetData<T>(string key) {
            if (ContainsKey(key))
            {
                object value = JsonConvert.DeserializeObject(_Data[key].SerializedObject);
                if (typeof(T) == _Data[key].SerializedObjectType)
                {
                    try
                    {
                        T data = (T)value;
                        return data;
                    }
                    catch
                    {
                        Plugin.BepinLogger.LogWarning($"Unable to get {key} as {typeof(T).ToString()}.");
                        return default(T);
                    }
                }
                Plugin.BepinLogger.LogWarning($"Type of {key} does not match expected Type of {_Data[key].SerializedObjectType.ToString()}.");
                return default(T);
            }
            Plugin.BepinLogger.LogWarning($"Key {key} does not exist.");
            return default(T);
        }
        // Loads the data from the state into an object, if the file cannot be found creates a file and generates an empty state store.
        private static StateStore LoadData() {
            if (File.Exists(StateFilePath))
            {
                string data = File.ReadAllText(StateFilePath);
                if (data != null && data.Length != 0)
                {
                    return JsonConvert.DeserializeObject<StateStore>(data);
                }
            }
            File.Create(StateFilePath);
            return new StateStore();
        }
        private static void WriteToState() {
            try
            {
                using (StreamWriter writer = new StreamWriter(StateFilePath))
                {
                    writer.Write(JsonConvert.SerializeObject(_Data));
                }
                // The 'using' statement automatically calls Dispose(), which flushes and closes the writer.
            }
            catch (IOException e)
            {
                Plugin.BepinLogger.LogMessage($"An error occurred: {e.Message}");
            }
        }
        //Resets State to minimal information to ensure it doesn't break on trying to start a new run after completing an Archipelago.
        public static void Reset() {
            _Data = new StateStore();
            // Store the Archipelago settings for the next launch.
            ArchipelagoData blankData = new ArchipelagoData(ArchipelagoClient.ServerData.Uri, ArchipelagoClient.ServerData.SlotName, ArchipelagoClient.ServerData.Password);
            // Store the newly created data for next time.
            blankData.Store<ArchipelagoData>("ServerData");
        }
        //Extensions that allow you to store an object in the state file under a given key. Making save false will prevent the State from automatically saving to file.
        public static void Store<T>(this object data, string key, bool save = true)
        {
            if (save)
            {
                Update(key, data, typeof(T));
            }
            else { 
                UpdateNoSave(key, data, typeof(T));
            }
        }
        public static void Store<T>(this object data, string key, Type type, bool save = true)
        {
            if (save)
            {
                Update(key, data, type);
            }
            else
            {
                UpdateNoSave(key, data, type);
            }
        }
    }
}