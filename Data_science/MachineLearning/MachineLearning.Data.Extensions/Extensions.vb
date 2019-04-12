Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.QLearning.DataModel
Imports Microsoft.VisualBasic.Text
Imports row = Microsoft.VisualBasic.Data.csv.IO.DataSet

Public Module Extensions

    ''' <summary>
    ''' 从csv文件数据之中读取和当前的数据集一样的元素顺序的向量用于预测分析
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetInput(dataset As DataSet, data As row) As Double()
        Return dataset _
            .NormalizeMatrix _
            .names _
            .Select(Function(key) data(key)) _
            .ToArray
    End Function
End Module

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