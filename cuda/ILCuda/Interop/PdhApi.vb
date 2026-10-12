#Region "Microsoft.VisualBasic::pdh-api, cuda\ILCuda\Interop\PdhApi.vb"

    ' Author:
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) sciBASIC.NET Foundation
    '
    ' GNU GENERAL PUBLIC LICENSE (GPL3)

#End Region

' ------------------------------------------------------------------------
' Windows 性能计数器（PDH, pdh.dll）的 P/Invoke 声明
'
' 说明：
'   1. 只用 BCL 的 System.Runtime.InteropServices.DllImport，零 NuGet 依赖，
'      与 CudaDriverApi.vb / NvmlApi.vb 风格一致；
'   2. 使用 PdhAddEnglishCounterW 以"英文计数器名"添加计数器，
'      规避非英文 Windows 上计数器名被本地化的问题；
'   3. 仅适用于 Windows；非 Windows 上运行期才会因找不到 pdh.dll 抛异常，
'      由上层 try/catch 降级；
'   4. 相关计数器：
'        \GPU Engine(*)\Utilization Percentage   （每进程各引擎利用率 %）
'        \GPU Process Memory(*)\Dedicated Usage  （每进程专用显存，字节）
' ------------------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Public Module PdhApi

    ''' <summary>pdh.dll（Windows 性能数据助手）</summary>
    Friend Const PdhLib As String = "pdh"

    ' ------------------------------------------------------------------
    ' PDH_STATUS（错误码）
    ' ------------------------------------------------------------------
    Public Const PDH_SUCCESS As UInteger = 0UI
    Public Const PDH_MORE_DATA As UInteger = &H800007D2UI
    Public Const PDH_CSTATUS_VALID_DATA As UInteger = &H0UI
    Public Const PDH_CSTATUS_NEW_DATA As UInteger = &H1UI
    Public Const PDH_CSTATUS_NO_OBJECT As UInteger = &HC0000BB8UI
    Public Const PDH_CSTATUS_NO_COUNTER As UInteger = &HC0000BB9UI
    Public Const PDH_CSTATUS_NO_INSTANCE As UInteger = &H800007D1UI
    Public Const PDH_INVALID_HANDLE As UInteger = &HC0000BBCUI
    Public Const PDH_INVALID_ARGUMENT As UInteger = &HC0000BBDUI
    Public Const PDH_FUNCTION_NOT_FOUND As UInteger = &HC0000BBEUI
    Public Const PDH_NO_DATA As UInteger = &H800007D5UI

    ' ------------------------------------------------------------------
    ' 取值格式标志（PdhGetFormattedCounterArrayW 的 dwFormat）
    ' ------------------------------------------------------------------
    ''' <summary>把计数值格式化为 Double（PDH_FMT_DOUBLE）。</summary>
    Public Const PDH_FMT_DOUBLE As UInteger = &H200UI
    ''' <summary>不使用计数器自带的缩放因子（PDH_FMT_NOSCALE）。</summary>
    Public Const PDH_FMT_NOSCALE As UInteger = &H1000UI

    ' ------------------------------------------------------------------
    ' 结构体
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' PDH_FMT_COUNTERVALUE：按 <see cref="CStatus"/> 决定该联合体中哪个字段有效。
    ''' 我们统一用 PDH_FMT_DOUBLE 取值，故只保留 Double 重载。
    ''' </summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure PDH_FMT_COUNTERVALUE
        Public CStatus As UInteger
        Public DoubleValue As Double
    End Structure

    ''' <summary>PDH_FMT_COUNTERVALUE_ITEM：一个实例名 + 其计数格式化值。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure PDH_FMT_COUNTERVALUE_ITEM
        ''' <summary>实例名（指向多字符串缓冲中的一项，宽字符）</summary>
        Public SzName As IntPtr
        Public FmtValue As PDH_FMT_COUNTERVALUE
    End Structure

    ' ------------------------------------------------------------------
    ' API 声明
    ' ------------------------------------------------------------------
    <DllImport(PdhLib, EntryPoint:="PdhOpenQueryW", CharSet:=CharSet.Unicode)>
    Friend Function PdhOpenQueryW(dataSource As String, userData As IntPtr, ByRef query As IntPtr) As UInteger
    End Function

    <DllImport(PdhLib, EntryPoint:="PdhCloseQuery")>
    Friend Function PdhCloseQuery(query As IntPtr) As UInteger
    End Function

    <DllImport(PdhLib, EntryPoint:="PdhAddEnglishCounterW", CharSet:=CharSet.Unicode)>
    Friend Function PdhAddEnglishCounterW(query As IntPtr, fullCounterPath As String, userData As IntPtr, ByRef counter As IntPtr) As UInteger
    End Function

    <DllImport(PdhLib, EntryPoint:="PdhRemoveCounter")>
    Friend Function PdhRemoveCounter(counter As IntPtr) As UInteger
    End Function

    <DllImport(PdhLib, EntryPoint:="PdhCollectQueryData")>
    Friend Function PdhCollectQueryData(query As IntPtr) As UInteger
    End Function

    <DllImport(PdhLib, EntryPoint:="PdhGetFormattedCounterArrayW", CharSet:=CharSet.Unicode)>
    Friend Function PdhGetFormattedCounterArrayW(counter As IntPtr, format As UInteger, ByRef bufferSize As UInteger, ByRef itemCount As UInteger, buffer As IntPtr) As UInteger
    End Function

    ' ------------------------------------------------------------------
    ' 辅助：错误检查
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' 检查 PDH 调用返回状态，非成功时抛出 <see cref="PdhException"/>
    ''' </summary>
    Public Function Check(status As UInteger, <CallerMemberName> Optional apiName As String = Nothing) As UInteger
        If status = PDH_SUCCESS Then
            Return status
        End If
        Throw New PdhException(status, apiName)
    End Function

    ''' <summary>PDH 错误码是否表示"需要更大的缓冲区"（PdhGetFormattedCounterArray 的常见返回）</summary>
    Public Function IsMoreData(status As UInteger) As Boolean
        Return status = PDH_MORE_DATA
    End Function

    ''' <summary>实例计数项的 CStatus 是否为有效数据（VALID_DATA 或 NEW_DATA）</summary>
    Public Function IsValidValue(cStatus As UInteger) As Boolean
        Return cStatus = PDH_CSTATUS_VALID_DATA OrElse cStatus = PDH_CSTATUS_NEW_DATA
    End Function
End Module
