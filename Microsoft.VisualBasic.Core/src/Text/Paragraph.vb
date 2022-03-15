#Region "Microsoft.VisualBasic::9bc73033c5aafb7d024daec3c94433eb, sciBASIC#\Microsoft.VisualBasic.Core\src\Text\Paragraph.vb"

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

    '   Total Lines: 92
    '    Code Lines: 64
    ' Comment Lines: 16
    '   Blank Lines: 12
    '     File Size: 3.47 KB


    '     Module Paragraph
    ' 
    '         Function: Chunks, SplitParagraph, Trim
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Text

    Public Module Paragraph

        ''' <summary>
        ''' 对于空文本，这个函数返回一个空集合
        ''' </summary>
        ''' <param name="text">原始文本</param>
        ''' <param name="lineBreak">每一行文本的最大字符数量</param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function Chunks(text$, lineBreak%) As IEnumerable(Of String)
            If Not text.StringEmpty Then
                For i As Integer = 1 To text.Length Step lineBreak
                    Yield Mid(text, i, lineBreak)
                Next
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="text">一大段文本</param>
        ''' <param name="len">每一行文本的最大字符串数量长度</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 假若长度分割落在单词内，则添加一个连接符，假如是空格或者标点符号，则不处理
        ''' </remarks>
        <Extension>
        Public Iterator Function SplitParagraph(text$, len%) As IEnumerable(Of String)
            Dim lines$() = text.LineTokens

            For Each i As SeqValue(Of String) In lines.SeqIterator
                Dim line$ = i.value
                Dim s As New Value(Of String)
                Dim left% = Scan0 + 1

                Do While (s = Mid$(line$, left, len)).Length = len
                    If s.Value.Length = 0 Then
                        ' 已经结束了
                        Exit Do
                    Else
                        left += len
                    End If

                    Dim nextLine$ = Mid(line, left, len).Replace(ASCII.TAB, " "c)
                    Dim part As NamedValue(Of String) = nextLine.GetTagValue

                    If String.IsNullOrEmpty(nextLine) Then
                        Exit Do
                    End If

                    If Not String.IsNullOrEmpty(part.Name) Then ' 有空格
                        s.Value = Trim((+s) & part.Name)
                        left += part.Name.Length + 1
                    ElseIf nextLine.First = " "c Then ' 第一个字符是空格，则忽略掉
                        left += 1
                    Else
                        s.Value &= nextLine  ' 是剩余的结束部分
                        Yield Trim(+s)
                        s.Value = Nothing ' 必须要value值删除字符串，否则会重复出现最后一行
                        Exit Do
                    End If

                    Yield Trim(+s)
                Loop

                If Not String.IsNullOrEmpty(+s) Then
                    Yield Trim(+s)
                Else
                    If i.i <> lines.Length - 1 Then
                        Yield vbCrLf
                    End If
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Trim(s$) As String
            If s Is Nothing Then
                Return ""
            Else
                Return s.Trim(" ", ASCII.TAB)
            End If
        End Function
    End Module
End Namespace
