Imports System
Imports System.Collections

''' <summary>
''' Q Learning sample class <br/>
''' <b>The goal of this code sample is for the character @ to reach the goal area G</b> <br/>
''' compile using "javac QLearning.java" <br/>
''' test using "java QLearning" <br/>
''' 
''' @author A.Liapis (Original author), A. Hartzen (2013 modifications) 
''' </summary>
Public Class QLearning
    ' --- variables
    Protected ReadOnly startingmap() As Char = {"@"c, " "c, "#"c, " "c, "#"c, "G"c, " "c, " "c, " "c}
    Protected _map() As Char

    ' --- movement constants
    Public Const UP As Integer = 0
    Public Const RIGHT As Integer = 1
    Public Const DOWN As Integer = 2
    Public Const LEFT As Integer = 3

    Public Sub New()
        resetMaze()
    End Sub

    Public Overridable ReadOnly Property Map As Char()
        Get
            Return CType(_map.Clone(), Char())
        End Get
    End Property

    Public Overridable Sub resetMaze()
        _map = CType(startingmap.Clone(), Char())
    End Sub

    Public Overridable ReadOnly Property ActionRange As Integer
        Get
            Return 4
        End Get
    End Property

    ''' <summary>
    ''' Returns the map state which results from an initial map state after an
    ''' action is applied. In case the action is invalid, the returned map is the
    ''' same as the initial one (no move). </summary>
    ''' <param name="action"> taken by the avatar ('@') </param>
    ''' <param name="map"> map before the action is taken </param>
    ''' <returns> resulting map after the action is taken </returns>
    Public Overridable Function getNextState(action As Integer, map() As Char) As Char()
        Dim nextMap() As Char = CType(map.Clone(), Char())
        ' get location of '@'
        Dim avatarIndex As Integer = Array.IndexOf(map, "@"c)
        If avatarIndex = -1 Then
            Return nextMap ' no effect
        End If

        Dim nextAvatarIndex As Integer = __getNextAvatarIndex(action, avatarIndex)
        If nextAvatarIndex >= 0 AndAlso nextAvatarIndex < map.Length Then
            If nextMap(nextAvatarIndex) <> "#"c Then
                ' change the map
                nextMap(avatarIndex) = " "c
                nextMap(nextAvatarIndex) = "@"c
            End If
        End If
        Return nextMap
    End Function

    Private Function __getNextState(action As Integer) As Char()
        Dim nextMap() As Char = CType(Map.Clone(), Char())
        ' get location of '@'
        Dim avatarIndex As Integer = Array.IndexOf(Map, "@"c)
        If avatarIndex = -1 Then
            Return nextMap ' no effect
        End If

        Dim nextAvatarIndex As Integer = __getNextAvatarIndex(action, avatarIndex)
        If nextAvatarIndex >= 0 AndAlso nextAvatarIndex < Map.Length Then
            If nextMap(nextAvatarIndex) <> "#"c Then
                ' change the map
                nextMap(avatarIndex) = " "c
                nextMap(nextAvatarIndex) = "@"c
            End If
        End If
        Return nextMap
    End Function

    Private Sub __goToNextState(action As Integer)
        _map = __getNextState(action)
    End Sub

    Public Overridable Function isValidMove(action As Integer) As Boolean
        Dim nextMap() As Char = __getNextState(action)
        Return (nextMap = Map)
    End Function

    Public Overridable Function isValidMove(action As Integer, map() As Char) As Boolean
        Dim nextMap() As Char = getNextState(action, map)
        Return (nextMap = map)
    End Function

    Public Overridable ReadOnly Property AvatarIndex As Integer
        Get
            Return Array.IndexOf(Map, "@"c)
        End Get
    End Property

    Public Overridable ReadOnly Property GoalReached As Boolean
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

    Public Overridable Function isGoalReached(map() As Char) As Boolean
        Dim goalIndex As Integer = -1
        For i As Integer = 0 To map.Length - 1
            If map(i) = "G"c Then
                goalIndex = i
            End If
        Next i
        Return (goalIndex = -1)
    End Function

    Private Function __getNextAvatarIndex(action As Integer, currentAvatarIndex As Integer) As Integer
        Dim x As Integer = currentAvatarIndex Mod 3
        Dim y As Integer = currentAvatarIndex \ 3
        If action = UP Then
            y -= 1
        End If
        If action = RIGHT Then
            x += 1
        ElseIf action = DOWN Then
            y += 1
        ElseIf action = LEFT Then
            x -= 1
        End If
        If x < 0 OrElse y < 0 OrElse x >= 3 OrElse y >= 3 Then
            Return currentAvatarIndex ' no move
        End If
        Return x + 3 * y
    End Function

    Public Overridable Sub PrintMap()
        Call PrintMap(Map)
    End Sub

    Public Shared Sub PrintMap(map() As Char)
        For i As Integer = 0 To map.Length - 1
            If i Mod 3 = 0 Then
                Console.WriteLine("+-+-+-+")
            End If
            Console.Write("|" & AscW(map(i)))
            If i Mod 3 = 2 Then
                Console.WriteLine("|")
            End If
        Next i
        Console.WriteLine("+-+-+-+")
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

    Public Overridable Sub runLearningLoop()
        Dim q As New QTable(ActionRange)
        Dim moveCounter As Integer = 0

        Dim count As Integer = 0
        Do While count < 100
            ' PRINT MAP
            PrintMap()
            ' CHECK IF WON, THEN RESET
            If GoalReached Then
                Console.WriteLine("GOAL REACHED IN " & moveCounter & " MOVES!")
                resetMaze()
                moveCounter = 0
                count += 1
                '                q.updateQvalue(100, map);
                '                return;
            End If

            ' DETERMINE ACTION
            Dim action As Integer = q.NextAction(Map)
            __goToNextState(action)
            moveCounter += 1


            ' REWARDS AND ADJUSTMENT OF WEIGHTS SHOULD TAKE PLACE HERE
            If GoalReached Then
                q.UpdateQvalue(1, Map)
            Else
                q.UpdateQvalue(-100, Map)
            End If


            ' COMMENT THE SLEEP FUNCTION IF YOU NEED FAST TRAINING WITHOUT
            ' NEEDING TO ACTUALLY SEE IT PROGRESS
            'Thread.sleep(1000);
        Loop
    End Sub


    ''' <summary>
    ''' Q-learning maze-solving testing method </summary>
    Public Shared Sub Main(s() As String)
        Dim app As New QLearning()
        Try
            app.runLearningLoop()
        Catch e As Exception
            Console.WriteLine("Thread.sleep interrupted!")
        End Try
    End Sub
End Class
