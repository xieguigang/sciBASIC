#Region "Microsoft.VisualBasic::c079a3366665f8e118f16ab3b051ea84, Data_science\MachineLearning\MachineLearning\QLearning\Model.vb"

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

    '   Total Lines: 54
    '    Code Lines: 38 (70.37%)
    ' Comment Lines: 6 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (18.52%)
    '     File Size: 1.73 KB


    '     Interface IQTable
    ' 
    '         Properties: ActionRange, ExplorationChance, GammaValue, LearningRate, Table
    ' 
    '     Class QModel
    ' 
    '         Properties: ActionRange, Actions, ExplorationChance, GammaValue, LearningRate
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class IndexCurve
    ' 
    '         Properties: uid
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace QLearning.DataModel

    ''' <summary>
    ''' The common interface of the tabular Q-learning models.
    ''' </summary>
    Public Interface IQTable
        ''' <summary>
        ''' The state-action value table: the key is the environment state and 
        ''' the value is the Q-value vector of all of the possible actions.
        ''' </summary>
        ReadOnly Property Table As Dictionary(Of Action)
        ''' <summary>
        ''' The number of the possible actions of each environment state.
        ''' </summary>
        ReadOnly Property ActionRange As Integer
        ''' <summary>
        ''' The chance of taking a random action instead of the best known 
        ''' action, which is the epsilon value of the epsilon-greedy strategy.
        ''' </summary>
        Property ExplorationChance As Single
        ''' <summary>
        ''' The discount factor of the future reward.
        ''' </summary>
        Property GammaValue As Single
        ''' <summary>
        ''' The learning rate of the Q-value update.
        ''' </summary>
        Property LearningRate As Single
    End Interface

    ''' <summary>
    ''' Data model of the <see cref="QTable(Of T)"/>, you can using this object to stores the trained QL_AI into a file.
    ''' </summary>
    Public Class QModel

        ''' <summary>
        ''' All of the state-action pairs which are stored in the Q table.
        ''' </summary>
        ''' <returns>An array of the <see cref="Action"/> objects.</returns>
        Public Property Actions As Action()
        ''' <summary>
        ''' The number of the possible actions of each environment state.
        ''' </summary>
        ''' <returns>An <see cref="Integer"/> value.</returns>
        Public Property ActionRange As Integer
        ''' <summary>
        ''' The chance of taking a random action instead of the best known action.
        ''' </summary>
        ''' <returns>A <see cref="Single"/> value in the interval ``[0, 1]``.</returns>
        Public Property ExplorationChance As Single
        ''' <summary>
        ''' The discount factor of the future reward.
        ''' </summary>
        ''' <returns>A <see cref="Single"/> value in the interval ``[0, 1]``.</returns>
        Public Property GammaValue As Single
        ''' <summary>
        ''' The learning rate of the Q-value update.
        ''' </summary>
        ''' <returns>A <see cref="Single"/> value in the interval ``[0, 1]``.</returns>
        Public Property LearningRate As Single

        ''' <summary>
        ''' Create the data model from a trained Q table object.
        ''' </summary>
        ''' <param name="qtable">A trained <see cref="IQTable"/> object.</param>
        Sub New(qtable As IQTable)
            Actions = qtable.Table.Values.ToArray
            ActionRange = qtable.ActionRange
            ExplorationChance = qtable.ExplorationChance
            GammaValue = qtable.GammaValue
            LearningRate = qtable.LearningRate
        End Sub

        ''' <summary>
        ''' Create a new empty Q model.
        ''' </summary>
        Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' 属性是时间
    ''' </summary>
    ''' <remarks>
    ''' The property keys of this dynamic property object are the time index 
    ''' and the property values are the curve values at the corresponding time.
    ''' </remarks>
    Public Class IndexCurve : Inherits DynamicPropertyBase(Of Double)
        Implements INamedValue

        ''' <summary>
        ''' The unique reference id of this time curve.
        ''' </summary>
        ''' <returns>A string value.</returns>
        Public Property uid As String Implements INamedValue.Key

        ''' <summary>
        ''' Create a new empty time curve.
        ''' </summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Create a new time curve with a specific unique reference id.
        ''' </summary>
        ''' <param name="uid">The unique reference id of this time curve.</param>
        Sub New(uid As String)
            Me.Properties = New Dictionary(Of String, Double)
            Me.uid = uid
        End Sub
    End Class
End Namespace
