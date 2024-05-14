#Region "Microsoft.VisualBasic::5e37bcdbf8516ce3bb219d53ef15b407, Microsoft.VisualBasic.Core\test\test\markdownDisplayTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 53
    '    Code Lines: 34
    ' Comment Lines: 1
    '   Blank Lines: 18
    '     File Size: 1.12 KB


    ' Module markdownDisplayTest
    ' 
    '     Sub: consoleTest, Main1
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal

Module markdownDisplayTest
    Sub Main1()
        ' Call consoleTest()
        Call MarkdownRender.Print("# title

This is a inline ``code`` span. **bold** font style test.

table test:

|id|name|value|
|--|----|-----|
|1 |aaa |bbb  |
|2 |ccc |ddd  |
|3 |eee |fff  |

list test:

+ quote
+ list
+ data

> quote
> test
> block
>
> A ``code span`` in this block quot

A new ``paragraph``.

A url test: http://test.url/a/b/c/xxxx.txt


", indent:=10)

        Pause()
    End Sub

    Sub consoleTest()

        Dim format As New ConsoleFormat(Foreground:=AnsiColor.Green, Background:=AnsiColor.Red,
                                        Bold:=False, Underline:=False, Inverted:=False)

        Call Console.WriteLine(ToAnsiEscapeSequenceSlow(format) & "aaaaaaaa")

        format = New ConsoleFormat(Foreground:=AnsiColor.White, Background:=AnsiColor.Blue, Bold:=True, Underline:=True)

        Call Console.WriteLine(ToAnsiEscapeSequenceSlow(format) & "dgdfgdfg")

        Pause()
    End Sub
End Module
