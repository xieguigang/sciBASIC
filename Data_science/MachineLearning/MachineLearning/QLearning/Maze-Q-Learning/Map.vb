#Region "Microsoft.VisualBasic::fd0f8df9c96ccee2b02bdf2eae38016e, Data_science\MachineLearning\MachineLearning\QLearning\Maze-Q-Learning\Map.vb"

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

    '   Total Lines: 42
    '    Code Lines: 36
    ' Comment Lines: 2
    '   Blank Lines: 4
    '     File Size: 1.49 KB


    ' Class Map
    ' 
    '     Function: __getNextAvatarIndex, GetNextState
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.QLearning

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
