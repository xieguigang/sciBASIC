#Region "Microsoft.VisualBasic::5da2667554f7cb39d33e0deadbaf50fc, Data_science\Graph\Model\Tree.vb"

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

    ' Class Tree
    ' 
    '     Properties: Data
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class AbstractTree
    ' 
    '     Properties: Childs, Count, IsLeaf, IsRoot, Parent
    '                 QualifyName
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: (+2 Overloads) CountLeafs, EnumerateChilds, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Tree node with data..(可以直接被使用的树对象类型)
''' </summary>
''' <typeparam name="T"></typeparam>
Public Class Tree(Of T) : Inherits AbstractTree(Of Tree(Of T))

    Public Property Data As T

    Sub New(Optional qualDeli$ = ".")
        MyBase.New(qualDeli)
    End Sub
End Class

Public Class AbstractTree(Of T As AbstractTree(Of T)) : Inherits Vertex

    ''' <summary>
    ''' Childs table, key is the property <see cref="Vertex.Label"/>
    ''' </summary>
    ''' <returns></returns>
    Public Property Childs As Dictionary(Of String, T)
    Public Property Parent As T

    Dim qualDeli$ = "."

    ''' <summary>
    ''' Not null child count in this tree node.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Count As Integer
        Get
            Dim childs = Me.EnumerateChilds _
                .SafeQuery _
                .Where(Function(c) Not c Is Nothing) _
                .ToArray

            If childs.IsNullOrEmpty Then
                ' 自己算一个节点，所以数量总是1的
                Return 1
            Else
                Dim n% = childs.Length

                For Each node In childs
                    ' 如果节点没有childs，则会返回1，因为他自身就是一个节点
                    n += node.Count
                Next

                Return n
            End If
        End Get
    End Property

    Public ReadOnly Property QualifyName As String
        Get
            If Not Parent Is Nothing Then
                Return Parent.QualifyName & qualDeli & Label
            Else
                Return Label
            End If
        End Get
    End Property

    Public ReadOnly Property IsRoot As Boolean
        Get
            Return Parent Is Nothing
        End Get
    End Property

    Public ReadOnly Property IsLeaf As Boolean
        Get
            Return Childs.IsNullOrEmpty
        End Get
    End Property

    Sub New(Optional qualDeli$ = ".")
        Me.qualDeli = qualDeli
    End Sub

    ''' <summary>
    ''' Returns the values of <see cref="Childs"/>
    ''' </summary>
    ''' <returns></returns>
    Public Function EnumerateChilds() As IEnumerable(Of T)
        Return Childs?.Values
    End Function

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
            count += 1
        End If

        For Each child As T In node.EnumerateChilds.SafeQuery
            count += child.CountLeafs()
        Next

        Return count
    End Function
End Class
