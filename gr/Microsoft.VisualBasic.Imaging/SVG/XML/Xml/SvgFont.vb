#Region "Microsoft.VisualBasic::96dd4af58ba9369c933b78b76c9354b2, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\SvgFont.vb"

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

    '   Total Lines: 60
    '    Code Lines: 43 (71.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (28.33%)
    '     File Size: 1.81 KB


    '     Class SvgFont
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    '     Class SvgFontFace
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    '     Class SvgGlyph
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    '     Class SvgMissingGlyph
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml

Namespace SVG.XML

    Public Class SvgFont : Inherits SvgContainer

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgFont
            Dim element = parent.OwnerDocument.CreateElement("font")
            parent.AppendChild(element)
            Return New SvgFont(element)
        End Function

    End Class

    Public Class SvgFontFace : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgFontFace
            Dim element = parent.OwnerDocument.CreateElement("font-face")
            parent.AppendChild(element)
            Return New SvgFontFace(element)
        End Function

    End Class

    Public Class SvgGlyph : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgGlyph
            Dim element = parent.OwnerDocument.CreateElement("glyph")
            parent.AppendChild(element)
            Return New SvgGlyph(element)
        End Function

    End Class

    Public Class SvgMissingGlyph : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgMissingGlyph
            Dim element = parent.OwnerDocument.CreateElement("missing-glyph")
            parent.AppendChild(element)
            Return New SvgMissingGlyph(element)
        End Function

    End Class
End Namespace
