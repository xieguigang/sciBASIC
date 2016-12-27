# BatchTasks
_namespace: [Microsoft.VisualBasic.Parallel.Threads](./index.md)_

Parallel batch task tool for processor



### Methods

#### BatchTask``1
```csharp
Microsoft.VisualBasic.Parallel.Threads.BatchTasks.BatchTask``1(System.Func{``0}[],System.Int32,System.Int32,System.Double)
```
Using parallel linq that may stuck the program when a linq task partion wait a long time task to complete. 
 By using this parallel function that you can avoid this problem from parallel linq, and also you can 
 controls the task thread number manually by using this parallel task function.
 (由于LINQ是分片段来执行的，当某个片段有一个线程被卡住之后整个进程都会被卡住，所以执行大型的计算任务的时候效率不太好，
 使用这个并行化函数可以避免这个问题，同时也可以自己手动控制线程的并发数)

|Parameter Name|Remarks|
|--------------|-------|
|actions|Tasks collection|
|numThreads|
 You can controls the parallel tasks number from this parameter, smaller or equals to ZERO means auto 
 config the thread number, If want single thread, not parallel, set this value to 1, and positive 
 value greater than 1 will makes the tasks parallel.
 (可以在这里手动的控制任务的并发数，这个数值小于或者等于零则表示自动配置线程的数量)
 |
|TimeInterval|The task run loop sleep time, unit is **ms**|
|smart|
 ZERO or negative value will turn off this smart mode, default value is ZERO, mode was turn off.
 If this parameter value is set to any positive value, that means this smart mode will be turn on.
 then, if the CPU load is higher than the value of this parameter indicated, then no additional 
 task thread would be added, if CPU load lower than this parameter value, then some additional 
 task thread will be added for utilize the CPU resources and save the computing time. 
 (假若开启smart模式的话，在CPU负载较高的时候会保持在限定的线程数量来执行批量任务，
 假若CPU的负载较低的话，则会开启超量的线程，以保持执行效率充分利用计算资源来节省总任务的执行时间
 任意正实数都将会开启smart模式
 小于等于零的数将不会开启，默认值为零，不开启)
 |


#### BatchTask``2
```csharp
Microsoft.VisualBasic.Parallel.Threads.BatchTasks.BatchTask``2(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1},System.Int32,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|getTask|-|
|numThreads|可以在这里手动的控制任务的并发数，这个数值小于或者等于零则表示自动配置线程的数量，如果想要单线程，请将这个参数设置为1|
|TimeInterval|-|



