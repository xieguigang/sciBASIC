#Region "Microsoft.VisualBasic::a72d7d5f8cd96702ad8ad1c439afad2b, gr\network-visualization\Network.IO.Extensions\Gephi\edge.vb"

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
    '     File Size: 282 B


    '     Class edge
    ' 
    '         Properties: fill, id, source, target, value
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Gephi

    Public Class edge

        Public Property id As Integer
        Public Property source As Integer
        Public Property target As Integer
        Public Property value As Double
        Public Property fill As String

    End Class

End Namespace
