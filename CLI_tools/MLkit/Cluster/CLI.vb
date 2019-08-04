#Region "Microsoft.VisualBasic::e854ad30b18bd6f872d5f5e08409b227, CLI_tools\MLkit\Cluster\CLI.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    ' Module CLI
    ' 
    '     Function: ClusterDataSet, ClusterNumbers
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language.Default
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
            nums = [in].ReadAllLines.Select(Function(s) Val(s)).ToArray
            If String.IsNullOrEmpty(out) Then
                out = [in].TrimSuffix & ".cluster.csv"
            End If
        Else
            nums = [in].Split(","c).Select(Function(s) Val(s.Trim)).ToArray
            If String.IsNullOrEmpty(out) Then
                out = App.CurrentDirectory & "/ClusterNumber.Csv"
            End If
        End If

        Return nums.ValueGroups(args.GetInt32("/n")) >> out.FileOpen
    End Function

    <ExportAPI("/kmeans")>
    <Description("Performance a kmeans clustering operation.")>
    <Usage("/kmeans /in <entity_matrix.csv> /n <num_of_cluster> [/map <Name> /filter <column=value> /ignores <column1,column2,...> /out <cluster.csv>]")>
    Public Function ClusterDataSet(args As CommandLine.CommandLine) As Integer
        Dim inFile As String = args("/in")
        Dim n As Integer = args.GetInt32("/n")
        Dim filter As Func(Of EntityObject, Boolean) = args("/filter") _
            .DefaultValue _
            .GetTagValue("=", trim:=True) _
            .CreateFilter
        Dim ignores$() = args("/ignores").Split(",")
        Dim out As String = args("/out") Or (inFile.TrimSuffix & ".Cluster.Csv")
        Dim ds As IEnumerable(Of EntityClusterModel) = EntityObject _
            .LoadDataSet(inFile, args("/map") Or "Name") _
            .Where(Function(d) True = filter(d)) _
            .AsDataSet(ignores:=ignores) _
            .Select(Function(d)
                        Return EntityClusterModel.FromDataSet(d)
                    End Function) _
            .ToArray

        Return Kmeans(ds, n) >> out.FileOpen
    End Function

    '<ExportAPI("/bTree.Cluster",
    '           Info:="Performance a binary tree clustering operations on the input data set.",
    '           Usage:="/bTree.Cluster /MAT <entity_matrix.csv> [/map <Name> /parallel /out <out.cluster.csv>]")>
    'Public Function bTreeCluster(args As CommandLine.CommandLine) As Integer
    '    Dim inFile As String = args - "/MAT"
    '    Dim parallel As Boolean = args.GetBoolean("/parallel")
    '    Dim out As String = args.GetValue("/out", $"{inFile.TrimFileExt}.{NameOf(bTreeCluster)}.csv")
    '    Dim map As String = args.GetValue("/map", "Name")
    '    Dim maps As Dictionary(Of String, String) =
    '        New Dictionary(Of String, String) From {{map, NameOf(EntityLDM.Name)}}
    '    Dim dataSet As IEnumerable(Of EntityLDM) =
    '        inFile.LoadCsv(Of EntityLDM)(maps:=maps)
    '    dataSet = dataSet.TreeCluster
    '    Return dataSet.SaveTo(out)
    'End Function

    '''' <summary>
    '''' Converts the bTree clustering result into the cytoscape network model file.
    '''' </summary>
    '''' <param name="args"></param>
    '''' <returns></returns>
    '<ExportAPI("/bTree",
    '           Info:="Converts the bTree clustering result into the cytoscape network model file.",
    '           Usage:="/bTree /cluster <tree.cluster.csv> [/out <outDIR>]")>
    'Public Function bTree(args As CommandLine.CommandLine) As Integer
    '    Dim inFile As String = args - "/cluster"
    '    Dim out As String = ("/out" <= args) ^ $"{inFile.TrimFileExt}-bTree/"
    '    Dim clusters As IEnumerable(Of EntityLDM) = inFile.LoadCsv(Of EntityLDM)
    '    Dim tree As Network = clusters.bTreeNET
    '    Return tree > out
    'End Function

    '    ''' <summary>
    '    ''' Partitioning the binary tree cluster by using the clustering path.
    '    ''' </summary>
    '    ''' <param name="args"></param>
    '    ''' <returns></returns>
    '    <ExportAPI("/bTree.Partitioning",
    '               Info:="Partitioning the binary tree cluster by using the clustering path.",
    '               Usage:="/bTree.Partitioning /cluster <clusters.csv> [/depth <-1> /out <partions.csv>]")>
    '    <Argument("/depth", True,
    '                   Description:="The maximum depth of the clustering path using for the partitioning operations. 
    'Default or value zero or negative is using the shortest path in the cluster entity.")>
    '    Public Function bTreePartitioning(args As CommandLine.CommandLine) As Integer
    '        Dim inFile As String = args - "/cluster"
    '        Dim depth As Integer = args.GetValue("/depth", -1)
    '        Dim out As String = args.GetValue("/out", inFile.TrimFileExt & $".{depth}.csv")
    '        Dim result As List(Of Partition) = inFile.LoadCsv(Of EntityLDM).Partitioning(depth)
    '        Return result > out
    '    End Function
End Module
