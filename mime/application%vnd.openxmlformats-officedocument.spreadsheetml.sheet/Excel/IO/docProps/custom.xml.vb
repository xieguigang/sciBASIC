#Region "Microsoft.VisualBasic::569f56ad7990a64f681bca8cf411e33e, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IO\docProps\custom.xml.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class custom
    ' 
    '         Properties: properties
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: filePath, toXml
    ' 
    '     Class [property]
    ' 
    '         Properties: fmtid, lpwstr, name, pid
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports OpenXML = Microsoft.VisualBasic.MIME.Office.Excel.Model.Xmlns

Namespace XML.docProps

    <XmlRoot("Properties", [Namespace]:="http://schemas.openxmlformats.org/officeDocument/2006/custom-properties")>
    Public Class custom : Inherits IXml

        <XmlElement("property")>
        Public Property properties As [property]()

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces
            xmlns.Add("vt", OpenXML.vt)
        End Sub

        Protected Overrides Function filePath() As String
            Return "docProps/custom.xml"
        End Function

        Protected Overrides Function toXml() As String
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class [property]

        <XmlAttribute> Public Property fmtid As String
        <XmlAttribute> Public Property pid As String
        <XmlAttribute> Public Property name As String

        <XmlElement(NameOf(lpwstr), [Namespace]:=OpenXML.vt)>
        Public Property lpwstr As String

        Public Overrides Function ToString() As String
            Return lpwstr
        End Function
    End Class
End Namespace
