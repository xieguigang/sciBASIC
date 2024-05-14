#Region "Microsoft.VisualBasic::7cd16e63540b8754b215831f05c7c152, Microsoft.VisualBasic.Core\src\Text\Xml\OpenXml\Content_Types.vb"

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

    '   Total Lines: 40
    '    Code Lines: 29
    ' Comment Lines: 4
    '   Blank Lines: 7
    '     File Size: 1.24 KB


    '     Class ContentTypes
    ' 
    '         Properties: [Default], [Overrides]
    ' 
    '         Function: ToString
    ' 
    '     Structure Type
    ' 
    '         Properties: ContentType, Extension, PartName
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.OpenXml

    ''' <summary>
    ''' ``[Content_Types].xml``
    ''' </summary>
    ''' 
    <XmlRoot("Types", Namespace:="http://schemas.openxmlformats.org/package/2006/content-types")>
    Public Class ContentTypes

        <XmlElement> Public Property [Default] As Type()
        <XmlElement("Override")>
        Public Property [Overrides] As List(Of Type)

        Public Overrides Function ToString() As String
            Return [Overrides] _
                .Select(Function(t) t.PartName) _
                .ToArray _
                .GetJson
        End Function
    End Class

    Public Structure Type

        <XmlAttribute> Public Property Extension As String
        <XmlAttribute> Public Property ContentType As String
        <XmlAttribute> Public Property PartName As String

        Public Overrides Function ToString() As String
            If PartName.StringEmpty Then
                Return ContentType
            Else
                Return $"({PartName}) {ContentType}"
            End If
        End Function
    End Structure
End Namespace
