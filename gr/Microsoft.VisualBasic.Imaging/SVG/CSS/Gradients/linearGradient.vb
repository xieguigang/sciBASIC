#Region "Microsoft.VisualBasic::de57f0dd8f985f475d53ed6e6ba963b1, gr\Microsoft.VisualBasic.Imaging\SVG\CSS\Gradients\linearGradient.vb"

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

    '     Class linearGradient
    ' 
    '         Properties: x1, x2, y1, y2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace SVG.CSS

    Public Class linearGradient : Inherits Gradient

        <XmlAttribute> Public Property x1 As String
        <XmlAttribute> Public Property x2 As String
        <XmlAttribute> Public Property y1 As String
        <XmlAttribute> Public Property y2 As String

    End Class
End Namespace
