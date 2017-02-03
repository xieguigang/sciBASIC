# ScriptEngine
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_

Math expression script engine.



### Methods

#### AddConstant
```csharp
Microsoft.VisualBasic.Mathematical.ScriptEngine.AddConstant(System.String,System.String)
```
Add constant object

|Parameter Name|Remarks|
|--------------|-------|
|Name|-|
|expr|-|


#### SetVariable
```csharp
Microsoft.VisualBasic.Mathematical.ScriptEngine.SetVariable(System.String,System.String)
```
Set variable value

|Parameter Name|Remarks|
|--------------|-------|
|Name|-|
|expr|-|


#### Shell
```csharp
Microsoft.VisualBasic.Mathematical.ScriptEngine.Shell(System.String)
```
Run the simple script that stores in the @``P:Microsoft.VisualBasic.Mathematical.ScriptEngine.Scripts`` table.

|Parameter Name|Remarks|
|--------------|-------|
|statement|-|



### Properties

#### Expression
The default expression engine.
#### Scripts
Lambda expression table.
#### StatementEngine
all of the commands are stored at here
