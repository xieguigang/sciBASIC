#Region "Microsoft.VisualBasic::d7d11b95ddd248e0b60b4c4565a9e7a9, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\docProps\core.xml.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 63
    '    Code Lines: 46
    ' Comment Lines: 0
    '   Blank Lines: 17
    '     File Size: 2.09 KB


    '     Class core
    ' 
    '         Properties: category, contentStatus, created, creator, description
    '                     keywords, lastModifiedBy, modified, subject, title
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
Imports OpenXML = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model.Xmlns

Namespace XLSX.XML.docProps

    <XmlRoot("coreProperties", [Namespace]:=OpenXML.cp)>
    Public Class core : Implements IXml

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

        Public Property title As String
        Public Property subject As String

        <XmlElement([Namespace]:=OpenXML.cp)>
        Public Property keywords As String

        <XmlElement([Namespace]:=OpenXML.dc)>
        Public Property description As String

        <XmlElement([Namespace]:=OpenXML.cp)>
        Public Property category As String

        <XmlElement([Namespace]:=OpenXML.cp)>
        Public Property contentStatus As String

        Protected Function filePath() As String Implements IXml.filePath
            Return "docProps/core.xml"
        End Function

        Protected Function toXml() As String Implements IXml.toXml
            Return Me.GetXml
        End Function
    End Class

    Public Structure W3CDTF
        <XmlAttribute("type", [Namespace]:=OpenXML.xsi)>
        Public Property type As String
        <XmlText> Public Property [Date] As Date
    End Structure
End Namespace
