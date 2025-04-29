#Region "Microsoft.VisualBasic::97641b63f12ae2751ec21508fb09a9a2, Data_science\Mathematica\SignalProcessing\SignalProcessing\KalmanFilter\HungarianAlgorithm.vb"

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

    '   Total Lines: 279
    '    Code Lines: 213 (76.34%)
    ' Comment Lines: 14 (5.02%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 52 (18.64%)
    '     File Size: 10.10 KB


    '     Module HungarianAlgorithm
    ' 
    '         Function: FindAssignments, FindMinimum, FindPrimeInRow, FindStarInColumn, FindStarInRow
    '                   FindZero, RunStep1, RunStep2, RunStep3, RunStep4
    ' 
    '         Sub: ClearCovers, ClearPrimes, ConvertPath
    '         Structure Location
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
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
        Public Function FindAssignments(costs As Double(,)) As Integer()
            Dim h = costs.GetLength(0)
            Dim w = costs.GetLength(1)
            Dim rowsGreaterThanCols = h > w

            If rowsGreaterThanCols Then
                ' make sure cost matrix has number of rows greater than columns
                Dim row = w
                Dim col = h
                Dim transposeCosts = New Double(row - 1, col - 1) {}
                For i = 0 To row - 1
                    For j = 0 To col - 1
                        transposeCosts(i, j) = costs(j, i)
                    Next
                Next
                costs = transposeCosts
                h = row
                w = col
            End If

            For i = 0 To h - 1
                Dim min As Double = Double.MaxValue

                For j = 0 To w - 1
                    min = std.Min(min, costs(i, j))
                Next

                For j = 0 To w - 1
                    costs(i, j) -= min
                Next
            Next

            Dim masks = New Byte(h - 1, w - 1) {}
            Dim rowsCovered = New Boolean(h - 1) {}
            Dim colsCovered = New Boolean(w - 1) {}

            For i = 0 To h - 1
                For j = 0 To w - 1
                    If costs(i, j) = 0.0 AndAlso Not rowsCovered(i) AndAlso Not colsCovered(j) Then
                        masks(i, j) = 1
                        rowsCovered(i) = True
                        colsCovered(j) = True
                    End If
                Next
            Next

            Call ClearCovers(rowsCovered, colsCovered, w, h)

            Dim path = New Location(w * h - 1) {}
            Dim pathStart As Location = Nothing
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

            For i = 0 To h - 1
                For j = 0 To w - 1
                    If masks(i, j) = 1 Then
                        agentsTasks(i) = j
                        Exit For
                    Else
                        agentsTasks(i) = -1
                    End If
                Next
            Next

            If rowsGreaterThanCols Then
                Dim agentsTasksTranspose = New Integer(w - 1) {}
                For i = 0 To w - 1
                    agentsTasksTranspose(i) = -1
                Next

                For j = 0 To h - 1
                    agentsTasksTranspose(agentsTasks(j)) = j
                Next
                agentsTasks = agentsTasksTranspose
            End If

            Return agentsTasks
        End Function

        Private Function RunStep1(masks As Byte(,), colsCovered As Boolean(), w As Integer, h As Integer) As Integer
            For i = 0 To h - 1
                For j = 0 To w - 1
                    If masks(i, j) = 1 Then colsCovered(j) = True
                Next
            Next

            Dim colsCoveredCount = 0

            For j = 0 To w - 1
                If colsCovered(j) Then colsCoveredCount += 1
            Next

            If colsCoveredCount = std.Min(w, h) Then Return -1

            Return 2
        End Function

        Private Function RunStep2(costs As Double(,), masks As Byte(,), rowsCovered As Boolean(), colsCovered As Boolean(), w As Integer, h As Integer, ByRef pathStart As Location) As Integer
            While True
                Dim loc = FindZero(costs, rowsCovered, colsCovered, w, h)
                If loc.row = -1 Then Return 4

                masks(loc.row, loc.column) = 2

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
        Private Function RunStep3(masks As Byte(,), rowsCovered As Boolean(), colsCovered As Boolean(), w As Integer, h As Integer, path As Location(), pathStart As Location) As Integer
            Dim pathIndex = 0
            path(0) = pathStart

            While True
                Dim row = FindStarInColumn(masks, h, path(pathIndex).column)
                If row = -1 Then Exit While

                pathIndex += 1
                path(pathIndex) = New Location(row, path(pathIndex - 1).column)

                Dim col = FindPrimeInRow(masks, w, path(pathIndex).row)

                pathIndex += 1
                path(pathIndex) = New Location(path(pathIndex - 1).row, col)
            End While

            ConvertPath(masks, path, pathIndex + 1)
            ClearCovers(rowsCovered, colsCovered, w, h)
            ClearPrimes(masks, w, h)

            Return 1
        End Function

        Private Function RunStep4(costs As Double(,), rowsCovered As Boolean(), colsCovered As Boolean(), w As Integer, h As Integer) As Integer
            Dim minValue = FindMinimum(costs, rowsCovered, colsCovered, w, h)

            For i = 0 To h - 1
                For j = 0 To w - 1
                    If rowsCovered(i) Then costs(i, j) += minValue
                    If Not colsCovered(j) Then costs(i, j) -= minValue
                Next
            Next
            Return 2
        End Function

        Private Function FindMinimum(costs As Double(,), rowsCovered As Boolean(), colsCovered As Boolean(), w As Integer, h As Integer) As Double
            Dim minValue = Double.MaxValue

            For i = 0 To h - 1
                For j = 0 To w - 1
                    If Not rowsCovered(i) AndAlso Not colsCovered(j) Then minValue = std.Min(minValue, costs(i, j))
                Next
            Next

            Return minValue
        End Function

        Private Function FindStarInRow(masks As Byte(,), w As Integer, row As Integer) As Integer
            For j = 0 To w - 1
                If masks(row, j) = 1 Then Return j
            Next

            Return -1
        End Function

        Private Function FindStarInColumn(masks As Byte(,), h As Integer, col As Integer) As Integer
            For i = 0 To h - 1
                If masks(i, col) = 1 Then Return i
            Next

            Return -1
        End Function

        Private Function FindPrimeInRow(masks As Byte(,), w As Integer, row As Integer) As Integer
            For j = 0 To w - 1
                If masks(row, j) = 2 Then Return j
            Next

            Return -1
        End Function

        Private Function FindZero(costs As Double(,), rowsCovered As Boolean(), colsCovered As Boolean(), w As Integer, h As Integer) As Location
            For i = 0 To h - 1
                For j = 0 To w - 1
                    If costs(i, j) = 0.0 AndAlso Not rowsCovered(i) AndAlso Not colsCovered(j) Then Return New Location(i, j)
                Next
            Next

            Return New Location(-1, -1)
        End Function

        Private Sub ConvertPath(masks As Byte(,), path As Location(), pathLength As Integer)
            For i = 0 To pathLength - 1
                Dim x = masks(path(i).row, path(i).column)

                Select Case x
                    Case 1
                        x = 0
                    Case 2
                        x = 1
                    Case Else
                        x = masks(path(i).row, path(i).column)
                End Select

                masks(path(i).row, path(i).column) = x
            Next
        End Sub
        Private Sub ClearPrimes(masks As Byte(,), w As Integer, h As Integer)
            For i = 0 To h - 1
                For j = 0 To w - 1
                    If masks(i, j) = 2 Then masks(i, j) = 0
                Next
            Next
        End Sub
        Private Sub ClearCovers(rowsCovered As Boolean(), colsCovered As Boolean(), w As Integer, h As Integer)
            For i = 0 To h - 1
                rowsCovered(i) = False
            Next

            For j = 0 To w - 1
                colsCovered(j) = False
            Next
        End Sub

        Private Structure Location

            Friend ReadOnly row As Integer
            Friend ReadOnly column As Integer

            Friend Sub New(row As Integer, col As Integer)
                Me.row = row
                Me.column = col
            End Sub
        End Structure
    End Module
End Namespace

