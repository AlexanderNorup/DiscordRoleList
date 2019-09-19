# DiscordRoleList
This Discord bot will then make sure that only people who appear in a MySQL database has certain role. Very simple bot to be expanded on in the future.

DiscordRoleList works on all platforms, as it runs on dotnet. The `portable`-release can be run on every system.  The `windows` release can obviously only be run on Windows (This release for the people who just wants an EXE file)

## Download and run 

1) Make sure you have dotNET installed: https://dotnet.microsoft.com/download
2) Go to releases: https://github.com/AlexanderNorup/DiscordRoleList/releases
3) Download the latest release
4) Take a copy of the `DiscordRoleList.dll.config.sample` file, and rename it to: `DiscordRoleList.dll.config`
5) Fill in the config file and run the .dll file

## Running the bot

**All platforms:**
`dotnet DiscordRoleList.dll`

**Windows only:** (The Windows Release)
`DiscordRoleList.exe`

Alternatively, you can use the Windows build, and simply just run the `DiscordRoleList.exe` file. Please note that you still have to fill in the config file.

The bot supports running on multiple servers. Adding more roles to the `rolesToAdd` setting, will add the roles on the servers where it's available. 

Do of course make sure the bot has `manage role`-permission, and that the bot has "higher priority".

## Configuration

When running the bot, it looks for a configuration file within the same directory called `DiscordRoleList.dll.config`. The config file should look like this:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="mysqlConfig" value="server=IP_HERE;uid=USERNAME_HERE;pwd=PASSWORD_HERE;database=DATABASE_HERE;"/> <!-- Mysql Connection string.. -->
    <add key="mysqlColoum" value="COLOUM_NAME" /> <!-- The coloum in the database. The coloum has to contain the discord-id! Not the username!! -->
    <add key="mysqlTable" value="TABLE_NAME" /> <!-- The table from the databse -->
    <add key="bottoken" value="DISCORD_ID" /> <!-- Your Discord-bot token -->
    <add key="statusText" value="Making sure Patreons have the Donator role..." /> <!-- What "Game" should the bot be "playing" -->
    <add key="timerInterval" value="900000" /> <!-- In milliseconds: How often to recheck the database. 900000 is recommended (15m) -->
    <add key="rolesToAdd" value="ROLE_ID1;ROLE_ID2;ROLE_ID3" /> <!-- RoleID's.. To add more seperate by ; -->
  </appSettings>
</configuration>
```

This is what each of the keys mean.

Key | Type of input | Additional information
------------ | ------------- | -------------
`mysqlConfig` | MySQL connection string | Please refer to the [MySQL Docs](https://dev.mysql.com/doc/connector-net/en/connector-net-6-10-connection-options.html) for more information on these
`mysqlColoum` | Table coloum name | Insert the name of the coloum that contains the Discord-ID's of the users who have to recieve the role
`mysqlTable` | MySQL table name | Insert the name of the table in the database that contains the `mysqlColoum`. Please notice the database is specefied in the `mysqlConfig` setting
`bottoken` | Discord Bot-Token | Insert the bot-token of the bot you want to use. You can get a free discord bot-token by going to: [Discord Developer Portal](https://discordapp.com/developers/applications/)
`statusText` | String | This is what the bot appears to be "playing". Can be set to anything
`timerInterval` | Time in milliseconds | Set this to configure how often the bot should check the database, and hereby update the user-roles. It is recommended to set this to 900000 (15m) at mimimum. 
`rolesToAdd` | List of role-id's | Insert the the id to the Discord-roles that you want the bot to give the users. You can add multiple roles, by simply seperating them by semicolons `;`. 

The bot can be used on multiple servers with the same MySQL configuration. The roles in `rolesToAdd` dosn't have to exist in every server. The bot will check what roles exist in what servers, before trying to give anyone the roles.

