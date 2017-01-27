# Task`2
_namespace: [Microsoft.VisualBasic.Parallel.Tasks](./index.md)_

更加底层的线程模式，和LINQ相比不会受到CPU核心数目的限制



### Methods

#### GetValue
```csharp
Microsoft.VisualBasic.Parallel.Tasks.Task`2.GetValue
```
假若后台任务还没有完成，则函数会一直阻塞在这里直到任务执行完毕，假若任务早已完成，则函数会立即返回数据


### Properties

#### Value
假若任务已经完成，则会返回计算值，假若没有完成，则只会返回空值，假若想要在任何情况之下都会得到后台任务所执行的计算结果，请使用@``M:Microsoft.VisualBasic.Parallel.Tasks.Task`2.GetValue``方法
