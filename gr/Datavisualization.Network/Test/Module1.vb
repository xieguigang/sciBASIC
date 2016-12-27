#Region "Microsoft.VisualBasic::0265e2b773e151927ce449634601b672, ..\sciBASIC#\gr\Datavisualization.Network\Test\Module1.vb"

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

Imports Microsoft.VisualBasic.Language

Module Module1

    Dim fsfs As String = <s>"78","7.1741573","0.0","0.13938919","0.0","","1","1","13",,"","false","ARGININE-SYN","8.0","1","0","0","0.69129213",,"false","0","ARGININE-SYN","0","false","0.0"</s>

    Sub test()
        Dim rr = CharsParser(fsfs)

        ' Call MsgBox(String.Join(vbCrLf, rr.ToArray))
    End Sub

    ''' <summary>
    ''' 通过Chars枚举来解析域
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    Public Function CharsParser(s As String) As List(Of String)
        Dim tokens As New List(Of String)
        Dim temp As New List(Of Char)
        Dim stack As Boolean = False ' 解析器是否是处于由双引号所产生的栈之中？
        Dim preToken As Boolean = False
        Dim deliExit As Boolean = False

        For Each c As Char In s.Replace("""""", """")
            If c = ","c Then
                If Not stack Then
                    Call tokens.Add(New String(temp.ToArray))
                    Call temp.Clear()
                    deliExit = True
                Else  '  是以双引号开始的
                    If temp.Count > 0 AndAlso temp.Last = """"c Then ' 但是逗号的前一个符号是双引号，则是结束的标识
                        Call temp.RemoveLast
                        stack = False
                        Call tokens.Add(New String(temp.ToArray))
                        Call temp.Clear()
                        deliExit = True
                    Else
                        If temp.Count = 0 AndAlso stack Then
                            Call tokens.Add("")
                            deliExit = True
                            stack = False
                        Else
                            Call temp.Add(c)
                            deliExit = False
                        End If
                    End If
                End If
            ElseIf c = """"c Then  ' 必须要在逗号分隔符之前才起作用
                If temp.Count = 0 Then  ' 这个双引号是在最开始的位置
                    If stack = True Then
                        Call temp.Add(c)
                    Else
                        stack = True
                    End If
                Else
                    Call temp.Add(c)
                End If
                deliExit = False
            Else
                Call temp.Add(c)
                deliExit = False
            End If
        Next

        If temp.Count > 0 Then
            If temp.Last = """"c Then
                Call temp.RemoveLast  ' BUGS fixed for test data:   "Iron ion, (Fe2+)","Iron homeostasis",PM0352,"Iron homeostasis","Fur - Pasteurellales",+,XC_2767,"XC_1988; XC_1989"
            End If
            Call tokens.Add(New String(temp.ToArray))
        Else
            If deliExit Then
                Call tokens.Add("")
            End If
        End If

        Return tokens
    End Function
End Module
