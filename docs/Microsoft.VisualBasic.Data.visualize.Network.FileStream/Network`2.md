# Network`2
_namespace: [Microsoft.VisualBasic.Data.visualize.Network.FileStream](./index.md)_

The network csv data information with specific type of the datamodel



### Methods

#### op_Addition
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network`2.op_Addition(Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network{`0,`1},System.Collections.Generic.IEnumerable{`1})
```


|Parameter Name|Remarks|
|--------------|-------|
|net|-|
|x|由于会调用ToArray，所以这里建议使用Iterator|


#### op_Concatenate
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network`2.op_Concatenate(Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network{`0,`1},System.String)
```
GET node

|Parameter Name|Remarks|
|--------------|-------|
|net|-|
|node|-|


#### op_Exponent
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network`2.op_Exponent(Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network{`0,`1},`0)
```
Network contains node?

|Parameter Name|Remarks|
|--------------|-------|
|net|-|
|node|-|


#### op_LessThanOrEqual
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network`2.op_LessThanOrEqual(Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network{`0,`1},System.Collections.Generic.IEnumerable{System.String})
```
Select nodes from the network based on the input identifers **`nodes`**

|Parameter Name|Remarks|
|--------------|-------|
|net|-|
|nodes|-|


#### RemoveDuplicated
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network`2.RemoveDuplicated
```
移除的重复的边

#### RemoveSelfLoop
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network`2.RemoveSelfLoop
```
移除自身与自身的边

#### Save
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network`2.Save(System.String,System.Text.Encoding)
```


|Parameter Name|Remarks|
|--------------|-------|
|outDIR|The data directory for the data export, if the value of this directory is null then the data
 will be exported at the current work directory.
 (进行数据导出的文件夹，假若为空则会保存数据至当前的工作文件夹之中)|
|encoding|The file encoding of the exported node and edge csv file.|



