#Region "Microsoft.VisualBasic::0dcd2f927e6fa580518fa84f1c2fafba, Data_science\Graph\Model\Tree\AbstractTree.vb"

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

    '   Total Lines: 157
    '    Code Lines: 91 (57.96%)
    ' Comment Lines: 47 (29.94%)
    '    - Xml Docs: 89.36%
    ' 
    '   Blank Lines: 19 (12.10%)
    '     File Size: 4.64 KB


    ' Class AbstractTree
    ' 
    '     Properties: ChildNodes, Childs, Count, IsLeaf, IsRoot
    '                 Parent, QualifyName
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: (+2 Overloads) CountLeafs, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree

''' <summary>
''' An abstract tree data model
''' </summary>
''' <typeparam name="T">the data type of the reference key its associated data.</typeparam>
''' <typeparam name="K">the data type of the reference key</typeparam>
<DataContract>
Public Class AbstractTree(Of T As AbstractTree(Of T, K), K) : Inherits Vertex
    Implements ITreeNodeData(Of T)

    ''' <summary>
    ''' Childs table, key is the property <see cref="Vertex.Label"/>
    ''' </summary>
    ''' <returns></returns>
    Public Property Childs As Dictionary(Of K, T)

    ''' <summary>
    ''' a collection of the direct childs in current tree node
    ''' </summary>
    ''' <returns></returns>
    Private ReadOnly Property ChildNodes As IReadOnlyCollection(Of T) Implements ITreeNodeData(Of T).ChildNodes
        Get
            Return Childs.Values
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 在序列化之中会需要忽略掉这个属性，否则会产生无限递归
    ''' </remarks>
    <XmlIgnore>
    <DataIgnored>
    <IgnoreDataMember>
    Public Property Parent As T Implements ITreeNodeData(Of T).Parent

    Dim qualDeli$ = "."

    ''' <summary>
    ''' Get child node with a reference key
    ''' </summary>
    ''' <param name="key"></param>
    ''' <returns></returns>
    Default Public ReadOnly Property Child(key As K) As T
        Get
            Return Childs(key)
        End Get
    End Property

    ''' <summary>
    ''' Not null child count in this tree node.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 请注意，这个属性并不是返回的<see cref="Childs"/>的元素数量，
    ''' 而是返回当前树节点下的所有的子节点的数量
    ''' </remarks>
    ''' 
    <XmlIgnore>
    <DataIgnored>
    <IgnoreDataMember>
    Public ReadOnly Property Count As Integer
        Get
            If Childs.IsNullOrEmpty Then
                ' 自己算一个节点，所以数量总是1的
                Return 1
            Else
                Dim n% = 0

                For Each node As T In ChildNodes
                    ' 如果节点没有childs，则会返回1，因为他自身就是一个节点
                    n += node.Count
                Next

                Return n
            End If
        End Get
    End Property

    <XmlIgnore>
    <DataIgnored>
    <IgnoreDataMember>
    Public Overridable ReadOnly Property QualifyName As String Implements ITreeNodeData(Of T).FullyQualifiedName
        Get
            If Not Parent Is Nothing Then
                Return Parent.QualifyName & qualDeli & label
            Else
                Return label
            End If
        End Get
    End Property

    <XmlIgnore>
    <DataIgnored>
    <IgnoreDataMember>
    Public ReadOnly Property IsRoot As Boolean Implements ITreeNodeData(Of T).IsRoot
        Get
            Return Parent Is Nothing
        End Get
    End Property

    <XmlIgnore>
    <DataIgnored>
    <IgnoreDataMember>
    Public ReadOnly Property IsLeaf As Boolean Implements ITreeNodeData(Of T).IsLeaf
        Get
            Return Childs.IsNullOrEmpty
        End Get
    End Property

    Sub New(Optional qualDeli$ = ".")
        Me.qualDeli = qualDeli
    End Sub

    Sub New()
        Call Me.New(".")
    End Sub

    Public Overrides Function ToString() As String
        Return QualifyName
    End Function

    ''' <summary>
    ''' 计算出所有的叶节点的总数，包括自己的child的叶节点
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function CountLeafs() As Integer
        Return CountLeafs(Me, 0)
    End Function

    ''' <summary>
    ''' 对某一个节点的所有的叶节点进行计数
    ''' </summary>
    ''' <param name="node"></param>
    ''' <param name="count"></param>
    ''' <returns></returns>
    Public Shared Function CountLeafs(node As T, count As Integer) As Integer
        If node.IsLeaf Then
            Return count + 1
        End If

        For Each child As T In node.ChildNodes
            count += child.CountLeafs()
        Next

        Return count
    End Function
End Class
