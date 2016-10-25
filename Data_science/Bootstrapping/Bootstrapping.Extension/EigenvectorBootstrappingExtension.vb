Imports Microsoft.VisualBasic.DataMining.KMeans.Tree
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module EigenvectorBootstrappingExtension

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"><see cref="LoadData"/>的输出数据</param>
    ''' <returns></returns>
    <Extension>
    Public Function BinaryKMeans(data As IEnumerable(Of VectorTagged(Of Dictionary(Of String, Double))), partitionDepth As Integer, Optional [stop] As Integer = -1) As Dictionary(Of NamedValue(Of Double()), Dictionary(Of String, Double)())
        Dim strTags As NamedValue(Of VectorTagged(Of Dictionary(Of String, Double)))() =
            LinqAPI.Exec(Of NamedValue(Of VectorTagged(Of Dictionary(Of String, Double)))) <=
 _
            From x As VectorTagged(Of Dictionary(Of String, Double))
            In data.AsParallel
            Select New NamedValue(Of VectorTagged(Of Dictionary(Of String, Double))) With {
                .Name = x.Tag.GetJson,
                .x = x
            }

        Call "Load data complete!".__DEBUG_ECHO

        Dim uid As New Uid
        Dim datasets As EntityLDM() = strTags.ToArray(
            Function(x) New EntityLDM With {
                .Name = "boot" & uid.Plus,
                .Properties = x.x.Tag _
                    .SeqIterator _
                    .ToDictionary(Function(o) CStr(o.i),
                                  Function(o) o.obj)   ' 在这里使用特征向量作为属性来进行聚类操作
        })

        Call "Creates dataset complete!".__DEBUG_ECHO

        Dim clusters As EntityLDM() = datasets.TreeCluster(parallel:=True, [stop]:=[stop])
        Dim out As New Dictionary(Of NamedValue(Of Double()), Dictionary(Of String, Double)())
        Dim raw = (From x As NamedValue(Of VectorTagged(Of Dictionary(Of String, Double)))
                   In strTags
                   Select x
                   Group x By x.Name Into Group) _
                        .ToDictionary(Function(x) x.Name,
                                      Function(x) x.Group.ToArray)
        Dim treeParts = clusters.Partitioning(partitionDepth)

        For Each cluster As Partition In treeParts
            Dim key As New NamedValue(Of Double()) With {
                .Name = cluster.Tag,
                .x = cluster.PropertyMeans
            } ' out之中的key
            Dim tmp As New List(Of Dictionary(Of String, Double))   ' out之中的value

            For Each x As EntityLDM In cluster.members
                Dim rawKey As String = x.Properties.Values.ToArray.GetJson
                Dim rawParams = raw(rawKey).ToArray(Function(o) o.x.value)

                tmp += rawParams
            Next

            out(key) = tmp.ToArray
        Next

        Return out
    End Function
End Module
