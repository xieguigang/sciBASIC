#Region "Microsoft.VisualBasic::884aaf13de7be82a7dd904fe271f0c05, Data_science\Mathematica\Math\Math.Statistics\RANSAC\Vector.vb"

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

    '   Total Lines: 37
    '    Code Lines: 27 (72.97%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (27.03%)
    '     File Size: 1.05 KB


    '     Class Vector
    ' 
    '         Properties: magnitude, normalized
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Operators: *, +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace RANSAC

    Friend Class Vector : Inherits Point

        Public ReadOnly Property magnitude As Double
            Get
                Return std.Sqrt(std.Pow(x, 2) + std.Pow(y, 2) + std.Pow(z, 2))
            End Get
        End Property

        Public ReadOnly Property normalized As Vector
            Get
                Return New Vector(x / magnitude, y / magnitude, z / magnitude)
            End Get
        End Property

        Public Sub New(x As Double, y As Double, z As Double)
            MyBase.New(x, y, z)

        End Sub

        Public Sub New(point As Double())
            MyBase.New(point)

        End Sub

        Public Shared Operator +(v1 As Vector, v2 As Vector) As Vector
            Return New Vector(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z)
        End Operator

        Public Shared Operator *(v As Vector, num As Double) As Vector
            Return New Vector(v.x * num, v.y * num, v.z * num)
        End Operator
    End Class
End Namespace
