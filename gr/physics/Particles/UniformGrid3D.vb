' /********************************************************************************/
'
'   UniformGrid3D.vb
'
'   扁平均匀网格（计数排序）—— 3D SPH 的邻居搜索结构
'
'   作用：
'       取代旧的 Dictionary(Of (Integer,Integer,Integer), Particle3D()) + LINQ
'       GroupBy / ToDictionary 的空间哈希。旧实现每一步都要重建字典（大量元组哈希
'       与数组分配），并且 SpatialLookup3D 是一个 Iterator，每个粒子每趟邻居遍历
'       都要分配一个枚举器对象 —— 密度 / 压力 / 粘性三趟就是 3N 次分配。
'
'   新实现：
'       - 网格边长 = 平滑半径 h，因此只需查询 3x3x3 = 27 个相邻格即可覆盖全部邻居
'       - 采用计数排序（counting sort）两步构建：
'           1) 统计每格粒子数 counts(c)
'           2) 前缀和得到 cellStart(c)，再散粒子索引到 entries()
'         复杂度 O(n)，全程只写入预分配的 Int32() 数组，零 GC
'       - cellStart / entries 为扁平 Int32 数组，可直接上传 GPU，
'         使 CPU 与 CUDA 两条路径共用同一套邻居遍历语义
'
'   约定：
'       网格覆盖 [Origin, Origin + N * CellSize)，落在域外的粒子被夹取到边缘格，
'       因此 CellOf() 永不返回 -1（保证 GPU 端无需分支处理越界格）。
'
' /********************************************************************************/

Imports std = System.Math

