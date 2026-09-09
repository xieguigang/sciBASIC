#Region "Microsoft.VisualBasic::90797993600ddf94aa6c9a42a8f62ab1, cuda\ILCuda\Interop\CUresult\CUdevice_attribute.vb"

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
    '    Code Lines: 21 (63.64%)
    ' Comment Lines: 8 (24.24%)
    '    - Xml Docs: 37.50%
    ' 
    '   Blank Lines: 4 (12.12%)
    '     File Size: 1.36 KB


    ' Enum CUdevice_attribute
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' CUDA / NVRTC 状态码与异常类型
'
' 本文件不引用任何第三方 NuGet 程序包，全部依赖均来自 .NET 基类库。
' ------------------------------------------------------------------------


''' <summary>
''' 设备属性查询键（CUdevice_attribute）
''' </summary>
Public Enum CUdevice_attribute
    CU_DEVICE_ATTRIBUTE_MAX_THREADS_PER_BLOCK = 1
    CU_DEVICE_ATTRIBUTE_MAX_BLOCK_DIM_X = 2
    CU_DEVICE_ATTRIBUTE_MAX_BLOCK_DIM_Y = 3
    CU_DEVICE_ATTRIBUTE_MAX_BLOCK_DIM_Z = 4
    CU_DEVICE_ATTRIBUTE_MAX_GRID_DIM_X = 5
    CU_DEVICE_ATTRIBUTE_MAX_GRID_DIM_Y = 6
    CU_DEVICE_ATTRIBUTE_MAX_GRID_DIM_Z = 7
    CU_DEVICE_ATTRIBUTE_MAX_SHARED_MEMORY_PER_BLOCK = 8
    CU_DEVICE_ATTRIBUTE_WARP_SIZE = 10
    CU_DEVICE_ATTRIBUTE_MAX_REGISTERS_PER_BLOCK = 12
    CU_DEVICE_ATTRIBUTE_CLOCK_RATE = 13
    CU_DEVICE_ATTRIBUTE_MULTIPROCESSOR_COUNT = 16
    CU_DEVICE_ATTRIBUTE_MEMORY_CLOCK_RATE = 36
    CU_DEVICE_ATTRIBUTE_GLOBAL_MEMORY_BUS_WIDTH = 37
    CU_DEVICE_ATTRIBUTE_L2_CACHE_SIZE = 38
    CU_DEVICE_ATTRIBUTE_MAX_THREADS_PER_MULTIPROCESSOR = 39
    CU_DEVICE_ATTRIBUTE_COMPUTE_CAPABILITY_MAJOR = 75
    CU_DEVICE_ATTRIBUTE_COMPUTE_CAPABILITY_MINOR = 76
    CU_DEVICE_ATTRIBUTE_MAX_SHARED_MEMORY_PER_BLOCK_OPTIN = 97
End Enum



