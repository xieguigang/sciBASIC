#Region "Microsoft.VisualBasic::eb1f8a65860720fde6973039e33d7f63, Data_science\Graph\Extensions.vb"

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

    '   Total Lines: 132
    '    Code Lines: 76 (57.58%)
    ' Comment Lines: 41 (31.06%)
    '    - Xml Docs: 95.12%
    ' 
    '   Blank Lines: 15 (11.36%)
    '     File Size: 4.52 KB


    ' Module Extensions
    ' 
    '     Function: Add, BacktrackingRoot, CreateGraph, DefaultSteps, Grid
    '               Reverse, VisitTree
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq

<HideModuleName> Public Module Extensions

    ''' <summary>
    ''' Visit tree node by a given path token
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="tree"></param>
    ''' <param name="path">Collection of <see cref="Tree(Of T,k).Label"/></param>
    ''' <returns></returns>
    <Extension>
    Public Function VisitTree(Of T)(tree As Tree(Of T), path As IEnumerable(Of String)) As Tree(Of T)
        Dim node As Tree(Of T) = tree

        With path.ToArray
            For Each name As String In .ByRef
                ' 如果路径不存在是会报出键名没有找到的错误的
                If Not node.Childs.ContainsKey(name) Then
                    Throw New EntryPointNotFoundException("entry=" & name & $" on path: {path.JoinBy("/")}")
                Else
                    node = node.Childs(name)
                End If
            Next
        End With

        Return node
    End Function

    ''' <summary>
    ''' Travel to root node from current tree node.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="tree"></param>
    ''' <returns></returns>
    <Extension>
    Public Function BacktrackingRoot(Of T)(tree As Tree(Of T)) As Tree(Of T)
        Do While Not tree.IsRoot
            tree = tree.Parent
        Loop

        Return tree
    End Function

    ''' <summary>
    ''' Inline extension of function calls <see cref="Add(Of T, K)(Graph, Tree(Of T, K))"/>
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="K"></typeparam>
    ''' <param name="tree"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateGraph(Of T, K)(tree As Tree(Of T, K)) As Graph
        Return New Graph().Add(tree)
    End Function

    ''' <summary>
    ''' Convert a tree to graph
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="K"></typeparam>
    ''' <param name="g"></param>
    ''' <param name="tree"></param>
    ''' <returns></returns>
    <Extension>
    Private Function Add(Of T, K)(g As Graph, tree As Tree(Of T, K)) As Graph
        Dim childs = tree _
            .EnumerateChilds _
            .SafeQuery _
            .Where(Function(c) Not c Is Nothing)

        Call g.AddVertex(tree)

        For Each child As Tree(Of T, K) In childs
            Call g.Add(child)
            Call g.AddEdge(tree, child)
        Next

        Return g
    End Function

    ''' <summary>
    ''' Swap the location of <see cref="VertexEdge.U"/> and <see cref="VertexEdge.V"/> in <paramref name="edge"/>.
    ''' </summary>
    ''' <param name="edge"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Reverse(edge As VertexEdge) As VertexEdge
        Return New VertexEdge With {
            .U = edge.V,
            .V = edge.U,
            .weight = edge.weight
        }
    End Function

    ''' <summary>
    ''' Generate 2D grid data model by a given [x,y] range value.
    ''' </summary>
    ''' <param name="xy"></param>
    ''' <param name="steps">如果这个参数为空的话，默认分为50份</param>
    ''' <returns></returns>
    <Extension>
    Public Function Grid(xy As (X As DoubleRange, Y As DoubleRange), Optional steps As SizeF = Nothing) As Grid
        With xy
            Dim size As New SizeF(.X.Length, .Y.Length)
            Dim min As New PointF(.X.Min, .Y.Min)
            Dim rect As New RectangleF(min, size)

            Return New Grid(rect, steps Or size.DefaultSteps())
        End With
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function DefaultSteps(size As SizeF, Optional n% = 50) As [Default](Of SizeF)
        Return New SizeF With {
            .Width = size.Width / 50,
            .Height = size.Height / 50
        }.AsDefault(Function(sz)
                        Return DirectCast(sz, SizeF).IsEmpty
                    End Function)
    End Function
End Module
