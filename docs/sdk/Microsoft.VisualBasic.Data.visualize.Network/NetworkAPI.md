# NetworkAPI
_namespace: [Microsoft.VisualBasic.Data.visualize.Network](./index.md)_





### Methods

#### FromCorrelations
```csharp
Microsoft.VisualBasic.Data.visualize.Network.NetworkAPI.FromCorrelations(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Data.csv.IO.DataSet},System.Collections.Generic.Dictionary{System.String,System.String},System.Collections.Generic.Dictionary{System.String,System.String},System.Double,System.Boolean)
```
变量的属性里面必须是包含有相关度的

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|cut|@``M:System.Math.Abs(System.Double)``|
|trim|Removes the duplicated edges and self loops?|


#### GetConnections
```csharp
Microsoft.VisualBasic.Data.visualize.Network.NetworkAPI.GetConnections(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Data.visualize.Network.FileStream.NetworkEdge},System.String)
```
这个查找函数是忽略掉了方向了的

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|node|-|


#### GetNextConnects
```csharp
Microsoft.VisualBasic.Data.visualize.Network.NetworkAPI.GetNextConnects(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Data.visualize.Network.FileStream.NetworkEdge},System.String)
```
查找To关系的节点边

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|from|-|



