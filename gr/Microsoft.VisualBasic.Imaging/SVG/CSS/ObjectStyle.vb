#Region "Microsoft.VisualBasic::97e9e4b3d2e231fd1587178c531ef4b1, gr\Microsoft.VisualBasic.Imaging\SVG\CSS\ObjectStyle.vb"

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

    '     Class ObjectStyle
    ' 
    '         Properties: CSSValue, fill, stroke
    ' 
    '         Function: ToString
    ' 
    '     Class CSSStyles
    ' 
    '         Properties: filters, linearGradients, radialGradients, styles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.MIME.Markup.HTML.XmlMeta

Namespace SVG.CSS

    Public Class ObjectStyle : Inherits ICSSValue

        Public Property stroke As Stroke
        Public Property fill As String

        Public Overrides ReadOnly Property CSSValue As String
            Get
                Return ToString()
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return stroke.ToString & " fill: " & fill
        End Function
    End Class

    ''' <summary>
    ''' 在这个SVG对象之中所定义的CSS样式数据
    ''' </summary>
    Public Class CSSStyles : Inherits Node

        <XmlElement("linearGradient")>
        Public Property linearGradients As linearGradient()
        <XmlElement("radialGradient")>
        Public Property radialGradients As radialGradient()
        <XmlElement("style")>
        Public Property styles As XmlMeta.CSS()
        <XmlElement("filter")>
        Public Property filters As Filter()

    End Class
End Namespace
