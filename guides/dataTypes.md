# [Type Characters](https://msdn.microsoft.com/en-us/library/s9cz43ek.aspx)

In addition to specifying a data type in a declaration statement, you can force the data type of some programming elements with a type character. The type character must immediately follow the element, with no intervening characters of any kind.
The type character is not part of the name of the element. An element defined with a type character can be referenced without the type character.

## Identifier Type Characters
Visual Basic supplies a set of identifier type characters, which you can use in a declaration to specify the data type of a variable or constant. The following table shows the available identifier type characters with examples of usage.

><table>
<tr><td>标识符类型字符</td><td>数据类型</td><td>示例</td></tr>
<tr><td>%</td><td>Integer</td><td>Dim L%</td></tr>
<tr><td>&amp;</td><td>Long</td><td>Dim M&amp;</td></tr>
<tr><td>@</td><td>Decimal</td><td>Const W@ = 37.5</td></tr>
<tr><td>!</td><td>Single</td><td>Dim Q!</td></tr>
<tr><td>#</td><td>Double</td><td>Dim X#</td></tr>
<tr><td>$</td><td>String</td><td>Dim V$ = "Secret"</td></tr>
</table>

## 十六进制文本和八进制文本

编译器通常将整数解释为十进制（基数为 10）数制。可以用 &H 前缀将整数强制为十六进制（基数为 16），可以用 &O 前缀将整数强制为八进制（基数为 8）。跟在前缀后面的数字必须适合于数制。下表阐释了上述内容。
<table>
<tr><td>数基</td><td>前缀</td><td>有效数值</td><td>示例</td></tr>
<tr><td>十六进制（以 16 为基）</td><td>&amp;H</td><td>0-9 和 A-F</td><td>&amp;HFFFF</td></tr>
<tr><td>八进制（以 10 为基）</td><td>&amp;O</td><td>0-7</td><td>&amp;O77</td></tr>
</table>
可以在前缀文本后面加一个文本类型字符。下面的示例显示如何执行此项操作。

```vb.net
Dim counter As Short = &H8000S
Dim flags As UShort = &H8000US
```
