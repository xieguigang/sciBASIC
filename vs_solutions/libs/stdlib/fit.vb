
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports umapHelper = Microsoft.VisualBasic.DataMining.UMAP.NumericTableExtensions

Public Module fit

    Public Function linear(x As IEnumerable(Of Double), y As IEnumerable(Of Double), Optional w As IEnumerable(Of Double) = Nothing) As IFitted
        If w Is Nothing Then
            Return LeastSquares.LinearFit(x.ToArray, y.ToArray)
        Else
            Return WeightedLinearRegression.Regress(x.ToArray, y.ToArray, w.ToArray)
        End If
    End Function

    Public Function lma()

    End Function

    Public Function umap(x As NumericTable, Optional dims As Integer = 3) As NumericTable
        Return umapHelper.umap(x, dims)
    End Function

    Public Function tsne(x As NumericTable, Optional dims As Integer = 3) As NumericTable

    End Function

End Module
