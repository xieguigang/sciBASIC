#Region "Microsoft.VisualBasic::43b72cb99a0f29a294a9ece36bd0b9e1, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\docProps\core.xml.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
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

Namespace XML.docProps

    <XmlRoot("coreProperties", [Namespace]:=cp)>
    Public Class core : Inherits IXml

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces

            xmlns.Add("cp", Excel.Xmlns.cp)
            xmlns.Add("dc", Excel.Xmlns.dc)
            xmlns.Add("dcterms", Excel.Xmlns.dcterms)
            xmlns.Add("dcmitype", Excel.Xmlns.dcmitype)
            xmlns.Add("xsi", Excel.Xmlns.xsi)
        End Sub

        <XmlElement(ElementName:=NameOf(creator), [Namespace]:=dc)>
        Public Property creator As String
        <XmlElement(ElementName:=NameOf(lastModifiedBy), [Namespace]:=cp)>
        Public Property lastModifiedBy As String
        <XmlElement(NameOf(created), [Namespace]:=dcterms)>
        Public Property created As W3CDTF
        <XmlElement(NameOf(modified), [Namespace]:=dcterms)>
        Public Property modified As W3CDTF

        Protected Overrides Function filePath() As String
            Return "docProps/core.xml"
        End Function

        Protected Overrides Function toXml() As String
            Return Me.GetXml
        End Function
    End Class

    Public Structure W3CDTF
        <XmlAttribute("type", [Namespace]:=xsi)>
        Public Property type As String
        <XmlText> Public Property [Date] As Date
    End Structure
End Namespace