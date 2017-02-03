# TaskQueue`1
_namespace: [Microsoft.VisualBasic.Parallel.Tasks](./index.md)_

这个只有一条线程来执行



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Parallel.Tasks.TaskQueue`1.#ctor
```
会单独启动一条新的线程来用来执行任务队列

#### __taskQueueEXEC
```csharp
Microsoft.VisualBasic.Parallel.Tasks.TaskQueue`1.__taskQueueEXEC
```
有一条线程单独执行这个任务队列

#### Enqueue
```csharp
Microsoft.VisualBasic.Parallel.Tasks.TaskQueue`1.Enqueue(System.Func{`0},System.Action{`0})
```
这个函数只会讲任务添加到队列之中，而不会阻塞线程

|Parameter Name|Remarks|
|--------------|-------|
|handle|-|


#### Join
```csharp
Microsoft.VisualBasic.Parallel.Tasks.TaskQueue`1.Join(System.Func{`0})
```
函数会被插入一个队列之中，之后线程会被阻塞在这里直到函数执行完毕，这个主要是用来控制服务器上面的任务并发的
 一般情况下不会使用这个方法，这个方法主要是控制服务器资源的利用程序的，当线程处于忙碌的状态的时候，
 当前线程会被一直阻塞，直到线程空闲

|Parameter Name|Remarks|
|--------------|-------|
|handle|-|


_returns: 假若本对象已经开始Dispose了，则为完成的任务都会返回Nothing_


### Properties

#### RunningTask
当这个属性为False的时候说明没有任务在执行，此时为空闲状态
#### Tasks
返回当前的任务池之中的任务数量
