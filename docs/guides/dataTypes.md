# [Type Characters](https://msdn.microsoft.com/en-us/library/s9cz43ek.aspx)

In addition to specifying a data type in a declaration statement, you can force the data type of some programming elements with a type character. The type character must immediately follow the element, with no intervening characters of any kind.
The type character is not part of the name of the element. An element defined with a type character can be referenced without the type character.

## Identifier Type Characters
Visual Basic supplies a set of identifier type characters, which you can use in a declaration to specify the data type of a variable or constant. The following table shows the available identifier type characters with examples of usage.

|标识符类型字符|数据类型|示例                 |
|-------------|-------|---------------------|
|%            |Integer|``Dim L%``           |
|&amp;        |Long   |``Dim M&``           |
|@            |Decimal|``Const W@ = 37.5``  |
|!            |Single |``Dim Q!``           |
|#            |Double |``Dim X#``           |
|$            |String |``Dim V$ = "Secret"``|

For example:
```vbnet
Public Function FromDistributes(
                data As IEnumerable(Of Double), 
       Optional base! = 10.0F, 
       Optional color$ = "darkblue") As HistogramGroup
```
```vbnet
Public Function Iterations(model As Type, observation As ODEsOut, k&,
                           Optional expected% = 10,
                           Optional stop% = -1,
                           Optional partN% = 20,
                           Optional cut# = 0.3,
                           Optional work$ = Nothing,
                           Optional parallel As Boolean = False,
                           Optional ByRef outIterates As Dictionary(Of String, Dictionary(Of String, Double)()) = Nothing) _
                                                      As Dictionary(Of String, Double)()
```

## 十六进制文本和八进制文本

编译器通常将整数解释为十进制（基数为 10）数制。可以用``&H``前缀将整数强制为十六进制（基数为 16），可以用``&O``前缀将整数强制为八进制（基数为 8）。跟在前缀后面的数字必须适合于数制。下表阐释了上述内容。

|数基                 |前缀   |有效数值  |示例      |
|---------------------|------|----------|---------|
|十六进制（以 16 为基）|``&H``|0-9 和 A-F|``&HFFFF``|
|八进制（以 10 为基）  |``&O``|0-7       |``&O77``  |

可以在前缀文本后面加一个文本类型字符。下面的示例显示如何执行此项操作。

```vbnet
Dim counter As Short = &H8000S
Dim flags As UShort = &H8000US
```
