# HermesProxy

This project enables play on existing legacy WoW emulation cores using the modern clients. It serves as a translation layer, converting all network traffic to the appropriate format each side can understand. There are 4 major components to the application. The modern BNetServer to which the client initially logs into. The legacy AuthClient which will in turn login to the remote authentication server (realmd). The modern WorldServer to which the game client will connect once a realm has been selected. And the legacy WorldClient which communicates with the remote world server (mangosd).

### Modern Versions Supported
- 1.14.0
- 2.5.2

### Legacy Versions Supported
- 1.12.1
- 2.4.3

## Usage Instructions

- Edit the app's config to specify the exact versions of your game client and the remote server, along with the address.
- Go into your game folder, in the Classic or Classic Era subdirectory, and edit WTF/Config.wtf to set the portal to 127.0.0.1.
- Download the static auth seed branch of the [Arctium Launcher](https://github.com/Arctium/WoW-Launcher/tree/static-auth-seed) into the main game folder, and then run it with the "--version=ClassicEra" (or Classic for TBC) argument.
- Start the proxy app and login through the game with your usual credentials.

## Acknowledgements

Parts of this poject's code are based on [CypherCore](https://github.com/CypherCore/CypherCore) and [BotFarm](https://github.com/jackpoz/BotFarm). I would like to extend my sincere thanks to these projects, as the creation of this app might have never happened without their example.
