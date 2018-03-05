#Region "Microsoft.VisualBasic::14a0f2801edc83c85c1320e092067316, Microsoft.VisualBasic.Core\Text\Xml\Linq\NodeIterator.vb"

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

    '     Module NodeIterator
    ' 
    '         Function: IterateArrayNodes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.Language

Namespace Text.Xml.Linq

    Public Module NodeIterator

        ''' <summary>
        ''' 使用<see cref="XmlDocument.Load"/>方法加载XML文档依旧是一次性的全部加载所有的文本到内存之中，第一次加载效率会比较低
        ''' 则可以使用这个方法来加载非常大的XML文档
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <returns></returns>
        <Extension> Public Iterator Function IterateArrayNodes(path$, tag$) As IEnumerable(Of String)
            Dim buffer As New List(Of String)
            Dim start$ = "<" & tag
            Dim ends$ = $"</{tag}>"
            Dim stack%
            Dim tagOpen As Boolean = False
            Dim lefts$
            Dim i%

            For Each line As String In path.IterateAllLines
                If tagOpen Then

                    i = InStr(line, ends)

                    If i > 0 Then
                        ' 遇到了结束标签，则取出来
                        If stack > 0 Then
                            stack -= 1 ' 内部栈，还没有结束，则忽略当前的这个标签
                        Else
                            ' 这个是真正的结束标签
                            lefts = Mid(line, i + ends.Length)
                            buffer += ends
                            tagOpen = False

                            Yield buffer.JoinBy(vbLf)

                            buffer *= 0
                            buffer += lefts

                            ' 这里要跳出来，否则后面buffer += line处任然会添加这个结束标签行的
                            Continue For
                        End If
                    ElseIf InStr(line, start) > 0 Then
                        stack += 1
                    End If

                    buffer += line
                Else
                    ' 需要一直遍历到开始标签为止
                    i = InStr(line, start)

                    If i > 0 Then
                        tagOpen = True
                        buffer += Mid(line, i)
                    End If
                End If
            Next
        End Function
    End Module
End Namespace
