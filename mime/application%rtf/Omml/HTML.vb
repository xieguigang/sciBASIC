#Region "Microsoft.VisualBasic::dff8b8855b823b9787546b54b5850aca, ..\sciBASIC#\mime\application%rtf\Omml\HTML.vb"

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

Imports System.Text
Imports System.Xml.Serialization

Namespace Omml

    ''' <summary>
    ''' Omml: office microsoft word xml
    ''' </summary>
    ''' <remarks></remarks>
    <XmlRoot("html", Namespace:="http://www.w3.org/TR/REC-html40")>
    Public Class HTML

        Public Const WORD_XML_NAMESPACE As String = "xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:m=""http://schemas.microsoft.com/office/2004/12/omml"""

        Public Property Head As Head

        Public Function SaveDocument(path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
            Throw New NotImplementedException
        End Function

        Private Function InternalCreateDocument() As String
            Throw New NotImplementedException
        End Function
    End Class
End Namespace
