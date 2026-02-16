' <copyright file="VectorUtils.cs" company="Microsoft">
' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT License.
' </copyright>

Imports System.Numerics
Imports std = System.Math

Namespace KNearNeighbors.HNSW

    ''' <summary>
    ''' Utilities to work with vectors.
    ''' </summary>
    Public Module VectorUtils
        ''' <summary>
        ''' Calculates magnitude of the vector.
        ''' </summary>
        ''' <param name="vector">The vector to calculate magnitude for.</param>
        ''' <returns>The magnitude.</returns>
        Public Function Magnitude(vector As IList(Of Single)) As Single
            Dim lMagnitude = 0.0F
            Dim i = 0

            While i < vector.Count
                lMagnitude += vector(i) * vector(i)
                Threading.Interlocked.Increment(i)
            End While

            Return std.Sqrt(lMagnitude)
        End Function

        ''' <summary>
        ''' Turns vector to unit vector.
        ''' </summary>
        ''' <param name="vector">The vector to normalize.</param>
        Public Sub Normalize(vector As IList(Of Single))
            Dim normFactor = 1 / Magnitude(vector)
            Dim i = 0

            While i < vector.Count
                vector(i) *= normFactor
                Threading.Interlocked.Increment(i)
            End While
        End Sub

        ''' <summary>
        ''' SIMD optimized version of <see cref="Magnitude(IList(Of Single))"/>.
        ''' </summary>
        ''' <param name="vector">The vector to calculate magnitude for.</param>
        ''' <returns>The magnitude.</returns>
        Public Function MagnitudeSIMD(vector As Single()) As Single
            If Not Numerics.Vector.IsHardwareAccelerated Then
                Throw New NotSupportedException($"{NameOf(VectorUtils.NormalizeSIMD)} is not supported")
            End If

            Dim magnitude = 0.0F
            Dim [step] = Numerics.Vector(Of Single).Count

            Dim i As Integer, [to] = vector.Length - [step]
            i = 0

            While i <= [to]
                Dim vi = New Vector(Of Single)(vector, i)
                magnitude += Numerics.Vector.Dot(vi, vi)
                i += Numerics.Vector(Of Single).Count
            End While

            While i < vector.Length
                magnitude += vector(i) * vector(i)
                Threading.Interlocked.Increment(i)
            End While

            Return std.Sqrt(magnitude)
        End Function

        ''' <summary>
        ''' SIMD optimized version of <see cref="Normalize(IList(Of Single))"/>.
        ''' </summary>
        ''' <param name="vector">The vector to normalize.</param>
        Public Sub NormalizeSIMD(vector As Single())
            If Not Numerics.Vector.IsHardwareAccelerated Then
                Throw New NotSupportedException($"{NameOf(VectorUtils.NormalizeSIMD)} is not supported")
            End If

            Dim normFactor = 1 / MagnitudeSIMD(vector)
            Dim [step] = Numerics.Vector(Of Single).Count

            Dim i As Integer, [to] = vector.Length - [step]
            i = 0

            While i <= [to]
                Dim vi = New Vector(Of Single)(vector, i)
                vi = Numerics.Vector.Multiply(normFactor, vi)
                vi.CopyTo(vector, i)
                i += [step]
            End While

            While i < vector.Length
                vector(i) *= normFactor
                Threading.Interlocked.Increment(i)
            End While
        End Sub
    End Module
End Namespace
