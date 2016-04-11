Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.Framework.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.DocumentFormat.Csv

Module CLI

    <ExportAPI("/n.cluster", Usage:="/n.cluster /in <txt/values> /n <num_of_cluster> [/out <out.csv>]")>
    Public Function ClusterNumbers(args As CommandLine.CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim out As String = args("/out")
        Dim nums As Double()

        If [in].FileExists Then
            nums = [in].ReadAllLines.ToArray(Function(s) Val(s))
            If String.IsNullOrEmpty(out) Then
                out = [in].TrimFileExt & ".cluster.csv"
            End If
        Else
            nums = [in].Split(","c).ToArray(Function(s) Val(s.Trim))
            If String.IsNullOrEmpty(out) Then
                out = App.CurrentWork & "/ClusterNumber.Csv"
            End If
        End If

        Dim entities As EntityLDM() = nums.ToArray(
            Function(n, i) New EntityLDM With {
                .Name = i & ":" & n,
                .Properties = New Dictionary(Of String, Double) From {{"val", n}}
            })

        Return __kmeansCommon(entities, args.GetInt32("/n")) > out
    End Function

    Private Function __kmeansCommon(source As IEnumerable(Of EntityLDM), n As Integer) As List(Of EntityLDM)
        Dim maps As String() = source.First.Properties.Keys.ToArray
        Dim clusters As ClusterCollection(Of Entity) = n.ClusterDataSet(source.ToArray(Function(x) x.ToModel))
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

        Return result
    End Function

    <ExportAPI("/kmeans",
               Info:="Performance a kmeans clustering operation.",
               Usage:="/kmeans /MAT <entity_matrix.Csv> /n <num_of_cluster> [/map <Name> /out <cluster.csv>]")>
    Public Function ClusterDataSet(args As CommandLine.CommandLine) As Integer
        Dim inFile As String = args("/MAT")
        Dim n As Integer = args.GetInt32("/n")
        Dim out As String = args.GetValue("/out", inFile.TrimFileExt & ".Cluster.Csv")
        Dim ds As IEnumerable(Of EntityLDM) = EntityLDM.Load(inFile, args.GetValue("/map", "Name"))

        Return __kmeansCommon(ds, n) > out
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

    <ExportAPI("/bTree.Partitioning",
               Usage:="/bTree.Partitioning /cluster <clusters.csv> [/depth <-1> /out <partions.csv>]")>
    Public Function bTreePartitioning(args As CommandLine.CommandLine) As Integer
        Dim inFile As String = args - "/cluster"
        Dim depth As Integer = args.GetValue("/depth", -1)
        Dim out As String = args.GetValue("/out", inFile.TrimFileExt & $".{depth}.csv")
        Dim result As List(Of Partition) = inFile.LoadCsv(Of EntityLDM).Partitioning(depth)
        Return result > out
    End Function
End Module
