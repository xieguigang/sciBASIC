# RegexExtensions
_namespace: [Microsoft.VisualBasic](./index.md)_





### Methods

#### EachValue
```csharp
Microsoft.VisualBasic.RegexExtensions.EachValue(System.Text.RegularExpressions.MatchCollection)
```
Each match its value in the source match collection.

|Parameter Name|Remarks|
|--------------|-------|
|m|-|


#### IsPattern
```csharp
Microsoft.VisualBasic.RegexExtensions.IsPattern(System.String,System.String,System.Text.RegularExpressions.RegexOptions)
```
The enitre string input equals to the pattern's matched.

|Parameter Name|Remarks|
|--------------|-------|
|s|-|
|pattern|-|


#### ToArray
```csharp
Microsoft.VisualBasic.RegexExtensions.ToArray(System.Text.RegularExpressions.MatchCollection)
```
Gets the matched strings from the regex match result as source

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### ToArray``1
```csharp
Microsoft.VisualBasic.RegexExtensions.ToArray``1(System.Text.RegularExpressions.MatchCollection,System.Func{System.String,``0})
```
Converts the regex string match results to the objects.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|[CType]|The object parser|



