#Region "Microsoft.VisualBasic::a254f979bda0f5760469da0a00c86700, Data\DataFrame\IO\CSVText\CSVFile\Tokenizer.vb"

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

    '   Total Lines: 88
    '    Code Lines: 49 (55.68%)
    ' Comment Lines: 23 (26.14%)
    '    - Xml Docs: 95.65%
    ' 
    '   Blank Lines: 16 (18.18%)
    '     File Size: 2.80 KB


    '     Class Tokenizer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CharsParser, IsEmptyRow, RegexTokenizer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Option Explicit On
Option Strict Off

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace IO.CSVFile

    ''' <summary>
    ''' RowObject parsers
    ''' </summary>
    Public NotInheritable Class Tokenizer

        Private Sub New()
        End Sub

        ''' <summary>
        ''' A regex expression string that use for split the line text.
        ''' </summary>
        ''' <remarks></remarks>
        Const SplitRegxExpression As String = "[" & vbTab & ",](?=(?:[^""]|""[^""]*"")*$)"

        ''' <summary>
        ''' Parsing the row data from the input string line.(通过正则表达式来解析域)
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Shared Function RegexTokenizer(s As String) As List(Of String)
            If String.IsNullOrEmpty(s) Then
                Return New List(Of String)
            End If

            Dim Row As String() = Regex.Split(s, SplitRegxExpression)
            For i As Integer = 0 To Row.Length - 1
                s = Row(i)

                If Not String.IsNullOrEmpty(s) AndAlso s.Length > 1 Then
                    If s.First = """"c AndAlso s.Last = """"c Then
                        s = Mid(s, 2, s.Length - 2)
                    End If
                End If

                Row(i) = s
            Next

            Return Row.AsList
        End Function

        ''' <summary>
        ''' 通过Chars枚举来解析域，分隔符默认为逗号
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns>this function returns empty string array for null or empty string input.</returns>
        Public Shared Function CharsParser(s$,
                                           Optional delimiter As Char = ","c,
                                           Optional quot As Char = ASCII.Quot) As IEnumerable(Of String)

            If s.StringEmpty(whitespaceAsEmpty:=False) Then
                Return New String() {}
            Else
                Return New RowTokenizer(s).GetTokens(delimiter, quot)
            End If
        End Function

        ''' <summary>
        ''' 是否等于``,,,,,,,,,``
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Shared Function IsEmptyRow(s$, del As Char) As Boolean
            Dim l% = Strings.Len(s)

            If l = 0 Then
                Return True
            End If

            For Each c As Char In s
                If c = del Then
                    l -= 1
                End If
            Next

            ' 长度为零说明整个字符串都是分隔符，即为空行
            Return l = 0
        End Function
    End Class
End Namespace
