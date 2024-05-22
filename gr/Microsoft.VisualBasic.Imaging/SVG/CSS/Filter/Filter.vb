#Region "Microsoft.VisualBasic::a8641172a6a4fa400915aab6b2d30f55, gr\Microsoft.VisualBasic.Imaging\SVG\CSS\Filter\Filter.vb"

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

    '   Total Lines: 34
    '    Code Lines: 22 (64.71%)
    ' Comment Lines: 6 (17.65%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (17.65%)
    '     File Size: 1.08 KB


    '     Class Filter
    ' 
    '         Properties: Composites, Floods, GaussianBlurs, height, Merges
    '                     Morphologys, Offsets, width, x, y
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Html.XmlMeta

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
End Namespace
