# Bardiche - Discord Bot

A Discord bot written using the Discord.NET library. Primary functions are queries to Anilist and vndb, as well as RSS feeds with and without filters customizable through bot commands.

### Usage

Only self-hosting is supported. To do so, feel free to copile the project yourself or to download the [release version] (bin/).
Next, edit in the needed information into the config.json file under the Resources folder.
The needed information is as follows:
* Bot token - Get it by creating the bot application to add to your server [here] (https://discordapp.com/developers/applications/me).
* RSS channel id - Channel where the rss feed information will be posted. Required only if you want to use the rss module.
* Admin id - List for admin ids. Used for the two commands that require the user to be an admin: prune everyone's messages and prune the bot messages.
* Bot id - Required for pruning the bot messages.
The last three values can be gotten by right-clicking the channel / users / bots within Discord with the Developer mode enabled and selecting "Copy ID".

Bot commands can be seen by using the !help command.

### Built With

* [Discord.NET](https://github.com/RogueException/Discord.Net) - Discord API wrapper
* [VndbClient by FredTheBarber] (https://github.com/FredTheBarber/VndbClient) - vndb API interaction was heavily based on this project

### Authors

* Dusk252

### License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

