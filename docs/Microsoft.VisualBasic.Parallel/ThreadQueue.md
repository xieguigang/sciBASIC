# ThreadQueue
_namespace: [Microsoft.VisualBasic.Parallel](./index.md)_

任务线程队列



### Methods

#### AddToQueue
```csharp
Microsoft.VisualBasic.Parallel.ThreadQueue.AddToQueue(System.Action)
```
Add an Action to the queue.

|Parameter Name|Remarks|
|--------------|-------|
|A|()=>{ .. }|


#### exeQueue
```csharp
Microsoft.VisualBasic.Parallel.ThreadQueue.exeQueue
```
Execute the queue list

#### WaitQueue
```csharp
Microsoft.VisualBasic.Parallel.ThreadQueue.WaitQueue
```
Wait for all thread queue job done.(Needed if you are using multiThreaded queue)


### Properties

#### dummy
lock
#### MultiThreadSupport
If TRUE, the Writing process will be separated from the main thread.
#### MyThread
Writer Thread ☺
#### QSolverRunning
Is thread running?
 hum
#### Queue
Just my queue
