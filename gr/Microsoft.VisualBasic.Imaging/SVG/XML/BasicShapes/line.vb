Imports System.Drawing
Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;line> element takes the positions of two points as 
    ''' parameters and draws a straight line between them.
    ''' </summary>
    Public NotInheritable Class SvgLine : Inherits SvgBasicShape

        ''' <summary>
        ''' The x position of point 1.
        ''' </summary>
        ''' <returns></returns>
        Public Property X1 As Double
            Get
                Return Element.GetAttribute("x1", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x1", value)
            End Set
        End Property

        ''' <summary>
        ''' The y position of point 1.
        ''' </summary>
        ''' <returns></returns>
        Public Property Y1 As Double
            Get
                Return Element.GetAttribute("y1", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y1", value)
            End Set
        End Property

        ''' <summary>
        ''' The x position of point 2.
        ''' </summary>
        ''' <returns></returns>
        Public Property X2 As Double
            Get
                Return Element.GetAttribute("x2", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x2", value)
            End Set
        End Property

        ''' <summary>
        ''' The y position of point 2.
        ''' </summary>
        ''' <returns></returns>
        Public Property Y2 As Double
            Get
                Return Element.GetAttribute("y2", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y2", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Public Sub SetPoint(a As PointF, b As PointF)
            X1 = a.X
            Y1 = a.Y
            X2 = b.X
            Y2 = b.Y
        End Sub

        Public Sub SetPoint(x1 As Double, y1 As Double, x2 As Double, y2 As Double)
            Me.X1 = x1
            Me.X2 = x2
            Me.Y1 = y1
            Me.Y2 = y2
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgLine
            Dim element = parent.OwnerDocument.CreateElement("line")
            parent.AppendChild(element)
            Return New SvgLine(element)
        End Function
    End Class
End Namespace
