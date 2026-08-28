#Region "Microsoft.VisualBasic::f47405e935a4d28f0029783133e8715c, Data_science\DataMining\UMAP\Components\Utils.vb"

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

    '   Total Lines: 94
    '    Code Lines: 64 (68.09%)
    ' Comment Lines: 15 (15.96%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 15 (15.96%)
    '     File Size: 3.07 KB


    ' Module Utils
    ' 
    '     Function: Empty, Filled, Range, RejectionSample
    ' 
    '     Sub: ShuffleTogether
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math
Imports std = System.Math

Module Utils

    ''' <summary>
    ''' Creates an empty array
    ''' </summary>
    Public Function Empty(n As Integer) As Double()
        Return New Double(n - 1) {}
    End Function

    ''' <summary>
    ''' Creates an array filled with index values
    ''' </summary>
    Public Function Range(n As Integer) As Double()
        Return Enumerable.Range(0, n).[Select](Function(i) CDbl(i)).ToArray()
    End Function

    ''' <summary>
    ''' Creates an array filled with a specific value
    ''' </summary>
    Public Function Filled(count As Integer, value As Double) As Double()
        Return Enumerable.Range(0, count).[Select](Function(i) value).ToArray()
    End Function

    ''' <summary>
    ''' Generate nSamples many integers from 0 to poolSize such that no integer is selected twice.The duplication constraint is achieved via rejection sampling.
    ''' </summary>
    ''' <remarks>
    ''' the deduplication check of this function is implemented based on a 
    ''' <see cref="HashSet(Of T)"/> object instead of the original O(n^2) 
    ''' linear scan, so that the sampling procedure is O(n) now.
    ''' 
    ''' note about that the number of the random numbers that have been 
    ''' consumed by this function is not changed, so the generated result 
    ''' is still the same as the original implementation for a given 
    ''' random source.
    ''' </remarks>
    Public Function RejectionSample(nSamples As Integer, poolSize As Integer, random As IProvideRandomValues) As Integer()
        Dim result = New Integer(nSamples - 1) {}
        Dim maxItrs As Integer = 10000

        If poolSize <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(poolSize), "the pool size should be a positive integer!")
        End If
        If nSamples >= poolSize Then
            ' the rejection sampling is impossible to be terminated in such 
            ' condition, just do a partial Fisher-Yates shuffle of the whole 
            ' pool and fill the left slots with -1
            Call ShuffleSample(result, poolSize, random)
            Return result
        End If

        Dim seen As New HashSet(Of Integer)()

        For i As Integer = 0 To nSamples - 1
            Dim rejectSample = True
            Dim counter As Integer = 0
            Dim j As Integer = 0

            ' 20250610 possible dead loop at here
            ' if always broken
            ' use a counter for avoid such possible error
            While rejectSample AndAlso counter < maxItrs
                j = random.Next(0, poolSize)

                If Not seen.Contains(j) Then
                    rejectSample = False
                End If

                counter += 1
            End While

            result(i) = j
            seen.Add(j)

            If counter >= maxItrs Then
                Call $"dead loop was detected while make sample rejection for sample {i}!".Warning
            End If
        Next

        Return result
    End Function

    ''' <summary>
    ''' partial Fisher-Yates shuffle of the index pool [0, poolSize) for the 
    ''' degenerate case of <see cref="RejectionSample(Integer, Integer, IProvideRandomValues)"/>
    ''' </summary>
    Private Sub ShuffleSample(result As Integer(), poolSize As Integer, random As IProvideRandomValues)
        Dim pool As Integer() = New Integer(poolSize - 1) {}

        For i As Integer = 0 To poolSize - 1
            pool(i) = i
        Next

        Dim n As Integer = std.Min(result.Length, poolSize)

        For i As Integer = 0 To n - 1
            Dim j As Integer = random.Next(i, poolSize)
            Dim tmp As Integer = pool(i)

            pool(i) = pool(j)
            pool(j) = tmp

            result(i) = pool(i)
        Next

        For i As Integer = n To result.Length - 1
            result(i) = -1
        Next
    End Sub

    ''' <summary>
    ''' Create a striped lock object array.
    ''' </summary>
    ''' <param name="stripes">
    ''' the number of the lock objects, a larger value means a lower lock 
    ''' contention but a higher memory footprint.
    ''' </param>
    ''' <returns>
    ''' a lock array that is indexed via ``row Mod length``.
    ''' </returns>
    ''' <remarks>
    ''' the striped lock is used for protect the heap row while running the 
    ''' heap push operation in parallel. the lock objects are always 
    ''' acquired in the ascending order of the stripe index, so that the 
    ''' dead lock is impossible.
    ''' </remarks>
    Friend Function NewStripedLocks(Optional stripes As Integer = 4096) As Object()
        If stripes <= 0 Then
            stripes = 4096
        End If

        Dim locks As Object() = New Object(stripes - 1) {}

        For i As Integer = 0 To stripes - 1
            locks(i) = New Object()
        Next

        Return locks
    End Function

    <Extension>
    Friend Sub ShuffleTogether(Of T, T2, T3)(list As List(Of T), other As List(Of T2), weights As List(Of T3), randf As IProvideRandomValues)
        Dim n As Integer = list.Count
        Dim k As Integer
        Dim value As T
        Dim otherValue As T2
        Dim weightsValue As T3

        If other.Count <> n Then
            Throw New Exception()
        End If

        While n > 1
            n -= 1
            k = randf.Next(0, n + 1)
            value = list(k)
            list(k) = list(n)
            list(n) = value
            otherValue = other(k)
            other(k) = other(n)
            other(n) = otherValue
            weightsValue = weights(k)
            weights(k) = weights(n)
            weights(n) = weightsValue
        End While
    End Sub
End Module
