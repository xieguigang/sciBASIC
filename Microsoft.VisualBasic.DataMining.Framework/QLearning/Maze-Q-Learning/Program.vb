Module Program

    Sub Main()
        Dim maze As Maze = New Maze
        Call maze.RunLearningLoop(100, Function(n) New QTable(n))
    End Sub
End Module
