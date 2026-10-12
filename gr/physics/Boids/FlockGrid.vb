#Region "Microsoft.VisualBasic::7b2e41d0c95a4f83bb0a6d72e5c1f903, gr\physics\Boids\FlockGrid.vb"

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

    ' A dense counting-sort uniform grid that replaces the legacy 
    ' LINQ GroupBy + nested Dictionary spatial hash. Building the grid is two 
    ' linear passes over the boid list and allocates nothing after warm-up; 
    ' neighbor queries become flat index scans over <see cref="sortedIds"/>.

    '     Class FlockGrid
    ' 
    '         Properties: cellOfX, cellOfY, cellStart, minCx, minCy, nCells, nx, ny, sortedIds
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Build
    '         Function: CellRound
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Boids

    ''' <summary>
    ''' Dense uniform spatial grid built with counting sort.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The legacy implementation rebuilt the generic <c>Grid(Of T())</c> spatial 
    ''' hash (a nested dictionary of dictionaries) every frame via LINQ GroupBy, 
    ''' and every neighbor lookup probed that dictionary-of-dictionaries nine 
    ''' times. At 80k boids that is ~1.4M dictionary probes plus hundreds of 
    ''' thousands of allocations per frame.
    ''' </para>
    ''' <para>
    ''' This grid is rebuilt every frame in O(N): one pass computes the rounded 
    ''' cell coordinate of every boid (the same ``CInt(x / cellSize)`` banker's 
    ''' rounding the legacy code used), one counting pass produces the CSR-style 
    ''' <see cref="cellStart"/> offsets, and one fill pass writes boid ids into
    ''' <see cref="sortedIds"/> grouped by cell. Boid ids inside a cell keep their 
    ''' original ascending order, so the neighbor enumeration order is identical 
    ''' to the legacy grid.
    ''' </para>
    ''' <para>
    ''' The grid is padded by one cell on each side of the per-frame bounding box. 
    ''' Because the wall bounce rule only applies a steering force (it never 
    ''' clamps positions), boids can drift outside the canvas; the padding 
    ''' preserves the legacy behavior where such out-of-bounds boids can still 
    ''' see each other as neighbors.
    ''' </para>
    ''' </remarks>
    Friend NotInheritable Class FlockGrid

        ''' <summary>
        ''' cell id = (cy - minCy) * nx + (cx - minCx); 
        ''' <see cref="cellStart"/>(c) .. <see cref="cellStart"/>(c+1) is the 
        ''' half-open range of <see cref="sortedIds"/> owned by cell c.
        ''' length = nCells + 1
        ''' </summary>
        Public cellStart() As Integer
        ''' <summary>
        ''' boid ids sorted by cell id
        ''' </summary>
        Public sortedIds() As Integer
        ''' <summary>
        ''' rounded cell x of each boid (recomputed on every <see cref="Build"/>)
        ''' </summary>
        Public cellOfX() As Integer
        ''' <summary>
        ''' rounded cell y of each boid (recomputed on every <see cref="Build"/>)
        ''' </summary>
        Public cellOfY() As Integer

        ''' <summary>
        ''' padded cell counts: the cell bounding box is expanded by one cell 
        ''' on every side
        ''' </summary>
        Public nx, ny As Integer
        Public minCx, minCy As Integer
        Public nCells As Integer

        ''' <summary>
        ''' fill cursor reused by <see cref="Build"/>
        ''' </summary>
        Private cursor() As Integer

        ''' <summary>
        ''' the sentinel stored in <see cref="cellOfX"/>/<see cref="cellOfY"/> for 
        ''' boids whose position is not finite (NaN/Inf); such boids are pinned 
        ''' to the minimum cell corner instead of crashing the sort
        ''' </summary>
        Private Const badCell As Integer = Integer.MinValue

        Public Sub New(capacity As Integer)
            If capacity < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(capacity))
            ElseIf capacity = 0 Then
                cellOfX = Array.Empty(Of Integer)()
                cellOfY = Array.Empty(Of Integer)()
                sortedIds = Array.Empty(Of Integer)()
            Else
                cellOfX = New Integer(capacity - 1) {}
                cellOfY = New Integer(capacity - 1) {}
                sortedIds = New Integer(capacity - 1) {}
            End If
        End Sub

        ''' <summary>
        ''' Rebuild the grid from the current boid positions.
        ''' </summary>
        ''' <param name="state">the SoA simulation state</param>
        ''' <param name="cellSize">the grid cell size, in simulation units</param>
        Public Sub Build(state As FlockState, cellSize As Double)
            Dim n As Integer = state.count

            If n = 0 Then
                nx = 0
                ny = 0
                nCells = 0
                minCx = 0
                minCy = 0
                Return
            End If

            Dim px = state.px
            Dim py = state.py

            ' 1) rounded cell coordinate of every boid + bounding box of the finite cells
            Dim minX As Integer = Integer.MaxValue, minY As Integer = Integer.MaxValue
            Dim maxX As Integer = Integer.MinValue, maxY As Integer = Integer.MinValue

            For i As Integer = 0 To n - 1
                Dim cx, cy As Integer
                Dim x = px(i)
                Dim y = py(i)

                If Double.IsNaN(x) OrElse Double.IsInfinity(x) OrElse
                   Double.IsNaN(y) OrElse Double.IsInfinity(y) Then
                    cx = badCell
                    cy = badCell
                Else
                    cx = CellRound(x / cellSize)
                    cy = CellRound(y / cellSize)

                    If cx < minX Then minX = cx
                    If cx > maxX Then maxX = cx
                    If cy < minY Then minY = cy
                    If cy > maxY Then maxY = cy
                End If

                cellOfX(i) = cx
                cellOfY(i) = cy
            Next

            ' 2) padded dense grid dimensions
            If maxX < minX Then
                ' every boid position was non-finite: pin everything into a single cell
                minX = 0
                maxX = 0
                minY = 0
                maxY = 0
            End If

            minCx = minX - 1
            minCy = minY - 1
            nx = maxX - minX + 3
            ny = maxY - minY + 3
            nCells = nx * ny

            If cellStart Is Nothing OrElse cellStart.Length < nCells + 1 Then
                cellStart = New Integer(nCells) {}
                cursor = New Integer(nCells) {}
            End If

            Array.Clear(cellStart, 0, nCells + 1)

            ' 3) counting sort: count -> prefix sum -> fill
            For i As Integer = 0 To n - 1
                Dim c = CellId(i)
                cellStart(c + 1) += 1
            Next

            For c As Integer = 1 To nCells - 1
                cellStart(c) += cellStart(c - 1)
            Next

            Array.Copy(cellStart, cursor, nCells)

            For i As Integer = 0 To n - 1
                Dim c = CellId(i)
                sortedIds(cursor(c)) = i
                cursor(c) += 1
            Next
        End Sub

        ''' <summary>
        ''' dense cell id of the boid at the given index (requires 
        ''' <see cref="cellOfX"/>/<see cref="cellOfY"/> to be filled by <see cref="Build"/>)
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function CellId(i As Integer) As Integer
            Dim cx = cellOfX(i)
            Dim cy = cellOfY(i)

            If cx = badCell Then
                cx = minCx + 1
                cy = minCy + 1
            End If

            Return (cy - minCy) * nx + (cx - minCx)
        End Function

        ''' <summary>
        ''' Fast round-half-to-even (banker's rounding) of a double, bit-compatible
        ''' with the VB <c>CInt()</c> conversion that the legacy cell hashing used.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CellRound(v As Double) As Integer
            Return BoidMath.Round(v)
        End Function
    End Class
End Namespace
