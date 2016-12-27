# StringHelpers
_namespace: [Microsoft.VisualBasic](./index.md)_

The extensions module for facilities the string operations.



### Methods

#### Count
```csharp
Microsoft.VisualBasic.StringHelpers.Count(System.String,System.Char)
```
Counts the specific char that appeared in the input string.
 (计数在字符串之中所出现的指定的字符的出现的次数)

|Parameter Name|Remarks|
|--------------|-------|
|str|-|
|ch|-|


#### CountTokens
```csharp
Microsoft.VisualBasic.StringHelpers.CountTokens(System.Collections.Generic.IEnumerable{System.String},System.Boolean)
```
Count the string value numbers.(请注意，这个函数是倒序排序的)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### EqualsAny
```csharp
Microsoft.VisualBasic.StringHelpers.EqualsAny(System.String,System.String[])
```
判断目标字符串是否与字符串参数数组之中的任意一个字符串相等，大小写不敏感，假若没有相等的字符串，则会返回空字符串，假若找到了相等的字符串，则会返回该字符串

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|compareTo|-|


#### FormatNumeric
```csharp
Microsoft.VisualBasic.StringHelpers.FormatNumeric(System.Double,System.Int32)
```
分别处理正常的小数或者科学记数法的小数

|Parameter Name|Remarks|
|--------------|-------|
|n#|-|
|decimal%|-|


#### FormatZero``1
```csharp
Microsoft.VisualBasic.StringHelpers.FormatZero``1(``0,System.String)
```
在字符串前面填充指定长度的00序列，假若输入的字符串长度大于fill的长度，则不再进行填充

|Parameter Name|Remarks|
|--------------|-------|
|n|-|
|fill|-|


#### GetString
```csharp
Microsoft.VisualBasic.StringHelpers.GetString(System.String,System.Char)
```
获取""或者其他字符所包围的字符串的值，请注意，假若只有一个**`wrapper`**的话，字符串将不会进行任何处理

|Parameter Name|Remarks|
|--------------|-------|
|s|-|
|wrapper|-|


#### GetTagValue
```csharp
Microsoft.VisualBasic.StringHelpers.GetTagValue(System.String,System.String,System.Boolean,System.Boolean)
```
tagName{**`delimiter`**}value

|Parameter Name|Remarks|
|--------------|-------|
|s|-|
|delimiter|-|
|trim|Needs Removes all leading and trailing white-space characters from 
 the current @``T:System.String`` object.|


#### InStrAny
```csharp
Microsoft.VisualBasic.StringHelpers.InStrAny(System.String,System.String[])
```
查找到任意一个既返回位置，大小写不敏感，假若查找不到，则返回-1值，判断是否查找成功，可以使用 <0 来完成，
 因为是通过InStr来完成的，所以查找成功的时候，最小的值是1，即字符串序列的第一个位置，也是元素0位置

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|find|-|


#### Intersection
```csharp
Microsoft.VisualBasic.StringHelpers.Intersection(System.String[][])
```
求交集

#### IsBlank
```csharp
Microsoft.VisualBasic.StringHelpers.IsBlank(System.String)
```
**`s`** Is Nothing, @``M:System.String.IsNullOrEmpty(System.String)``, @``M:System.String.IsNullOrWhiteSpace(System.String)``

|Parameter Name|Remarks|
|--------------|-------|
|s|The input test string|


#### IsNullOrEmpty
```csharp
Microsoft.VisualBasic.StringHelpers.IsNullOrEmpty(System.Collections.Generic.IEnumerable{System.String},System.Boolean)
```
判断这个字符串集合是否为空集合，函数会首先按照常规的集合为空进行判断，然后假若不为空的话，假若只含有一个元素并且该唯一的元素的值为空字符串，则也认为这个字符串集合为空集合

|Parameter Name|Remarks|
|--------------|-------|
|values|-|
|strict|FALSE 为非严谨，只进行常规判断，TRUE 为严谨模式，会假若不为空的话，假若只含有一个元素并且该唯一的元素的值为空字符串，则也认为这个字符串集合为空集合|


#### Located
```csharp
Microsoft.VisualBasic.StringHelpers.Located(System.Collections.Generic.IEnumerable{System.String},System.String,System.Boolean,System.Boolean)
```
String compares using @``T:System.String``, if the target value could not be located, 
 then -1 will be return from this function.

|Parameter Name|Remarks|
|--------------|-------|
|collection|-|
|text|-|
|caseSensitive|-|
|fuzzy|
 If fuzzy, then @``M:Microsoft.VisualBasic.Strings.InStr(System.Int32,System.String,System.String,Microsoft.VisualBasic.CompareMethod)`` will be used if ``String.Equals`` method have no result.
 |


#### Lookup
```csharp
Microsoft.VisualBasic.StringHelpers.Lookup(System.Collections.Generic.IEnumerable{System.String},System.String,System.Boolean)
```
Search the string by keyword in a string collection. Unlike search function @``M:Microsoft.VisualBasic.StringHelpers.Located(System.Collections.Generic.IEnumerable{System.String},System.String,System.Boolean,System.Boolean)``
 using function @``T:System.String`` function to search string, this function using @``M:Microsoft.VisualBasic.Strings.InStr(System.String,System.String,Microsoft.VisualBasic.CompareMethod)``
 to search the keyword.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|keyword|-|
