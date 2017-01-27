# NeedlemanWunsch`1
_namespace: [Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming](./index.md)_

Needleman-Wunsch Algorithm
 Bioinformatics 1, WS 15/16
 Dr. Kay Nieselt and Alexander Seitz



### Methods

#### compute
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.NeedlemanWunsch`1.compute
```
computes the matrix for the Needleman-Wunsch Algorithm

#### fillTracebackMatrix
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.NeedlemanWunsch`1.fillTracebackMatrix(System.Int32,System.Int32,System.Int32)
```
return the maximizing cell(s)
 1 , if the maximizing cell is the upper cell
 2 , if the maximizing cell is the left-upper cell
 4 , if the maximizing cell is the left cell
 if there are more than one maximizing cells, the values are summed up

|Parameter Name|Remarks|
|--------------|-------|
|upperLeft|-|
|left|-|
|upper|-|


_returns:  code for the maximizing cell(s) _

#### traceback
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.NeedlemanWunsch`1.traceback(System.Collections.Generic.Stack{`0},System.Collections.Generic.Stack{`0},System.Int32,System.Int32)
```
* this function is called for the first time with two empty stacks
 * and the end indices of the matrix
 * 
 * the function computes a traceback over the matrix, it calls itself recursively
 * for each sequence, it pushes the aligned character (a,c,g,t or -)
 * on a stack (use java.util.Stack with the function push()) 
 *

|Parameter Name|Remarks|
|--------------|-------|
|s1|-|
|s2|-|
|i|-|
|j|-|


#### writeAlignment
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.NeedlemanWunsch`1.writeAlignment(System.String,System.Boolean)
```
This funktion provide a easy way to write a computed alignment into a fasta file

|Parameter Name|Remarks|
|--------------|-------|
|outFile|-|
|single|-|



