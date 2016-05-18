Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Parallel

    ''' <summary>
    ''' Parallel Linq query library for VisualBasic.
    ''' (用于高效率执行批量查询操作和用于检测操作超时的工具对象，请注意，为了提高查询的工作效率，请尽量避免在查询操作之中生成新的临时对象
    ''' 并行版本的LINQ查询和原始的线程操作相比具有一些性能上面的局限性)
    ''' </summary>
    ''' <remarks>在使用Parallel LINQ的时候，请务必要注意不能够使用Let语句操作共享变量，因为排除死锁的开销比较大</remarks>
    Public Class LQuerySchedule : Implements System.IDisposable

        ''' <summary>
        ''' 查询操作超时的时间阈值，单位为秒
        ''' </summary>
        ''' <remarks></remarks>
        Dim _TimeOut As Integer
        ''' <summary>
        ''' 查询操作的线程数目
        ''' </summary>
        ''' <remarks></remarks>
        Dim _n As Integer

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="timeout">查询操作超时的时间阈值，单位为秒</param>
        ''' <param name="n">这个参数通常是指CPU的核心数目，或者查询任务执行的最大线程数目</param>
        ''' <remarks></remarks>
        Sub New(Timeout As Integer, Optional n As Integer = -1)
            _n = n
            _TimeOut = Timeout

            If n <= 0 Then
                _n = 12  '默认是12条线程
            End If
        End Sub

        ''' <summary>
        ''' Get the number of processors on the current machine.(获取当前的系统主机的CPU核心数)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared ReadOnly Property CPU_NUMBER As Integer
            Get
                Return Environment.ProcessorCount
            End Get
        End Property

        ''' <summary>
        ''' The possible recommended threads of the linq based on you machine processors number, i'm not sure...
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared ReadOnly Property Recommended_NUM_THREADS As Integer
            Get
                Return Environment.ProcessorCount * 10
            End Get
        End Property

        Public Structure LQueryCacheHandle(Of T)
            Dim data As T
            Dim p_Handle As Integer

            Public Overrides Function ToString() As String
                Return String.Format("{0} ---> {1}", p_Handle, data.ToString)
            End Function
        End Structure

        Public Shared Function get_LQueryCacheHandles(Of T)(data As Generic.IEnumerable(Of T)) As LQueryCacheHandle(Of T)()
            Return (From i As Integer In data.Sequence Select New LQueryCacheHandle(Of T) With {.p_Handle = i, .data = data(i)}).ToArray
        End Function

        Public Function InvokeQuery(Of T, TOut)(data As Generic.IEnumerable(Of LQueryCacheHandle(Of T)), invoke As Func(Of T, TOut), Optional [Default] As TOut = Nothing) As TOut()
            Dim LQuery As LQueryHandle(Of T, TOut)() = __startLQuery(Of T, TOut)(data, invoke)
            Dim get_LQueryResult As LQueryResult(Of TOut)() = (From query_HWND As LQueryHandle(Of T, TOut) In LQuery.AsParallel
                                                               Let p As Integer = query_HWND.p
                                                               Let query As TOut = InternalTimeoutQuery(Of T, TOut)(invoke, handle:=query_HWND.Handle, [default]:=Function() [Default], Thread_ID:=query_HWND.TID)
                                                               Let result As LQueryResult(Of TOut) = New LQueryResult(Of TOut) With {.p_Handle = p, .Result = query}
                                                               Select result
                                                               Order By result.p_Handle Ascending).ToArray  '并行化的获取查询结果并判断查询是否超时
            Dim ChunkBuffer As TOut() = (From item As LQueryResult(Of TOut) In get_LQueryResult Select item.Result).ToArray '取出查询结果
            Return ChunkBuffer
        End Function

        Public Shared Function InvokeParallelLQuery(Of T, TOut)(data As Generic.IEnumerable(Of LQueryCacheHandle(Of T)), invoke As Func(Of T, TOut)) As TOut()
            Dim LQuery As LQueryHandle(Of T, TOut)() = __startLQuery(Of T, TOut)(data, invoke)
            Dim get_LQueryResult As LQueryResult(Of TOut)() = (From query_HWND As LQueryHandle(Of T, TOut) In LQuery.AsParallel
                                                               Let p As Integer = query_HWND.p
                                                               Let query As TOut = invoke(query_HWND.Source)
                                                               Let result As LQueryResult(Of TOut) = New LQueryResult(Of TOut) With {.p_Handle = p, .Result = query}
                                                               Select result
                                                               Order By result.p_Handle Ascending).ToArray  '并行化的获取查询结果并判断查询是否超时
            Dim ChunkBuffer As TOut() = (From item As LQueryResult(Of TOut) In get_LQueryResult Select item.Result).ToArray '取出查询结果
            Return ChunkBuffer
        End Function

        Public Function InvokeQuery(Of T, TOut)(data As Generic.IEnumerable(Of LQueryCacheHandle(Of T)), invoke As Func(Of T, TOut), Optional [Default] As Func(Of T, TOut) = Nothing) As TOut()
            Dim LQuery As LQueryHandle(Of T, TOut)() = __startLQuery(Of T, TOut)(data, invoke)
            Dim get_LQueryResult As LQueryResult(Of TOut)() = (From query_HWND As LQueryHandle(Of T, TOut) In LQuery.AsParallel
                                                               Let p As Integer = query_HWND.p
                                                               Let query As TOut = InternalTimeoutQuery(Of T, TOut)(invoke, handle:=query_HWND.Handle, [default]:=Function() [Default](query_HWND.Source), Thread_ID:=query_HWND.TID)
                                                               Let result As LQueryResult(Of TOut) = New LQueryResult(Of TOut) With {.p_Handle = p, .Result = query}
                                                               Select result
                                                               Order By result.p_Handle Ascending).ToArray  '并行化的获取查询结果并判断查询是否超时
            Dim ChunkBuffer As TOut() = (From item As LQueryResult(Of TOut) In get_LQueryResult Select item.Result).ToArray '取出查询结果
            Return ChunkBuffer
        End Function

        ''' <summary>
        ''' 当查询操作超时的时候，单条查询线程会返回<paramref name="Default">默认值</paramref>
        ''' </summary>
        ''' <typeparam name="Out">返回的值类型</typeparam>
        ''' <param name="source"></param>
        ''' <param name="invoke"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function InternalInvokeQuery(Of T, Out)(source As Generic.IEnumerable(Of T), invoke As Func(Of T, Out), Optional [Default] As Out = Nothing) As Out()
            Dim LQuery As LQueryHandle(Of T, Out)() = __startLQuery(Of T, Out)(source, invoke)
            Dim getLQueryResult As LQueryResult(Of Out)() =
                (From query_HWND As LQueryHandle(Of T, Out) In LQuery.AsParallel
                 Let p As Integer = query_HWND.p
                 Let query As Out = InternalTimeoutQuery(Of T, Out)(invoke, handle:=query_HWND.Handle, [default]:=Function() [Default], Thread_ID:=query_HWND.TID)
                 Let result As LQueryResult(Of Out) = New LQueryResult(Of Out) With {.p_Handle = p, .Result = query}
                 Select result
                 Order By result.p_Handle Ascending).ToArray  '并行化的获取查询结果并判断查询是否超时

            Dim ChunkBuffer As Out() = (From x As LQueryResult(Of Out) In getLQueryResult Select x.Result).ToArray '取出查询结果
            Return ChunkBuffer
        End Function

        Public Function InvokeQuery(Of T, Out)(source As IEnumerable(Of T), invoke As Func(Of T, Out), Optional [Default] As Out = Nothing) As Out()
            Dim ChunkTemp As T()() = source.Split(source.Count / Me._n)
            Dim LQuery As Out() = (From Chunk As T()
                                   In ChunkTemp.AsParallel
                                   Select InternalInvokeQuery(Of T, Out)(Chunk, invoke, [Default])).ToArray.MatrixToVector
            Return LQuery
        End Function

        ''' <summary>
        ''' 采取非并行化的方式启动计算线程
        ''' </summary>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="invoke"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function __startLQuery(Of T, TOut)(source As Generic.IEnumerable(Of T), invoke As Func(Of T, TOut)) As LQueryHandle(Of T, TOut)()
            Dim LQuery As LQueryHandle(Of T, TOut)() =
                (From p As Integer In source.Sequence
                 Let item As T = source(p)
                 Let tid As String = item.ToString
                 Let queryHandle As System.IAsyncResult = invoke.BeginInvoke(item, Nothing, Nothing)
                 Let handle = New LQueryHandle(Of T, TOut) With {
                     .Handle = queryHandle,
                     .Query = invoke,
                     .TID = tid,
                     .p = p,
                     .Source = item
                 }
                 Select handle).ToArray  '使用非并行化的方法启动查询以提高查询性能
            Return LQuery
        End Function

        ''' <summary>
        ''' 采取非并行化的方式启动计算线程
        ''' </summary>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="invoke"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function __startLQuery(Of T, TOut)(source As Generic.IEnumerable(Of LQueryCacheHandle(Of T)),
                                                          invoke As Func(Of T, TOut)) As LQueryHandle(Of T, TOut)()
            Dim LQuery As LQueryHandle(Of T, TOut)() =
                (From cache As LQueryCacheHandle(Of T)
                 In source
                 Let tid As String = cache.ToString
                 Let queryHandle As IAsyncResult = invoke.BeginInvoke(cache.data, Nothing, Nothing)
                 Let handle = New LQueryHandle(Of T, TOut) With {
                     .Handle = queryHandle,
                     .Query = invoke,
                     .TID = tid,
                     .p = cache.p_Handle,
                     .Source = cache.data
                 }
                 Select handle).ToArray  '使用非并行化的方法启动查询以提高查询性能
            Return LQuery
        End Function

        ''' <summary>
        ''' 当需要进行超大规模的查询的时候，请使用本方法，本方法的的思想是将查询操作分解为多于CPU核心数目的线程池进行批量计算
        ''' </summary>
        ''' <typeparam name="TOut"></typeparam>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function InvokeQuery_UltraLargeSize(Of T, TOut)(source As Generic.IEnumerable(Of T), invoke As Func(Of T, TOut)) As TOut()
            Dim s_ChunkBuffer As T()() = source.Split(source.Count / Recommended_NUM_THREADS)
            Dim ChunkBuffer = (From i As Integer In s_ChunkBuffer.Sequence Select handle = i, data = s_ChunkBuffer(i)).ToArray
            Dim InternalInvoke = Function(chunk As T()) As TOut()
                                     Return (From obj As T In chunk Select invoke(obj)).ToArray
                                 End Function
            Dim LQuery = (From item In ChunkBuffer.AsParallel
                          Let Chunk As T() = item.data
                          Let ayHandle = InternalInvoke.BeginInvoke(Chunk, Nothing, Nothing)
                          Select ayHandle, item.handle).ToArray
            Dim EndInvokeLQuery = (From item In LQuery Select Chunk = InternalInvoke.EndInvoke(item.ayHandle), item.handle Order By handle Ascending).ToArray
            Dim Result = (From item In EndInvokeLQuery Select item.Chunk).ToArray.MatrixToVector
            Return Result
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="Out"></typeparam>
        ''' <param name="Collection"></param>
        ''' <param name="invoke"></param>
        ''' <param name="get_Default">当查询超时的时候，使用本方法获得一个默认值</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function InternalInvokeQuery_p(Of T, Out)(Collection As Generic.IEnumerable(Of T), invoke As Func(Of T, Out), get_Default As Func(Of T, Out)) As Out()
            Dim LQuery As LQueryHandle(Of T, Out)() = __startLQuery(Of T, Out)(Collection, invoke)
            Dim get_LQueryResult As LQueryResult(Of Out)() = (
            From queryHandle In LQuery.AsParallel
            Let p As Integer = queryHandle.p
            Let query As Out = InternalTimeoutQuery(Of T, Out)(invoke, handle:=queryHandle.Handle, [default]:=Function() get_Default(queryHandle.Source), Thread_ID:=queryHandle.TID)
            Let result As LQueryResult(Of Out) = New LQueryResult(Of Out) With {.p_Handle = p, .Result = query}
            Select result
            Order By result.p_Handle Ascending).ToArray  '并行化的获取查询结果并判断查询是否超时
            Dim ChunkBuffer As Out() = (From item As LQueryResult(Of Out) In get_LQueryResult Select item.Result).ToArray '取出查询结果
            Return ChunkBuffer
        End Function

        ''' <summary>
        ''' 非并行版本
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="Out"></typeparam>
        ''' <param name="Collection"></param>
        ''' <param name="invoke"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function InvokeQuery(Of T, Out)(Collection As Generic.IEnumerable(Of T), invoke As Func(Of T, Out)) As Out()
            Dim LQuery = (From obj As T In Collection Select invoke(obj)).ToArray
            Return LQuery
        End Function

        Public Function InvokeQuery_p(Of T, Out)(Collection As Generic.IEnumerable(Of T), invoke As Func(Of T, Out), get_Default As Func(Of T, Out)) As Out()
            Dim ChunkTemp = Collection.Split(Collection.Count / Me._n)
            Dim LQuery = (From Chunk In ChunkTemp.AsParallel Select InternalInvokeQuery_p(Of T, Out)(Chunk, invoke, get_Default)).ToArray.MatrixToVector
            Return LQuery
        End Function

        Private Structure LQueryHandle(Of T, TOut)
            Dim Handle As System.IAsyncResult
            Dim TID As String
            Dim Query As Func(Of T, TOut)
            Dim Source As T
            Dim p As Integer

            Public Overrides Function ToString() As String
                Return String.Format("[{0}]  {1}", TID, Handle.ToString)
            End Function
        End Structure

        Private Structure LQueryResult(Of TOut)
            Dim p_Handle As Integer
            Dim Result As TOut

            Public Overrides Function ToString() As String
                Return String.Format("[{0}] ==> {1}", p_Handle, Result.ToString)
            End Function
        End Structure

        Public Overrides Function ToString() As String
            Return String.Format("TIME_OUT query schedule operations in time threshold {1}(s).", _TimeOut)
        End Function

        Private Function InternalTimeoutQuery(Of T, Out)(invoke As Func(Of T, Out), handle As System.IAsyncResult, [default] As Func(Of Out), Thread_ID As String) As Out
            For p As Integer = 0 To _TimeOut

                If handle.IsCompleted Then
                    GoTo END_INVOKE
                End If

                Call System.Threading.Thread.Sleep(1000)  '休眠1秒
            Next

            If handle.IsCompleted Then
END_INVOKE:     Dim rtvl As Out = invoke.EndInvoke(handle)
                Return rtvl
            Else
                '当前的这条线程操作超时，则输出错误信息
                Call Console.WriteLine("[DEBUG] LINQ thread ""{0}"" operation timeout!", Thread_ID)
                Call handle.InvokeSet(CType(Nothing, IAsyncResult))  '放弃该条查询线程

                Dim rtvl As Out = [default]()
                Return rtvl
            End If
        End Function

        ''' <summary>
        ''' .NET 4.6之中的并行LINQ失效了？？？？？？
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="Input"></param>
        ''' <param name="Handle"></param>
        ''' <returns></returns>
        Public Shared Function LQuery(Of T, TOut)(Input As Generic.IEnumerable(Of T), Handle As Func(Of T, TOut)) As TOut()
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Start schedule task pool for {GetType(T).FullName }   ---->  {GetType(TOut).FullName }")
            Dim HandleHash = (From HandleData As T In Input Select OutAr = New Tasks.Task(Of T, TOut)(HandleData, Handle).Start).ToArray
            Call Console.WriteLine($"[DEBUG {Now.ToString }] Start task pool job completed!, waiting for data processing....")
            Dim Out = (From HandleHashAr In HandleHash Select HandleHashAr.GetValue).ToArray
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Task job done!")
            Return Out
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

        ''' <summary>
        ''' DEBUG
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function PBS_TEST() As String
            Dim invoke_handle = Function(n As Integer) New Integer(n - 1) {}
            Dim sw As Stopwatch = Stopwatch.StartNew
            Dim LQuery = (From i As Integer In Integer.MaxValue.Sequence Let s As Integer() = invoke_handle(i) Select s).ToArray
            Call Console.WriteLine("Normal {0}ms", sw.ElapsedMilliseconds)

            Using PBS As LQuerySchedule = New LQuerySchedule(10 * 60, Recommended_NUM_THREADS)
                sw = Stopwatch.StartNew
                LQuery = LQuerySchedule.InvokeQuery(Integer.MaxValue.Sequence, invoke_handle)
                Call Console.WriteLine("Timeout query {0}ms", sw.ElapsedMilliseconds)

                sw = Stopwatch.StartNew
                LQuery = PBS.InvokeQuery_UltraLargeSize(Integer.MaxValue.Sequence, invoke_handle)
                Call Console.WriteLine("Ultra large size query {0}ms", sw.ElapsedMilliseconds)
            End Using

            Return ""
        End Function
    End Class
End Namespace