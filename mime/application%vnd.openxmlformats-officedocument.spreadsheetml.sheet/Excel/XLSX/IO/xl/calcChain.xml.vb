#Region "Microsoft.VisualBasic::90fd7d4f30a0b769cb4f57af1e10e6ba, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\xl\calcChain.xml.vb"

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

    '   Total Lines: 16
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 473 B


    '     Class calcChain
    ' 
    '         Properties: c
    ' 
    '     Class c
    ' 
    '         Properties: i, l, r
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace XLSX.XML.xl

    <XmlType("calcChain", [Namespace]:="http://schemas.openxmlformats.org/spreadsheetml/2006/main")>
    Public Class calcChain
        <XmlElement("c")>
        Public Property c As c()
    End Class

    Public Class c
        <XmlAttribute> Public Property r As String
        <XmlAttribute> Public Property i As String
        <XmlAttribute> Public Property l As String
    End Class
End Namespace
