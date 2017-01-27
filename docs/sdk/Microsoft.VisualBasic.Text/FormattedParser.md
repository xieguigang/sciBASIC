# FormattedParser
_namespace: [Microsoft.VisualBasic.Text](./index.md)_

Parser API for the well formatted documents.



### Methods

#### CrossFields
```csharp
Microsoft.VisualBasic.Text.FormattedParser.CrossFields(System.String)
```
Example as: ------- ------ ----- ------- ------ ----- ---- -- -------- -----------

|Parameter Name|Remarks|
|--------------|-------|
|s|-|


#### FieldParser
```csharp
Microsoft.VisualBasic.Text.FormattedParser.FieldParser(System.String,System.Int32[])
```
Parsing a line of string into several fields fragments based on the fields length.

|Parameter Name|Remarks|
|--------------|-------|
|s|The input text line.|
|pos|The text length of each field property value.|


#### FlagSplit
```csharp
Microsoft.VisualBasic.Text.FormattedParser.FlagSplit(System.Collections.Generic.IEnumerable{System.String},System.Func{System.String,System.Boolean})
```
String collection tokens by a certain delimiter string element.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|isFlag|-|


#### ReadHead
```csharp
Microsoft.VisualBasic.Text.FormattedParser.ReadHead(System.String[],System.Int32@,Microsoft.VisualBasic.Text.FormattedParser.DoContinute)
```
Parsing the document head section from the document.

|Parameter Name|Remarks|
|--------------|-------|
|buf|-|
|offset|
 This function will returns the new offset value from this reference parameter.
 (从这里向调用者返回偏移量)
 |
|__isHead|Condition for continue move the parser pointer to the next line.|



