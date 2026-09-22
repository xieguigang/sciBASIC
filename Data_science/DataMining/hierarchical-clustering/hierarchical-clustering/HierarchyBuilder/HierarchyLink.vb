#Region "Microsoft.VisualBasic::719af8bb82d06b412bbea62624e82bc8, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\HierarchyBuilder\HierarchyLink.vb"

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

    '   Total Lines: 76
    '    Code Lines: 47 (61.84%)
    ' Comment Lines: 16 (21.05%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 13 (17.11%)
    '     File Size: 2.80 KB


    '     Class HierarchyLink
    ' 
    '         Properties: HashKey
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __compareTo, compareTo, LessThan, ToString
    ' 
    '     Module LinkHashCode
    ' 
    '         Function: (+2 Overloads) hashCodePair
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.HashMaps

Namespace Hierarchy

    Public Class HierarchyLink : Implements IComparable, IComparable(Of HierarchyLink)

        Public ReadOnly Tree As HierarchyTreeNode
        Public ReadOnly Property HashKey As ULong

        Friend removed As Boolean = False

        Sub New(p As HierarchyTreeNode)
            Tree = p
            HashKey = p.hashCodePair
        End Sub

        Public Function compareTo(o As HierarchyLink) As Integer Implements IComparable(Of HierarchyLink).CompareTo
            Return Tree.compareTo(o.Tree)
        End Function

        Public Overrides Function ToString() As String
            Return HashKey
        End Function

        Private Function __compareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Return compareTo(obj)
        End Function

        Public Shared Function LessThan() As Func(Of HierarchyLink, HierarchyLink, Boolean)
            Return Function(a, b)
                       If a.compareTo(b) < 0 Then
                           Return True
                       Else
                           Return False
                       End If
                   End Function
        End Function
    End Class

    Module LinkHashCode

        ''' <summary>
        ''' Compute some kind of unique ID for a given cluster pair. </summary>
        ''' <returns> The ID </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function hashCodePair(link As HierarchyTreeNode) As ULong
            Return hashCodePair(link.Left(), link.Right())
        End Function

        ''' <summary>
        ''' 由两个簇的唯一整数 <see cref="Cluster.Id"/> 组合出的链接键。
        ''' 
        ''' <para>
        ''' 使用 <c>(min &lt;&lt; 32) | max</c> 的位拼接：两个 ID 均为 &lt; 2^31 的非负整数，
        ''' 因此不同的簇对所产生的结果必然不同（无哈希冲突）。
        ''' </para>
        ''' <para>
        ''' 相比旧实现基于 <see cref="String.GetHashCode"/> 的簇名哈希 + <c>String.CompareTo</c> 比较，
        ''' 既消除了每次链接查找的字符串开销，也避免了重名簇共享链接键的风险。
        ''' </para>
        ''' </summary>
        Public Function hashCodePair(lCluster As Cluster, rCluster As Cluster) As ULong
            Dim lId As ULong = CULng(lCluster.Id)
            Dim rId As ULong = CULng(rCluster.Id)

            If lId <= rId Then
                Return (lId << 32) Or rId
            Else
                Return (rId << 32) Or lId
            End If
        End Function
    End Module
End Namespace
