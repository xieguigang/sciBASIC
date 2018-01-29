#Region "Microsoft.VisualBasic::f4051337be761b433de4b7654cfc5803, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IO\docProps\core.xml.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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
