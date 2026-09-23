<!-- Improved compatibility of back to top link: See: https://github.com/othneildrew/Best-README-Template/pull/73 -->
<a id="readme-top"></a>
<!--
*** Thanks for checking out the Best-README-Template. If you have a suggestion
*** that would make this better, please fork the repo and create a pull request
*** or simply open an issue with the tag "enhancement".
*** Don't forget to give the project a star!
*** Thanks again! Now go create something AMAZING! :D
-->



<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)][license-url]



<!-- PROJECT LOGO -->
<br />
<div align="center">
  <!-- <a href="https://github.com/github_username/repo_name">
    <img src="images/logo.png" alt="Logo" width="80" height="80">
  </a> -->

<h3 align="center">Blue Prince Archipelago</h3>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
		  <ul>
			  <li><a href="#disclaimer">Disclaimer</a></li>
			  <li><a href="#player-setup">Player Setup</a></li>
			  <li><a href="#developer-setup">Developer Setup</a></li>
		  </ul>
		<li><a href="#unity-setup">Contributing</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#development-roadmap">Development Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project


This is the Alpha version of the Archipelago mod for the 2025 roguelite puzzle game Blue Prince. This mod is still in active development as we continue to add features and QoL changes.

Special Thanks to: 
- ChaseoQueso for the inital item code and the custom archipelago swirly asset.
- Mac for helping out on the mod and APworld
- deefdragon and BatemenzDW for their work on the APworld.
- Shavnir for helping out with the mod.
- Zygan for some custom art assets.
- The Blue Prince community on the Archipelago Discord for all of their fantastic ideas.
- The Silksong/HK community for a lot of great tools which made modding so much easier.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GETTING STARTED -->
## Getting Started

If you are a player, Installation instructions are just below. If you are developer please check out the <a href=#developer-setup>Developer Setup</a> section.

### Installation

