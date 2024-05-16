
Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;symbol> element is used to define graphical template 
    ''' objects which can be instantiated by a &lt;use> element.
    ''' 
    ''' The use Of &lt;symbol> elements For graphics that are used multiple 
    ''' times In the same document adds Structure And semantics. Documents 
    ''' that are rich In Structure may be rendered graphically, As speech,
    ''' Or As Braille, And thus promote accessibility.
    ''' </summary>
    Public Class SvgSymbol : Inherits SvgContainer

        Public Property overflow As String
            Get
                Return Element.GetAttribute("overflow", defaultValue:="")
            End Get
            Set(value As String)
                Element.SetAttribute("overflow", value)
            End Set
        End Property

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgSymbol
            Dim element = parent.OwnerDocument.CreateElement("symbol")
            parent.AppendChild(element)
            Return New SvgSymbol(element)
        End Function
    End Class
End Namespace