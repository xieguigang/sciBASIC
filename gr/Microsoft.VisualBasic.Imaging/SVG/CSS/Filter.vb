#Region "Microsoft.VisualBasic::3c72d06ff32b6e2117dbc3218e2c9c5d, gr\Microsoft.VisualBasic.Imaging\SVG\CSS\Filter.vb"

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

    '     Class Filter
    ' 
    '         Properties: GaussianBlurs, Merges, Offsets
    ' 
    '     Class feGaussianBlur
    ' 
    '         Properties: [in], stdDeviation
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
Imports Microsoft.VisualBasic.MIME.Markup.HTML.XmlMeta

Namespace SVG.CSS

    ''' <summary>
    ''' 图层滤镜
    ''' </summary>
    ''' <remarks>
    ''' 请注意：filter里面的元素是有执行顺序的
    ''' </remarks>
    Public Class Filter : Inherits Node

        <XmlElement("feMorphology")>
        Public Property Morphologys As feMorphology()

        <XmlElement("feGaussianBlur")>
        Public Property GaussianBlurs As feGaussianBlur()
        <XmlElement("feFlood")>
        Public Property Floods As feFlood()
        <XmlElement("feComposite")>
        Public Property Composites As feComposite()
        <XmlElement("feOffset")>
        Public Property Offsets As feOffset()
        <XmlArray("feMerge")>
        Public Property Merges As feMergeNode()

        <XmlAttribute> Public Property x As String
        <XmlAttribute> Public Property y As String
        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property height As String

    End Class

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
