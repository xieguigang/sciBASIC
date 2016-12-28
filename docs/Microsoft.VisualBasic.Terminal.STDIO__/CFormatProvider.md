# CFormatProvider
_namespace: [Microsoft.VisualBasic.Terminal.STDIO__](./index.md)_

Provides C like format print

> https://github.com/mlnlover11/SharpLua


### Methods

#### IsPositive
```csharp
Microsoft.VisualBasic.Terminal.STDIO__.CFormatProvider.IsPositive(System.Object,System.Boolean)
```
Determines whether the specified value is positive.

|Parameter Name|Remarks|
|--------------|-------|
|Value|The value.|
|ZeroIsPositive|if set to true treats 0 as positive.|


_returns: 
 true if the specified value is positive; otherwise, false.
 _

#### ReplaceMetaChars
```csharp
Microsoft.VisualBasic.Terminal.STDIO__.CFormatProvider.ReplaceMetaChars(System.String)
```
Replaces the string representations of meta chars with their corresponding
 character values.

|Parameter Name|Remarks|
|--------------|-------|
|input|The input.|


_returns: A string with all string meta chars are replaced_

#### sprintf
```csharp
Microsoft.VisualBasic.Terminal.STDIO__.CFormatProvider.sprintf(System.String,System.Object[])
```
Format string like C

|Parameter Name|Remarks|
|--------------|-------|
|Format|-|
|Parameters|-|


#### ToInteger
```csharp
Microsoft.VisualBasic.Terminal.STDIO__.CFormatProvider.ToInteger(System.Object,System.Boolean)
```
Converts the specified values boxed type to its correpsonding integer
 type.

|Parameter Name|Remarks|
|--------------|-------|
|Value|The value.|


_returns: A boxed numeric object whos type is an integer type._

#### ToUnsigned
```csharp
Microsoft.VisualBasic.Terminal.STDIO__.CFormatProvider.ToUnsigned(System.Object)
```
Converts the specified values boxed type to its correpsonding unsigned
 type.

|Parameter Name|Remarks|
|--------------|-------|
|Value|The value.|


_returns: A boxed numeric object whos type is unsigned._


### Properties

#### Formats
%[parameter][flags][width][.precision][length]type
