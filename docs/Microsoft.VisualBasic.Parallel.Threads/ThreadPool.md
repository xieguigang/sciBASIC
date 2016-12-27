# ThreadPool
_namespace: [Microsoft.VisualBasic.Parallel.Threads](./index.md)_

使用多条线程来执行任务队列，推荐在编写Web服务器的时候使用这个模块来执行任务



### Methods

#### GetAvaliableThread
```csharp
Microsoft.VisualBasic.Parallel.Threads.ThreadPool.GetAvaliableThread
```
这个函数总是会返回一个线程对象的
 
 + 当有空闲的线程，会返回第一个空闲的线程
 + 当没有空闲的线程，则会返回任务队列最短的线程

#### RunTask
```csharp
Microsoft.VisualBasic.Parallel.Threads.ThreadPool.RunTask(System.Action,System.Action{System.Int64})
```
使用线程池里面的空闲线程来执行任务

|Parameter Name|Remarks|
|--------------|-------|
|task|-|
|callback|回调函数里面的参数是任务的执行的时间长度|



### Properties

#### __pendings
临时的句柄缓存
#### FullCapacity
是否所有的线程都是处于工作状态的
#### NumOfThreads
线程池之中的线程数量
#### WorkingThreads
返回当前正在处于工作状态的线程数量
