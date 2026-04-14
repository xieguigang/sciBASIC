
''' <summary>
''' CVODE工具类
''' </summary>
Public NotInheritable Class CVODEUtils

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 创建默认选项
    ''' </summary>
    Public Shared Function DefaultOptions() As CVODEOptions
        Return New CVODEOptions()
    End Function

    ''' <summary>
    ''' 创建刚性问题的默认选项
    ''' </summary>
    Public Shared Function StiffOptions() As CVODEOptions
        Return New CVODEOptions() With {
            .RelativeTolerance = 0.000001,
            .AbsoluteTolerance = 0.00000001,
            .MaxOrder = 5,
            .MaxNewtonIterations = 10
        }
    End Function

    ''' <summary>
    ''' 创建非刚性问题的默认选项
    ''' </summary>
    Public Shared Function NonStiffOptions() As CVODEOptions
        Return New CVODEOptions() With {
            .RelativeTolerance = 0.000001,
            .AbsoluteTolerance = 0.00000001,
            .MaxOrder = 12
        }
    End Function

    ''' <summary>
    ''' 创建高精度选项
    ''' </summary>
    Public Shared Function HighPrecisionOptions() As CVODEOptions
        Return New CVODEOptions() With {
            .RelativeTolerance = 0.0000000001,
            .AbsoluteTolerance = 0.000000000001,
            .MaxOrder = 5,
            .MaxNewtonIterations = 10
        }
    End Function

End Class

