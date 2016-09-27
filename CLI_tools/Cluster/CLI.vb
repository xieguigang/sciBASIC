#Region "Microsoft.VisualBasic::c0cce5893014d4ddec6d19af3b8b7869, ..\visualbasic_App\CLI_tools\Cluster\CLI.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.Framework.KMeans
Imports Microsoft.VisualBasic.DataVisualization.Network.FileStream
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Linq

Module CLI

    ''' <summary>
    ''' 对一组数字进行聚类操作，其实在这里就是将这组数值生成Entity数据对象，然后将数值本身作为自动生成的Entity对象的一个唯一属性
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/n.cluster",
               Info:="Cluserting the numbers.",
               Usage:="/n.cluster /in <txt/values> /n <num_of_cluster> [/out <out.csv>]")>
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
                out = App.CurrentDirectory & "/ClusterNumber.Csv"
            End If
        End If

        Return nums.ValueGroups(args.GetInt32("/n")) > out
    End Function

    <ExportAPI("/kmeans",
               Info:="Performance a kmeans clustering operation.",
               Usage:="/kmeans /MAT <entity_matrix.Csv> /n <num_of_cluster> [/map <Name> /out <cluster.csv>]")>
    Public Function ClusterDataSet(args As CommandLine.CommandLine) As Integer

        Throw New Exception("Hello world!")

        Dim inFile As String = args("/MAT")
        Dim n As Integer = args.GetInt32("/n")
        Dim out As String = args.GetValue("/out", inFile.TrimFileExt & ".Cluster.Csv")
        Dim ds As IEnumerable(Of EntityLDM) = EntityLDM.Load(inFile, args.GetValue("/map", "Name"))

        Return Kmeans(ds, n) > out
    End Function

    <ExportAPI("/bTree.Cluster",
               Info:="Performance a binary tree clustering operations on the input data set.",
               Usage:="/bTree.Cluster /MAT <entity_matrix.csv> [/map <Name> /parallel /out <out.cluster.csv>]")>
    Public Function bTreeCluster(args As CommandLine.CommandLine) As Integer
        Dim inFile As String = args - "/MAT"
        Dim parallel As Boolean = args.GetBoolean("/parallel")
        Dim out As String = args.GetValue("/out", $"{inFile.TrimFileExt}.{NameOf(bTreeCluster)}.csv")
        Dim map As String = args.GetValue("/map", "Name")
        Dim maps As Dictionary(Of String, String) =
            New Dictionary(Of String, String) From {{map, NameOf(EntityLDM.Name)}}
        Dim dataSet As IEnumerable(Of EntityLDM) =
            inFile.LoadCsv(Of EntityLDM)(maps:=maps)
        dataSet = dataSet.TreeCluster
        Return dataSet.SaveTo(out)
    End Function

    ''' <summary>
    ''' Converts the bTree clustering result into the cytoscape network model file.
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/bTree",
               Info:="Converts the bTree clustering result into the cytoscape network model file.",
               Usage:="/bTree /cluster <tree.cluster.csv> [/out <outDIR>]")>
    Public Function bTree(args As CommandLine.CommandLine) As Integer
        Dim inFile As String = args - "/cluster"
        Dim out As String = ("/out" <= args) ^ $"{inFile.TrimFileExt}-bTree/"
        Dim clusters As IEnumerable(Of EntityLDM) = inFile.LoadCsv(Of EntityLDM)
        Dim tree As Network = clusters.bTreeNET
        Return tree > out
    End Function

    ''' <summary>
    ''' Partitioning the binary tree cluster by using the clustering path.
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/bTree.Partitioning",
               Info:="Partitioning the binary tree cluster by using the clustering path.",
               Usage:="/bTree.Partitioning /cluster <clusters.csv> [/depth <-1> /out <partions.csv>]")>
    <ParameterInfo("/depth", True,
                   Description:="The maximum depth of the clustering path using for the partitioning operations. 
Default or value zero or negative is using the shortest path in the cluster entity.")>
    Public Function bTreePartitioning(args As CommandLine.CommandLine) As Integer
        Dim inFile As String = args - "/cluster"
        Dim depth As Integer = args.GetValue("/depth", -1)
        Dim out As String = args.GetValue("/out", inFile.TrimFileExt & $".{depth}.csv")
        Dim result As List(Of Partition) = inFile.LoadCsv(Of EntityLDM).Partitioning(depth)
        Return result > out
    End Function
End Module
