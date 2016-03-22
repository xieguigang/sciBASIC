Imports Microsoft.VisualBasic.DocumentFormat.Csv


Module Program




    Sub Main()

        Call {New table, New table, New table, New table}.SaveTo("x:\fff_test.csv")

        Dim maze As Maze = New Maze
        Call maze.RunLearningLoop(200)
    End Sub
End Module


Public Class table
    Public Property x As Integer
End Class