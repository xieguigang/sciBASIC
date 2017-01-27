# DoubleRange
_namespace: [Microsoft.VisualBasic.ComponentModel.Ranges](./index.md)_

Represents a double range with minimum and maximum values



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange.#ctor(System.Double,System.Double)
```
Initializes a new instance of the @``T:Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange`` class

|Parameter Name|Remarks|
|--------------|-------|
|min|Minimum value of the range|
|max|Maximum value of the range|


#### IsInside
```csharp
Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange.IsInside(Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange)
```
Check if the specified range is inside this range

|Parameter Name|Remarks|
|--------------|-------|
|range|Range to check|


_returns: True if the specified range is inside this range or
 false otherwise._

#### IsOverlapping
```csharp
Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange.IsOverlapping(Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange)
```
Check if the specified range overlaps with this range

|Parameter Name|Remarks|
|--------------|-------|
|range|Range to check for overlapping|


_returns: True if the specified range overlaps with this range or
 false otherwise._


### Properties

#### Length
Length of the range (deffirence between maximum and minimum values)
#### Max
Maximum value
#### Min
Minimum value
