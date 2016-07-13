#Region "Microsoft.VisualBasic::180e17f207183196d23f7421a96887e8, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\XmlDoc\Serialization\Doc.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Namespace SoftwareToolkits.XmlDoc.Serialization

    ''' <summary>
    ''' .NET assembly generated XML comments documents file.
    ''' </summary>
    <XmlType("doc")> Public Class Doc
        Public Property assembly As assembly
        Public Property members As member()

        Public Overrides Function ToString() As String
            Return assembly.name
        End Function
    End Class

    Public Class assembly : Implements IMember

        Public Property name As String Implements IMember.name

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Class

    Public Interface IMember
        Property name As String
    End Interface
End Namespace
