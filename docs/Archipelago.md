# Archipelago

## General Info

These are the core functionality and supportive classes for interacting with the archipelago server and creating the client. A lot of the server interaction is handled through the [Archipelago.MultiClient.Net](https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net) Api.

## Archipelago Client

This Class contains the majority of the code for Connecting, Reconnecting, Disconnecting to the Archipelago Server. It also handles sending and recieving important server packets such as Messages, Errors, Items, SocketClosed, and Locations being manually checked by the server.

In addition the following is also handled here:
* Rebuilding the local state based on a combination of locally stored data and server data on reconnect.
* Figuring out if the item received should be handled immediately or delayed for later.

### Archipelago QueueManager

This Class handles my custom Queue like lists for queueing specific types of data. The reason lists were used at this time was because queues were running into threadedness issues for some reason. I could probably rework all this to try again with queues but haven't gotten to that yet.

## Archipelago Console

This class handles displaying the UI and the underlying code for the in game console added by the mod as well as any related logging functions. It also has

### Commands

## Archipelago Data

## Archipelago Options

## DeathLink Handler

## DeathLink Messages
