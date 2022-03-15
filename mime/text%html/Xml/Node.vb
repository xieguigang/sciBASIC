#Region "Microsoft.VisualBasic::6332ac4898baa85d28703b9746753e2f, sciBASIC#\mime\text%html\Xml\Node.vb"

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

    '   Total Lines: 31
    '    Code Lines: 12
    ' Comment Lines: 11
    '   Blank Lines: 8
    '     File Size: 930.00 B


    '     Class Node
    ' 
    '         Properties: [class], id, style
    ' 
    '     Class GenericNode
    ' 
    '         Properties: Tag
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace XmlMeta

    ''' <summary>
    ''' The base of the html node
    ''' </summary>
    Public MustInherit Class Node

        <XmlAttribute> Public Property id As String
        ''' <summary>
        ''' node class id, just like the id in HTML, you can also using this attribute to tweaks on the style by CSS.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property [class] As String

        ''' <summary>
        ''' CSS style definition <see cref="ICSSValue"/>.(请注意，假若是SVG对象则赋值这个属性无效)
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property style As String

    End Class

    Public Class GenericNode : Inherits Node

        <XmlAttribute> Public Property Tag As String

    End Class
End Namespace
