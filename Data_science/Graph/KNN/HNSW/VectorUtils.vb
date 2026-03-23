#Region "Microsoft.VisualBasic::e457239b9ad8847b9d37f7392244c99a, Data_science\Graph\KNN\HNSW\VectorUtils.vb"

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

    '   Total Lines: 104
    '    Code Lines: 61 (58.65%)
    ' Comment Lines: 25 (24.04%)
    '    - Xml Docs: 84.00%
    ' 
    '   Blank Lines: 18 (17.31%)
    '     File Size: 3.55 KB


    '     Module VectorUtils
    ' 
    '         Function: Magnitude, MagnitudeSIMD
    ' 
    '         Sub: Normalize, NormalizeSIMD
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
                i = i + 1
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
                i = i + 1
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
                i = i + 1
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
                i = i + 1
            End While
        End Sub
    End Module
End Namespace

