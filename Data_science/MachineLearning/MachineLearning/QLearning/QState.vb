#Region "Microsoft.VisualBasic::a7523a95ebd9f5c59c1a765c693dfccb, Data_science\MachineLearning\MachineLearning\QLearning\QState.vb"

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

    '   Total Lines: 126
    '    Code Lines: 35 (27.78%)
    ' Comment Lines: 78 (61.90%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (10.32%)
    '     File Size: 5.40 KB


    '     Interface IQStateFeatureSet
    ' 
    '         Properties: AllQStates, QValueNames, stateFeatures
    ' 
    '         Function: ExtractStateVector
    ' 
    '     Class QState
    ' 
    '         Properties: AllQStates, Current, State
    ' 
    '         Sub: SetState
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace QLearning

    ''' <summary>
    ''' The feature set description of the Q-learning state objects, this 
    ''' interface is a helper for writing the cdf model file.
    ''' </summary>
    Public Interface IQStateFeatureSet

        ''' <summary>
        ''' The feature names of the state object.
        ''' </summary>
        ReadOnly Property stateFeatures As IEnumerable(Of String)
        ''' <summary>
        ''' The names of the Q-values of each state.
        ''' </summary>
        ReadOnly Property QValueNames As IEnumerable(Of String)
        ''' <summary>
        ''' All of the Q-learning state objects.
        ''' </summary>
        ReadOnly Property AllQStates As IEnumerable

        ''' <summary>
        ''' Extract the feature vector from a state object.
        ''' </summary>
        ''' <param name="stat">The target state object.</param>
        ''' <returns>An array of the feature values.</returns>
        Function ExtractStateVector(stat As Object) As Double()

    End Interface

    ''' <summary>
    ''' The abstract environment state model of the tabular Q-learning.
    ''' </summary>
    ''' <typeparam name="T">Status object</typeparam>
    ''' <remarks>
    ''' The derived class should implement the <see cref="GetNextState"/> 
    ''' function, so that the Q-learning engine is able to know how the 
    ''' environment changes after an action is taken.
    ''' </remarks>
    Public MustInherit Class QState(Of T As ICloneable) : Implements IQStateFeatureSet

        ''' <summary>
        ''' The current environment state value.
        ''' </summary>
        Protected stateValue As T
        ''' <summary>
        ''' All of the environment states which have been visited, the key is 
        ''' the string expression of the state object.
        ''' </summary>
        Protected allStates As New Dictionary(Of String, T)

        ''' <summary>
        ''' Set the current environment state value, and then register this 
        ''' state into the <see cref="allStates"/> collection.
        ''' </summary>
        ''' <param name="x">The new environment state value.</param>
        Public Sub SetState(x As T)
            stateValue = x
            allStates(x.ToString) = x
        End Sub

        ''' <summary>
        ''' All of the environment states which have been visited.
        ''' </summary>
        ''' <returns>A collection of the state objects.</returns>
        Public ReadOnly Property AllQStates As IEnumerable Implements IQStateFeatureSet.AllQStates
            Get
                Return allStates.Values
            End Get
        End Property

        ''' <summary>
        ''' 假若操作不会涉及到数据修改，请使用这个属性来减少性能的损失，<see cref="Current"/>属性返回的值和本属性是一样的，
        ''' 只不过<see cref="Current"/>属性是从<see cref="ICloneable.Clone()"/>方法得到的数据，所以性能方面会有损失
        ''' </summary>
        ''' <returns>
        ''' The reference of the current state object; since no clone is applied, 
        ''' the caller should never modify the returned object.
        ''' </returns>
        Public ReadOnly Property State As T
            Get
                Return stateValue
            End Get
        End Property

        ''' <summary>
        ''' map before the action is taken, clone object: <see cref="ICloneable.Clone()"/>
        ''' </summary>
        ''' <returns>
        ''' A cloned copy of the current state object, so that the caller can 
        ''' modify the returned object safely.
        ''' </returns>
        Public ReadOnly Property Current As T
            Get
                Return DirectCast(stateValue.Clone, T)
            End Get
        End Property

        ''' <summary>
        ''' The feature names of the state object.
        ''' </summary>
        ''' <returns>A collection of the feature names.</returns>
        Public MustOverride ReadOnly Property stateFeatures As IEnumerable(Of String) Implements IQStateFeatureSet.stateFeatures
        ''' <summary>
        ''' The names of the Q-values of each state.
        ''' </summary>
        ''' <returns>A collection of the Q-value names.</returns>
        Public MustOverride ReadOnly Property QValueNames As IEnumerable(Of String) Implements IQStateFeatureSet.QValueNames

        ''' <summary>
        ''' Gets the <see cref="Current"/> states.
        ''' Returns the map state which results from an initial map state after an
        ''' action is applied. In case the action is invalid, the returned map is the
        ''' same as the initial one (no move). </summary>
        ''' <param name="action"> taken by the avatar ('@') </param>
        ''' <returns> resulting map after the action is taken </returns>
        Public MustOverride Function GetNextState(action As Integer) As T

        ''' <summary>
        ''' Extract the feature vector from a given state object.
        ''' </summary>
        ''' <param name="stat">The target state object.</param>
        ''' <returns>An array of the feature values.</returns>
        Public MustOverride Function ExtractStateVector(stat As Object) As Double() Implements IQStateFeatureSet.ExtractStateVector
    End Class
End Namespace
