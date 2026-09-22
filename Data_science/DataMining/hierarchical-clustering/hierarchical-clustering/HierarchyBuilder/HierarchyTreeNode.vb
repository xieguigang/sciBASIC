#Region "Microsoft.VisualBasic::165febde72287eb5b091e453b57c4704, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\HierarchyBuilder\HierarchyTreeNode.vb"

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

    '   Total Lines: 119
    '    Code Lines: 67 (56.30%)
    ' Comment Lines: 29 (24.37%)
    '    - Xml Docs: 34.48%
    ' 
    '   Blank Lines: 23 (19.33%)
    '     File Size: 4.12 KB


    '     Class HierarchyTreeNode
    ' 
    '         Properties: Left, LinkageDistance, Right
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Agglomerate, compareTo, GetOtherCluster, Reverse, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text

'
'*****************************************************************************
' Copyright 2013 Lars Behnke
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'

Namespace Hierarchy

    Public Class HierarchyTreeNode : Implements IComparable(Of HierarchyTreeNode)

        Private Shared globalIndex As Long = 0

        Public Sub New()
        End Sub

        Public Sub New(left As Cluster, right As Cluster, distance As Double)
            Me.Left = left
            Me.Right = right
            LinkageDistance = distance
        End Sub

        ''' <summary>
        ''' returns the partner of the given cluster
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns>
        ''' if the given c is left then returns partner right, otherwise returns partner left
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetOtherCluster(c As Cluster) As Cluster
            Return If(Left Is c, Right, Left)
        End Function

        Public Property Left As Cluster
        Public Property Right As Cluster
        Public Property LinkageDistance As Double

        ''' <returns> 
        ''' a new ClusterPair with the two left/right inverted
        ''' </returns>
        Public Function Reverse() As HierarchyTreeNode
            Return New HierarchyTreeNode(Right(), Left(), LinkageDistance)
        End Function

        Public Function compareTo(o As HierarchyTreeNode) As Integer Implements IComparable(Of HierarchyTreeNode).CompareTo
            Dim result As Integer

            If o Is Nothing OrElse o.LinkageDistance = 0 Then
                result = -1
            ElseIf LinkageDistance = 0 Then
                result = 1
            Else
                result = LinkageDistance.CompareTo(o.LinkageDistance)
            End If

            Return result
        End Function

        Public Function Agglomerate(name As String) As Cluster
            If name Is Nothing Then
                globalIndex += 1
                name = "clstr#" & (globalIndex)
            End If

            Dim cluster As New Cluster(name) With {
                .Distance = New Distance(LinkageDistance)
            }

            cluster.AddChild(Left)
            cluster.AddChild(Right)

            Left.Parent = cluster
            Right.Parent = cluster

            cluster.Distance.Weight = Left.WeightValue + Right.WeightValue
            ' 叶名不再在每次合并时 eager 拷贝（原实现累计 O(n^2) 时间与内存，且 LeafNames 极少被使用，
            ' 现改为首次访问时惰性计算）；这里改为增量维护叶节点数量缓存，使 Leafs 为 O(1) 读取
            cluster.LeafCount = Left.Leafs + Right.Leafs

            Return cluster
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            If Left IsNot Nothing Then
                sb.Append(Left.Name)
            End If

            If Right IsNot Nothing Then
                If sb.Length > 0 Then
                    sb.Append(" + ")
                End If
                sb.Append(Right.Name)
            End If

            Call sb _
                .Append(" : ") _
                .Append(LinkageDistance)

            Return sb.ToString()
        End Function
    End Class
End Namespace
