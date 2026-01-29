using BepInEx;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.ModRooms;
using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BluePrinceArchipelago
{
    internal class ModInstance : MonoBehaviour
    {
        private static ModRoomManager modRoomManager = new();
        private static Dictionary<string, PlayMakerArrayListProxy> PickerArrays = new();
        public ModInstance(IntPtr ptr) : base(ptr)
        {
        }
        private void Start()
        {
            SceneManager.sceneLoaded += (Action<Scene, LoadSceneMode>)OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Plugin.BepinLogger.LogMessage($"Scene: {scene.name} loaded in {mode}");
            if (scene.name.Equals("Mount Holly Estate"))
            {
                LoadArrays();
                InitializeRooms();
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= (Action<Scene, LoadSceneMode>)OnSceneLoaded;
        }

        private void OnGUI()
        {
            // show the mod is currently loaded in the corner
            GUI.Label(new Rect(16, 16, 300, 20), Plugin.ModDisplayInfo);
            ArchipelagoConsole.OnGUI();

            string statusMessage;
            // show the Archipelago Version and whether we're connected or not
            if (ArchipelagoClient.Authenticated)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = false;

                statusMessage = " Status: Connected";
                GUI.Label(new Rect(16, 50, 300, 20), Plugin.APDisplayInfo + statusMessage);
            }
            else
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = true;

                statusMessage = " Status: Disconnected";
                GUI.Label(new Rect(16, 50, 300, 20), Plugin.APDisplayInfo + statusMessage);
                GUI.Label(new Rect(16, 70, 150, 20), "Host: ");
                GUI.Label(new Rect(16, 90, 150, 20), "Player Name: ");
                GUI.Label(new Rect(16, 110, 150, 20), "Password: ");

                ArchipelagoClient.ServerData.Uri = GUI.TextField(new Rect(150, 70, 150, 20),
                    ArchipelagoClient.ServerData.Uri);
                ArchipelagoClient.ServerData.SlotName = GUI.TextField(new Rect(150, 90, 150, 20),
                    ArchipelagoClient.ServerData.SlotName);
                ArchipelagoClient.ServerData.Password = GUI.TextField(new Rect(150, 110, 150, 20),
                    ArchipelagoClient.ServerData.Password);

                // requires that the player at least puts *something* in the slot name
                if (GUI.Button(new Rect(16, 130, 100, 20), "Connect") &&
                    !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
                {
                    Plugin.ArchipelagoClient.Connect();
                }
            }
            // this is a good place to create and add a bunch of debug buttons
        }
        private static void LoadArrays() {

        }
        private static void InitializeRooms()
        {
            Plugin.BepinLogger.LogMessage("Initializing Rooms");

            if (modRoomManager == null)
            {
                modRoomManager.AddRoom("AQUARIUM", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G" }, true);
                modRoomManager.AddRoom("ARCHIVES", new List<string>() { "CENTER - Tier 2" }, true);
                modRoomManager.AddRoom("ATTIC", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G" }, true);
                modRoomManager.AddRoom("BALLROOM", new List<string>() { "FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G" }, true);
                modRoomManager.AddRoom("BEDROOM", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("BILLIARD ROOM", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("BOILER ROOM", new List<string>() { "CENTER - Tier 2 G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G" }, true);
                modRoomManager.AddRoom("BOOKSHOP", new List<string>() { "" }, true, false);
                modRoomManager.AddRoom("BOUDOIR", new List<string>() { "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("BUNK ROOM", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("CASINO", new List<string>() { "FRONTBACK G - RARE", "EDGEPIERCE G", "EDGE ADVANCE EASTWING - G", "EDGE ADVANCE WESTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "NORTH PIERCE G", "CENTER - Tier 1 G", "CORNER - Tier 1 G" }, false);
                modRoomManager.AddRoom("CHAMBER OF MIRRORS", new List<string>() { "CENTER - Tier 2" }, true);
                modRoomManager.AddRoom("CHAPEL", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("CLASSROOM", new List<string>() { "CENTER - Tier 1 G", "FRONT - Tier 1 G", "CORNER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G" }, true);
                modRoomManager.AddRoom("CLOCK TOWER", new List<string>() { "CENTER - Tier 2 G", "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G" }, false);
                modRoomManager.AddRoom("CLOISTER", new List<string>() { "CENTER - Tier 2 G" }, true);
                modRoomManager.AddRoom("CLOSED EXHIBIT", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "EDGEPIERCE - RARE", "EDGECREEP - RARE", "CENTER - Tier 2" }, false);
                modRoomManager.AddRoom("CLOSET", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("COAT CHECK", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("COMMISSARY", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G" }, true);
                modRoomManager.AddRoom("CONFERENCE ROOM", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE" }, true);
                modRoomManager.AddRoom("CONSERVATORY", new List<string>() { "CORNER - Tier 1 G" }, true);
                modRoomManager.AddRoom("CORRIDOR", new List<string>() { "FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST" }, true);
                modRoomManager.AddRoom("COURTYARD", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G" }, true);
                modRoomManager.AddRoom("DARKROOM", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("DEN", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("DINING ROOM", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("DORMITORY", new List<string>() { "CORNER - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, false);
                modRoomManager.AddRoom("DOVECOTE", new List<string>() { "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE", "CENTER - Tier 2" }, false);
                modRoomManager.AddRoom("DRAFTING STUDIO", new List<string>() { "FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G" }, true);
                modRoomManager.AddRoom("DRAWING ROOM", new List<string>() { "FRONT - Tier 1 G", "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("EAST WING HALL", new List<string>() { "EDGECREEP EAST", "EDGEPIERCE EAST" }, true);
                modRoomManager.AddRoom("FOYER", new List<string>() { "FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G" }, true);
                modRoomManager.AddRoom("FURNACE", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 3", "EDGECREEP - RARE", "EDGEPIERCE - RARE" }, true);
                modRoomManager.AddRoom("FREEZER", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G" }, true);
                modRoomManager.AddRoom("GALLERY", new List<string>() { "FRONTBACK - RARE", "CENTER - Tier 3", "EDGECREEP - RARE" }, false);
                modRoomManager.AddRoom("GARAGE", new List<string>() { "EDGE ADVANCE WESTWING - G", "EDGEPIERCE G" }, true);
                modRoomManager.AddRoom("GIFT SHOP", new List<string>() { "CENTER - Tier 2", "FRONT - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("GREAT HALL", new List<string>() { "CENTER - Tier 3" }, true);
                modRoomManager.AddRoom("GREENHOUSE", new List<string>() { "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G" }, true);
                modRoomManager.AddRoom("GUEST BEDROOM", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("GYMNASIUM", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE - RARE" }, true);
                modRoomManager.AddRoom("HALLWAY", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1" }, true);
                modRoomManager.AddRoom("HER LADYSHIP'S CHAMBER", new List<string>() { "EDGE RETREAT WESTWING -  G" }, true);
                modRoomManager.AddRoom("HOVEL", new List<string>() { "STANDALONE ARRAY", "STANDALONE ARRAY FULL" }, true);
                modRoomManager.AddRoom("KITCHEN", new List<string>() { "FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G" }, true);
                modRoomManager.AddRoom("LABORATORY", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G" }, true);
                modRoomManager.AddRoom("LAUNDRY ROOM", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G" }, true);
                modRoomManager.AddRoom("LAVATORY", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("LIBRARY", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("LOCKER ROOM", new List<string>() { "FRONT - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING - G", "EDGE RETREAT EASTWING - G", "CENTER - Tier 2 G" }, false);
                modRoomManager.AddRoom("LOCKSMITH", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G" }, true);
                modRoomManager.AddRoom("LOST & FOUND", new List<string>() { "FRONTBACK - RARE", "CORNER - Tier 1", "EDGECREEP WEST", "EDGECREEP EAST", "EDGEPIERCE WEST", "EDGEPIERCE EAST", "SOUTH PIERCE", "CENTER - Tier 2" }, true);
                modRoomManager.AddRoom("MAID'S CHAMBER", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE" }, true);
                modRoomManager.AddRoom("MAIL ROOM", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 3", "EDGECREEP - RARE", "EDGEPIERCE - RARE" }, true);
                modRoomManager.AddRoom("MASTER BEDROOM", new List<string>() { "EDGE ADVANCE EASTWING - G", "EDGE RETREAT EASTTWING -  G" }, true);
                modRoomManager.AddRoom("MECHANARIUM", new List<string>() { "CENTER - Tier 2" }, false);
                modRoomManager.AddRoom("MORNING ROOM", new List<string>() { "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, false, false);
                modRoomManager.AddRoom("MUSIC ROOM", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G" }, true);
                modRoomManager.AddRoom("NOOK", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("NURSERY", new List<string>() { "FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G" }, true);
                modRoomManager.AddRoom("OBSERVATORY", new List<string>() { "FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G" }, true);
                modRoomManager.AddRoom("OFFICE", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "Center Rare G" }, true);
                modRoomManager.AddRoom("PANTRY", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("PARLOR", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("PASSAGEWAY", new List<string>() { "CENTER - Tier 1 G" }, true);
                modRoomManager.AddRoom("PATIO", new List<string>() { "EDGE ADVANCE WESTWING - G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G" }, true);
                modRoomManager.AddRoom("PLANETARIUM", new List<string>() { "CENTER - Tier 2", "FRONT - Tier 1", "CORNER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE" }, false);
                modRoomManager.AddRoom("PUMP ROOM", new List<string>() { "FRONTBACK - RARE", "CORNER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE", "CENTER - Tier 2" }, true, false);
                modRoomManager.AddRoom("ROOM 8", new List<string>() { "DOWSING NOMBOS" }, false, false);
                modRoomManager.AddRoom("ROOT CELLAR", new List<string>() { "STANDALONE ARRAY", "STANDALONE ARRAY FULL" }, true);
                modRoomManager.AddRoom("ROTUNDA", new List<string>() { "CENTER - Tier 2 G" }, true);
                modRoomManager.AddRoom("RUMPUS ROOM", new List<string>() { "FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "Center Rare G" }, true);
                modRoomManager.AddRoom("SAUNA", new List<string>() { "CENTER - Tier 1", "FRONT - Tier 1", "CORNER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE" }, true, false);
                modRoomManager.AddRoom("SCHOOLHOUSE", new List<string>() { "STANDALONE ARRAY", "STANDALONE ARRAY FULL" }, true);
                modRoomManager.AddRoom("SECRET GARDEN", new List<string>() { "" }, true, false);
                modRoomManager.AddRoom("SECRET PASSAGE", new List<string>() { "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "Center Rare G" }, true);
                modRoomManager.AddRoom("SECURITY", new List<string>() { "NORTH PIERCE G", "CENTER - Tier 1 G", "EDGEPIERCE G" }, true);
                modRoomManager.AddRoom("SERVANT'S QUARTERS", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 2 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G" }, true);
                modRoomManager.AddRoom("BOMB SHELTER", new List<string>() { "STANDALONE ARRAY", "STANDALONE ARRAY FULL" }, true);
                modRoomManager.AddRoom("SHOWROOM", new List<string>() { "FRONTBACK G - RARE", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "SILVER KEY LIST 2", "Center Rare G" }, true);
                modRoomManager.AddRoom("SHRINE", new List<string>() { "STANDALONE ARRAY", "STANDALONE ARRAY FULL" }, true);
                modRoomManager.AddRoom("SOLARIUM", new List<string>() { "CORNER - RARE G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "NORTH PIERCE G", "CENTER - Tier 2 G" }, true);
                modRoomManager.AddRoom("SPARE ROOM", new List<string>() { "FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST" }, true);
                modRoomManager.AddRoom("STOREROOM", new List<string>() { "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("STUDY", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE", "Center Rare" }, true);
                modRoomManager.AddRoom("TERRACE", new List<string>() { "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("THE ARMORY", new List<string>() { "CENTER - Tier 1 G", "CORNER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "NORTH PIERCE G" }, false, false);
                modRoomManager.AddRoom("THE FOUNDATION", new List<string>() { "CENTER - Tier 1", "CENTER - Tier 2", "CENTER - Tier 3" }, true);
                modRoomManager.AddRoom("THE KENNEL", new List<string>() { "FRONT - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "CENTER - Tier 1" }, false);
                modRoomManager.AddRoom("THE POOL", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "Center Rare G" }, true);
                modRoomManager.AddRoom("THRONE ROOM", new List<string>() { "EDGEPIERCE - RARE G", "CENTER - Tier 2 G" }, false);
                modRoomManager.AddRoom("TOMB", new List<string>() { "STANDALONE ARRAY", "STANDALONE ARRAY FULL" }, true);
                modRoomManager.AddRoom("TOOLSHED", new List<string>() { "STANDALONE ARRAY", "STANDALONE ARRAY FULL" }, true);
                modRoomManager.AddRoom("TRADING POST", new List<string>() { "STANDALONE ARRAY", "STANDALONE ARRAY FULL" }, true);
                modRoomManager.AddRoom("TREASURE TROVE", new List<string>() { "SILVER KEY LIST 2", "LIBRARY LIST", "LIBRARY LIST RARER", "DOWSING NOMBOS", "BLACKPRINTS" }, false);
                modRoomManager.AddRoom("TROPHY ROOM", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G", "Center Rare G" }, true);
                modRoomManager.AddRoom("TUNNEL", new List<string>() { "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST" }, false);
                modRoomManager.AddRoom("UTILITY CLOSET", new List<string>() { "FRONTBACK - RARE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("VAULT", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 2 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G", "Center Rare G" }, true);
                modRoomManager.AddRoom("VERANDA", new List<string>() { "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G" }, true);
                modRoomManager.AddRoom("VESTIBULE", new List<string>() { "CENTER - Tier 1 G" }, false);
                modRoomManager.AddRoom("WALK-IN CLOSET", new List<string>() { "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 2 G", "EDGECREEP - RARE G", "EDGEPIERCE G", "Center Rare G" }, true);
                modRoomManager.AddRoom("WEIGHT ROOM", new List<string>() { "CENTER - Tier 3", "Center Rare" }, true);
                modRoomManager.AddRoom("WEST WING HALL", new List<string>() { "EDGECREEP WEST", "EDGEPIERCE WEST" }, true);
                modRoomManager.AddRoom("WINE CELLAR", new List<string>() { "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE - RARE" }, true);
                modRoomManager.AddRoom("WORKSHOP", new List<string>() { "FRONTBACK - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "Center Rare" }, true);
            }
        }
    }
}
