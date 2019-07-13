#Region "Microsoft.VisualBasic::ddb4c57bc1f3054fd3cec0e0518dba0c, Data_science\DataMining\DataMining\Clustering\KMeans\EntityModels\Entity.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
            Return $"[{entityVector.JoinBy(", ")}]"
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
                                  Function(x) x.value)
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
                                  Function(x) x.value)
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
