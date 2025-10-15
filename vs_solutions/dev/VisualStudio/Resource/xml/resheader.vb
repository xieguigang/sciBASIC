#Region "Microsoft.VisualBasic::088bbc38890390d1bd5d3be03af4b320, vs_solutions\dev\VisualStudio\Resource\xml\resheader.vb"

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

    '   Total Lines: 29
    '    Code Lines: 17 (58.62%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (41.38%)
    '     File Size: 583 B


    '     Class resheader
    ' 
    '         Properties: name, value
    ' 
    '     Class assembly
    ' 
    '         Properties: [alias], name
    ' 
    '     Class data
    ' 
    '         Properties: name, type, value
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace Resource

    Public Class resheader

        <XmlAttribute>
        Public Property name As String
        Public Property value As String

    End Class

    Public Class assembly

        <XmlAttribute> Public Property [alias] As String
        <XmlAttribute> Public Property name As String

    End Class

    Public Class data

        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property type As String
        Public Property value As String

    End Class


End Namespace
