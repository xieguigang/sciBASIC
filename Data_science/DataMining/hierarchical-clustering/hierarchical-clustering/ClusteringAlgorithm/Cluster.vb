﻿#Region "Microsoft.VisualBasic::0444cc5817effe7e932f28332d44cae3, G:/GCModeller/src/runtime/sciBASIC#/Data_science/DataMining/hierarchical-clustering/hierarchical-clustering//ClusteringAlgorithm/Cluster.vb"

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

    '   Total Lines: 172
    '    Code Lines: 107
    ' Comment Lines: 40
    '   Blank Lines: 25
    '     File Size: 5.14 KB


    ' Class Cluster
    ' 
    '     Properties: Children, Distance, DistanceValue, isLeaf, LeafNames
    '                 Leafs, Name, Parent, TotalDistance, WeightValue
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: contains, CountLeafs, Equals, GetHashCode, OrderLeafs
    '               ToString
    ' 
    '     Sub: AddChild, AddLeafName, AppendLeafNames
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy

'
'*****************************************************************************
' Copyright 2013 Lars Behnke
' <p/>
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' <p/>
' http://www.apache.org/licenses/LICENSE-2.0
' <p/>
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'

Public Class Cluster : Implements INamedValue

    Public Property Distance As Distance

    Public ReadOnly Property WeightValue As Double
        Get
            Return Distance.Weight
        End Get
    End Property

    ''' <summary>
    ''' value of <see cref="Distance"/>
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property DistanceValue As Double
        Get
            Return Distance.Distance
        End Get
    End Property

    Public Property Parent As Cluster
    ''' <summary>
    ''' 名称是唯一的？
    ''' </summary>
    ''' <returns></returns>
    Public Property Name As String Implements INamedValue.Key
    Public ReadOnly Property Children As IList(Of Cluster)
    Public ReadOnly Property LeafNames As List(Of String)

    ''' <summary>
    ''' 是否是一个叶节点？
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property isLeaf As Boolean
        Get
            Return Children.Count = 0
        End Get
    End Property

    ''' <summary>
    ''' 计算出所有的叶节点的总数，包括自己的child的叶节点
    ''' </summary>
    ''' <returns></returns>
    ''' 
    Public ReadOnly Property Leafs() As Integer
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return CountLeafs(Me, 0)
        End Get
    End Property

    Public ReadOnly Property TotalDistance As Double
        Get
            Dim dist As Double = If(Distance Is Nothing, 0, Distance.Distance)
            If Children.Count > 0 Then
                dist += Children(0).TotalDistance
            End If
            Return dist
        End Get
    End Property

    Public Sub New(name$)
        Me.Name = name
        LeafNames = New List(Of String)
        Children = New List(Of Cluster)
        Distance = New Distance
    End Sub

    Public Sub AddLeafName(lname$)
        LeafNames.Add(lname)
    End Sub

    Public Sub AppendLeafNames(lnames As IEnumerable(Of String))
        LeafNames.AddRange(lnames)
    End Sub

    Public Sub AddChild(cluster As Cluster)
        Children.Add(cluster)
    End Sub

    Public Function contains(cluster As Cluster) As Boolean
        Return Children.Contains(cluster)
    End Function

    ''' <summary>
    ''' get the cluster entity plot orders from this function
    ''' </summary>
    ''' <returns></returns>
    Public Function OrderLeafs() As String()
        If Children.IsNullOrEmpty Then
            Return New String() {Name}
        Else
            Dim orders = Children.OrderBy(Function(c) c.Leafs).ToArray
            Dim names As New List(Of String)

            For Each node In orders
                names.AddRange(node.OrderLeafs)
            Next

            Return names.ToArray
        End If
    End Function

    Public Overrides Function ToString() As String
        If isLeaf Then
            Return "Leaf " & Name
        Else
            Return "Cluster " & Name
        End If
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean
        If obj Is Nothing Then
            Return False
        End If
        If Me Is obj Then
            Return True
        End If

        If Me.GetType() IsNot obj.GetType() Then
            Return False
        End If

        Dim other As Cluster = CType(obj, Cluster)

        If Name Is Nothing Then
            If other.Name IsNot Nothing Then
                Return False
            End If
        ElseIf Not Name.Equals(other.Name) Then
            Return False
        End If

        Return True
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return If(Name Is Nothing, 0, Name.GetHashCode())
    End Function

    ''' <summary>
    ''' 对某一个节点的所有的叶节点进行计数
    ''' </summary>
    ''' <param name="node"></param>
    ''' <param name="count"></param>
    ''' <returns></returns>
    Public Shared Function CountLeafs(node As Cluster, count As Integer) As Integer
        If node.isLeaf Then count += 1
        For Each child As Cluster In node.Children
            count += child.Leafs()
        Next
        Return count
    End Function
End Class
