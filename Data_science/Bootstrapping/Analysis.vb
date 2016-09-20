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

        For Each cluster As KMeansCluster(Of Entity) In clusters

        Next
    End Function
End Module
