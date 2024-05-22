#Region "Microsoft.VisualBasic::480a87893651d06864c2d5391a78f2a1, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\docProps\app.xml.vb"

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

    '   Total Lines: 68
    '    Code Lines: 47 (69.12%)
    ' Comment Lines: 8 (11.76%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 13 (19.12%)
    '     File Size: 2.20 KB


    '     Class app
    ' 
    '         Properties: Application, AppVersion, Company, DocSecurity, HeadingPairs
    '                     HyperlinksChanged, LinksUpToDate, ScaleCrop, SharedDoc, TitlesOfParts
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: filePath, toXml
    ' 
    '     Class Vectors
    ' 
    '         Properties: vector
    ' 
    '     Class vector
    ' 
    '         Properties: baseType, lpstrs, size, variants
    ' 
    '     Class [variant]
    ' 
    '         Properties: i4, lpstr
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports OpenXML = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model.Xmlns

Namespace XLSX.XML.docProps

    <XmlRoot("Properties", [Namespace]:="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties")>
    Public Class app : Implements IXml

        Public Property Application As String
        Public Property DocSecurity As String
        Public Property ScaleCrop As Boolean
        Public Property HeadingPairs As Vectors
        Public Property TitlesOfParts As Vectors
        Public Property Company As String
        Public Property LinksUpToDate As Boolean
        Public Property SharedDoc As Boolean
        Public Property HyperlinksChanged As Boolean
        Public Property AppVersion As String

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces
            xmlns.Add("vt", OpenXML.vt)
        End Sub

        Protected Function filePath() As String Implements IXml.filePath
            Return "docProps/app.xml"
        End Function

        Protected Function toXml() As String Implements IXml.toXml
            Throw New NotImplementedException()
        End Function
    End Class

    <XmlRoot("vector", [Namespace]:=OpenXML.vt)>
    Public Class Vectors
        Public Property vector As vector
    End Class

    Public Class vector

        <XmlAttribute>
        Public Property size As Integer

        ''' <summary>
        ''' 这个属性指示了当前的这个向量数组对象哪个属性有值：
        ''' 
        ''' + ``lpstr``   -> <see cref="lpstrs"/>
        ''' + ``variant`` -> <see cref="variants"/>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property baseType As String
        <XmlElement("variant")>
        Public Property variants As [variant]()
        <XmlElement("lpstr")>
        Public Property lpstrs As String()

    End Class

    Public Class [variant]
        Public Property lpstr As String
        Public Property i4 As String
    End Class
End Namespace
