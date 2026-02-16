' <copyright file="CosineDistance.cs" company="Microsoft">
' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT License.
' </copyright>

Imports System.Numerics

Namespace KNearNeighbors.HNSW

    ''' <summary>
    ''' Calculates cosine similarity.
    ''' </summary>
    ''' <remarks>
    ''' Intuition behind selecting float as a carrier.
    '''
    ''' 1. In practice we work with vectors of dimensionality 100 and each component has value in range [-1; 1]
    '''    There certainly is a possibility of underflow.
    '''    But we assume that such cases are rare and we can rely on such underflow losses.
    '''
    ''' 2. According to the article http://www.ti3.tuhh.de/paper/rump/JeaRu13.pdf
    '''    the floating point rounding error is less then 100 * 2^-24 * sqrt(100) * sqrt(100) &lt; 0.0005960
    '''    We deem such precision is satisfactory for out needs.
    ''' </remarks>
    Public Module CosineDistance
        ''' <summary>
        ''' Calculates cosine distance without making any optimizations.
        ''' </summary>
        ''' <param name="u">Left vector.</param>
        ''' <param name="v">Right vector.</param>
        ''' <returns>Cosine distance between u and v.</returns>
        Public Function NonOptimized(u As IReadOnlyList(Of Single), v As IReadOnlyList(Of Single)) As Single
            If u.Count <> v.Count Then
                Throw New ArgumentException("Vectors have non-matching dimensions")
            End If

            Dim dot = 0.0F
            Dim nru = 0.0F
            Dim nrv = 0.0F
            Dim i = 0

            While i < u.Count
                dot += u(i) * v(i)
                nru += u(i) * u(i)
                nrv += v(i) * v(i)
                Threading.Interlocked.Increment(i)
            End While

            Dim similarity = dot / CSng(Math.Sqrt(nru) * Math.Sqrt(nrv))
            Return 1 - similarity
        End Function

        ''' <summary>
        ''' Calculates cosine distance with assumption that u and v are unit vectors.
        ''' </summary>
        ''' <param name="u">Left vector.</param>
        ''' <param name="v">Right vector.</param>
        ''' <returns>Cosine distance between u and v.</returns>
        Public Function ForUnits(u As IReadOnlyList(Of Single), v As IReadOnlyList(Of Single)) As Single
            If u.Count <> v.Count Then
                Throw New ArgumentException("Vectors have non-matching dimensions")
            End If

            Dim dot As Single = 0
            Dim i = 0

            While i < u.Count
                dot += u(i) * v(i)
                Threading.Interlocked.Increment(i)
            End While

            Return 1 - dot
        End Function

        ''' <summary>
        ''' Calculates cosine distance optimized using SIMD instructions.
        ''' </summary>
        ''' <param name="u">Left vector.</param>
        ''' <param name="v">Right vector.</param>
        ''' <returns>Cosine distance between u and v.</returns>
        Public Function SIMD(u As Single(), v As Single()) As Single
            If Not Vector.IsHardwareAccelerated Then
                Throw New NotSupportedException($"SIMD version of {NameOf(CosineDistance)} is not supported")
            End If

            If u.Length <> v.Length Then
                Throw New ArgumentException("Vectors have non-matching dimensions")
            End If

            Dim dot As Single = 0
            Dim norm = DirectCast(Nothing, Vector2)
            Dim [step] = Vector(Of Single).Count

            Dim i As Integer, [to] = u.Length - [step]
            i = 0

            While i <= [to]
                Dim ui = New Vector(Of Single)(u, i)
                Dim vi = New Vector(Of Single)(v, i)
                dot += Vector.Dot(ui, vi)
                norm.X += Vector.Dot(ui, ui)
                norm.Y += Vector.Dot(vi, vi)
                i += [step]
            End While

            While i < u.Length
                dot += u(i) * v(i)
                norm.X += u(i) * u(i)
                norm.Y += v(i) * v(i)
                Threading.Interlocked.Increment(i)
            End While

            norm = Vector2.SquareRoot(norm)
            Dim similarity = dot / (norm.X * norm.Y)
            Return 1 - similarity
        End Function

        ''' <summary>
        ''' Calculates cosine distance with assumption that u and v are unit vectors using SIMD instructions.
        ''' </summary>
        ''' <param name="u">Left vector.</param>
        ''' <param name="v">Right vector.</param>
        ''' <returns>Cosine distance between u and v.</returns>
        Public Function SIMDForUnits(u As Single(), v As Single()) As Single
            If Not Vector.IsHardwareAccelerated Then
                Throw New NotSupportedException($"SIMD version of {NameOf(CosineDistance)} is not supported")
            End If

            If u.Length <> v.Length Then
                Throw New ArgumentException("Vectors have non-matching dimensions")
            End If

            Dim dot As Single = 0
            Dim [step] = Vector(Of Single).Count

            Dim i As Integer, [to] = u.Length - [step]
            i = 0

            While i <= [to]
                Dim ui = New Vector(Of Single)(u, i)
                Dim vi = New Vector(Of Single)(v, i)
                dot += Vector.Dot(ui, vi)
                i += [step]
            End While

            While i < u.Length
                dot += u(i) * v(i)
                Threading.Interlocked.Increment(i)
            End While

            Return 1 - dot
        End Function
    End Module
End Namespace
