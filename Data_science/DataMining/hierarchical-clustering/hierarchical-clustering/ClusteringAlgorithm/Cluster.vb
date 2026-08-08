#Region "Microsoft.VisualBasic::5c60731238d2af8bea3a459516dffc8c, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\ClusteringAlgorithm\Cluster.vb"

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

    '   Total Lines: 299
    '    Code Lines: 118 (39.46%)
    ' Comment Lines: 151 (50.50%)
    '    - Xml Docs: 88.74%
    ' 
    '   Blank Lines: 30 (10.03%)
    '     File Size: 13.47 KB


    ' Class Cluster
    ' 
    '     Properties: Children, Distance, DistanceValue, isLeaf, IsRoot
    '                 LeafNames, Leafs, Name, Parent, TotalDistance
    '                 WeightValue
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
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy

'
'*****************************************************************************
' Copyright2013 Lars Behnke
' <p/>
' Licensed under the Apache License, Version2.0 (the "License");
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
''' <summary>
''' Represents a node (cluster) in the hierarchical clustering tree (dendrogram).
''' Each cluster can be a leaf node (containing a single data point) or an internal node
''' (containing child clusters formed by merging). Implements the tree node data structure
''' required for hierarchical clustering and provides named identification via <see cref="INamedValue"/> .
''' </summary>
Public Class Cluster : Implements INamedValue, ITreeNodeData(Of Cluster)

    ''' <summary>
    ''' Gets or sets the <see cref="Distance"/>  object associated with this cluster,
    ''' representing the distance at which this cluster was formed (for internal nodes)
    ''' or the initial distance for leaf nodes.
    ''' </summary>
    ''' <returns>A <see cref="Distance"/>  instance containing the distance value and optional weight.</returns>
    Public Property Distance As Distance

    ''' <summary>
    ''' Gets the weight of this cluster from the underlying <see cref="Distance"/>  object.
    ''' The weight is used in weighted linkage strategies such as WPGMA.
    ''' </summary>
    ''' <returns>A <see cref="Double"/>  value representing the weight of this cluster.</returns>
    Public ReadOnly Property WeightValue As Double
        Get
            Return Distance.Weight
        End Get
    End Property

    ''' <summary>
    ''' Gets the distance value of this cluster from the underlying <see cref="Distance"/>  object.
    ''' For internal nodes, this represents the cophenetic distance at which child clusters were merged.
    ''' For leaf nodes, this is typically zero or the initial distance.
    ''' </summary>
    ''' <returns>A <see cref="Double"/>  value representing the distance associated with this cluster.</returns>
    Public ReadOnly Property DistanceValue As Double
        Get
            Return Distance.Distance
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the parent cluster of this node in the dendrogram.
    ''' A <c>Nothing</c> value indicates this cluster is the root of the tree.
    ''' Implements <see cref="ITreeNodeData(Of Cluster).Parent"/> .
    ''' </summary>
    ''' <returns>The parent <see cref="Cluster"/>  object, or <c>Nothing</c> if this is the root node.</returns>
    Public Property Parent As Cluster Implements ITreeNodeData(Of Cluster).Parent

    ''' <summary>
    ''' Gets or sets the unique name identifier for this cluster.
    ''' The name should be unique among all leaf clusters to ensure correct equality comparisons.
    ''' Implements <see cref="INamedValue.Key"/>  and <see cref="ITreeNodeData(Of Cluster).FullyQualifiedName"/> .
    ''' </summary>
    ''' <returns>A <see cref="String"/>  representing the name of this cluster.</returns>
    Public Property Name As String Implements INamedValue.Key, ITreeNodeData(Of Cluster).FullyQualifiedName

    ''' <summary>
    ''' Gets the read-only collection of child clusters directly under this node.
    ''' An empty collection indicates that this node is a leaf cluster.
    ''' Implements <see cref="ITreeNodeData(Of Cluster).ChildNodes"/> .
    ''' </summary>
    ''' <returns>An <see cref="IReadOnlyCollection(Of Cluster)"/>  containing the direct children of this cluster.</returns>
    Public ReadOnly Property Children As IReadOnlyCollection(Of Cluster) Implements ITreeNodeData(Of Cluster).ChildNodes
        Get
            Return m_childs
        End Get
    End Property

    Dim m_childs As New List(Of Cluster)

    ''' <summary>
    ''' Gets the list of leaf names contained within this cluster's subtree.
    ''' For leaf clusters, this list contains the cluster's own name. For internal clusters,
    ''' it accumulates the names of all descendant leaf nodes.
    ''' </summary>
    ''' <returns>A <see cref="List(Of String)"/>  containing the names of all leaf nodes in this subtree.</returns>
    Public ReadOnly Property LeafNames As List(Of String)

    ''' <summary>
    ''' Gets a value indicating whether this cluster is the root node of the dendrogram.
    ''' A cluster is considered the root if it has no parent or if its parent is itself.
    ''' Implements <see cref="ITreeNodeData(Of Cluster).IsRoot"/> .
    ''' </summary>
    ''' <returns><c>True</c> if this cluster is the root; otherwise, <c>False</c>.</returns>
    Public ReadOnly Property IsRoot As Boolean Implements ITreeNodeData(Of Cluster).IsRoot
        Get
            Return Parent Is Nothing OrElse Parent Is Me
        End Get
    End Property

    ''' <summary>
    ''' Gets a value indicating whether this cluster is a leaf node.
    ''' A cluster is a leaf if it has no children.
    ''' Implements <see cref="ITreeNodeData(Of Cluster).IsLeaf"/> .
    ''' </summary>
    ''' <returns><c>True</c> if this cluster has no children (i.e., it is a leaf node);
    ''' otherwise, <c>False</c>.</returns>
    Public ReadOnly Property isLeaf As Boolean Implements ITreeNodeData(Of Cluster).IsLeaf
        Get
            Return Children.Count = 0
        End Get
    End Property

    ''' <summary>
    ''' Gets the total number of leaf nodes in the subtree rooted at this cluster.
    ''' Recursively counts all descendant leaf nodes, including direct children and their children.
    ''' </summary>
    ''' <returns>An <see cref="Integer"/>  representing the total count of leaf nodes in this subtree.</returns>
    Public ReadOnly Property Leafs() As Integer
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return CountLeafs(Me, 0)
        End Get
    End Property

    ''' <summary>
    ''' Gets the total distance value accumulated along the left-most path from this node
    ''' to its deepest descendant. This is the sum of distances from this node down through
    ''' the first child at each level.
    ''' </summary>
    ''' <returns>A <see cref="Double"/>  value representing the accumulated distance along the
    ''' left-most branch of this subtree.</returns>
    Public ReadOnly Property TotalDistance As Double
        Get
            Dim dist As Double = If(Distance Is Nothing, 0, Distance.Distance)
            If Children.Count > 0 Then
                dist += Children(0).TotalDistance
            End If

            Return dist
        End Get
    End Property

    ''' <summary>
    ''' Initializes a new instance of the <see cref="Cluster"/>  class with the specified name.
    ''' The cluster is created as a leaf node with an empty child list, an empty leaf name list,
    ''' and a default <see cref="Distance"/>  object.
    ''' </summary>
    ''' <param name="name">The unique name identifier for this cluster.</param>
    Public Sub New(name$)
        Me.Name = name
        LeafNames = New List(Of String)
        Distance = New Distance
    End Sub

    ''' <summary>
    ''' Adds a single leaf name to this cluster's list of leaf names.
    ''' This is used to track which original data points are contained within a cluster.
    ''' </summary>
    ''' <param name="lname">The leaf name to add.</param>
    Public Sub AddLeafName(lname$)
        LeafNames.Add(lname)
    End Sub

    ''' <summary>
    ''' Adds a collection of leaf names to this cluster's list of leaf names.
    ''' This is used when merging clusters to aggregate all descendant leaf names.
    ''' </summary>
    ''' <param name="lnames">An enumerable collection of leaf names to append.</param>
    Public Sub AppendLeafNames(lnames As IEnumerable(Of String))
        LeafNames.AddRange(lnames)
    End Sub

    ''' <summary>
    ''' Adds a child cluster to this node's children collection.
    ''' This method is called during the agglomerative clustering process when two clusters are merged.
    ''' </summary>
    ''' <param name="cluster">The child <see cref="Cluster"/>  to add.</param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub AddChild(cluster As Cluster)
        m_childs.Add(cluster)
    End Sub

    ''' <summary>
    ''' Determines whether the specified cluster is a direct child of this cluster.
    ''' </summary>
    ''' <param name="cluster">The <see cref="Cluster"/>  to check for containment.</param>
    ''' <returns><c>True</c> if the specified cluster is a direct child; otherwise, <c>False</c>.</returns>
    Public Function contains(cluster As Cluster) As Boolean
        Return Children.Contains(cluster)
    End Function

    ''' <summary>
    ''' Returns the ordered sequence of leaf node names for plotting a dendrogram or heatmap.
    ''' Leaf clusters are returned in order of increasing leaf count (smallest subtrees first),
    ''' which produces a visually balanced dendrogram layout.
    ''' </summary>
    ''' <returns>An array of <see cref="String"/>  values representing the leaf names in plot order.
    ''' For leaf clusters, returns an array containing only the cluster's own name.</returns>
    ''' <remarks>
    ''' This function is used to obtain the re-ordered labels for data rows when plotting
    ''' a clustered heatmap, ensuring that the dendrogram branches are displayed without crossing.
    ''' </remarks>
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

    ''' <summary>
    ''' Returns a string representation of this cluster.
    ''' Leaf nodes are prefixed with "Leaf" and internal nodes with "Cluster",
    ''' followed by the cluster's name.
    ''' </summary>
    ''' <returns>A <see cref="String"/>  in the format "Leaf {Name}" or "Cluster {Name}".</returns>
    Public Overrides Function ToString() As String
        If isLeaf Then
            Return "Leaf " & Name
        Else
            Return "Cluster " & Name
        End If
    End Function

    ''' <summary>
    ''' Determines whether the specified object is equal to the current cluster.
    ''' Equality is determined solely by the <see cref="Name"/>  property.
    ''' </summary>
    ''' <param name="obj">The object to compare with the current cluster.</param>
    ''' <returns><c>True</c> if the specified object is a <see cref="Cluster"/>  with the same
    ''' <see cref="Name"/> ; otherwise, <c>False</c>.</returns>
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

    ''' <summary>
    ''' Returns a hash code for this cluster based on its <see cref="Name"/> .
    ''' </summary>
    ''' <returns>A32-bit signed integer hash code derived from the cluster's name,
    ''' or0 if the name is <c>Nothing</c>.</returns>
    Public Overrides Function GetHashCode() As Integer
        Return If(Name Is Nothing, 0, Name.GetHashCode())
    End Function

    ''' <summary>
    ''' Recursively counts the total number of leaf nodes in the subtree rooted at the specified node.
    ''' This is a helper method used by the <see cref="Leafs"/>  property.
    ''' </summary>
    ''' <param name="node">The root node of the subtree to count leaves for.</param>
    ''' <param name="count">The current accumulated count (used for recursive accumulation).</param>
    ''' <returns>An <see cref="Integer"/>  representing the total number of leaf nodes in the subtree.</returns>
    Public Shared Function CountLeafs(node As Cluster, count As Integer) As Integer
        If node.isLeaf Then count += 1
        For Each child As Cluster In node.Children
            count += child.Leafs()
        Next

        Return count
    End Function
End Class
