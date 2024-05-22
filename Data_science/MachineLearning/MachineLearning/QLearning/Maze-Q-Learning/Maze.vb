#Region "Microsoft.VisualBasic::2ffd1694f7b3bd50ce7b3cb054a35f52, Data_science\MachineLearning\MachineLearning\QLearning\Maze-Q-Learning\Maze.vb"

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

    '   Total Lines: 130
    '    Code Lines: 103 (79.23%)
    ' Comment Lines: 3 (2.31%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 24 (18.46%)
    '     File Size: 3.76 KB


    ' Class Maze
    ' 
    '     Properties: ActionRange, AvatarIndex, GoalReached
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: __getMoveName, isValidMove, PrintMap
    ' 
    '     Sub: __finishLearn, __goToNextState, __init, __reset, __run
    '          PrintMap, resetMaze
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.QLearning
Imports Microsoft.VisualBasic.MachineLearning.QLearning.DataModel

Public Class Maze : Inherits QLearning(Of Char())

    ' --- movement constants
    Public Const UP As Integer = 0
    Public Const RIGHT As Integer = 1
    Public Const DOWN As Integer = 2
    Public Const LEFT As Integer = 3

    Public Overrides ReadOnly Property ActionRange As Integer
        Get
            Return 4
        End Get
    End Property

    Public Overridable ReadOnly Property AvatarIndex As Integer
        Get
            Return Array.IndexOf(_stat.Current, "@"c)
        End Get
    End Property

    Public Overrides ReadOnly Property GoalReached As Boolean
        Get
            Dim map As Char() = _stat.Current
            Dim goalIndex As Integer = -1
            For i As Integer = 0 To map.Length - 1
                If map(i) = "G"c Then
                    goalIndex = i
                End If
            Next i
            Return (goalIndex = -1)
        End Get
    End Property

    Sub New()
        Call MyBase.New(New Map, Function(n) New QTable(n))
    End Sub

    Public Overridable Sub resetMaze()
        Call _stat.SetState({"@"c, " "c, "#"c, " "c, "#"c, "G"c, " "c, " "c, " "c}) 'startingmap
    End Sub

    Public Overridable Function isValidMove(action As Integer) As Boolean
        Dim nextMap() As Char = _stat.GetNextState(action)
        Return (nextMap = _stat.Current)
    End Function

    Public Overridable Sub PrintMap()
        Call sb.WriteLine(PrintMap(_stat.Current))
    End Sub

    Public Shared Function PrintMap(map() As Char) As String
        Dim sb As StringBuilder = New StringBuilder

        For i As Integer = 0 To map.Length - 1
            If i Mod 3 = 0 Then
                sb.AppendLine("+-+-+-+")
            End If
            sb.Append("|" & map(i))
            If i Mod 3 = 2 Then
                sb.AppendLine("|")
            End If
        Next i
        sb.AppendLine("+-+-+-+")
        sb.AppendLine()

        Return sb.ToString
    End Function

    Public Overridable Function __getMoveName(action As Integer) As String
        Dim result As String = "ERROR"
        If action = UP Then
            result = "UP"
        ElseIf action = RIGHT Then
            result = "RIGHT"
        ElseIf action = DOWN Then
            result = "DOWN"
        ElseIf action = LEFT Then
            result = "LEFT"
        End If
        Return result
    End Function

    Private Sub __goToNextState(action As Integer)
        Call _stat.SetState(_stat.GetNextState(action))
    End Sub

    Dim sb As StreamWriter

    Protected Overrides Sub __run(i As Integer)
        ' PRINT MAP
        PrintMap()

        ' DETERMINE ACTION
        Dim action As Integer = Q.NextAction(_stat.Current)
        __goToNextState(action)
        moveCounter += 1
    End Sub

    Dim moveCounter As Integer = 0

    Dim dump As New QTableDump

    Protected Overrides Sub __reset(i As Integer)
        If Not sb Is Nothing Then
            Call sb.Flush()
            Call sb.Close()
        End If

        Call dump.Save(App.HOME & "/results/QTable.Csv")
        sb = New StreamWriter(New FileStream(App.HOME & $"/results/maze_{i}.txt", FileMode.OpenOrCreate))
        sb.WriteLine("GOAL REACHED IN " & moveCounter & " MOVES!")
        resetMaze()
        moveCounter = 0

        Call dump.Dump(Q, i)
    End Sub

    Protected Overrides Sub __init()
        resetMaze()
    End Sub

    Protected Overrides Sub __finishLearn()
        Call dump.Save(App.HOME & "/QTable.Csv")
    End Sub
End Class
