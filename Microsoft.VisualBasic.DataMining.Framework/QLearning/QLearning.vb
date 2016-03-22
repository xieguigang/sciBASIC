Imports System
Imports System.Collections

Namespace QLearning

    ''' <summary>
    ''' Q Learning sample class <br/>
    ''' <b>The goal of this code sample is for the character @ to reach the goal area G</b> <br/>
    ''' compile using "javac QLearning.java" <br/>
    ''' test using "java QLearning" <br/>
    ''' 
    ''' @author A.Liapis (Original author), A. Hartzen (2013 modifications) 
    ''' </summary>
    Public MustInherit Class QLearning(Of T As ICloneable)

        Protected _stat As QState(Of T)

        Sub New()
            Call __init()
        End Sub

        Protected MustOverride Sub __init()

        ''' <summary>
        ''' The size of the <see cref="QTable"/>
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property ActionRange As Integer

        Public MustOverride ReadOnly Property GoalReached As Boolean

        ''' <summary>
        ''' Takes a action for the agent.
        ''' </summary>
        ''' <param name="Q"></param>
        ''' <param name="i">Iteration counts.</param>
        Protected MustOverride Sub __run(Q As QTable, i As Integer)
        ''' <summary>
        ''' If the <see cref="GoalReached"/> then reset and continute learning.
        ''' </summary>
        Protected MustOverride Sub __reset()

        ''' <summary>
        ''' 目标达成所得到的奖励
        ''' </summary>
        ''' <returns></returns>
        Public Property GoalRewards As Integer = 1
        ''' <summary>
        ''' 目标没有达成的罚分
        ''' </summary>
        ''' <returns></returns>
        Public Property GoalPenalty As Integer = -100

        Public Sub RunLearningLoop(n As Integer)
            Dim q As New QTable(ActionRange)

            For count As Integer = 0 To n

                Do While Not GoalReached   ' CHECK IF WON, THEN RESET
                    Call __run(q, count)

                    If Not GoalReached Then
                        Call q.UpdateQvalue(GoalPenalty, _stat.Current)  ' 目标还没有达成，则罚分
                    End If
                Loop

                Call __reset()
                Call q.UpdateQvalue(GoalRewards, _stat.Current)  ' REWARDS AND ADJUSTMENT OF WEIGHTS SHOULD TAKE PLACE HERE
            Next
        End Sub
    End Class
End Namespace