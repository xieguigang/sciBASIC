#Region "Microsoft.VisualBasic::5d1266535f38b574c765f091b0b95414, Data_science\Visualization\Visualization\Embedding\UmapGraph.vb"

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

    ' Module UmapGraph
    ' 
    '     Function: CreateGraph
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Language
Imports stdNum = System.Math

Public Module UmapGraph

    <Extension>
    Public Function CreateGraph(umap As Umap, uid As String(),
                                Optional labels As String() = Nothing,
                                Optional threshold As Double = 0) As NetworkGraph

        Dim matrix = umap.GetGraph.ToArray
        Dim g As New NetworkGraph
        Dim points As PointF() = Nothing
        Dim data As NodeData = Nothing
        Dim index As i32 = Scan0

        If umap.dimension = 2 Then
            points = umap.GetPoint2D
        End If

        If labels Is Nothing Then
            labels = uid
        End If

        Dim getLabel As Func(Of String) = Function() labels(index)

        For Each label As String In uid
            data = New NodeData With {
                .label = getLabel(),
                .origID = getLabel()
            }

            If Not points Is Nothing Then
                data.initialPostion = New FDGVector2(points(++index))
            Else
                index += 1
            End If

            Call g.CreateNode(label, data)
        Next

        For i As Integer = 0 To matrix.Length - 1
            For j As Integer = 0 To matrix(i).Length - 1
                If i <> j AndAlso stdNum.Abs(matrix(i)(j)) >= threshold Then
                    Call g.CreateEdge(uid(i), uid(j), weight:=matrix(i)(j))
                End If
            Next
        Next

        Return g
    End Function
End Module
