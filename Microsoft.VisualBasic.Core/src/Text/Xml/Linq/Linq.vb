#Region "Microsoft.VisualBasic::7d623521efcbe41b7bf9a68275174f55, Microsoft.VisualBasic.Core\src\Text\Xml\Linq\Linq.vb"

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

    '   Total Lines: 415
    '    Code Lines: 251 (60.48%)
    ' Comment Lines: 115 (27.71%)
    '    - Xml Docs: 95.65%
    ' 
    '   Blank Lines: 49 (11.81%)
    '     File Size: 20.20 KB


    '     Module Data
    ' 
    '         Function: ArrayNodesFromDocument, CreateNodeObject, GetNodeNameDefine, GetTypeName, GetXmlNodeDoc
    '                   InternalIterates, IteratesArrayNodes, LoadArrayNodes, (+2 Overloads) LoadUltraLargeXMLDataSet, LoadXmlDataSet
    '                   LoadXmlDocument, NodeInstanceBuilder, NodeStream, PopulateXmlElementText, UltraLargeXmlNodesIterator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
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

        <Extension>
        Public Function GetXmlNodeDoc(element As XElement) As XmlDocument
            Using xmlReader As XmlReader = element.CreateReader()
                Dim XmlDoc As New XmlDocument()
                XmlDoc.Load(xmlReader)
                Return XmlDoc
            End Using
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

        ''' <summary>
        ''' 使用<see cref="XmlDocument"/>进行小文件的加载操作
        ''' </summary>
        ''' <param name="XML$"></param>
        ''' <param name="nodeName$"></param>
        ''' <param name="filter"></param>
        ''' <returns></returns>
        Private Iterator Function InternalIterates(XML$, nodeName$, filter As Func(Of String, Boolean)) As IEnumerable(Of String)
            Dim xmlDocs As XmlDocument = XML.LoadXmlDocument
            Dim XmlNodeList As XmlNodeList = xmlDocs.GetElementsByTagName(nodeName)
            Dim sb As New StringBuilder
            Dim xmlText$

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

                xmlText = sb.ToString

                If Not filter Is Nothing AndAlso filter(xmlText) Then
                    Continue For
                End If

                Yield xmlText
            Next
        End Function

        ''' <summary>
        ''' Only works for the xml file that contains a list or array of xml element, 
        ''' and then this function using this list element as linq data source.
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
        ''' <param name="elementFilter">
        ''' 如果这个函数指针返回true, 则表示当前的数据元素节点需要被抛弃
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (这个函数只建议在读取比较小的XML文件的时候使用，并且这个XML文件仅仅是一个数组或者列表的序列化结果)
        ''' </remarks>
        <Extension>
        Public Function LoadXmlDataSet(Of T As Class)(XML$,
                                                      Optional typeName$ = Nothing,
                                                      Optional xmlns$ = Nothing,
                                                      Optional forceLargeMode As Boolean = False,
                                                      Optional elementFilter As Func(Of String, Boolean) = Nothing,
                                                      Optional ignoreError As Boolean = False) As IEnumerable(Of T)

            Dim nodeName$ = GetType(T).GetTypeName([default]:=typeName)
            Dim source As IEnumerable(Of String)

            If forceLargeMode OrElse XML.FileLength > 1024 * 1024 * 128 Then
                ' 这是一个超大的XML文档
                source = NodeIterator.IterateArrayNodes(XML, nodeName, elementFilter)
                xmlns = Nothing
            Else
                source = InternalIterates(XML, nodeName, elementFilter)
            End If

            Return source.NodeInstanceBuilder(Of T)(xmlns, xmlNode:=nodeName, ignoreError:=ignoreError)
        End Function

        <Extension>
        Public Function NodeStream(Of T As Class)(stream As IEnumerable(Of String),
                                                  Optional typeName$ = Nothing,
                                                  Optional xmlns$ = Nothing,
                                                  Optional ignoreError As Boolean = False) As IEnumerable(Of T)

            Return stream.NodeInstanceBuilder(Of T)(xmlns, xmlNode:=GetType(T).GetTypeName([default]:=typeName), ignoreError:=ignoreError)
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
        Private Iterator Function NodeInstanceBuilder(Of T As Class)(nodes As IEnumerable(Of String), replaceXmlns$, xmlNode$,
                                                                     ignoreError As Boolean,
                                                                     Optional variants As Type() = Nothing) As IEnumerable(Of T)

            Dim handle As New DeserializeHandler(Of T)(xmlNode) With {
                .ReplaceXmlns = replaceXmlns
            }
            Dim element As T

            For Each xml As String In nodes
                Try
                    element = handle.LoadXml(xml, variants)
                Catch ex As Exception
                    If ignoreError Then
                        Call $"find invalid xml text content! [{Mid(xml, 1, 32).TrimNewLine}...]".Warning
                        Continue For
                    Else
                        Throw
                    End If
                End Try

                ' populate a new element from the 
                ' node Text
                Yield element
            Next
        End Function

        Public Function CreateNodeObject(Of T As Class)(xmlBlock As String, Optional typeName$ = Nothing, Optional xmlns$ = Nothing) As T
            Dim xmlNode As String = GetType(T).GetTypeName([default]:=typeName)
            Dim handle As New DeserializeHandler(Of T)(xmlNode) With {
                .ReplaceXmlns = xmlns
            }
            Dim element As T = handle.LoadXml(xmlBlock)

            Return element
        End Function

        ''' <summary>
        ''' Apply on a ultra large size XML database, which its data size is greater than 1GB to 100GB or even more.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="path">文件路径</param>
        ''' <param name="typeName">目标节点名称,默认是使用类型<typeparamref name="T"/>的名称</param>
        ''' <param name="xmlns">``xmlns=...``,只需要给出等号后面的url即可</param>
        ''' <param name="selector">
        ''' 在加载数据集的时候,过滤掉一些不使用的数据,可以节省很多内存以及减少数据的加载时间.
        ''' 因为后续的Xml反序列化操作在大数据集合下话费的时间会非常长
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (这个函数是直接忽略掉根节点的名称以及属性的,使用这个函数只需要关注于需要提取的数据的节点名称即可)
        ''' </remarks>
        <Extension>
        Public Function LoadUltraLargeXMLDataSet(Of T As Class)(path$,
                                                                Optional typeName$ = Nothing,
                                                                Optional xmlns$ = Nothing,
                                                                Optional selector As Func(Of XElement, Boolean) = Nothing,
                                                                Optional preprocess As Func(Of String, String) = Nothing,
                                                                Optional ignoreError As Boolean = False,
                                                                Optional variants As Type() = Nothing,
                                                                Optional tqdm As Boolean = False) As IEnumerable(Of T)

            Dim nodeName = GetType(T).GetTypeName([default]:=typeName)

            Return nodeName _
                .UltraLargeXmlNodesIterator(path, tqdm, selector) _
                .Select(Function(node)
                            If Not preprocess Is Nothing Then
                                Return preprocess(node.ToString)
                            Else
                                Return node.ToString
                            End If
                        End Function) _
                .NodeInstanceBuilder(Of T)(
                    replaceXmlns:=xmlns,
                    xmlNode:=nodeName,
                    ignoreError:=ignoreError,
                    variants:=variants
                )
        End Function


        ''' <summary>
        ''' Apply on a ultra large size XML database, which its data size is greater than 1GB to 100GB or even more.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="s">The document stream data</param>
        ''' <param name="typeName">目标节点名称,默认是使用类型<typeparamref name="T"/>的名称</param>
        ''' <param name="xmlns">``xmlns=...``,只需要给出等号后面的url即可</param>
        ''' <param name="selector">
        ''' 在加载数据集的时候,过滤掉一些不使用的数据,可以节省很多内存以及减少数据的加载时间.
        ''' 因为后续的Xml反序列化操作在大数据集合下话费的时间会非常长
        ''' </param>
        ''' <param name="preprocess">
        ''' pre-processing the each xml document text block before parsed as the target clr object
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (这个函数是直接忽略掉根节点的名称以及属性的,使用这个函数只需要关注于需要提取的数据的节点名称即可)
        ''' </remarks>
        <Extension>
        Public Function LoadUltraLargeXMLDataSet(Of T As Class)(s As Stream,
                                                                Optional typeName$ = Nothing,
                                                                Optional xmlns$ = Nothing,
                                                                Optional selector As Func(Of XElement, Boolean) = Nothing,
                                                                Optional preprocess As Func(Of String, String) = Nothing,
                                                                Optional ignoreError As Boolean = False,
                                                                Optional variants As Type() = Nothing,
                                                                Optional tqdm As Boolean = False) As IEnumerable(Of T)

            Dim nodeName = GetType(T).GetTypeName([default]:=typeName)
            Dim reader As New XmlStreamReader(nodeName, s, selector) With {
                .ShowProgress = tqdm
            }

            Return reader.UltraLargeXmlNodesIterator() _
                .Select(Function(node)
                            If Not preprocess Is Nothing Then
                                Return preprocess(node.ToString)
                            Else
                                Return node.ToString
                            End If
                        End Function) _
                .NodeInstanceBuilder(Of T)(
                    replaceXmlns:=xmlns,
                    xmlNode:=nodeName,
                    ignoreError:=ignoreError,
                    variants:=variants
                )
        End Function

        ''' <summary>
        ''' Parse xml element array from a given xml document text
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="documentText">the xml document text</param>
        ''' <param name="typeName$"></param>
        ''' <param name="xmlns$"></param>
        ''' <param name="selector"></param>
        ''' <param name="ignoreError"></param>
        ''' <param name="tqdm"></param>
        ''' <returns></returns>
        Public Function LoadArrayNodes(Of T As Class)(documentText$,
                                                      Optional typeName$ = Nothing,
                                                      Optional xmlns$ = Nothing,
                                                      Optional selector As Func(Of XElement, Boolean) = Nothing,
                                                      Optional ignoreError As Boolean = False,
                                                      Optional tqdm As Boolean = False) As IEnumerable(Of T)

            Dim nodeName = GetType(T).GetTypeName([default]:=typeName)
            Dim s As New MemoryStream(Encoding.UTF8.GetBytes(documentText))
            Dim reader As New XmlStreamReader(nodeName, s, selector) With {.ShowProgress = tqdm}

            Return reader.UltraLargeXmlNodesIterator _
                .Select(Function(node) node.ToString) _
                .NodeInstanceBuilder(Of T)(xmlns, xmlNode:=nodeName, ignoreError:=ignoreError)
        End Function

        ''' <summary>
        ''' 可以使用函数<see cref="GetXmlNodeDoc(XElement)"/>来进行类型的转换操作
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="typeName$"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IteratesArrayNodes(path$, typeName$,
                                           Optional selector As Func(Of XElement, Boolean) = Nothing,
                                           Optional tqdm As Boolean = False) As IEnumerable(Of XElement)

            Return typeName.UltraLargeXmlNodesIterator(path, tqdm, selector)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ArrayNodesFromDocument(documentText$, typeName$,
                                               Optional selector As Func(Of XElement, Boolean) = Nothing,
                                               Optional tqdm As Boolean = False) As IEnumerable(Of XElement)

            Dim s As New MemoryStream(Encoding.UTF8.GetBytes(documentText))
            Dim reader As New XmlStreamReader(typeName, s, selector) With {.ShowProgress = tqdm}

            Return reader.UltraLargeXmlNodesIterator
        End Function

        <Extension>
        Private Iterator Function UltraLargeXmlNodesIterator(nodeName$, path$,
                                                             tqdm As Boolean,
                                                             selector As Func(Of XElement, Boolean)) As IEnumerable(Of XElement)

            Using file As Stream = path.Open(FileMode.Open, [readOnly]:=True)
                Dim reader As New XmlStreamReader(nodeName, file, selector) With {
                    .ShowProgress = tqdm
                }

                For Each node In reader.UltraLargeXmlNodesIterator
                    ' 因为在这里打开了一个文件,假若不使用iterator迭代的话
                    ' 文件会被直接关闭,导致无法读取
                    Yield node
                Next
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PopulateXmlElementText(Of T As Class)(path$,
                                                              Optional typeName$ = Nothing,
                                                              Optional selector As Func(Of XElement, Boolean) = Nothing,
                                                              Optional tqdm As Boolean = False) As IEnumerable(Of String)
            Return GetType(T) _
                .GetTypeName([default]:=typeName) _
                .UltraLargeXmlNodesIterator(path, tqdm, selector)
        End Function
    End Module
End Namespace
