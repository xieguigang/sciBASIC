Namespace Boids

    ''' <summary>
    ''' Dense uniform spatial grid in three dimensions, built with counting sort.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This is the three dimensional counterpart of <see cref="FlockGrid"/>: one
    ''' pass computes the rounded cell coordinate of every boid, one counting pass
    ''' produces the CSR style <see cref="cellStart"/> offsets and one fill pass
    ''' writes the boid ids into <see cref="sortedIds"/> grouped by cell. Boid ids
    ''' inside a cell keep their original ascending order, so the neighbor
    ''' enumeration is deterministic.
    ''' </para>
    ''' <para>
    ''' The grid is padded by one cell on every side of the per frame bounding
    ''' box, for the very same reason as in the two dimensional grid: the wall
    ''' rule only applies a steering force and never clamps a position, so a boid
    ''' can drift outside of the box and still has to see its neighbors.
    ''' </para>
    ''' <para>
    ''' Compared with the two dimensional version this grid adds two robustness
    ''' guards that a three dimensional field makes necessary: a position whose
    ''' cell coordinate would leave the valid range of <see cref="BoidMath.Round"/>
    ''' is treated as a bad position, and the cell count is validated before the
    ''' arrays are allocated - a sparse distribution over a huge volume would
    ''' otherwise either overflow the integer index or exhaust the memory. A cell
    ''' size that is too small for the current spread is silently enlarged, which
    ''' degrades the neighbor search into a coarser grid instead of failing.
    ''' </para>
    ''' </remarks>
    Friend NotInheritable Class FlockGrid3D

        ''' <summary>
        ''' cell id = (cz - minCz) * nx * ny + (cy - minCy) * nx + (cx - minCx);
        ''' <see cref="cellStart"/>(c) .. <see cref="cellStart"/>(c+1) is the
        ''' half-open range of <see cref="sortedIds"/> owned by the cell c.
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
        ''' rounded cell z of each boid (recomputed on every <see cref="Build"/>)
        ''' </summary>
        Public cellOfZ() As Integer

        ''' <summary>
        ''' padded cell counts: the cell bounding box is expanded by one cell on
        ''' every side
        ''' </summary>
        Public nx, ny, nz As Integer
        ''' <summary>
        ''' the cell coordinate of the first padded cell
        ''' </summary>
        Public minCx, minCy, minCz As Integer
        ''' <summary>
        ''' the number of the padded cells, <c>nx * ny * nz</c>
        ''' </summary>
        Public nCells As Integer

        ''' <summary>
        ''' the cell size that <see cref="Build"/> finally used: it is the
        ''' requested size unless the spread of the flock forced a coarser grid
        ''' </summary>
        Public ReadOnly Property CellSize As Double
            Get
                Return m_cellSize
            End Get
        End Property

        Private m_cellSize As Double = 0
        ''' <summary>
        ''' fill cursor reused by <see cref="Build"/>
        ''' </summary>
        Private cursor() As Integer

        ''' <summary>
        ''' the sentinel stored in <see cref="cellOfX"/>/<see cref="cellOfY"/>/
        ''' <see cref="cellOfZ"/> for boids whose position is not finite or is too
        ''' large for the rounded cell coordinate; such boids are pinned to the
        ''' minimum cell corner instead of crashing the sort
        ''' </summary>
        Private Const badCell As Integer = Integer.MinValue

        ''' <summary>
        ''' the largest cell coordinate that <see cref="BoidMath.Round"/> can
        ''' convert (its magic number trick is only valid below 2^51)
        ''' </summary>
        Private Const maxCellCoordinate As Double = 1125899906842624.0R

        ''' <summary>
        ''' the upper bound of the allocated cell grid: about 4 million cells
        ''' (two <see cref="Integer"/> arrays of 16 MB each)
        ''' </summary>
        Private Const MaxCells As Integer = 4194304

        ''' <summary>
        ''' create a new grid that can hold the given number of boids
        ''' </summary>
        ''' <param name="capacity">the number of the boid slots to allocate</param>
        Public Sub New(capacity As Integer)
            If capacity < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(capacity))
            End If

            If capacity = 0 Then
                cellOfX = Array.Empty(Of Integer)()
                cellOfY = Array.Empty(Of Integer)()
                cellOfZ = Array.Empty(Of Integer)()
                sortedIds = Array.Empty(Of Integer)()
            Else
                cellOfX = New Integer(capacity - 1) {}
                cellOfY = New Integer(capacity - 1) {}
                cellOfZ = New Integer(capacity - 1) {}
                sortedIds = New Integer(capacity - 1) {}
            End If
        End Sub

        ''' <summary>
        ''' Rebuild the grid from the current boid positions.
        ''' </summary>
        ''' <param name="state">the SoA simulation state</param>
        ''' <param name="cellSize">
        ''' the requested grid cell size, in simulation units; a value that is too
        ''' small for the spread of the flock is enlarged automatically (see
        ''' <see cref="CellSize"/>)
        ''' </param>
        Public Sub Build(state As FlockState3D, cellSize As Double)
            Dim n As Integer = state.count

            If n = 0 Then
                nx = 0
                ny = 0
                nz = 0
                nCells = 0
                minCx = 0
                minCy = 0
                minCz = 0
                m_cellSize = cellSize
                Return
            End If

            If cellSize <= 0 OrElse Double.IsNaN(cellSize) OrElse Double.IsInfinity(cellSize) Then
                cellSize = 1.0
            End If

            ' 1) rounded cell coordinate of every boid + the bounding box, then
            '    validate the grid size; a spread that is too wide for the
            '    requested cell size is measured again with a coarser grid
            Dim minX As Integer = 0, minY As Integer = 0, minZ As Integer = 0
            Dim maxX As Integer = 0, maxY As Integer = 0, maxZ As Integer = 0

            For attempt As Integer = 0 To 31
                Call MeasureCells(state, cellSize, minX, maxX, minY, maxY, minZ, maxZ)

                nx = maxX - minX + 3
                ny = maxY - minY + 3
                nz = maxZ - minZ + 3

                ' the product is evaluated in double precision: a spread over a
                ' huge volume overflows a Long long before the cell size has
                ' grown enough, and a wrapped product would silently pass the
                ' budget check below with a garbage cell count
                Dim cells As Double = CDbl(nx) * CDbl(ny) * CDbl(nz)

                If cells <= MaxCells Then
                    m_cellSize = cellSize
                    Exit For
                ElseIf attempt = 31 Then
                    Throw New InvalidOperationException(
                        $"the flock spread needs {cells:F0} grid cells, which exceeds the {MaxCells} cell budget even at the coarsest cell size")
                End If

                ' grow the cell size by the cube root of the overshoot, so the
                ' grid becomes valid after a couple of iterations even when the
                ' flock is spread over a practically unbounded volume
                Dim factor As Double = CSng(cells / CDbl(MaxCells)) ^ (1.0 / 3.0) * 1.05

                If factor < 1.1 Then factor = 1.1

                cellSize *= factor
            Next

            minCx = minX - 1
            minCy = minY - 1
            minCz = minZ - 1

            nCells = nx * ny * nz

            If cellStart Is Nothing OrElse cellStart.Length < nCells + 1 Then
                ' the arrays keep the size of the largest grid that was ever
                ' needed, a frame only clears the range that it uses
                cellStart = New Integer(nCells) {}
                cursor = New Integer(nCells) {}
            End If

            Array.Clear(cellStart, 0, nCells + 1)

            ' 2) counting sort: count -> prefix sum -> fill
            For i As Integer = 0 To n - 1
                Dim c As Integer = CellId(i)
                cellStart(c + 1) += 1
            Next

            For c As Integer = 1 To nCells - 1
                cellStart(c) += cellStart(c - 1)
            Next

            Array.Copy(cellStart, cursor, nCells)

            For i As Integer = 0 To n - 1
                Dim c As Integer = CellId(i)
                sortedIds(cursor(c)) = i
                cursor(c) += 1
            Next
        End Sub

        ''' <summary>
        ''' compute the rounded cell coordinate of every boid and the bounding
        ''' box of the finite (and representable) cells
        ''' </summary>
        Private Sub MeasureCells(state As FlockState3D, cellSize As Double,
                                 ByRef minX As Integer, ByRef maxX As Integer,
                                 ByRef minY As Integer, ByRef maxY As Integer,
                                 ByRef minZ As Integer, ByRef maxZ As Integer)
            Dim px = state.px
            Dim py = state.py
            Dim pz = state.pz
            Dim n As Integer = state.count

            minX = Integer.MaxValue
            minY = Integer.MaxValue
            minZ = Integer.MaxValue
            maxX = Integer.MinValue
            maxY = Integer.MinValue
            maxZ = Integer.MinValue

            For i As Integer = 0 To n - 1
                Dim cx, cy, cz As Integer
                Dim x = px(i)
                Dim y = py(i)
                Dim z = pz(i)

                If Not IsUsable(x) OrElse Not IsUsable(y) OrElse Not IsUsable(z) Then
                    cx = badCell
                    cy = badCell
                    cz = badCell
                Else
                    Dim rx As Double = x / cellSize
                    Dim ry As Double = y / cellSize
                    Dim rz As Double = z / cellSize

                    ' a coordinate that the fast rounding cannot represent is
                    ' pinned to the same corner as a NaN, otherwise the cell id
                    ' would silently wrap around and corrupt the sort
                    If System.Math.Abs(rx) >= maxCellCoordinate OrElse
                       System.Math.Abs(ry) >= maxCellCoordinate OrElse
                       System.Math.Abs(rz) >= maxCellCoordinate Then

                        cx = badCell
                        cy = badCell
                        cz = badCell
                    Else
                        cx = BoidMath.Round(rx)
                        cy = BoidMath.Round(ry)
                        cz = BoidMath.Round(rz)

                        If cx < minX Then minX = cx
                        If cx > maxX Then maxX = cx
                        If cy < minY Then minY = cy
                        If cy > maxY Then maxY = cy
                        If cz < minZ Then minZ = cz
                        If cz > maxZ Then maxZ = cz
                    End If
                End If

                cellOfX(i) = cx
                cellOfY(i) = cy
                cellOfZ(i) = cz
            Next

            If maxX < minX Then
                ' every boid position was unusable: pin everything into the
                ' single cell at the origin of the padded grid
                minX = 0
                maxX = 0
                minY = 0
                maxY = 0
                minZ = 0
                maxZ = 0
            End If
        End Sub

        ''' <summary>
        ''' is the given coordinate finite?
        ''' </summary>
        Private Shared Function IsUsable(v As Double) As Boolean
            Return Not (Double.IsNaN(v) OrElse Double.IsInfinity(v))
        End Function

        ''' <summary>
        ''' dense cell id of the boid at the given index (requires
        ''' <see cref="cellOfX"/>/<see cref="cellOfY"/>/<see cref="cellOfZ"/> to be
        ''' filled by <see cref="Build"/>)
        ''' </summary>
        Private Function CellId(i As Integer) As Integer
            Dim cx = cellOfX(i)
            Dim cy = cellOfY(i)
            Dim cz = cellOfZ(i)

            If cx = badCell Then
                cx = minCx + 1
                cy = minCy + 1
                cz = minCz + 1
            End If

            Return ((cz - minCz) * ny + (cy - minCy)) * nx + (cx - minCx)
        End Function

        ''' <summary>
        ''' Fast round-half-to-even (banker's rounding) of a double, bit-compatible
        ''' with the VB <c>CInt()</c> conversion that the cell hashing of the two
        ''' dimensional grid uses.
        ''' </summary>
        ''' <param name="v">the cell coordinate to round</param>
        ''' <returns>the rounded cell coordinate</returns>
        Public Shared Function CellRound(v As Double) As Integer
            Return BoidMath.Round(v)
        End Function
    End Class
End Namespace
