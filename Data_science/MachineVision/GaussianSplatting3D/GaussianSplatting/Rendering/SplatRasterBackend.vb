' ---------------------------------------------------------------------------
' 光栅化后端契约 + 默认 CPU 实现
'
' 设计要点：
'   * 请求（<see cref="SplatRasterRequest"/>）已经完成投影 / 剔除 / 排序 / 分箱，
'     后端只做"按 tile 的像素合成"，因此宿主注入的 CUDA 后端不需要理解模型结构。
'   * 前向与反向使用**同一套 tile 顺序与同一套跳过条件**，否则 S_total 与
'     前缀和会对不上（这是解析反传最容易被忽略的一致性要求）。
'   * 每个 tile 使用独立局部缓冲（颜色 + 透射率 + 前缀贡献），
'     并行时无任何跨线程写冲突，也不需要原子操作。
' ---------------------------------------------------------------------------

Imports std = System.Math

''' <summary>
''' 可注入的光栅化后端。
''' </summary>
    Public Interface ISplatRasterBackend

        ''' <summary>后端名称（诊断输出用）。</summary>
        ReadOnly Property Name As String

        ''' <summary>当前环境是否可用。</summary>
        Function IsAvailable() As Boolean

        ''' <summary>
        ''' 前向：把 request 中的高斯合成到 <paramref name="image"/>（长度 H·W·3，行优先）
        ''' 与 <paramref name="transmittance"/>（长度 H·W）。
        ''' </summary>
        ''' <returns>True 表示本后端完成了这次光栅化；False 表示不可用（调用方应回退）。</returns>
        Function Rasterize(request As SplatRasterRequest, image As Double(), transmittance As Double()) As Boolean

        ''' <summary>
        ''' 反向：给定前向图像与像素梯度，向 <paramref name="grads"/> 累加屏幕空间梯度
        ''' （Colors / Opacities / Conic / Means）。
        ''' </summary>
        Function RasterizeBackward(request As SplatRasterRequest, image As Double(),
                                   imageGradient As Double(), grads As SplatGradients) As Boolean

    End Interface

    ''' <summary>
    ''' 后端注册表。
    ''' </summary>
    Public NotInheritable Class SplatBackends

        Private Shared ReadOnly _available As New List(Of ISplatRasterBackend)()
        Private Shared _default As ISplatRasterBackend

        Shared Sub New()
            _default = New CpuTileRasterBackend()
            _available.Add(_default)
        End Sub

        ''' <summary>当前默认后端。</summary>
        Public Shared Property [Default] As ISplatRasterBackend
            Get
                Return _default
            End Get
            Set(value As ISplatRasterBackend)
                If value Is Nothing OrElse Not value.IsAvailable() Then Return

                _default = value

                SyncLock _available
                    If Not _available.Contains(value) Then _available.Add(value)
                End SyncLock
            End Set
        End Property

        ''' <summary>注册一个后端（可选设为默认）。</summary>
        Public Shared Sub Register(backend As ISplatRasterBackend, Optional makeDefault As Boolean = False)
            If backend Is Nothing Then Return
            If Not backend.IsAvailable() Then Return

            SyncLock _available
                If Not _available.Contains(backend) Then _available.Add(backend)
            End SyncLock

            If makeDefault Then [Default] = backend
        End Sub

        ''' <summary>已注册的后端列表。</summary>
        Public Shared Function Available() As ISplatRasterBackend()
            SyncLock _available
                Return _available.ToArray()
            End SyncLock
        End Function

        ''' <summary>恢复为默认的 CPU 后端。</summary>
        Public Shared Sub Reset()
            [Default] = New CpuTileRasterBackend()
        End Sub

    End Class

    ''' <summary>tile 局部累积缓冲。</summary>
    Friend NotInheritable Class TileScratch

        Public ReadOnly R As Double()
        Public ReadOnly G As Double()
        Public ReadOnly B As Double()
        ''' <summary>透射率 T</summary>
        Public ReadOnly T As Double()
        ''' <summary>反向传播用的前缀贡献和 Σ_{k&le;i} w_k (c_k·dLdC)</summary>
        Public ReadOnly P As Double()

        Private ReadOnly _n As Integer

        Public Sub New(tileSize As Integer)
            _n = tileSize * tileSize
            R = New Double(_n - 1) {}
            G = New Double(_n - 1) {}
            B = New Double(_n - 1) {}
            T = New Double(_n - 1) {}
            P = New Double(_n - 1) {}
        End Sub

        Public Sub Clear()
            Array.Clear(R, 0, _n)
            Array.Clear(G, 0, _n)
            Array.Clear(B, 0, _n)
            Array.Clear(P, 0, _n)
            Array.Fill(T, 1.0, 0, _n)
        End Sub

        Public Sub ClearForward()
            Array.Clear(R, 0, _n)
            Array.Clear(G, 0, _n)
            Array.Clear(B, 0, _n)
            Array.Fill(T, 1.0, 0, _n)
        End Sub

    End Class

    ''' <summary>
    ''' 默认后端：tile 分块 + 多线程 + 连续内存（零 Tensor 索引器开销）的 CPU 光栅化器。
    ''' </summary>
    Public Class CpuTileRasterBackend
        Implements ISplatRasterBackend

        Public ReadOnly Property Name As String Implements ISplatRasterBackend.Name
            Get
                Return "CPU-Tile"
            End Get
        End Property

        Public Function IsAvailable() As Boolean Implements ISplatRasterBackend.IsAvailable
            Return True
        End Function

        Public Function Rasterize(request As SplatRasterRequest, image As Double(),
                                   transmittance As Double()) As Boolean Implements ISplatRasterBackend.Rasterize
            Dim options = If(request.Options, SplatRenderOptions.Default)
            Dim tileCount = request.TileCount

            Array.Clear(image, 0, image.Length)
            Array.Fill(transmittance, 1.0)

            Dim chunks = ParallelChunks(tileCount, options)

            If chunks <= 1 Then
                Dim scratch As New TileScratch(request.TileSize)

                For tile = 0 To tileCount - 1
                    Call RasterizeTile(request, options, image, transmittance, tile, scratch)
                Next
            Else
                Dim scratches(chunks - 1) As TileScratch

                For c = 0 To chunks - 1
                    scratches(c) = New TileScratch(request.TileSize)
                Next

                Dim perChunk = (tileCount + chunks - 1) \ chunks

                System.Threading.Tasks.Parallel.For(0, chunks, options.ParallelOptions(),
                    Sub(c)
                        Dim scratch = scratches(c)
                        Dim from = c * perChunk
                        Dim [to] = std.Min(tileCount, from + perChunk) - 1

                        For tile = from To [to]
                            Call RasterizeTile(request, options, image, transmittance, tile, scratch)
                        Next
                    End Sub)
            End If

            Return True
        End Function

        Public Function RasterizeBackward(request As SplatRasterRequest, image As Double(),
                                           imageGradient As Double(), grads As SplatGradients) As Boolean Implements ISplatRasterBackend.RasterizeBackward
            If grads Is Nothing Then Return False

            Dim options = If(request.Options, SplatRenderOptions.Default)
            Dim tileCount = request.TileCount
            Dim n = request.Screen.Count

            Dim chunks = ParallelChunks(tileCount, options)

            ' 线程本地梯度缓冲的总量受 ThreadGradientBudgetMB 约束
            Const BytesPerSplat As Long = (3 + 1 + 3 + 2) * 8

            Dim budgetBytes = CLng(std.Max(1, options.ThreadGradientBudgetMB)) * 1024L * 1024L
            Dim maxChunks = std.Max(1, CInt(budgetBytes \ std.Max(1L, CLng(n) * BytesPerSplat)))

            If chunks > maxChunks Then chunks = maxChunks

            If chunks <= 1 Then
                Dim scratch As New TileScratch(request.TileSize)
                Dim local As New ChannelGradients(n)

                For tile = 0 To tileCount - 1
                    Call BackwardTile(request, options, image, imageGradient, tile, scratch, local)
                Next

                Call Accumulate(local, grads)
            Else
                Dim scratches(chunks - 1) As TileScratch
                Dim locals(chunks - 1) As ChannelGradients

                For c = 0 To chunks - 1
                    scratches(c) = New TileScratch(request.TileSize)
                    locals(c) = New ChannelGradients(n)
                Next

                Dim perChunk = (tileCount + chunks - 1) \ chunks

                System.Threading.Tasks.Parallel.For(0, chunks, options.ParallelOptions(),
                    Sub(c)
                        Dim scratch = scratches(c)
                        Dim local = locals(c)
                        Dim from = c * perChunk
                        Dim [to] = std.Min(tileCount, from + perChunk) - 1

                        For tile = from To [to]
                            Call BackwardTile(request, options, image, imageGradient, tile, scratch, local)
                        Next
                    End Sub)

                For c = 0 To chunks - 1
                    Call Accumulate(locals(c), grads)
                Next
            End If

            Return True
        End Function

        Private Shared Function ParallelChunks(tileCount As Integer, options As SplatRenderOptions) As Integer
            Dim degree = If(options.MaxDegreeOfParallelism > 0, options.MaxDegreeOfParallelism, Environment.ProcessorCount)

            If tileCount < options.ParallelMinTiles OrElse degree <= 1 Then Return 1

            Return std.Min(degree, tileCount)
        End Function

        ''' <summary>前向：单个 tile 的合成。</summary>
        Private Shared Sub RasterizeTile(request As SplatRasterRequest, options As SplatRenderOptions,
                                         image As Double(), transmittance As Double(),
                                         tile As Integer, scratch As TileScratch)
            Dim imgW = request.Width
            Dim ts = request.TileSize
            Dim tx = tile Mod request.TilesX
            Dim ty = tile \ request.TilesX
            Dim px0 = tx * ts, py0 = ty * ts
            Dim px1 = std.Min(px0 + ts - 1, imgW - 1)
            Dim py1 = std.Min(py0 + ts - 1, request.Height - 1)

            Dim g0 = request.TileOffsets(tile)
            Dim g1 = request.TileOffsets(tile + 1)

            If g0 = g1 Then
                ' 空 tile：只写回初始值
                Dim rows = py0

                While rows <= py1
                    Dim dstT = rows * imgW + px0
                    Dim cnt = px1 - px0 + 1

                    Array.Clear(image, dstT * 3, cnt * 3)
                    Array.Fill(transmittance, 1.0, dstT, cnt)
                    rows += 1
                End While

                Return
            End If

            scratch.ClearForward()

            Dim screen = request.Screen
            Dim alphaMax = options.AlphaMax
            Dim fast = options.ExpMode = ExpMode.Fast
            Dim cutoff = options.RadiusSigma * options.RadiusSigma

            For gi = g0 To g1 - 1
                Dim idx = request.TileGaussians(gi)

                Dim x0 = std.Max(screen.RectX0(idx), px0)
                Dim x1 = std.Min(screen.RectX1(idx), px1)
                Dim y0 = std.Max(screen.RectY0(idx), py0)
                Dim y1 = std.Min(screen.RectY1(idx), py1)

                If x0 > x1 OrElse y0 > y1 Then Continue For

                Dim a = screen.ConicA(idx)
                Dim b = screen.ConicB(idx)
                Dim c = screen.ConicC(idx)
                Dim op = screen.Opacity(idx)
                Dim mu = screen.MeanX(idx)
                Dim mv = screen.MeanY(idx)

                Dim cr = screen.ColorR(idx)
                Dim cg = screen.ColorG(idx)
                Dim cb = screen.ColorB(idx)

                Dim dqStep = 2 * a

                For y = y0 To y1
                    Dim dy = y - mv
                    Dim dx = x0 - mu

                    Dim q = a * dx * dx + 2 * b * dx * dy + c * dy * dy
                    Dim dq = 2 * (a * dx + b * dy) + a
                    Dim off = (y - py0) * ts + (x0 - px0)

                    For x = x0 To x1
                        Dim t = scratch.T(off)

                        If t > 0.000001 AndAlso q <= cutoff Then
                            Dim e = If(fast, FastExp(-0.5 * q), std.Exp(-0.5 * q))
                            Dim alpha = op * e

                            If alpha > 0.000001 Then
                                If alpha > alphaMax Then alpha = alphaMax

                                Dim w = alpha * t

                                scratch.R(off) += cr * w
                                scratch.G(off) += cg * w
                                scratch.B(off) += cb * w

                                scratch.T(off) = t * (1 - alpha)
                            End If
                        End If

                        q += dq
                        dq += dqStep
                        off += 1
                    Next
                Next
            Next

            ' ---- 写回全局缓冲 ----
            For y = py0 To py1
                Dim dst = (y * imgW + px0) * 3
                Dim src = (y - py0) * ts
                Dim dstT = y * imgW + px0

                For x = px0 To px1
                    image(dst) = scratch.R(src)
                    image(dst + 1) = scratch.G(src)
                    image(dst + 2) = scratch.B(src)
                    transmittance(dstT) = scratch.T(src)

                    dst += 3
                    dstT += 1
                    src += 1
                Next
            Next
        End Sub

        ''' <summary>反向：单个 tile 的解析梯度累加。</summary>
        Private Shared Sub BackwardTile(request As SplatRasterRequest, options As SplatRenderOptions,
                                        image As Double(), imageGradient As Double(),
                                        tile As Integer, scratch As TileScratch, grads As ChannelGradients)
            Dim imgW = request.Width
            Dim ts = request.TileSize
            Dim tx = tile Mod request.TilesX
            Dim ty = tile \ request.TilesX
            Dim px0 = tx * ts, py0 = ty * ts
            Dim px1 = std.Min(px0 + ts - 1, imgW - 1)
            Dim py1 = std.Min(py0 + ts - 1, request.Height - 1)

            Dim g0 = request.TileOffsets(tile)
            Dim g1 = request.TileOffsets(tile + 1)

            If g0 = g1 Then Return

            scratch.Clear()

            Dim screen = request.Screen
            Dim alphaMax = options.AlphaMax
            Dim fast = options.ExpMode = ExpMode.Fast
            Dim cutoff = options.RadiusSigma * options.RadiusSigma

            For gi = g0 To g1 - 1
                Dim idx = request.TileGaussians(gi)

                Dim x0 = std.Max(screen.RectX0(idx), px0)
                Dim x1 = std.Min(screen.RectX1(idx), px1)
                Dim y0 = std.Max(screen.RectY0(idx), py0)
                Dim y1 = std.Min(screen.RectY1(idx), py1)

                If x0 > x1 OrElse y0 > y1 Then Continue For

                Dim a = screen.ConicA(idx)
                Dim b = screen.ConicB(idx)
                Dim c = screen.ConicC(idx)
                Dim op = screen.Opacity(idx)
                Dim mu = screen.MeanX(idx)
                Dim mv = screen.MeanY(idx)

                Dim cr = screen.ColorR(idx)
                Dim cg = screen.ColorG(idx)
                Dim cb = screen.ColorB(idx)

                Dim dqStep = 2 * a
                Dim o3 = idx * 3
                Dim o2 = idx * 2

                For y = y0 To y1
                    Dim dy = y - mv
                    Dim dx0 = x0 - mu

                    Dim q = a * dx0 * dx0 + 2 * b * dx0 * dy + c * dy * dy
                    Dim dq = 2 * (a * dx0 + b * dy) + a
                    Dim off = (y - py0) * ts + (x0 - px0)
                    Dim px = x0 - mu

                    For x = x0 To x1
                        Dim t = scratch.T(off)

                        If t > 0.000001 AndAlso q <= cutoff Then
                            Dim e = If(fast, FastExp(-0.5 * q), std.Exp(-0.5 * q))
                            Dim alpha = op * e

                            If alpha > 0.000001 Then
                                Dim clamped = alpha > alphaMax

                                If clamped Then alpha = alphaMax

                                Dim pix = y * imgW + x
                                Dim gi3 = pix * 3
                                Dim gR = imageGradient(gi3)
                                Dim gG = imageGradient(gi3 + 1)
                                Dim gB = imageGradient(gi3 + 2)

                                If gR <> 0 OrElse gG <> 0 OrElse gB <> 0 Then
                                    Dim sTotal = image(gi3) * gR + image(gi3 + 1) * gG + image(gi3 + 2) * gB
                                    Dim cd = cr * gR + cg * gG + cb * gB
                                    Dim w = alpha * t

                                    scratch.P(off) += w * cd
                                    Dim rAfter = sTotal - scratch.P(off)
                                    Dim dAlpha = t * cd - rAfter / (1 - alpha)

                                    grads.Colors(o3) += w * gR
                                    grads.Colors(o3 + 1) += w * gG
                                    grads.Colors(o3 + 2) += w * gB

                                    If Not clamped Then grads.Opacities(idx) += dAlpha * e

                                    Dim dPower = -0.5 * alpha * dAlpha

                                    grads.Conic(o3) += dPower * px * px
                                    grads.Conic(o3 + 1) += dPower * 2 * px * dy
                                    grads.Conic(o3 + 2) += dPower * dy * dy

                                    Dim fX = dPower * 2 * (a * px + b * dy)
                                    Dim fY = dPower * 2 * (b * px + c * dy)

                                    grads.Means(o2) -= fX
                                    grads.Means(o2 + 1) -= fY

                                    scratch.T(off) = t * (1 - alpha)
                                Else
                                    scratch.T(off) = t * (1 - alpha)
                                End If
                            End If
                        End If

                        q += dq
                        dq += dqStep
                        px += 1
                        off += 1
                    Next
                Next
            Next
        End Sub

        Private Shared Sub Accumulate(local As ChannelGradients, grads As SplatGradients)
            Call ChannelGradients.Accumulate(local.Colors, grads.Colors)
            Call ChannelGradients.Accumulate(local.Opacities, grads.Opacities)
            Call ChannelGradients.Accumulate(local.Conic, grads.Conic)
            Call ChannelGradients.Accumulate(local.Means, grads.Means)
        End Sub

        ''' <summary>
        ''' 快速指数：x = k·ln2 + r，|r| ≤ ln2/2，用 6 阶多项式逼近 e^r 并左移 2^k。
        ''' 相对误差约 1e-7，比 <c>System.Math.Exp</c> 快 2~3 倍；用于像素内循环。
        ''' </summary>
        Public Shared Function FastExp(x As Double) As Double
            Const InvLn2 As Double = 1.4426950408889634R
            Const Ln2Hi As Double = 0.6931471805599453R

            If x < -700.0 Then Return 0.0
            If x > 700.0 Then Return Double.PositiveInfinity

            Dim k = std.Floor(x * InvLn2 + 0.5)
            Dim r = x - k * Ln2Hi

            Dim p = 1.0 + r * (1.0 + r * (0.5 + r * (0.16666666666666666R +
                            r * (0.041666666666666664R + r * (0.008333333333333333R +
                            r * 0.001388888888888889R)))))

            Dim ki = CLng(k)
            If ki < -1022 Then Return 0.0
            If ki > 1023 Then Return Double.PositiveInfinity

            Return p * BitConverter.Int64BitsToDouble((ki + 1023) << 52)
        End Function

    End Class

    ''' <summary>
    ''' 线程本地的通道梯度缓冲（只含光栅化阶段能直接算出的 4 组量）。
    ''' </summary>
    Friend NotInheritable Class ChannelGradients

        Public ReadOnly Colors As Double()
        Public ReadOnly Opacities As Double()
        Public ReadOnly Conic As Double()
        Public ReadOnly Means As Double()

        Public Sub New(n As Integer)
            Colors = New Double(n * 3 - 1) {}
            Opacities = New Double(n - 1) {}
            Conic = New Double(n * 3 - 1) {}
            Means = New Double(n * 2 - 1) {}
        End Sub

        Public Shared Sub Accumulate(source As Double(), target As Double())
            If source.Length = 0 Then Return

            Call Microsoft.VisualBasic.Math.SIMD.SimdEngine.AddInPlace(Of Double)(target, source)
        End Sub

    End Class
