# Extensions
_namespace: [Microsoft.VisualBasic.Data.csv.Excel](./index.md)_





### Methods

#### __updateMaps
```csharp
Microsoft.VisualBasic.Data.csv.Excel.Extensions.__updateMaps(Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame@,System.String)
```
仅限于 @``T:Microsoft.VisualBasic.Data.csv.DocumentStream.DataSet``和@``T:Microsoft.VisualBasic.Data.csv.DocumentStream.EntityObject``

|Parameter Name|Remarks|
|--------------|-------|
|df|-|
|mapName|-|


#### LoadDataSet
```csharp
Microsoft.VisualBasic.Data.csv.Excel.Extensions.LoadDataSet(System.String,System.String,System.String)
```
@``T:Microsoft.VisualBasic.Data.csv.DocumentStream.DataSet``

|Parameter Name|Remarks|
|--------------|-------|
|file|-|
|sheetName|-|
|uidMaps|-|


#### LoadEntitySet
```csharp
Microsoft.VisualBasic.Data.csv.Excel.Extensions.LoadEntitySet(System.String,System.String,System.String)
```
@``T:Microsoft.VisualBasic.Data.csv.DocumentStream.EntityObject``

|Parameter Name|Remarks|
|--------------|-------|
|file|-|
|sheetName|-|
|uidMaps|-|


#### ReadXlsx
```csharp
Microsoft.VisualBasic.Data.csv.Excel.Extensions.ReadXlsx(System.String,System.String)
```
Load data frame from the excel file.

|Parameter Name|Remarks|
|--------------|-------|
|file$|*.xlsx file path.|
|sheetName|Table name|


#### ReadXlsx``1
```csharp
Microsoft.VisualBasic.Data.csv.Excel.Extensions.ReadXlsx``1(System.String,System.String,System.Collections.Generic.Dictionary{System.String,System.String})
```


|Parameter Name|Remarks|
|--------------|-------|
|file|-|
|sheetName|-|
|maps|Field(Csv) -> Class.Property Name|



