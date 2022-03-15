#Region "Microsoft.VisualBasic::5cf45fb0eca42c7b1e9459d8e8d2836e, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\Orthogonal\Workspace.vb"

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

    '   Total Lines: 94
    '    Code Lines: 50
    ' Comment Lines: 24
    '   Blank Lines: 20
    '     File Size: 3.06 KB


    '     Class Workspace
    ' 
    '         Properties: totalEdgeLength, totalIntersections
    ' 
    '         Function: intersectionCounts, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace Layouts.Orthogonal

    Friend Class Workspace

        Public g As NetworkGraph
        Public grid As Grid
        Public V As Node()
        ''' <summary>
        ''' c
        ''' </summary>
        Public cellSize As Double
        Public delta As Double

        Public width As Dictionary(Of String, Double)
        Public height As Dictionary(Of String, Double)

        Public ReadOnly Property totalEdgeLength As Double
            Get
                'Dim len As Double

                'For Each edge As Edge In g.graphEdges
                '    len += distance(edge.U, edge.V, cellSize, delta)
                'Next

                'Return len
                Return g.graphEdges _
                    .AsParallel _
                    .Select(Function(edge)
                                Return distance(edge.U, edge.V, cellSize, delta)
                            End Function) _
                    .Sum
            End Get
        End Property

        Public ReadOnly Property totalIntersections As Integer
            Get
                'Dim n As Integer

                'For Each i As Edge In g.graphEdges
                '    Dim a = i.U.data.initialPostion
                '    Dim b = i.V.data.initialPostion

                '    For Each j As Edge In g.graphEdges
                '        If i Is j Then
                '            Continue For
                '        End If

                '        Dim c = j.U.data.initialPostion
                '        Dim d = j.V.data.initialPostion

                '        If GeometryMath.GetLineIntersection(a.x, a.y, b.x, b.y, c.x, c.y, d.x, d.y) = Intersections.Intersection Then
                '            n += 1
                '        End If
                '    Next
                'Next

                'Return n
                Return g.graphEdges _
                    .AsParallel _
                    .Select(AddressOf intersectionCounts) _
                    .Sum
            End Get
        End Property

        Private Function intersectionCounts(i As Edge) As Integer
            Dim a = i.U.data.initialPostion
            Dim b = i.V.data.initialPostion
            Dim n As Integer

            For Each j As Edge In g.graphEdges
                If i Is j Then
                    Continue For
                End If

                Dim c = j.U.data.initialPostion
                Dim d = j.V.data.initialPostion

                If GeometryMath.GetLineIntersection(a.x, a.y, b.x, b.y, c.x, c.y, d.x, d.y) = Intersections.Intersection Then
                    n += 1
                End If
            Next

            Return n
        End Function

        Public Overrides Function ToString() As String
            Return $"total length: {totalEdgeLength}, temperature: {totalIntersections}"
        End Function

    End Class
End Namespace
