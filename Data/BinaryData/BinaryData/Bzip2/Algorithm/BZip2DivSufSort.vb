#Region "Microsoft.VisualBasic::0a9cf150a0c573f5fff03811159eb8f9, Data\BinaryData\BinaryData\Bzip2\Algorithm\BZip2DivSufSort.vb"

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

    '   Total Lines: 2558
    '    Code Lines: 2019 (78.93%)
    ' Comment Lines: 27 (1.06%)
    '    - Xml Docs: 29.63%
    ' 
    '   Blank Lines: 512 (20.02%)
    '     File Size: 94.86 KB


    '     Class BZip2DivSufSort
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: BUCKET_B, BUCKET_BSTAR, BWT, constructBWT, getIDX
    '                   sortTypeBstar, ssCompare, ssCompareLast, ssLog, ssMedian3
    '                   ssMedian5, ssPivot, ssSubstringPartition, trGetC, trLog
    '                   trMedian3, trMedian5, trPartition, trPivot
    ' 
    '         Sub: lsIntroSort, lsSort, lsUpdateGroup, ssBlockSwap, ssFixdown
    '              ssHeapSort, ssInsertionSort, ssMerge, ssMergeBackward, ssMergeCheckEqual
    '              ssMergeForward, ssMultiKeyIntroSort, subStringSort, swapElements, trCopy
    '              trFixdown, trHeapSort, trInsertionSort, trIntroSort, trSort
    '         Class StackEntry
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Class PartitionResult
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Class TRBudget
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: update
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Bzip2 library for .net
' By Jaime Olivares
' Location: http://github.com/jaime-olivares/bzip2
' Ported from the Java implementation by Matthew Francis: https://github.com/MateuszBartosiewicz/bzip2

Imports Microsoft.VisualBasic.Language
Imports std = System.Math

