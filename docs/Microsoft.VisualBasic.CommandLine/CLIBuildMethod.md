# CLIBuildMethod
_namespace: [Microsoft.VisualBasic.CommandLine](./index.md)_





### Methods

#### __booleanRule
```csharp
Microsoft.VisualBasic.CommandLine.CLIBuildMethod.__booleanRule(System.Object,Microsoft.VisualBasic.CommandLine.Reflection.Optional,System.Reflection.PropertyInfo)
```
Property value to boolean flag in the CLI

|Parameter Name|Remarks|
|--------------|-------|
|value|-|
|attr|-|
|prop|-|


#### __pathRule
```csharp
Microsoft.VisualBasic.CommandLine.CLIBuildMethod.__pathRule(System.Object,Microsoft.VisualBasic.CommandLine.Reflection.Optional,System.Reflection.PropertyInfo)
```
The different between the String and Path is that applying @``M:Microsoft.VisualBasic.Extensions.CLIToken(System.String)`` or @``M:Microsoft.VisualBasic.Extensions.CLIPath(System.String)``.

|Parameter Name|Remarks|
|--------------|-------|
|value|只能是@``T:System.String``类型的|
|attr|-|
|prop|-|


#### __stringRule
```csharp
Microsoft.VisualBasic.CommandLine.CLIBuildMethod.__stringRule(System.Object,Microsoft.VisualBasic.CommandLine.Reflection.Optional,System.Reflection.PropertyInfo)
```
可能包含有枚举值

|Parameter Name|Remarks|
|--------------|-------|
|value|-|
|attr|-|
|prop|-|


#### ClearParameters``1
```csharp
Microsoft.VisualBasic.CommandLine.CLIBuildMethod.ClearParameters``1(``0)
```
Reset the CLI parameters property in the target class object.

|Parameter Name|Remarks|
|--------------|-------|
|inst|-|


_returns: 返回所重置的参数的个数_

#### GetCLI``1
```csharp
Microsoft.VisualBasic.CommandLine.CLIBuildMethod.GetCLI``1(``0)
```
Generates the command line string value for the invoked target cli program using this interop services object instance.
 (生成命令行参数)

|Parameter Name|Remarks|
|--------------|-------|
|app|目标交互对象的实例|

> 
>  依照类型@``T:Microsoft.VisualBasic.CommandLine.Reflection.CLITypes``来生成参数字符串
>  
>  @``F:Microsoft.VisualBasic.CommandLine.Reflection.CLITypes.Boolean``, True => 参数名；
>  @``F:Microsoft.VisualBasic.CommandLine.Reflection.CLITypes.Double``, @``F:Microsoft.VisualBasic.CommandLine.Reflection.CLITypes.Integer``, @``F:Microsoft.VisualBasic.CommandLine.Reflection.CLITypes.String``, => 参数名 + 参数值，假若字符串为空则不添加；
>  （假若是枚举值类型，可能还需要再枚举值之中添加@``T:System.ComponentModel.DescriptionAttribute``属性）
>  @``F:Microsoft.VisualBasic.CommandLine.Reflection.CLITypes.File``, 假若字符串为空则不添加，有空格自动添加双引号，相对路径会自动转换为全路径。
>  

#### SimpleBuilder
```csharp
Microsoft.VisualBasic.CommandLine.CLIBuildMethod.SimpleBuilder(System.String,System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}})
```
Creates a command line string by using simply fills the name and parameter values

|Parameter Name|Remarks|
|--------------|-------|
|name|-|
|args|-|



### Properties

#### __getMethods
Converts the property value to a CLI token
