# BinaryTree`1
_namespace: [Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree](./index.md)_

The Binary tree itself.
 
 A very basic Binary Search Tree. Not generalized, stores
 name/value pairs in the tree nodes. name is the node key.
 The advantage of a binary tree is its fast insert and lookup
 characteristics. This version does not deal with tree balancing.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.BinaryTree`1.#ctor(System.String,`0)
```
初始化有一个根节点

|Parameter Name|Remarks|
|--------------|-------|
|ROOT|-|
|obj|-|


#### Add
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.BinaryTree`1.Add(Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.TreeNode{`0},Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.TreeNode{`0}@,System.Int32)
```
Recursively locates an empty slot in the binary tree and inserts the node

|Parameter Name|Remarks|
|--------------|-------|
|node|-|
|tree|-|
|[overrides]|
 0不复写，函数自动处理
 <0  LEFT
 >0 RIGHT
 |


#### clear
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.BinaryTree`1.clear
```
Clear the binary tree.

#### delete
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.BinaryTree`1.delete(System.String)
```
Delete a given node. This is the more complex method in the binary search
 class. The method considers three senarios, 1) the deleted node has no
 children; 2) the deleted node as one child; 3) the deleted node has two
 children. Case one and two are relatively simple to handle, the only
 unusual considerations are when the node is the root node. Case 3) is
 much more complicated. It requires the location of the successor node.
 The node to be deleted is then replaced by the sucessor node and the
 successor node itself deleted. Throws an exception if the method fails
 to locate the node for deletion.

|Parameter Name|Remarks|
|--------------|-------|
|key|Name key of node to delete|


#### DirectFind
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.BinaryTree`1.DirectFind(System.String)
```
假若节点是不适用标识符来标识自己的左右的位置，则必须要使用这个方法才可以查找成功

|Parameter Name|Remarks|
|--------------|-------|
|name|-|


#### findSuccessor
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.BinaryTree`1.findSuccessor(Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.TreeNode{`0},Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.TreeNode{`0}@)
```
Find the next ordinal node starting at node startNode.
 Due to the structure of a binary search tree, the
 successor node is simply the left most node on the right branch.

|Parameter Name|Remarks|
|--------------|-------|
|startNode|Name key to use for searching|
|parent|Returns the parent node if search successful|


_returns: Returns a reference to the node if successful, else null_

#### FindSymbol
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.BinaryTree`1.FindSymbol(System.String)
```
Find name in tree. Return a reference to the node
 if symbol found else return null to indicate failure.

|Parameter Name|Remarks|
|--------------|-------|
|name|Name of node to locate|


_returns: Returns null if it fails to find the node, else returns reference to node_

#### insert
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.BinaryTree`1.insert(System.String,`0)
```
Add a symbol to the tree if it's a new one. Returns reference to the new
 node if a new node inserted, else returns null to indicate node already present.

|Parameter Name|Remarks|
|--------------|-------|
|name|Name of node to add to tree|
|d|Value of node|


_returns:  Returns reference to the new node is the node was inserted.
 If a duplicate node (same name was located then returns null_

#### ToString
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.BinaryTree`1.ToString
```
Return the tree depicted as a simple string, useful for debugging, eg
 50(40(30(20, 35), 45(44, 46)), 60)

_returns: Returns the tree_


### Properties

#### _counts
Points to the root of the tree
#### Length
Returns the number of nodes in the tree
#### Root
The root of the tree.
