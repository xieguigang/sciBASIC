#Region "Microsoft.VisualBasic::a97670ed0d5849f8ec54d55ab3f1f99f, sciBASIC#\Data_science\DataMining\DataMining\Clustering\KMeans\EntityModels\Entity.vb"

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

    '   Total Lines: 79
    '    Code Lines: 51
    ' Comment Lines: 20
    '   Blank Lines: 8
    '     File Size: 3.09 KB


    '     Class ClusterEntity
    ' 
    '         Properties: cluster, uid
    ' 
    '         Function: (+2 Overloads) ToDataModel, ToString
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
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace KMeans

    ''' <summary>
    ''' 计算所使用的对象实例实体模型
    ''' </summary>
    Public Class ClusterEntity : Inherits EntityBase(Of Double)
        Implements INamedValue

        <XmlAttribute> Public Property uid As String Implements INamedValue.Key
        <XmlAttribute> Public Property cluster As Integer

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
    End Class
End Namespace
