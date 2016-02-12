Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.DocumentFormat.RDF.DocumentStream

Namespace DocumentElements

    <Xml.Serialization.XmlType(RDF.RDF_PREFIX & "RDF")>
    Public Class RDF

        Public Const RDF_PREFIX As String = "rdf__"

        <Xml.Serialization.XmlElement(RDF.RDF_PREFIX & "Description")>
        Public Property ResourceDescription As RDFResourceDescription

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Text">参数值为文件之中的字符串内容，而非文件的路径</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function LoadDocument(Text As String) As RDF
            Dim sBuilder As StringBuilder = New StringBuilder(Text, 1024)
            Call sBuilder.Replace("rdf:", DocumentElements.RDF.RDF_PREFIX)
            Dim Document As GenericXmlDocument = GenericXmlDocument.CreateObjectFromXmlText(sBuilder.ToString)
            Dim Description As String = Document.DocumentNodes.First.InternalText
            Dim RDF = New RDF With {
                .ResourceDescription = Description.CreateObjectFromXmlFragment(Of RDFResourceDescription)()
            }
            RDF.ResourceDescription.InternalText = Description
            Return RDF
        End Function

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

    <Xml.Serialization.XmlType(RDF.RDF_PREFIX & "Description")>
    Public Class RDFResourceDescription

        <Xml.Serialization.XmlAttribute(RDF.RDF_PREFIX & "about")>
        Public Property About As String

        <Xml.Serialization.XmlText> Public Property InternalText As String

        Public Overrides Function ToString() As String
            Return InternalText
        End Function
    End Class
End Namespace
