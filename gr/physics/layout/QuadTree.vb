#Region "Microsoft.VisualBasic::22d529a3b06becec315d4644ed6f410a, gr\physics\layout\QuadTree.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.

    ' /********************************************************************************/

    '     Class QuadTree

    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace layout

    Public Class QuadTree

        Friend ReadOnly quads As QuadNode()
        Friend ReadOnly COLUMNS As Integer
        Friend ReadOnly ROWS As Integer

        Public XMin, XMax, YMin, YMax As Double

        Public Sub New(numberNodes As Integer, aspectRatio As Single)
            If aspectRatio > 0 Then
                COLUMNS = CInt(std.Ceiling(numberNodes / 50.0F))
                ROWS = CInt(std.Ceiling(COLUMNS / aspectRatio))
            Else
                ROWS = CInt(std.Ceiling(numberNodes / 50.0F))
                COLUMNS = CInt(std.Ceiling(ROWS / aspectRatio))
            End If
            quads = New QuadNode(COLUMNS * ROWS - 1) {}
            For row = 0 To ROWS - 1
                For col = 0 To COLUMNS - 1
                    quads(row * COLUMNS + col) = New QuadNode(row * COLUMNS + col, row, col)
                Next
            Next
        End Sub

        Public Overridable Sub Add(node As Node, labels As Dictionary(Of Node, TextProperties))
            Dim x As Single = node.X()
            Dim y As Single = node.Y()
            Dim t As TextProperties = labels(node)
            Dim w As Single = t.Width
            Dim h As Single = t.Height
            Dim radius As Single = node.Size

            ' Get the rectangle occupied by the node (size + label)
            Dim nxmin = std.Min(x - w / 2, x - radius)
            Dim nxmax = std.Max(x + w / 2, x + radius)
            Dim nymin = std.Min(y - h / 2, y - radius)
            Dim nymax = std.Max(y + h / 2, y + radius)

            ' Get the rectangle as boxes
            Dim minXbox As Integer = std.Floor((COLUMNS - 1) * (nxmin - XMin) / (XMax - XMin))
            Dim maxXbox As Integer = std.Floor((COLUMNS - 1) * (nxmax - XMin) / (XMax - XMin))
            Dim minYbox As Integer = std.Floor((ROWS - 1) * ((YMax - YMin - (nymax - YMin)) / (YMax - YMin)))
            Dim maxYbox As Integer = std.Floor((ROWS - 1) * ((YMax - YMin - (nymin - YMin)) / (YMax - YMin)))
            Dim col = minXbox

            While col <= maxXbox AndAlso col < COLUMNS AndAlso col >= 0
                Dim row = minYbox

                While row <= maxYbox AndAlso row < ROWS AndAlso row >= 0
                    quads(CInt(row * COLUMNS + col)).Add(node)
                    row += 1
                End While

                col += 1
            End While

            'Get the node center
            Dim centerX As Integer = std.Floor((COLUMNS - 1) * (x - XMin) / (XMax - XMin))
            Dim centerY As Integer = std.Floor((ROWS - 1) * ((YMax - YMin - (y - YMin)) / (YMax - YMin)))
            Dim layoutData As LabelAdjustLayoutData = node.LayoutData
            layoutData.LabelAdjustQuadNode = quads(centerY * COLUMNS + centerX).index
        End Sub

        Public Overridable Function GetNode(row As Integer, col As Integer) As IList(Of Node)
            Return quads(row * ROWS + col).Nodes
        End Function

        Public Overridable Function GetAdjacentNodes(row As Integer, col As Integer) As IList(Of Node)
            If quads.Length = 1 Then
                Return quads(0).Nodes
            End If

            Dim adjNodes As New List(Of Node)()
            Dim left = std.Max(0, col - 1)
            Dim top = std.Max(0, row - 1)
            Dim right = std.Min(COLUMNS - 1, col + 1)
            Dim bottom = std.Min(ROWS - 1, row + 1)
            For i = left To right
                For j = top To bottom
                    CType(adjNodes, List(Of Node)).AddRange(quads(j * COLUMNS + i).Nodes)
                Next
            Next
            Return adjNodes
        End Function

        Public Overridable Function GetQuadNode(index As Integer) As QuadNode
            Return quads(index)
        End Function

        Public Class QuadNode

            Friend ReadOnly index As Integer
            Friend ReadOnly row As Integer
            Friend ReadOnly col As Integer

            Public Sub New(index As Integer, row As Integer, col As Integer)
                Me.index = index
                Me.row = row
                Me.col = col
                Me.Nodes = New List(Of Node)()
            End Sub

            Public Overridable ReadOnly Property Nodes As IList(Of Node)

            Public Overridable Sub Add(n As Node)
                Nodes.Add(n)
            End Sub
        End Class
    End Class

End Namespace
