Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' 这个方法的结果可能结果不是太好
''' </summary>
Public Module Analysis

    Public Delegate Function GetPoints(vals As Double()) As Integer()

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="DIR"></param>
    ''' <param name="analysis"></param>
    ''' <param name="clustersExpected">最终的结果的聚类分组cluster的数量</param>
    ''' <returns></returns>
    Public Function GroupsAnalysis(DIR As String, analysis As Dictionary(Of String, GetPoints), clustersExpected As Integer) As Dictionary(Of String, Dictionary(Of String, DoubleRange))
        Dim codes As New List(Of NamedValue(Of Dictionary(Of String, Double)))

        For Each file As String In ls - l - r - wildcards("*.csv") <= DIR
            Dim outData As ODEsOut = ODEsOut.LoadFromDataFrame(file)
            Dim ps As New List(Of Integer)

            For Each m In analysis
                ps += m.Value(outData.y(m.Key).x)
            Next

            codes += New NamedValue(Of Dictionary(Of String, Double)) With {
                .Name = ps.JoinBy(","),
                .x = outData.params
            }
        Next

        Dim datasets As Entity() = codes.ToArray(
            Function(x) New Entity With {
                .uid = x.Name,
                .Properties = x.Name.Split(","c).ToArray(AddressOf Val)
        })
        Dim clusters = ClusterDataSet(clustersExpected, datasets)
        Dim out As New Dictionary(Of String, Dictionary(Of String, DoubleRange))
        Dim bufs = codes.GroupBy(Function(x) x.Name).ToDictionary(Function(g) g.First.Name, Function(g) g.ToArray)

        For Each cluster As KMeansCluster(Of Entity) In clusters
            Dim key As String = cluster.ClusterMean.JoinBy(",")
            Dim value As New Dictionary(Of String, DoubleRange)
            Dim data As New List(Of KeyValuePair(Of String, Double))

            For Each x As Entity In cluster
                data += bufs(x.uid).Select(Function(d) d.x.ToArray).MatrixAsIterator
            Next

            Dim pg = data.GroupBy(Function(p) p.Key)

            For Each p In pg
                Dim array = p.ToArray
                Dim vals = array.ToArray(Function(x) x.Value)
                value(p.Key) = New DoubleRange(vals.Min, vals.Max)
            Next

            out(key) = value
        Next

        Return out
    End Function
End Module
