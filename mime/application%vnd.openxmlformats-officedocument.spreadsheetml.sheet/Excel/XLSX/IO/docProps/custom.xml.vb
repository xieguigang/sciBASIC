#Region "Microsoft.VisualBasic::6e9299f914bd1d8386c592837ced86fe, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\docProps\custom.xml.vb"

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

    '   Total Lines: 42
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 11
    '     File Size: 1.34 KB


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
Imports OpenXML = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model.Xmlns

Namespace XLSX.XML.docProps

    <XmlRoot("Properties", [Namespace]:="http://schemas.openxmlformats.org/officeDocument/2006/custom-properties")>
    Public Class custom : Implements IXml

        <XmlElement("property")>
        Public Property properties As [property]()

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces
            xmlns.Add("vt", OpenXML.vt)
        End Sub

        Protected Function filePath() As String Implements IXml.filePath
            Return "docProps/custom.xml"
        End Function

        Protected Function toXml() As String Implements IXml.toXml
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
