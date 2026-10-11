#Region "Microsoft.VisualBasic::nvml-exception, cuda\ILCuda\Interop\CUresult\NvmlException.vb"

    ' Author:
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) sciBASIC.NET Foundation
    '
    ' GNU GENERAL PUBLIC LICENSE (GPL3)

#End Region

''' <summary>
''' NVML 调用失败时抛出的异常（风格对齐 <see cref="CudaException"/>）
''' </summary>
Public Class NvmlException : Inherits Exception

    ''' <summary>NVML 返回的错误码</summary>
    Public ReadOnly Property Status As nvmlReturnT
    ''' <summary>出错的 API 名称</summary>
    Public ReadOnly Property ApiName As String

    Public Sub New(status As nvmlReturnT, Optional apiName As String = Nothing)
        MyBase.New(NvmlException.ComposeMessage(status, apiName))
        Me.Status = status
        Me.ApiName = apiName
    End Sub

    Private Shared Function ComposeMessage(status As nvmlReturnT, apiName As String) As String
        Dim api As String = If(String.IsNullOrEmpty(apiName), "<unknown>", apiName)
        Dim code As Integer = CInt(status)

        Select Case status
            Case nvmlReturnT.NVML_ERROR_UNINITIALIZED
                Return $"{api} 失败: {status} ({code}) —— NVML 尚未初始化，请先调用 nvmlInit_v2。"
            Case nvmlReturnT.NVML_ERROR_NOT_SUPPORTED
                Return $"{api} 失败: {status} ({code}) —— 当前设备或驱动不支持该查询。"
            Case nvmlReturnT.NVML_ERROR_DRIVER_NOT_LOADED
                Return $"{api} 失败: {status} ({code}) —— NVIDIA 驱动未加载，请确认显卡驱动已正确安装。"
            Case nvmlReturnT.NVML_ERROR_GPU_IS_LOST
                Return $"{api} 失败: {status} ({code}) —— GPU 失去响应（可能需重置）。"
            Case nvmlReturnT.NVML_ERROR_LIBRARY_NOT_FOUND
                Return $"{api} 失败: {status} ({code}) —— 找不到 nvml.dll（NVIDIA 管理库），请检查驱动安装。"
            Case nvmlReturnT.NVML_ERROR_FUNCTION_NOT_FOUND
                Return $"{api} 失败: {status} ({code}) —— nvml.dll 中缺少该入口，驱动版本可能过旧。"
            Case Else
                Return $"{api} 失败: {status} ({code})"
        End Select
    End Function

    Public Overrides Function ToString() As String
        Return Message
    End Function
End Class
