Namespace Runtime

    ''' <summary>
    ''' 框架自带内核的函数名（与 Kernels\*.cu 中的 extern "C" 名称一一对应）。
    '''
    ''' demo 或业务方自行注册的内核名不在本表中，应在使用方自己的代码里定义，
    ''' 并通过 <see cref="KernelCatalog"/> 注册元数据。
    ''' </summary>
    Public Module KernelNames
        ' ---- Kernels\basic.cu ----
        Public Const VecAdd As String = "vecAddKernel"
        Public Const Saxpy As String = "saxpyKernel"

        ' ---- Kernels\reduce.cu ----
        Public Const ReduceSum As String = "reduceSumKernel"
        Public Const ReduceMax As String = "reduceMaxKernel"
        Public Const ReduceMin As String = "reduceMinKernel"
        Public Const ReduceFinalSum As String = "reduceFinalSumKernel"
        Public Const ReduceFinalMax As String = "reduceFinalMaxKernel"
        Public Const ReduceFinalMin As String = "reduceFinalMinKernel"

        ' ---- Kernels\elementwise.cu ----
        Public Const EwAdd As String = "ewAddKernel"
        Public Const EwSub As String = "ewSubKernel"
        Public Const EwMul As String = "ewMulKernel"
        Public Const EwDiv As String = "ewDivKernel"
        Public Const EwScale As String = "ewScaleKernel"
        Public Const EwAxpy As String = "ewAxpyKernel"
        Public Const EwRelu As String = "ewReluKernel"
        Public Const EwExp As String = "ewExpKernel"
        Public Const EwLog As String = "ewLogKernel"
        Public Const EwSqrt As String = "ewSqrtKernel"
        Public Const EwAbs As String = "ewAbsKernel"

        ' ---- Kernels\blas.cu ----
        Public Const Gemm As String = "gemmKernel"
        Public Const Gemv As String = "gemvKernel"
    End Module
End Namespace
