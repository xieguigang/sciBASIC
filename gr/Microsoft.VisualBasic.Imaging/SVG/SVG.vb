Imports System.Drawing
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG

    ''' <summary>
    ''' The svg vector graphics in Xml document format.
    ''' </summary>
    <XmlType("svg")> Public Class SVGXml : Inherits g
        Implements ISaveHandle

#Region "xml root property"

        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property height As String
        <XmlAttribute> Public Property version As String
        <XmlAttribute> Public Property viewBox As Double()
#End Region

        ''' <summary>
        ''' Style definition of the xml node in this svg document. 
        ''' you can define the style by using css and set the class 
        ''' attribute for the specific node to controls the 
        ''' visualize style.
        ''' </summary>
        ''' <returns></returns>
        Public Property defs As CSSStyles
        <XmlElement("style")> Public Property styles As XmlMeta.CSS()
        <XmlElement("image")> Public Property images As Image()

        Public Sub SetSize(size As Size)
            width = size.Width & "px"
            height = size.Height & "px"
        End Sub

        Public Shared Function TryLoad(xml As String) As SVGXml
            Dim xmlDoc As New XmlDoc(xml)
            xmlDoc.xmlns.xmlns = ""
            Dim sb As New StringBuilder(xmlDoc.ToString)
            Call sb.Replace("xlink:href=""", "image.data=""")
            Return sb.ToString.LoadFromXml(Of SVGXml)(throwEx:=True)
        End Function

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Private Function SaveAsXml(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
            Dim sb As New StringBuilder(Me.GetXml)
            Call sb.Replace("image.data=""", "xlink:href=""")

            Dim xml As New XmlDoc(sb.ToString)
            xml.encoding = XmlEncodings.UTF8
            xml.standalone = False
            xml.xmlns.Set("xlink", "http://www.w3.org/1999/xlink")
            xml.xmlns.xmlns = "http://www.w3.org/2000/svg"

            Return xml.SaveTo(Path, encoding)
        End Function

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Function SaveAsXml(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return SaveAsXml(Path, encoding.GetEncodings)
        End Function
    End Class
End Namespace