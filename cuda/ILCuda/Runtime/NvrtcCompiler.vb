' ------------------------------------------------------------------------
' 内核镜像（PTX / cubin）的多通道获取
'
' 背景：CUDA 工具包版本与显卡驱动版本必须匹配。
'   例如本机驱动 572.83 只支持 CUDA 12.8，而安装的是 CUDA 13.3 工具包，
'   13.x 的 NVRTC 产出的 PTX（ISA 9.0）无法被该驱动 JIT 编译，
'   cuModuleLoadData 会返回 CUDA_ERROR_INVALID_PTX (218)。
'
' 因此这里采用"多通道自动降级"策略：
'   1) 扫描系统中所有 nvrtc64_*.dll，按版本号排序（优先不高于驱动版本的）
'   2) 逐个尝试：编译 -> cuModuleLoadData 实测 -> 成功即采用
'   3) 全部失败则尝试加载预编译的 .ptx / .cubin 文件
'   4) 仍然失败则由调用方回退到 CPU 实现
' ------------------------------------------------------------------------

Imports System.IO
Imports Microsoft.VisualBasic.Computing.ILCuda.Kernels

Namespace Runtime

    Public Module NvrtcCompiler

        ' 从高到低的架构候选表（major * 10 + minor）
        Private ReadOnly ArchLadder As Integer() = {90, 89, 87, 86, 80, 75, 70, 61, 53, 52, 50}

        ''' <summary>
        ''' 获取一份可以被当前驱动加载的内核镜像。
        ''' </summary>
        ''' <param name="device">目标设备（用于推导 -arch）</param>
        ''' <param name="diagnostics">过程日志，失败时用于向用户解释原因</param>
        ''' <param name="explicitNvrtc">--nvrtc 显式指定的动态库路径</param>
        ''' <param name="explicitImage">--cubin 显式指定的预编译镜像路径</param>
        ''' <param name="image">成功时返回镜像描述</param>
        ''' <param name="loadedModule">成功时返回已加载的模块</param>
        Public Function TryBuild(device As CudaDevice,
                                 diagnostics As List(Of String),
                                 explicitNvrtc As String,
                                 explicitImage As String,
                                 ByRef image As KernelImage,
                                 ByRef loadedModule As CudaModule,
                                 Optional forceImage As Boolean = False) As Boolean
            image = Nothing
            loadedModule = Nothing

            ' ---------- 通道 0：显式指定镜像时优先使用 ----------
            If Not String.IsNullOrWhiteSpace(explicitImage) Then
                If TryLoadImages(explicitImage, diagnostics, image, loadedModule, forceImage) Then Return True
            End If

            ' ---------- 通道 1/2：NVRTC 运行时编译 ----------
            Dim sourceFiles = KernelSources.All()
            Dim source = KernelSources.CombinedSource()

            If String.IsNullOrWhiteSpace(source) Then
                diagnostics.Add("程序资源中没有任何 .cu 内核源码，跳过 NVRTC 编译。")
            Else
                ' 把参与编译的源码列出来：外部注册的 .cu 若与框架内核重名会在此暴露
                diagnostics.Add($"[源码] 参与编译的内核源码共 {sourceFiles.Count} 个：" &
                                String.Join(", ", sourceFiles.Select(Function(f) f.ToString())))

                Dim candidates = FindCandidates(explicitNvrtc)

                If candidates.Count = 0 Then
                    diagnostics.Add("没有找到任何 NVRTC 动态库（nvrtc64_*.dll）：请安装 CUDA Toolkit，或用 --nvrtc <dll> 显式指定。")
                End If

                For Each candidate In candidates
                    Dim nvrtc As NvrtcLibrary = Nothing
                    Dim loadError As String = Nothing

                    If Not NvrtcLibrary.TryLoad(candidate, nvrtc, loadError) Then
                        diagnostics.Add($"[NVRTC] {candidate} 无法载入: {loadError}")
                        Continue For
                    End If

                    diagnostics.Add($"[NVRTC] 试用 {nvrtc.VersionText} ({candidate})")

                    For Each arch In CandidateArchs(device)
                        Dim compiled As Byte() = Nothing
                        Dim isCubin As Boolean = False
                        Dim log As String = Nothing
                        Dim status = nvrtc.Compile(source, "ilcuda_kernels.cu",
                                                   New String() {"-arch=" & arch}, compiled, isCubin, log)

                        If status <> nvrtcResult.NVRTC_SUCCESS Then
                            diagnostics.Add($"        arch={arch} 编译失败: {status}{TrimLog(log)}")
                            Continue For
                        End If

                        Dim cudaModule As CudaModule = Nothing
                        Dim moduleError As String = Nothing

                        If CudaModule.TryLoad(compiled, cudaModule, moduleError) Then
                            image = New KernelImage With {
                                .Image = compiled,
                                .Source = $"NVRTC {nvrtc.VersionText} ({If(isCubin, "cubin", "PTX")})",
                                .Arch = arch,
                                .Detail = candidate
                            }
                            loadedModule = cudaModule
                            diagnostics.Add($"        arch={arch} 编译并加载成功（{If(isCubin, "cubin", "PTX")} {compiled.Length} 字节）")
                            Return True
                        End If

                        diagnostics.Add($"        arch={arch} 编译成功，但模块加载失败: {moduleError}")

                        ' PTX 版本与驱动不兼容时，换架构也无效，直接进入下一个 NVRTC 版本
                        If IsCompatibilityError(moduleError) Then
                            diagnostics.Add("        该 NVRTC 版本产出的 PTX 与当前驱动不兼容，尝试下一个候选。")
                            Exit For
                        End If
                    Next
                Next
            End If

            ' ---------- 通道 3：预编译的 .ptx / .cubin 兜底 ----------
            Return TryLoadImages(explicitImage, diagnostics, image, loadedModule, forceImage)
        End Function

        ''' <summary>尝试加载预编译镜像（--cubin 指定路径，或程序目录 kernels\ 下的 .ptx/.cubin）</summary>
        Private Function TryLoadImages(explicitImage As String,
                                       diagnostics As List(Of String),
                                       ByRef image As KernelImage,
                                       ByRef loadedModule As CudaModule,
                                       Optional forceImage As Boolean = False) As Boolean
            For Each imageFile In FindImageFiles(explicitImage)
                Dim bytes As Byte()

                Try
                    bytes = File.ReadAllBytes(imageFile)
                Catch ex As Exception
                    diagnostics.Add($"[镜像] {imageFile} 读取失败: {ex.Message}")
                    Continue For
                End Try

                Dim cudaModule As CudaModule = Nothing
                Dim moduleError As String = Nothing

                If CudaModule.TryLoad(bytes, cudaModule, moduleError, forceImage) Then
                    image = New KernelImage With {
                        .Image = bytes,
                        .Source = "预编译镜像",
                        .Arch = "",
                        .Detail = imageFile
                    }
                    loadedModule = cudaModule
                    diagnostics.Add($"[镜像] {imageFile} 加载成功")
                    Return True
                End If

                diagnostics.Add($"[镜像] {imageFile} 加载失败: {moduleError}")
            Next

            Return False
        End Function

        Private Function TrimLog(log As String) As String
            If String.IsNullOrWhiteSpace(log) Then Return String.Empty

            Dim text = log.Trim()
            Const maxLength As Integer = 800

            If text.Length > maxLength Then text = text.Substring(0, maxLength) & " ..."

            Return Environment.NewLine & "          " & text.Replace(vbLf, vbLf & "          ")
        End Function

        ''' <summary>判断是否为"PTX/镜像与驱动不兼容"这类换架构也救不回来的错误</summary>
        Private Function IsCompatibilityError(errorText As String) As Boolean
            If String.IsNullOrEmpty(errorText) Then Return False

            Return errorText.Contains("CUDA_ERROR_INVALID_PTX") OrElse
                   errorText.Contains("CUDA_ERROR_UNSUPPORTED_PTX_VERSION") OrElse
                   errorText.Contains("CUDA_ERROR_NO_BINARY_FOR_GPU")
        End Function

        ''' <summary>
        ''' 按设备计算能力推导 -arch 候选（从高到低）。
        '''
        ''' 关键点：NVRTC 的 -arch 既接受虚拟架构 compute_XX，也接受真实架构 sm_XX。
        '''   * sm_XX      -> NVRTC 内部直接调用 ptxas 产出 cubin（SASS），
        '''                  加载时不依赖驱动的 PTX JIT，兼容性更好；
        '''   * compute_XX -> 产出 PTX，由驱动在加载时 JIT 编译，
        '''                  一旦"工具包版本 > 驱动支持的 CUDA 版本"就会被拒绝。
        ''' 因此每个架构都先试 sm_XX，再试 compute_XX。
        ''' </summary>
        Public Function CandidateArchs(device As CudaDevice) As List(Of String)
            Dim deviceKey = device.CapabilityMajor * 10 + device.CapabilityMinor
            Dim list As New List(Of String)()

            ' 优先使用设备自身的架构
            list.Add($"sm_{device.CapabilityMajor}{device.CapabilityMinor}")
            list.Add($"compute_{device.CapabilityMajor}{device.CapabilityMinor}")

            ' 再往下退两档（NVRTC 可能不支持过旧的架构，失败会被记录并跳过）
            Dim lower = ArchLadder.Where(Function(k) k < deviceKey).Take(2).ToList()

            For Each key In lower
                list.Add($"sm_{key}")
                list.Add($"compute_{key}")
            Next

            Return list
        End Function

        ' ------------------------------------------------------------------
        ' NVRTC 动态库候选发现
        ' ------------------------------------------------------------------

        ''' <summary>扫描系统中所有可能的 nvrtc64_*.dll，返回按优先级排序的路径</summary>
        Public Function FindCandidates(explicitPath As String) As List(Of String)
            Dim found As New List(Of String)()

            ' 1) 命令行显式指定
            If Not String.IsNullOrWhiteSpace(explicitPath) Then
                AddFile(found, explicitPath)
            End If

            ' 2) CUDA_PATH 环境变量
            Dim cudaPath = Environment.GetEnvironmentVariable("CUDA_PATH")
            If Not String.IsNullOrWhiteSpace(cudaPath) Then
                AddToolkitBin(found, cudaPath)
            End If

            ' 3) 默认安装目录下的所有 CUDA 工具包（版本从新到旧）
            Dim toolkitRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "NVIDIA GPU Computing Toolkit", "CUDA")

            If Directory.Exists(toolkitRoot) Then
                Dim versions = Directory.EnumerateDirectories(toolkitRoot) _
                    .OrderByDescending(Function(d) d, StringComparer.OrdinalIgnoreCase)

                For Each toolkitDir In versions
                    AddToolkitBin(found, toolkitDir)
                Next
            End If

            ' 4) PATH 中的目录
            Dim pathEnv = Environment.GetEnvironmentVariable("PATH")
            If Not String.IsNullOrWhiteSpace(pathEnv) Then
                For Each pathEntry In pathEnv.Split(Path.PathSeparator)
                    AddFromDirectory(found, pathEntry)
                Next
            End If

            ' 5) 应用程序目录（便于把 nvrtc 拷到程序旁边随包分发）
            AddFromDirectory(found, AppContext.BaseDirectory)
            AddFromDirectory(found, Environment.CurrentDirectory)

            ' 6) 其它 NVIDIA 应用顺带安装的 NVRTC（例如 NVIDIA Canvas 自带的 11.2）
            For Each root In New String() {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NVIDIA Corporation"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "NVIDIA Corporation")
            }
                If Directory.Exists(root) Then
                    For Each vendorDir In Directory.EnumerateDirectories(root)
                        AddFromDirectory(found, vendorDir)
                    Next
                End If
            Next

            Return SortByPreference(found)
        End Function

        Private Sub AddToolkitBin(found As List(Of String), root As String)
            AddFromDirectory(found, Path.Combine(root, "bin", "x64"))
            AddFromDirectory(found, Path.Combine(root, "bin"))
        End Sub

        Private Sub AddFromDirectory(found As List(Of String), folder As String)
            If String.IsNullOrWhiteSpace(folder) OrElse Not Directory.Exists(folder) Then Return

            Try
                For Each dll In Directory.EnumerateFiles(folder, "nvrtc64_*.dll")
                    AddFile(found, dll)
                Next
            Catch
            End Try
        End Sub

        Private Sub AddFile(found As List(Of String), filePath As String)
            If String.IsNullOrWhiteSpace(filePath) Then Return

            For Each existing In found
                If String.Equals(existing, filePath, StringComparison.OrdinalIgnoreCase) Then Return
            Next

            found.Add(filePath)
        End Sub

        ''' <summary>
        ''' 排序策略：优先"版本不高于驱动支持的 CUDA 版本"的动态库，
        ''' 同组内版本号从新到旧；无法判断版本的排在最后。
        ''' </summary>
        Private Function SortByPreference(found As List(Of String)) As List(Of String)
            Dim driverKey As Integer

            Try
                driverKey = CudaDevice.DriverVersionKey()
            Catch
                driverKey = Integer.MaxValue
            End Try

            Dim scored As New List(Of KeyValuePair(Of String, Integer))()

            For Each path In found
                scored.Add(New KeyValuePair(Of String, Integer)(path, NvrtcLibrary.VersionNumberFromFileName(path)))
            Next

            scored.Sort(Function(a, b)
                            Dim fitA = If(a.Value > 0 AndAlso a.Value <= driverKey, 0, 1)
                            Dim fitB = If(b.Value > 0 AndAlso b.Value <= driverKey, 0, 1)

                            If fitA <> fitB Then Return fitA.CompareTo(fitB)

                            ' 版本从新到旧
                            If a.Value <> b.Value Then Return b.Value.CompareTo(a.Value)

                            Return String.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase)
                        End Function)

            Return scored.Select(Function(x) x.Key).ToList()
        End Function

        ''' <summary>查找预编译镜像文件（--cubin 指定的路径，或程序目录下的 kernels/*.ptx|*.cubin）</summary>
        Public Function FindImageFiles(explicitImage As String) As List(Of String)
            Dim result As New List(Of String)()

            If Not String.IsNullOrWhiteSpace(explicitImage) Then
                If File.Exists(explicitImage) Then result.Add(explicitImage)
                Return result
            End If

            For Each searchDir In New String() {
                Path.Combine(AppContext.BaseDirectory, "kernels"),
                AppContext.BaseDirectory,
                Path.Combine(Environment.CurrentDirectory, "kernels")
            }
                If Not Directory.Exists(searchDir) Then Continue For

                For Each pattern In New String() {"*.ptx", "*.cubin"}
                    For Each candidate In Directory.EnumerateFiles(searchDir, pattern).OrderBy(Function(f) f)
                        If Not result.Any(Function(r) String.Equals(r, candidate, StringComparison.OrdinalIgnoreCase)) Then
                            result.Add(candidate)
                        End If
                    Next
                Next
            Next

            Return result
        End Function
    End Module
End Namespace
