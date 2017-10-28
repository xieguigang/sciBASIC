#Region "Microsoft.VisualBasic::dbf0e2e5da27ee80536b305acf9a6069, ..\sciBASIC#\Data_science\MachineLearning\QLearning\QTable.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.MachineLearning.QLearning.DataModel

Namespace QLearning

    ''' <summary>
    ''' The heart of the Q-learning algorithm, the QTable contains the table
    ''' which maps states, actions and their Q values. This class has elaborate
    ''' documentation, and should be the focus of the students' body of work
    ''' for the purposes of this tutorial.
    '''
    ''' @author A.Liapis (Original author), A. Hartzen (2013 modifications); xie.guigang@gcmodeller.org (2016 modifications)
    ''' </summary>
    Public MustInherit Class QTable(Of T As ICloneable)
        Implements IQTable

        ''' <summary>
        ''' for creating random numbers
        ''' </summary>
        Protected ReadOnly __randomGenerator As Random

        ''' <summary>
        ''' the table variable stores the Q-table, where the state is saved
        ''' directly as the actual map. Each map state has an array of Q values
        ''' for all the actions available for that state.
        ''' </summary>
        Public ReadOnly Property Table As Dictionary(Of Action) Implements IQTable.Table

        ''' <summary>
        ''' the actionRange variable determines the number of actions available
        ''' at any map state, and therefore the number of Q values in each entry
        ''' of the Q-table.
        ''' </summary>
        Public ReadOnly Property ActionRange As Integer Implements IQTable.ActionRange

#Region "E-GREEDY Q-LEARNING SPECIFIC VARIABLES"

        ''' <summary>
        ''' for e-greedy Q-learning, when taking an action a random number is
        ''' checked against the explorationChance variable: if the number is
        ''' below the explorationChance, then exploration takes place picking
        ''' an action at random. Note that the explorationChance is not a final
        ''' because it is customary that the exploration chance changes as the
        ''' training goes on.
        ''' </summary>
        Public Property ExplorationChance As Single = 0.05F Implements IQTable.ExplorationChance
        ''' <summary>
        ''' the discount factor is saved as the gammaValue variable. The
        ''' discount factor determines the importance of future rewards.
        ''' If the gammaValue is 0 then the AI will only consider immediate
        ''' rewards, while with a gammaValue near 1 (but below 1) the AI will
        ''' try to maximize the long-term reward even if it is many moves away.
        ''' </summary>
        Public Property GammaValue As Single = 0.9F Implements IQTable.GammaValue
        ''' <summary>
        ''' the learningRate determines how new information affects accumulated
        ''' information from previous instances. If the learningRate is 1, then
        ''' the new information completely overrides any previous information.
        ''' Note that the learningRate is not a final because it is
        ''' customary that the learningRate changes as the
        ''' training goes on.
        ''' </summary>
        Public Property LearningRate As Single = 0.15F Implements IQTable.LearningRate
