#Region "Microsoft.VisualBasic::a8e873335bea41b5b6e0482169acc63b, mime\text%html\Document\Elements\HtmlElement.vb"

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

    '   Total Lines: 414
    '    Code Lines: 238 (57.49%)
    ' Comment Lines: 122 (29.47%)
    '    - Xml Docs: 96.72%
    ' 
    '   Blank Lines: 54 (13.04%)
    '     File Size: 17.86 KB


    '     Class HtmlElement
    ' 
    '         Properties: [class], Attributes, HtmlElements, id, IsEmpty
    '                     IsPlantText, name, OnlyInnerText, TagName
    ' 
    '         Function: GetAllChilds, GetAllChildsByNodeName, GetDirectChilds, getElementById, getElementsByClassName
    '                   getElementsByName, getElementsByTagName, GetHtmlText, GetPlantText, hasAttribute
    '                   Query, ToString
    ' 
    '         Sub: (+4 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Text.Xml

Namespace Document

    ''' <summary>
    ''' 一个标签所标记的元素以及内部文本
    ''' </summary>
    ''' <remarks>
    ''' 在选择器里：
    '''
    ''' + ID 和 类 选择器 区分 大小写
    ''' + 标签选择器、属性选择器不区分大小写
    ''' 
    ''' > 类选择器和 ID 选择器可能是区分大小写的。这取决于文档的语言。
    ''' > HTML 和 XHTML 将类和 ID 值定义为区分大小写，所以类和 ID 
    ''' > 值的大小写必须与文档中的相应值匹配。
    ''' > 
    ''' > —— W3C
    ''' </remarks>
    Public Class HtmlElement : Inherits InnerPlantText
        Implements IXmlDocumentTree
        Implements IStyleSelector(Of HtmlElement)

        ''' <summary>
        ''' 标签名
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TagName As String Implements IXmlDocumentTree.nodeName

        ''' <summary>
        ''' 标签的属性列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Attributes As ValueAttribute()
            Get
                Return attrs.Values.ToArray
            End Get
            Set(value As ValueAttribute())
                If value.IsNullOrEmpty Then
                    attrs = New Dictionary(Of ValueAttribute)
                Else
                    attrs = value.ToDictionary(Function(a) a.Name.ToLower)
                End If
            End Set
        End Property

        Public Property HtmlElements As InnerPlantText()
            Get
                Return elementNodes.ToArray
            End Get
            Set(value As InnerPlantText())
                elementNodes = New List(Of InnerPlantText)

                For Each node As InnerPlantText In value.SafeQuery
                    Call Add(node)
                Next
            End Set
        End Property

        ''' <summary>
        ''' get attribute value by attribute name
        ''' </summary>
        ''' <param name="name">case insensitive</param>
        ''' <returns>
        ''' this property returns nothing if the attribute is not found from the current element node
        ''' </returns>
        Default Public Property Element(name As String) As ValueAttribute
            Get
                Return attrs.TryGetValue(LCase(name))
            End Get
            Set(value As ValueAttribute)
                name = name.ToLower

                If attrs.ContainsKey(name) Then
                    attrs(name) = value
                Else
                    Call attrs.Add(name, value)
                End If
            End Set
        End Property

        Default Public ReadOnly Property Element(i As Integer) As InnerPlantText
            Get
                Return HtmlElements.ElementAtOrDefault(i)
            End Get
        End Property

        Public Overrides ReadOnly Property IsPlantText As Boolean
            Get
                Return False
            End Get
        End Property

        Dim attrs As New Dictionary(Of ValueAttribute)
        ''' <summary>
        ''' 当前的这个节点下面所拥有的子节点
        ''' </summary>
        Dim elementNodes As New List(Of InnerPlantText)

        ''' <summary>
        ''' 唯一编号是区分大小写的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property id As String
            Get
                Return attrs.TryGetValue("id").Value
            End Get
        End Property

        Public ReadOnly Property name As String
            Get
                Return attrs.TryGetValue("name").Value
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' multiple class could be assigned to a html node
        ''' </remarks>
        Public ReadOnly Property [class] As String()
            Get
                Dim names As String = Trim(attrs.TryGetValue("class").Value)

                If names = "" Then
                    Return Nothing
                Else
                    Return names.Split
                End If
            End Get
        End Property

        ''' <summary>
        ''' 大小写不敏感
        ''' </summary>
        Dim tagIndex As New Dictionary(Of String, List(Of HtmlElement))

        ''' <summary>
        ''' ** 大小写敏感
        ''' </summary>
        Dim classIndex As New Dictionary(Of String, List(Of HtmlElement))
        ''' <summary>
        ''' ** 大小写敏感
        ''' </summary>
        Dim nameIndex As New Dictionary(Of String, List(Of HtmlElement))
        ''' <summary>
        ''' ** 大小写敏感
        ''' </summary>
        Dim idIndex As New Dictionary(Of String, HtmlElement)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function hasAttribute(name As String) As Boolean
            Return attrs.ContainsKey(LCase(name))
        End Function

        Public Overrides Function GetPlantText() As String
            If Me.TagName.TextEquals("br") Then
                Return vbCrLf
            Else
                Dim sb As New StringBuilder(Me.InnerText.UnescapeHTML)

                If Not Me.HtmlElements Is Nothing Then
                    For Each node In HtmlElements
                        Call sb.Append(node.GetPlantText)
                    Next
                End If

                Return sb.ToString
            End If
        End Function

        Public Sub Add(node As InnerPlantText)
            Call elementNodes.Add(node)

            If Not TypeOf node Is HtmlElement Then
                Return
            End If

            Dim element As HtmlElement = DirectCast(node, HtmlElement)
            Dim id As String = Strings.Trim(element.id)

            If (Not id.StringEmpty) AndAlso (Not idIndex.ContainsKey(id)) Then
                idIndex.Add(id, element)
            End If

            Dim name As String = element.name

            If Not name.StringEmpty Then
                Call Add(nameIndex, name, element)
            End If

            Call Add(tagIndex, LCase(element.TagName), element)

            Dim classNames As String() = element.class

            If Not classNames.IsNullOrEmpty Then
                For Each className As String In classNames
                    Call Add(classIndex, className, element)
                Next
            End If
        End Sub

        Private Sub Add(hashlist As Dictionary(Of String, List(Of HtmlElement)), key As String, element As HtmlElement)
            If Not hashlist.ContainsKey(key) Then
                Call hashlist.Add(key, New List(Of HtmlElement))
            End If

            Call hashlist(key).Add(element)
        End Sub

        Public Sub Add(attr As ValueAttribute)
            If attrs.ContainsKey(attr.Name) Then
                Call attrs(attr.Name).Values.AddRange(attr.Values)
            Else
                Call attrs.Add(attr.Name, attr)
            End If
        End Sub

        ''' <summary>
        ''' add tagged attribute value.
        ''' </summary>
        ''' <param name="name">the attribute name</param>
        ''' <param name="value">the attribute value string.</param>
        Public Sub Add(name As String, value As String)
            If attrs.ContainsKey(name) Then
                Call attrs(name).Values.Add(value)
            Else
                Call attrs.Add(name, New ValueAttribute With {.Name = name, .Values = New List(Of String) From {value}})
            End If
        End Sub

        Public ReadOnly Property OnlyInnerText As Boolean
            Get
                Return elementNodes.Count = 1 AndAlso elementNodes(Scan0).IsPlantText
            End Get
        End Property

        Public Overrides ReadOnly Property IsEmpty As Boolean
            Get
                Return MyBase.IsEmpty AndAlso
                    String.IsNullOrEmpty(TagName) AndAlso
                    Attributes.IsNullOrEmpty AndAlso
                    HtmlElements.IsNullOrEmpty
            End Get
        End Property

        ''' <summary>
        ''' 这个函数是应用于判断<see cref="getElementsByClassName(String)"/>或者<see cref="getElementsByName(String)"/>或者<see cref="getElementsByTagName(String)"/>
        ''' 函数所返回来的结果，所以不会对<see cref="InnerPlantText"/>节点产生误判
        ''' </summary>
        ''' <param name="query"></param>
        ''' <returns></returns>
        Public Iterator Function GetDirectChilds(query As IEnumerable(Of InnerPlantText)) As IEnumerable(Of InnerPlantText)
            For Each item As InnerPlantText In query
                For Each child As InnerPlantText In elementNodes
                    If child Is item Then
                        Yield item
                        Exit For
                    End If
                Next
            Next
        End Function

        ''' <summary>
        ''' Get the element with the specified ID
        ''' </summary>
        ''' <param name="id">Required. The ID attribute's value of the element you want to get</param>
        ''' <returns>An Element Object, representing an element with the specified ID. Returns null if no elements with the specified ID exists</returns>
        ''' <remarks>
        ''' The getElementById() method returns the element that has the ID attribute with the specified value.
        ''' This method Is one Of the most common methods In the HTML DOM, And Is used almost every time you want To manipulate, Or Get info from, an element On your document.
        ''' Returns null If no elements With the specified ID exists.
        ''' An ID should be unique within a page. However, If more than one element With the specified ID exists, the getElementById() method returns the first element In the source code.
        ''' </remarks>
        Public Function getElementById(id As String) As HtmlElement Implements IStyleSelector(Of HtmlElement).GetElementById
            If idIndex.ContainsKey(id) Then
                Return idIndex(id)
            Else
                For Each element As InnerPlantText In elementNodes
                    If TypeOf element Is HtmlElement Then
                        element = DirectCast(element, HtmlElement).getElementById(id)

                        If Not element Is Nothing Then
                            Return element
                        End If
                    End If
                Next

                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Get all elements with the specified class name
        ''' </summary>
        ''' <param name="classname">
        ''' Required. The class name of the elements you want to get.
        ''' To search for multiple class names, separate them with spaces, Like "test demo".
        ''' </param>
        ''' <returns>
        ''' An HTMLCollection object, representing a collection of elements with the specified class name. The elements in the returned collection are sorted as they appear in the source code.
        ''' </returns>
        ''' <remarks>
        ''' The getElementsByClassName() method returns a collection of all elements in the document with the specified class name, as an HTMLCollection object.
        ''' The HTMLCollection Object represents a collection Of nodes. The nodes can be accessed by index numbers. The index starts at 0.
        ''' Tip: You can use the length Property Of the HTMLCollection Object To determine the number Of elements With a specified Class name, Then you can Loop through all elements And extract the info you want.
        ''' </remarks>
        Public Function getElementsByClassName(classname As String) As IEnumerable(Of HtmlElement) Implements IStyleSelector(Of HtmlElement).GetElementsByClassName
            Static api As MethodInfo = GetType(HtmlElement).GetMethod(NameOf(getElementsByClassName))

            Return classIndex _
                .TryGetValue(classname) _
                .JoinIterates(Query(api, classname)) _
                .Distinct
        End Function

        ''' <summary>
        ''' Get all elements with the specified name
        ''' </summary>
        ''' <param name="name">Required. The name attribute value of the element you want to access/manipulate</param>
        ''' <returns>
        ''' An HTMLCollection object, representing a collection of elements with the specified name. The elements in the returned collection are sorted as they appear in the source code.
        ''' </returns>
        ''' <remarks>
        ''' The getElementsByName() method returns a collection of all elements in the document with the specified name (the value of the name attribute), as an HTMLCollection object.
        ''' The HTMLCollection Object represents a collection Of nodes. The nodes can be accessed by index numbers. The index starts at 0.
        ''' Tip: You can use the length Property Of the HTMLCollection Object To determine the number Of elements With the specified name, Then you can Loop through all elements And extract the info you want.
        ''' Note: In HTML5, the "name" attribute Is deprecated And has been replaced by the "id" attribute for many elements. Use the document.getElementById() method where it Is appropriate. Also look at the getElementsByClassName() And getElementsByTagName() methods.
        ''' </remarks>
        Public Function getElementsByName(name As String) As HtmlElement() Implements IStyleSelector(Of HtmlElement).GetElementsByName
            Static api As MethodInfo = GetType(HtmlElement).GetMethod(NameOf(getElementsByName))

            Return nameIndex _
                .TryGetValue(name) _
                .JoinIterates(Query(api, name)) _
                .Distinct _
                .ToArray
        End Function

        Private Iterator Function Query(calls As MethodInfo, arg As String) As IEnumerable(Of HtmlElement)
            For Each node As HtmlElement In From obj As InnerPlantText
                                            In elementNodes
                                            Where TypeOf obj Is HtmlElement
                                            Select DirectCast(obj, HtmlElement)

                Dim pull As IEnumerable(Of HtmlElement) = calls.Invoke(node, {arg})
                Dim collection As HtmlElement() = pull.SafeQuery.ToArray

                If collection.Length > 0 Then
                    For Each item In collection
                        Yield item
                    Next
                End If
            Next
        End Function

        ''' <summary>
        ''' Get all elements in the document with the specified tag name
        ''' </summary>
        ''' <param name="tagname">Required. The tagname of the elements you want to get</param>
        ''' <returns>
        ''' An HTMLCollection object, representing a collection of elements with the specified tag name. The elements in the returned collection are sorted as they appear in the source code.
        ''' </returns>
        ''' <remarks>
        ''' The getElementsByTagName() method returns a collection of all elements in the document with the specified tag name, as an HTMLCollection object.
        ''' The HTMLCollection Object represents a collection Of nodes. The nodes can be accessed by index numbers. The index starts at 0.
        ''' Tip: The parametervalue "*" returns all elements In the document.
        ''' Tip: You can use the length Property Of the HTMLCollection Object To determine the number Of elements With the specified tag name, Then you can Loop through all elements And extract the info you want.
        ''' </remarks>
        Public Function getElementsByTagName(tagname As String) As HtmlElement()
            Static api As MethodInfo = GetType(HtmlElement).GetMethod(NameOf(getElementsByTagName))

            ' 20210603 html文档之中的标签名称应该是大小写无关的么？
            Return tagIndex _
                .TryGetValue(Strings.LCase(tagname)) _
                .JoinIterates(Query(api, tagname)) _
                .Distinct _
                .ToArray
        End Function

        Public Overrides Function ToString() As String
            Dim name As String = LCase(TagName)

            If attrs.Count = 0 Then
                Return $"<{name}>...</{name}>"
            Else
                Return $"<{name} {attrs.Values.JoinBy(" ")}>...</{name}>"
            End If
        End Function

        Public Function GetAllChildsByNodeName(nodename As String) As IXmlDocumentTree() Implements IXmlDocumentTree.GetAllChildsByNodeName
            Return getElementsByTagName(nodename).Select(Function(n) DirectCast(n, IXmlDocumentTree)).ToArray
        End Function

        Public Function GetAllChilds() As IXmlNode() Implements IXmlDocumentTree.GetAllChilds
            Return elementNodes.Select(Function(n) DirectCast(n, IXmlNode)).ToArray
        End Function

        Public Overrides Function GetHtmlText() As String
            Return Me.ToHtml
        End Function
    End Class
End Namespace
