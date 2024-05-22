#Region "Microsoft.VisualBasic::9c37c67453803ce5048b5b92670dd3b0, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\Enums\SvgFillRule.vb"

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

    '   Total Lines: 13
    '    Code Lines: 9 (69.23%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (30.77%)
    '     File Size: 358 B


    '     Class SvgFillRule
    ' 
    '         Properties: EvenOdd, NonZero
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVG.XML.Enums

    Public Class SvgFillRule : Inherits SvgEnum

        Public Shared ReadOnly Property NonZero As New SvgFillRule("nonzero")
        Public Shared ReadOnly Property EvenOdd As New SvgFillRule("evenodd")

        Private Sub New(value As String)
            MyBase.New(value)
        End Sub

    End Class
End Namespace
