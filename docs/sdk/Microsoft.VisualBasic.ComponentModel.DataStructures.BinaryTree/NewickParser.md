# NewickParser
_namespace: [Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree](./index.md)_

http://www.evolgenius.info/evolview/



### Methods

#### __makeInternalNode``1
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.NewickParser.__makeInternalNode``1(System.String,System.Boolean,Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.TreeNode{``0}@)
```
Dec 5, 2011; can be used to make rootnode

|Parameter Name|Remarks|
|--------------|-------|
|id|-|
|isroot|-|
|parentnode|-|


#### parseInforAndMakeNewLeafNode``1
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.NewickParser.parseInforAndMakeNewLeafNode``1(System.String,System.Collections.Generic.Dictionary{System.String,System.String},Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.TreeNode{``0})
```
created on Oct 20, 2013 
 input: the leafstr to be parsed, the internal node the leaf node has to be added to

#### TreeParser``1
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.NewickParser.TreeParser``1(System.String,System.Collections.Generic.Dictionary{System.String,System.String},Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree.TreeNode{``0}@)
```
created: Oct 20, 2013 : a better and easier to maintain parser for newick and nexus trees
 NOTE: this is a recursive function

|Parameter Name|Remarks|
|--------------|-------|
|inputstr| : input tree string |
|hashTranslate| : aliases for lead nodes (for nexsus format) |
|iNode| : current internal node; == rootNode the first time 'newickParser' is called  |



