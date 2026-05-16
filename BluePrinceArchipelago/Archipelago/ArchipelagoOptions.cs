using Archipelago.MultiClient.Net;
using BluePrinceArchipelago.Models;
using BluePrinceArchipelago.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BluePrinceArchipelago.Archipelago;

/// <summary>
/// Stores and provides access to Archipelago slot options for Blue Prince.
/// Options are retrieved from the server's SlotData when connecting.
/// </summary>
public static class ArchipelagoOptions
{
    /// <summary>
    /// Indicates whether options have been loaded from the server.
    /// </summary>
    public static bool IsLoaded { get; private set; } = false;

    /// <summary>
    /// Raw slot data dictionary from the server (for any custom lookups).
    /// </summary>
    public static Dictionary<string, object> RawSlotData { get; private set; }

    // ============================================================
    // Blue Prince Options - should match APWorld options
    // ============================================================

    /// <summary>
    /// When true, room drafts are randomized via Archipelago.
    /// When false, use vanilla room draft behavior.
    /// </summary>
    public static bool RoomDraftSanity { get; private set; } = true;

    /// <summary>
    /// Number of common locked trunks in the item pool.
    /// </summary>
    public static int LockedTrunksCommon { get; private set; } = 0;

    /// <summary>
    /// Number of rare locked trunks in the item pool.
    /// </summary>
    public static int LockedTrunksRare { get; private set; } = 0;

    /// <summary>
    /// Number of complex locked trunks in the item pool.
    /// </summary>
    public static int LockedTrunksComplex { get; private set; } = 0;

    /// <summary>
    /// Item logic mode for the randomizer.
    /// </summary>
    public static ItemLogicMode ItemLogicMode { get; private set; } = ItemLogicMode.option_default;

    /// <summary>
    /// When true, standard items are randomized.
    /// </summary>
    public static bool StandardItemSanity { get; private set; } = false;

    /// <summary>
    /// When true, workshop items are randomized.
    /// </summary>
    public static bool WorkshopSanity { get; private set; } = false;

    /// <summary>
    /// When true, upgrade disks are randomized.
    /// </summary>
    public static bool UpgradeDiskSanity { get; private set; } = false;

    /// <summary>
    /// When true, keys are randomized.
    /// </summary>
    public static bool KeySanity { get; private set; } = false;

    /// <summary>
    /// Death link type setting.
    /// </summary>
    public static DeathLinkType DeathLinkType { get; private set; } = DeathLinkType.option_none;

    /// <summary>
    /// Death link grace.
    /// </summary>
    public static int DeathLinkGrace { get; private set; } = 0;

    /// <summary>
    /// Death link protection.
    /// </summary>
    public static int DeathLinkProtection { get; private set; } = 0;


    /// <summary>
    /// Death link monk exception setting.
    /// </summary>
    public static bool DeathLinkMonkException { get; private set; } = true;

    /// <summary>
    /// Goal type for winning.
    /// </summary>
    public static GoalType GoalType { get; private set; } = GoalType.option_sanctum;

    /// <summary>
    /// Number of sanctum solves required for goal (if applicable).
    /// </summary>
    public static int GoalSanctumSolves { get; private set; } = 1;

    // ============================================================
    // Methods
    // ============================================================

    /// <summary>
    /// Loads options from the SlotData model.
    /// Call this after successful connection.
    /// </summary>
    /// <param name="slotData">The SlotData received from the server.</param>
    public static void LoadFromSlotData(SlotData slotData)
    {
        if (slotData == null)
        {
            Logging.LogWarning("SlotData is null - using default options");
            IsLoaded = true;
            return;
        }

        try
        {
            // Parse options from SlotData model
            RoomDraftSanity = slotData.RoomDraftSanity;
            LockedTrunksCommon = slotData.LockedTrunksCommon;
            LockedTrunksRare = slotData.LockedTrunksRare;
            LockedTrunksComplex = slotData.LockedTrunksComplex;
            ItemLogicMode = slotData.ItemLogicMode;
            StandardItemSanity = slotData.StandardItemSanity;
            WorkshopSanity = slotData.WorkShopSanity;
            UpgradeDiskSanity = slotData.UpgradeDiskSanity;
            KeySanity = slotData.KeySanity;
            DeathLinkType = slotData.DeathLinkType;
            DeathLinkGrace = slotData.DeathLinkGrace;
            DeathLinkProtection = slotData.DeathLinkProtection;
            DeathLinkMonkException = slotData.DeathLinkMonkException;
            GoalType = slotData.GoalType;
            GoalSanctumSolves = slotData.GoalSanctumSolves;

            IsLoaded = true;
            LogOptions();
        }
        catch (Exception ex)
        {
            Logging.LogError($"Failed to load options: {ex.Message}", "ArchipelagoOptions");
            IsLoaded = false;
        }
    }

