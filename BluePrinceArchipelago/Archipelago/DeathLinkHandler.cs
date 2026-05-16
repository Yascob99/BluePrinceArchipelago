using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using System.Collections;
using UnityEngine;

namespace BluePrinceArchipelago.Archipelago;

public class DeathLinkHandler
{
    private static bool _deathLinkEnabled = true;
    public static bool deathLinkEnabled
    {
        get => _deathLinkEnabled && ArchipelagoOptions.DeathLinkType != DeathLinkType.option_none;
        private set
        {
            _deathLinkEnabled = value;
        }
    }

    private int _deathLinkCount = 0;
    private string slotName;
    private readonly DeathLinkService service;
    private readonly Queue<DeathLink> deathLinks = new();

    /// <summary>
    /// instantiates our death link handler, sets up the hook for receiving death links, and enables death link if needed
    /// </summary>
    /// <param name="deathLinkService">The new DeathLinkService that our handler will use to send and
    /// receive death links</param>
    /// <param name="enableDeathLink">Whether we should enable death link or not on startup</param>
    public DeathLinkHandler(DeathLinkService deathLinkService, string name, bool enableDeathLink = true)
    {
        service = deathLinkService;
        service.OnDeathLinkReceived += DeathLinkReceived;
        slotName = name;
        deathLinkEnabled = enableDeathLink;

        if (deathLinkEnabled)
        {
            service.EnableDeathLink();
        }
    }

    /// <summary>
    /// enables/disables death link
    /// </summary>
    public void ToggleDeathLink()
    {
        deathLinkEnabled = !deathLinkEnabled;

        if (deathLinkEnabled)
        {
            service.EnableDeathLink();
        }
        else
        {
            service.DisableDeathLink();
        }
    }

    /// <summary>
    /// what happens when we receive a deathLink
    /// </summary>
    /// <param name="deathLink">Received Death Link object to handle</param>
    private void DeathLinkReceived(DeathLink deathLink)
    {
        deathLinks.Enqueue(deathLink);

        Logging.Log(deathLink.Cause.IsNullOrWhiteSpace()
            ? $"Received Death Link from: {deathLink.Source}"
            : deathLink.Cause, "DeathLink");

        KillPlayer();
    }

    private int _blockedDeathLinks = 0;

    /// <summary>
    /// can be called when in a valid state to kill the player, dequeueing and immediately killing the player with a
    /// message if we have a death link in the queue
    /// </summary>
    public void KillPlayer()
    {
        try
        {
            if (!ModInstance.IsInRun) return;
            if (deathLinks.Count < 1) return;

            var deathLink = deathLinks.Dequeue();
            var cause = deathLink.Cause.IsNullOrWhiteSpace() ? GetDeathLinkCause(deathLink) : deathLink.Cause;

            if (ArchipelagoOptions.DeathLinkProtection > _blockedDeathLinks)
            {
                _blockedDeathLinks++;
                ArchipelagoConsole.LogMessage($"{cause}. Blocked by protection. Blocks remaining until next: {ArchipelagoOptions.DeathLinkProtection - _blockedDeathLinks}", "DeathLink");
                return;
            }

            ModInstance.Instance.StartCoroutine(KillPlayer(cause, deathLink));
        }
        catch (Exception e)
        {
            Logging.Log(e, "DeathLink");
        }
    }

    public static void ForceKillPlayer(string cause)
    {
        try
        {
            ModInstance.Instance.StartCoroutine(KillPlayer(cause));
        }
        catch (Exception e)
        {
            Logging.LogError(e, "DeathLink");
        }
    }

    private static int _localDeathsInProgress = 0;

    public static void OnRoom46FirstEntered()
    {
        _localDeathsInProgress += 1;
    }

    private static IEnumerator KillPlayer(string cause, DeathLink deathLink = null)
    {
        yield return null;
        _localDeathsInProgress += 1;
        ArchipelagoConsole.LogMessage($"{cause}, {_localDeathsInProgress} local deaths in progress.", "DeathLink");

        ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value = -1000;
        yield return null;
        try
        {
            ModInstance.StepManager.SendEvent("Update");
        }
        catch (Exception e)
        {
            Logging.LogFatal(e, "DeathLink");
            if (deathLink != null)
            {
                Plugin.ArchipelagoClient.DeathLinkHandler.deathLinks.Enqueue(deathLink);
            }
        }

        // ZERO STEP ENDING: Send Event- State 8
    }

    /// <summary>
    /// returns message for the player to see when a death link is received without a cause
    /// </summary>
    /// <param name="deathLink">death link object to get relevant info from</param>
    /// <returns></returns>
    private string GetDeathLinkCause(DeathLink deathLink)
    {
        return $"Received death from {deathLink.Source}";
    }

    private bool _bedroom = false;
    private static readonly string[] _bedroomStrings = ["adyship", "aster", "ervants", "unk", "edroom", "quarium", "oudoir", "ormitory", "ovel", "aid", "ursery"];
    public void SendStepsDeathLink()
    {
        if (ArchipelagoOptions.DeathLinkType != DeathLinkType.option_steps) return;

        if (_localDeathsInProgress > 0)
        {
            Logging.Log($"Steps deathlink prevented due to local death in progress. {_localDeathsInProgress} local deaths in progress.", "DeathLink");
            _localDeathsInProgress -= 1;
            return;
        }

        if (!deathLinkEnabled) return;

        SendDeathLink("Ran out of steps");
    }

    public void SendEndOfDayDeathLink(PlayMakerFSM fsm)
    {
        if (ArchipelagoOptions.DeathLinkType == DeathLinkType.option_steps) return;

        Logging.Log("End of Day, checking for deathlink send", "DeathLink");
        if (_localDeathsInProgress > 0)
        {
            Logging.Log($"End of Day deathlink prevented due to local death in progress. {_localDeathsInProgress} local deaths in progress.", "DeathLink");
            _localDeathsInProgress -= 1;
            return;
        }

        if (!deathLinkEnabled) return;

        var currentRoom = fsm.GetStringVariable("Current Room String").Value;

        if (_bedroomStrings.Any(s => currentRoom.Contains(s)))
        {
            _bedroom = true;
        }

        if (ArchipelagoOptions.DeathLinkType != DeathLinkType.option_steps) SendDeathLink("End of Day");
    }

    /// <summary>
    /// called to send a death link to the multiworld
    /// </summary>
    /// <param name="cause">The cause of the death link.</param>
    public void SendDeathLink(string cause = null)
    {
        try
        {
            if (!deathLinkEnabled) return;

            if (_bedroom)
            {
                _bedroom = false;
                return;
            }

            if (ArchipelagoOptions.DeathLinkMonkException && ModInstance.GetPersistentDataString("Blessing") == "Monk")
            {
                ArchipelagoConsole.LogMessage("Death Link prevented due to Monk blessing.", "DeathLink");
                return;
            }

            if (ArchipelagoOptions.DeathLinkGrace > _deathLinkCount)
            {
                _deathLinkCount++;
                ArchipelagoConsole.LogMessage($"Death Link grace active. Deaths until next deathlink can be sent: {ArchipelagoOptions.DeathLinkGrace - _deathLinkCount}", "DeathLink");
                return;
            }

            ArchipelagoConsole.LogMessage($"Sent {cause} DeathLink", "DeathLink");

            // add the cause here
            var linkToSend = new DeathLink(slotName, cause);

            service.SendDeathLink(linkToSend);
        }
        catch (Exception e)
        {
            Logging.LogError(e, "DeathLink");
        }
    }
}