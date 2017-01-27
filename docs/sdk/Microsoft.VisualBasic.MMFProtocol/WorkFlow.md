# WorkFlow
_namespace: [Microsoft.VisualBasic.MMFProtocol](./index.md)_





### Methods

#### FolkProc``2
```csharp
Microsoft.VisualBasic.MMFProtocol.WorkFlow.FolkProc``2(System.String,System.String,``0,System.Func{``0,System.Byte[]},System.Func{System.Byte[],``1})
```
创建出一个子进程，然后按照命令行参数**`CLI`**执行制定的命令，同时通过内存映射传递复杂参数，最后结束后通过内存映射传递回数据
 主要是通过内存映射减少数据IO的时间，加快计算流程

|Parameter Name|Remarks|
|--------------|-------|
|exe|-|
|CLI|-|
|[in]|-|
|writer|-|
|reader|-|



