using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BluePrinceArchipelago.Archipelago
{
    public static class ArchipelagoPrefabs
    {
        public static List<GameObject> Prefabs { get; set; } = new List<GameObject>();

        public static GameObject GetPrefab(string name) {
            if (Prefabs != null)
            {
                foreach (GameObject prefab in Prefabs)
                {
                    if (prefab.name.ToLower().Trim() == name.ToLower().Trim())
                    {
                        return prefab;
                    }
                }
                Logging.LogWarning($"No prefab with name '{name}' found.");
                return null;
            }
            Logging.LogWarning($"Prefabs have not loaded.");
            return null;
        }
    }
}
