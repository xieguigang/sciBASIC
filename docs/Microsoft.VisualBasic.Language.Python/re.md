# re
_namespace: [Microsoft.VisualBasic.Language.Python](./index.md)_

This module provides regular expression matching operations similar to those found in Perl. ``re`` module in the python language.



### Methods

#### __trimComment
```csharp
Microsoft.VisualBasic.Language.Python.re.__trimComment(System.String)
```
假设所有的注释都是由#和一个空格开始起始的 ``# ``

|Parameter Name|Remarks|
|--------------|-------|
|s|-|


#### FindAll
```csharp
Microsoft.VisualBasic.Language.Python.re.FindAll(System.String,System.String,System.Text.RegularExpressions.RegexOptions)
```
Return all non-overlapping matches of pattern in string, as a list of strings. The string is scanned left-to-right, and matches are returned in the order found. 
 If one or more groups are present in the pattern, return a list of groups; this will be a list of tuples if the pattern has more than one group. 
 Empty matches are included in the result unless they touch the beginning of another match.

|Parameter Name|Remarks|
|--------------|-------|
|pattern|这个会首先被分行然后去除掉python注释|
|input|-|
|options|-|



