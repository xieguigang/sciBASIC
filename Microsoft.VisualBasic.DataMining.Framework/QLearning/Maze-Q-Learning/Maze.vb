Imports Microsoft.VisualBasic.DataMining.Framework
Imports Microsoft.VisualBasic.DataMining.Framework.QLearning

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
            For i As Integer = 0 To Map.Length - 1
                If Map(i) = "G"c Then
                    goalIndex = i
                End If
            Next i
            Return (goalIndex = -1)
        End Get
    End Property

    ' --- variables
    Protected ReadOnly startingmap() As Char = {"@"c, " "c, "#"c, " "c, "#"c, "G"c, " "c, " "c, " "c}

    Public Overridable Sub resetMaze()
        Call _stat.SetState(DirectCast(startingmap.Clone, Char()))
    End Sub

    Public Overridable Function isValidMove(action As Integer, map() As Char) As Boolean
        Dim nextMap() As Char = getNextState(action, map)
        Return (nextMap = map)
    End Function

    Public Overridable Function isValidMove(action As Integer) As Boolean
        Dim nextMap() As Char = __getNextState(action)
        Return (nextMap = Map)
    End Function

    Public Overridable Sub PrintMap()
        Call PrintMap(_stat.Current)
    End Sub

    Public Shared Sub PrintMap(map() As Char)
        For i As Integer = 0 To map.Length - 1
            If i Mod 3 = 0 Then
                Console.WriteLine("+-+-+-+")
            End If
            Console.Write("|" & map(i))
            If i Mod 3 = 2 Then
                Console.WriteLine("|")
            End If
        Next i
        Console.WriteLine("+-+-+-+")
        Console.WriteLine()
    End Sub

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
        _map = __getNextState(action)
    End Sub

    Protected Overrides Sub __run(Q As QTable, i As Integer)
        ' PRINT MAP
        PrintMap()

        ' DETERMINE ACTION
        Dim action As Integer = Q.NextAction(_stat.Current)
        __goToNextState(action)
        moveCounter += 1
    End Sub

    Dim moveCounter As Integer = 0

    Protected Overrides Sub __reset()
        Console.WriteLine("GOAL REACHED IN " & moveCounter & " MOVES!")
        resetMaze()
        moveCounter = 0
    End Sub

    Protected Overrides Sub __init()
        resetMaze()
    End Sub
End Class
