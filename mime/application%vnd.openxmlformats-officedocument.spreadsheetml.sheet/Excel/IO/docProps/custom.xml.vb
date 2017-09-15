#Region "Microsoft.VisualBasic::cabfa8fc951e44ec523e7a8ea5e8f9bb, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\docProps\custom.xml.vb"

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

Namespace docProps

    <XmlRoot("Properties", [Namespace]:="http://schemas.openxmlformats.org/officeDocument/2006/custom-properties")>
    Public Class custom : Inherits IXml

        <XmlElement("property")>
        Public Property properties As [property]()

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

        Public Property lpwstr As String

        Public Overrides Function ToString() As String
            Return lpwstr
        End Function
    End Class
End Namespace