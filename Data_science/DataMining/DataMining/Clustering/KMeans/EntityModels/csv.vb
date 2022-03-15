#Region "Microsoft.VisualBasic::c7bcf801b98cdd60f2e55a666bd97d5e, sciBASIC#\Data_science\DataMining\DataMining\Clustering\KMeans\EntityModels\csv.vb"

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

    '   Total Lines: 66
    '    Code Lines: 43
    ' Comment Lines: 15
    '   Blank Lines: 8
    '     File Size: 2.63 KB


    '     Class EntityClusterModel
    ' 
    '         Properties: Cluster, ID
    ' 
    '         Function: FromDataSet, FromModel, ToModel, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace KMeans

    ''' <summary>
    ''' 存储在Csv文件里面的数据模型，近似等价于csv DataSet对象，
    ''' 只不过多带了一个用来描述cluster的<see cref="Cluster"/>
    ''' 属性标签
    ''' </summary>
    Public Class EntityClusterModel : Inherits DynamicPropertyBase(Of Double)
        Implements INamedValue

        Public Property ID As String Implements INamedValue.Key

        ''' <summary>
        ''' 聚类结果的类编号
        ''' </summary>
        ''' <returns></returns>
        Public Property Cluster As String

        ''' <summary>
        ''' 用于生成聚类所需要的数据集，所以通过这个函数所构建的数据集对象的<see cref="Cluster"/>属性值都是空的
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Public Shared Function FromDataSet(Of T As {INamedValue, DynamicPropertyBase(Of Double)})(data As T) As EntityClusterModel
            Return New EntityClusterModel With {
                .ID = data.Key,
                .Properties = data.Properties
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Cluster}] {ID}"
        End Function

        Public Function ToModel(Optional projection As String() = Nothing) As ClusterEntity
            If projection.IsNullOrEmpty Then
                Return New ClusterEntity With {
                    .uid = ID,
                    .entityVector = Properties.Values.ToArray
                }
            Else
                Return New ClusterEntity With {
                    .uid = ID,
                    .entityVector = projection _
                        .Select(Function(key)
                                    Return Properties.TryGetValue(key)
                                End Function) _
                        .ToArray
                }
            End If
        End Function

        Public Shared Iterator Function FromModel(data As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double)))) As IEnumerable(Of EntityClusterModel)
            For Each x As NamedValue(Of Dictionary(Of String, Double)) In data
                Yield New EntityClusterModel With {
                    .ID = x.Name,
                    .Properties = x.Value
                }
            Next
        End Function
    End Class
End Namespace
