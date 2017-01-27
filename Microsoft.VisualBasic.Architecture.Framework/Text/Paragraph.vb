#Region "Microsoft.VisualBasic::2a6f5c2346625d0ad50059dbcd45724d, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\Paragraph.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Text

    Public Module Paragraph

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="text$"></param>
        ''' <param name="len%"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 假若长度分割落在单词内，则添加一个连接符，假如是空格或者标点符号，则不处理
        ''' </remarks>
        Public Iterator Function Split(text$, len%) As IEnumerable(Of String)
            Dim lines$() = text.lTokens

            For Each i As SeqValue(Of String) In lines.SeqIterator
                Dim line$ = +i
                Dim s As New Value(Of String)
                Dim left% = Scan0 + 1

                Do While (s = Mid$(line$, left, len)).Length = len
                    If s.value.Length = 0 Then
                        Exit Do ' 已经结束了
                    Else
                        left += len
                    End If

                    Dim nextLine$ = Mid(line, left, len) _
                        .Replace(ASCII.TAB, " "c)
                    Dim part As NamedValue(Of String) =
                        nextLine.GetTagValue

                    If String.IsNullOrEmpty(nextLine) Then
                        Exit Do
                    End If

                    If Not String.IsNullOrEmpty(part.Name) Then ' 有空格
                        s.value = Trim((+s) & part.Name)
                        left += part.Name.Length + 1
                    ElseIf nextLine.First = " "c Then ' 第一个字符是空格，则忽略掉
                        left += 1
                    Else
                        s.value &= nextLine  ' 是剩余的结束部分
                        Yield Trim(+s)
                        s.value = Nothing ' 必须要value值删除字符串，否则会重复出现最后一行
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

        Private Function Trim(s$) As String
            If s Is Nothing Then
                Return ""
            Else
                Return s.Trim(" ", ASCII.TAB)
            End If
        End Function
    End Module
End Namespace
