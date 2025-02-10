#Region "Microsoft.VisualBasic::06df92ed3e784008ae4f006c9c8d018a, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Tree\TreeNodeBase.vb"

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

    '   Total Lines: 186
    '    Code Lines: 96 (51.61%)
    ' Comment Lines: 63 (33.87%)
    '    - Xml Docs: 90.48%
    ' 
    '   Blank Lines: 27 (14.52%)
    '     File Size: 6.37 KB


    '     Class TreeNodeBase
    ' 
    '         Properties: ChildNodes, FullyQualifiedName, IsLeaf, IsRoot, Name
    '                     Parent
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: __travelInternal, GenericEnumerator, GetLeafNodes, GetNonLeafNodes, GetRootNode
    '                   MaxTravelDepth
    ' 
    '         Sub: AddChild, AddChildren, ChildCountsTravel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataStructures.Tree

    ''' <summary>
    ''' Generic Tree Node base class
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>https://www.codeproject.com/Articles/345191/Simple-Generic-Tree</remarks>
    Public MustInherit Class TreeNodeBase(Of T As {Class, ITreeNode(Of T)})
        Implements ITreeNode(Of T), Enumeration(Of T)

        ''' <summary>
        ''' Name
        ''' </summary>
        Public Property Name() As String

        ''' <summary>
        ''' Parent
        ''' </summary>
        Public Property Parent() As T Implements ITreeNode(Of T).Parent

        ''' <summary>
        ''' Children
        ''' </summary>
        Public ReadOnly Property ChildNodes() As IReadOnlyCollection(Of T) Implements ITreeNode(Of T).ChildNodes
            Get
                Return m_childs
            End Get
        End Property

        Protected m_childs As New List(Of T)

        ''' <summary>
        ''' Me/this
        ''' </summary>
        Public MustOverride ReadOnly Property MySelf() As T

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Sub New(name As String)
            Me.Name = name
        End Sub

        ''' <summary>
        ''' True if a Leaf Node
        ''' </summary>
        ''' <value>
        ''' True for <see cref="ChildNodes"/> contains no data.
        ''' </value>
        Public ReadOnly Property IsLeaf() As Boolean Implements ITreeNode(Of T).IsLeaf
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ChildNodes.IsNullOrEmpty
            End Get
        End Property

        ''' <summary>
        ''' True if the Root of the Tree
        ''' </summary>
        Public ReadOnly Property IsRoot() As Boolean Implements ITreeNode(Of T).IsRoot
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Parent Is Nothing
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function MaxTravelDepth() As Integer
            Return __travelInternal(Me.MySelf)
        End Function

        Private Shared Function __travelInternal(child As T) As Integer
            Dim l As New List(Of Integer) From {0}

            For Each c As T In child.ChildNodes
                l.Add(__travelInternal(child:=c))
            Next

            ' 最后的 +1 是因为当前的对象自己本身也是一层节点
            Return l.Max + 1
        End Function

        ''' <summary>
        ''' List of Leaf Nodes
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetLeafNodes() As List(Of T)
            Return ChildNodes.Where(Function(x) x.IsLeaf).AsList()
        End Function

        ''' <summary>
        ''' List of Non Leaf Nodes
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetNonLeafNodes() As List(Of T)
            Return ChildNodes.Where(Function(x) Not x.IsLeaf).AsList()
        End Function

        ''' <summary>
        ''' Get the Root Node of the Tree
        ''' </summary>
        Public Function GetRootNode() As T Implements ITreeNode(Of T).GetRootNode
            If Parent Is Nothing Then
                Return MySelf
            End If

            Return Parent.GetRootNode()
        End Function

        ''' <summary>
        ''' Dot separated name from the Root to this Tree Node
        ''' </summary>
        Public ReadOnly Property FullyQualifiedName() As String Implements ITreeNode(Of T).FullyQualifiedName
            Get
                If Parent Is Nothing Then
                    Return Name
                End If

                Return String.Format("{0}.{1}", Parent.FullyQualifiedName(), Name)
            End Get
        End Property

        ''' <summary>
        ''' Add a Child Tree Node
        ''' </summary>
        ''' <param name="child"></param>
        ''' <remarks>
        ''' 1. hook <see cref="MySelf"/> to <see cref="Parent"/>
        ''' 2. add <paramref name="child"/> to <see cref="ChildNodes"/>
        ''' </remarks>
        Public Sub AddChild(child As T) Implements ITreeNode(Of T).Add
            m_childs.Add(child)
            child.Parent = MySelf
        End Sub

        ''' <summary>
        ''' Add a collection of child Tree Nodes
        ''' </summary>
        ''' <param name="children"></param>
        Public Sub AddChildren(children As IEnumerable(Of T))
            For Each child As T In children
                Call AddChild(child)
            Next
        End Sub

        Public Sub ChildCountsTravel(distribute As Dictionary(Of String, Double), Optional getID As Func(Of T, String) = Nothing) Implements ITreeNode(Of T).ChildCountsTravel
            Dim count As Double = ChildNodes.Count
            Dim childCounts As New Dictionary(Of String, Double)

            If getID Is Nothing Then
                getID = Function(x) x.FullyQualifiedName
            End If

            For Each child As T In ChildNodes
                ' 首先进行递归visit，之后才会有计数数据
                Call childCounts.Clear()
                Call child.ChildCountsTravel(childCounts, getID)
                For Each counts In childCounts
                    Call distribute.Add(counts.Key, counts.Value)
                    count += counts.Value
                Next
            Next

            Dim key$ = getID(MySelf)
            Call distribute.Add(key, count)
        End Sub

        ''' <summary>
        ''' enumerates all childs in current tree node
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GenericEnumerator() As IEnumerator(Of T) Implements Enumeration(Of T).GenericEnumerator
            For Each child As T In Me.EnumerateAllChilds
                Yield child
            Next
        End Function
    End Class
End Namespace
