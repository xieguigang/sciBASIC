#Region "Microsoft.VisualBasic::da4fb06d7e476bb6cfb4dadb47df2fc6, gr\Microsoft.VisualBasic.Imaging\SVG\CSS\Gradients\Stop.vb"

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

    '   Total Lines: 18
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 524 B


    '     Class [stop]
    ' 
    '         Properties: offset, stopColor, stopOpacity
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Html.XmlMeta

Namespace SVG.CSS

    Public Class [stop] : Inherits Node

        <XmlAttribute("offset")> Public Property offset As String
        <XmlAttribute("stop-color")>
        Public Property stopColor As String
        <XmlAttribute("stop-opacity")>
        Public Property stopOpacity As String

        Public Overrides Function ToString() As String
            Return Me.GetXml
        End Function
    End Class
End Namespace
