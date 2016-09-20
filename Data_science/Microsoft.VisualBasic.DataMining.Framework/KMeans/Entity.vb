#Region "Microsoft.VisualBasic::a4df46b937de237393cc373c17e8c74e, ..\visualbasic_App\Microsoft.VisualBasic.DataMining.Framework\KMeans\Entity.vb"

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
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Linq

Namespace KMeans

    ''' <summary>
    ''' 存储在Csv文件里面的数据模型
    ''' </summary>
    Public Class EntityLDM : Inherits DynamicPropertyBase(Of Double)
        Implements sIdEnumerable

        Public Property Name As String Implements sIdEnumerable.Identifier

        <Meta(GetType(Double))>
        Public Overrides Property Properties As Dictionary(Of String, Double)
            Get
                Return MyBase.Properties
            End Get
            Set(value As Dictionary(Of String, Double))
                MyBase.Properties = value
            End Set
        End Property

        Public Property Cluster As String

        Public Sub Add(key As String, n As Double)
            Call Properties.Add(key, n)
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Shared Function Load(path As String) As Entity()
            Return Entity.Load(path)
        End Function

        Public Function ToModel() As Entity
            Return New Entity With {
                .uid = Name,
                .Properties = Properties.Values.ToArray
            }
        End Function

        Public Shared Function Load(path As String, map As String) As EntityLDM()
            Dim maps As New Dictionary(Of String, String) From {{map, NameOf(EntityLDM.Name)}}
            Return path.LoadCsv(Of EntityLDM)(maps:=maps).ToArray
        End Function
    End Class

    ''' <summary>
    ''' 计算所使用的对象实例实体模型
    ''' </summary>
    Public Class Entity : Inherits EntityBase(Of Double)
        Implements sIdEnumerable

        Public Property uid As String Implements sIdEnumerable.Identifier

        Public Overrides Function ToString() As String
            Return $"{uid}  ({Length} Properties)"
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="path">Csv文件之中除了第一列是名称标识符，其他的都必须是该实体对象的属性</param>
        ''' <returns></returns>
        Public Shared Function Load(path As String, Optional map As String = "Name") As Entity()
            Dim data As EntityLDM() = EntityLDM.Load(path, map)
            Dim source As Entity() = data.ToArray(
                Function(x) New Entity With {
                    .uid = x.Name,
                    .Properties = x.Properties.Values.ToArray})
            Return source
        End Function

        Public Function ToLDM() As EntityLDM
            Return New EntityLDM With {
                .Name = uid,
                .Properties = Properties _
                    .SeqIterator _
                    .ToDictionary(Function(x) CStr(x.i),
                                  Function(x) x.obj)
            }
        End Function

        Public Function ToLDM(maps As String()) As EntityLDM
            Return New EntityLDM With {
                .Name = uid,
                .Properties = Properties _
                    .SeqIterator _
                    .ToDictionary(Function(x) maps(x.i),
                                  Function(x) x.obj)
            }
        End Function
    End Class
End Namespace
