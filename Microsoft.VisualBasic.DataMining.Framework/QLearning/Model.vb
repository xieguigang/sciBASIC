Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization

Namespace QLearning

    Public Interface IQTable
        ReadOnly Property Table As Dictionary(Of Action)
        ReadOnly Property ActionRange As Integer
        Property ExplorationChance As Single
        Property GammaValue As Single
        Property LearningRate As Single
    End Interface

    ''' <summary>
    ''' Data model of the <see cref="QTable(Of T)"/>
    ''' </summary>
    Public Class QModel : Inherits ClassObject

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
End Namespace