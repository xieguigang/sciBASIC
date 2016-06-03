Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Parallel.Linq

    ''' <summary>
    ''' Parallel Linq query library for VisualBasic.
    ''' (用于高效率执行批量查询操作和用于检测操作超时的工具对象，请注意，为了提高查询的工作效率，请尽量避免在查询操作之中生成新的临时对象
    ''' 并行版本的LINQ查询和原始的线程操作相比具有一些性能上面的局限性)
    ''' </summary>
    ''' <remarks>
    ''' 在使用Parallel LINQ的时候，请务必要注意不能够使用Let语句操作共享变量，因为排除死锁的开销比较大
    ''' 
    ''' 在设计并行任务的时候应该遵循的一些原则:
    ''' 1. 假若每一个任务之间都是相互独立的话，则才可以进行并行化调用
    ''' 2. 在当前程序域之中只能够通过线程的方式进行并行化，对于时间较短的任务而言，非并行化会比并行化更加有效率
    ''' 3. 但是对于这些短时间的任务，仍然可以将序列进行分区合并为一个大型的长时间任务来产生并行化
    ''' 4. 对于长时间的任务，可以直接使用并行化Linq拓展执行并行化
    ''' 
    ''' 这个模块主要是针对大量的短时间的任务序列的并行化的，用户可以在这里配置线程的数量自由的控制并行化的程度
    ''' </remarks>
    Public Module LQuerySchedule

        ''' <summary>
        ''' 查询操作超时的时间阈值，单位为秒
        ''' </summary>
        ''' <remarks></remarks>
        Public Property TimeOut As Integer
        ''' <summary>
        ''' 查询操作的线程数目
        ''' </summary>
        ''' <remarks></remarks>
        Public Property NumThreads As Integer

        ''' <summary>
        ''' Get the number of processors on the current machine.(获取当前的系统主机的CPU核心数)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CPU_NUMBER As Integer
            Get
                Return Environment.ProcessorCount
            End Get
        End Property

        ''' <summary>
        ''' 假如小于0，则认为是自动配置，0被认为是单线程，反之直接返回
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Function AutoConfig(n As Integer) As Integer
            If n < 0 Then
                Return CPU_NUMBER
            ElseIf n = 0 OrElse n = 1 Then
                Return 1
            Else
                Return n
            End If
        End Function

        ''' <summary>
        ''' The possible recommended threads of the linq based on you machine processors number, i'm not sure...
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Recommended_NUM_THREADS As Integer
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

        Public Function get_LQueryCacheHandles(Of T)(data As Generic.IEnumerable(Of T)) As LQueryCacheHandle(Of T)()
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

        Public Function InvokeParallelLQuery(Of T, TOut)(data As Generic.IEnumerable(Of LQueryCacheHandle(Of T)), invoke As Func(Of T, TOut)) As TOut()
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
        Public Function InternalInvokeQuery(Of T, Out)(source As IEnumerable(Of T), invoke As Func(Of T, Out), Optional [Default] As Out = Nothing) As Out()
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
            Dim ChunkTemp As T()() = source.Split(source.Count / NumThreads)
            Dim LQuery As Out() = (From Chunk As T()
                                   In ChunkTemp.AsParallel
                                   Select InternalInvokeQuery(Of T, Out)(Chunk, invoke, [Default])).MatrixToVector
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
        Private Function __startLQuery(Of T, TOut)(source As Generic.IEnumerable(Of T), invoke As Func(Of T, TOut)) As LQueryHandle(Of T, TOut)()
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
        Private Function __startLQuery(Of T, TOut)(source As Generic.IEnumerable(Of LQueryCacheHandle(Of T)),
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
        Public Function InvokeQuery(Of T, Out)(Collection As Generic.IEnumerable(Of T), invoke As Func(Of T, Out)) As Out()
            Dim LQuery = (From obj As T In Collection Select invoke(obj)).ToArray
            Return LQuery
        End Function

        Public Function InvokeQuery_p(Of T, Out)(Collection As Generic.IEnumerable(Of T), invoke As Func(Of T, Out), get_Default As Func(Of T, Out)) As Out()
            Dim ChunkTemp = Collection.Split(Collection.Count / NumThreads)
            Dim LQuery = (From Chunk In ChunkTemp.AsParallel Select InternalInvokeQuery_p(Of T, Out)(Chunk, invoke, get_Default)).ToArray.MatrixToVector
            Return LQuery
        End Function

        Private Structure LQueryHandle(Of T, TOut)
            Dim Handle As IAsyncResult
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
        ''' 将大量的短时间的任务进行分区，合并，然后再执行并行化
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="inputs"></param>
        ''' <param name="task"></param>
        ''' <returns></returns>
        Public Iterator Function LQuery(Of T, TOut)(inputs As IEnumerable(Of T), task As Func(Of T, TOut), Optional parTokens As Integer = 20000) As IEnumerable(Of TOut)
            Call $"Start schedule task pool for {GetType(T).FullName}  -->  {GetType(TOut).FullName}".__DEBUG_ECHO

            Dim buf = TaskPartitions.Partitioning(inputs, parTokens, task)
            Dim LQueryInvoke = From part As Func(Of TOut())
                               In buf.AsParallel
                               Select part()

            For Each part As TOut() In LQueryInvoke
                For Each x As TOut In part
                    Yield x
                Next
            Next

            Call $"Task job done!".__DEBUG_ECHO
        End Function
    End Module
End Namespace