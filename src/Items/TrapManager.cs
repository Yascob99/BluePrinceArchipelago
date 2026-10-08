using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
#endif
#if ML
using Il2CppHutongGames.PlayMaker;
#endif
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     Handles traps and the priority in which they are executed.
    /// </summary>
    public static class TrapManager
    {

       public static List<string> TrapQueue {get; set;} = [];
       public static List<string> TrapQueueTypes {get; set;} = [];
       private static int EODTrapsCount = 0;
       private static int FreezeTrapsCount = 0;

       public static bool FrozenToday = false;

       public static bool EODTrapToday = false;
       
       /// <summary>
       ///      Queues a Trap for later execution of the trap's code.
       /// </summary>
       /// <param name="trapName">The Name of the trap.</param>
       /// <param name="traptype">The Type of the trap.</param>
       /// <returns>True if successfully Queued, false if an error occured.</returns>
       public static bool QueueTrap(string trapName, string traptype)
       {
            try {
                TrapQueue.Add(trapName);
                TrapQueueTypes.Add(traptype);
                if (traptype == "EOD")
                {
                    EODTrapsCount++;
                }
                else if (traptype == "Freeze")
                {
                    FreezeTrapsCount ++;
                }
                return true;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        ///     Dequeues the traps that should be dequeued. Priority goes EOD (only execute this, rest is queued for tomorrow) > Others (confirm if out of steps before executing next trap) > Freeze.
        /// </summary>
        /// <returns></returns>
        public static bool DequeueTraps()
        {
            // Don't attempt further dequeues if there's none left to dequeue. 
            // Don't set off traps if there are only Freeze traps left and the player is frozen. 
            // If the EOD trap has gone off today don't queue any other traps. 
            if (TrapQueue.Count == 0 || (FrozenToday && TrapQueue.Count == FreezeTrapsCount) || EODTrapToday) return true;
            if (CanDequeue())
            {
                bool EOD = EODTrapsCount > 0;
                List<int> idsToRun = new List<int>();
                int stepTotal = ResourceManager.GetResourceCount("steps");
                bool FreezeTrap = false;

                // Queue up traps based on priority and context.
                for(int i = 0; i < TrapQueue.Count; i++)
                {
                    string type = TrapQueueTypes[i];
                    string name = TrapQueue[i];

                    // EOD traps happen first and execute alone.
                    if (EOD)
                    {
                        if (type == "EOD")
                        {
                            Trap eodTrap = ModItemManager.GetTrap(name);
                            try {
                                eodTrap.ActivateTrap();
                                TrapQueueTypes.RemoveAt(i);
                                TrapQueue.RemoveAt(i);
                                EODTrapToday = true;
                                return true;
                            }
                            catch {
                                Logging.LogWarning("Error Activating EoD Trap", "Traps");
                                return false;
                            }
                        }
                    }
                    else
                    {

                        if (type == "Freeze")
                        {
                            if (!FreezeTrap && !FrozenToday){
                                FreezeTrap = true;
                                idsToRun.Add(i);
                            }
                        }
                        else if (type == "Steps" && stepTotal >= 0)
                        {
                            Trap stepTrap = ModItemManager.GetTrap(name);
                            if (stepTrap != null)
                            {
                                stepTotal += stepTrap.Count;
                                idsToRun.Add(i);
                            }
                        }
                        else
                        {
                            idsToRun.Add(i);
                        }
                    }
                }
                // Run the traps in the prescribed order.
                for (int j = idsToRun.Count -1; j > -1; j--)
                {
                    int id = idsToRun[j];
                    string name = TrapQueue[id];
                    string type = TrapQueueTypes[id];
                    if (type != "freeze")
                    {
                            
                    
                        Trap trap = ModItemManager.GetTrap(name);
                        Logging.LogWarning($"Attempting to Dequeue Trap: {name}");
                        try
                        {
                        
                            trap.ActivateTrap();
                            TrapQueue.RemoveAt(id);
                            TrapQueueTypes.RemoveAt(id);
                        }
                        catch
                        {
                            // Minor traps can fail and just not retry. Other traps it's more important that they get properly adjusted.
                            Logging.LogWarning($"Failed to run trap {name}", "Traps");
                        }
                    }
                    else
                    {
                        TrapQueue.RemoveAt(id);
                        TrapQueueTypes.RemoveAt(id);
                        FreezeTrapsCount -= 1;
                    }
                }
                if (FreezeTrap)
                {
                    Trap trap = ModItemManager.GetTrap("Trap Freeze Items");
                     try
                        {
                            trap.ActivateTrap();

                        }
                        catch
                        {
                            TrapQueue.Add("Trap Freeze Items");
                            TrapQueueTypes.Add("Freeze");
                            FreezeTrapsCount += 1;
                        }
                }
                return true;
            }
            return false;  
        }
        public static bool CanDequeue()
        {
            return  ArchipelagoClient.Authenticated && ModInstance.IsInRun && ModInstance.RanStartOfDay;  
        }

    }
}