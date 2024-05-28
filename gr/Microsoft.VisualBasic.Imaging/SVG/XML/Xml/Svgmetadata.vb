#Region "Microsoft.VisualBasic::96af9540d71db1d2e482f5f00102be7a, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\Svgmetadata.vb"

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

    '   Total Lines: 20
    '    Code Lines: 13 (65.00%)
    ' Comment Lines: 3 (15.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (20.00%)
    '     File Size: 566 B


    '     Class SvgMetadata
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml

Namespace SVG.XML

    ''' <summary>
    ''' the svg RDF metadata node
    ''' </summary>
    Public Class SvgMetadata : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgMetadata
            Dim element = parent.OwnerDocument.CreateElement("metadata")
            parent.AppendChild(element)
            Return New SvgMetadata(element)
        End Function
    End Class
End Namespace
