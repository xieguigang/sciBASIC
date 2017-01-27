# ExportAPIAttribute
_namespace: [Microsoft.VisualBasic.CommandLine.Reflection](./index.md)_

A command object that with a specific name.(一个具有特定名称命令执行对象)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.CommandLine.Reflection.ExportAPIAttribute.#ctor(System.String)
```
You are going to define a available export api for you application to another language or scripting program environment.
 (定义一个命令行程序之中可以使用的命令)

|Parameter Name|Remarks|
|--------------|-------|
|Name|The name of the commandline object or you define the exported API name here.(这个命令的名称)|



### Properties

#### Example
A example that to useing this command.
 (对这个命令的使用示例，本属性仅仅是一个助记符，当用户没有编写任何示例信息的时候才会使用本属性的值，
 在编写帮助示例的时候，需要编写出包括命令开关名称的完整的例子)
#### Info
Something detail of help information.(详细的帮助信息)
#### Name
The name of the commandline object.(这个命令的名称)
#### Usage
The usage of this command.(这个命令的用法，本属性仅仅是一个助记符，当用户没有编写任何的使用方法信息的时候才会使用本属性的值)
