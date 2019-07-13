#Region "Microsoft.VisualBasic::9a3d95fdb473ab81f1abb1be0519052f, Data_science\DataMining\DataMining\Clustering\DBSCAN\DbscanPoint.vb"

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

    '     Class DbscanPoint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace DBSCAN

    Public Class DbscanPoint(Of T)

        Public IsVisited As Boolean
        Public ClusterPoint As T
        Public ClusterId As Integer

        Public Sub New(x As T)
            ClusterPoint = x
            IsVisited = False
            ClusterId = ClusterIDs.Unclassified
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{ClusterId}] {ClusterPoint.ToString}"
        End Function
    End Class
End Namespace
