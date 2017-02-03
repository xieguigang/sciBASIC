# AsyncHandle`1
_namespace: [Microsoft.VisualBasic.Parallel.Tasks](./index.md)_

Represents the status of an asynchronous operation.(背景线程加载数据)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Parallel.Tasks.AsyncHandle`1.#ctor(System.Func{`0})
```
Creates a new background task from a function handle.

|Parameter Name|Remarks|
|--------------|-------|
|Task|-|


#### GetValue
```csharp
Microsoft.VisualBasic.Parallel.Tasks.AsyncHandle`1.GetValue
```
没有完成会一直阻塞线程在这里

#### Run
```csharp
Microsoft.VisualBasic.Parallel.Tasks.AsyncHandle`1.Run
```
Start the background task thread.(启动后台背景线程)


### Properties

#### IsCompleted
Gets a value that indicates whether the asynchronous operation has completed.
