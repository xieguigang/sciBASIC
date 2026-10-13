#Region "Microsoft.VisualBasic::621ed432ac16f104f7172479f7b065f3, cuda\ILCuda\Interop\CUresult\PdhException.vb"

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

    '   Total Lines: 45
    '    Code Lines: 35 (77.78%)
    ' Comment Lines: 5 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (11.11%)
    '     File Size: 2.44 KB


    ' Class PdhException
    ' 
    '     Properties: ApiName, Status
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ComposeMessage, ToString
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Windows 性能计数器（PDH）调用失败时抛出的异常（风格对齐 <see cref="NvmlException"/>）
''' </summary>
Public Class PdhException : Inherits Exception

    ''' <summary>PDH 返回的状态码（PDH_STATUS）</summary>
    Public ReadOnly Property Status As UInteger
    ''' <summary>出错的 API 名称</summary>
    Public ReadOnly Property ApiName As String

    Public Sub New(status As UInteger, Optional apiName As String = Nothing)
        MyBase.New(PdhException.ComposeMessage(status, apiName))
        Me.Status = status
        Me.ApiName = apiName
    End Sub

    Private Shared Function ComposeMessage(status As UInteger, apiName As String) As String
        Dim api As String = If(String.IsNullOrEmpty(apiName), "<unknown>", apiName)

        Select Case status
            Case PdhApi.PDH_CSTATUS_NO_OBJECT
                Return $"{api} 失败: PDH_CSTATUS_NO_OBJECT (0x{status:X8}) —— 本机不存在该性能计数器集（需 Windows 10 1709+ / WDDM 2.0+）。"
            Case PdhApi.PDH_CSTATUS_NO_COUNTER
                Return $"{api} 失败: PDH_CSTATUS_NO_COUNTER (0x{status:X8}) —— 计数器集中没有该计数器。"
            Case PdhApi.PDH_CSTATUS_NO_INSTANCE
                Return $"{api} 失败: PDH_CSTATUS_NO_INSTANCE (0x{status:X8}) —— 当前采样窗口内没有该计数器的实例。"
            Case PdhApi.PDH_INVALID_HANDLE
                Return $"{api} 失败: PDH_INVALID_HANDLE (0x{status:X8}) —— 无效的查询/计数器句柄。"
            Case PdhApi.PDH_INVALID_ARGUMENT
                Return $"{api} 失败: PDH_INVALID_ARGUMENT (0x{status:X8}) —— 参数无效。"
            Case PdhApi.PDH_FUNCTION_NOT_FOUND
                Return $"{api} 失败: PDH_FUNCTION_NOT_FOUND (0x{status:X8}) —— 找不到该计数器函数。"
            Case PdhApi.PDH_NO_DATA
                Return $"{api} 失败: PDH_NO_DATA (0x{status:X8}) —— 尚无可用数据（速率型计数器需两次采集）。"
            Case PdhApi.PDH_MORE_DATA
                Return $"{api} 失败: PDH_MORE_DATA (0x{status:X8}) —— 缓冲区不足，需要更大的缓冲区。"
            Case Else
                Return $"{api} 失败: 0x{status:X8}"
        End Select
    End Function

    Public Overrides Function ToString() As String
        Return Message
    End Function
End Class
