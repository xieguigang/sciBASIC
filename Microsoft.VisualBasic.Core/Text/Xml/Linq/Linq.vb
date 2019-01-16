#Region "Microsoft.VisualBasic::992ffb00fb8ac813b61f7e02a3637e6e, Microsoft.VisualBasic.Core\Text\Xml\Linq\Linq.vb"

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

    '     Module Data
    ' 
    '         Function: GetNodeNameDefine, GetTypeName, InternalIterates, LoadUltraLargeXMLDataSet, LoadXmlDataSet
    '                   LoadXmlDocument, NodeInstanceBuilder, PopulateXmlElementText, UltraLargeXmlNodesIterator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language

Namespace Text.Xml.Linq

    ''' <summary>
    ''' Using large xml file as Linq data source
    ''' </summary>
    Public Module Data

        ''' <summary>
        ''' Load a specific xml file from a file location <paramref name="pathOrDoc"/>/ or 
        ''' a xml text document data into a <see cref="XmlDocument"/> object.
        ''' </summary>
        ''' <param name="pathOrDoc"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' using internally <see cref="XDocument.Load"/> to parse whole XML at once
        ''' </remarks>
        <Extension>
        Public Function LoadXmlDocument(pathOrDoc$, Optional preprocess As Func(Of String, String) = Nothing) As XmlDocument
            Dim xmlDoc As New XmlDocument()
            Dim doc$

            If pathOrDoc.FileExists Then
                If Not preprocess Is Nothing Then
                    doc = preprocess(pathOrDoc.ReadAllText)
                    xmlDoc.LoadXml(doc)
                Else
                    Call xmlDoc.Load(pathOrDoc)
                End If
            Else
                If Not preprocess Is Nothing Then
                    doc = preprocess(pathOrDoc)
                Else
                    doc = pathOrDoc
                End If

                Call xmlDoc.LoadXml(doc)
            End If

            Return xmlDoc
        End Function

        ''' <summary>
        ''' Using <paramref name="default"/> string name or <see cref="Type.Name"/>
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="default$">
        ''' If this parameter value is <see cref="StringEmpty"/>, then <see cref="Type.Name"/> will be use as the xml node name.
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function GetTypeName(type As Type, default$) As String
            If [default].StringEmpty Then
                Return type.Name
            Else
                Return [default]
            End If
        End Function

        ''' <summary>
        ''' 分别解析<see cref="XmlTypeAttribute"/>，<see cref="XmlRootAttribute"/>，如果这两个定义都不存在的话就返回<see cref="Type.Name"/>
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetNodeNameDefine(type As Type) As String
            With type.GetCustomAttributes(GetType(XmlTypeAttribute), inherit:=True)
                If Not .IsNullOrEmpty Then
                    With DirectCast(.First, XmlTypeAttribute).TypeName
                        If Not .StringEmpty Then
                            Return .ByRef
                        End If
                    End With
                End If
            End With

            With type.GetCustomAttributes(GetType(XmlRootAttribute), inherit:=True)
                If Not .IsNullOrEmpty Then
                    With DirectCast(.First, XmlRootAttribute).ElementName
                        If Not .StringEmpty Then
                            Return .ByRef
                        End If
                    End With
                End If
            End With

            Return type.Name
        End Function

        Private Iterator Function InternalIterates(XML$, nodeName$) As IEnumerable(Of String)
            Dim XmlNodeList As XmlNodeList = XML _
                .LoadXmlDocument _
                .GetElementsByTagName(nodeName)
            Dim sb As New StringBuilder

            For Each xmlNode As XmlNode In XmlNodeList
                Call sb.Clear()
                Call sb.Append($"<{nodeName} xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""")
                Call sb.Append(" ")

                For Each attr As XmlAttribute In xmlNode.Attributes
                    Call sb.Append($"{attr.Name}=""{attr.Value}""")
                    Call sb.Append(" ")
                Next

                Call sb.AppendLine(">")
                Call sb.AppendLine(xmlNode.InnerXml)
                Call sb.AppendLine($"</{nodeName}>")

                Yield sb.ToString
            Next
        End Function

        ''' <summary>
        ''' Only works for the xml file that contains a list or array of xml element, and then this function using this list element as linq data source.
        ''' (这个函数只建议在读取比较小的XML文件的时候使用，并且这个XML文件仅仅是一个数组或者列表的序列化结果)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="XML$">超大的XML文件的文件路径</param>
        ''' <param name="typeName">
        ''' 列表之中的节点在XML之中的tag标记名称，假若这个参数值为空的话，则会默认使用目标类型名称<see cref="Type.Name"/>
        ''' </param>
        ''' <param name="xmlns">
        ''' Using for the namespace replacement.
        ''' (当这个参数存在的时候，目标命名空间申明将会被替换为空字符串，数据对象才会被正确的加载)
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadXmlDataSet(Of T As Class)(XML$, Optional typeName$ = Nothing, Optional xmlns$ = Nothing, Optional forceLargeMode As Boolean = False) As IEnumerable(Of T)
            Dim nodeName$ = GetType(T).GetTypeName([default]:=typeName)
            Dim source As IEnumerable(Of String)

            If forceLargeMode OrElse XML.FileLength > 1024 * 1024 * 128 Then
                ' 这是一个超大的XML文档
                source = NodeIterator.IterateArrayNodes(XML, nodeName)
                xmlns = Nothing
            Else
                source = InternalIterates(XML, nodeName)
            End If

            Return source.NodeInstanceBuilder(Of T)(xmlns, xmlNode:=nodeName)
        End Function

        ''' <summary>
        ''' 从给定的文本之中利用反序列化从XML字符串构建出.NET对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="nodes"></param>
        ''' <param name="replaceXmlns$"></param>
        ''' <param name="xmlNode$">文件之中的节点名称</param>
        ''' <returns></returns>
        <Extension>
        Private Iterator Function NodeInstanceBuilder(Of T As Class)(nodes As IEnumerable(Of String), replaceXmlns$, xmlNode$) As IEnumerable(Of T)
            Dim handle As New DeserializeHandler(Of T)(xmlNode) With {
                .ReplaceXmlns = replaceXmlns
            }

            For Each xml As String In nodes
                Yield handle.LoadXml(xml)
            Next
        End Function

        ''' <summary>
        ''' Apply on a ultra large size XML database, which its data size is greater than 1GB to 100GB or even more.
        ''' (这个函数是直接忽略掉根节点的名称以及属性的,使用这个函数只需要关注于需要提取的数据的节点名称即可)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="path">文件路径</param>
        ''' <param name="typeName">目标节点名称,默认是使用类型<typeparamref name="T"/>的名称</param>
        ''' <param name="xmlns">``xmlns=...``,只需要给出等号后面的url即可</param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadUltraLargeXMLDataSet(Of T As Class)(path$, Optional typeName$ = Nothing, Optional xmlns$ = Nothing) As IEnumerable(Of T)
            Dim nodeName$ = GetType(T).GetTypeName([default]:=typeName)
            Return nodeName _
                .UltraLargeXmlNodesIterator(path) _
                .NodeInstanceBuilder(Of T)(xmlns, xmlNode:=nodeName)
        End Function

        <Extension>
        Private Iterator Function UltraLargeXmlNodesIterator(nodeName$, path$) As IEnumerable(Of String)
            Dim el As New Value(Of XElement)
            Dim XML$

            Using reader As XmlReader = XmlReader.Create(path)

                Call reader.MoveToContent()

                Do While (reader.Read())
                    ' Parse the file And return each of the child_node
                    If (reader.NodeType = XmlNodeType.Element AndAlso reader.Name = nodeName) Then
                        If (Not (el = XNode.ReadFrom(reader)) Is Nothing) Then
                            XML = el.Value.ToString
                            Yield XML
                        End If
                    End If
                Loop
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PopulateXmlElementText(Of T As Class)(path$, Optional typeName$ = Nothing) As IEnumerable(Of String)
            Return GetType(T) _
                .GetTypeName([default]:=typeName) _
                .UltraLargeXmlNodesIterator(path)
        End Function
    End Module
End Namespace