Namespace Bzip2
    ''' <summary>
    ''' DivSufSort suffix array generator
    ''' Based on libdivsufsort 1.2.3 patched to support BZip2
    ''' </summary>
    ''' <remarks>
    ''' This is a simple conversion of the original C with two minor bugfixes applied(see "BUGFIX"
    ''' comments within the class). Documentation within the class is largely absent.
    ''' </remarks>
    Friend Class BZip2DivSufSort
#Region "Nested classes"
        Private Class StackEntry
            Public ReadOnly a As Integer
            Public ReadOnly b As Integer
            Public ReadOnly c As Integer
            Public ReadOnly d As Integer

            Public Sub New(a As Integer, b As Integer, c As Integer, d As Integer)
                Me.a = a
                Me.b = b
                Me.c = c
                Me.d = d
            End Sub
        End Class

        Private Class PartitionResult
            Public ReadOnly first As Integer
            Public ReadOnly last As Integer

            Public Sub New(first As Integer, last As Integer)
                Me.first = first
                Me.last = last
            End Sub
        End Class

        Private Class TRBudget
            Private budget As Integer
            Public chance As Integer

            Public Function update(size As Integer, n As Integer) As Boolean
                budget -= n

                If budget <= 0 Then
                    If Threading.Interlocked.Decrement(chance) = 0 Then
                        Return False
                    End If

                    budget += size
                End If

                Return True
            End Function

            Public Sub New(budget As Integer, chance As Integer)
                Me.budget = budget
                Me.chance = chance
            End Sub
        End Class
#End Region

#Region "Private fields"
        Private Const STACK_SIZE As Integer = 64
        Private Const BUCKET_A_SIZE As Integer = 256
        Private Const BUCKET_B_SIZE As Integer = 65536
        Private Const SS_BLOCKSIZE As Integer = 1024
        Private Const INSERTIONSORT_THRESHOLD As Integer = 8
        Private Shared ReadOnly log2table As Integer() = {-1, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7}
        Private ReadOnly SA As Integer()
        Private ReadOnly T As Byte()
        Private ReadOnly n As Integer
#End Region

#Region "Public methods"
        ' *
        '  @param T The input array
        '  @param SA The output array
        '  @param n The length of the input data
        ' 

        Public Sub New(T As Byte(), SA As Integer(), n As Integer)
            Me.T = T
            Me.SA = SA
            Me.n = n
        End Sub

        ' *
        '  Performs a Burrows Wheeler Transform on the input array
        '  @return the index of the first character of the input array within the output array
        ' 

        Public Function BWT() As Integer
            If n = 0 Then Return 0

            If n = 1 Then
                SA(0) = T(0)
                Return 0
            End If

            Dim bucketA = New Integer(255) {}
            Dim bucketB = New Integer(65535) {}
            Dim m = sortTypeBstar(bucketA, bucketB)
            Return If(0 < m, constructBWT(bucketA, bucketB), 0)
        End Function
#End Region

#Region "Private methods"
        ' ReSharper disable LoopVariableIsNeverChangedInsideLoop

        Private Shared Sub swapElements(array1 As Integer(), index1 As Integer, array2 As Integer(), index2 As Integer)
            Dim temp = array1(index1)
            array1(index1) = array2(index2)
            array2(index2) = temp
        End Sub

        Private Function ssCompare(p1 As Integer, p2 As Integer, depth As Integer) As Integer
            Dim U1n, U2n As Integer ' pointers within T
            Dim U1, U2 As Integer
            U1 = depth + SA(p1)
            U2 = depth + SA(p2)
            U1n = SA(p1 + 1) + 2
            U2n = SA(p2 + 1) + 2

            While U1 < U1n AndAlso U2 < U2n AndAlso T(U1) = T(U2)
                Threading.Interlocked.Increment(U1)
                Threading.Interlocked.Increment(U2)
            End While

            Return If(U1 < U1n, If(U2 < U2n, (T(U1) And &HFF) - (T(U2) And &HFF), 1), If(U2 < U2n, -1, 0))
        End Function

        Private Function ssCompareLast(PA As Integer, p1 As Integer, p2 As Integer, depth As Integer, size As Integer) As Integer
            Dim U1, U2, U1n, U2n As Integer
            U1 = depth + SA(p1)
            U2 = depth + SA(p2)
            U1n = size
            U2n = SA(p2 + 1) + 2

            While U1 < U1n AndAlso U2 < U2n AndAlso T(U1) = T(U2)
                Threading.Interlocked.Increment(U1)
                Threading.Interlocked.Increment(U2)
            End While

            If U1 < U1n Then Return If(U2 < U2n, (T(U1) And &HFF) - (T(U2) And &HFF), 1)
            If U2 = U2n Then Return 1
            U1 = U1 Mod size
            U1n = SA(PA) + 2

            While U1 < U1n AndAlso U2 < U2n AndAlso T(U1) = T(U2)
                Threading.Interlocked.Increment(U1)
                Threading.Interlocked.Increment(U2)
            End While

            Return If(U1 < U1n, If(U2 < U2n, (T(U1) And &HFF) - (T(U2) And &HFF), 1), If(U2 < U2n, -1, 0))
        End Function

        Private Sub ssInsertionSort(PA As Integer, first As Integer, last As Integer, depth As Integer)
            Dim i As Integer ' pointer within SA
            Dim r As Value(Of Integer) = 0

            i = last - 2

            While first <= i
                Dim j As Integer ' pointer within SA
                Dim t As Integer

                t = SA(i)
                j = i + 1

                While 0 < (r = ssCompare(PA + t, PA + SA(j), depth))

                    Do
                        SA(j - 1) = SA(j)
                    Loop While Threading.Interlocked.Increment(j) < last AndAlso SA(j) < 0

                    If last <= j Then
                        Exit While
                    End If
                End While

                If CInt(r) = 0 Then
                    SA(j) = Not SA(j)
                End If

                SA(j - 1) = t
                Threading.Interlocked.Decrement(i)
            End While
        End Sub

        Private Sub ssFixdown(Td As Integer, PA As Integer, sa As Integer, i As Integer, size As Integer)
            Dim j As Value(Of Integer) = 0
            Dim k As Integer
            Dim v As Integer
            Dim c As Integer
            v = Me.SA(sa + i)
            c = T(Td + Me.SA(PA + v)) And &HFF

            While (j = 2 * i + 1) < size
                k = std.Min(Threading.Interlocked.Increment(j.Value), CInt(j) - 1)

                Dim d = T(Td + Me.SA(PA + Me.SA(sa + k))) And &HFF
                Dim e As Integer = T(Td + Me.SA(PA + Me.SA(sa + CInt(j)))) And &HFF

                If d < e Then
                    k = j
                    d = e
                End If

                If d <= c Then Exit While
                Me.SA(sa + i) = Me.SA(sa + k)
                i = k
            End While

            Me.SA(sa + i) = v
        End Sub

        Private Sub ssHeapSort(Td As Integer, PA As Integer, sa As Integer, size As Integer)
            Dim i As Integer
            Dim m = size

            If size Mod 2 = 0 Then
                m -= 1

                If (T(Td + Me.SA(PA + Me.SA(sa + m / 2))) And &HFF) < (T(Td + Me.SA(PA + Me.SA(sa + m))) And &HFF) Then
                    swapElements(Me.SA, sa + m, Me.SA, sa + m / 2)
                End If
            End If

            i = m / 2 - 1

            While 0 <= i
                ssFixdown(Td, PA, sa, i, m)
                Threading.Interlocked.Decrement(i)
            End While

            If size Mod 2 = 0 Then
                swapElements(Me.SA, sa, Me.SA, sa + m)
                ssFixdown(Td, PA, sa, 0, m)
            End If

            i = m - 1

            While 0 < i
                Dim t = Me.SA(sa)
                Me.SA(sa) = Me.SA(sa + i)
                ssFixdown(Td, PA, sa, 0, i)
                Me.SA(sa + i) = t
                Threading.Interlocked.Decrement(i)
            End While
        End Sub

        Private Function ssMedian3(Td As Integer, PA As Integer, v1 As Integer, v2 As Integer, v3 As Integer) As Integer
            Dim T_v1 = T(Td + SA(PA + SA(v1))) And &HFF
            Dim T_v2 = T(Td + SA(PA + SA(v2))) And &HFF
            Dim T_v3 = T(Td + SA(PA + SA(v3))) And &HFF

            If T_v1 > T_v2 Then
                Dim temp = v1
                v1 = v2
                v2 = temp
                Dim T_vtemp = T_v1
                T_v1 = T_v2
                T_v2 = T_vtemp
            End If

            If T_v2 > T_v3 Then
                Return If(T_v1 > T_v3, v1, v3)
            End If

            Return v2
        End Function

        Private Function ssMedian5(Td As Integer, PA As Integer, v1 As Integer, v2 As Integer, v3 As Integer, v4 As Integer, v5 As Integer) As Integer
            Dim T_v1 = T(Td + SA(PA + SA(v1))) And &HFF
            Dim T_v2 = T(Td + SA(PA + SA(v2))) And &HFF
            Dim T_v3 = T(Td + SA(PA + SA(v3))) And &HFF
            Dim T_v4 = T(Td + SA(PA + SA(v4))) And &HFF
            Dim T_v5 = T(Td + SA(PA + SA(v5))) And &HFF
            Dim temp As Integer
            Dim T_vtemp As Integer

            ' ReSharper disable RedundantAssignment
            If T_v2 > T_v3 Then
                temp = v2
                v2 = v3
                v3 = temp
                T_vtemp = T_v2
                T_v2 = T_v3
                T_v3 = T_vtemp
            End If

            If T_v4 > T_v5 Then
                temp = v4
                v4 = v5
                v5 = temp
                T_vtemp = T_v4
                T_v4 = T_v5
                T_v5 = T_vtemp
            End If

            If T_v2 > T_v4 Then
                temp = v2
                v2 = v4
                v4 = temp
                T_vtemp = T_v2
                T_v2 = T_v4
                T_v4 = T_vtemp
                temp = v3
                v3 = v5
                v5 = temp
                T_vtemp = T_v3
                T_v3 = T_v5
                T_v5 = T_vtemp
            End If

            If T_v1 > T_v3 Then
                temp = v1
                v1 = v3
                v3 = temp
                T_vtemp = T_v1
                T_v1 = T_v3
                T_v3 = T_vtemp
            End If

            If T_v1 > T_v4 Then
                temp = v1
                v1 = v4
                v4 = temp
                T_vtemp = T_v1
                T_v1 = T_v4
                T_v4 = T_vtemp
                temp = v3
                v3 = v5
                v5 = temp
                T_vtemp = T_v3
                T_v3 = T_v5
                T_v5 = T_vtemp
            End If
            ' ReSharper restore RedundantAssignment

            Return If(T_v3 > T_v4, v4, v3)
        End Function

        Private Function ssPivot(Td As Integer, PA As Integer, first As Integer, last As Integer) As Integer
            Dim t = last - first
            Dim middle As Integer = first + t / 2

            If t <= 512 Then
                If t <= 32 Then
                    Return ssMedian3(Td, PA, first, middle, last - 1)
                End If

                t >>= 2
                Return ssMedian5(Td, PA, first, first + t, middle, last - 1 - t, last - 1)
            End If

            t >>= 3
            Return ssMedian3(Td, PA, ssMedian3(Td, PA, first, first + t, first + (t << 1)), ssMedian3(Td, PA, middle - t, middle, middle + t), ssMedian3(Td, PA, last - 1 - (t << 1), last - 1 - t, last - 1))
        End Function

        Private Shared Function ssLog(x As Integer) As Integer
            Return If((x And &HFF00) <> 0, 8 + log2table(x >> 8 And &HFF), log2table(x And &HFF))
        End Function

        Private Function ssSubstringPartition(PA As Integer, first As Integer, last As Integer, depth As Integer) As Integer
            Dim a, b As Integer
            a = first - 1
            b = last

            While True

                While Threading.Interlocked.Increment(a) < b AndAlso SA(PA + SA(a)) + depth >= SA(PA + SA(a) + 1) + 1
                    SA(a) = Not SA(a)
                End While

                While a < Threading.Interlocked.Decrement(b) AndAlso SA(PA + SA(b)) + depth < SA(PA + SA(b) + 1) + 1
                End While

                If b <= a Then
                    Exit While
                End If

                Dim t = Not SA(b)
                SA(b) = SA(a)
                SA(a) = t
            End While

            If first < a Then SA(first) = Not SA(first)
            Return a
        End Function

        Private Sub ssMultiKeyIntroSort(PA As Integer, first As Integer, last As Integer, depth As Integer)
            Dim stack = New StackEntry(63) {}
            Dim ssize As Integer
            Dim limit As Integer
            Dim x = 0
            ssize = 0
            limit = ssLog(last - first)

            While True

                If last - first <= INSERTIONSORT_THRESHOLD Then
                    If 1 < last - first Then
                        ssInsertionSort(PA, first, last, depth)
                    End If

                    If ssize = 0 Then Return
                    Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                    first = entry.a
                    last = entry.b
                    depth = entry.c
                    limit = entry.d
                    Continue While
                End If

                Dim Td = depth

                If std.Max(Threading.Interlocked.Decrement(limit), limit + 1) = 0 Then
                    ssHeapSort(Td, PA, first, last - first)
                End If

                Dim a As Integer
                Dim v As Integer

                If limit < 0 Then
                    a = first + 1
                    v = T(Td + SA(PA + SA(first))) And &HFF

                    While a < last

                        x = T(Td + SA(PA + SA(a))) And &HFF

                        If x <> v Then
                            If 1 < a - first Then
                                Exit While
                            End If

                            v = x
                            first = a
                        End If

                        Threading.Interlocked.Increment(a)
                    End While

                    If (T(Td + SA(PA + SA(first)) - 1) And &HFF) < v Then
                        first = ssSubstringPartition(PA, first, a, depth)
                    End If

                    If a - first <= last - a Then
                        If 1 < a - first Then
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(a, last, depth, -1)
                            last = a
                            depth += 1
                            limit = ssLog(a - first)
                        Else
                            first = a
                            limit = -1
                        End If
                    Else

                        If 1 < last - a Then
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(first, a, depth + 1, ssLog(a - first))
                            first = a
                            limit = -1
                        Else
                            last = a
                            depth += 1
                            limit = ssLog(a - first)
                        End If
                    End If

                    Continue While
                End If

                a = ssPivot(Td, PA, first, last)
                v = T(Td + SA(PA + SA(a))) And &HFF
                swapElements(SA, first, SA, a)
                Dim b As Integer
                Dim xi As Value(Of Integer) = 0

                b = first

                While Threading.Interlocked.Increment(b) < last AndAlso (xi = T(Td + SA(PA + SA(b))) And &HFF) = v
                End While

                x = xi
                a = b

                If a < last AndAlso x < v Then
                    While Threading.Interlocked.Increment(b) < last AndAlso (xi = T(Td + SA(PA + SA(b))) And &HFF) <= v
                        x = xi

                        If x = v Then
                            swapElements(SA, b, SA, a)
                            Threading.Interlocked.Increment(a)
                        End If
                    End While
                End If

                Dim c As Integer
                c = last

                While b < Threading.Interlocked.Decrement(c) AndAlso (xi = T(Td + SA(PA + SA(c))) And &HFF) = v
                End While

                x = xi

                Dim d As Integer = c

                If b < c AndAlso x > v Then
                    While b < Threading.Interlocked.Decrement(c) AndAlso (xi = T(Td + SA(PA + SA(c))) And &HFF) >= v
                        x = xi

                        If x = v Then
                            swapElements(SA, c, SA, d)
                            Threading.Interlocked.Decrement(d)
                        End If
                    End While
                End If

                While b < c
                    swapElements(SA, b, SA, c)

                    While Threading.Interlocked.Increment(b) < c AndAlso (xi = T(Td + SA(PA + SA(b))) And &HFF) <= v
                        x = xi

                        If x = v Then
                            swapElements(SA, b, SA, a)
                            Threading.Interlocked.Increment(a)
                        End If
                    End While

                    While b < Threading.Interlocked.Decrement(c) AndAlso (xi = T(Td + SA(PA + SA(c))) And &HFF) >= v
                        x = xi

                        If x = v Then
                            swapElements(SA, c, SA, d)
                            Threading.Interlocked.Decrement(d)
                        End If
                    End While
                End While

                If a <= d Then
                    c = b - 1
                    Dim s As Integer = a - first
                    Dim t As Integer = b - a

                    If s > t Then
                        s = t
                    End If

                    Dim e As Integer
                    Dim f As Integer
                    e = first
                    f = b - s

                    While 0 < s
                        swapElements(SA, e, SA, f)
                        Threading.Interlocked.Decrement(s)
                        Threading.Interlocked.Increment(e)
                        Threading.Interlocked.Increment(f)
                    End While

                    s = (d - c)
                    t = (last - d - 1)

                    If s > t Then
                        s = t
                    End If

                    e = b
                    f = last - s

                    While 0 < s
                        swapElements(SA, e, SA, f)
                        Threading.Interlocked.Decrement(s)
                        Threading.Interlocked.Increment(e)
                        Threading.Interlocked.Increment(f)
                    End While

                    a = first + (b - a)
                    c = last - (d - c)
                    b = If(v <= (Me.T(Td + SA(PA + SA(a)) - 1) And &HFF), a, ssSubstringPartition(PA, a, c, depth))

                    If a - first <= last - c Then
                        If last - c <= c - b Then
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(b, c, depth + 1, ssLog(c - b))
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(c, last, depth, limit)
                            last = a
                        ElseIf a - first <= c - b Then
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(c, last, depth, limit)
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(b, c, depth + 1, ssLog(c - b))
                            last = a
                        Else
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(c, last, depth, limit)
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(first, a, depth, limit)
                            first = b
                            last = c
                            depth += 1
                            limit = ssLog(c - b)
                        End If
                    Else

                        If a - first <= c - b Then
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(b, c, depth + 1, ssLog(c - b))
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(first, a, depth, limit)
                            first = c
                        ElseIf last - c <= c - b Then
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(first, a, depth, limit)
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(b, c, depth + 1, ssLog(c - b))
                            first = c
                        Else
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(first, a, depth, limit)
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(c, last, depth, limit)
                            first = b
                            last = c
                            depth += 1
                            limit = ssLog(c - b)
                        End If
                    End If
                Else
                    limit += 1

                    If (T(Td + SA(PA + SA(first)) - 1) And &HFF) < v Then
                        first = ssSubstringPartition(PA, first, last, depth)
                        limit = ssLog(last - first)
                    End If

                    depth += 1
                End If
            End While
        End Sub

        Private Shared Sub ssBlockSwap(array1 As Integer(), first1 As Integer, array2 As Integer(), first2 As Integer, size As Integer)
            Dim a, b As Integer
            Dim i As Integer
            i = size
            a = first1
            b = first2

            While 0 < i
                swapElements(array1, a, array2, b)
                Threading.Interlocked.Decrement(i)
                Threading.Interlocked.Increment(a)
                Threading.Interlocked.Increment(b)
            End While
        End Sub

        Private Sub ssMergeForward(PA As Integer, buf As Integer(), bufoffset As Integer, first As Integer, middle As Integer, last As Integer, depth As Integer)
            Dim i, j, k As Integer
            Dim t As Integer
            Dim bufend = bufoffset + (middle - first) - 1
            ssBlockSwap(buf, bufoffset, SA, first, middle - first)
            t = SA(first)
            i = first
            j = bufoffset
            k = middle

            While True
                Dim r = ssCompare(PA + buf(j), PA + SA(k), depth)

                If r < 0 Then
                    Do
                        SA(std.Min(Threading.Interlocked.Increment(i), i - 1)) = buf(j)

                        If bufend <= j Then
                            buf(j) = t
                            Return
                        End If

                        buf(std.Min(Threading.Interlocked.Increment(j), j - 1)) = SA(i)
                    Loop While buf(j) < 0
                ElseIf r > 0 Then

                    Do
                        SA(std.Min(Threading.Interlocked.Increment(i), i - 1)) = SA(k)
                        SA(std.Min(Threading.Interlocked.Increment(k), k - 1)) = SA(i)

                        If last <= k Then
                            While j < bufend
                                SA(std.Min(Threading.Interlocked.Increment(i), i - 1)) = buf(j)
                                buf(std.Min(Threading.Interlocked.Increment(j), j - 1)) = SA(i)
                            End While

                            SA(i) = buf(j)
                            buf(j) = t
                            Return
                        End If
                    Loop While SA(k) < 0
                Else
                    SA(k) = Not SA(k)

                    Do
                        SA(std.Min(Threading.Interlocked.Increment(i), i - 1)) = buf(j)

                        If bufend <= j Then
                            buf(j) = t
                            Return
                        End If

                        buf(std.Min(Threading.Interlocked.Increment(j), j - 1)) = SA(i)
                    Loop While buf(j) < 0

                    Do
                        SA(std.Min(Threading.Interlocked.Increment(i), i - 1)) = SA(k)
                        SA(std.Min(Threading.Interlocked.Increment(k), k - 1)) = SA(i)

                        If last <= k Then
                            While j < bufend
                                SA(std.Min(Threading.Interlocked.Increment(i), i - 1)) = buf(j)
                                buf(std.Min(Threading.Interlocked.Increment(j), j - 1)) = SA(i)
                            End While

                            SA(i) = buf(j)
                            buf(j) = t
                            Return
                        End If
                    Loop While SA(k) < 0
                End If
            End While
        End Sub

        Private Sub ssMergeBackward(PA As Integer, buf As Integer(), bufoffset As Integer, first As Integer, middle As Integer, last As Integer, depth As Integer)
            Dim p1, p2 As Integer
            Dim i, j, k As Integer
            Dim t As Integer
            Dim bufend = bufoffset + (last - middle)
            ssBlockSwap(buf, bufoffset, SA, middle, last - middle)
            Dim x = 0

            If buf(bufend - 1) < 0 Then
                x = x Or 1
                p1 = PA + Not buf(bufend - 1)
            Else
                p1 = PA + buf(bufend - 1)
            End If

            If SA(middle - 1) < 0 Then
                x = x Or 2
                p2 = PA + Not SA(middle - 1)
            Else
                p2 = PA + SA(middle - 1)
            End If

            t = SA(last - 1)
            i = last - 1
            j = bufend - 1
            k = middle - 1

            While True
                Dim r = ssCompare(p1, p2, depth)

                If r > 0 Then
                    If (x And 1) <> 0 Then
                        Do
                            SA(std.Max(Threading.Interlocked.Decrement(i), i + 1)) = buf(j)
                            buf(std.Max(Threading.Interlocked.Decrement(j), j + 1)) = SA(i)
                        Loop While buf(j) < 0

                        x = x Xor 1
                    End If

                    SA(std.Max(Threading.Interlocked.Decrement(i), i + 1)) = buf(j)

                    If j <= bufoffset Then
                        buf(j) = t
                        Return
                    End If

                    buf(std.Max(Threading.Interlocked.Decrement(j), j + 1)) = SA(i)

                    If buf(j) < 0 Then
                        x = x Or 1
                        p1 = PA + Not buf(j)
                    Else
                        p1 = PA + buf(j)
                    End If
                ElseIf r < 0 Then

                    If (x And 2) <> 0 Then
                        Do
                            SA(std.Max(Threading.Interlocked.Decrement(i), i + 1)) = SA(k)
                            SA(std.Max(Threading.Interlocked.Decrement(k), k + 1)) = SA(i)
                        Loop While SA(k) < 0

                        x = x Xor 2
                    End If

                    SA(std.Max(Threading.Interlocked.Decrement(i), i + 1)) = SA(k)
                    SA(std.Max(Threading.Interlocked.Decrement(k), k + 1)) = SA(i)

                    If k < first Then
                        While bufoffset < j
                            SA(std.Max(Threading.Interlocked.Decrement(i), i + 1)) = buf(j)
                            buf(std.Max(Threading.Interlocked.Decrement(j), j + 1)) = SA(i)
                        End While

                        SA(i) = buf(j)
                        buf(j) = t
                        Return
                    End If

                    If SA(k) < 0 Then
                        x = x Or 2
                        p2 = PA + Not SA(k)
                    Else
                        p2 = PA + SA(k)
                    End If
                Else

                    If (x And 1) <> 0 Then
                        Do
                            SA(std.Max(Threading.Interlocked.Decrement(i), i + 1)) = buf(j)
                            buf(std.Max(Threading.Interlocked.Decrement(j), j + 1)) = SA(i)
                        Loop While buf(j) < 0

                        x = x Xor 1
                    End If

                    SA(std.Max(Threading.Interlocked.Decrement(i), i + 1)) = Not buf(j)

                    If j <= bufoffset Then
                        buf(j) = t
                        Return
                    End If

                    buf(std.Max(Threading.Interlocked.Decrement(j), j + 1)) = SA(i)

                    If (x And 2) <> 0 Then
                        Do
                            SA(std.Max(Threading.Interlocked.Decrement(i), i + 1)) = SA(k)
                            SA(std.Max(Threading.Interlocked.Decrement(k), k + 1)) = SA(i)
                        Loop While SA(k) < 0

                        x = x Xor 2
                    End If

                    SA(std.Max(Threading.Interlocked.Decrement(i), i + 1)) = SA(k)
                    SA(std.Max(Threading.Interlocked.Decrement(k), k + 1)) = SA(i)

                    If k < first Then
                        While bufoffset < j
                            SA(std.Max(Threading.Interlocked.Decrement(i), i + 1)) = buf(j)
                            buf(std.Max(Threading.Interlocked.Decrement(j), j + 1)) = SA(i)
                        End While

                        SA(i) = buf(j)
                        buf(j) = t
                        Return
                    End If

                    If buf(j) < 0 Then
                        x = x Or 1
                        p1 = PA + Not buf(j)
                    Else
                        p1 = PA + buf(j)
                    End If

                    If SA(k) < 0 Then
                        x = x Or 2
                        p2 = PA + Not SA(k)
                    Else
                        p2 = PA + SA(k)
                    End If
                End If
            End While
        End Sub

        Private Shared Function getIDX(a As Integer) As Integer
            Return If(0 <= a, a, Not a)
        End Function

        Private Sub ssMergeCheckEqual(PA As Integer, depth As Integer, a As Integer)
            If 0 <= SA(a) AndAlso ssCompare(PA + getIDX(SA(a - 1)), PA + SA(a), depth) = 0 Then
                SA(a) = Not SA(a)
            End If
        End Sub

        Private Sub ssMerge(PA As Integer, first As Integer, middle As Integer, last As Integer, buf As Integer(), bufoffset As Integer, bufsize As Integer, depth As Integer)
            Dim stack = New StackEntry(63) {}
            Dim ssize As Integer
            Dim check As Integer
            check = 0
            ssize = 0

            While True

                If last - middle <= bufsize Then
                    If first < middle AndAlso middle < last Then
                        ssMergeBackward(PA, buf, bufoffset, first, middle, last, depth)
                    End If

                    If (check And 1) <> 0 Then
                        ssMergeCheckEqual(PA, depth, first)
                    End If

                    If (check And 2) <> 0 Then
                        ssMergeCheckEqual(PA, depth, last)
                    End If

                    If ssize = 0 Then Return
                    Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                    first = entry.a
                    middle = entry.b
                    last = entry.c
                    check = entry.d
                    Continue While
                End If

                If middle - first <= bufsize Then
                    If first < middle Then
                        ssMergeForward(PA, buf, bufoffset, first, middle, last, depth)
                    End If

                    If (check And 1) <> 0 Then
                        ssMergeCheckEqual(PA, depth, first)
                    End If

                    If (check And 2) <> 0 Then
                        ssMergeCheckEqual(PA, depth, last)
                    End If

                    If ssize = 0 Then Return
                    Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                    first = entry.a
                    middle = entry.b
                    last = entry.c
                    check = entry.d
                    Continue While
                End If

                Dim m As Integer
                Dim len As Integer
                Dim half As Integer
                m = 0
                len = std.Min(middle - first, last - middle)
                half = len >> 1

                While 0 < len

                    If ssCompare(PA + getIDX(SA(middle + m + half)), PA + getIDX(SA(middle - m - half - 1)), depth) < 0 Then
                        m += half + 1
                        half -= len And 1 Xor 1
                    End If

                    len = half
                    half >>= 1
                End While

                If 0 < m Then
                    ssBlockSwap(SA, middle - m, SA, middle, m)
                    Dim j As Integer = middle
                    Dim i As Integer = middle
                    Dim [next] = 0

                    If middle + m < last Then
                        If SA(middle + m) < 0 Then
                            While SA(i - 1) < 0
                                Threading.Interlocked.Decrement(i)
                            End While

                            SA(middle + m) = Not SA(middle + m)
                        End If

                        j = middle

                        While SA(j) < 0
                            Threading.Interlocked.Increment(j)
                        End While

                        [next] = 1
                    End If

                    If i - first <= last - j Then
                        stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(j, middle + m, last, check And 2 Or [next] And 1)
                        middle -= m
                        last = i
                        check = check And 1
                    Else

                        If i = middle AndAlso middle = j Then
                            [next] <<= 1
                        End If

                        stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(first, middle - m, i, check And 1 Or [next] And 2)
                        first = j
                        middle += m
                        check = check And 2 Or [next] And 1
                    End If
                Else

                    If (check And 1) <> 0 Then
                        ssMergeCheckEqual(PA, depth, first)
                    End If

                    ssMergeCheckEqual(PA, depth, middle)

                    If (check And 2) <> 0 Then
                        ssMergeCheckEqual(PA, depth, last)
                    End If

                    If ssize = 0 Then Return
                    Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                    first = entry.a
                    middle = entry.b
                    last = entry.c
                    check = entry.d
                End If
            End While
        End Sub

        Private Sub subStringSort(PA As Integer, first As Integer, last As Integer, buf As Integer(), bufoffset As Integer, bufsize As Integer, depth As Integer, lastsuffix As Boolean, size As Integer)
            Dim a As Integer
            Dim i As Integer
            Dim k As Integer
            If lastsuffix Then Threading.Interlocked.Increment(first)
            a = first
            i = 0

            While a + SS_BLOCKSIZE < last
                ssMultiKeyIntroSort(PA, a, a + SS_BLOCKSIZE, depth)
                Dim curbuf = SA
                Dim curbufoffset = a + SS_BLOCKSIZE
                Dim curbufsize = last - (a + SS_BLOCKSIZE)

                If curbufsize <= bufsize Then
                    curbufsize = bufsize
                    curbuf = buf
                    curbufoffset = bufoffset
                End If

                Dim b As Integer
                Dim j As Integer
                b = a
                k = SS_BLOCKSIZE
                j = i

                While (j And 1) <> 0
                    ssMerge(PA, b - k, b, b + k, curbuf, curbufoffset, curbufsize, depth)
                    b -= k
                    k <<= 1
                    j >>= 1
                End While

                a += SS_BLOCKSIZE
                Threading.Interlocked.Increment(i)
            End While

            ssMultiKeyIntroSort(PA, a, last, depth)
            k = SS_BLOCKSIZE

            While i <> 0

                If (i And 1) <> 0 Then
                    ssMerge(PA, a - k, a, last, buf, bufoffset, bufsize, depth)
                    a -= k
                End If

                k <<= 1
                i >>= 1
            End While

            If lastsuffix Then
                Dim r As Value(Of Integer)

                a = first
                i = SA(first - 1)
                r = 1

                While a < last AndAlso (SA(a) < 0 OrElse 0 < (r = ssCompareLast(PA, PA + i, PA + SA(a), depth, size)))
                    SA(a - 1) = SA(a)
                    Threading.Interlocked.Increment(a)
                End While

                If CInt(r) = 0 Then
                    SA(a) = Not SA(a)
                End If

                SA(a - 1) = i
            End If
        End Sub

        Private Function trGetC(ISA As Integer, ISAd As Integer, ISAn As Integer, p As Integer) As Integer
            Return If(ISAd + p < ISAn, SA(ISAd + p), SA(ISA + (ISAd - ISA + p) Mod (ISAn - ISA)))
        End Function

        Private Sub trFixdown(ISA As Integer, ISAd As Integer, ISAn As Integer, sa As Integer, i As Integer, size As Integer)
            Dim j As Value(Of Integer) = 0
            Dim k As Integer
            Dim v As Integer
            Dim c As Integer

            v = Me.SA(sa + i)
            c = trGetC(ISA, ISAd, ISAn, v)

            While (j = 2 * i + 1) < size
                k = std.Min(Threading.Interlocked.Increment(j.Value), CInt(j) - 1)

                Dim d = trGetC(ISA, ISAd, ISAn, Me.SA(sa + k))
                Dim e As Integer = trGetC(ISA, ISAd, ISAn, Me.SA(sa + CInt(j)))

                If d < e Then
                    k = j
                    d = e
                End If

                If d <= c Then
                    Exit While
                End If

                Me.SA(sa + i) = Me.SA(sa + k)
                i = k
            End While

            Me.SA(sa + i) = v
        End Sub

        Private Sub trHeapSort(ISA As Integer, ISAd As Integer, ISAn As Integer, sa As Integer, size As Integer)
            Dim i As Integer
            Dim m = size

            If size Mod 2 = 0 Then
                m -= 1

                If trGetC(ISA, ISAd, ISAn, Me.SA(sa + m / 2)) < trGetC(ISA, ISAd, ISAn, Me.SA(sa + m)) Then
                    swapElements(Me.SA, sa + m, Me.SA, sa + m / 2)
                End If
            End If

            i = m / 2 - 1

            While 0 <= i
                trFixdown(ISA, ISAd, ISAn, sa, i, m)
                Threading.Interlocked.Decrement(i)
            End While

            If size Mod 2 = 0 Then
                swapElements(Me.SA, sa + 0, Me.SA, sa + m)
                trFixdown(ISA, ISAd, ISAn, sa, 0, m)
            End If

            i = m - 1

            While 0 < i
                Dim t = Me.SA(sa + 0)
                Me.SA(sa + 0) = Me.SA(sa + i)
                trFixdown(ISA, ISAd, ISAn, sa, 0, i)
                Me.SA(sa + i) = t
                Threading.Interlocked.Decrement(i)
            End While
        End Sub

        Private Sub trInsertionSort(ISA As Integer, ISAd As Integer, ISAn As Integer, first As Integer, last As Integer)
            Dim a As Integer
            a = first + 1

            While a < last
                Dim b As Integer
                Dim t As Integer
                Dim r As Value(Of Integer) = 0

                t = SA(a)
                b = a - 1

                While 0 > (r = trGetC(ISA, ISAd, ISAn, t) - trGetC(ISA, ISAd, ISAn, SA(b)))

                    Do
                        SA(b + 1) = SA(b)
                    Loop While first <= Threading.Interlocked.Decrement(b) AndAlso SA(b) < 0

                    If b < first Then
                        Exit While
                    End If
                End While

                If CInt(r) = 0 Then
                    SA(b) = Not SA(b)
                End If

                SA(b + 1) = t
                Threading.Interlocked.Increment(a)
            End While
        End Sub

        Private Shared Function trLog(x As Integer) As Integer
            Return If((x And &HFFFF0000) <> 0, If((x And &HFF000000) <> 0, 24 + log2table(x >> 24 And &HFF), 16 + log2table(x >> 16 And &HFF)), If((x And &HFF00) <> 0, 8 + log2table(x >> 8 And &HFF), 0 + log2table(x >> 0 And &HFF)))
        End Function

        Private Function trMedian3(ISA As Integer, ISAd As Integer, ISAn As Integer, v1 As Integer, v2 As Integer, v3 As Integer) As Integer
            Dim SA_v1 = trGetC(ISA, ISAd, ISAn, SA(v1))
            Dim SA_v2 = trGetC(ISA, ISAd, ISAn, SA(v2))
            Dim SA_v3 = trGetC(ISA, ISAd, ISAn, SA(v3))

            If SA_v1 > SA_v2 Then
                Dim temp = v1
                v1 = v2
                v2 = temp
                Dim SA_vtemp = SA_v1
                SA_v1 = SA_v2
                SA_v2 = SA_vtemp
            End If

            If SA_v2 > SA_v3 Then
                Return If(SA_v1 > SA_v3, v1, v3)
            End If

            Return v2
        End Function

        Private Function trMedian5(ISA As Integer, ISAd As Integer, ISAn As Integer, v1 As Integer, v2 As Integer, v3 As Integer, v4 As Integer, v5 As Integer) As Integer
            Dim SA_v1 = trGetC(ISA, ISAd, ISAn, SA(v1))
            Dim SA_v2 = trGetC(ISA, ISAd, ISAn, SA(v2))
            Dim SA_v3 = trGetC(ISA, ISAd, ISAn, SA(v3))
            Dim SA_v4 = trGetC(ISA, ISAd, ISAn, SA(v4))
            Dim SA_v5 = trGetC(ISA, ISAd, ISAn, SA(v5))
            Dim temp As Integer
            Dim SA_vtemp As Integer

            ' ReSharper disable RedundantAssignment
            If SA_v2 > SA_v3 Then
                temp = v2
                v2 = v3
                v3 = temp
                SA_vtemp = SA_v2
                SA_v2 = SA_v3
                SA_v3 = SA_vtemp
            End If

            If SA_v4 > SA_v5 Then
                temp = v4
                v4 = v5
                v5 = temp
                SA_vtemp = SA_v4
                SA_v4 = SA_v5
                SA_v5 = SA_vtemp
            End If

            If SA_v2 > SA_v4 Then
                temp = v2
                v2 = v4
                v4 = temp
                SA_vtemp = SA_v2
                SA_v2 = SA_v4
                SA_v4 = SA_vtemp
                temp = v3
                v3 = v5
                v5 = temp
                SA_vtemp = SA_v3
                SA_v3 = SA_v5
                SA_v5 = SA_vtemp
            End If

            If SA_v1 > SA_v3 Then
                temp = v1
                v1 = v3
                v3 = temp
                SA_vtemp = SA_v1
                SA_v1 = SA_v3
                SA_v3 = SA_vtemp
            End If

            If SA_v1 > SA_v4 Then
                temp = v1
                v1 = v4
                v4 = temp
                SA_vtemp = SA_v1
                SA_v1 = SA_v4
                SA_v4 = SA_vtemp
                temp = v3
                v3 = v5
                v5 = temp
                SA_vtemp = SA_v3
                SA_v3 = SA_v5
                SA_v5 = SA_vtemp
            End If
            ' ReSharper restore RedundantAssignment

            Return If(SA_v3 > SA_v4, v4, v3)
        End Function

        Private Function trPivot(ISA As Integer, ISAd As Integer, ISAn As Integer, first As Integer, last As Integer) As Integer
            Dim t = last - first
            Dim middle As Integer = first + t / 2

            If t <= 512 Then
                If t <= 32 Then
                    Return trMedian3(ISA, ISAd, ISAn, first, middle, last - 1)
                End If

                t >>= 2
                Return trMedian5(ISA, ISAd, ISAn, first, first + t, middle, last - 1 - t, last - 1)
            End If

            t >>= 3
            Return trMedian3(ISA, ISAd, ISAn, trMedian3(ISA, ISAd, ISAn, first, first + t, first + (t << 1)), trMedian3(ISA, ISAd, ISAn, middle - t, middle, middle + t), trMedian3(ISA, ISAd, ISAn, last - 1 - (t << 1), last - 1 - t, last - 1))
        End Function

        Private Sub lsUpdateGroup(ISA As Integer, first As Integer, last As Integer)
            Dim a As Integer
            a = first

            While a < last
                Dim b As Integer

                If 0 <= SA(a) Then
                    b = a

                    Do
                        SA(ISA + SA(a)) = a
                    Loop While Threading.Interlocked.Increment(a) < last AndAlso 0 <= SA(a)

                    SA(b) = b - a

                    If last <= a Then
                        Exit While
                    End If
                End If

                b = a

                Do
                    SA(a) = Not SA(a)
                Loop While SA(Threading.Interlocked.Increment(a)) < 0

                Dim t = a

                Do
                    SA(ISA + SA(b)) = t
                Loop While Threading.Interlocked.Increment(b) <= a

                Threading.Interlocked.Increment(a)
            End While
        End Sub

        Private Sub lsIntroSort(ISA As Integer, ISAd As Integer, ISAn As Integer, first As Integer, last As Integer)
            Dim stack = New StackEntry(63) {}
            Dim limit As Integer
            Dim x As Value(Of Integer) = 0
            Dim ssize As Integer

            ssize = 0
            limit = trLog(last - first)

            While True

                If last - first <= INSERTIONSORT_THRESHOLD Then
                    If 1 < last - first Then
                        trInsertionSort(ISA, ISAd, ISAn, first, last)
                        lsUpdateGroup(ISA, first, last)
                    ElseIf last - first = 1 Then
                        SA(first) = -1
                    End If

                    If ssize = 0 Then Return
                    Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                    first = entry.a
                    last = entry.b
                    limit = entry.c
                    Continue While
                End If

                Dim a As Integer
                Dim b As Integer

                If std.Max(Threading.Interlocked.Decrement(limit), limit + 1) = 0 Then
                    trHeapSort(ISA, ISAd, ISAn, first, last - first)
                    a = last - 1

                    While first < a
                        x = trGetC(ISA, ISAd, ISAn, SA(a))
                        b = a - 1

                        While first <= b AndAlso trGetC(ISA, ISAd, ISAn, SA(b)) = CInt(x)
                            SA(b) = Not SA(b)
                            Threading.Interlocked.Decrement(b)
                        End While

                        a = b
                    End While

                    lsUpdateGroup(ISA, first, last)
                    If ssize = 0 Then Return
                    Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                    first = entry.a
                    last = entry.b
                    limit = entry.c
                    Continue While
                End If

                a = trPivot(ISA, ISAd, ISAn, first, last)
                swapElements(SA, first, SA, a)
                Dim v = trGetC(ISA, ISAd, ISAn, SA(first))
                b = first

                While Threading.Interlocked.Increment(b) < last AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(b))) = v
                End While

                a = b

                If a < last AndAlso CInt(x) < v Then
                    While Threading.Interlocked.Increment(b) < last AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(b))) <= v

                        If CInt(x) = v Then
                            swapElements(SA, b, SA, a)
                            Threading.Interlocked.Increment(a)
                        End If
                    End While
                End If

                Dim c As Integer
                c = last

                While b < Threading.Interlocked.Decrement(c) AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(c))) = v
                End While

                Dim d As Integer = c

                If b < d AndAlso CInt(x) > v Then
                    While b < Threading.Interlocked.Decrement(c) AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(c))) >= v

                        If CInt(x) = v Then
                            swapElements(SA, c, SA, d)
                            Threading.Interlocked.Decrement(d)
                        End If
                    End While
                End If

                While b < c
                    swapElements(SA, b, SA, c)

                    While Threading.Interlocked.Increment(b) < c AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(b))) <= v

                        If CInt(x) = v Then
                            swapElements(SA, b, SA, a)
                            Threading.Interlocked.Increment(a)
                        End If
                    End While

                    While b < Threading.Interlocked.Decrement(c) AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(c))) >= v

                        If CInt(x) = v Then
                            swapElements(SA, c, SA, d)
                            Threading.Interlocked.Decrement(d)
                        End If
                    End While
                End While

                If a <= d Then
                    c = b - 1
                    Dim s As Integer = a - first
                    Dim t As Integer = b - a

                    If s > t Then
                        s = t
                    End If

                    Dim e As Integer
                    Dim f As Integer
                    e = first
                    f = b - s

                    While 0 < s
                        swapElements(SA, e, SA, f)
                        Threading.Interlocked.Decrement(s)
                        Threading.Interlocked.Increment(e)
                        Threading.Interlocked.Increment(f)
                    End While

                    s = d - c
                    t = last - d - 1

                    If s > t Then
                        s = t
                    End If

                    e = b
                    f = last - s

                    While 0 < s
                        swapElements(SA, e, SA, f)
                        Threading.Interlocked.Decrement(s)
                        Threading.Interlocked.Increment(e)
                        Threading.Interlocked.Increment(f)
                    End While

                    a = first + (b - a)
                    b = last - (d - c)
                    c = first
                    v = a - 1

                    While c < a
                        SA(ISA + SA(c)) = v
                        Threading.Interlocked.Increment(c)
                    End While

                    If b < last Then
                        c = a
                        v = b - 1

                        While c < b
                            SA(ISA + SA(c)) = v
                            Threading.Interlocked.Increment(c)
                        End While
                    End If

                    If b - a = 1 Then
                        SA(a) = -1
                    End If

                    If a - first <= last - b Then
                        If first < a Then
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(b, last, limit, 0)
                            last = a
                        Else
                            first = b
                        End If
                    Else

                        If b < last Then
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(first, a, limit, 0)
                            first = b
                        Else
                            last = a
                        End If
                    End If
                Else
                    If ssize = 0 Then Return
                    Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                    first = entry.a
                    last = entry.b
                    limit = entry.c
                End If
            End While
        End Sub

        Private Sub lsSort(ISA As Integer, x As Integer, depth As Integer)
            Dim ISAd As Integer
            Dim t As Value(Of Integer) = Nothing

            ISAd = ISA + depth

            While -x < SA(0)
                Dim first = 0
                Dim skip = 0
                Dim last As Integer

                Do

                    If (t = SA(first)) < 0 Then
                        first -= CInt(t)
                        skip += CInt(t)
                    Else

                        If skip <> 0 Then
                            SA(first + skip) = skip
                            skip = 0
                        End If

                        last = SA(ISA + CInt(t)) + 1
                        lsIntroSort(ISA, ISAd, ISA + x, first, last)
                        first = last
                    End If
                Loop While first < x

                If skip <> 0 Then SA(first + skip) = skip

                If x < ISAd - ISA Then
                    first = 0

                    Do

                        If (t = SA(first)) < 0 Then
                            first -= CInt(t)
                        Else
                            last = SA(ISA + CInt(t)) + 1
                            Dim i As Integer
                            i = first

                            While i < last
                                SA(ISA + SA(i)) = i
                                Threading.Interlocked.Increment(i)
                            End While

                            first = last
                        End If
                    Loop While first < x

                    Exit While
                End If

                ISAd += ISAd - ISA
            End While
        End Sub

        Private Function trPartition(ISA As Integer, ISAd As Integer, ISAn As Integer, first As Integer, last As Integer, v As Integer) As PartitionResult
            Dim a, b, c, d As Integer
            Dim x As Value(Of Integer) = 0

            b = first - 1

            While Threading.Interlocked.Increment(b) < last AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(b))) = v
            End While

            a = b

            If a < last AndAlso CInt(x) < v Then
                While Threading.Interlocked.Increment(b) < last AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(b))) <= v

                    If CInt(x) = v Then
                        swapElements(SA, b, SA, a)
                        Threading.Interlocked.Increment(a)
                    End If
                End While
            End If

            c = last

            While b < Threading.Interlocked.Decrement(c) AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(c))) = v
            End While

            d = c

            If b < d AndAlso CInt(x) > v Then
                While b < Threading.Interlocked.Decrement(c) AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(c))) >= v

                    If CInt(x) = v Then
                        swapElements(SA, c, SA, d)
                        Threading.Interlocked.Decrement(d)
                    End If
                End While
            End If

            While b < c
                swapElements(SA, b, SA, c)

                While Threading.Interlocked.Increment(b) < c AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(b))) <= v

                    If CInt(x) = v Then
                        swapElements(SA, b, SA, a)
                        Threading.Interlocked.Increment(a)
                    End If
                End While

                While b < Threading.Interlocked.Decrement(c) AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(c))) >= v

                    If CInt(x) = v Then
                        swapElements(SA, c, SA, d)
                        Threading.Interlocked.Decrement(d)
                    End If
                End While
            End While

            If a <= d Then
                c = b - 1
                Dim t As Integer = b - a
                Dim s As Integer = a - first

                If s > t Then
                    s = t
                End If

                Dim e As Integer
                Dim f As Integer
                e = first
                f = b - s

                While 0 < s
                    swapElements(SA, e, SA, f)
                    Threading.Interlocked.Decrement(s)
                    Threading.Interlocked.Increment(e)
                    Threading.Interlocked.Increment(f)
                End While

                s = (d - c)
                t = (last - d - 1)

                If s > t Then
                    s = t
                End If

                e = b
                f = last - s

                While 0 < s
                    swapElements(SA, e, SA, f)
                    Threading.Interlocked.Decrement(s)
                    Threading.Interlocked.Increment(e)
                    Threading.Interlocked.Increment(f)
                End While

                first += b - a
                last -= d - c
            End If

            Return New PartitionResult(first, last)
        End Function

        Private Sub trCopy(ISA As Integer, ISAn As Integer, first As Integer, a As Integer, b As Integer, last As Integer, depth As Integer)
            Dim c, d, e As Integer
            Dim s As Value(Of Integer) = Nothing
            Dim v = b - 1

            c = first
            d = a - 1

            While c <= d

                If (s = SA(c) - depth) < 0 Then
                    s.Value += ISAn - ISA
                End If

                If SA(ISA + CInt(s)) = v Then
                    SA(Threading.Interlocked.Increment(d)) = s
                    SA(ISA + CInt(s)) = d
                End If

                Threading.Interlocked.Increment(c)
            End While

            c = last - 1
            e = d + 1
            d = b

            While e < d

                If (s = SA(c) - depth) < 0 Then
                    s.Value += ISAn - ISA
                End If

                If SA(ISA + CInt(s)) = v Then
                    SA(Threading.Interlocked.Decrement(d)) = s
                    SA(ISA + CInt(s)) = d
                End If

                Threading.Interlocked.Decrement(c)
            End While
        End Sub

        Private Sub trIntroSort(ISA As Integer, ISAd As Integer, ISAn As Integer, first As Integer, last As Integer, budget As TRBudget, size As Integer)
            Dim stack = New StackEntry(63) {}
            Dim s As Integer
            Dim x As Value(Of Integer) = 0
            Dim limit As Integer
            Dim ssize As Integer
            ssize = 0
            limit = trLog(last - first)

            While True
                Dim a As Integer
                Dim b As Integer
                Dim c As Integer
                Dim v As Integer
                Dim [next] As Integer

                If limit < 0 Then
                    If limit = -1 Then
                        If Not budget.update(size, last - first) Then Exit While
                        Dim result = trPartition(ISA, ISAd - 1, ISAn, first, last, last - 1)
                        a = result.first
                        b = result.last

                        If first < a OrElse b < last Then
                            If a < last Then
                                c = first
                                v = a - 1

                                While c < a
                                    SA(ISA + SA(c)) = v
                                    Threading.Interlocked.Increment(c)
                                End While
                            End If

                            If b < last Then
                                c = a
                                v = b - 1

                                While c < b
                                    SA(ISA + SA(c)) = v
                                    Threading.Interlocked.Increment(c)
                                End While
                            End If

                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(0, a, b, 0)
                            stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd - 1, first, last, -2)

                            If a - first <= last - b Then
                                If 1 < a - first Then
                                    stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, b, last, trLog(last - b))
                                    last = a
                                    limit = trLog(a - first)
                                ElseIf 1 < last - b Then
                                    first = b
                                    limit = trLog(last - b)
                                Else
                                    If ssize = 0 Then Return
                                    Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                                    ISAd = entry.a
                                    first = entry.b
                                    last = entry.c
                                    limit = entry.d
                                End If
                            Else

                                If 1 < last - b Then
                                    stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, first, a, trLog(a - first))
                                    first = b
                                    limit = trLog(last - b)
                                ElseIf 1 < a - first Then
                                    last = a
                                    limit = trLog(a - first)
                                Else
                                    If ssize = 0 Then Return
                                    Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                                    ISAd = entry.a
                                    first = entry.b
                                    last = entry.c
                                    limit = entry.d
                                End If
                            End If
                        Else
                            c = first

                            While c < last
                                SA(ISA + SA(c)) = c
                                Threading.Interlocked.Increment(c)
                            End While

                            If ssize = 0 Then Return
                            Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                            ISAd = entry.a
                            first = entry.b
                            last = entry.c
                            limit = entry.d
                        End If
                    ElseIf limit = -2 Then
                        a = stack(Threading.Interlocked.Decrement(ssize)).b
                        b = stack(ssize).c
                        trCopy(ISA, ISAn, first, a, b, last, ISAd - ISA)
                        If ssize = 0 Then Return
                        Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                        ISAd = entry.a
                        first = entry.b
                        last = entry.c
                        limit = entry.d
                    Else

                        If 0 <= SA(first) Then
                            a = first

                            Do
                                SA(ISA + SA(a)) = a
                            Loop While Threading.Interlocked.Increment(a) < last AndAlso 0 <= SA(a)

                            first = a
                        End If

                        If first < last Then
                            a = first

                            Do
                                SA(a) = Not SA(a)
                            Loop While SA(Threading.Interlocked.Increment(a)) < 0

                            [next] = If(SA(ISA + SA(a)) <> SA(ISAd + SA(a)), trLog(a - first + 1), -1)

                            If Threading.Interlocked.Increment(a) < last Then
                                b = first
                                v = a - 1

                                While b < a
                                    SA(ISA + SA(b)) = v
                                    Threading.Interlocked.Increment(b)
                                End While
                            End If

                            If a - first <= last - a Then
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, a, last, -3)
                                ISAd += 1
                                last = a
                                limit = [next]
                            Else

                                If 1 < last - a Then
                                    stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd + 1, first, a, [next])
                                    first = a
                                    limit = -3
                                Else
                                    ISAd += 1
                                    last = a
                                    limit = [next]
                                End If
                            End If
                        Else
                            If ssize = 0 Then Return
                            Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                            ISAd = entry.a
                            first = entry.b
                            last = entry.c
                            limit = entry.d
                        End If
                    End If

                    Continue While
                End If

                If last - first <= INSERTIONSORT_THRESHOLD Then
                    If Not budget.update(size, last - first) Then Exit While
                    trInsertionSort(ISA, ISAd, ISAn, first, last)
                    limit = -3
                    Continue While
                End If

                If std.Max(Threading.Interlocked.Decrement(limit), limit + 1) = 0 Then
                    If Not budget.update(size, last - first) Then Exit While
                    trHeapSort(ISA, ISAd, ISAn, first, last - first)
                    a = last - 1

                    While first < a
                        x = trGetC(ISA, ISAd, ISAn, SA(a))
                        b = a - 1

                        While first <= b AndAlso trGetC(ISA, ISAd, ISAn, SA(b)) = CInt(x)
                            SA(b) = Not SA(b)
                            Threading.Interlocked.Decrement(b)
                        End While

                        a = b
                    End While

                    limit = -3
                    Continue While
                End If

                a = trPivot(ISA, ISAd, ISAn, first, last)
                swapElements(SA, first, SA, a)
                v = trGetC(ISA, ISAd, ISAn, SA(first))
                b = first

                While Threading.Interlocked.Increment(b) < last AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(b))) = v
                End While

                a = b

                If a < last AndAlso CInt(x) < v Then
                    While Threading.Interlocked.Increment(b) < last AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(b))) <= v

                        If CInt(x) = v Then
                            swapElements(SA, b, SA, a)
                            Threading.Interlocked.Increment(a)
                        End If
                    End While
                End If

                c = last

                While b < Threading.Interlocked.Decrement(c) AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(c))) = v
                End While

                Dim d As Integer = c

                If b < d AndAlso CInt(x) > v Then
                    While b < Threading.Interlocked.Decrement(c) AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(c))) >= v

                        If CInt(x) = v Then
                            swapElements(SA, c, SA, d)
                            Threading.Interlocked.Decrement(d)
                        End If
                    End While
                End If

                While b < c
                    swapElements(SA, b, SA, c)

                    While Threading.Interlocked.Increment(b) < c AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(b))) <= v

                        If CInt(x) = v Then
                            swapElements(SA, b, SA, a)
                            Threading.Interlocked.Increment(a)
                        End If
                    End While

                    While b < Threading.Interlocked.Decrement(c) AndAlso (x = trGetC(ISA, ISAd, ISAn, SA(c))) >= v

                        If CInt(x) = v Then
                            swapElements(SA, c, SA, d)
                            Threading.Interlocked.Decrement(d)
                        End If
                    End While
                End While

                If a <= d Then
                    c = b - 1
                    Dim t As Integer

                    s = a - first
                    t = b - a

                    If s > t Then
                        s = t
                    End If

                    Dim e As Integer
                    Dim f As Integer
                    e = first
                    f = b - s

                    While 0 < s
                        swapElements(SA, e, SA, f)
                        Threading.Interlocked.Decrement(s)
                        Threading.Interlocked.Increment(e)
                        Threading.Interlocked.Increment(f)
                    End While

                    s = d - c
                    t = last - d - 1

                    If s > t Then
                        s = t
                    End If

                    e = b
                    f = last - s

                    While 0 < s
                        swapElements(SA, e, SA, f)
                        Threading.Interlocked.Decrement(s)
                        Threading.Interlocked.Increment(e)
                        Threading.Interlocked.Increment(f)
                    End While

                    a = first + (b - a)
                    b = last - (d - c)
                    [next] = If(SA(ISA + SA(a)) <> v, trLog(b - a), -1)
                    c = first
                    v = a - 1

                    While c < a
                        SA(ISA + SA(c)) = v
                        Threading.Interlocked.Increment(c)
                    End While

                    If b < last Then
                        c = a
                        v = b - 1

                        While c < b
                            SA(ISA + SA(c)) = v
                            Threading.Interlocked.Increment(c)
                        End While
                    End If

                    If a - first <= last - b Then
                        If last - b <= b - a Then
                            If 1 < a - first Then
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd + 1, a, b, [next])
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, b, last, limit)
                                last = a
                            ElseIf 1 < last - b Then
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd + 1, a, b, [next])
                                first = b
                            ElseIf 1 < b - a Then
                                ISAd += 1
                                first = a
                                last = b
                                limit = [next]
                            Else
                                If ssize = 0 Then Return
                                Dim entry = stack(Threading.Interlocked.Decrement(ssize))
                                ISAd = entry.a
                                first = entry.b
                                last = entry.c
                                limit = entry.d
                            End If
                        ElseIf a - first <= b - a Then

                            If 1 < a - first Then
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, b, last, limit)
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd + 1, a, b, [next])
                                last = a
                            ElseIf 1 < b - a Then
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, b, last, limit)
                                ISAd += 1
                                first = a
                                last = b
                                limit = [next]
                            Else
                                first = b
                            End If
                        Else

                            If 1 < b - a Then
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, b, last, limit)
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, first, a, limit)
                                ISAd += 1
                                first = a
                                last = b
                                limit = [next]
                            Else
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, b, last, limit)
                                last = a
                            End If
                        End If
                    Else

                        If a - first <= b - a Then
                            If 1 < last - b Then
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd + 1, a, b, [next])
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, first, a, limit)
                                first = b
                            ElseIf 1 < a - first Then
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd + 1, a, b, [next])
                                last = a
                            ElseIf 1 < b - a Then
                                ISAd += 1
                                first = a
                                last = b
                                limit = [next]
                            Else
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, first, last, limit)
                            End If
                        ElseIf last - b <= b - a Then

                            If 1 < last - b Then
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, first, a, limit)
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd + 1, a, b, [next])
                                first = b
                            ElseIf 1 < b - a Then
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, first, a, limit)
                                ISAd += 1
                                first = a
                                last = b
                                limit = [next]
                            Else
                                last = a
                            End If
                        Else

                            If 1 < b - a Then
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, first, a, limit)
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, b, last, limit)
                                ISAd += 1
                                first = a
                                last = b
                                limit = [next]
                            Else
                                stack(std.Min(Threading.Interlocked.Increment(ssize), ssize - 1)) = New StackEntry(ISAd, first, a, limit)
                                first = b
                            End If
                        End If
                    End If
                Else
                    If Not budget.update(size, last - first) Then Exit While ' BUGFIX : Added to prevent an infinite loop in the original code
                    limit += 1
                    ISAd += 1
                End If
            End While

            s = 0

            While s < ssize

                If stack(s).d = -3 Then
                    lsUpdateGroup(ISA, stack(s).b, stack(s).c)
                End If

                Threading.Interlocked.Increment(s)
            End While
        End Sub

        Private Sub trSort(ISA As Integer, x As Integer, depth As Integer)
            Dim first = 0
            Dim t As Value(Of Integer) = 0

            If -x < SA(0) Then
                Dim budget = New TRBudget(x, trLog(x) * 2 / 3 + 1)

                Do
                    If (t = SA(first)) < 0 Then
                        first -= CInt(t)
                    Else
                        Dim last = SA(ISA + CInt(t)) + 1

                        If 1 < last - first Then
                            trIntroSort(ISA, ISA + depth, ISA + x, first, last, budget, x)

                            If budget.chance = 0 Then
                                ' Switch to Larsson-Sadakane sorting algorithm. 
                                If 0 < first Then
                                    SA(0) = -first
                                End If

                                lsSort(ISA, x, depth)
                                Exit Do
                            End If
                        End If

                        first = last
                    End If
                Loop While first < x
            End If
        End Sub

        Private Shared Function BUCKET_B(c0 As Integer, c1 As Integer) As Integer
            Return c1 << 8 Or c0
        End Function

        Private Shared Function BUCKET_BSTAR(c0 As Integer, c1 As Integer) As Integer
            Return c0 << 8 Or c1
        End Function

        Private Function sortTypeBstar(bucketA As Integer(), bucketB As Integer()) As Integer
            Dim tempbuf = New Integer(255) {}
            Dim i, j, k, t As Integer
            Dim c0, c1 As Integer
            Dim flag As Integer
            i = 1
            flag = 1

            While i < n

                If Me.T(i - 1) <> Me.T(i) Then
                    If (Me.T(i - 1) And &HFF) > (Me.T(i) And &HFF) Then
                        flag = 0
                    End If

                    Exit While
                End If

                Threading.Interlocked.Increment(i)
            End While

            i = n - 1

            Dim m = n
            Dim ti As Integer = Me.T(i) And &HFF
            Dim ti1 As Integer
            Dim t0 As Integer = Me.T(0) And &HFF
            Dim tii As Value(Of Integer) = 0
            Dim tij As Value(Of Integer) = 0

            If ti < t0 OrElse Me.T(i) = Me.T(0) AndAlso flag <> 0 Then
                If flag = 0 Then
                    Threading.Interlocked.Increment(bucketB(BUCKET_BSTAR(ti, t0)))
                    SA(Threading.Interlocked.Decrement(m)) = i
                Else
                    Threading.Interlocked.Increment(bucketB(BUCKET_B(ti, t0)))
                End If

                Threading.Interlocked.Decrement(i)

                While 0 <= i AndAlso (tii = Me.T(i) And &HFF) <= (tij = Me.T(i + 1) And &HFF)
                    ti = tii
                    ti1 = tij

                    Threading.Interlocked.Increment(bucketB(BUCKET_B(ti, ti1)))
                    Threading.Interlocked.Decrement(i)
                End While
            End If

            While 0 <= i

                Do
                    Threading.Interlocked.Increment(bucketA(Me.T(i) And &HFF))
                Loop While 0 <= Threading.Interlocked.Decrement(i) AndAlso (Me.T(i) And &HFF) >= (Me.T(i + 1) And &HFF)

                If 0 <= i Then
                    Threading.Interlocked.Increment(bucketB(BUCKET_BSTAR(Me.T(i) And &HFF, Me.T(i + 1) And &HFF)))
                    SA(Threading.Interlocked.Decrement(m)) = i
                    Threading.Interlocked.Decrement(i)

                    While 0 <= i AndAlso (tii = Me.T(i) And &HFF) <= (tij = Me.T(i + 1) And &HFF)
                        ti = tii
                        ti1 = tij

                        Threading.Interlocked.Increment(bucketB(BUCKET_B(ti, ti1)))
                        Threading.Interlocked.Decrement(i)
                    End While
                End If
            End While

            m = n - m

            If m = 0 Then
                i = 0

                While i < n
                    SA(i) = i
                    Threading.Interlocked.Increment(i)
                End While

                Return 0
            End If

            c0 = 0
            i = -1
            j = 0

            While c0 < 256
                t = i + bucketA(c0)
                bucketA(c0) = i + j
                i = t + bucketB(BUCKET_B(c0, c0))
                c1 = c0 + 1

                While c1 < 256
                    j += bucketB(BUCKET_BSTAR(c0, c1))
                    bucketB(c0 << 8 Or c1) = j
                    i += bucketB(BUCKET_B(c0, c1))
                    Threading.Interlocked.Increment(c1)
                End While

                Threading.Interlocked.Increment(c0)
            End While

            Dim PAb = n - m
            Dim ISAb = m
            i = m - 2

            While 0 <= i
                t = SA(PAb + i)
                c0 = Me.T(t) And &HFF
                c1 = Me.T(t + 1) And &HFF
                SA(Threading.Interlocked.Decrement(bucketB(BUCKET_BSTAR(c0, c1)))) = i
                Threading.Interlocked.Decrement(i)
            End While

            t = SA(PAb + m - 1)
            c0 = Me.T(t) And &HFF
            c1 = Me.T(t + 1) And &HFF
            SA(Threading.Interlocked.Decrement(bucketB(BUCKET_BSTAR(c0, c1)))) = m - 1
            Dim buf = SA
            Dim bufoffset = m
            Dim bufsize = n - 2 * m

            If bufsize <= 256 Then
                buf = tempbuf
                bufoffset = 0
                bufsize = 256
            End If

            c0 = 255
            j = m

            While 0 < j
                c1 = 255

                While c0 < c1
                    i = bucketB(BUCKET_BSTAR(c0, c1))

                    If 1 < j - i Then
                        subStringSort(PAb, i, j, buf, bufoffset, bufsize, 2, SA(i) = m - 1, n)
                    End If

                    j = i
                    Threading.Interlocked.Decrement(c1)
                End While

                Threading.Interlocked.Decrement(c0)
            End While

            i = m - 1

            While 0 <= i

                If 0 <= SA(i) Then
                    j = i

                    Do
                        SA(ISAb + SA(i)) = i
                    Loop While 0 <= Threading.Interlocked.Decrement(i) AndAlso 0 <= SA(i)

                    SA(i + 1) = i - j

                    If i <= 0 Then
                        Exit While
                    End If
                End If

                j = i

                Do
                    SA(i) = Not SA(i)
                    SA(ISAb + SA(i)) = j
                Loop While SA(Threading.Interlocked.Decrement(i)) < 0

                SA(ISAb + SA(i)) = j
                Threading.Interlocked.Decrement(i)
            End While

            trSort(ISAb, m, 1)
            i = n - 1
            j = m

            If (Me.T(i) And &HFF) < (Me.T(0) And &HFF) OrElse Me.T(i) = Me.T(0) AndAlso flag <> 0 Then
                If flag = 0 Then
                    SA(SA(ISAb + Threading.Interlocked.Decrement(j))) = i
                End If

                Threading.Interlocked.Decrement(i)

                While 0 <= i AndAlso (Me.T(i) And &HFF) <= (Me.T(i + 1) And &HFF)
                    Threading.Interlocked.Decrement(i)
                End While
            End If

            While 0 <= i
                Threading.Interlocked.Decrement(i)

                While 0 <= i AndAlso (Me.T(i) And &HFF) >= (Me.T(i + 1) And &HFF)
                    Threading.Interlocked.Decrement(i)
                End While

                If 0 <= i Then
                    SA(SA(ISAb + Threading.Interlocked.Decrement(j))) = i
                    Threading.Interlocked.Decrement(i)

                    While 0 <= i AndAlso (Me.T(i) And &HFF) <= (Me.T(i + 1) And &HFF)
                        Threading.Interlocked.Decrement(i)
                    End While
                End If
            End While

            c0 = 255
            i = n - 1
            k = m - 1

            While 0 <= c0
                c1 = 255

                While c0 < c1
                    t = i - bucketB(BUCKET_B(c0, c1))
                    bucketB(BUCKET_B(c0, c1)) = i + 1
                    i = t
                    j = bucketB(BUCKET_BSTAR(c0, c1))

                    While j <= k
                        SA(i) = SA(k)
                        Threading.Interlocked.Decrement(i)
                        Threading.Interlocked.Decrement(k)
                    End While

                    Threading.Interlocked.Decrement(c1)
                End While

                t = i - bucketB(BUCKET_B(c0, c0))
                bucketB(BUCKET_B(c0, c0)) = i + 1

                If c0 < 255 Then
                    bucketB(BUCKET_BSTAR(c0, c0 + 1)) = t + 1
                End If

                i = bucketA(c0)
                Threading.Interlocked.Decrement(c0)
            End While

            Return m
        End Function

        Private Function constructBWT(bucketA As Integer(), bucketB As Integer()) As Integer
            Dim i As Integer
            Dim t = 0
            Dim s, s1 As Integer
            Dim c0, c1 As Integer, c2 = 0
            Dim orig = -1
            c1 = 254

            While 0 <= c1
                Dim j As Integer
                i = bucketB(BUCKET_BSTAR(c1, c1 + 1))
                j = bucketA(c1 + 1)
                t = 0
                c2 = -1

                While i <= j
                    s = SA(j)
                    s1 = s

                    If 0 <= s1 Then
                        If Threading.Interlocked.Decrement(s) < 0 Then s = n - 1

                        c0 = Me.T(s) And &HFF

                        If c0 <= c1 Then
                            SA(j) = Not s1

                            If 0 < s AndAlso (Me.T(s - 1) And &HFF) > c0 Then
                                s = Not s
                            End If

                            If c2 = c0 Then
                                SA(Threading.Interlocked.Decrement(t)) = s
                            Else
                                If 0 <= c2 Then bucketB(BUCKET_B(c2, c1)) = t

                                c2 = c0
                                t = bucketB(BZip2DivSufSort.BUCKET_B(c2, c1)) - 1
                                SA(t) = s
                            End If
                        End If
                    Else
                        SA(j) = Not s
                    End If

                    Threading.Interlocked.Decrement(j)
                End While

                Threading.Interlocked.Decrement(c1)
            End While

            i = 0

            While i < n
                s = SA(i)
                s1 = s

                If 0 <= s1 Then
                    If Threading.Interlocked.Decrement(s) < 0 Then s = n - 1

                    c0 = Me.T(s) And &HFF

                    If c0 >= (Me.T(s + 1) And &HFF) Then
                        If 0 < s AndAlso (Me.T(s - 1) And &HFF) < c0 Then s = Not s

                        If c0 = c2 Then
                            SA(Threading.Interlocked.Increment(t)) = s
                        Else
                            If c2 <> -1 Then bucketA(c2) = t ' BUGFIX: Original code can write to bucketA[-1]

                            c2 = c0
                            t = bucketA(c2) + 1
                            SA(t) = s
                        End If
                    End If
                Else
                    s1 = Not s1
                End If

                If s1 = 0 Then
                    SA(i) = Me.T(n - 1)
                    orig = i
                Else
                    SA(i) = Me.T(s1 - 1)
                End If

                Threading.Interlocked.Increment(i)
            End While

            Return orig
        End Function
#End Region
    End Class
End Namespace
