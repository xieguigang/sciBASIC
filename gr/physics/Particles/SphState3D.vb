' /********************************************************************************/
'
'   SphState3D.vb
'
'   3D SPH 粒子状态 —— Structure of Arrays (SoA) 存储
'
'   作用：
'       把 3D SPH 求解器的全部粒子状态从 "每粒子一个类对象(Particle3D + Vector3)"
'       改为若干条连续的 Single() 数组。这是本次优化的性能基石：
'
'         - 消除每粒子对象与 Vector3 的堆分配（1500 粒子每步创建上万个临时对象）
'         - 邻居遍历是纯顺序/近顺序访存，缓存友好
'         - 数组可直接上传 CUDA 设备缓冲（DeviceBuffer(Of Single)），
'           CPU 与 GPU 两条计算路径共享同一份数据布局
'
'   设计说明：
'       - 位置/速度/预测位置/密度/压力/加速度各一条数组，长度 = Capacity。
'       - Count 为当前活跃粒子数；数组只扩容不缩容（热路径零分配）。
'       - Particle3D() 仅为"对外视图"：通过 SyncFromParticles / SyncToParticles
'         与旧的 AoS 数据结构互同步，保证 FluidEngine3D.Entity 等既有 API 不变。
'
' /********************************************************************************/

Imports System.Math
Imports System.Threading.Tasks

