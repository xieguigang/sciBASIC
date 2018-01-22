Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Extensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function X(fit As FittedResult) As Vector
        Return fit.ErrorTest.Select(Function(point) point.X).AsVector
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Y(fit As FittedResult) As Vector
        Return fit.ErrorTest.Select(Function(point) point.Y).AsVector
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Yfit(fit As FittedResult) As Vector
        Return fit.ErrorTest.Select(Function(point) point.Yfit).AsVector
    End Function
End Module
