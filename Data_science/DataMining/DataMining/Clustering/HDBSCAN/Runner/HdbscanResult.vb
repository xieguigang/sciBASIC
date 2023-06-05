Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Hdbscanstar

Namespace HDBSCAN.Runner
    Public Class HdbscanResult
        Public Property Labels As Integer()
        Public Property OutliersScore As List(Of OutlierScore)
        Public Property HasInfiniteStability As Boolean
    End Class
End Namespace
