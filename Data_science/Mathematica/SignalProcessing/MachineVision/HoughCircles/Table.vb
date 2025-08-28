#Region "Microsoft.VisualBasic::10324b3574e8c2a8aaef67ff08dbf3b8, Data_science\Mathematica\SignalProcessing\MachineVision\HoughCircles\Table.vb"

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

    '   Total Lines: 29
    '    Code Lines: 23 (79.31%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (20.69%)
    '     File Size: 802 B


    '     Class TableRow
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Table
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace HoughCircles

    Friend Class TableRow

        Public Length As Double
        Public Angle As Double

        Public Sub New(angle As Double, length As Double)
            Me.Length = length
            Me.Angle = angle
        End Sub
    End Class

    Friend Class Table

        Public Shared ReadOnly Square As TableRow() = {
            New TableRow(0, 1),
            New TableRow(std.PI / 4, std.Sqrt(2.0R)),
            New TableRow(std.PI / 2, 1),
            New TableRow(3 * std.PI / 4, std.Sqrt(2.0R)),
            New TableRow(std.PI, 1),
            New TableRow(-std.PI / 4, 1),
            New TableRow(-std.PI / 2, 1),
            New TableRow(-3 * std.PI / 4, std.Sqrt(2.0R))
        }
    End Class
End Namespace

