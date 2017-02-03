# CLITypes
_namespace: [Microsoft.VisualBasic.CommandLine.Reflection](./index.md)_

The data type enumeration of the target optional parameter switch.




### Properties

#### Boolean
This is a flag value, if this flag presents in the CLI, then this named Boolean value is TRUE, otherwise is FALSE.
#### Double
Real
#### File
File path, is equals most string.(对于指定为路径类型的参数值，在生成命令行的时候会自动调用@``M:Microsoft.VisualBasic.Extensions.CLIPath(System.String)``函数)
#### Integer
Int
#### String
String.(对于指定为字符串类型的参数，在调用的时候回自动调用@``M:Microsoft.VisualBasic.Extensions.CLIToken(System.String)``函数)
