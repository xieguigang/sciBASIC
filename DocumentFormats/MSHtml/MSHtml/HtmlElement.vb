Imports System.Runtime.CompilerServices
Imports System.Net
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Text
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Public Structure ValueAttribute : Implements sIdEnumerable

    Public Property Name As String Implements sIdEnumerable.Identifier
    Public Property Value As String

    Sub New(strText As String)
        Dim ep As Integer = InStr(strText, "=")
        Name = Mid(strText, 1, ep - 1)
        Value = Mid(strText, ep + 1)
        If Value.First = """"c AndAlso Value.Last = """"c Then
            Value = Mid(Value, 2, Len(Value) - 2)
        End If
    End Sub

    Sub New(name As String, value As String)
        Me.Name = name
        Me.Value = value
    End Sub

    Public Overrides Function ToString() As String
        Return $"{Name}=""{Value}"""
    End Function
End Structure

''' <summary>
''' 一个标签所标记的元素以及内部文本
''' </summary>
Public Class HtmlElement : Inherits PlantText

    Public Const HTML_ELEMENT_REGEX As String = "<[^/].+?>"
    Public Const HTML_SINGLE_ELEMENT As String = "<[^/].+? />"

    Public Const ATTRIBUTE_STRING As String = "\S+="".+?"""

    ''' <summary>
    ''' 标签名
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Name As String
    ''' <summary>
    ''' 标签的属性列表
    ''' </summary>
    ''' <returns></returns>
    Public Property Attributes As ValueAttribute()
        Get
            Return __attrsArray
        End Get
        Set(value As ValueAttribute())
            __attrsArray = value

            If __attrsArray.IsNullOrEmpty Then
                __attrs = New Dictionary(Of ValueAttribute)
            Else
                __attrs = value.ToDictionary
            End If
        End Set
    End Property

    Public Property HtmlElements As PlantText()
        Get
            Return __elementNodes.ToArray
        End Get
        Set(value As PlantText())
            If value.IsNullOrEmpty Then
                __elementNodes = New List(Of PlantText)
            Else
                __elementNodes = value.ToList
            End If
        End Set
    End Property

    Default Public Property Attribute(name As String) As ValueAttribute
        Get
            Return __attrs.TryGetValue(name)
        End Get
        Set(value As ValueAttribute)
            If __attrs.ContainsKey(name) Then
                __attrs(name) = value
            Else
                Call __attrs.Add(name, value)
            End If
        End Set
    End Property

    Public Overrides ReadOnly Property IsPlantText As Boolean
        Get
            Return False
        End Get
    End Property

    Dim __attrs As Dictionary(Of ValueAttribute)
    Dim __attrsArray As ValueAttribute()
    Dim __elementNodes As List(Of PlantText) = New List(Of PlantText)

    Public Overrides Function GetPlantText() As String
        Dim sbr As StringBuilder = New StringBuilder(Me.InnerText)

        If Not Me.HtmlElements Is Nothing Then
            For Each node In HtmlElements
                Call sbr.Append(node.GetPlantText)
            Next
        End If

        Return sbr.ToString
    End Function

    Public Sub Add(attr As ValueAttribute)
        If __attrs Is Nothing Then
            __attrs = New Dictionary(Of ValueAttribute)
        End If
        Call __attrs.Add(attr.Name, attr)
    End Sub

    Public Sub Add(Node As PlantText)
        Call __elementNodes.Add(Node)
    End Sub

    Public ReadOnly Property OnlyInnerText As Boolean
        Get
            Return __elementNodes.Count = 1 AndAlso
                __elementNodes(Scan0).IsPlantText
        End Get
    End Property

    Public Overrides ReadOnly Property IsEmpty As Boolean
        Get
            Return MyBase.IsEmpty AndAlso
                String.IsNullOrEmpty(Name) AndAlso
                Attributes.IsNullOrEmpty AndAlso
                HtmlElements.IsNullOrEmpty
        End Get
    End Property

    Public Shared Function SingleNodeParser(value As String) As HtmlElement
        Return TextParse(value).As(Of HtmlElement)
    End Function

    ''' <summary>
    ''' 解析标签开始和结束的位置之间的内部html文本
    ''' </summary>
    ''' <param name="doc"></param>
    ''' <returns></returns>
    ''' <remarks>这个方法是最开始的解析函数，非递归的</remarks>
    Public Shared Function TextParse(ByRef doc As String) As PlantText
        Dim strElement As String = Regex.Match(doc, HTML_ELEMENT_REGEX).Value  ' 得到开始的标签

        If String.IsNullOrEmpty(strElement) Then
            Return New PlantText With {.InnerText = doc} '找不到开始的标签，则为纯文本
        End If

        Dim p As Integer

        If String.Equals(strElement, SpecialHtmlElements.HTML_DOCTYPE, StringComparison.OrdinalIgnoreCase) Then
            p = InStr(doc, SpecialHtmlElements.HTML_DOCTYPE)
            doc = Mid(doc, p + Len(SpecialHtmlElements.HTML_DOCTYPE) + 1)
            Return SpecialHtmlElements.DocumentType
        End If

        Dim el As HtmlElement = New HtmlElement With {
            .Name = HtmlElement.__getElementName(strElement),
            .Attributes = __getElementAttrs(strElement)
        }

        p = InStr(doc, strElement)  '由于前面的文本已经解析完了，所以前面的文本全部扔掉
        doc = Mid(doc, p + Len(strElement))

        If String.Equals(el.Name, "?xml", StringComparison.OrdinalIgnoreCase) Then
            '这个是在解析XML文档，并且这个是头部，则跳过后续
            Return el
        End If

        '解析内部文本

        Do While True
            Dim parentEnd As Boolean = False
            Dim node As PlantText = __innerTextParser(doc, el.Name, parentEnd)
            If node Is Nothing Then
                Exit Do
            End If

            If Not node.IsEmpty Then
                Call el.Add(node)
            End If

            If parentEnd Then
                Exit Do
            End If
        Loop

        Return el
    End Function

    ''' <summary>
    ''' 在得到一个标签之后前面的数据会被扔掉，开始解析标签后面的数据
    ''' </summary>
    ''' <param name="innerText"></param>
    ''' <param name="parent"></param>
    ''' <returns>这个函数是一个递归函数</returns>
    Private Shared Function __innerTextParser(ByRef innerText As String,
                                              parent As String,
                                              ByRef parentEnd As Boolean) As PlantText
        If String.IsNullOrEmpty(innerText) Then
            Return Nothing
        End If

        Dim strElement = Regex.Match(innerText, HTML_ELEMENT_REGEX).Value '匹配下一个标签
        Dim p As Integer = InStr(innerText, strElement)
        ' 下一个标签和父节点标签之间的文本为内部文本
        Dim innerDoc As String = Mid(innerText, 1, p - 1)  ' 如果内部文档里面含有父节点的结束标签，则父节点结束
        parent = $"</{parent}>"
        Dim endTag As String = Regex.Match(innerDoc, parent, RegexOptions.IgnoreCase).Value
        If Not String.IsNullOrEmpty(endTag) Then
            Dim innerLen As Integer = Len(innerDoc)

            p = InStr(innerDoc, endTag)
            innerDoc = Mid(innerDoc, 1, p - 1)
            parentEnd = True

            If p = 1 Then
                innerLen = Len(endTag)
            Else

            End If

            innerText = Mid(innerText, 1 + innerLen)

            Return New PlantText With {.InnerText = innerDoc}
        End If

        If Not String.IsNullOrEmpty(innerDoc) Then
            ' 这部分的文本是纯文本，也是父节点的一部分
            innerText = Mid(innerText, Len(innerDoc) + 1)
            parentEnd = False
            Return New PlantText With {.InnerText = innerDoc}
        End If

        If String.IsNullOrEmpty(strElement) Then '准备结束了，因为已经没有新的节点了
            p = InStr(innerText, parent, CompareMethod.Text)
            Dim lenth = p - 1
            If lenth < 0 Then
                lenth = 0
            End If
            innerDoc = Mid(innerText, 1, lenth)
            innerText = Mid(innerText, p + Len(parent))
            parentEnd = True
            Return New PlantText With {.InnerText = innerDoc}
        End If

        '新的子节点的解析开始了

        Dim x As HtmlElement = New HtmlElement With {
            .Name = HtmlElement.__getElementName(strElement),
            .Attributes = __getElementAttrs(strElement)
        }

        innerText = Mid(innerText, Len(strElement) + 1) '由于父节点的内部文本已经在前面清除掉了，所以这里的子节点直接从第一个字符开始

        If SpecialHtmlElements.IsBrChangeLine(strElement) Then
            parentEnd = False
            Return x
        End If

        If String.Equals(x.Name, "img", StringComparison.OrdinalIgnoreCase) Then
            parentEnd = False
            Return x
        End If

        If String.Equals(x.Name, "meta", StringComparison.OrdinalIgnoreCase) Then
            If String.Equals(parent, "</head>", StringComparison.OrdinalIgnoreCase) Then
                Return x '头部区域的元数据，没有子节点的
            End If
        End If

        Do While True
            Dim innerParentEnd As Boolean = False
            Dim node As PlantText = __innerTextParser(innerText, x.Name, innerParentEnd)
            If node Is Nothing Then
                Exit Do
            End If

            If Not node.IsEmpty Then
                Call x.Add(node)
            End If

            If innerParentEnd Then
                Exit Do
            End If
        Loop

        parentEnd = False

        Return x
    End Function

    Private Shared Function __getElementAttrs(s As String) As ValueAttribute()
        Dim tokens As String() = CommandLine.GetTokens(s).Skip(1).ToArray
        If tokens.Length > 0 Then
            tokens(tokens.Length - 1) =
                Mid(tokens.Last, 1, Len(tokens.Last) - 1)
        End If
        Return tokens _
            .TakeWhile(AddressOf __takesWhile) _
            .ToArray(Function(attr) New ValueAttribute(attr))
    End Function

    Private Shared Function __takesWhile(attr As String) As Boolean
        Return Not String.Equals(attr, "\") AndAlso Not String.Equals(attr, "/")
    End Function

    Private Shared Function __getElementName(s As String) As String
        Dim p As Integer = InStr(s, " ")
        If p = 0 Then
            Return Mid(s, 2, Len(s) - 2)
        Else
            Dim Name As String = Mid(s, 2, p - 2)
            Return Name
        End If
    End Function

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class

''' <summary>
''' 纯文本对象
''' </summary>
Public Class PlantText

    Public Overridable Property InnerText As String

    Public Overrides Function ToString() As String
        Return InnerText
    End Function

    Public Overridable ReadOnly Property IsEmpty As Boolean
        Get
            Return String.IsNullOrEmpty(InnerText)
        End Get
    End Property

    Public Overridable ReadOnly Property IsPlantText As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overridable Function GetPlantText() As String
        Return InnerText
    End Function
End Class

Public Module SpecialHtmlElements

    Public Const HTML_DOCTYPE As String = "<!DOCTYPE HTML>"

    Public Function IsBrChangeLine(str As String) As Boolean
        Return Regex.Match(str, "<br \s*/>").Success
    End Function

    Public ReadOnly Property Title As HtmlElement
        Get
            Return New HtmlElement With {.Name = "title"}
        End Get
    End Property

    Public ReadOnly Property DocumentType As HtmlElement
        Get
            Return New HtmlElement With {.Name = HTML_DOCTYPE}
        End Get
    End Property

    Public ReadOnly Property Html As HtmlElement
        Get
            Return New HtmlElement With {.Name = "html"}
        End Get
    End Property

    Public ReadOnly Property Br As HtmlElement
        Get
            Return New HtmlElement With {.Name = "br"}
        End Get
    End Property

    Public ReadOnly Property Head As HtmlElement
        Get
            Return New HtmlElement With {.Name = "head"}
        End Get
    End Property
End Module