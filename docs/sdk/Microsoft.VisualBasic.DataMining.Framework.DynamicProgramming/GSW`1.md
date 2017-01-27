# GSW`1
_namespace: [Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming](./index.md)_

Generic Smith-Waterman computing kernel.(Smith-Waterman泛型化通用计算核心)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.GSW`1.#ctor(`0[],`0[],Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.ISimilarity{`0},Microsoft.VisualBasic.Text.LevenshteinDistance.ToChar{`0})
```
Public Function @``T:Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.ISimilarity`1``(x As **`T`**, y As **`T`**) As @``T:System.Int32``

|Parameter Name|Remarks|
|--------------|-------|
|query|-|
|subject|-|
|similarity|Blosum matrix or motif similarity|
|asChar|Display alignment|


#### __buildMatrix
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.GSW`1.__buildMatrix
```
Build the score matrix using dynamic programming.
 Note: The indel scores must be negative. Otherwise, the
 part handling the first row and column has to be
 modified.

#### __similarity
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.GSW`1.__similarity(System.Int32,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|i| Position of the character in str1 |
|j| Position of the character in str2 |


_returns:  Cost of substitution of the character in str1 by the one in str2 _

#### GetDPMAT
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.GSW`1.GetDPMAT
```
Gets the dynmaic programming matrix

#### GetTraceback
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.GSW`1.GetTraceback
```
See this overview on implementing Smith-Waterman, including trace back. At each location in the matrix you should check 4 things:
 
 If the location value equals the gap penalty plus the location above, up Is a valid move.
 If the location value equals the gap penalty plus the location left, left Is a valid move.
 If the location value equals the match value plus the location up And left, diagonal Is a valid move.
 If the location value Is 0, you're done.
 
 The first And second options correlate To inserting a gap In one Of the strings, And the third correlates To aligning two characters. 
 If multiple paths work, Then you have multiple possible alignments. 
 
 As the article states, the decision at that point depends largely On context (you have several options).

#### printAlignments
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.GSW`1.printAlignments
```
Output the local alignments with the maximum score.

#### printDPMatrix
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.GSW`1.printDPMatrix
```
print the dynmaic programming matrix

#### traceback
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.GSW`1.traceback(System.Int32,System.Int32)
```
given the bottom right corner point trace back the top left conrner.
 at entry: i, j hold bottom right (end of Aligment coords)
 at return: hold top left (start of Alignment coords)


### Properties

#### AlignmentScore
Get the alignment score between the two input strings.
#### DR_LEFT
Constants of directions.
 Multiple directions are stored by bits.
 The zero direction is the starting point.
#### MATCH_SCORE
The similarity function constants.
 They are amplified by the normalization factor to be integers.
#### Matches
Return a set of Matches idenfied in Dynamic programming matrix. 
 A match is a pair of subsequences whose score is higher than the 
 preset scoreThreshold
#### MaxScore
Get the maximum value in the score matrix.
#### NORM_FACTOR
The normalization factor.
 To get the true score, divide the integer score used in computation
 by the normalization factor.
#### prevCells
The directions pointing to the cells that
 give the maximum score at the current cell.
 The first index is the column index.
 The second index is the row index.
#### query
The first input string
#### queryLength
The lengths of the input strings
#### score
The score matrix.
 The true scores should be divided by the normalization factor.
#### similarity
Compute the similarity score of substitution: use a substitution matrix if the cost model
 The position of the first character is 1.
 A position of 0 represents a gap.
#### subject
The second input String
#### subjectLength
The lengths of the input strings
