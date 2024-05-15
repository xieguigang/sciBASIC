#Region "Microsoft.VisualBasic::825938a0ef487abc46c075f2f27cdb49, Microsoft.VisualBasic.Core\src\Text\IO\Encodings.vb"

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

    '   Total Lines: 42
    '    Code Lines: 16
    ' Comment Lines: 20
    '   Blank Lines: 6
    '     File Size: 1.37 KB


    '     Enum Encodings
    ' 
    '         Unicode, UTF16, UTF32, UTF7, UTF8
    '         UTF8WithoutBOM
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Text

Namespace Text

    ''' <summary>
    ''' The text document encodings constant for text file read and write
    ''' </summary>
    Public Enum Encodings As Byte

        ''' <summary>
        ''' <see cref="Encoding.Default"/>: Gets an encoding for the operating system's current ANSI code page.
        ''' </summary>
        [Default] = 0

        ANSI = 1

        ASCII = 10
        ''' <summary>
        ''' Alias of the value <see cref="UTF16"/>.
        ''' (utf-16编码的别名？所以使用这个编码的效果是和<see cref="UTF16"/>的效果是一样的)
        ''' </summary>
        Unicode
        UTF7
        ''' <summary>
        ''' 在Linux平台上面是<see cref="TextEncodings.UTF8WithoutBOM"/>，而在Windows平台上面则是带有BOM的UTF8格式. 
        ''' (HTML的默认的编码格式，假若所保存的html文本出现乱码，请考虑是不是应该要选择这个编码才行？)
        ''' </summary>
        UTF8
        UTF8WithoutBOM
        ''' <summary>
        ''' VB.NET的XML文件的默认编码格式为utf-16
        ''' </summary>
        UTF16
        UTF32

        ''' <summary>
        ''' Text encoding for simplify Chinese.
        ''' </summary>
        <Description("gb-2312")> GB2312
    End Enum
End Namespace
