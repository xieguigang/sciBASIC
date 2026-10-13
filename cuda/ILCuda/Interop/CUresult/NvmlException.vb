#Region "Microsoft.VisualBasic::4eaf86e6aea05228c53264896d3cacfb, cuda\ILCuda\Interop\CUresult\NvmlException.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 42
    '    Code Lines: 32 (76.19%)
    ' Comment Lines: 5 (11.90%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (11.90%)
    '     File Size: 2.14 KB


    ' Class NvmlException
    ' 
    '     Properties: ApiName, Status
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ComposeMessage, ToString
    ' 
    ' /********************************************************************************/

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
