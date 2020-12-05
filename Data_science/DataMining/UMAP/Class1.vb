Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Public NotInheritable Class DistanceFunctions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Cosine(lhs As Single(), rhs As Single()) As Single
        Return 1 - SIMD.DotProduct(lhs, rhs) / (SIMD.Magnitude(lhs) * SIMD.Magnitude(rhs))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function CosineForNormalizedVectors(lhs As Single(), rhs As Single()) As Single
        Return 1 - SIMD.DotProduct(lhs, rhs)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Euclidean(lhs As Single(), rhs As Single()) As Single
        Return stdNum.Sqrt(SIMD.Euclidean(lhs, rhs)) ' TODO: Replace with netcore3 MathF class when the framework is available
    End Function
End Class