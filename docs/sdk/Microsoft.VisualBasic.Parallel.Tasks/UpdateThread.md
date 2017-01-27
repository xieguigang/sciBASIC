# UpdateThread
_namespace: [Microsoft.VisualBasic.Parallel.Tasks](./index.md)_

Running a specific @``T:System.Action`` in the background periodically.
 (比较适合用于在服务器上面执行周期性的计划任务)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Parallel.Tasks.UpdateThread.#ctor(System.Int32,System.Action,System.String)
```
Running a specific action in the background periodically. The time unit of the parameter **`Periods`** is ms or Ticks.

|Parameter Name|Remarks|
|--------------|-------|
|Periods|ms for update thread sleeps|
|updates|-|


#### GetTicks
```csharp
Microsoft.VisualBasic.Parallel.Tasks.UpdateThread.GetTicks(System.Int32,System.Int32,System.Int32)
```
获取得到总的毫秒数

|Parameter Name|Remarks|
|--------------|-------|
|hh|-|
|mm|-|
|ss%|-|


#### Start
```csharp
Microsoft.VisualBasic.Parallel.Tasks.UpdateThread.Start
```
运行这条线程，假若更新线程已经在运行了，则会自动忽略这次调用

#### Stop
```csharp
Microsoft.VisualBasic.Parallel.Tasks.UpdateThread.Stop
```
停止更新线程的运行


### Properties

#### Caller
The caller stack name
#### ErrHandle
If this exception handler is null, then when the unhandled exception occurring,
 this thread object will throw the exception and then stop working.
#### Periods
Sleeps n **ms** interval
#### Running
指示当前的这个任务处理对象是否处于运行状态
