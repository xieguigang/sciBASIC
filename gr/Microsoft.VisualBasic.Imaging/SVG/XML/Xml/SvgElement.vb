Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.Imaging.SVG.XML.Enums
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The basically SVG XML document node, it can be tweaks on the style by using CSS
    ''' </summary>
    Public MustInherit Class SvgElement

        Protected ReadOnly Element As XmlElement

        ''' <summary>
        ''' html element node id liked identifer 
        ''' </summary>
        ''' <returns></returns>
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

        ''' <summary>
        ''' the stroke color, value of this property should be html color code
        ''' </summary>
        ''' <returns></returns>
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

        ''' <summary>
        ''' The stroke-dasharray attribute is a presentation attribute defining the pattern of 
        ''' dashes and gaps used to paint the outline of the shape;
        ''' </summary>
        ''' <returns></returns>
        Public Property StrokeDashArray() As Double()
            Get
                Return Element.GetAttribute("stroke-dasharray", Attributes.FillAndStroke.StrokeDashArray)
            End Get
            Set(value As Double())
                Element.SetAttribute("stroke-dasharray", value.JoinBy(" "))
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

        ''' <summary>
        ''' current element node css style
        ''' </summary>
        ''' <returns></returns>
        Public Property Style As String
            Get
                Return Element.GetAttribute("style")
            End Get
            Set(value As String)
                Element.SetAttribute("style", value)
            End Set
        End Property

        Protected Friend Sub New(element As XmlElement)
            If element Is Nothing Then
                Throw New ArgumentNullException(NameOf(element))
            Else
                Me.Element = element
            End If
        End Sub

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function ParseStyleAttribute() As Dictionary(Of String, String)
            Return CssParser.ParseStyle(Element.GetAttribute("style")).Properties
        End Function

        Private Sub SetStyleAttribute(styles As IReadOnlyDictionary(Of String, String))
            If styles Is Nothing OrElse Not styles.Any() Then
                Element.RemoveAttribute("style")
                Return
            End If

            Dim value = String.Join(";", styles.[Select](Function(kvp) $"{kvp.Key}: {kvp.Value}"))
            Element.SetAttribute("style", value)
        End Sub

        Public Shared Function Create(Of T As SvgElement)(e As XmlElement) As SvgElement
            Select Case GetType(T)
                Case GetType(SvgCircle) : Return New SvgCircle(e)
                Case GetType(SvgRect) : Return New SvgRect(e)
                Case GetType(SvgLine) : Return New SvgLine(e)
                Case GetType(SvgEllipse) : Return New SvgEllipse(e)
                Case GetType(SvgImage) : Return New SvgImage(e)
                Case GetType(SvgPath) : Return New SvgPath(e)
                Case GetType(SvgPolygon) : Return New SvgPolygon(e)
                Case GetType(SvgPolyLine) : Return New SvgPolyLine(e)
                Case GetType(SvgText) : Return New SvgText(e)
                Case GetType(SvgTitle) : Return New SvgTitle(e)

                Case GetType(SvgGroup) : Return New SvgGroup(e)
                Case GetType(SvgClipPath) : Return New SvgClipPath(e)
                Case GetType(SvgMarker) : Return New SvgMarker(e)

                Case Else
                    Throw New NotImplementedException(GetType(T).FullName)
            End Select
        End Function

        ''' <summary>
        ''' create svg element based on the given <see cref="XmlElement.Name"/>
        ''' </summary>
        ''' <param name="e"></param>
        ''' <returns></returns>
        Public Shared Function Create(e As XmlElement) As SvgElement
            Select Case Strings.LCase(e.Name)
                Case "circle" : Return New SvgCircle(e)
                Case "rect" : Return New SvgRect(e)
                Case "line" : Return New SvgLine(e)
                Case "ellipse" : Return New SvgEllipse(e)
                Case "image" : Return New SvgImage(e)
                Case "path" : Return New SvgPath(e)
                Case "polygon" : Return New SvgPolygon(e)
                Case "polyline" : Return New SvgPolyLine(e)
                Case "text" : Return New SvgText(e)
                Case "title" : Return New SvgTitle(e)

                Case "g" : Return New SvgGroup(e)
                Case "clipPath" : Return New SvgClipPath(e)
                Case "marker" : Return New SvgMarker(e)

                Case Else
                    Throw New NotImplementedException(e.Name)
            End Select
        End Function
    End Class
End Namespace
