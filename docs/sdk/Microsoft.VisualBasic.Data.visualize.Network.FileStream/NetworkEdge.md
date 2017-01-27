# NetworkEdge
_namespace: [Microsoft.VisualBasic.Data.visualize.Network.FileStream](./index.md)_

The edge between the two nodes in the network.(节点与节点之间的相互关系)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.NetworkEdge.#ctor(Microsoft.VisualBasic.Data.visualize.Network.FileStream.NetworkEdge)
```
Copy value

|Parameter Name|Remarks|
|--------------|-------|
|clone|-|


#### GetConnectedNode
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.NetworkEdge.GetConnectedNode(System.String)
```
假若存在连接则返回相对的节点，否则返回空字符串

|Parameter Name|Remarks|
|--------------|-------|
|Node|-|


#### GetDirectedGuid
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.NetworkEdge.GetDirectedGuid
```
带有方向的互作关系字符串

#### GetNullDirectedGuid
```csharp
Microsoft.VisualBasic.Data.visualize.Network.FileStream.NetworkEdge.GetNullDirectedGuid(System.Boolean)
```
返回没有方向性的统一标识符


### Properties

#### SelfLoop
起始节点是否是终止节点
