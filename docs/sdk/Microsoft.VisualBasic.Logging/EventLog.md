# EventLog
_namespace: [Microsoft.VisualBasic.Logging](./index.md)_

Provides interaction with Windows event logs.(这个日志入口点对象的创建应该调用于安装程序的模块之中，并且以管理员权限执行)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Logging.EventLog.#ctor(System.String,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|Product|Source|
|Services|-|


#### LogException
```csharp
Microsoft.VisualBasic.Logging.EventLog.LogException(System.Exception,System.Reflection.MethodBase,System.Int32)
```
Writes a localized entry to the event log.

|Parameter Name|Remarks|
|--------------|-------|
|category|A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event.|


#### ToString
```csharp
Microsoft.VisualBasic.Logging.EventLog.ToString
```
$"{@``P:Microsoft.VisualBasic.Logging.EventLog.Services``}//{@``P:Microsoft.VisualBasic.Logging.EventLog.Product``}"

#### WriteEntry
```csharp
Microsoft.VisualBasic.Logging.EventLog.WriteEntry(System.String,System.Reflection.MethodBase,System.Diagnostics.EventLogEntryType,System.Int32)
```
Writes a localized entry to the event log.

|Parameter Name|Remarks|
|--------------|-------|
|EventType|An @``T:System.Diagnostics.EventLogEntryType`` value that indicates the event type.|
|category|A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event.|



