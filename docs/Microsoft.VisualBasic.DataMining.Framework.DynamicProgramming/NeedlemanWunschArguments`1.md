# NeedlemanWunschArguments`1
_namespace: [Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming](./index.md)_

Base class for the Needleman-Wunsch Algorithm
 Bioinformatics 1, WS 15/16
 Dr. Kay Nieselt and Alexander Seitz



### Methods

#### addAligned1
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.NeedlemanWunschArguments`1.addAligned1(`0[])
```
set aligned sequence 1

|Parameter Name|Remarks|
|--------------|-------|
|aligned1|-|


#### addAligned2
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.NeedlemanWunschArguments`1.addAligned2(`0[])
```
set aligned sequence 2

|Parameter Name|Remarks|
|--------------|-------|
|aligned2|-|


#### getAligned1
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.NeedlemanWunschArguments`1.getAligned1(System.Int32)
```
get aligned version of sequence 1

|Parameter Name|Remarks|
|--------------|-------|
|i|-|


_returns:   aligned sequence 1 _

#### getAligned2
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.NeedlemanWunschArguments`1.getAligned2(System.Int32)
```
get aligned version of sequence 2

|Parameter Name|Remarks|
|--------------|-------|
|i|-|


_returns:  aligned sequence 2 _

#### match
```csharp
Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming.NeedlemanWunschArguments`1.match(`0,`0)
```
if char a is equal to char b
 return the match score
 else return mismatch score


### Properties

#### GapPenalty
get gap open penalty
#### MatchScore
get match score
#### MismatchScore
get mismatch score
#### NumberOfAlignments
get numberOfAlignments
#### Score
get computed score
#### Sequence1
get sequence 1
#### Sequence2
get sequence 2cted int max (int a, int b, int c) {
 return Math.max(a, Math.max(b, c));
