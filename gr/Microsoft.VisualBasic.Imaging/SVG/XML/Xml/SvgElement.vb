Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml

Namespace SvgLib

    Public MustInherit Class SvgElement

        Protected ReadOnly Element As XmlElement

        Protected Sub New(element As XmlElement)
            If element Is Nothing Then
                Throw New ArgumentNullException(NameOf(element))
            Else
                Me.Element = element
            End If
        End Sub

        Public Property Id As String
            Get
                Return Element.GetAttribute("id")
            End Get
            Set(value As String)
                Element.SetAttribute("id", value)
            End Set
        End Property

        Public Property TabIndex As Integer?
            Get
                Return Element.GetAttribute("tabindex", CType(Nothing, Integer?))
            End Get
            Set(value As Integer?)
                Element.SetAttribute("tabindex", value)
            End Set
        End Property

        ' TODO Add https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/Presentation

        Public Property Fill As String
            Get
                Return Element.GetAttribute("fill", Attributes.FillAndStroke.Fill)
            End Get
            Set(value As String)
                Element.SetAttribute("fill", value)
            End Set
        End Property

        Public Property FillOpacity As Double
            Get
                Return Element.GetAttribute("fill-opacity", Attributes.FillAndStroke.FillOpacity)
            End Get
            Set(value As Double)
                Element.SetAttribute("fill-opacity", value)
            End Set
        End Property

        Public Property Stroke As String
            Get
                Return Element.GetAttribute("stroke", Attributes.FillAndStroke.Stroke)
            End Get
            Set(value As String)
                Element.SetAttribute("stroke", value)
            End Set
        End Property

        Public Property StrokeOpacity As Double
            Get
                Return Element.GetAttribute("stroke-opacity", Attributes.FillAndStroke.StrokeOpacity)
            End Get
            Set(value As Double)
                Element.SetAttribute("stroke-opacity", value)
            End Set
        End Property

        Public Property StrokeWidth As Double
            Get
                Return Element.GetAttribute("stroke-width", Attributes.FillAndStroke.StrokeWidth)
            End Get
            Set(value As Double)
                Element.SetAttribute("stroke-width", value)
            End Set
        End Property

        Public Property StrokeLineCap As SvgStrokeLineCap
            Get
                Return Element.GetAttribute(Of SvgStrokeLineCap)("stroke-linecap", Attributes.FillAndStroke.StrokeLineCap)
            End Get
            Set(value As SvgStrokeLineCap)
                Element.SetAttribute("stroke-linecap", value)
            End Set
        End Property

        Public Property Transform As String
            Get
                Return Element.GetAttribute("transform")
            End Get
            Set(value As String)
                Element.SetAttribute("transform", value)
            End Set
        End Property

        Public Property Visible As Boolean
            Get
                Return Not Equals(GetStyle("display"), "none")
            End Get
            Set(value As Boolean)
                SetStyle("display", If(value, String.Empty, "none"))
            End Set
        End Property

        Public Function GetClasses() As IEnumerable(Of String)
            Return ParseClassAttribute()
        End Function

        Public Function HasClass(name As String) As Boolean
            Return GetClasses().Contains(name)
        End Function

        Public Sub AddClass(name As String)
            Dim classes = ParseClassAttribute()
            classes.Add(name)
            SetClassAttribute(classes)
        End Sub

        Public Sub RemoveClass(name As String)
            Dim classes = ParseClassAttribute()
            classes.Remove(name)
            SetClassAttribute(classes)
        End Sub

        Public Sub ToggleClass(name As String)
            If HasClass(name) Then
                RemoveClass(name)
            Else
                AddClass(name)
            End If
        End Sub

        Protected Function GetStyle(name As String) As String
            Dim styles = ParseStyleAttribute()
            Return styles(name)
        End Function

        Protected Sub SetStyle(name As String, value As String)
            Dim styles = ParseStyleAttribute()
            styles(name) = value
            SetStyleAttribute(styles)
        End Sub

        Private Function ParseClassAttribute() As HashSet(Of String)
            Return New HashSet(Of String)(Element.GetAttribute("class").Split({" "c}, StringSplitOptions.RemoveEmptyEntries))
        End Function

        Private Sub SetClassAttribute(classes As IEnumerable(Of String))
            If classes Is Nothing OrElse Not classes.Any() Then
                Element.RemoveAttribute("class")
                Return
            End If

            Dim value = String.Join(" ", classes)
            Element.SetAttribute("class", value)
        End Sub

        Private Function ParseStyleAttribute() As Dictionary(Of String, String)
            Return Element.GetAttribute("style").Split({";"c}, StringSplitOptions.RemoveEmptyEntries).[Select](Function(x) x.Split({":"c})).Where(Function(x) x.Length = 2).ToDictionary(Function(x) x(0).Trim(), Function(x) x(1).Trim(), StringComparer.OrdinalIgnoreCase)
        End Function

        Private Sub SetStyleAttribute(styles As IReadOnlyDictionary(Of String, String))
            If styles Is Nothing OrElse Not styles.Any() Then
                Element.RemoveAttribute("style")
                Return
            End If

            Dim value = String.Join(";", styles.[Select](Function(kvp) $"{kvp.Key}: {kvp.Value}"))
            Element.SetAttribute("style", value)
        End Sub
    End Class
End Namespace
