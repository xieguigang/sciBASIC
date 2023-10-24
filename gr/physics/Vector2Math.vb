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
        Return Vector.DotProduct({lhs.x, lhs.y}, {rhs.x, rhs.y})
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function saturate(x As Single) As Single
        Return Max(0, Min(1, x))
    End Function
End Module
