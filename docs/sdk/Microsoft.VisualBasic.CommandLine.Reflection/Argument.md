# Argument
_namespace: [Microsoft.VisualBasic.CommandLine.Reflection](./index.md)_

Use for the detail description for a specific commandline switch.(用于对某一个命令的开关参数的具体描述帮助信息)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.CommandLine.Reflection.Argument.#ctor(System.String,System.Boolean,Microsoft.VisualBasic.CommandLine.Reflection.CLITypes,Microsoft.VisualBasic.CommandLine.Reflection.PipelineTypes)
```
对命令行之中的某一个参数进行描述性信息的创建，包括用法和含义

|Parameter Name|Remarks|
|--------------|-------|
|Name|The name of this command line parameter switch.(该命令开关的名称)|
|Optional|Is this parameter switch is an optional value.(本开关是否为可选的参数)|



### Properties

#### AcceptTypes
Accept these types as input or output data in this types if @``P:Microsoft.VisualBasic.CommandLine.Reflection.Argument.Out`` is true.
#### Description
The description and brief help information about this parameter switch, 
 you can using the \n escape string to gets a VbCrLf value.
 (对这个开关参数的具体的描述以及帮助信息，可以使用\n转义字符进行换行)
#### Example
The usage example of this parameter switch.(该开关的值的示例)
#### Name
The name of this command line parameter switch.(该命令开关的名称)
#### Optional
Is this parameter switch is an optional value.(本开关是否为可选的参数)
#### Out
Is this parameter is using for the output
#### Usage
The usage syntax information about this parameter switch.(本开关参数的使用语法)
