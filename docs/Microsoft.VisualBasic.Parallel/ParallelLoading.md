# ParallelLoading
_namespace: [Microsoft.VisualBasic.Parallel](./index.md)_





### Methods

#### DynamicsVBCTask
```csharp
Microsoft.VisualBasic.Parallel.ParallelLoading.DynamicsVBCTask(Microsoft.VisualBasic.Parallel.ParallelLoading.LoadEntry)
```
动态编译

|Parameter Name|Remarks|
|--------------|-------|
|LoadEntry|-|


#### GetReferences
```csharp
Microsoft.VisualBasic.Parallel.ParallelLoading.GetReferences(System.String,System.Int32,Microsoft.VisualBasic.Language.List{System.String}@)
```


|Parameter Name|Remarks|
|--------------|-------|
|url|+特殊符号存在于这个字符串之中的话，函数会出错|
|i|-|


#### IsSystemAssembly
```csharp
Microsoft.VisualBasic.Parallel.ParallelLoading.IsSystemAssembly(System.String,System.Boolean)
```
放在C:\WINDOWS\Microsoft.Net\这个文件夹下面的所有的引用都是本地编译的，哈希值已经不对了

|Parameter Name|Remarks|
|--------------|-------|
|url|-|
|strict|-|


#### Load``1
```csharp
Microsoft.VisualBasic.Parallel.ParallelLoading.Load``1(System.String,System.String)
```
通过与并行进程进行内存共享来传输加载完毕的数据

|Parameter Name|Remarks|
|--------------|-------|
|url|-|
|Process|-|

> 函数会自动从**`T`**

#### SendMessageAPI
```csharp
Microsoft.VisualBasic.Parallel.ParallelLoading.SendMessageAPI(System.Int32)
```
动态编译的加载进程的调用API来向主进程返回消息

|Parameter Name|Remarks|
|--------------|-------|
|Port|-|



