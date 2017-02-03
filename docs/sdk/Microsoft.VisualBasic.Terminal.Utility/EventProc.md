# EventProc
_namespace: [Microsoft.VisualBasic.Terminal.Utility](./index.md)_

Generates the task progress for the console output.(处理任务进度)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Terminal.Utility.EventProc.#ctor(System.Int32,System.String,System.IO.StreamWriter)
```


|Parameter Name|Remarks|
|--------------|-------|
|n|-|
|tag|-|
|out|Default is @``T:System.Console``|


#### Tick
```csharp
Microsoft.VisualBasic.Terminal.Utility.EventProc.Tick
```
会自动输出进度的

#### ToString
```csharp
Microsoft.VisualBasic.Terminal.Utility.EventProc.ToString
```
Generates progress output


### Properties

#### Capacity
The total @``M:Microsoft.VisualBasic.Terminal.Utility.EventProc.Tick``
#### percentage
Current progress percentage.
#### preElapsedMilliseconds
Previous @``P:System.Diagnostics.Stopwatch.ElapsedMilliseconds``
