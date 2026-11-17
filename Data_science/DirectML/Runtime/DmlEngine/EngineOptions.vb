' ------------------------------------------------------------------------
' 引擎组装选项
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>显存信息。</summary>
    Public Structure DmlMemoryInfo

        ''' <summary>驱动允许本进程使用的显存预算（字节）。</summary>
        Public Budget As ULong

        ''' <summary>当前已使用（字节）。</summary>
        Public CurrentUsage As ULong

        ''' <summary>适配器专用显存总量（字节）。</summary>
        Public DedicatedVideoMemory As ULong

        Public ReadOnly Property BudgetMB As Double
            Get
                Return Budget / 1024.0 / 1024.0
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"budget={BudgetMB:F1} MB, used={CurrentUsage / 1024.0 / 1024.0:F1} MB"
        End Function

    End Structure

    ''' <summary>
    ''' <see cref="DmlEngine"/> 的组装选项。
    ''' </summary>
    Public Class EngineOptions

        ''' <summary>使用的适配器序号（从 0 开始，按枚举顺序）。</summary>
        Public Property AdapterIndex As Integer = 0

        ''' <summary>直接指定适配器对象（优先于 <see cref="AdapterIndex"/>）。</summary>
        Public Property Adapter As DmlAdapter

        ''' <summary>
        ''' 额外的原生库搜索目录（例如 Microsoft.AI.DirectML 的分发目录）。
        ''' </summary>
        Public Property DirectMLDirectory As String

        ''' <summary>是否启用 D3D12/DirectML 调试层。</summary>
        Public Property EnableDebugLayer As Boolean = False

        ''' <summary>
        ''' 是否允许 DirectML 在内部使用 FP16 计算（更快，但数值误差更大）。
        ''' </summary>
        Public Property AllowHalfPrecision As Boolean = False

        ''' <summary>要求的 DirectML 最低特性级别。</summary>
        Public Property MinimumFeatureLevel As DML_FEATURE_LEVEL = DML_FEATURE_LEVEL.LEVEL_1_0

        ''' <summary><see cref="DmlEngine.TryCreate"/> 失败时的原因。</summary>
        Public Property ErrorMessage As String

        ''' <summary>枚举得到的全部候选适配器（诊断用）。</summary>
        Public Property Candidates As List(Of DmlAdapter)

    End Class

End Namespace
