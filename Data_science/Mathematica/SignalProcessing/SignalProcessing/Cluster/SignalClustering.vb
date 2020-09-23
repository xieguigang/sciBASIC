Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Public Class SignalClustering

    ReadOnly tree As AVLTree(Of GeneralSignal, GeneralSignal)

    ReadOnly steps#
    ReadOnly sig_equals As Double
    ReadOnly sig_simialr As Double

    Sub New(Optional sig_equals# = 0.8, Optional sig_simialr# = 0.4, Optional steps# = 0.25)
        Me.tree = New AVLTree(Of GeneralSignal, GeneralSignal)(AddressOf CompareTwoSignals, Function(sig) sig.reference)
        Me.steps = steps
        Me.sig_simialr = sig_simialr
        Me.sig_equals = sig_equals
    End Sub

    Public Iterator Function GetClusters() As IEnumerable(Of NamedCollection(Of GeneralSignal))
        Dim i As i32 = Scan0

        For Each node In tree.GetAllNodes
            Yield New NamedCollection(Of GeneralSignal) With {
                .name = ++i,
                .value = node.Members
            }
        Next
    End Function

    Public Function CompareTwoSignals(a As GeneralSignal, b As GeneralSignal) As Integer
        Dim var As Double = Alignment.Similarity(a, b, steps)

        If var >= sig_equals Then
            Return 0
        ElseIf var >= sig_simialr Then
            Return 1
        Else
            Return -1
        End If
    End Function

    Public Function AddSignal(signal As GeneralSignal) As SignalClustering
        signal = signal.Normalize
        tree.Add(signal, signal, valueReplace:=False)

        Return Me
    End Function
End Class
