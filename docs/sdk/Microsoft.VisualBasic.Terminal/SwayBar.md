# SwayBar
_namespace: [Microsoft.VisualBasic.Terminal](./index.md)_





### Methods

#### BlankPointer
```csharp
Microsoft.VisualBasic.Terminal.SwayBar.BlankPointer
```
sets the atribute blankPointer with a empty string the same length that the pointer

_returns: A string filled with space characters_

#### ClearBar
```csharp
Microsoft.VisualBasic.Terminal.SwayBar.ClearBar
```
reset the bar to its original state

#### PlacePointer
```csharp
Microsoft.VisualBasic.Terminal.SwayBar.PlacePointer(System.Int32,System.Int32)
```
remove the previous pointer and place it in a new possition

|Parameter Name|Remarks|
|--------------|-------|
|start|start index|
|end|end index|


#### Step
```csharp
Microsoft.VisualBasic.Terminal.SwayBar.Step
```
prints the progress bar acorrding to pointers and current direction


