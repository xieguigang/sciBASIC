# TimeOutAPI
_namespace: [Microsoft.VisualBasic.Parallel](./index.md)_





### Methods

#### OperationTimeOut
```csharp
Microsoft.VisualBasic.Parallel.TimeOutAPI.OperationTimeOut(System.Action,System.Double)
```


|Parameter Name|Remarks|
|--------------|-------|
|handle|-|
|TimeOut|The time unit of this parameter is second.(单位为秒)|


#### OperationTimeOut``1
```csharp
Microsoft.VisualBasic.Parallel.TimeOutAPI.OperationTimeOut``1(System.Func{``0},``0@,System.Double)
```


|Parameter Name|Remarks|
|--------------|-------|
|handle|-|
|Out|-|
|TimeOut|The time unit of this parameter is second.(单位为秒)|


#### OperationTimeOut``2
```csharp
Microsoft.VisualBasic.Parallel.TimeOutAPI.OperationTimeOut``2(System.Func{``0,``1},``0,``1@,System.Double)
```
The returns value of TRUE represent of the target operation has been time out.(返回真，表示操作超时)

|Parameter Name|Remarks|
|--------------|-------|
|handle|-|
|Out|-|
|TimeOut|The time unit of this parameter is second.(单位为秒)|



