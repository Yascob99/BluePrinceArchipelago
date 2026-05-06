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
		 <li><a href="#unity-setup">Contributing</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project


This is an in development Archipelago mod for the 2025 roguelite puzzle game Blue Prince. **Please note that the mod is not currently playable yet** and is still being developed.

Special Thanks to: 
- ChaseoQueso for the item code and archipelago version of items
- Mac for helping out on the mod and APworld
- deefdragon and BatemenzDW for their work on the APworld.
- Shavnir for helping out with the mod.
- The Silksong/HK community for a lot of great tools which made modding so much easier.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

If you are a player, there's no installation instructions yet. If you are developer please check out the <a href=#installation>Installation</a> section.

### Prerequisites

Please make sure you have Bepinex 6 installed as we need the IL2CPP support.
* [Bepinex 6](https://docs.bepinex.dev/master/articles/user_guide/installation/index.html)

### Installation

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
6. Create a new file in the root of the repository and call it Directory.Build.props. Add this to the file replacing the directory "Path/To/BluePrinceHere/" with the path to your Blue Prince Installation:
```
<Project>
	<PropertyGroup>
		<BluePrinceDir>Path/To/BluePrinceHere/</BluePrinceDir>
	</PropertyGroup>
</Project>
```
7. After you have built for the first time you will need to copy the Archipelago dll into the BluePrinceArchipelago folder.  This will appear by default in `C:\Users\USERNAME\.nuget\packages\archipelago.multiclient.net\6.7.1\lib\net6.0\Archipelago.MultiClient.Net.dll`.  Copy this into the Blue Prince\BepInEx\plugins\BluePrinceArchipelago folder
<p align="right">(<a href="#readme-top">back to top</a>)</p>


### Other Useful Tools

* [Cinematic Unity Explorer](https://github.com/asd9176506911298/CinematicUnityExplorer/tree/master) - download BIE 6.X be.647+ IL2CPP then get the plugin into the `Blue Prince\BepInEx\plugins` folder

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
    - [ ] Add better handling of certain rooms that rely on other events to be added to the pool (eg. Morning Room)
    - [ ] Add ways of better handling upgraded rooms.
- Items
    - [ ] Create AP assets for replacement unique item locations
    - [ ] Handle recieving items mid-run and remove it from the appropriate inventories.
    - [ ] Handle Junk item rewards.
    - [ ] Handle Permanent items (rewards that persist between days).
- Reverse Engineering
    - [ ] Find events to hook to track if a run is ongoing so items and traps, and deathlinks can be applied at the proper times.
    - [ ] Find out how shops choose their inventory and how to change it based on our items.
        - [ ] Find out how to add checks that can be bought at a randomized price (for other players).
    - [ ] Look into how the trading post functions and how to handle replacing the tradeable items with AP versions when appropriate.
    - [ ] Find a place to hook for trunk goals.
- Goals
    - [ ] Find where to hook for specific goals being achieved.
- Archipelago
    - [ ] Create the logic for handling events from the AP server.
    - [ ] Create a reconnect logic that will reconstruct as much of the state as possible from the Data from the AP Server.
    - [ ] Create a way of storing run specific data in case of a game crash. (eg which save file, any queued checks, any temporary effects applied to the current day)
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
