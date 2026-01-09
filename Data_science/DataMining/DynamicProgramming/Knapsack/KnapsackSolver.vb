#Region "Microsoft.VisualBasic::ed7ca8761ecff331969643e5a858332d, Data_science\DataMining\DynamicProgramming\Knapsack\KnapsackSolver.vb"

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

    '   Total Lines: 73
    '    Code Lines: 57 (78.08%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 16 (21.92%)
    '     File Size: 2.42 KB


    '     Class KnapsackSolver
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: FillTable, GetValue, GetWeight, Solve, TakeItems
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Knapsack

    Public Class KnapsackSolver

        Dim table As Double(,)
        Dim items As Item()
        Dim capacity As Integer

        Sub New(items As IEnumerable(Of Item), capacity As Integer)
            Me.items = items.ToArray
            Me.capacity = capacity
            Me.table = New Double(Me.items.Length + 1 - 1, capacity + 1 - 1) {}
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetWeight(items As IList(Of Item)) As Double
            Return Aggregate i As Item In items Into Sum(i.Weight)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetValue(items As IList(Of Item)) As Double
            Return Aggregate i As Item In items Into Sum(i.Value)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Solve() As KnapsackSolution
            Return FillTable().TakeItems()
        End Function

        Private Function TakeItems() As KnapsackSolution
            Dim row As Integer = items.Length
            Dim col As Integer = capacity
            Dim best As New KnapsackSolution() With {
                .Items = New List(Of Item)()
            }

            While row > 0
                If table(row, col) <> table(row - 1, col) Then
                    best.Items.Add(items(row - 1))
                    col -= items(row - 1).Weight
                End If

                row -= 1
            End While

            best.TotalWeight = GetWeight(best.Items)
            best.Value = GetValue(best.Items)

            Return best
        End Function

        Private Function FillTable() As KnapsackSolver
            Dim item As Item

            For row As Integer = 1 To items.Length
                item = items(row - 1)

                For col As Integer = 0 To capacity
                    If item.Weight > col Then
                        table(row, col) = table(row - 1, col)
                    Else
                        table(row, col) = std.Max(table(row - 1, col), table(row - 1, col - item.Weight) + item.Value)
                    End If
                Next
            Next

            Return Me
        End Function
    End Class
End Namespace
