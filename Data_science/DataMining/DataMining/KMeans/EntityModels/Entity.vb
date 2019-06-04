#Region "Microsoft.VisualBasic::ecaebdc046433067d6995949a1717047, Data_science\DataMining\DataMining\KMeans\EntityModels\Entity.vb"

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

'     Class Entity
' 
'         Properties: uid
' 
'         Function: (+2 Overloads) ToLDM, ToString
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

    Public Class ClusterEntity : Inherits EntityBase(Of Double)

        Public Property cluster As Integer

        Public Overrides Function ToString() As String
            Return $"[{Properties.JoinBy(", ")}]"
        End Function

    End Class

    ''' <summary>
    ''' 计算所使用的对象实例实体模型
    ''' </summary>
    Public Class Entity : Inherits EntityBase(Of Double)
        Implements INamedValue

        <XmlAttribute>
        Public Property uid As String Implements INamedValue.Key

        Public Overrides Function ToString() As String
            Return $"{uid}  ({Length} Properties)"
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
                .Properties = Properties _
                    .SeqIterator _
                    .ToDictionary(Function(x) CStr(x.i),
                                  Function(x) x.value)
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="maps">名称字符串向量应该是和<see cref="Properties"/>属性向量等长的</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToDataModel(maps As String()) As EntityClusterModel
            Return New EntityClusterModel With {
                .ID = uid,
                .Properties = Properties _
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
        Public Shared Operator =(a As Entity, b As Entity) As Boolean
            Return a.uid.TextEquals(b.uid) AndAlso VectorEqualityComparer.VectorEqualsToAnother(a.Properties, b.Properties)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(a As Entity, b As Entity) As Boolean
            Return Not a = b
        End Operator
    End Class
End Namespace
