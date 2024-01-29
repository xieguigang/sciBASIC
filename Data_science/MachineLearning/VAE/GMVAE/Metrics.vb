Imports System.Data
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

''' <summary>
''' Metrics used to evaluate our model
''' </summary>
Public Class Metrics

    ''' <summary>
    ''' Code taken from the work 
    ''' VaDE (Variational Deep Embedding:A Generative Approach to Clustering)
    ''' </summary>
    ''' <param name="Y_pred"></param>
    ''' <param name="Y"></param>
    ''' <returns></returns>
    Public Function cluster_acc(Y_pred As Vector, Y As Vector) As Double
        Dim D = std.Max(Y_pred.Max(), Y.Max()) + 1
        Dim w = NumericMatrix.Zero(D, D)

        For i As Integer = 0 To Y_pred.Dim - 1
            w(CInt(Y_pred(i)), CInt(Y(i))) += 1.0
        Next

        Dim max = linear.linear_sum_assignment(w.max - w)
        Dim row = max.row, col = max.col
        Dim sum = Enumerable.Range(0, row.Dim).Select(Function(i) w(CInt(row(i)), CInt(col(i)))).Sum

        Return sum * 1.0 / Y_pred.Dim
    End Function

    Public Function nmi(Y_pred As Vector, Y As Vector)
        Return linear.normalized_mutual_info_score(Y_pred, Y, average_method:="arithmetic")
    End Function
End Class
