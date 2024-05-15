#Region "Microsoft.VisualBasic::c895a168c2404b2ac4d280f4e9862519, Data_science\DataMining\DataMining\Clustering\KMeans\EntityModels\EntityClusterModel.vb"

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

    '   Total Lines: 82
    '    Code Lines: 57
    ' Comment Lines: 16
    '   Blank Lines: 9
    '     File Size: 3.39 KB


    '     Class EntityClusterModel
    ' 
    '         Properties: Cluster, ID
    ' 
    '         Function: FromDataSet, (+2 Overloads) FromModel, ToModel, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
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
        Implements IReadOnlyId

        Public Property ID As String Implements INamedValue.Key, IReadOnlyId.Identity

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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromDataSet(Of T As {INamedValue, DynamicPropertyBase(Of Double)})(data As T) As EntityClusterModel
            Return New EntityClusterModel With {
                .ID = data.Key,
                .Properties = data.Properties
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Iterator Function FromModel(Of T As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of T)) As IEnumerable(Of EntityClusterModel)
            For Each xi As T In data
                Yield New EntityClusterModel With {
                    .ID = xi.Key,
                    .Properties = xi.Properties
                }
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Iterator Function FromModel(data As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double)))) As IEnumerable(Of EntityClusterModel)
            For Each xi As NamedValue(Of Dictionary(Of String, Double)) In data
                Yield New EntityClusterModel With {
                    .ID = xi.Name,
                    .Properties = xi.Value
                }
            Next
        End Function
    End Class
End Namespace
