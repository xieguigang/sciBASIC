#Region "Microsoft.VisualBasic::3518bbc4a76ff4ccaa57a210bbadcbda, Data_science\Visualization\Visualization\Extensions.vb"

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

    '   Total Lines: 101
    '    Code Lines: 74 (73.27%)
    ' Comment Lines: 17 (16.83%)
    '    - Xml Docs: 88.24%
    ' 
    '   Blank Lines: 10 (9.90%)
    '     File Size: 4.07 KB


    ' Module Extensions
    ' 
    '     Function: BuildTransactions, CastTo, ClusterResultFastLoad, Load, ToEntityObject
    '               ToEntityObjects
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module Extensions

    ''' <summary>
    ''' 假若有很多个节点的话，则进行聚类会得到很多的属性，但是想要加载的数据
    ''' 只有ID和cluster结果等非附加属性部分，则这个时候可以使用这个函数进行快速加载
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <returns></returns>
    Public Iterator Function ClusterResultFastLoad(path$) As IEnumerable(Of EntityClusterModel)
        Using reader As StreamReader = path.OpenReader
            Dim header As New RowObject(reader.ReadLine)
            Dim cluster% = header.IndexOf(NameOf(EntityClusterModel.Cluster))
            Dim name% = header.IndexOf(NameOf(EntityClusterModel.ID))
            Dim row As New Value(Of RowObject)

            Do While Not reader.EndOfStream
                Yield New EntityClusterModel With {
                    .ID = (row = New RowObject(reader.ReadLine))(name),
                    .Cluster = (+row)(cluster)
                }
            Loop
        End Using
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="row">第一个元素为分类，其余元素为属性</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CastTo(row As RowObject) As IntegerEntity
        Dim LQuery = From s As String
                     In row.Skip(1)
                     Select CType(Val(s), Integer) '

        Return New IntegerEntity With {
            .Class = Val(row.First),
            .entityVector = LQuery.ToArray
        }
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ToEntityObjects(dataset As IEnumerable(Of EntityClusterModel)) As IEnumerable(Of EntityObject)
        Return dataset.Select(AddressOf ToEntityObject)
    End Function

    Private Function ToEntityObject(data As EntityClusterModel) As EntityObject
        Dim cluster As New NamedValue(Of String) With {
                           .Name = NameOf(data.Cluster),
                           .Value = data.Cluster
                       }
        Return New EntityObject With {
            .ID = data.ID,
            .Properties = cluster + data.Properties.AsCharacter
        }
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function BuildTransactions(data As IEnumerable(Of EntityObject)) As IEnumerable(Of Transaction)
        Return data _
            .SafeQuery _
            .Select(Function(t)
                        Return New Transaction With {
                            .Name = t.ID,
                            .Items = t.Properties.Values.ToArray
                        }
                    End Function)
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="path">Csv文件之中除了第一列是名称标识符，其他的都必须是该实体对象的属性</param>
    ''' <returns></returns>
    Public Function Load(path As String, Optional map As String = "Name") As ClusterEntity()
        Dim data = DataSet.LoadDataSet(path, map).ToArray
        Dim source As ClusterEntity() = data _
                .Select(Function(x)
                            Return New ClusterEntity With {
                                .uid = x.ID,
                                .entityVector = x.Properties.Values.ToArray
                            }
                        End Function) _
                .ToArray

        Return source
    End Function
End Module