#### Disclaimer:
This version of Blue Prince Archipelago is in Alpha and as such has quite a few known issues and limitations. All known Bugs will be listed [here](https://github.com/Yascob99/BluePrinceArchipelago/issues?q=is%3Aissue%20state%3Aopen%20label%3Abug). Any QoL changes or future planned changes will live [here](https://github.com/Yascob99/BluePrinceArchipelago/issues?q=is%3Aissue%20state%3Aopen%20label%3Aenhancement).

Below are a List of Limitations and Known issues of the Mod that are not expected to be fixed in the short term.
While fixes are being looked into and worked on they were not prioritized for Alpha.
- Only save files created on Bequest Mode (the game's default mode) are currently compatible.
- Some special items and rooms can bypass the limited draft pool (such as the Prism Key and Secret Passage).
- Room Upgrades aren't accounted for in logic and may result in out of logic checks.
- Item logic only accounts for the possibility of an item spawning, regardless of the rarity of that item actually showing up.
- Trunk logic currently doesn't account for how likely the rooms are to appear and the luck requirements for a trunk to spawn. I have made a [list in discord](https://discord.com/channels/731205301247803413/1362478224604397739/1524408912114221238) to use as a guide for selecting trunk options for now.
- This implementation requires some late game knowledge for items and locations even on early goals.
- Certain items don't look quite right in pickup menus or in game please add any to [this bug](https://github.com/Yascob99/BluePrinceArchipelago/issues/63) if they aren't already listed so this can be fixed in a future patch.
- Sometimes rooms outside of your draft pool appear in drafts. I am currently unsure on how to fix this fully and need more data on which rooms drafted from where Please add any new instances of this to [this issue](https://github.com/Yascob99/BluePrinceArchipelago/issues/62).
- Certain items like Repellant may not function as intended. Please check if a bug exists here or create a new one if you find an item that doesn't work [here](https://github.com/Yascob99/BluePrinceArchipelago/issues?q=is%3Aissue%20state%3Aopen%20label%3Abug)

#### Player Setup

**Before you install this mod, I highly reccomend backing up your save file MtHollyBlueprint.es3 from your ~\AppData\LocalLow\Dogubomb\BLUE PRINCE\storage folder. Copy it somewhere safe.**
Blue Prince Archipelago does not touch the save file process in any way, but I can't guarantee that it won't. Use this mod at your own discresion.

If you prefer a simpler install we do have a [Thunderstore version](https://thunderstore.io/c/blue-prince/p/BluePrinceArchipelago/BluePrinceArchipelago/). Please note that I accidentally messed up the semantic versioning so 1.1.1 is actually meant to be 0.1.1 and so on. Due to how Thunderstore is setup this is not a mistake I can easily fix. 

1. Install [Bepinex 6](https://docs.bepinex.dev/master/articles/user_guide/installation/index.html)
* Specifically you will want to get build #755's [BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.755+3fab71a.zip](https://builds.bepinex.dev/projects/bepinex_be/755/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.755%2B3fab71a.zip) from the page [here](https://builds.bepinex.dev/projects/bepinex_be)
* Extract the files inside the  Blue Prince folder (Steam default of `C:\Program Files (x86)\Steam\steamapps\common\Blue Prince` )

2. After installing Bepinex, start the game once from Steam. Wait for the game to start as normal then quit out. This will take some extra time only on the first time you do this on a fresh patch.
	- If you are on linux you will need to add the following to your steam launch options `WINEDLLOVERRIDES="winhttp=n,b" %command%`. You can find them by clicking the gear icon on the right side of the game and click properties.

3. Download the Latest mod release from [here](https://github.com/Yascob99/BluePrinceArchipelago/releases) and the APworld Release from [here](https://github.com/BatmenzDW/Archipelago/releases).

4. Extract the contents of the mod to <YourBluePrinceInstallLocationHere>/BepInEx/plugins/BluePrinceArchipelago. Your final folder should look something like this:
<img width="598" height="149" alt="image" src="https://github.com/user-attachments/assets/9b6a8ec9-44dd-481f-b2f1-07839efcbfcd" />

5. Start the game. You will notice a new UI at the top. It is best to connect here before starting a new profile. This UI can be shown by pressing "/" and dismissed by pressing "ESC". This also doubles as your game client. Tying /help in the command box will show all the local commands. You can also run archipelago server commands from this console once connected. A lot of the commands are intended for dev usage to allow for progress when there is an issue with the mod.

6. **IMPORTANT** If you have played a Blue Prince AP before in a **different multiworld** please press the **New Run Data Reset Button** or **run the /ResetData command** before connecting. Due to limitations in the mod, the mod currently can't cleanly detect when it is safe to delete the local data on the last seed you played, so this must be done manually when starting a new seed.

7. To connect to your slot enter the connection details into the box on the left side of the UI. You will receive a message in the console if something goes wrong. If you are continueing a run (like in an async) load the same profile you loaded previously on the async run before connecting. The mod currently cannot detect it has loaded the correct seed so loading the wrong file may bork the mod's local saved data on your seed which may result in unexpected glitches.

8. **IMPORTANT** After connecting to the Archipelago server, if you are starting a new multiworld, **create a new file in Bequest Mode**. While the game will function on other modes, it may create impossible to complete scenarios, so it is **Bequest Mode only** for now. If you are reconnecting or continueing a run, be sure to load the correct profile; the mod currently cannot detect it has loaded the correct seed so loading the wrong file may bork the mod's local saved data on your seed which may result in unexpected glitches.

9. Enjoy!

**Extras:**
- Blue Prince supports Universal Tracker. You can find it in #Universal-Tracker on the discord or download the client [here](https://github.com/FarisTheAncient/Archipelago/releases/latest).
- To disable your mods open up the doorstop_config.ini in your Blue Prince Installation folder and set enabled to false. This will prevent Bepinex from loading.

#### Developer Setup:

1. Install [Bepinex 6](https://docs.bepinex.dev/master/articles/user_guide/installation/index.html)
* Specifically you will want to get build #755's [BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.755+3fab71a.zip](https://builds.bepinex.dev/projects/bepinex_be/755/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.755%2B3fab71a.zip) from the page [here](https://builds.bepinex.dev/projects/bepinex_be)
* Extract the files inside the  Blue Prince folder (Steam default of `C:\Program Files (x86)\Steam\steamapps\common\Blue Prince` )
2. Clone the repo
   ```sh
   git clone https://github.com/Yascob99/BluePrinceArchipelago.git
   ```
3. Change git remote url to avoid accidental pushes to base project or check out <a href=#contributing>Contributing</a> if you want to help contribute instead.
   ```sh
   git remote set-url origin github_username/repo_name
   git remote -v # confirm the changes
   ```
4. Add extra nuget package locations
```dotnet nuget add source https://nuget.bepinex.dev/v3/index.json --name Bepinex
   dotnet nuget add source https://nuget.samboy.dev/v3/index.json --name Samboy
 ```

5. If nuget didn't install the required dependencies and the previous step didn't fully fix the issue, you will need to run the following to install these packages by running these commands in the **project** folder.
    ```
    dotnet add package BepInEx.Unity.IL2CPP --version 6.0.0-be.755
    dotnet add package Archipelago.MultiClient.Net --version 6.7.1
    ```

6. Go to [SteamDB](https://steamdb.info/app/1569580/depots/), find the depot for your platform. Then go to manifests. Switch it to Steam Console then click the copy icon next to the newest manifest. Open steam console when prompted.

7. Download [MelonLoader 0.7.3](https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.3) for your platform. Install it on the version of Blue Prince that you downloaded with the Steam Console.

8. In Steam choose "Add a Game" from the bottom right and choose "Add Non-Steam Game" navigate the the melonmodded version. This will be your Melonmodded version of Blue Prince. Like Bepinex run the game once without any mods installed.

9. Create a new file in the root of the repository and call it Directory.Build.props. Add this to the file replacing the directory "Path/To/BepInEx/BluePrinceHere/" with the path to your BepInEx Blue Prince Installation and "Path/To/MelonLoader/BluePrinceHere/" with your MelonLoader Installation.
It will be on the lines marked with:
`<BluePrinceBepInExDir>`
and
`<BluePrinceMelonLoaderDir>`
```xml
<Project>
	<PropertyGroup>
        <TargetFramework>net6.0</TargetFramework>
        <AssemblyName>BluePrinceArchipelago</AssemblyName>
        <Description>Blue Prince Archipelago</Description>
        <Version>0.1.5</Version>
        <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
        <LangVersion>latest</LangVersion>
        <!-- you may need this for getting the multiclient dll and Newtonsoft.Json.dll to output for .net 6 and netstandard 2.0 -->
		<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
        <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
        <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
		<GenerateDependencyFile>false</GenerateDependencyFile>
        <RootNamespace>BluePrinceArchipelago</RootNamespace>
        <Configurations>Debug;Release</Configurations>
        <PlatformTarget>AnyCPU</PlatformTarget>
		<GenerateDocumentationFile>true</GenerateDocumentationFile>
		<NoWarn>$(NoWarn);1591</NoWarn>
		<BluePrinceBepInExDir>C:\Program Files (x86)\Steam\steamapps\common\Blue Prince</BluePrinceBepInExDir>
		<BluePrinceMelonLoaderDir>C:\Program Files (x86)\Steam\steamapps\common\Blue Prince - MelonModded</BluePrinceMelonLoaderDir>
    </PropertyGroup>
	<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
		<DebugType>none</DebugType>
		<Optimize>true</Optimize>
		<OutputPath>$(SolutionDir)\bin\Release\$(Configuration)\$(MSBuildProjectName)\</OutputPath>
		<DefineConstants>TRACE</DefineConstants>
		<ErrorReport>prompt</ErrorReport>
		<IsPublishable>False</IsPublishable>
	</PropertyGroup>
	 <ItemGroup>
        <Compile Include="../src/**" LinkBase="."/>
    </ItemGroup>
	
	<ItemGroup>
        <PackageReference Include="Archipelago.MultiClient.Net" Version="6.7.1" IncludeAssets="all" />
        <PackageReference Include="Archipelago.MultiClient.Net.Analyzers" Version="2.0.3">
          <PrivateAssets>all</PrivateAssets>
          <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
		<EmbeddedResource Include="assets/apprefabs" />
	</ItemGroup>
</Project>
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>


### Other Useful Tools

* [Cinematic Unity Explorer](https://github.com/asd9176506911298/CinematicUnityExplorer/tree/master) - download BIE 6.X be.647+ IL2CPP then get the plugin into the `Blue Prince\BepInEx\plugins` folder

* [FSMHelper](https://github.com/Yascob99/FSMHelper) - An extension mod for CinematicUnityExplorer and Unity Explorer which makes FSMs easier to examine in the Object Inspector.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## Unity Setup

### Setting Up Unity Editor

Below are instructions to get access to opening the files in Unity editor.
1. Install [Unity Hub](https://docs.unity.com/en-us/hub/install-hub). You can continue to the step 3 while this installs.
2. Install [Unity 6000.0.58f2](https://unity.com/releases/editor/whats-new/6000.0.58f2). You can continue to the step 3 while this installs.
3. Download (AssetRipper)[https://github.com/AssetRipper/AssetRipper]
4. Open AssetRipper and Click File > Open Folder and navigate to the root install folder of Blue Prince.
5. Once AssetRipper has imported all the files click Export > All Files.
6. Click select folder and choose an output folder.
7. Click Export UnityProject to export it as a Unity project.
8. In Unity Hub go to the Projects tab. Click Add > "Add Project From Disk" and navigate to the project you exported via assetRipper.
9. Open the project. It will launch in safe mode and give you errors.
10. Navigate to the Project folder and go to ExportedProject\Assets\Plugins\Assembly-Csharp-firstpass\Rewired\Demos and delete the ControlRemappingDemo1.cs.
11. Navigate to the "ExportedProject\Assets\Plugins\Assembly-CSharp-firstpass\Rewired\UI\ControlMapper\ControlMapper.cs" file and Go to line 152. Change it to:
```
public GUIInputField(GameObject gameObject) : base(gameObject)
```
12. Change line 156 to:
```
public GUIInputField(Button button, TMP_Text label) : base(button, label)
```
13. Change line 194 to:
```
public GUIToggle(GameObject gameObject) : base(gameObject)
```
14. Change line 198 to:
```
public GUIToggle(Toggle toggle, TMP_Text label) : base(toggle, label)
```
15. Save the file then navigate to ExportedProject\Assets\Plugins\Assembly-CSharp-firstpass\Rewired\Glyphs\UnityUI\UnityUITextMeshProGlyphHelper.cs and go to line 64 and add this line below it:
```
protected Tag() { }
```
16. Save the file and check for more errors, attempt to solve them in a similar fashion. You can ignore all of the warnings.

17. The project will now attempt to open and will fail to open. Navigate to ExportedProject\Assets\ and open up the MainMenu.Unity file with Unity 6000.0.58f2. You now will be able to open most of the games prefabs by browsing for them in the asset brwoser in the bottom left. Some room prefabs may not open and will cause the scene to crash for unknown reasons.

### Creating Asset Bundles

1. First naviagate to ExportedProject\Assets\Editor then create a file. Call it something on theme that ends with ".cs". It will be a script for creating AssetBundles. Paste the following into it:
```

using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class BuildSubsetAssetBundles
{
    [MenuItem("Assets/Build Selected AssetBundles")]
    static void BuildSpecificAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();

        // Example: Only build AssetBundles that start with "ap"
        foreach (string bundleName in allAssetBundleNames)
        {
            if (bundleName.StartsWith("ap"))
            {
                AssetBundleBuild build = new AssetBundleBuild
                {
                    assetBundleName = bundleName,
                    assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName)
                };
                builds.Add(build);
            }
        }

        if (builds.Count > 0)
        {
            BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                            builds.ToArray(),
                                            BuildAssetBundleOptions.None,
                                            BuildTarget.StandaloneWindows);
            Debug.Log($"Built {builds.Count} specific AssetBundles.");
        }
        else
        {
            Debug.Log("No AssetBundles matching criteria found to build.");
        }
    }

    [MenuItem("Assets/Log All AssetBundle Assignments")]
    static void LogAllAssetBundleAssignments()
    {
        string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        Debug.Log($"Total AssetBundles Defined: {allAssetBundleNames.Length}");
        foreach (string bundleName in allAssetBundleNames)
        {
            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            Debug.Log($"AssetBundle: {bundleName} (Assets: {assetPaths.Length})");
            foreach (string path in assetPaths)
            {
                Debug.Log($"  - {path}");
            }
        }
    }
}
```
2. Save the file. Unity Editor will reload and recompile the script automatically.
3. You will need to find a prefab file you want to add to the asset bundle. You can create a new prefab by dragging a game object from the hierarchy into the asset browser. You can then open it and edit as desired.
4. Once you have chosen a asset bundle right click it in the asset browser and select "properties".
5. At the bottom of properties select an assetbundle click new. Name it something starting with ap (unless you change the above script to only build assetbundles that start with whatever is relevant to your name). 
6. Select the menu option Assets > "Build Selected AssetBundles" and the Asset bundle(s) will be build and exported to the ExportedProject\Assets\AssetBundles folder. 
7. The asset bundle is now ready to import into the mod!
   
<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- USAGE EXAMPLES -->
## Usage

Use this space to show useful examples of how a project can be used. Additional screenshots, code examples and demos work well in this space. You may also link to more resources.

_For more examples, please refer to the [Documentation](https://example.com)_

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ROADMAP -->
## Development Roadmap

- Rooms
    - [x] Ability to change initial draft pool and dynamically add back in rooms to the pool.
    - [x] Add ability to add extra copies of rooms to the pool
    - [ ] Add better handling of certain rooms that rely on other events to be added to the pool (eg. Morning Room) - partial done
    - [ ] Add ways of better handling upgraded rooms.
- Items
    - [x] Create AP assets for replacement unique item locations
    - [x] Handle recieving items mid-run and remove it from the appropriate inventories.
    - [x] Handle Junk item rewards.
    - [x] Handle Permanent items (rewards that persist between days).
- Reverse Engineering
    - [x] Find events to hook to track if a run is ongoing so items and traps, and deathlinks can be applied at the proper times.
    - [x] Find out how shops choose their inventory and how to change it based on our items.
        - [ ] Find out how to add checks that can be bought at a randomized price (for other players).
    - [ ] Look into how the trading post functions and how to handle replacing the tradeable items with AP versions when appropriate.
    - [x] Find a place to hook for trunk goals.
- Goals
    - [x] Find where to hook for specific goals being achieved.
- Archipelago
    - [x] Create the logic for handling events from the AP server.
    - [x] Create a reconnect logic that will reconstruct as much of the state as possible from the Data from the AP Server.
    - [x] Create a way of storing run specific data in case of a game crash. (eg which save file, any queued checks, any temporary effects applied to the current day)
- UI
    - [ ] Create a better looking UI
    - [ ] Add a menu option for Archipelago Mode on creating a new file.
- Potential Long Term Goals
    - [ ] Check the ease of changing puzzles like the Mora Jai puzzles for use in future optional modes.
    - [ ] Add in the ability to swap in first enter room checks for a physical item hidden inside each room.


<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- LICENSE -->
## License

Distributed under the MIT. See `LICENSE.MD` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/Yascob99/BluePrinceArchipelago.svg?style=for-the-badge
[contributors-url]: https://github.com/Yascob99/BluePrinceArchipelago/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Yascob99/BluePrinceArchipelago.svg?style=for-the-badge
[forks-url]: https://github.com/Yascob99/BluePrinceArchipelago/network/members
[stars-shield]: https://img.shields.io/github/stars/Yascob99/BluePrinceArchipelago.svg?style=for-the-badge
[stars-url]: https://github.com/Yascob99/BluePrinceArchipelago/stargazers
[issues-shield]: https://img.shields.io/github/issues/Yascob99/BluePrinceArchipelago.svg?style=for-the-badge
[issues-url]: https://github.com/Yascob99/BluePrinceArchipelago/issues
[license-shield]: https://img.shields.io/github/license/Yascob99/BluePrinceArchipelago.svg?style=for-the-badge
[license-url]: https://github.com/Yascob99/BluePrinceArchipelago/blob/main/LISCENSE.md
<!-- Shields.io badges. You can a comprehensive list with many more badges at: https://github.com/inttter/md-badges -->
