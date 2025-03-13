#Region "Microsoft.VisualBasic::b79a9ef227b95215ab4632009d5a4d14, gr\physics\Vector2Math.vb"

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

    '   Total Lines: 25
    '    Code Lines: 18 (72.00%)
    ' Comment Lines: 3 (12.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (16.00%)
    '     File Size: 779 B


    ' Module Vector2Math
    ' 
    '     Function: Abs, Dot, saturate
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

''' <summary>
''' unity math
''' </summary>
Public Module Vector2Math

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Abs(v As Vector2) As Vector2
        Return New Vector2(std.Abs(v.x), std.Abs(v.y))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Dot(lhs As Vector2, rhs As Vector2) As Double
        Return Vector.dot({lhs.x, lhs.y}, {rhs.x, rhs.y})
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function saturate(x As Single) As Single
        Return Max(0, Min(1, x))
    End Function
End Module
