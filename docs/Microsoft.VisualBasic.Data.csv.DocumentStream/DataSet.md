# DataSet
_namespace: [Microsoft.VisualBasic.Data.csv.DocumentStream](./index.md)_

The numeric dataset, @``T:Microsoft.VisualBasic.ComponentModel.DataSourceModel.DynamicPropertyBase`1``, @``T:System.Double``.
 (数值类型的数据集合，每一个数据实体对象都有自己的编号以及数据属性)



### Methods

#### Copy
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataSet.Copy
```
Copy prop[erty value

#### LoadDataSet
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataSet.LoadDataSet(System.String,System.String)
```
**`uidMap`**一般情况下会自动进行判断，不需要具体的设置

|Parameter Name|Remarks|
|--------------|-------|
|path|-|
|uidMap|
 默认是使用csv文件的第一行第一个单元格中的内容作为标识符，但是有时候可能标识符不是在第一列的，则这个时候就需要对这个参数进行赋值了
 |



