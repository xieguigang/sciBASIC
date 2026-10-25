' ---------------------------------------------------------------------------
' 光栅化几何准备：可见性剔除 → 深度排序 → tile 分箱
'
' 与后端（像素合成）解耦的原因：
'   这部分是纯几何/索引运算，与"在 GPU 还是 CPU 上做混合"无关；
'   任何后端（含宿主注入的 CUDA 后端）都消费同一份 request。
' ---------------------------------------------------------------------------

Imports std = System.Math

''' <summary>
''' 构建光栅化请求（投影 + 剔除 + 排序 + 分箱）。
''' </summary>
Public Module SplatRasterizer

    ''' <summary>
    ''' 对一个视图完成投影与分箱。
    ''' </summary>
    Public Function Build(model As GaussianModel, camera As Camera,
                          Optional options As SplatRenderOptions = Nothing) As SplatRasterRequest
            If options Is Nothing Then options = SplatRenderOptions.Default

            Dim screen = SplatProjection.Project(model, camera, options)
            Return Build(screen, options)
        End Function

    ''' <summary>
    ''' 从已算好的 <see cref="SplatScreenData"/> 构建请求（供多视图复用投影结果）。
    ''' </summary>
    Public Function Build(screen As SplatScreenData,
                          Optional options As SplatRenderOptions = Nothing) As SplatRasterRequest
            If options Is Nothing Then options = SplatRenderOptions.Default

            Dim request As New SplatRasterRequest With {
                .Width = screen.Width,
                .Height = screen.Height,
                .TileSize = std.Max(1, options.TileSize),
                .Screen = screen,
                .Options = options
            }

            request.TilesX = (screen.Width + request.TileSize - 1) \ request.TileSize
            request.TilesY = (screen.Height + request.TileSize - 1) \ request.TileSize

            Call BuildOrder(screen, request)
            Call BuildTiles(screen, request)

            Return request
        End Function

        ''' <summary>收集可见高斯并按深度升序（near → far）排序。</summary>
        Private Sub BuildOrder(screen As SplatScreenData, request As SplatRasterRequest)
            Dim n = screen.Count
            Dim count = screen.VisibleCount

            If count = 0 Then
                request.Order = New Integer() {}
                request.Screen.VisibleCount = 0
                Return
            End If

            Dim keys(count - 1) As Double
            Dim items(count - 1) As Integer
            Dim k = 0

            For i = 0 To n - 1
                If screen.Visible(i) Then
                    keys(k) = screen.CamZ(i)
                    items(k) = i
                    k += 1
                End If
            Next

            ' 键值排序：直接对 Double 键比较，不使用 Comparison(Of T) 委托
            Call Array.Sort(keys, items)

            request.Order = items
        End Sub

        ''' <summary>把每个高斯登记到它覆盖的所有 tile（计数 + 前缀和 + 填充）。</summary>
        Private Sub BuildTiles(screen As SplatScreenData, request As SplatRasterRequest)
            Dim t0 = PerfTrace.Begin()

            Dim ts = request.TileSize
            Dim tilesX = request.TilesX, tilesY = request.TilesY
            Dim tileCount = tilesX * tilesY
            Dim order = request.Order
            Dim counts(tileCount - 1) As Integer

            ' ---- 第一遍：计数 ----
            For k = 0 To order.Length - 1
                Dim idx = order(k)
                Dim tx0 = screen.RectX0(idx) \ ts
                Dim tx1 = screen.RectX1(idx) \ ts
                Dim ty0 = screen.RectY0(idx) \ ts
                Dim ty1 = screen.RectY1(idx) \ ts

                For ty = ty0 To ty1
                    Dim row = ty * tilesX
                    For tx = tx0 To tx1
                        counts(row + tx) += 1
                    Next
                Next
            Next

            ' ---- 前缀和 ----
            Dim offsets(tileCount) As Integer
            Dim running = 0

            For t = 0 To tileCount - 1
                offsets(t) = running
                running += counts(t)
            Next

            offsets(tileCount) = running

            ' ---- 第二遍：填充（按深度顺序写入，因此每个 tile 的列表天然有序）----
            Dim cursor(tileCount - 1) As Integer
            Call Array.Copy(offsets, cursor, tileCount)

            Dim fill(running - 1) As Integer

            For k = 0 To order.Length - 1
                Dim idx = order(k)
                Dim tx0 = screen.RectX0(idx) \ ts
                Dim tx1 = screen.RectX1(idx) \ ts
                Dim ty0 = screen.RectY0(idx) \ ts
                Dim ty1 = screen.RectY1(idx) \ ts

                For ty = ty0 To ty1
                    Dim row = ty * tilesX
                    For tx = tx0 To tx1
                        Dim slot = cursor(row + tx)
                        fill(slot) = idx
                        cursor(row + tx) = slot + 1
                    Next
                Next
            Next

            request.TileOffsets = offsets
            request.TileGaussians = fill

            Call PerfTrace.Done(PerfPhase.Binning, t0)
        End Sub

End Module
