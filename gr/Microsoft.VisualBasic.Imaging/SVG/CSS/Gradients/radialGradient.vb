#Region "Microsoft.VisualBasic::e86a136d912896380e4cec90a65fe6d0, gr\Microsoft.VisualBasic.Imaging\SVG\CSS\Gradients\radialGradient.vb"

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
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 533 B


    '     Class radialGradient
    ' 
    '         Properties: cx, cy, fx, fy, gradientUnits
    '                     r, spreadMethod
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace SVG.CSS

    Public Class radialGradient : Inherits Gradient

        <XmlAttribute> Public Property cx As String
        <XmlAttribute> Public Property cy As String
        <XmlAttribute> Public Property r As String
        <XmlAttribute> Public Property fx As String
        <XmlAttribute> Public Property fy As String
        <XmlAttribute> Public Property spreadMethod As String
        <XmlAttribute> Public Property gradientUnits As String

    End Class
End Namespace
