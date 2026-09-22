using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BluePrinceArchipelago.Triggers
{
    /// <summary>
    ///     Triggers related to GameObjects other than Items, Rooms, and PermanentUnlocks
    /// </summary>
    public static class OtherGameObjectTriggers
    {
        /// <summary>
        ///     When something other than a room or item is spawned.
        /// </summary>
        /// <param name="obj">The spawned object.</param>
        /// <param name="poolName">The name of the spawn pool that object is from.</param>
        /// <param name="transformObj">The object with the spawn location data.</param>
        public static void OnBeforeOtherSpawn(GameObject obj, string poolName, GameObject transformObj)
        {

        }
    }
}
