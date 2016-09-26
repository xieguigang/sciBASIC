Imports System.Xml.Serialization

Namespace SVG

    ''' <summary>
    ''' SVG之中的画布对象，<see cref="SVGXml"/>和<see cref="g"/>都属于这种类型
    ''' </summary>
    Public Interface ICanvas
        Property transform As String
        Property texts As text()
        Property gs As g()
        Property path As path()
        Property rect As rect()
        Property polygon As polygon()
        Property lines As line()
        Property circles As circle()
        Property title As String
    End Interface

    ''' <summary>
    ''' SVG graphics unit
    ''' </summary>
    Public Class g : Inherits node
        Implements ICanvas

        <XmlAttribute> Public Property transform As String Implements ICanvas.transform
        <XmlElement("text")> Public Property texts As text() Implements ICanvas.texts
        <XmlElement("g")> Public Property gs As g() Implements ICanvas.gs
        <XmlElement> Public Property path As path() Implements ICanvas.path
        <XmlElement> Public Property rect As rect() Implements ICanvas.rect
        <XmlElement> Public Property polygon As polygon() Implements ICanvas.polygon
        <XmlElement("line")> Public Property lines As line() Implements ICanvas.lines
        <XmlElement("circle")> Public Property circles As circle() Implements ICanvas.circles
        <XmlAttribute> Public Property fill As String
        <XmlElement> Public Property title As String Implements ICanvas.title
    End Class
End Namespace