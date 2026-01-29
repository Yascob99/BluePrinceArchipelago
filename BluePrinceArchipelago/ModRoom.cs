using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BluePrinceArchipelago.ModRooms
{
    internal class ModRoomManager {
        public static List<ModRoom> rooms = new List<ModRoom>();
        public ModRoomManager() {
        }
        public void AddRoom(ModRoom room) { 
            rooms.Add(room);
        }
        public void AddRoom(string name, List<string> pickerArrays, bool isUnlocked, bool isRandomizable = true) {
            rooms.Add(new ModRoom(name, GameObject.Find(name), pickerArrays, isUnlocked, isRandomizable));
        }
    }
    internal class ModRoom
    {
        private string Name { get; set; }
        private GameObject GameObj { get; set; }
        private List<string> PickerArrays { get; set; }
        private bool IsUnlocked { get; set; }
        private bool HasBeenDrafted { get; set; }
        private bool IsRandomizable { get; set; }
        

        public ModRoom(String name, GameObject gameObject, List<string> pickerArrays, bool isUnlocked, bool isRandomizable = true) { 
            Name = name;
            GameObj = gameObject;
            foreach (string arrayName in pickerArrays) {
                //handle get array objects and add them to the arrays.
            }
            PickerArrays = pickerArrays;
            IsUnlocked = isUnlocked;
            HasBeenDrafted = false;
            IsRandomizable = isRandomizable;
        }
    }
}
