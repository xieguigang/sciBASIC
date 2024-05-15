#Region "Microsoft.VisualBasic::91628eb84a3daf226e8550b56e4605a3, Data_science\Mathematica\Math\Math.Statistics\RANSAC\Point.vb"

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

    '   Total Lines: 24
    '    Code Lines: 17
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 445 B


    '     Class Point
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RANSAC

    Friend Class Point

        Public x, y, z As Double

        Public Sub New()

        End Sub

        Public Sub New(x As Double, y As Double, z As Double)
            Me.x = x
            Me.y = y
            Me.z = z
        End Sub

        Public Sub New(point As Double())
            x = point(0)
            y = point(1)
            z = point(2)
        End Sub
    End Class

End Namespace
