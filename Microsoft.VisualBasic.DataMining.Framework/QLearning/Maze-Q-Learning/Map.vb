Imports Microsoft.VisualBasic.DataMining.Framework.QLearning

Public Class Map : Inherits QState(Of Char())

    Public Overrides Function GetNextState(action As Integer) As Char()
        Dim nextMap() As Char = Me.Current
        ' get location of '@'
        Dim avatarIndex As Integer = Array.IndexOf(nextMap, "@"c)
        If avatarIndex = -1 Then
            Return nextMap ' no effect
        End If

        Dim nextAvatarIndex As Integer = __getNextAvatarIndex(action, avatarIndex)
        If nextAvatarIndex >= 0 AndAlso nextAvatarIndex < nextMap.Length Then
            If nextMap(nextAvatarIndex) <> "#"c Then
                ' change the map
                nextMap(avatarIndex) = " "c
                nextMap(nextAvatarIndex) = "@"c
            End If
        End If
        Return nextMap
    End Function

    Private Function __getNextAvatarIndex(action As Integer, currentAvatarIndex As Integer) As Integer
        Dim x As Integer = currentAvatarIndex Mod 3
        Dim y As Integer = currentAvatarIndex \ 3
        If action = Maze.UP Then
            y -= 1
        End If
        If action = Maze.RIGHT Then
            x += 1
        ElseIf action = Maze.DOWN Then
            y += 1
        ElseIf action = Maze.Left Then
            x -= 1
        End If
        If x < 0 OrElse y < 0 OrElse x >= 3 OrElse y >= 3 Then
            Return currentAvatarIndex ' no move
        End If
        Return x + 3 * y
    End Function
End Class
