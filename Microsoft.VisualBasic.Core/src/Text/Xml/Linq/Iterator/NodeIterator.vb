#Region "Microsoft.VisualBasic::782686d8aab9014878ffe863400e4aa4, Microsoft.VisualBasic.Core\src\Text\Xml\Linq\Iterator\NodeIterator.vb"

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

    '   Total Lines: 50
    '    Code Lines: 28 (56.00%)
    ' Comment Lines: 13 (26.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (18.00%)
    '     File Size: 2.30 KB


    '     Module NodeIterator
    ' 
    '         Function: CreateBlockReader, GetArrayTemplate, (+2 Overloads) IterateArrayNodes
    ' 
    '     Delegate Function
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Xml

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

        <Extension>
        Public Function IterateArrayNodes(file As StreamReader, tag$, Optional filter As Func(Of String, Boolean) = Nothing) As IEnumerable(Of String)
            Return New NodeIteratorImpl(tag).PopulateData(file, filter)
        End Function

        Public Function CreateBlockReader(tag$, Optional filter As Func(Of String, Boolean) = Nothing, Optional name As String = "n/a") As ReadBlocks
            Return Function(lines) New NodeIteratorImpl(tag).PopulateData(lines, name, filter)
        End Function
    End Module

    Public Delegate Function ReadBlocks(lines As IEnumerable(Of String)) As IEnumerable(Of String)

End Namespace
