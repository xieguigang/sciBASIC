
Namespace DBSCAN

    Public Class DbscanPoint(Of T)

        Public IsVisited As Boolean
        Public ClusterPoint As T
        Public ClusterId As Integer

        Public Sub New(x As T)
            ClusterPoint = x
            IsVisited = False
            ClusterId = CInt(ClusterIds.Unclassified)
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{ClusterId}] {ClusterPoint.ToString}"
        End Function
    End Class
End Namespace