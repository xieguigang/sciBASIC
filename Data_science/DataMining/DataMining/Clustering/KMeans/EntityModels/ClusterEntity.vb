#Region "Microsoft.VisualBasic::d60bbd5d9d698fbbc7fb1c926aaa8130, Data_science\DataMining\DataMining\Clustering\KMeans\EntityModels\ClusterEntity.vb"

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

    '   Total Lines: 152
    '    Code Lines: 79
    ' Comment Lines: 57
    '   Blank Lines: 16
    '     File Size: 5.46 KB


    '     Class ClusterEntity
    ' 
    '         Properties: cluster, entityVector, uid
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: Equals, (+2 Overloads) ToDataModel, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace KMeans

    ''' <summary>
    ''' A tagged numeric vector
    ''' </summary>
    ''' <remarks>
    ''' uid -- feature_vector
    ''' 
    ''' (计算所使用的对象实例实体模型)
    ''' </remarks>
    Public Class ClusterEntity : Inherits EntityBase(Of Double)
        Implements INamedValue
        Implements IVector
        Implements IClusterPoint
        Implements IReadOnlyId

        ''' <summary>
        ''' the unique reference id of current entity object
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property uid As String Implements INamedValue.Key, IReadOnlyId.Identity
        ''' <summary>
        ''' the cluster class label
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property cluster As Integer Implements IClusterPoint.Cluster

        ''' <summary>
        ''' the point data vector
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("v")>
        Public Overrides Property entityVector As Double() Implements IVector.Data

        Sub New()
        End Sub

        Sub New(v As IVector)
            entityVector = v.Data
        End Sub

        Sub New(id As String, v As IVector)
            Call Me.New(id, v.Data.ToArray)
        End Sub

        ''' <summary>
        ''' Create a new entity point data
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="data"></param>
        Sub New(id As String, data As Double())
            uid = id
            entityVector = data
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{entityVector.Select(Function(x) x.ToString("G3")).JoinBy(", ")}]"
        End Function

        ''' <summary>
        ''' 使用index序列编号来作为属性名称
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToDataModel() As EntityClusterModel
            Return New EntityClusterModel With {
                .ID = uid,
                .Cluster = cluster,
                .Properties = entityVector _
                    .SeqIterator _
                    .ToDictionary(Function(x) CStr(x.i),
                                  Function(x)
                                      Return x.value
                                  End Function)
            }
        End Function

        ''' <summary>
        ''' check all vector elements equals to another
        ''' </summary>
        ''' <param name="another"></param>
        ''' <returns></returns>
        Public Overloads Function Equals(another As ClusterEntity) As Boolean
            Return VectorEqualityComparer.VectorEqualsToAnother(entityVector, another.entityVector)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="maps">名称字符串向量应该是和<see cref="entityVector"/>属性向量等长的</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToDataModel(maps As String()) As EntityClusterModel
            Return New EntityClusterModel With {
                .ID = uid,
                .Cluster = cluster,
                .Properties = entityVector _
                    .SeqIterator _
                    .ToDictionary(Function(x) maps(x.i),
                                  Function(x)
                                      Return x.value
                                  End Function)
            }
        End Function

        ''' <summary>
        ''' 值相等判断
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(a As ClusterEntity, b As ClusterEntity) As Boolean
            Return a.uid.TextEquals(b.uid) AndAlso VectorEqualityComparer.VectorEqualsToAnother(a.entityVector, b.entityVector)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(a As ClusterEntity, b As ClusterEntity) As Boolean
            Return Not a = b
        End Operator

        ''' <summary>
        ''' get the class label of current point
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        Public Shared Narrowing Operator CType(c As ClusterEntity) As Integer
            Return c.cluster
        End Operator

        ''' <summary>
        ''' get data vector <see cref="entityVector"/>.
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        Public Shared Narrowing Operator CType(c As ClusterEntity) As Double()
            Return c.entityVector
        End Operator
    End Class
End Namespace
