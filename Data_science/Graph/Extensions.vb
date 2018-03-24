#Region "Microsoft.VisualBasic::16733df755f642b827c77f884b4bfdb6, Data_science\Graph\Extensions.vb"

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

    ' Module Extensions
    ' 
    '     Function: Add, CreateGraph, DefaultSteps, Grid, Reverse
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq

Public Module Extensions

    <Extension>
    Public Function CreateGraph(Of T)(tree As Tree(Of T)) As Graph
        Return New Graph().Add(tree)
    End Function

    <Extension>
    Private Function Add(Of T)(g As Graph, tree As Tree(Of T)) As Graph
        Dim childs = tree _
            .Childs _
            .SafeQuery _
            .Where(Function(c) Not c Is Nothing)

        Call g.AddVertex(tree)

        For Each child As Tree(Of T) In childs
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
    <Extension>
    Public Function Reverse(edge As VertexEdge) As VertexEdge
        Return New VertexEdge With {
            .U = edge.V,
            .V = edge.U,
            .Weight = edge.Weight
        }
    End Function

    ''' <summary>
    ''' 
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

    <Extension>
    Public Function DefaultSteps(size As SizeF, Optional n% = 50) As DefaultValue(Of SizeF)
        Return New SizeF With {
            .Width = size.Width / 50,
            .Height = size.Height / 50
        }.AsDefault(Function(sz)
                        Return DirectCast(sz, SizeF).IsEmpty
                    End Function)
    End Function
End Module
