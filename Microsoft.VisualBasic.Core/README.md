## Framework core runtime of sciBASIC

> 为了能够在其他的项目之间也可以共享这个共享项目里面的方法，请不要引用其他的非.NET框架的类型，只能够使用系统类型
> 
>

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