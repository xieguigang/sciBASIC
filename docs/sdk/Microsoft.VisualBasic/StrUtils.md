# StrUtils
_namespace: [Microsoft.VisualBasic](./index.md)_





### Methods

#### AddWithDelim
```csharp
Microsoft.VisualBasic.StrUtils.AddWithDelim(System.String,System.String,System.String)
```
Concats two strings with a delimiter.

|Parameter Name|Remarks|
|--------------|-------|
|s1|string 1|
|delim|delimiter|
|s2|string 2|


#### ContactWithDelim
```csharp
Microsoft.VisualBasic.StrUtils.ContactWithDelim(System.Collections.Generic.IEnumerable{System.String},System.String,System.String,System.String)
```
Contacts the items with delim.

|Parameter Name|Remarks|
|--------------|-------|
|items|The items.|
|delim|The delim.|
|initialValue|The initial value.|
|endValue|The end value.|


#### ContactWithDelim``1
```csharp
Microsoft.VisualBasic.StrUtils.ContactWithDelim``1(System.Collections.Generic.IEnumerable{``0},System.String,System.String,System.String)
```
Contacts the items with delim.

|Parameter Name|Remarks|
|--------------|-------|
|items|The items.|
|delim|The delim.|
|initialValue|The initial value.|
|endValue|The end value.|


#### ContactWithDelimSkipEmpty
```csharp
Microsoft.VisualBasic.StrUtils.ContactWithDelimSkipEmpty(System.Collections.Generic.IEnumerable{System.String},System.String)
```
Contact with delim, delim is used after the first not Empty item

|Parameter Name|Remarks|
|--------------|-------|
|items|-|
|delim|-|


#### ContactWithDelimSkipNull
```csharp
Microsoft.VisualBasic.StrUtils.ContactWithDelimSkipNull(System.Collections.Generic.IEnumerable{System.String},System.String)
```
Contact with delim, delim is used after the first not null item

|Parameter Name|Remarks|
|--------------|-------|
|items|-|
|delim|-|


#### ContactWithDelimSkipSome
```csharp
Microsoft.VisualBasic.StrUtils.ContactWithDelimSkipSome(System.Collections.Generic.IEnumerable{System.String},System.String,System.String)
```
Contacts the items with delim skip some.

|Parameter Name|Remarks|
|--------------|-------|
|items|The items.|
|delim|The delim.|
|skip|The skip.|


#### GetHeader
```csharp
Microsoft.VisualBasic.StrUtils.GetHeader(System.String,System.Int32)
```
Gets the header.

|Parameter Name|Remarks|
|--------------|-------|
|text|The text.|
|length|The length.|


#### GetLastSubStringBetween
```csharp
Microsoft.VisualBasic.StrUtils.GetLastSubStringBetween(System.String,System.Char,System.Char)
```
Get the sub string between 'ket' and 'bra'.

|Parameter Name|Remarks|
|--------------|-------|
|text|-|
|bra|-|
|ket|-|


#### GetSubStringBetween
```csharp
Microsoft.VisualBasic.StrUtils.GetSubStringBetween(System.String,System.Char,System.Char)
```
Get the sub string between 'ket' and 'bra'.

|Parameter Name|Remarks|
|--------------|-------|
|text|-|
|bra|-|
|ket|-|


#### GetWords
```csharp
Microsoft.VisualBasic.StrUtils.GetWords(System.String)
```
split text into words by space and newline chars, multiple spaces are treated as a single space.

|Parameter Name|Remarks|
|--------------|-------|
|text|-|


#### LowerCaseFirstChar
```csharp
Microsoft.VisualBasic.StrUtils.LowerCaseFirstChar(System.String)
```
Lowers the case of the first char.

|Parameter Name|Remarks|
|--------------|-------|
|name|The name.|


#### SplitIntoLines
```csharp
Microsoft.VisualBasic.StrUtils.SplitIntoLines(System.String)
```
Splits the text into lines.

|Parameter Name|Remarks|
|--------------|-------|
|text|The text.|


#### SplitWithSeparator
```csharp
Microsoft.VisualBasic.StrUtils.SplitWithSeparator(System.String,System.Char)
```
Split text with a separator char

|Parameter Name|Remarks|
|--------------|-------|
|text|The text.|
|sep|The separator.|


#### SplitWithSeparatorFromRight
```csharp
Microsoft.VisualBasic.StrUtils.SplitWithSeparatorFromRight(System.String,System.Char)
```
Split text with a separator char

|Parameter Name|Remarks|
|--------------|-------|
|text|The text.|
|sep|The separator.|


#### SplitWithSpaces
```csharp
Microsoft.VisualBasic.StrUtils.SplitWithSpaces(System.String)
```
Splits the text with spaces.

|Parameter Name|Remarks|
|--------------|-------|
|text|The text.|


#### StartWithUpperCase
```csharp
Microsoft.VisualBasic.StrUtils.StartWithUpperCase(System.String)
```
Starts with upper case.

|Parameter Name|Remarks|
|--------------|-------|
|name|The name.|


#### UpperCaseFirstChar
```csharp
Microsoft.VisualBasic.StrUtils.UpperCaseFirstChar(System.String)
```
Uppers the case of the first char.

|Parameter Name|Remarks|
|--------------|-------|
|name|The name.|



### Properties

#### InvariantCulture
@``P:System.Globalization.CultureInfo.InvariantCulture``, Gets the System.Globalization.CultureInfo object that is culture-independent
 (invariant).
