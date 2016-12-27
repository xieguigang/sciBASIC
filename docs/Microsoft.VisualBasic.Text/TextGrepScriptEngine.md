# TextGrepScriptEngine
_namespace: [Microsoft.VisualBasic.Text](./index.md)_

A script object for grep the gene id in the blast output query and subject title.(用于解析基因名称的脚本类，这个对象是在项目的初始阶段，为了方便命令行操作而设置的)



### Methods

#### Compile
```csharp
Microsoft.VisualBasic.Text.TextGrepScriptEngine.Compile(System.String)
```
对用户所输入的脚本进行编译，对于内部的空格，请使用单引号'进行分割

#### Grep
```csharp
Microsoft.VisualBasic.Text.TextGrepScriptEngine.Grep(System.String)
```
修整目标字符串，按照脚本之中的方法取出所需要的字符串信息

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### MidString
```csharp
Microsoft.VisualBasic.Text.TextGrepScriptEngine.MidString(System.String,System.String[])
```


|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|ScriptTokens|向量之中的第一个元素为命令的名字，第二个元素为Mid函数的Start参数，第三个元素为Mid函数的Length参数，可以被忽略掉|


#### Tokens
```csharp
Microsoft.VisualBasic.Text.TextGrepScriptEngine.Tokens(System.String,System.String[])
```


|Parameter Name|Remarks|
|--------------|-------|
|Source|-|
|Script|-|



### Properties

#### _Operations
Source,Script,ReturnValue
#### Method
字符串剪裁操作的函数指针
