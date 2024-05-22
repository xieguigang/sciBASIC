#Region "Microsoft.VisualBasic::3a81bdb40f9b2618198a20570793689e, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\Enums\SvgTextAnchor.vb"

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
    '    Code Lines: 11 (68.75%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (31.25%)
    '     File Size: 490 B


    '     Class SvgTextAnchor
    ' 
    '         Properties: [End], Middle, Start
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVG.XML.Enums

    Public Class SvgTextAnchor
        Inherits SvgEnum

        Private Sub New(value As String)
            MyBase.New(value)
        End Sub

        Public Shared ReadOnly Property Start As SvgTextAnchor = New SvgTextAnchor("start")

        Public Shared ReadOnly Property Middle As SvgTextAnchor = New SvgTextAnchor("middle")

        Public Shared ReadOnly Property [End] As SvgTextAnchor = New SvgTextAnchor("end")
    End Class
End Namespace
