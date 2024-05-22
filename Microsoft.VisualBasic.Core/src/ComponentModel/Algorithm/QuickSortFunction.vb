#Region "Microsoft.VisualBasic::6066079bd69ca401fc858155e337e572, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\QuickSortFunction.vb"

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

    '   Total Lines: 66
    '    Code Lines: 50 (75.76%)
    ' Comment Lines: 3 (4.55%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (19.70%)
    '     File Size: 2.11 KB


    '     Class QuickSortFunction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) QuickSort
    ' 
    '         Sub: QuickSort
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Algorithm

    ''' <summary>
    ''' 快速排序基本上被认为是相同数量级的所有排序算法中，平均性能最好的。
    ''' </summary>
    Public Class QuickSortFunction(Of K, T)

        ReadOnly compares As Comparison(Of K)

        Sub New(compares As Comparison(Of K))
            Me.compares = compares
        End Sub

        Public Function QuickSort(src As IEnumerable(Of (K, T))) As (K, T)()
            Dim input As (K, T)() = src.ToArray
            Dim ends As Integer = input.Length - 1

            If Scan0 < ends Then
                Call QuickSort(input, Scan0, ends)
            End If

            Return input
        End Function

        Public Function QuickSort(list As IEnumerable(Of T), key As Func(Of T, K)) As T()
            Dim input As (K, T)() = list.Select(Function(p) (key(p), p)).ToArray
            Dim ends As Integer = input.Length - 1

            If Scan0 < ends Then
                Call QuickSort(input, Scan0, ends)
            End If

            Return input.Select(Function(a) a.Item2).ToArray
        End Function

        Private Sub QuickSort(src As (key As K, T)(), begin As Integer, [end] As Integer)
            Dim t = src(begin)
            Dim i = begin
            Dim j = [end]

            While (i < j)
                While (i < j AndAlso compares(src(j).key, t.key) > 0)
                    j -= 1
                End While
                If (i < j) Then
                    src(i) = src(j)
                    i += 1
                End If
                While (i < j AndAlso compares(src(i).key, t.key) < 0)
                    i += 1
                End While
                If i < j Then
                    src(j) = src(i)
                    j -= 1
                End If
            End While

            src(i) = t
            j = i - 1
            i = i + 1

            If begin < j Then Call QuickSort(src, begin, j)
            If i < [end] Then Call QuickSort(src, i, [end])
        End Sub
    End Class
End Namespace
