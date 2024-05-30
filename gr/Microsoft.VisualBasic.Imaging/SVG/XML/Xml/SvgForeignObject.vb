#Region "Microsoft.VisualBasic::178989b78a480ba9eef8e6f6bd1a9dad, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\SvgForeignObject.vb"

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

    '   Total Lines: 17
    '    Code Lines: 13 (76.47%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (23.53%)
    '     File Size: 512 B


    '     Class SvgForeignObject
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml

Namespace SVG.XML

    Public Class SvgForeignObject : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgForeignObject
            Dim element = parent.OwnerDocument.CreateElement("foreignObject")
            parent.AppendChild(element)
            Return New SvgForeignObject(element)
        End Function
    End Class
End Namespace