''' <summary>
''' A flat uniform grid (counting sorted) neighbour search structure for the
''' 3D SPH solver.
''' </summary>
Public Class UniformGrid3D

    ''' <summary>edge length of one cell (equals to the SPH smoothing radius)</summary>
    Public ReadOnly Property CellSize As Single
    ''' <summary>number of cells along x</summary>
    Public ReadOnly Property Nx As Integer
    ''' <summary>number of cells along y</summary>
    Public ReadOnly Property Ny As Integer
    ''' <summary>number of cells along z</summary>
    Public ReadOnly Property Nz As Integer

    ''' <summary>lower corner of the grid domain</summary>
    Public ReadOnly Property OriginX As Single
    ''' <summary>lower corner of the grid domain</summary>
    Public ReadOnly Property OriginY As Single
    ''' <summary>lower corner of the grid domain</summary>
    Public ReadOnly Property OriginZ As Single

    ''' <summary>
    ''' prefix sum of the per cell particle count. particles of cell <c>c</c> are
    ''' stored as <c>entries(cellStart(c) .. cellStart(c+1)-1)</c>.
    ''' length = Nx*Ny*Nz + 1
    ''' </summary>
    Public CellStart() As Integer

    ''' <summary>particle indices, bucketed by cell. length = capacity</summary>
    Public Entries() As Integer

    ''' <summary>total number of grid cells</summary>
    Public ReadOnly Property CellCount As Integer
        Get
            Return Nx * Ny * Nz
        End Get
    End Property

    ' 构建期临时缓冲（复用，不每步分配）
    Private counts() As Integer
    Private cursor() As Integer
    Private cellIndexOf() As Integer

    ''' <summary>
    ''' create a uniform grid that covers the given axis aligned domain.
    ''' </summary>
    ''' <param name="cellSize">cell edge length (the SPH smoothing radius)</param>
    ''' <param name="minX">domain lower bound</param>
    ''' <param name="minY">domain lower bound</param>
    ''' <param name="minZ">domain lower bound</param>
    ''' <param name="maxX">domain upper bound</param>
    ''' <param name="maxY">domain upper bound</param>
    ''' <param name="maxZ">domain upper bound</param>
    ''' <param name="capacity">expected particle count (for buffer sizing)</param>
    Sub New(cellSize As Single,
            minX As Single, minY As Single, minZ As Single,
            maxX As Single, maxY As Single, maxZ As Single,
            capacity As Integer)

        Me.CellSize = CSng(std.Max(0.000001F, cellSize))
        Me.OriginX = minX
        Me.OriginY = minY
        Me.OriginZ = minZ

        ' +2 格余量：粒子在边界投影前可能略微越界，夹取后仍落在合法格内
        Me.Nx = std.Max(1, CInt(std.Ceiling((maxX - minX) / Me.CellSize)) + 2)
        Me.Ny = std.Max(1, CInt(std.Ceiling((maxY - minY) / Me.CellSize)) + 2)
        Me.Nz = std.Max(1, CInt(std.Ceiling((maxZ - minZ) / Me.CellSize)) + 2)

        Dim nCells = Me.CellCount

        Me.CellStart = New Integer(nCells) {}
        Me.counts = New Integer(nCells - 1) {}
        Me.cursor = New Integer(nCells - 1) {}
        Me.cellIndexOf = New Integer(std.Max(1, capacity) - 1) {}
        Me.Entries = New Integer(std.Max(1, capacity) - 1) {}
    End Sub

    Private Sub EnsureCapacity(n As Integer)
        If cellIndexOf.Length >= n AndAlso Entries.Length >= n Then Return

        Dim size As Integer = std.Max(n, cellIndexOf.Length * 2)
        cellIndexOf = New Integer(size - 1) {}
        Entries = New Integer(size - 1) {}
    End Sub

    ''' <summary>the cell coordinate along x of the given world position (clamped)</summary>
    Public Function CoordX(x As Single) As Integer
        Dim c = CInt(std.Floor((x - OriginX) / CellSize))
        If c < 0 Then Return 0
        If c >= Nx Then Return Nx - 1
        Return c
    End Function

    ''' <summary>the cell coordinate along y of the given world position (clamped)</summary>
    Public Function CoordY(y As Single) As Integer
        Dim c = CInt(std.Floor((y - OriginY) / CellSize))
        If c < 0 Then Return 0
        If c >= Ny Then Return Ny - 1
        Return c
    End Function

    ''' <summary>the cell coordinate along z of the given world position (clamped)</summary>
    Public Function CoordZ(z As Single) As Integer
        Dim c = CInt(std.Floor((z - OriginZ) / CellSize))
        If c < 0 Then Return 0
        If c >= Nz Then Return Nz - 1
        Return c
    End Function

    ''' <summary>
    ''' the flat cell index of the given integer cell coordinate,
    ''' returns -1 when the coordinate is outside of the grid.
    ''' </summary>
    Public Function CellIndex(cx As Integer, cy As Integer, cz As Integer) As Integer
        If cx < 0 OrElse cy < 0 OrElse cz < 0 Then Return -1
        If cx >= Nx OrElse cy >= Ny OrElse cz >= Nz Then Return -1
        Return (cx * Ny + cy) * Nz + cz
    End Function

    ''' <summary>the (never out of range) cell index of the given world position</summary>
    Public Function CellOf(x As Single, y As Single, z As Single) As Integer
        Return CellIndex(CoordX(x), CoordY(y), CoordZ(z))
    End Function

    ''' <summary>
    ''' rebuild the grid from the current particle positions (counting sort, O(n)).
    ''' </summary>
    ''' <param name="state">the particle state</param>
    ''' <param name="predicted">
    ''' when true the predicted positions (qx/qy/qz) are used, otherwise the
    ''' integrated positions (px/py/pz).
    ''' </param>
    Public Sub Build(state As SphState3D, Optional predicted As Boolean = True)
        Dim n = state.Count
        Call EnsureCapacity(n)

        Dim sx = If(predicted, state.qx, state.px)
        Dim sy = If(predicted, state.qy, state.py)
        Dim sz = If(predicted, state.qz, state.pz)

        Array.Clear(counts, 0, counts.Length)

        For i As Integer = 0 To n - 1
            Dim c = CellOf(sx(i), sy(i), sz(i))
            cellIndexOf(i) = c
            counts(c) += 1
        Next

        Dim acc As Integer = 0
        For c As Integer = 0 To counts.Length - 1
            CellStart(c) = acc
            acc += counts(c)
        Next
        CellStart(counts.Length) = acc

        Array.Copy(CellStart, cursor, counts.Length)

        For i As Integer = 0 To n - 1
            Dim c = cellIndexOf(i)
            Entries(cursor(c)) = i
            cursor(c) += 1
        Next
    End Sub

    ''' <summary>
    ''' number of particles currently stored inside the given cell.
    ''' </summary>
    Public Function CellSizeAt(c As Integer) As Integer
        Return CellStart(c + 1) - CellStart(c)
    End Function

End Class
