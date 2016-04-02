Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.Framework.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.DocumentFormat.Csv

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

    <ExportAPI("/bTree.Cluster",
               Usage:="/bTree.Cluster /MAT <entity_matrix.csv> [/map <Name> /parallel /out <out.cluster.csv>]")>
    Public Function bTreeCluster(args As CommandLine.CommandLine) As Integer
        Dim inFile As String = args - "/MAT"
        Dim parallel As Boolean = args.GetBoolean("/parallel")
        Dim out As String = "/out" <= args ^ $"{inFile.TrimFileExt}.{NameOf(bTreeCluster)}.csv"
        Dim map As String = "/map" <= args ^ "Name"
        Dim maps As Dictionary(Of String, String) = New Dictionary(Of String, String) From {{map, NameOf(EntityLDM.Name)}}
        Dim dataSet As IEnumerable(Of EntityLDM) = inFile.LoadCsv(Of EntityLDM)(maps:=maps)
        dataSet = dataSet.TreeCluster
        Return dataSet.SaveTo(out)
    End Function

    <ExportAPI("/bTree", Usage:="/bTree /cluster <tree.cluster.csv> [/out <outDIR>]")>
    Public Function bTree(args As CommandLine.CommandLine) As Integer
        Dim inFile As String = args - "/cluster"
        Dim out As String = ("/out" <= args) ^ $"{inFile.TrimFileExt}-bTree/"
        Dim clusters As IEnumerable(Of EntityLDM) = inFile.LoadCsv(Of EntityLDM)
        Dim tree = clusters.bTreeNET
        Return tree > out
    End Function
End Module
