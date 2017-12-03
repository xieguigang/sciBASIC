#Region "Microsoft.VisualBasic::f3f9e5f9a7cd6653d88144e6d0a6e13a, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\KMeans\EntityModels\csv.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO

Namespace KMeans

    ''' <summary>
    ''' 存储在Csv文件里面的数据模型，近似等价于<see cref="DataSet"/>，只不过多带了一个用来描述cluster的<see cref="Cluster"/>属性标签
    ''' </summary>
    Public Class EntityClusterModel : Inherits DataSet
        Implements INamedValue

        ''' <summary>
        ''' 聚类结果的类编号
        ''' </summary>
        ''' <returns></returns>
        Public Property Cluster As String

        Public Sub Add(key As String, n As Double)
            Call Properties.Add(key, n)
        End Sub

        Public Overrides Function ToString() As String
            Return ID
        End Function

        Public Shared Function Load(path As String) As Entity()
            Return Entity.Load(path)
        End Function

        Public Function ToModel() As Entity
            Return New Entity With {
                .uid = ID,
                .Properties = Properties.Values.ToArray
            }
        End Function

        Public Shared Function Load(path As String, map As String) As EntityClusterModel()
            Dim maps As New Dictionary(Of String, String) From {
                {map, NameOf(EntityClusterModel.ID)}
            }
            Return path.LoadCsv(Of EntityClusterModel)(maps:=maps).ToArray
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
