Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' SVG: Scalable Vector Graphics
    ''' 
    ''' Scalable Vector Graphics (SVG) is an XML-based markup language for describing 
    ''' two-dimensional based vector graphics.
    '''
    ''' As such, it's a text-based, open Web standard for describing images that can 
    ''' be rendered cleanly at any size and are designed specifically to work well with 
    ''' other web standards including CSS, DOM, JavaScript, and SMIL. SVG is, 
    ''' essentially, to graphics what HTML is to text.
    '''
    ''' SVG images And their related behaviors are defined In XML text files, which means
    ''' they can be searched, indexed, scripted, And compressed. Additionally, this means 
    ''' they can be created And edited With any text editor Or With drawing software.
    '''
    ''' Compared to classic bitmapped image formats such as JPEG Or PNG, SVG-format vector 
    ''' images can be rendered at any size without loss of quality And can be easily 
    ''' localized by updating the text within them, without the need of a graphical editor
    ''' to do so. With proper libraries, SVG files can even be localized on-the-fly.
    '''
    ''' SVG has been developed by the World Wide Web Consortium (W3C) since 1999.
    ''' </summary>
    Public NotInheritable Class SvgDocument : Inherits SvgContainer
        Implements ISaveHandle

        ReadOnly _document As XmlDocument

        Public Property X As Double
            Get
                Return Element.GetAttribute("x", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x", value)
            End Set
        End Property

        Public Property Y As Double
            Get
                Return Element.GetAttribute("y", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y", value)
            End Set
        End Property

        Public Property Width As Double
            Get
                Return Element.GetAttribute("width", Attributes.Size.Width)
            End Get
            Set(value As Double)
                Element.SetAttribute("width", value)
            End Set
        End Property

        Public Property Height As Double
            Get
                Return Element.GetAttribute("height", Attributes.Size.Height)
            End Get
            Set(value As Double)
                Element.SetAttribute("height", value)
            End Set
        End Property

        Public Property ViewBox As SvgViewBox
            Get
                Return Element.GetAttribute("viewBox", New SvgViewBox())
            End Get
            Set(value As SvgViewBox)
                Element.SetAttribute("viewBox", value.ToString())
            End Set
        End Property

        Private Sub New(document As XmlDocument, element As XmlElement)
            MyBase.New(element)
            _document = document
        End Sub

        ''' <summary>
        ''' set svg canvas size and viewbox size
        ''' </summary>
        ''' <param name="sz"></param>
        ''' <returns></returns>
        Public Function Size(sz As Size) As SvgDocument
            Width = sz.Width
            Height = sz.Height
            ViewBox = New Double() {0, 0, sz.Width, sz.Height}
            Return Me
        End Function

        Public Function Background(bg As String) As SvgDocument
            Call SetStyle("background", bg)
            Return Me
        End Function

        Public Overloads Shared Function Create() As SvgDocument
            Dim document = New XmlDocument()
            Dim rootElement = document.CreateElement("svg")
            document.AppendChild(rootElement)
            rootElement.SetAttribute("xmlns", "http://www.w3.org/2000/svg")
            rootElement.SetAttribute("preserveAspectRatio", "xMaxYMax")
            Return New SvgDocument(document, rootElement)
        End Function

        Public Sub Save(stream As Stream)
            Call _document.Save(stream)
            Call stream.Flush()
        End Sub

        Public Sub SetCommentText(text As String)
            Call _document.CreateComment(text)
        End Sub

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function SaveAsXml(path$, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Dim s As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True)
            Call Save(s)
            Call s.Dispose()
            Return True
        End Function

        ''' <summary>
        ''' 将当前的这个SVG对象序列化为XML字符串文本
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSVGXml() As String
            Dim s As New MemoryStream
            Call Save(s)
            Return Encoding.UTF8.GetString(s.ToArray)
        End Function

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SaveAsXml(path$, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return SaveAsXml(path, encoding.CodePage)
        End Function
    End Class
End Namespace
