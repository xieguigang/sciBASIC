#Region "Microsoft.VisualBasic::a731074b162cfbf607855d2c71fd2fa9, cuda\ILCuda\Interop\CUresult\CudaException.vb"

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

    '   Total Lines: 33
    '    Code Lines: 22 (66.67%)
    ' Comment Lines: 5 (15.15%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (18.18%)
    '     File Size: 1.33 KB


    ' Class CudaException
    ' 
    '     Properties: ApiName, Status
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ComposeMessage, ToString
    ' 
    ' /********************************************************************************/

#End Region

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
