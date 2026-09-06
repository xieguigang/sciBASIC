''' <summary>
''' CUDA 调用失败时抛出的异常
''' </summary>
Public Class CudaException : Inherits Exception

    ''' <summary>驱动返回的错误码</summary>
    Public ReadOnly Property Status As CUresult
    ''' <summary>出错的 API 名称</summary>
    Public ReadOnly Property ApiName As String

    Public Sub New(status As CUresult, Optional apiName As String = Nothing)
        MyBase.New(CudaException.ComposeMessage(status, apiName))
        Me.Status = status
        Me.ApiName = apiName
    End Sub

    Private Shared Function ComposeMessage(status As CUresult, apiName As String) As String
        Dim api As String = If(String.IsNullOrEmpty(apiName), "<unknown>", apiName)
        Dim code As Integer = CInt(status)

        If status = CUresult.CUDA_ERROR_INVALID_PTX OrElse
           status = CUresult.CUDA_ERROR_UNSUPPORTED_PTX_VERSION OrElse
           status = CUresult.CUDA_ERROR_NO_BINARY_FOR_GPU Then
            Return $"{api} 失败: {status} ({code}) —— PTX/镜像版本与当前显卡驱动不兼容，请安装与驱动匹配的 CUDA 工具包或升级 NVIDIA 驱动。"
        End If

        Return $"{api} 失败: {status} ({code})"
    End Function

    Public Overrides Function ToString() As String
        Return Message
    End Function
End Class