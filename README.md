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

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

If you are a player, there's no installation instructions yet. If you are developer please check out the <a href=#installation>Installation</a> section.

### Prerequisites

Please make sure you have Bepinex 6 installed as we need the IL2CPP support.
* [Bepinex 6](https://docs.bepinex.dev/master/articles/user_guide/installation/index.html)

### Installation

1. Install [Bepinex 6](https://docs.bepinex.dev/master/articles/user_guide/installation/index.html)
2. Clone the repo
   ```sh
   git clone https://github.com/Yascob99/BluePrinceArchipelago.git
   ```
3. Change git remote url to avoid accidental pushes to base project
   ```sh
   git remote set-url origin github_username/repo_name
   git remote -v # confirm the changes
   ```
4. If nuget fails to install the required nuget packages it's probably because you need to add a nuget feed for bepinex packages. Try running this command:
```dotnet nuget add source https://nuget.bepinex.dev/v3/index.json --name Bepinex```
5. If nuget didn't install the required dependencies and the previous step didn't fully fix the issue, you will need to run the following to install these packages by running these commands in the **project** folder.
    ```
    dotnet add package BepInEx.Unity.IL2CPP --version 6.0.0-be.753
    dotnet add package Archipelago.MultiClient.Net -- version 6.7.0
    ```

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
