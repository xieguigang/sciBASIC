Imports System.Runtime.CompilerServices

''' <summary>
''' A unified parallelism configuration for the UMAP pipeline
''' </summary>
''' <remarks>
''' All of the hot spots inside this UMAP implementation shares the same 
''' parallelism configuration object: the random projection forest build,
''' the nearest neighbour descent, the heap sort, the fuzzy simplicial set
''' construction and the SGD optimization loop.
''' 
''' note that the parallel optimization of the SGD layout step is a Hogwild 
''' style optimization, which means the embedding result is no longer 
''' bit-level reproducible between each run.
''' </remarks>
Public Class ParallelConfig

    ''' <summary>
    ''' set this property to FALSE for force all of the computation steps 
    ''' to be executed in sequence mode.
    ''' </summary>
    ''' <returns></returns>
    Public Property Enabled As Boolean = True
    ''' <summary>
    ''' the max number of the worker threads
    ''' </summary>
    ''' <returns></returns>
    Public Property MaxDegreeOfParallelism As Integer = App.CPUCoreNumbers
    ''' <summary>
    ''' The task will be degraded into the sequence mode when the work size
    ''' is less than this threshold value.
    ''' 
    ''' (the thread scheduling cost may be larger than the computing cost 
    ''' itself for a small size of the workload, so that a threshold value 
    ''' is required for avoid the performance regression on the small 
    ''' dataset.)
    ''' </summary>
    ''' <returns></returns>
    Public Property MinWorkSize As Integer = 1024

    ''' <summary>
    ''' A pre-defined config which disables all of the parallel computing 
    ''' inside the UMAP pipeline.
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property Sequential As ParallelConfig =
        New ParallelConfig With {
            .Enabled = False
        }

    ''' <summary>
    ''' get the effective parallelism degree of a given workload
    ''' </summary>
    ''' <param name="workSize">
    ''' the size of the pending workload, usually this parameter value is 
    ''' the number of the elements that will be processed by the loop.
    ''' </param>
    ''' <returns>
    ''' a value that is less than or equals to 1 means the task should be 
    ''' executed in sequence mode.
    ''' </returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function EffectiveDegree(Optional workSize As Integer = Integer.MaxValue) As Integer
        If Not Enabled Then
            Return 1
        ElseIf workSize < MinWorkSize Then
            Return 1
        ElseIf MaxDegreeOfParallelism <= 1 Then
            Return 1
        Else
            Return MaxDegreeOfParallelism
        End If
    End Function

    ''' <summary>
    ''' is the current workload capable of running in parallel?
    ''' </summary>
    ''' <param name="workSize"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function CanParallel(Optional workSize As Integer = Integer.MaxValue) As Boolean
        Return EffectiveDegree(workSize) > 1
    End Function

    ''' <summary>
    ''' Create a <see cref="ParallelOptions"/> object for run the 
    ''' <see cref="System.Threading.Tasks.Parallel"/> loops.
    ''' </summary>
    ''' <param name="workSize"></param>
    ''' <returns></returns>
    Public Function Options(Optional workSize As Integer = Integer.MaxValue) As ParallelOptions
        Return New ParallelOptions With {
            .MaxDegreeOfParallelism = EffectiveDegree(workSize)
        }
    End Function

    Public Overrides Function ToString() As String
        Return $"[parallel] {If(Enabled, "on", "off")}, workers={MaxDegreeOfParallelism}, min_workload={MinWorkSize}"
    End Function

End Class
