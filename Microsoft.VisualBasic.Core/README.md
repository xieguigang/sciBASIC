## Framework core runtime of sciBASIC

![](logo.png)

This core runtime consists of three parts:

1. Application services
2. Data extensions
3. Math extensions

The first part of code is trying to help people to build a commandline application using VisualBasic in a more easy way, and the second part of the code is try to help

+ **[ApplicationServices/](./ApplicationServices/)**: Application tools. Most of the class module and the extension function in this namespace is not relevant to the algorithm, it is more relevant to how to build application software.
+ **[CommandLine/](./CommandLine/)**: VB commandline application framework.
+ **[Extensions/](./Extensions/)**: The helper extensions. Includes bunch of math helper, gdi+ helpers, string helpers, file system helpers and the data collection linq extensions api.
+ **[Net/](./Net/)**: Tcp and Http class modules.
+ **[Scripting/](./Scripting/)**: A namespace contains the tools for R language and custom scripting helper extension functions.