''' <summary>
''' The Structure-of-Arrays particle state of the 3D SPH solver.
''' </summary>
''' <remarks>
''' All buffers are <c>Single</c> so that they can be uploaded to a CUDA
''' device buffer without any conversion (see <c>CudaSphBackend</c> in the
''' SPHEngine project).
''' </remarks>
Public Class SphState3D

    ''' <summary>current number of active particles.</summary>
    Public Property Count As Integer

    ''' <summary>position, x component</summary>
    Public px() As Single
    ''' <summary>position, y component</summary>
    Public py() As Single
    ''' <summary>position, z component</summary>
    Public pz() As Single

    ''' <summary>velocity, x component</summary>
    Public vx() As Single
    ''' <summary>velocity, y component</summary>
    Public vy() As Single
    ''' <summary>velocity, z component</summary>
    Public vz() As Single

    ''' <summary>predicted position, x component</summary>
    Public qx() As Single
    ''' <summary>predicted position, y component</summary>
    Public qy() As Single
    ''' <summary>predicted position, z component</summary>
    Public qz() As Single

    ''' <summary>SPH density sum of each particle</summary>
    Public dens() As Single
    ''' <summary>SPH near density sum (the (h-r)^3 kernel) of each particle</summary>
    Public densNear() As Single

    ''' <summary>pressure of each particle</summary>
    Public press() As Single
    ''' <summary>near pressure of each particle (always repulsive)</summary>
    Public pressNear() As Single

    ''' <summary>accumulated acceleration, x component</summary>
    Public ax() As Single
    ''' <summary>accumulated acceleration, y component</summary>
    Public ay() As Single
    ''' <summary>accumulated acceleration, z component</summary>
    Public az() As Single

    ''' <summary>allocated capacity of every buffer</summary>
    Public ReadOnly Property Capacity As Integer
        Get
            If px Is Nothing Then Return 0
            Return px.Length
        End Get
    End Property

    ''' <summary>
    ''' allocate the SoA buffers for <paramref name="n"/> particles
    ''' </summary>
    Sub New(n As Integer)
        Call EnsureCapacity(Math.Max(1, n))
        Me.Count = n
    End Sub

    ''' <summary>
    ''' grow every buffer so that it can hold at least <paramref name="n"/> particles.
    ''' buffers are never shrunk so that the hot loop stays allocation free.
    ''' </summary>
    Public Sub EnsureCapacity(n As Integer)
        If px IsNot Nothing AndAlso px.Length >= n Then Return

        Dim size As Integer = Math.Max(16, n)
        size = Math.Max(size, If(px Is Nothing, 0, px.Length * 2))

        px = Grow(px, size)
        py = Grow(py, size)
        pz = Grow(pz, size)
        vx = Grow(vx, size)
        vy = Grow(vy, size)
        vz = Grow(vz, size)
        qx = Grow(qx, size)
        qy = Grow(qy, size)
        qz = Grow(qz, size)
        dens = Grow(dens, size)
        densNear = Grow(densNear, size)
        press = Grow(press, size)
        pressNear = Grow(pressNear, size)
        ax = Grow(ax, size)
        ay = Grow(ay, size)
        az = Grow(az, size)
    End Sub

    Private Shared Function Grow(buf As Single(), size As Integer) As Single()
        If buf Is Nothing Then Return New Single(size - 1) {}
        If buf.Length >= size Then Return buf

        Dim next1 As Single() = New Single(size - 1) {}
        Array.Copy(buf, next1, buf.Length)
        Return next1
    End Function

    ''' <summary>
    ''' reset the dynamic buffers (velocity / density / acceleration) to zero.
    ''' </summary>
    Public Sub ClearDynamic()
        Dim n = Count
        Array.Clear(vx, 0, n)
        Array.Clear(vy, 0, n)
        Array.Clear(vz, 0, n)
        Array.Clear(dens, 0, n)
        Array.Clear(densNear, 0, n)
        Array.Clear(press, 0, n)
        Array.Clear(pressNear, 0, n)
        Array.Clear(ax, 0, n)
        Array.Clear(ay, 0, n)
        Array.Clear(az, 0, n)
    End Sub

#Region "AoS 视图互同步"

    ''' <summary>
    ''' copy the state from the legacy <see cref="Particle3D"/> array-of-structures view.
    ''' </summary>
    Public Sub SyncFromParticles(particles As Particle3D())
        Dim n = particles.Length
        Call EnsureCapacity(n)
        Me.Count = n

        For i As Integer = 0 To n - 1
            Dim p = particles(i)
            px(i) = CSng(p.Position.x)
            py(i) = CSng(p.Position.y)
            pz(i) = CSng(p.Position.z)
            vx(i) = CSng(p.Velocity.x)
            vy(i) = CSng(p.Velocity.y)
            vz(i) = CSng(p.Velocity.z)
            qx(i) = px(i)
            qy(i) = py(i)
            qz(i) = pz(i)
            dens(i) = CSng(p.Density.x)
            densNear(i) = CSng(p.Density.y)
        Next

        Array.Clear(press, 0, n)
        Array.Clear(pressNear, 0, n)
        Array.Clear(ax, 0, n)
        Array.Clear(ay, 0, n)
        Array.Clear(az, 0, n)
    End Sub

    ''' <summary>
    ''' write the state back into the legacy <see cref="Particle3D"/> view
    ''' (used by the read only <c>Entity</c> property and by the renderer).
    ''' </summary>
    Public Sub SyncToParticles(particles As Particle3D())
        Dim n = Math.Min(Count, particles.Length)

        For i As Integer = 0 To n - 1
            Dim p = particles(i)
            If p Is Nothing Then
                p = New Particle3D(i, New Vector3(1, 1, 1))
                particles(i) = p
            End If

            p.Position = New Vector3(px(i), py(i), pz(i))
            p.Velocity = New Vector3(vx(i), vy(i), vz(i))
            p.PredictedPosition = New Vector3(qx(i), qy(i), qz(i))
            p.Density = New Vector3(dens(i), densNear(i), 0)
        Next
    End Sub

    ''' <summary>
    ''' materialize the current state as a legacy <see cref="Particle3D"/> array.
    ''' </summary>
    Public Function ToParticles() As Particle3D()
        Dim out As Particle3D() = New Particle3D(Count - 1) {}
        For i As Integer = 0 To Count - 1
            out(i) = New Particle3D(i, New Vector3(1, 1, 1))
        Next
        Call SyncToParticles(out)
        Return out
    End Function

#End Region

#Region "统计"

    ''' <summary>the largest particle speed of the current state</summary>
    Public Function MaxSpeed() As Single
        Dim maxSq As Single = 0.0F
        Dim n = Count

        For i As Integer = 0 To n - 1
            Dim s = vx(i) * vx(i) + vy(i) * vy(i) + vz(i) * vz(i)
            If s > maxSq Then maxSq = s
        Next

        Return Sqrt(maxSq)
    End Function

    ''' <summary>the arithmetic mean density over all particles</summary>
    Public Function MeanDensity() As Single
        Dim sum As Double = 0
        Dim n = Count
        If n = 0 Then Return 0

        For i As Integer = 0 To n - 1
            sum += dens(i)
        Next

        Return CSng(sum / n)
    End Function

#End Region

End Class
