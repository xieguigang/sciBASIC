Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.Kernel.Classifier.Neuron

Namespace NeuralNetwork.Models

    Public Module Extensions

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="row">第一个元素为分类，其余元素为属性</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CastTo(row As RowObject) As Entity
            Dim LQuery = From s As String In row.Skip(1) Select Val(s) '
            Return New Entity With {.Y = Val(row.First), .Properties = LQuery.ToArray}
        End Function
    End Module
End Namespace