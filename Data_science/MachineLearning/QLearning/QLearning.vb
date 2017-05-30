#Region "Microsoft.VisualBasic::4c3086309ed003f353e618f339cb86d5, ..\sciBASIC#\Data_science\MachineLearning\QLearning\QLearning.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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

        Protected ReadOnly _stat As QState(Of T)

        Public ReadOnly Property Q As QTable(Of T)

        Sub New(state As QState(Of T), provider As Func(Of Integer, QTable(Of T)))
            _stat = state
            Q = provider(ActionRange)
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
        ''' <param name="i">Iteration counts.</param>
        Protected MustOverride Sub __run(i As Integer)
        ''' <summary>
        ''' If the <see cref="GoalReached"/> then reset and continute learning.
        ''' </summary>
        ''' <param name="i">机器学习的当前的迭代次数</param>
        Protected MustOverride Sub __reset(i As Integer)

        ''' <summary>
        ''' 目标达成所得到的奖励
        ''' </summary>
        ''' <returns></returns>
        Public Property GoalRewards As Integer = 10
        ''' <summary>
        ''' 目标没有达成的罚分
        ''' </summary>
        ''' <returns></returns>
        Public Property GoalPenalty As Integer = -100

        Public Sub RunLearningLoop(n As Integer)
            For count As Integer = 0 To n
                Call __reset(count)

                Do While Not GoalReached   ' CHECK IF WON, THEN RESET
                    Call __run(count)

                    If Not GoalReached Then
                        Call Q.UpdateQvalue(GoalPenalty, _stat.Current)  ' 目标还没有达成，则罚分
                    End If
                Loop

                Call Q.UpdateQvalue(GoalRewards, _stat.Current)  ' REWARDS AND ADJUSTMENT OF WEIGHTS SHOULD TAKE PLACE HERE
            Next

            Call __finishLearn()
        End Sub

        ''' <summary>
        ''' You can save you q table by overrides at here.
        ''' </summary>
        Protected Overridable Sub __finishLearn()

        End Sub
    End Class
End Namespace
