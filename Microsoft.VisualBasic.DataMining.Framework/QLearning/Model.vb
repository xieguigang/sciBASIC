Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization

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

    ''' <summary>
    ''' 属性是时间
    ''' </summary>
    Public Class IndexCurve : Inherits DynamicPropertyBase(Of Double)
        Implements sIdEnumerable

        Public Property uid As String Implements sIdEnumerable.Identifier

        Sub New()
        End Sub

        Sub New(uid As String)
            Me.Properties = New Dictionary(Of String, Double)
            Me.uid = uid
        End Sub
    End Class

    Public Class QTableDump

        ReadOnly __buffer As New Dictionary(Of IndexCurve)

        Public Sub Dump(table As IQTable, iteration As Integer)
            For Each o In table.Table.Values
                For i As Integer = 0 To table.ActionRange - 1
                    Dim uid As String = $"[{i}] {o.EnvirState}"
                    If Not __buffer.ContainsKey(uid) Then
                        Call __buffer.Add(uid, New IndexCurve(uid))
                    End If
                    Call __buffer(uid).Properties.Add(iteration, o.Qvalues(i))
                Next
            Next
        End Sub

        Public Sub Save(path As String)
            Call __buffer.Values.SaveTo(path, Encodings.ASCII)
        End Sub
    End Class
End Namespace