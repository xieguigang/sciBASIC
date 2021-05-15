#Region "Microsoft.VisualBasic::ddee2d7a2f2014576eb07d0a6774f086, mime\text%html\HTML\HtmlParser\HtmlElement.vb"

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

'     Structure ValueAttribute
' 
'         Properties: IsEmpty, Name, Value
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: ToString
' 
'     Class HtmlElement
' 
'         Properties: Attributes, HtmlElements, IsEmpty, IsPlantText, Name
'                     OnlyInnerText
' 
'         Function: GetPlantText, SingleNodeParser, ToString
' 
'         Sub: (+2 Overloads) Add
' 
'     Class InnerPlantText
' 
'         Properties: InnerText, IsEmpty, IsPlantText
' 
'         Function: GetPlantText, ToString
' 
'     Module SpecialHtmlElements
' 
'         Properties: Br, DocumentType, Head, Html, Title
' 
'         Function: IsBrChangeLine
' 
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace HTML

    ''' <summary>
    ''' 一个标签所标记的元素以及内部文本
    ''' </summary>
    Public Class HtmlElement : Inherits InnerPlantText

        ''' <summary>
        ''' 标签名
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TagName As String

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

        Default Public Property Attribute(name As String) As ValueAttribute
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

        Dim tagIndex As New Dictionary(Of String, List(Of HtmlElement))
        Dim classIndex As New Dictionary(Of String, List(Of HtmlElement))
        Dim nameIndex As New Dictionary(Of String, List(Of HtmlElement))
        Dim idIndex As New Dictionary(Of String, HtmlElement)

        Public Overrides Function GetPlantText() As String
            Dim sb As New StringBuilder(Me.InnerText)

            If Not Me.HtmlElements Is Nothing Then
                For Each node In HtmlElements
                    Call sb.Append(node.GetPlantText)
                Next
            End If

            Return sb.ToString
        End Function

        Public Sub Add(node As InnerPlantText)
            Call elementNodes.Add(node)

            If Not TypeOf node Is HtmlElement Then
                Return
            End If

            Dim element As HtmlElement = DirectCast(node, HtmlElement)
            Dim id As String = LCase(element.id)

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
        Public Function getElementById(id As String) As HtmlElement
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
        Public Function getElementsByClassName(classname As String) As HtmlElement()
            Static api As MethodInfo = GetType(HtmlElement).GetMethod(NameOf(getElementsByClassName))
            Return classIndex.TryGetValue(classname).JoinIterates(Query(api, classname)).ToArray
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
        Public Function getElementsByName(name As String) As HtmlElement()
            Static api As MethodInfo = GetType(HtmlElement).GetMethod(NameOf(getElementsByName))
            Return nameIndex.TryGetValue(name).JoinIterates(Query(api, name)).ToArray
        End Function

        Private Iterator Function Query(calls As MethodInfo, arg As String) As IEnumerable(Of HtmlElement)
            For Each node As HtmlElement In From obj As InnerPlantText
                                            In elementNodes
                                            Where TypeOf obj Is HtmlElement
                                            Select DirectCast(obj, HtmlElement)

                Dim collection As HtmlElement() = calls.Invoke(node, {arg})

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
            Return tagIndex.TryGetValue(tagname).JoinIterates(Query(api, tagname)).ToArray
        End Function

        Public Overrides Function ToString() As String
            Dim name As String = LCase(TagName)

            If attrs.Count = 0 Then
                Return $"<{name}>...</{name}>"
            Else
                Return $"<{name} {attrs.Values.JoinBy(" ")}>...</{name}>"
            End If
        End Function
    End Class
End Namespace
