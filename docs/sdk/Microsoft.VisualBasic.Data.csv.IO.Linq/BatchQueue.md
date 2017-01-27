# BatchQueue
_namespace: [Microsoft.VisualBasic.Data.csv.IO.Linq](./index.md)_





### Methods

#### IteratesAll``1
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.BatchQueue.IteratesAll``1(System.Collections.Generic.IEnumerable{System.String},Microsoft.VisualBasic.Text.Encodings)
```
Reads all data in the directory as a single data source.

|Parameter Name|Remarks|
|--------------|-------|
|files|Csv files list|
|encoding|-|


#### ReadQueue``1
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.BatchQueue.ReadQueue``1(System.Collections.Generic.IEnumerable{System.String},Microsoft.VisualBasic.Text.Encodings)
```
{@``M:System.IO.Path.GetFileNameWithoutExtension(System.String)``, **`T`**()}

|Parameter Name|Remarks|
|--------------|-------|
|files|-|

> 
>  在服务器上面可能会出现IO很慢的情况，这个时候可以试一下这个函数进行批量数据加载
>  

#### RequestData``1
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.BatchQueue.RequestData``1(System.String,Microsoft.VisualBasic.Text.Encodings)
```
函数会自动处理文件或者文件夹的情况

|Parameter Name|Remarks|
|--------------|-------|
|handle$|-|
|encoding|-|


#### RequestFiles``1
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.BatchQueue.RequestFiles``1(System.String,Microsoft.VisualBasic.Text.Encodings)
```
函数会自动处理文件或者文件夹的情况

|Parameter Name|Remarks|
|--------------|-------|
|handle$|-|
|encoding|-|



