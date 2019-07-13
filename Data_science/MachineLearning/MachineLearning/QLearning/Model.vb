#Region "Microsoft.VisualBasic::c6c7daf82b5167ad0880e0bc13025457, Data_science\MachineLearning\MachineLearning\QLearning\Model.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
Imports Microsoft.VisualBasic.Language

Namespace QLearning.DataModel

    Public Interface IQTable
        ReadOnly Property Table As Dictionary(Of Action)
        ReadOnly Property ActionRange As Integer
        Property ExplorationChance As Single
        Property GammaValue As Single
        Property LearningRate As Single
    End Interface

    ''' <summary>
    ''' Data model of the <see cref="QTable(Of T)"/>, you can using this object to stores the trained QL_AI into a file.
    ''' </summary>
    Public Class QModel

        Public Property Actions As Action()
        Public Property ActionRange As Integer
        Public Property ExplorationChance As Single
        Public Property GammaValue As Single
        Public Property LearningRate As Single

        Sub New(qtable As IQTable)
            Actions = qtable.Table.Values.ToArray
            ActionRange = qtable.ActionRange
            ExplorationChance = qtable.ExplorationChance
            GammaValue = qtable.GammaValue
            LearningRate = qtable.LearningRate
        End Sub

        Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' 属性是时间
    ''' </summary>
    Public Class IndexCurve : Inherits DynamicPropertyBase(Of Double)
        Implements INamedValue

        Public Property uid As String Implements INamedValue.Key

        Sub New()
        End Sub

        Sub New(uid As String)
            Me.Properties = New Dictionary(Of String, Double)
            Me.uid = uid
        End Sub
    End Class
End Namespace