    /// <summary>
    /// Loads options from a raw dictionary (alternative method).
    /// </summary>
    /// <param name="slotData">The raw slot data dictionary.</param>
    public static void LoadFromDictionary(Dictionary<string, object> slotData)
    {
        if (slotData == null)
        {
            Logging.LogWarning("SlotData dictionary is null - using default options", "ArchipelagoOptions");
            IsLoaded = true;
            return;
        }

        try
        {
            RawSlotData = slotData;

            // Parse options from dictionary
            RoomDraftSanity = GetBool(slotData, "room_draft_sanity", true);
            LockedTrunksCommon = GetInt(slotData, "locked_trunks_common", 0);
            LockedTrunksRare = GetInt(slotData, "locked_trunks_rare", 0);
            LockedTrunksComplex = GetInt(slotData, "locked_trunks_complex", 0);
            ItemLogicMode = (ItemLogicMode)GetInt(slotData, "item_logic_mode", 0);
            StandardItemSanity = GetBool(slotData, "standard_item_sanity", false);
            WorkshopSanity = GetBool(slotData, "workshop_sanity", false);
            UpgradeDiskSanity = GetBool(slotData, "upgrade_disk_sanity", false);
            KeySanity = GetBool(slotData, "key_sanity", false);
            DeathLinkType = (DeathLinkType)GetInt(slotData, "death_link_type", 0);
            DeathLinkGrace = GetInt(slotData, "death_link_grace", 0);
            DeathLinkMonkException = GetBool(slotData, "death_link_monk_exception", true);
            GoalType = (GoalType)GetInt(slotData, "goal_type", 0);
            GoalSanctumSolves = GetInt(slotData, "goal_sanctum_solves", 1);

            IsLoaded = true;
            LogOptions();
        }
        catch (Exception ex)
        {
            Logging.LogError($"Failed to load options from dictionary: {ex.Message}", "ArchipelagoOptions");
            IsLoaded = false;
        }
    }

    /// <summary>
    /// Resets all options to their default values.
    /// </summary>
    public static void Reset()
    {
        RoomDraftSanity = true;
        LockedTrunksCommon = 0;
        LockedTrunksRare = 0;
        LockedTrunksComplex = 0;
        ItemLogicMode = ItemLogicMode.option_default;
        StandardItemSanity = false;
        WorkshopSanity = false;
        UpgradeDiskSanity = false;
        KeySanity = false;
        DeathLinkType = DeathLinkType.option_none;
        DeathLinkGrace = 0;
        DeathLinkMonkException = true;
        GoalType = GoalType.option_sanctum;
        GoalSanctumSolves = 1;
        RawSlotData = null;
        IsLoaded = false;
    }

    /// <summary>
    /// Logs all current option values for debugging.
    /// </summary>
    public static void LogOptions()
    {
        Logging.Log("=== Archipelago Options ===", "ArchipelagoOptions");
        Logging.Log($"  RoomDraftSanity: {RoomDraftSanity}", "ArchipelagoOptions");
        Logging.Log($"  LockedTrunksCommon: {LockedTrunksCommon}", "ArchipelagoOptions");
        Logging.Log($"  LockedTrunksRare: {LockedTrunksRare}", "ArchipelagoOptions");
        Logging.Log($"  LockedTrunksComplex: {LockedTrunksComplex}", "ArchipelagoOptions");
        Logging.Log($"  ItemLogicMode: {ItemLogicMode}", "ArchipelagoOptions");
        Logging.Log($"  StandardItemSanity: {StandardItemSanity}", "ArchipelagoOptions");
        Logging.Log($"  WorkshopSanity: {WorkshopSanity}", "ArchipelagoOptions");
        Logging.Log($"  UpgradeDiskSanity: {UpgradeDiskSanity}", "ArchipelagoOptions");
        Logging.Log($"  KeySanity: {KeySanity}", "ArchipelagoOptions");
        Logging.Log($"  DeathLinkType: {DeathLinkType}", "ArchipelagoOptions");
        Logging.Log($"  DeathLinkGrace: {DeathLinkGrace}", "ArchipelagoOptions");
        Logging.Log($"  DeathLinkMonkException: {DeathLinkMonkException}", "ArchipelagoOptions");
        Logging.Log($"  GoalType: {GoalType}", "ArchipelagoOptions");
        Logging.Log($"  GoalSanctumSolves: {GoalSanctumSolves}", "ArchipelagoOptions");
    }

    /// <summary>
    /// Gets a raw option value by key. Returns null if not found.
    /// </summary>
    public static object GetRawOption(string key)
    {
        if (RawSlotData != null && RawSlotData.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;
    }

    // ============================================================
    // Helper methods for parsing slot data values
    // ============================================================

    private static bool GetBool(Dictionary<string, object> data, string key, bool defaultValue)
    {
        if (data.TryGetValue(key, out var value))
        {
            if (value is bool b) return b;
            if (value is long l) return l != 0;
            if (value is int i) return i != 0;
            if (value is JValue jv) return jv.ToObject<bool>();
            if (bool.TryParse(value?.ToString(), out var parsed)) return parsed;
        }
        return defaultValue;
    }

    private static int GetInt(Dictionary<string, object> data, string key, int defaultValue)
    {
        if (data.TryGetValue(key, out var value))
        {
            if (value is int i) return i;
            if (value is long l) return (int)l;
            if (value is JValue jv) return jv.ToObject<int>();
            if (int.TryParse(value?.ToString(), out var parsed)) return parsed;
        }
        return defaultValue;
    }

    private static string GetString(Dictionary<string, object> data, string key, string defaultValue)
    {
        if (data.TryGetValue(key, out var value))
        {
            if (value is string s) return s;
            if (value is JValue jv) return jv.ToObject<string>();
            return value?.ToString() ?? defaultValue;
        }
        return defaultValue;
    }
}
