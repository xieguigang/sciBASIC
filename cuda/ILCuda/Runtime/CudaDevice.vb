#Region "Microsoft.VisualBasic::58f9bae1a5b78ee4a7dcb91606a7b9f6, cuda\ILCuda\Runtime\CudaDevice.vb"

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

    '   Total Lines: 138
    '    Code Lines: 101 (73.19%)
    ' Comment Lines: 14 (10.14%)
    '    - Xml Docs: 78.57%
    ' 
    '   Blank Lines: 23 (16.67%)
    '     File Size: 6.40 KB


    '     Class CudaDevice
    ' 
    '         Properties: Arch, CapabilityMajor, CapabilityMinor, ClockRateKhz, ComputeCapability
    '                     GlobalMemoryBusWidth, Handle, L2CacheSize, MaxSharedMemoryPerBlock, MaxThreadsPerBlock
    '                     MaxThreadsPerMultiprocessor, MemoryClockRateKhz, MultiprocessorCount, Name, Ordinal
    '                     SmArch, TotalMemoryBytes, TotalMemoryMB, WarpSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: DriverVersion, DriverVersionKey, DriverVersionText, Enumerate, Query
    '                   ToString
    ' 
    '         Sub: EnsureInitialized
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' NVIDIA GPU 设备枚举与属性
' ------------------------------------------------------------------------

Imports System.Text

