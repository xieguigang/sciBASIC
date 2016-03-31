Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.Framework.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic

Module CLI

    <ExportAPI("/kmeans",
               Info:="Performance a kmeans clustering operation.",
               Usage:="/kmeans /MAT <entity_matrix.Csv> /n <num_of_cluster> [/map <Name> /out <cluster.csv>]")>
    Public Function ClusterDataSet(args As CommandLine.CommandLine) As Integer
        Dim inFile As String = args("/MAT")
        Dim n As Integer = args.GetInt32("/n")
        Dim out As String = args.GetValue("/out", inFile.TrimFileExt & ".Cluster.Csv")
        Dim ds As IEnumerable(Of EntityLDM) = EntityLDM.Load(inFile, args.GetValue("/map", "Name"))
        Dim maps As String() = ds.First.Properties.Keys.ToArray
        Dim clusters As ClusterCollection(Of Entity) = n.ClusterDataSet(ds.ToArray(Function(x) x.ToModel))
        Dim result As New List(Of EntityLDM)

        n = 1

        For Each cluster As Cluster(Of Entity) In clusters
            Dim values As EntityLDM() = cluster.ToArray(Function(x) x.ToLDM(maps))

            For Each x In values
                x.Cluster = n
            Next

            result += values
            n += 1
        Next

        Return result > out
    End Function
End Module
