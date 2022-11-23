# HermesProxy ![Build](https://github.com/WowLegacyCore/HermesProxy/actions/workflows/Build_Proxy.yml/badge.svg)

This project enables play on existing legacy WoW emulation cores using the modern clients. It serves as a translation layer, converting all network traffic to the appropriate format each side can understand.

There are 4 major components to the application:
- The modern BNetServer to which the client initially logs into.
- The legacy AuthClient which will in turn login to the remote authentication server (realmd). 
- The modern WorldServer to which the game client will connect once a realm has been selected. 
- The legacy WorldClient which communicates with the remote world server (mangosd).

## Supported Versions

| Modern Versions | Legacy Versions |
|-----------------|-----------------|
| 1.14.0          | 1.12.1          |
| 2.5.2           | 2.4.3           |

## Usage Instructions

- Edit the app's config to specify the exact versions of your game client and the remote server, along with the address.
- Go into your game folder, in the Classic or Classic Era subdirectory, and edit WTF/Config.wtf to set the portal to 127.0.0.1.
- Download [Arctium Launcher](https://github.com/Arctium/WoW-Launcher/releases/tag/latest) into the main game folder, and then run it  
with `--staticseed --version=ClassicEra` for vanilla  
or `--staticseed --version=Classic` for TBC.
- Start the proxy app and login through the game with your usual credentials.

## Chat commands
HermesProxy provides some internal chat commands:

| Command                    | Description                                                                  |
|----------------------------|------------------------------------------------------------------------------|
| `!qcomplete <questId>`     | Manually marks a quest as already completed (useful for quest helper addons) |
| `!quncomplete <questId>`   | Unmarks a quest as completed                                                 |

##v3.2 Not connecting troubleshoot

- If with the most recent version (v3.2) you are getting "You have been disconnected". In the config file change ExternalAddress to 0.0.0.0 from the default 127.0.0.1

## Acknowledgements

Parts of this poject's code are based on [CypherCore](https://github.com/CypherCore/CypherCore) and [BotFarm](https://github.com/jackpoz/BotFarm). I would like to extend my sincere thanks to these projects, as the creation of this app might have never happened without them. And I would also like to expressly thank [Modox](https://github.com/mdx7) for all his work on reverse engineering the classic clients and all the help he has personally given me. 

## Download HermesProxy
Stable Downloads: [Releases](https://github.com/Lihvar/HermesProxy/releases)

Latest Downloads (development, unstable): [Build Artifacts](https://github.com/Lihvar/HermesProxy/actions/workflows/Build_Proxy.yml?query=branch%3Amaster)  
(click at the latest successful build and look at the artifacts at the bottom)