Namespace Runtime

    Public NotInheritable Class CudaDevice

        Private Sub New(ordinal As Integer, handle As Integer)
            Me.Ordinal = ordinal
            Me.Handle = handle

            Dim buffer As New StringBuilder(256)
            CudaDriverApi.Check(CudaDriverApi.cuDeviceGetName(buffer, buffer.Capacity, handle), "cuDeviceGetName")
            Me.Name = buffer.ToString().TrimEnd(ChrW(0)).Trim()

            Dim bytes As ULong = 0
            CudaDriverApi.Check(CudaDriverApi.cuDeviceTotalMem_v2(bytes, handle), "cuDeviceTotalMem_v2")
            Me.TotalMemoryBytes = bytes

            Me.CapabilityMajor = Query(CUdevice_attribute.CU_DEVICE_ATTRIBUTE_COMPUTE_CAPABILITY_MAJOR)
            Me.CapabilityMinor = Query(CUdevice_attribute.CU_DEVICE_ATTRIBUTE_COMPUTE_CAPABILITY_MINOR)
            Me.MultiprocessorCount = Query(CUdevice_attribute.CU_DEVICE_ATTRIBUTE_MULTIPROCESSOR_COUNT)
            Me.MaxThreadsPerBlock = Query(CUdevice_attribute.CU_DEVICE_ATTRIBUTE_MAX_THREADS_PER_BLOCK)
            Me.MaxThreadsPerMultiprocessor = Query(CUdevice_attribute.CU_DEVICE_ATTRIBUTE_MAX_THREADS_PER_MULTIPROCESSOR)
            Me.MaxSharedMemoryPerBlock = Query(CUdevice_attribute.CU_DEVICE_ATTRIBUTE_MAX_SHARED_MEMORY_PER_BLOCK)
            Me.WarpSize = Query(CUdevice_attribute.CU_DEVICE_ATTRIBUTE_WARP_SIZE)
            Me.ClockRateKhz = Query(CUdevice_attribute.CU_DEVICE_ATTRIBUTE_CLOCK_RATE)
            Me.MemoryClockRateKhz = Query(CUdevice_attribute.CU_DEVICE_ATTRIBUTE_MEMORY_CLOCK_RATE)
            Me.GlobalMemoryBusWidth = Query(CUdevice_attribute.CU_DEVICE_ATTRIBUTE_GLOBAL_MEMORY_BUS_WIDTH)
            Me.L2CacheSize = Query(CUdevice_attribute.CU_DEVICE_ATTRIBUTE_L2_CACHE_SIZE)
        End Sub

        Private Function Query(attrib As CUdevice_attribute) As Integer
            Dim value As Integer = 0
            CudaDriverApi.Check(CudaDriverApi.cuDeviceGetAttribute(value, attrib, Me.Handle), "cuDeviceGetAttribute")
            Return value
        End Function

        ''' <summary>设备序号（从 0 开始）</summary>
        Public ReadOnly Property Ordinal As Integer
        ''' <summary>CUdevice 句柄（本质是 int）</summary>
        Public ReadOnly Property Handle As Integer
        ''' <summary>设备名称，例如 NVIDIA RTX A4000</summary>
        Public ReadOnly Property Name As String
        Public ReadOnly Property TotalMemoryBytes As ULong
        Public ReadOnly Property CapabilityMajor As Integer
        Public ReadOnly Property CapabilityMinor As Integer
        Public ReadOnly Property MultiprocessorCount As Integer
        Public ReadOnly Property MaxThreadsPerBlock As Integer
        Public ReadOnly Property MaxThreadsPerMultiprocessor As Integer
        Public ReadOnly Property MaxSharedMemoryPerBlock As Integer
        Public ReadOnly Property WarpSize As Integer
        Public ReadOnly Property ClockRateKhz As Integer
        Public ReadOnly Property MemoryClockRateKhz As Integer
        Public ReadOnly Property GlobalMemoryBusWidth As Integer
        Public ReadOnly Property L2CacheSize As Integer

        ''' <summary>计算能力，例如 8.6</summary>
        Public ReadOnly Property ComputeCapability As String
            Get
                Return $"{CapabilityMajor}.{CapabilityMinor}"
            End Get
        End Property

        ''' <summary>NVRTC 的 -arch 参数值，例如 compute_86</summary>
        Public ReadOnly Property Arch As String
            Get
                Return $"compute_{CapabilityMajor}{CapabilityMinor}"
            End Get
        End Property

        ''' <summary>SASS 目标架构，例如 sm_86</summary>
        Public ReadOnly Property SmArch As String
            Get
                Return $"sm_{CapabilityMajor}{CapabilityMinor}"
            End Get
        End Property

        Public ReadOnly Property TotalMemoryMB As Double
            Get
                Return TotalMemoryBytes / 1024.0 / 1024.0
            End Get
        End Property

        Private Shared _initialized As Boolean = False

        ''' <summary>整个进程只需要调用一次 cuInit</summary>
        Public Shared Sub EnsureInitialized()
            If _initialized Then Return
            CudaDriverApi.Check(CudaDriverApi.cuInit(0), "cuInit")
            _initialized = True
        End Sub

        ''' <summary>枚举当前系统中所有可用的 CUDA 设备</summary>
        Public Shared Function Enumerate() As IReadOnlyList(Of CudaDevice)
            EnsureInitialized()

            Dim count As Integer = 0
            CudaDriverApi.Check(CudaDriverApi.cuDeviceGetCount(count), "cuDeviceGetCount")

            Dim list As New List(Of CudaDevice)()
            For i As Integer = 0 To count - 1
                Dim handle As Integer = 0
                CudaDriverApi.Check(CudaDriverApi.cuDeviceGet(handle, i), "cuDeviceGet")
                list.Add(New CudaDevice(i, handle))
            Next

            Return list
        End Function

        ''' <summary>驱动版本号，例如 12080 表示 CUDA 12.8</summary>
        Public Shared Function DriverVersion() As Integer
            EnsureInitialized()
            Dim version As Integer = 0
            CudaDriverApi.Check(CudaDriverApi.cuDriverGetVersion(version), "cuDriverGetVersion")
            Return version
        End Function

        ''' <summary>驱动支持的 CUDA 版本整数键，例如 12.8 -> 128</summary>
        Public Shared Function DriverVersionKey() As Integer
            Dim v = DriverVersion()
            Return (v \ 1000) * 10 + (v Mod 1000) \ 10
        End Function

        ''' <summary>驱动支持的 CUDA 版本文本，例如 12.8</summary>
        Public Shared Function DriverVersionText() As String
            Dim v = DriverVersion()
            Return $"{v \ 1000}.{(v Mod 1000) \ 10}"
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Ordinal}] {Name} (CC {ComputeCapability}, {TotalMemoryMB:F0} MB)"
        End Function
    End Class
End Namespace

