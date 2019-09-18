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

## Where's the docs?

~Nowhere, I haven't had the time to write it yet.~

The docs? It's "self-documenting code"
