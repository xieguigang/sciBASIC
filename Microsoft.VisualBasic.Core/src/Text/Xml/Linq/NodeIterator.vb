#Region "Microsoft.VisualBasic::3977c1422e6f7aac3bf4b0be0171a9af, Microsoft.VisualBasic.Core\src\Text\Xml\Linq\NodeIterator.vb"

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
    '         Function: GetArrayTemplate, IterateArrayNodes
    ' 
    '     Class NodeIteratorImpl
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: checkStart, getXmlSection, PopulateData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.Language

Namespace Text.Xml.Linq

    Public Module NodeIterator

        Friend Const XmlDeclare$ = "<?xml version=""1.0"" encoding=""utf-16""?>"
        Friend Const ArrayOfTemplate$ = XmlDeclare & "
<ArrayOf{0} xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
%s
</ArrayOf{0}>"

        ''' <summary>
        ''' 可以将模板文本之中的``%s``替换为相应的Xml数组文本
        ''' </summary>
        ''' <typeparam name="T">
        ''' 在.NET的XML序列化之中，数组元素的类型名称首字母会自动的被转换为大写形式
        ''' </typeparam>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetArrayTemplate(Of T As Class)() As String
            Return ArrayOfTemplate.Replace("{0}", GetType(T).GetNodeNameDefine.UpperCaseFirstChar)
        End Function

        ''' <summary>
        ''' 使用<see cref="XmlDocument.Load"/>方法加载XML文档依旧是一次性的全部加载所有的文本到内存之中，第一次加载效率会比较低
        ''' 则可以使用这个方法来加载非常大的XML文档
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function IterateArrayNodes(path$, tag$, Optional filter As Func(Of String, Boolean) = Nothing) As IEnumerable(Of String)
            Return New NodeIteratorImpl(tag).PopulateData(path, filter)
        End Function
    End Module

    Friend Class NodeIteratorImpl

        ReadOnly start1$
        ReadOnly start3$
        ReadOnly start2$
        ReadOnly ends$

        Dim stack%
        Dim tagOpen As Boolean = False
        Dim lefts$
        Dim i%
        Dim buffer As New List(Of String)

        Sub New(tag As String)
            start1 = "<" & tag & "/"
            start2 = "<" & tag & " "
            start3 = "<" & tag & ">"
            ends = $"</{tag}>"
        End Sub

        Public Iterator Function PopulateData(path As String, filter As Func(Of String, Boolean)) As IEnumerable(Of String)
            For Each line As String In path.IterateAllLines
                If tagOpen Then
                    i = InStr(line, ends)

                    If i > 0 Then
                        ' 遇到了结束标签，则取出来
                        If stack > 0 Then
                            ' 内部栈，还没有结束，则忽略当前的这个标签
                            stack -= 1
                        Else
                            Dim xmlText$ = getXmlSection(line)

                            If Not filter Is Nothing AndAlso filter(xmlText) Then
                                ' skip
                            Else
                                ' populate data
                                Yield xmlText
                            End If

                            ' 这里要跳出来，否则后面buffer += line处任然会添加这个结束标签行的
                            Continue For
                        End If
                    ElseIf checkStart(line, Nothing) Then
                        stack += 1
                    End If

                    buffer += line
                Else
                    ' 需要一直遍历到开始标签为止
                    If checkStart(line, i) Then
                        tagOpen = True
                        buffer += Mid(line, i)
                    End If
                End If
            Next

            If buffer > 0 AndAlso Not buffer.All(Function(s) Strings.Trim(s).StringEmpty) Then
                Call $"[{path}] is an incomplete xml dataset!".Warning
            End If
        End Function

        Private Function getXmlSection(line As String) As String
            Dim xmlText$

            ' 这个是真正的结束标签
            lefts = Mid(line, i + ends.Length)
            buffer += ends
            tagOpen = False

            xmlText = buffer.JoinBy(vbLf)
            buffer *= 0
            buffer += lefts

            Return xmlText
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function checkStart(line As String, ByRef i As Integer) As Boolean
            i = InStr(line, start1)

            If i > 0 Then
                Return True
            End If

            i = InStr(line, start2)

            If i > 0 Then
                Return True
            End If

            i = InStr(line, start3)

            If i > 0 Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class
End Namespace
