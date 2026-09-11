#Region "Microsoft.VisualBasic::78fdf0c7be4ed179db1622881b17a3dc, cuda\ILCuda\Runtime\Diagnostics\DeviceReport.vb"

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

    '   Total Lines: 61
    '    Code Lines: 46 (75.41%)
    ' Comment Lines: 9 (14.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (9.84%)
    '     File Size: 3.02 KB


    '     Class DeviceReport
    ' 
    '         Properties: Arch, ClockRateMHz, ComputeCapability, GlobalMemoryBusWidth, L2CacheSizeKB
    '                     MaxSharedMemoryPerBlockKB, MaxThreadsPerBlock, MaxThreadsPerMultiprocessor, MemoryClockRateMHz, MultiprocessorCount
    '                     Name, Ordinal, SmArch, TotalMemoryMB, WarpSize
    ' 
    '         Function: (+2 Overloads) From, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>
    ''' 设备属性的纯数据快照：不持有任何 CUDA 句柄，可安全地跨层传递与格式化输出。
    ''' 框架只生成它，怎么显示由调用方决定。
    ''' </summary>
    Public Class DeviceReport
        Public Property Ordinal As Integer
        Public Property Name As String
        ''' <summary>计算能力文本，例如 8.6</summary>
        Public Property ComputeCapability As String
        ''' <summary>NVRTC 虚拟架构，例如 compute_86</summary>
        Public Property Arch As String
        ''' <summary>NVRTC 真实架构（产出 cubin），例如 sm_86</summary>
        Public Property SmArch As String
        Public Property TotalMemoryMB As Double
        Public Property MultiprocessorCount As Integer
        Public Property MaxThreadsPerBlock As Integer
        Public Property MaxThreadsPerMultiprocessor As Integer
        Public Property MaxSharedMemoryPerBlockKB As Double
        Public Property WarpSize As Integer
        Public Property ClockRateMHz As Double
        Public Property MemoryClockRateMHz As Double
        Public Property GlobalMemoryBusWidth As Integer
        Public Property L2CacheSizeKB As Double

        ''' <summary>从 <see cref="CudaDevice"/> 生成快照</summary>
        Public Shared Function From(device As CudaDevice) As DeviceReport
            If device Is Nothing Then Throw New ArgumentNullException(NameOf(device))

            Return New DeviceReport With {
                .Ordinal = device.Ordinal,
                .Name = device.Name,
                .ComputeCapability = device.ComputeCapability,
                .Arch = device.Arch,
                .SmArch = device.SmArch,
                .TotalMemoryMB = device.TotalMemoryMB,
                .MultiprocessorCount = device.MultiprocessorCount,
                .MaxThreadsPerBlock = device.MaxThreadsPerBlock,
                .MaxThreadsPerMultiprocessor = device.MaxThreadsPerMultiprocessor,
                .MaxSharedMemoryPerBlockKB = device.MaxSharedMemoryPerBlock / 1024.0,
                .WarpSize = device.WarpSize,
                .ClockRateMHz = device.ClockRateKhz / 1000.0,
                .MemoryClockRateMHz = device.MemoryClockRateKhz / 1000.0,
                .GlobalMemoryBusWidth = device.GlobalMemoryBusWidth,
                .L2CacheSizeKB = device.L2CacheSize / 1024.0
            }
        End Function

        ''' <summary>把多台设备的快照一次性生成</summary>
        Public Shared Function From(devices As IEnumerable(Of CudaDevice)) As IReadOnlyList(Of DeviceReport)
            If devices Is Nothing Then Return Array.Empty(Of DeviceReport)()

            Return devices.Select(Function(d) From(d)).ToList()
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Ordinal}] {Name} (CC {ComputeCapability}, {TotalMemoryMB:F0} MB)"
        End Function
    End Class
End Namespace
