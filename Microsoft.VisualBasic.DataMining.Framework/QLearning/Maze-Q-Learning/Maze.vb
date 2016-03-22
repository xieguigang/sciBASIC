Imports Microsoft.VisualBasic.DataMining.Framework

Public Class Maze : Inherits QLearning

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

    Public Overrides ReadOnly Property GoalReached As Boolean
        Get
            Dim goalIndex As Integer = -1
            For i As Integer = 0 To Map.Length - 1
                If Map(i) = "G"c Then
                    goalIndex = i
                End If
            Next i
            Return (goalIndex = -1)
        End Get
    End Property

    Public Overridable Sub PrintMap()
        Call PrintMap(Map)
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

    Protected Overrides Sub __run(Q As QTable, i As Integer)
        ' PRINT MAP
        PrintMap()


    End Sub
End Class
