# ReaderProvider
_namespace: [Microsoft.VisualBasic.Data.IO](./index.md)_





### Methods

#### Cleanup
```csharp
Microsoft.VisualBasic.Data.IO.ReaderProvider.Cleanup(Microsoft.VisualBasic.Data.IO.BinaryDataReader)
```
使用这个清理方法来释放@``M:Microsoft.VisualBasic.Data.IO.ReaderProvider.Open``打开的指针

|Parameter Name|Remarks|
|--------------|-------|
|reader|-|


#### Open
```csharp
Microsoft.VisualBasic.Data.IO.ReaderProvider.Open
```
请使用@``M:Microsoft.VisualBasic.Data.IO.ReaderProvider.Cleanup(Microsoft.VisualBasic.Data.IO.BinaryDataReader)``方法来释放资源

#### Read
```csharp
Microsoft.VisualBasic.Data.IO.ReaderProvider.Read(System.Action{Microsoft.VisualBasic.Data.IO.BinaryDataReader})
```


|Parameter Name|Remarks|
|--------------|-------|
|run|
 请不要在这里面执行@``M:System.IO.BinaryReader.Close``或者@``M:System.IO.BinaryReader.Dispose``
 |