#End Region

        'PREVIOUS STATE AND ACTION VARIABLES
        ''' <summary>
        ''' Since in Q-learning the updates to the Q values are made ONE STEP
        ''' LATE, the state of the world when the action resulting in the reward
        ''' was made must be stored.
        ''' </summary>
        Protected _prevState As T
        ''' <summary>
        ''' Since in Q-learning the updates to the Q values are made ONE STEP
        ''' LATE, the index of the action which resulted in the reward must be
        ''' stored.
        ''' </summary>
        Dim _prevAction As Integer

        ''' <summary>
        ''' Q table constructor, initiates variables. </summary>
        ''' <param name="actionRange"> number of actions available at any map state </param>
        Sub New(actionRange As Integer)
            Me.New()
            Me.ActionRange = actionRange
            Me.Table = New Dictionary(Of Action)
        End Sub

        Sub New(model As QModel)
            Me.New()
            Me.ActionRange = model.ActionRange
            Me.ExplorationChance = model.ExplorationChance
            Me.GammaValue = model.GammaValue
            Me.LearningRate = model.LearningRate
            Me.Table = model.Actions.ToDictionary
        End Sub

        Private Sub New()
            __randomGenerator = New Random()
        End Sub

        ''' <summary>
        ''' For this example, the getNextAction function uses an e-greedy
        ''' approach, having exploration happen if the exploration chance
        ''' is rolled.
        ''' ( **** 请注意，这个函数所返回的值为最佳选择的Index编号，所以可能还需要进行一些转换 **** )
        ''' </summary>
        ''' <param name="map"> current map (state) </param>
        ''' <returns> the action to be taken by the calling program </returns>
        Public Overridable Function NextAction(map As T) As Integer
            _prevState = CType(map.Clone(), T)

            If __randomGenerator.NextDouble() < ExplorationChance Then
                _prevAction = __explore()
            Else
                _prevAction = __getBestAction(map)
            End If
            Return _prevAction
        End Function

        ''' <summary>
        ''' The getBestAction function uses a greedy approach for finding
        ''' the best action to take. Note that if all Q values for the current
        ''' state are equal (such as all 0 if the state has never been visited
        ''' before), then getBestAction will always choose the same action.
        ''' If such an action is invalid, this may lead to a deadlock as the
        ''' map state never changes: for situations like these, exploration
        ''' can get the algorithm out of this deadlock.
        ''' </summary>
        ''' <param name="map"> current map (state) </param>
        ''' <returns> the action with the highest Q value </returns>
        Private Function __getBestAction(map As T) As Integer
            Dim rewards() As Single = Me.__getActionsQValues(map)
            Dim maxRewards As Single = Single.NegativeInfinity
            Dim indexMaxRewards As Integer = 0

            For i As Integer = 0 To rewards.Length - 1
                ' Gets the max element value its index in the Qvalues

                If maxRewards < rewards(i) Then
                    maxRewards = rewards(i)
                    indexMaxRewards = i
                End If
            Next i

            ' decode this index value as the action controls
            Return indexMaxRewards
        End Function

        ''' <summary>
        ''' The explore function is called for e-greedy algorithms.
        ''' It can choose an action at random from all available,
        ''' or can put more weight towards actions that have not been taken
        ''' as often as the others (most unknown).
        ''' </summary>
        ''' <returns> index of action to take </returns>
        ''' <remarks>在这里得到可能的下一步的动作的在动作列表里面编号值， Index</remarks>
        Protected Function __explore() As Integer
            Return (New Random(Me.__randomGenerator.Next(ActionRange + 100 * _prevAction))).Next(ActionRange)
        End Function

        ''' <summary>
        ''' The updateQvalue is the heart of the Q-learning algorithm. Based on
        ''' the reward gained by taking the action prevAction while being in the
        ''' state prevState, the updateQvalue must update the Q value of that
        ''' {prevState, prevAction} entry in the Q table. In order to do that,
        ''' the Q value of the best action of the current map state must also
        ''' be calculated.
        ''' </summary>
        ''' <param name="reward"> at the current map state </param>
        ''' <param name="map"> current map state (for finding the best action of the
        ''' current map state) </param>
        Public Overridable Sub UpdateQvalue(reward As Integer, map As T)
            Dim preVal() As Single = Me.__getActionsQValues(Me._prevState)
            preVal(Me._prevAction) += Me.LearningRate * (reward + Me.GammaValue * Me.__getActionsQValues(map)(Me.__getBestAction(map)) - preVal(Me._prevAction))
        End Sub

        ''' <summary>
        ''' This helper function is used for entering the map state into the
        ''' HashMap </summary>
        ''' <param name="map"> </param>
        ''' <returns> String used as a key for the HashMap </returns>
        Protected MustOverride Function __getMapString(map As T) As String

        ''' <summary>
        ''' The getActionsQValues function returns an array of Q values for
        ''' all the actions available at any state. Note that if the current
        ''' map state does not already exist in the Q table (never visited
        ''' before), then it is initiated with Q values of 0 for all of the
        ''' available actions.
        ''' </summary>
        ''' <param name="map"> current map (state) </param>
        ''' <returns> an array of Q values for all the actions available at any state </returns>
        Private Function __getActionsQValues(map As T) As Single()
            Dim actions() As Single = GetValues(map)
            If actions Is Nothing Then ' 还不存在这个动作，则添加新的动作
                Dim initialActions(ActionRange - 1) As Single
                For i As Integer = 0 To ActionRange - 1
                    initialActions(i) = 0.0F
                Next i
                _Table += New Action With {  ' If the current environment state is not in the program's memory, then store it, this is the so called learn
                    .EnvirState = __getMapString(map),
                    .Qvalues = initialActions
                }
                Return initialActions
            End If
            Return actions
        End Function

        ''' <summary>
        ''' Helper function to find the Q-values of a given map state.
        ''' </summary>
        ''' <param name="map"> current map (state) </param>
        ''' <returns> the Q-values stored of the Qtable entry of the map state, otherwise null if it is not found </returns>
        Public Overridable Function GetValues(map As T) As Single()
            Dim mapKey As String = __getMapString(map)
            If Table.ContainsKey(mapKey) Then
                Return Table(mapKey).Qvalues
            End If
            Return Nothing
        End Function
    End Class
End Namespace
