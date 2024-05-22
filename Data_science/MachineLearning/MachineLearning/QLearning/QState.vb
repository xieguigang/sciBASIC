#Region "Microsoft.VisualBasic::38db7f194f62b5cf3172d0da708b2383, Data_science\MachineLearning\MachineLearning\QLearning\QState.vb"

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

    '   Total Lines: 71
    '    Code Lines: 35 (49.30%)
    ' Comment Lines: 23 (32.39%)
    '    - Xml Docs: 95.65%
    ' 
    '   Blank Lines: 13 (18.31%)
    '     File Size: 2.86 KB


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
    ''' interface helper for write cdf model file
    ''' </summary>
    Public Interface IQStateFeatureSet

        ReadOnly Property stateFeatures As IEnumerable(Of String)
        ReadOnly Property QValueNames As IEnumerable(Of String)
        ReadOnly Property AllQStates As IEnumerable

        Function ExtractStateVector(stat As Object) As Double()

    End Interface

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T">Status object</typeparam>
    Public MustInherit Class QState(Of T As ICloneable) : Implements IQStateFeatureSet

        Protected stateValue As T
        Protected allStates As New Dictionary(Of String, T)

        Public Sub SetState(x As T)
            stateValue = x
            allStates(x.ToString) = x
        End Sub

        Public ReadOnly Property AllQStates As IEnumerable Implements IQStateFeatureSet.AllQStates
            Get
                Return allStates.Values
            End Get
        End Property

        ''' <summary>
        ''' 假若操作不会涉及到数据修改，请使用这个属性来减少性能的损失，<see cref="Current"/>属性返回的值和本属性是一样的，
        ''' 只不过<see cref="Current"/>属性是从<see cref="ICloneable.Clone()"/>方法得到的数据，所以性能方面会有损失
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property State As T
            Get
                Return stateValue
            End Get
        End Property

        ''' <summary>
        ''' map before the action is taken, clone object: <see cref="ICloneable.Clone()"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Current As T
            Get
                Return DirectCast(stateValue.Clone, T)
            End Get
        End Property

        Public MustOverride ReadOnly Property stateFeatures As IEnumerable(Of String) Implements IQStateFeatureSet.stateFeatures
        Public MustOverride ReadOnly Property QValueNames As IEnumerable(Of String) Implements IQStateFeatureSet.QValueNames

        ''' <summary>
        ''' Gets the <see cref="Current"/> states.
        ''' Returns the map state which results from an initial map state after an
        ''' action is applied. In case the action is invalid, the returned map is the
        ''' same as the initial one (no move). </summary>
        ''' <param name="action"> taken by the avatar ('@') </param>
        ''' <returns> resulting map after the action is taken </returns>
        Public MustOverride Function GetNextState(action As Integer) As T

        Public MustOverride Function ExtractStateVector(stat As Object) As Double() Implements IQStateFeatureSet.ExtractStateVector
    End Class
End Namespace
