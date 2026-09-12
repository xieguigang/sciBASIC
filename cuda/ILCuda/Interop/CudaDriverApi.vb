#Region "Microsoft.VisualBasic::ee7809e07b55a491181aff541af9578f, cuda\ILCuda\Interop\CudaDriverApi.vb"

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

    '   Total Lines: 298
    '    Code Lines: 188 (63.09%)
    ' Comment Lines: 57 (19.13%)
    '    - Xml Docs: 12.28%
    ' 
    '   Blank Lines: 53 (17.79%)
    '     File Size: 13.87 KB


    ' Module CudaDriverApi
    ' 
    '     Function: Check, cuCtxGetCurrent, cuCtxSetCurrent, cuCtxSynchronize, cuDeviceGet
    '               cuDeviceGetAttribute, cuDeviceGetCount, cuDeviceGetName, cuDevicePrimaryCtxRelease, cuDevicePrimaryCtxRetain
    '               cuDeviceTotalMem_v2, cuDriverGetVersion, cuEventCreate, cuEventDestroy_v2, cuEventElapsedTime
    '               cuEventQuery, cuEventRecord, cuEventSynchronize, cuGetErrorName, cuGetErrorString
    '               cuInit, cuLaunchKernel, cuMemAlloc_v2, cuMemAllocHost_v2, cuMemcpyDtoD_v2
    '               cuMemcpyDtoDAsync_v2, cuMemcpyDtoH_v2, cuMemcpyDtoHAsync_v2, cuMemcpyHtoD_v2, cuMemcpyHtoDAsync_v2
    '               cuMemFree_v2, cuMemFreeHost, cuMemGetInfo_v2, cuMemHostGetDevicePointer_v2, cuMemsetD8_v2
    '               cuMemsetD8Async, cuModuleGetFunction, cuModuleLoadData, cuModuleUnload, cuOccupancyMaxPotentialBlockSize
    '               cuStreamCreate, cuStreamDestroy_v2, cuStreamQuery, cuStreamSynchronize, cuStreamWaitEvent
    '               ErrorDescription, ErrorName, ErrorText
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' CUDA Driver API 的 P/Invoke 声明（nvcuda.dll / libcuda.so）
'
' 说明：
'   1. 这里只使用 BCL 的 System.Runtime.InteropServices.DllImport，
'      没有任何 NuGet 依赖；
'   2. 全部使用 _v2 版本的内存/上下文入口，避免 32/64 位指针歧义；
'   3. 进程必须是 x64（CUdeviceptr 为 64 位无符号整数）。
' ------------------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text

