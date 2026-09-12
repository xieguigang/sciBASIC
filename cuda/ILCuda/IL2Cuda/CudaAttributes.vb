#Region "Microsoft.VisualBasic::a2110c1f76ceda22e64f97466ad89269, cuda\ILCuda\IL2Cuda\CudaAttributes.vb"

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

    '   Total Lines: 82
    '    Code Lines: 40 (48.78%)
    ' Comment Lines: 29 (35.37%)
    '    - Xml Docs: 48.28%
    ' 
    '   Blank Lines: 13 (15.85%)
    '     File Size: 3.61 KB


    '     Enum CudaIndexMode
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum CudaIndexKind
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class CudaKernelAttribute
    ' 
    '         Properties: Mode, Name
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class CudaIndexAttribute
    ' 
    '         Properties: Kind
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class CudaInputAttribute
    ' 
    ' 
    ' 
    '     Class CudaOutputAttribute
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' IL -> CUDA 的内核映射标注
'
' 标注是可选的。没有标注时 IlCudaTranslator 会走"约定推断"：
'   * 存在名为 i / j（或 row / col）的整型参数  -> 视为线程索引参数
'       * 只有 i            -> Grid1D：i = blockIdx.x * blockDim.x + threadIdx.x
'       * 同时有 i 与 j     -> Grid2D：i 取行（blockIdx.y），j 取列（blockIdx.x）
'   * 否则若方法体里没有任何数组取元素（纯标量公式）
'                            -> 逐元素包裹：每个标量参数都变成"按 i 读取的数组"
'   * 都不满足              -> 只生成 __device__ 标量函数，不生成内核
'
' 想明确控制行为时用下面这几个标注：
'   <CudaKernel("名", 模式)>   标注在方法上
'   <CudaIndex(行/列/线性)>    标注在作为线程索引的形参上
' ---------------------------------------------------------------------------

Namespace IL2Cuda

    ''' <summary>内核的线程索引模式</summary>
    Public Enum CudaIndexMode
        ''' <summary>不生成 __global__ 内核，只生成 __device__ 标量函数</summary>
        None = 0
        ''' <summary>一维：i = blockIdx.x * blockDim.x + threadIdx.x</summary>
        Grid1D = 1
        ''' <summary>二维：行取 blockIdx.y，列取 blockIdx.x</summary>
        Grid2D = 2
    End Enum

    ''' <summary>被标注的形参承担哪一种线程索引</summary>
    Public Enum CudaIndexKind
        ''' <summary>一维线性下标</summary>
        Linear = 0
        ''' <summary>二维中的行号（对应 blockIdx.y）</summary>
        Row = 1
        ''' <summary>二维中的列号（对应 blockIdx.x）</summary>
        Col = 2
    End Enum

    ''' <summary>声明该方法参与 IL -> CUDA 转译</summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=False)>
    Public Class CudaKernelAttribute : Inherits Attribute

        ''' <summary>生成的内核函数名；留空时用 il_&lt;方法名&gt;_kernel</summary>
        Public Property Name As String
        ''' <summary>线程索引模式</summary>
        Public Property Mode As CudaIndexMode

        Public Sub New()
            Me.Mode = CudaIndexMode.Grid1D
        End Sub

        Public Sub New(name As String, Optional mode As CudaIndexMode = CudaIndexMode.Grid1D)
            Me.Name = name
            Me.Mode = mode
        End Sub
    End Class

    ''' <summary>声明该形参由线程索引提供（不占用内核参数位）</summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=False)>
    Public Class CudaIndexAttribute : Inherits Attribute

        Public Property Kind As CudaIndexKind

        Public Sub New()
            Me.Kind = CudaIndexKind.Linear
        End Sub

        Public Sub New(kind As CudaIndexKind)
            Me.Kind = kind
        End Sub
    End Class

    ''' <summary>声明该形参是设备端输入数组（语义标注，当前与默认行为一致）</summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=False)>
    Public Class CudaInputAttribute : Inherits Attribute
    End Class

    ''' <summary>声明该形参接收内核输出（语义标注，当前与默认行为一致）</summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=False)>
    Public Class CudaOutputAttribute : Inherits Attribute
    End Class
End Namespace
