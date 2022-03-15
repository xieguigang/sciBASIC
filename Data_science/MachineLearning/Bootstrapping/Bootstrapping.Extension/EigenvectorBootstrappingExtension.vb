#Region "Microsoft.VisualBasic::5d04a3c3c5820c0a96164068e5711634, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Bootstrapping.Extension\EigenvectorBootstrappingExtension.vb"

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


    ' Code Statistics:

    '   Total Lines: 80
    '    Code Lines: 63
    ' Comment Lines: 6
    '   Blank Lines: 11
    '     File Size: 3.62 KB


    ' Module EigenvectorBootstrappingExtension
    ' 
    '     Function: BinaryKMeans
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Data.visualize.KMeans
Imports Microsoft.VisualBasic.Data.visualize.KMeans.Tree
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
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
                .Value = x
            }

        Call "Load data complete!".__DEBUG_ECHO

        Dim uid As New Uid
        Dim datasets As EntityClusterModel() = strTags _
            .Select(Function(x)
                        Return New EntityClusterModel With {
                            .ID = "boot" & uid.Plus,
                            .Properties = x.Value.Tag _
                                .SeqIterator _
                                .ToDictionary(Function(o) CStr(o.i),
                                              Function(o)
                                                  ' 在这里使用特征向量作为属性来进行聚类操作
                                                  Return o.value
                                              End Function)
                        }
                    End Function) _
            .ToArray

        Call "Creates dataset complete!".__DEBUG_ECHO

        Dim clusters As EntityClusterModel() = datasets.TreeCluster(parallel:=True, [stop]:=[stop])
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
                .Value = cluster.PropertyMeans
            } ' out之中的key
            Dim tmp As New List(Of Dictionary(Of String, Double))   ' out之中的value

            For Each x As EntityClusterModel In cluster.members
                Dim rawKey As String = x.Properties.Values.ToArray.GetJson
                Dim rawParams = raw(rawKey).Select(Function(o) o.Value.Value)

                tmp += rawParams
            Next

            out(key) = tmp.ToArray
        Next

        Return out
    End Function
End Module