Public Module CudaDriverApi

    ''' <summary>Windows 上为 nvcuda.dll，Linux 上为 libcuda.so</summary>
    Friend Const CudaLib As String = "nvcuda"

    ' ------------------------------------------------------------------
    ' 初始化与版本信息
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuInit")>
    Friend Function cuInit(flags As UInteger) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuDriverGetVersion")>
    Friend Function cuDriverGetVersion(ByRef driverVersion As Integer) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuGetErrorName")>
    Friend Function cuGetErrorName(status As CUresult, ByRef namePtr As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuGetErrorString")>
    Friend Function cuGetErrorString(status As CUresult, ByRef strPtr As IntPtr) As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' 设备枚举与属性
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuDeviceGetCount")>
    Friend Function cuDeviceGetCount(ByRef count As Integer) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuDeviceGet")>
    Friend Function cuDeviceGet(ByRef device As Integer, ordinal As Integer) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuDeviceGetName", CharSet:=CharSet.Ansi)>
    Friend Function cuDeviceGetName(name As StringBuilder, length As Integer, device As Integer) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuDeviceTotalMem_v2")>
    Friend Function cuDeviceTotalMem_v2(ByRef bytes As ULong, device As Integer) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuDeviceGetAttribute")>
    Friend Function cuDeviceGetAttribute(ByRef value As Integer, attrib As CUdevice_attribute, device As Integer) As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' 上下文管理（使用 primary context，替代已废弃的 cuCtxCreate）
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuDevicePrimaryCtxRetain")>
    Friend Function cuDevicePrimaryCtxRetain(ByRef ctx As IntPtr, device As Integer) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuDevicePrimaryCtxRelease")>
    Friend Function cuDevicePrimaryCtxRelease(device As Integer) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuCtxSetCurrent")>
    Friend Function cuCtxSetCurrent(ctx As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuCtxGetCurrent")>
    Friend Function cuCtxGetCurrent(ByRef ctx As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuCtxSynchronize")>
    Friend Function cuCtxSynchronize() As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' 模块（PTX / cubin）加载与内核函数查询
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuModuleLoadData")>
    Friend Function cuModuleLoadData(ByRef hmodule As IntPtr, image As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuModuleUnload")>
    Friend Function cuModuleUnload(hmodule As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuModuleGetFunction", CharSet:=CharSet.Ansi)>
    Friend Function cuModuleGetFunction(ByRef hfunc As IntPtr, hmodule As IntPtr, name As String) As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' 显存分配 / 释放 / 拷贝 / 清零
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuMemAlloc_v2")>
    Friend Function cuMemAlloc_v2(ByRef dptr As ULong, bytesize As ULong) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuMemFree_v2")>
    Friend Function cuMemFree_v2(dptr As ULong) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuMemcpyHtoD_v2")>
    Friend Function cuMemcpyHtoD_v2(dstDevice As ULong, srcHost As IntPtr, byteCount As ULong) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuMemcpyDtoH_v2")>
    Friend Function cuMemcpyDtoH_v2(dstHost As IntPtr, srcDevice As ULong, byteCount As ULong) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuMemsetD8_v2")>
    Friend Function cuMemsetD8_v2(dstDevice As ULong, value As Byte, count As ULong) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuMemcpyDtoD_v2")>
    Friend Function cuMemcpyDtoD_v2(dstDevice As ULong, srcDevice As ULong, byteCount As ULong) As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' 内核启动
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuLaunchKernel")>
    Friend Function cuLaunchKernel(hfunc As IntPtr,
                                   gridDimX As UInteger,
                                   gridDimY As UInteger,
                                   gridDimZ As UInteger,
                                   blockDimX As UInteger,
                                   blockDimY As UInteger,
                                   blockDimZ As UInteger,
                                   sharedMemBytes As UInteger,
                                   hStream As IntPtr,
                                   kernelParams As IntPtr(),
                                   extra As IntPtr()) As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' 流（Stream）：让拷贝与内核执行可以重叠、并按顺序排队
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuStreamCreate")>
    Friend Function cuStreamCreate(ByRef hStream As IntPtr, flags As UInteger) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuStreamDestroy_v2")>
    Friend Function cuStreamDestroy_v2(hStream As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuStreamSynchronize")>
    Friend Function cuStreamSynchronize(hStream As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuStreamQuery")>
    Friend Function cuStreamQuery(hStream As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuStreamWaitEvent")>
    Friend Function cuStreamWaitEvent(hStream As IntPtr, hEvent As IntPtr, flags As UInteger) As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' 异步拷贝（H<->D、D->D），需要配合流使用
    ' 源/目的主机内存必须是页锁定（pinned）内存
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuMemcpyHtoDAsync_v2")>
    Friend Function cuMemcpyHtoDAsync_v2(dstDevice As ULong, srcHost As IntPtr, byteCount As ULong, hStream As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuMemcpyDtoHAsync_v2")>
    Friend Function cuMemcpyDtoHAsync_v2(dstHost As IntPtr, srcDevice As ULong, byteCount As ULong, hStream As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuMemcpyDtoDAsync_v2")>
    Friend Function cuMemcpyDtoDAsync_v2(dstDevice As ULong, srcDevice As ULong, byteCount As ULong, hStream As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuMemsetD8Async")>
    Friend Function cuMemsetD8Async(dstDevice As ULong, value As Byte, count As ULong, hStream As IntPtr) As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' 页锁定主机内存（pinned host memory）
    ' 异步拷贝的主机端必须是这类内存，否则驱动会走慢速的分段拷贝路径
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuMemAllocHost_v2")>
    Friend Function cuMemAllocHost_v2(ByRef pp As IntPtr, bytesize As ULong) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuMemFreeHost")>
    Friend Function cuMemFreeHost(p As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuMemHostGetDevicePointer_v2")>
    Friend Function cuMemHostGetDevicePointer_v2(ByRef pdptr As ULong, p As IntPtr, flags As UInteger) As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' 显存信息
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuMemGetInfo_v2")>
    Friend Function cuMemGetInfo_v2(ByRef free As ULong, ByRef total As ULong) As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' 占用率计算（用于推导最佳 block 尺寸；老驱动可能没有该入口）
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuOccupancyMaxPotentialBlockSize")>
    Friend Function cuOccupancyMaxPotentialBlockSize(ByRef minGridSize As Integer, ByRef blockSize As Integer,
                                                     hfunc As IntPtr,
                                                     blockSizeToDynamicSMemSize As IntPtr,
                                                     dynamicSMemSize As UInteger,
                                                     blockSizeLimit As Integer) As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' CUDA event（用于 GPU 侧精确计时）
    ' ------------------------------------------------------------------
    <DllImport(CudaLib, EntryPoint:="cuEventCreate")>
    Friend Function cuEventCreate(ByRef hEvent As IntPtr, flags As UInteger) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuEventRecord")>
    Friend Function cuEventRecord(hEvent As IntPtr, hStream As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuEventSynchronize")>
    Friend Function cuEventSynchronize(hEvent As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuEventQuery")>
    Friend Function cuEventQuery(hEvent As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuEventElapsedTime")>
    Friend Function cuEventElapsedTime(ByRef milliseconds As Single, hStart As IntPtr, hEnd As IntPtr) As CUresult
    End Function

    <DllImport(CudaLib, EntryPoint:="cuEventDestroy_v2")>
    Friend Function cuEventDestroy_v2(hEvent As IntPtr) As CUresult
    End Function

    ' ------------------------------------------------------------------
    ' 辅助：错误检查与错误信息
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' 检查 CUDA 调用返回值，非成功时抛出 <see cref="CudaException"/>
    ''' </summary>
    Public Function Check(status As CUresult, <CallerMemberName> Optional apiName As String = Nothing) As CUresult
        If status = CUresult.CUDA_SUCCESS Then
            Return status
        End If
        Throw New CudaException(status, apiName)
    End Function

    ''' <summary>尝试把错误码转换为驱动给出的名称（例如 CUDA_ERROR_INVALID_PTX）</summary>
    Public Function ErrorName(status As CUresult) As String
        Try
            Dim ptr As IntPtr = IntPtr.Zero
            If cuGetErrorName(status, ptr) = CUresult.CUDA_SUCCESS AndAlso ptr <> IntPtr.Zero Then
                Dim s = Marshal.PtrToStringAnsi(ptr)
                If Not String.IsNullOrEmpty(s) Then Return s
            End If
        Catch
        End Try
        Return status.ToString()
    End Function

    ''' <summary>尝试把错误码转换为驱动给出的可读描述</summary>
    Public Function ErrorDescription(status As CUresult) As String
        Try
            Dim ptr As IntPtr = IntPtr.Zero
            If cuGetErrorString(status, ptr) = CUresult.CUDA_SUCCESS AndAlso ptr <> IntPtr.Zero Then
                Dim s = Marshal.PtrToStringAnsi(ptr)
                If Not String.IsNullOrEmpty(s) Then Return s
            End If
        Catch
        End Try
        Return String.Empty
    End Function

    ''' <summary>把错误码整理为 "名称 (编号): 描述" 形式</summary>
    Public Function ErrorText(status As CUresult) As String
        Dim name As String = ErrorName(status)
        Dim desc As String = ErrorDescription(status)

        If String.IsNullOrEmpty(desc) Then
            Return $"{name} ({CInt(status)})"
        End If
        Return $"{name} ({CInt(status)}): {desc}"
    End Function
End Module
