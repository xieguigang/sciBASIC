#Region "Microsoft.VisualBasic::58309957174e69b7821a47cfa5e1faca, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IO\docProps\core.xml.vb"

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

    '     Class core
    ' 
    '         Properties: created, creator, lastModifiedBy, modified
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: filePath, toXml
    ' 
    '     Structure W3CDTF
    ' 
    '         Properties: [Date], type
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports OpenXML = Microsoft.VisualBasic.MIME.Office.Excel.Model.Xmlns

Namespace XML.docProps

    <XmlRoot("coreProperties", [Namespace]:=OpenXML.cp)>
    Public Class core : Inherits IXml

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces

            xmlns.Add("cp", OpenXML.cp)
            xmlns.Add("dc", OpenXML.dc)
            xmlns.Add("dcterms", OpenXML.dcterms)
            xmlns.Add("dcmitype", OpenXML.dcmitype)
            xmlns.Add("xsi", OpenXML.xsi)
        End Sub

        <XmlElement(ElementName:=NameOf(creator), [Namespace]:=OpenXML.dc)>
        Public Property creator As String
        <XmlElement(ElementName:=NameOf(lastModifiedBy), [Namespace]:=OpenXML.cp)>
        Public Property lastModifiedBy As String
        <XmlElement(NameOf(created), [Namespace]:=OpenXML.dcterms)>
        Public Property created As W3CDTF
        <XmlElement(NameOf(modified), [Namespace]:=OpenXML.dcterms)>
        Public Property modified As W3CDTF

        Protected Overrides Function filePath() As String
            Return "docProps/core.xml"
        End Function

        Protected Overrides Function toXml() As String
            Return Me.GetXml
        End Function
    End Class

    Public Structure W3CDTF
        <XmlAttribute("type", [Namespace]:=OpenXML.xsi)>
        Public Property type As String
        <XmlText> Public Property [Date] As Date
    End Structure
End Namespace
