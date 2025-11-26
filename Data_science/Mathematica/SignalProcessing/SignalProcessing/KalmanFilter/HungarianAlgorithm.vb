#Region "Microsoft.VisualBasic::ab829ccb1fe7147749c023e57542bc29, Data_science\Mathematica\SignalProcessing\SignalProcessing\KalmanFilter\HungarianAlgorithm.vb"

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

    '   Total Lines: 334
    '    Code Lines: 250 (74.85%)
    ' Comment Lines: 24 (7.19%)
    '    - Xml Docs: 95.83%
    ' 
    '   Blank Lines: 60 (17.96%)
    '     File Size: 12.64 KB


    '     Module HungarianAlgorithm
    ' 
    '         Function: (+2 Overloads) FindAssignments, FindMinimum, FindPrimeInRow, FindStarInColumn, FindStarInRow
    '                   FindZero, RunStep1, RunStep2, RunStep3, RunStep4
    ' 
    '         Sub: ClearCovers, ClearPrimes, ConvertPath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace HungarianAlgorithm

    ''' <summary>
    ''' Hungarian Algorithm.
    ''' </summary>
    Public Module HungarianAlgorithm

        ''' <summary>
        ''' Finds the optimal assignments for a given matrix of agents and 
        ''' costed tasks such that the total cost is minimized.
        ''' </summary>
        ''' <param name="costs">A cost matrix; the element at row <em>i</em> 
        ''' and column <em>j</em> represents the cost of agent <em>i</em> 
        ''' performing task <em>j</em>.</param>
        ''' <returns>A matrix of assignments; the value of element <em>i</em> 
        ''' is the column of the task assigned to agent <em>i</em>.</returns>
        ''' <exception cref="ArgumentNullException"><paramref name="costs"/> is null.</exception>
        <Extension()>
        Public Function FindAssignments(costs As NumericMatrix) As Integer()
            Return FindAssignments(NumericMatrix.GetRectangularArray(costs))
        End Function

        ''' <summary>
        ''' Finds the optimal assignments for a given matrix of agents and 
        ''' costed tasks such that the total cost is minimized.
        ''' </summary>
        ''' <param name="costs">A cost matrix; the element at row <em>i</em> 
        ''' and column <em>j</em> represents the cost of agent <em>i</em> 
        ''' performing task <em>j</em>.</param>
        ''' <returns>A matrix of assignments; the value of element <em>i</em> 
        ''' is the column of the task assigned to agent <em>i</em>.</returns>
        ''' <exception cref="ArgumentNullException"><paramref name="costs"/> is null.</exception>
        <Extension()>
        Public Function FindAssignments(costs As Double()()) As Integer()
            Dim h = costs.Length
            Dim w = costs(0).Length
            Dim rowsGreaterThanCols = h > w

            If rowsGreaterThanCols Then
                ' make sure cost matrix has number of rows greater than columns
                Dim row = w
                Dim col = h
                Dim transposeCosts As Double()() = RectangularArray.Matrix(Of Double)(row, col)
                For i As Integer = 0 To row - 1
                    For j As Integer = 0 To col - 1
                        transposeCosts(i)(j) = costs(j)(i)
                    Next
                Next
                costs = transposeCosts
                h = row
                w = col
            End If

            For i As Integer = 0 To h - 1
                Dim min As Double = Double.MaxValue

                For j As Integer = 0 To w - 1
                    min = std.Min(min, costs(i)(j))
                Next

                For j As Integer = 0 To w - 1
                    costs(i)(j) -= min
                Next
            Next

            Dim masks As Byte()() = RectangularArray.Matrix(Of Byte)(h, w)
            Dim rowsCovered = New Boolean(h - 1) {}
            Dim colsCovered = New Boolean(w - 1) {}

            For i As Integer = 0 To h - 1
                For j As Integer = 0 To w - 1
                    If costs(i)(j) = 0.0 AndAlso Not rowsCovered(i) AndAlso Not colsCovered(j) Then
                        masks(i)(j) = 1
                        rowsCovered(i) = True
                        colsCovered(j) = True
                    End If
                Next
            Next

            Call ClearCovers(rowsCovered, colsCovered, w, h)

            Dim path = New(row As Integer, column As Integer)(w * h - 1) {}
            Dim pathStart As (row As Integer, column As Integer) = Nothing
            Dim [step] = 1

            While [step] <> -1
                Select Case [step]
                    Case 1
                        [step] = RunStep1(masks, colsCovered, w, h)
                    Case 2
                        [step] = RunStep2(costs, masks, rowsCovered, colsCovered, w, h, pathStart)
                    Case 3
                        [step] = RunStep3(masks, rowsCovered, colsCovered, w, h, path, pathStart)
                    Case 4
                        [step] = RunStep4(costs, rowsCovered, colsCovered, w, h)
                    Case Else
                End Select
            End While

            Dim agentsTasks = New Integer(h - 1) {}

            For i As Integer = 0 To h - 1
                For j As Integer = 0 To w - 1
                    If masks(i)(j) = 1 Then
                        agentsTasks(i) = j
                        Exit For
                    Else
                        agentsTasks(i) = -1
                    End If
                Next
            Next

            If rowsGreaterThanCols Then
                Dim agentsTasksTranspose = New Integer(w - 1) {}
                For i As Integer = 0 To w - 1
                    agentsTasksTranspose(i) = -1
                Next

                For j As Integer = 0 To h - 1
                    agentsTasksTranspose(agentsTasks(j)) = j
                Next
                agentsTasks = agentsTasksTranspose
            End If

            Return agentsTasks
        End Function

        Private Function RunStep1(ByRef masks As Byte()(), ByRef colsCovered As Boolean(), w As Integer, h As Integer) As Integer
            For i As Integer = 0 To h - 1
                For j As Integer = 0 To w - 1
                    If masks(i)(j) = 1 Then colsCovered(j) = True
                Next
            Next

            Dim colsCoveredCount = 0

            For j As Integer = 0 To w - 1
                If colsCovered(j) Then colsCoveredCount += 1
            Next

            If colsCoveredCount = std.Min(w, h) Then Return -1

            Return 2
        End Function

        Private Function RunStep2(ByRef costs As Double()(),
                                  ByRef masks As Byte()(),
                                  ByRef rowsCovered As Boolean(),
                                  ByRef colsCovered As Boolean(),
                                  w As Integer, h As Integer,
                                  ByRef pathStart As (row As Integer, column As Integer)) As Integer
            While True
                Dim loc = FindZero(costs, rowsCovered, colsCovered, w, h)

                If loc.row = -1 Then
                    Return 4
                Else
                    masks(loc.row)(loc.column) = 2
                End If

                Dim starCol = FindStarInRow(masks, w, loc.row)

                If starCol <> -1 Then
                    rowsCovered(loc.row) = True
                    colsCovered(starCol) = False
                Else
                    pathStart = loc
                    Return 3
                End If
            End While

            Throw New Exception("never!")
        End Function

        Private Function RunStep3(ByRef masks As Byte()(),
                                  ByRef rowsCovered As Boolean(),
                                  ByRef colsCovered As Boolean(),
                                  w As Integer, h As Integer,
                                  path As (row As Integer, column As Integer)(),
                                  pathStart As (row As Integer, column As Integer)) As Integer

            Dim pathIndex As Integer = 0
            path(0) = pathStart

            While True
                Dim row = FindStarInColumn(masks, h, path(pathIndex).column)
                If row = -1 Then Exit While

                pathIndex += 1
                path(pathIndex) = (row, path(pathIndex - 1).column)

                Dim col = FindPrimeInRow(masks, w, path(pathIndex).row)

                pathIndex += 1
                path(pathIndex) = (path(pathIndex - 1).row, col)
            End While

            ConvertPath(masks, path, pathIndex + 1)
            ClearCovers(rowsCovered, colsCovered, w, h)
            ClearPrimes(masks, w, h)

            Return 1
        End Function

        Private Function RunStep4(ByRef costs As Double()(),
                                  ByRef rowsCovered As Boolean(),
                                  ByRef colsCovered As Boolean(),
                                  w As Integer, h As Integer) As Integer

            Dim minValue As Double = FindMinimum(costs, rowsCovered, colsCovered, w, h)

            For i As Integer = 0 To h - 1
                For j As Integer = 0 To w - 1
                    If rowsCovered(i) Then
                        costs(i)(j) += minValue
                    End If
                    If Not colsCovered(j) Then
                        costs(i)(j) -= minValue
                    End If
                Next
            Next

            Return 2
        End Function

        Private Function FindMinimum(ByRef costs As Double()(),
                                     ByRef rowsCovered As Boolean(),
                                     ByRef colsCovered As Boolean(),
                                     w As Integer, h As Integer) As Double

            Dim minValue = Double.MaxValue

            For i As Integer = 0 To h - 1
                For j As Integer = 0 To w - 1
                    If Not rowsCovered(i) AndAlso Not colsCovered(j) Then
                        minValue = std.Min(minValue, costs(i)(j))
                    End If
                Next
            Next

            Return minValue
        End Function

        Private Function FindStarInRow(ByRef masks As Byte()(), w As Integer, row As Integer) As Integer
            For j As Integer = 0 To w - 1
                If masks(row)(j) = 1 Then
                    Return j
                End If
            Next

            Return -1
        End Function

        Private Function FindStarInColumn(ByRef masks As Byte()(), h As Integer, col As Integer) As Integer
            For i As Integer = 0 To h - 1
                If masks(i)(col) = 1 Then
                    Return i
                End If
            Next

            Return -1
        End Function

        Private Function FindPrimeInRow(ByRef masks As Byte()(), w As Integer, row As Integer) As Integer
            For j As Integer = 0 To w - 1
                If masks(row)(j) = 2 Then
                    Return j
                End If
            Next

            Return -1
        End Function

        Private Function FindZero(ByRef costs As Double()(),
                                  ByRef rowsCovered As Boolean(),
                                  ByRef colsCovered As Boolean(), w As Integer, h As Integer) As (row As Integer, column As Integer)

            For i As Integer = 0 To h - 1
                For j As Integer = 0 To w - 1
                    If costs(i)(j) = 0.0 AndAlso Not rowsCovered(i) AndAlso Not colsCovered(j) Then
                        Return (i, j)
                    End If
                Next
            Next

            Return (-1, -1)
        End Function

        Private Sub ConvertPath(ByRef masks As Byte()(),
                                ByRef path As (row As Integer, column As Integer)(),
                                pathLength As Integer)

            For i As Integer = 0 To pathLength - 1
                Dim x = masks(path(i).row)(path(i).column)

                Select Case x
                    Case 1
                        x = 0
                    Case 2
                        x = 1
                    Case Else
                        x = masks(path(i).row)(path(i).column)
                End Select

                masks(path(i).row)(path(i).column) = x
            Next
        End Sub

        Private Sub ClearPrimes(ByRef masks As Byte()(), w As Integer, h As Integer)
            For i As Integer = 0 To h - 1
                For j As Integer = 0 To w - 1
                    If masks(i)(j) = 2 Then
                        masks(i)(j) = 0
                    End If
                Next
            Next
        End Sub

        Private Sub ClearCovers(ByRef rowsCovered As Boolean(), ByRef colsCovered As Boolean(), w As Integer, h As Integer)
            For i As Integer = 0 To h - 1
                rowsCovered(i) = False
            Next

            For j As Integer = 0 To w - 1
                colsCovered(j) = False
            Next
        End Sub
    End Module
End Namespace
