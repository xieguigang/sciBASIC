#Region "Microsoft.VisualBasic::af24ed0186baada78326a28d0ce5e51c, gr\Microsoft.VisualBasic.Imaging\SVG\XML\BasicShapes\title.vb"

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

    '   Total Lines: 26
    '    Code Lines: 21
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 722 B


    '     Class SvgTitle
    ' 
    '         Properties: Text
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml

Namespace SVG.XML

    Public Class SvgTitle : Inherits SvgElement

        Public Property Text As String
            Get
                Return Element.InnerText
            End Get
            Set(value As String)
                Element.InnerText = value
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgTitle
            Dim element = parent.OwnerDocument.CreateElement("title")
            parent.AppendChild(element)
            Return New SvgTitle(element)
        End Function
    End Class
End Namespace
