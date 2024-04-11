# Overview

This is a demonstration solution which shows the dependency problems you get as a plugin writer whose plugin uses
MS Graph and Microsoft Authentification Library (MSAL).

The solution consists of 3 parts.

## common

This is a DLL which contains common interfaces, and nothing else. The purpose of this project is to give
both the executable and the plugin implementation a common type system.

The critical parts in this project are:
- IPluginFactory is the interface for a component which is used to create a plugin instance.
- IPlugin is the interface for the plugin itself. This interface covers some minimal parts of the features
  offered by MS Graph which are used by me "real life" plugin.
- Some data holder structures with no further logic.

## TestMSTeams

This is the executable which really loads the plugin dynamically during runtime and uses it afterwards.
This project has *no* direct dependeny to MS Graph and to MSAL - it only uses the interfaces defined in
the *common* project.

The Program is a WPF program with a rather simple UI.
It is fully controlled by environment variables, and uses those variables to load the plugin.
Following environment variables are used by this project:

- TENANT_ID is a required variable containing the Azure Tenant ID.
- CLIENT_ID is a required variable containing the clientID of the program accessing MS Graph - it is also created by Azure.
- USE_PROXY is an optional boolean valued variable telling the program is a proxy should be used or not. This variable is per default false,
  and if a proxy is used, this environment variable must contain the string "true" (case sensitive).
- REDIRECT_URL is an optional string variable containing the "redirect URL" registered for this application in Azure. The default value for
  this value is "http://localhost"
- PLUGIN_PATH contains the full path name where the plugin DLL (and it's dependant DLLs) are copied into. I found out that it is best to just
  copy everything into 1 directory (executable and all DLLs from the project and the plugin) and use this path as the plugin path.
  *WANRING:* The plugin DLL filename must be the last part of this environment variable!
- PLUGIN_FACTORY contains the full type name of the concrete factory class used. For this example, the value must be *pluginimpl.PluginMsGraph5Factory*.
