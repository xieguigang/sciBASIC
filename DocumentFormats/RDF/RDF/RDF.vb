Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.DocumentFormat.RDF.DocumentStream

''' <summary>
''' 做序列化的时候请务必要添加一个自定义的属性：&lt;XmlType(RDF.RDF_PREFIX &amp; "RDF")>
''' </summary>
Public Class RDF

    Public Const RDF_PREFIX As String = "rdf-"

    ' <XmlElement(RDF.RDF_PREFIX & "Description")>
    '   Public Property ResourceDescription As RDFResourceDescription

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Text">参数值为文件之中的字符串内容，而非文件的路径</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function LoadDocument(Text As String) As RDF
        Dim sBuilder As StringBuilder = New StringBuilder(Text, 1024)
        Call sBuilder.Replace("rdf:", RDF.RDF_PREFIX)
        Dim Document As GenericXmlDocument = GenericXmlDocument.CreateObjectFromXmlText(sBuilder.ToString)
        Dim Description As String = Document.DocumentNodes.First.InternalText
        ' Dim doc As New RDF With {
        ' .ResourceDescription = Description.CreateObjectFromXmlFragment(Of RDFResourceDescription)()
        '     }
        '    doc.ResourceDescription.InternalText = Description
        '   Return doc
    End Function

    Public Shared Function LoadDocument(Of T As RDF)(path As String, Proc As Func(Of StringBuilder, String)) As T
        Return path.LoadXml(Of T)(preprocess:=AddressOf New __docHelper With {.Proc = Proc}.ProcDoc)
    End Function

    Private Structure __docHelper
        Public Proc As Func(Of StringBuilder, String)

        Const XmlNs As String = "xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"""

        Public Function ProcDoc(doc As String) As String
            Dim sb As New StringBuilder(Regex.Replace(doc, "<rdf:RDF.+?>", $"<rdf:RDF {XmlNs} >", RegexICSng))

            Call sb.Replace("<rdf:", "<" & RDF.RDF_PREFIX)
            Call sb.Replace("</rdf:", "</" & RDF.RDF_PREFIX)
            Call sb.Replace(" rdf:", " " & RDF.RDF_PREFIX)
            Call sb.Replace(" xmlns:rdf=", " xmlns=")

            Return Proc(sb)
        End Function
    End Structure

    ''' <summary>
    ''' 将RDF对象转换为XML文件之中的字符串
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function ToString() As String
        Dim sBuilder As StringBuilder = New StringBuilder(Me.GetXml, capacity:=1024)
        Call sBuilder.Replace(RDF_PREFIX, "rdf:")
        Call sBuilder.Replace("&lt;", "<")
        Call sBuilder.Replace("&gt;", ">")
        Call sBuilder.Replace("<?xml version=""1.0"" encoding=""utf-16""?>", "")
        Call sBuilder.Replace(" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", "")
        Call sBuilder.Replace("<rdf:Description>", "")

        Dim value As String = sBuilder.ToString
        value = Regex.Replace(value, "</rdf:Description>.*</rdf:Description>", "</rdf:Description>")
        '  Dim AboutValue As String = Regex.Match(value, "rdf:about="".+?"">", RegexOptions.Singleline).Value
        '  value = value.Replace(AboutValue, AboutValue.Replace(""">", """ />"))

        ' <rdf:Description xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
        ' <rdf:Description xmlns:xsd=.+? xmlns:xsi=.+?>
        value = Regex.Replace(value, "<rdf:Description xmlns:xsd=.+? xmlns:xsi=.+?>", "")

        Return value
    End Function
End Class