|caseSensitive|-|


_returns: 返回第一个找到关键词的行数，没有找到则返回-1_

#### lTokens
```csharp
Microsoft.VisualBasic.StringHelpers.lTokens(System.String,System.Boolean)
```
Line tokens. **=> Parsing the text into lines by using @``F:Microsoft.VisualBasic.Constants.vbCr``, @``F:Microsoft.VisualBasic.Constants.vbLf``**.
 (函数对文本进行分行操作，由于在Windows(@``F:Microsoft.VisualBasic.Constants.vbCrLf``)和
 Linux(@``F:Microsoft.VisualBasic.Constants.vbCr``, @``F:Microsoft.VisualBasic.Constants.vbLf``)平台上面所生成的文本文件的换行符有差异，
 所以可以使用这个函数来进行统一的分行操作)

|Parameter Name|Remarks|
|--------------|-------|
|__text|-|
|trim|
 Set @``F:System.Boolean.FalseString`` to avoid a reader bug in the csv data reader @``T:Microsoft.VisualBasic.ComponentModel.BufferedStream``
 |


#### Match
```csharp
Microsoft.VisualBasic.StringHelpers.Match(System.Text.RegularExpressions.Match,System.String,System.Text.RegularExpressions.RegexOptions)
```
Get regex match value from the target input string.

|Parameter Name|Remarks|
|--------------|-------|
|input|-|
|pattern|-|
|options|-|


#### Matched
```csharp
Microsoft.VisualBasic.StringHelpers.Matched(System.String,System.String,System.Text.RegularExpressions.RegexOptions)
```
Does this input string is matched by the specific regex expression?

|Parameter Name|Remarks|
|--------------|-------|
|str|-|
|regex|-|


#### Parts
```csharp
Microsoft.VisualBasic.StringHelpers.Parts(System.String,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|s|-|
|len|-|


#### RemoveLast
```csharp
Microsoft.VisualBasic.StringHelpers.RemoveLast(System.Text.StringBuilder)
```
Call @``M:System.Text.StringBuilder.Remove(System.Int32,System.Int32)``(@``P:System.Text.StringBuilder.Length`` - 1, 1) for removes the last character in the string sequence.

|Parameter Name|Remarks|
|--------------|-------|
|s|-|


#### RepeatString
```csharp
Microsoft.VisualBasic.StringHelpers.RepeatString(System.String,System.Int32)
```
this is to emulate what's evailable in PHP

#### Reverse
```csharp
Microsoft.VisualBasic.StringHelpers.Reverse(System.String)
```
Returns a reversed version of String s.

|Parameter Name|Remarks|
|--------------|-------|
|s|-|


#### SaveTo
```csharp
Microsoft.VisualBasic.StringHelpers.SaveTo(System.Collections.Generic.IDictionary{System.String,System.String},System.String)
```
Save this string dictionary object as json file.

|Parameter Name|Remarks|
|--------------|-------|
|dict|-|
|path|-|


#### sFormat
```csharp
Microsoft.VisualBasic.StringHelpers.sFormat(System.String,System.Object[])
```
Replaces the format item in a specified string with the string representation
 of a corresponding object in a specified array.

|Parameter Name|Remarks|
|--------------|-------|
|s|A composite format string.|
|args|An object array that contains zero or more objects to format.|


_returns: A copy of format in which the format items have been replaced by the string representation
 of the corresponding objects in args._

#### Split
```csharp
Microsoft.VisualBasic.StringHelpers.Split(System.Collections.Generic.IEnumerable{System.String},System.String,System.Boolean)
```
String collection tokens by a certain delimiter string element.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|delimiter|
 Using ``String.Equals`` or Regular expression function to determined this delimiter 
 |


#### StringSplit
```csharp
Microsoft.VisualBasic.StringHelpers.StringSplit(System.String,System.String,System.Boolean)
```
This method is used to replace most calls to the Java String.split method.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|regexDelimiter|-|
|trimTrailingEmptyStrings|-|


#### TextEquals
```csharp
Microsoft.VisualBasic.StringHelpers.TextEquals(System.String,System.String)
```
Shortcuts for method @``M:System.Object.Equals(System.Object,System.Object)``(s1, s2, @``F:System.StringComparison.OrdinalIgnoreCase``)

|Parameter Name|Remarks|
|--------------|-------|
|s1|-|
|s2|-|


#### WildcardsLocated
```csharp
Microsoft.VisualBasic.StringHelpers.WildcardsLocated(System.Collections.Generic.IEnumerable{System.String},System.String,System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|collection|-|
|text|可以使用通配符|
|caseSensitive|-|



### Properties

#### NonStrictCompares
String compares with ignored chars' case.(忽略大小写为非严格的比较)
#### REGEX_EMAIL
Regex expression for parsing E-Mail URL
#### REGEX_URL
Regex exprression for parsing the http/ftp URL
#### RegexICMul
@``F:System.Text.RegularExpressions.RegexOptions.IgnoreCase`` + @``F:System.Text.RegularExpressions.RegexOptions.Multiline``
#### RegexICSng
@``F:System.Text.RegularExpressions.RegexOptions.IgnoreCase`` + @``F:System.Text.RegularExpressions.RegexOptions.Singleline``
