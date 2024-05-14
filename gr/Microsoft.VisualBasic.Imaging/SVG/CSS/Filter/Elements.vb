#Region "Microsoft.VisualBasic::7864519d18ed9757acb5094e7763ea18, gr\Microsoft.VisualBasic.Imaging\SVG\CSS\Filter\Elements.vb"

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

    '   Total Lines: 38
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.27 KB


    '     Class feMorphology
    ' 
    '         Properties: [in], [operator], radius, result
    ' 
    '     Class feGaussianBlur
    ' 
    '         Properties: [in], result, stdDeviation
    ' 
    '     Class feFlood
    ' 
    '         Properties: floodColor, result
    ' 
    '     Class feComposite
    ' 
    '         Properties: [in], [operator], in2, result
    ' 
    '     Class feOffset
    ' 
    '         Properties: dx, dy
    ' 
    '     Class feMergeNode
    ' 
    '         Properties: [in]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace SVG.CSS

    Public Class feMorphology
        <XmlAttribute> Public Property [operator] As String
        <XmlAttribute> Public Property radius As String
        <XmlAttribute> Public Property [in] As String
        <XmlAttribute> Public Property result As String
    End Class

    Public Class feGaussianBlur
        <XmlAttribute> Public Property [in] As String
        <XmlAttribute> Public Property stdDeviation As String
        <XmlAttribute> Public Property result As String
    End Class

    Public Class feFlood
        <XmlAttribute("flood-color")> Public Property floodColor As String
        <XmlAttribute> Public Property result As String
    End Class

    Public Class feComposite
        <XmlAttribute> Public Property [in] As String
        <XmlAttribute> Public Property in2 As String
        <XmlAttribute> Public Property [operator] As String
        <XmlAttribute> Public Property result As String
    End Class

    Public Class feOffset
        <XmlAttribute> Public Property dx As String
        <XmlAttribute> Public Property dy As String
    End Class

    Public Class feMergeNode
        <XmlAttribute> Public Property [in] As String
    End Class
End Namespace
