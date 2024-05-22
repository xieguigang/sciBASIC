#Region "Microsoft.VisualBasic::ec615370fabe2d6d3c29922c58c25ebd, Data_science\MachineLearning\MachineLearning\QLearning\QLearning.vb"

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

    '   Total Lines: 84
    '    Code Lines: 35 (41.67%)
    ' Comment Lines: 34 (40.48%)
    '    - Xml Docs: 88.24%
    ' 
    '   Blank Lines: 15 (17.86%)
    '     File Size: 2.86 KB


    '     Class QLearning
    ' 
    '         Properties: GoalPenalty, GoalRewards, Q
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: finishQLearn, RunLearningLoop
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        ''' <summary>
        ''' The size of the <see cref="QTable"/>
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property ActionRange As Integer

        Public MustOverride ReadOnly Property GoalReached As Boolean

        Sub New(state As QState(Of T), provider As Func(Of Integer, QTable(Of T)))
            _stat = state
            Q = provider(ActionRange)
        End Sub

        Protected MustOverride Sub initialize()

        ''' <summary>
        ''' Takes a action for the agent.
        ''' </summary>
        ''' <param name="i">Iteration counts.</param>
        Protected MustOverride Sub run(i As Integer)
        ''' <summary>
        ''' If the <see cref="GoalReached"/> then reset and continute learning.
        ''' </summary>
        ''' <param name="i">机器学习的当前的迭代次数</param>
        Protected MustOverride Sub reset(i As Integer)

        Public Sub RunLearningLoop(n As Integer)
            If n <= 0 Then
                n = Integer.MaxValue
            End If

            For iteration As Integer = 0 To n
                Call reset(iteration)

                ' CHECK IF WON, THEN RESET
                Do While Not GoalReached
                    Call run(iteration)

                    If Not GoalReached Then
                        ' 目标还没有达成，则罚分
                        Call Q.UpdateQvalue(GoalPenalty, _stat.Current)
                    End If
                Loop

                ' REWARDS AND ADJUSTMENT OF WEIGHTS SHOULD TAKE PLACE HERE
                Call Q.UpdateQvalue(GoalRewards, _stat.Current)
            Next

            Call finishQLearn()
        End Sub

        ''' <summary>
        ''' You can save you Q table by overrides at here.
        ''' </summary>
        Protected Overridable Sub finishQLearn()

        End Sub
    End Class
End Namespace
