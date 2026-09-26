' ------------------------------------------------------------------------
' CUDA 上下文的线程绑定
'
' CUDA 的"当前上下文"是线程局部状态：CudaContext 在构造时只对创建它的那条线程调用了
' cuCtxSetCurrent。之后换一条线程（线程池、工作线程、UI 线程）再去分配显存 / 拷贝 /
' 启动内核 / 加载模块，驱动会直接返回 CUDA_ERROR_INVALID_CONTEXT (201) ——
' 这类错误的表现特别迷惑：第一次调用完全正常，第二次换个线程就失败。
'
' 这个类把进程内已知的上下文登记起来，并提供一个 EnsureCurrent()：底层驱动调用之前
' 先确认"本线程的当前上下文"就是自己要用的那个，不是就绑定一次。
' 命中时只有一次 cuCtxGetCurrent（纳秒级），没有额外动作。
' ------------------------------------------------------------------------

Namespace Runtime

    ''' <summary>
    ''' 进程级 CUDA 上下文登记表 + 当前线程的上下文绑定。
    ''' </summary>
    ''' <remarks>
    ''' 使用约定：
    ''' <list type="bullet">
    '''   <item><see cref="CudaEngine.TryCreate"/> 在建立上下文时自动登记；</item>
    '''   <item>所有底层显存 / 内核 / 模块 / 流 / 事件的驱动调用之前调用
    '''         <see cref="EnsureCurrent"/>，于是<b>任何线程</b>都能安全使用同一个引擎；</item>
    '''   <item>引擎释放时自动注销。</item>
    ''' </list>
    ''' 
    ''' 登记表里出现多个上下文时（例如同进程为两块显卡各建一个引擎），
    ''' <see cref="EnsureCurrent"/> 绑定的是<b>最近创建</b>的那一个 —— 与
    ''' <c>CudaTensor</c> 只持有一个活跃引擎的用法一致。
    ''' </remarks>
    Public NotInheritable Class CudaRuntime

        Private Shared ReadOnly _sync As New Object()
        Private Shared ReadOnly _contexts As New List(Of CudaContext)()
        Private Shared _default As CudaContext

        ''' <summary>登记一个上下文（由 <see cref="CudaEngine.TryCreate"/> 调用）。</summary>
        Friend Shared Sub [Register](context As CudaContext)
            If context Is Nothing Then Return

            SyncLock _sync
                If Not _contexts.Contains(context) Then
                    Call _contexts.Add(context)
                End If

                _default = context
            End SyncLock
        End Sub

        ''' <summary>注销一个上下文（引擎释放时调用）。</summary>
        Friend Shared Sub Unregister(context As CudaContext)
            If context Is Nothing Then Return

            SyncLock _sync
                Call _contexts.Remove(context)

                If _default Is context Then
                    _default = If(_contexts.Count > 0, _contexts(_contexts.Count - 1), Nothing)
                End If
            End SyncLock
        End Sub

        ''' <summary>进程内是否已经登记过 CUDA 上下文？</summary>
        Public Shared ReadOnly Property HasContext As Boolean
            Get
                Return _default IsNot Nothing
            End Get
        End Property

        ''' <summary>当前活跃的上下文（未登记时为 ``Nothing``）。</summary>
        Public Shared ReadOnly Property CurrentContext As CudaContext
            Get
                Return _default
            End Get
        End Property

        ''' <summary>
        ''' 确保当前线程已经绑定到活跃的 CUDA 上下文。
        ''' </summary>
        ''' <remarks>
        ''' <b>命中路径几乎没有成本</b>：一次 <c>cuCtxGetCurrent</c> 与句柄比较，
        ''' 相同就直接返回；只有"本线程尚未绑定"或"绑到了别的上下文"时才会调用
        ''' <c>cuCtxSetCurrent</c>。因此可以放心地放在每一个底层驱动调用之前。
        ''' </remarks>
        Friend Shared Sub EnsureCurrent()
            Dim context As CudaContext = _default

            If context Is Nothing OrElse context.IsDisposed Then Return
            If context.IsCurrent() Then Return

            context.MakeCurrent()
        End Sub

    End Class

End Namespace
