' ------------------------------------------------------------------------
' DirectML 基础描述符结构体
'
' 布局与 DirectML.h 严格一致（Sequential + 自然对齐）。
' DML_OPERATOR_DESC 的 Desc 字段是 void*，指向具体算子的描述符；
' 具体描述符里又常常带 const UINT* / const FLOAT* / const DML_TENSOR_DESC* 数组，
' 这些数组必须用非托管内存分配（见 DmlInterop 里的辅助方法），并且
' 必须比 CreateOperator 调用活得更久。
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>张量描述：类型 + 指向具体（缓冲区）描述符的指针。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_TENSOR_DESC
        Public Type As DML_TENSOR_TYPE
        Public Desc As IntPtr

        Public Sub New(type As DML_TENSOR_TYPE, desc As IntPtr)
            Me.Type = type
            Me.Desc = desc
        End Sub
    End Structure

    ''' <summary>缓冲区张量描述。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_BUFFER_TENSOR_DESC
        Public DataType As DML_TENSOR_DATA_TYPE
        Public Flags As DML_TENSOR_FLAGS
        Public DimensionCount As UInteger
        ''' <summary>指向 <c>UINT[DimensionCount]</c>。</summary>
        Public Sizes As IntPtr
        ''' <summary>指向 <c>UINT[DimensionCount]</c>，可为空表示紧密排列。</summary>
        Public Strides As IntPtr
        Public TotalTensorSizeInBytes As ULong
        Public GuaranteedBaseOffsetAlignment As UInteger
    End Structure

    ''' <summary>算子描述：算子类型 + 指向具体算子描述符的指针。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_OPERATOR_DESC
        Public Type As DML_OPERATOR_TYPE
        Public Desc As IntPtr

        Public Sub New(type As DML_OPERATOR_TYPE, desc As IntPtr)
            Me.Type = type
            Me.Desc = desc
        End Sub
    End Structure

    ''' <summary>缓冲区绑定。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_BUFFER_BINDING
        ''' <summary>ID3D12Resource*</summary>
        Public Buffer As IntPtr
        Public Offset As ULong
        Public SizeInBytes As ULong

        Public Sub New(buffer As IntPtr, offset As ULong, sizeInBytes As ULong)
            Me.Buffer = buffer
            Me.Offset = offset
            Me.SizeInBytes = sizeInBytes
        End Sub
    End Structure

    ''' <summary>缓冲区数组绑定。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_BUFFER_ARRAY_BINDING
        Public BindingCount As UInteger
        ''' <summary>指向 <c>DML_BUFFER_BINDING[BindingCount]</c>。</summary>
        Public Bindings As IntPtr
    End Structure

    ''' <summary>绑定描述。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_BINDING_DESC
        Public Type As DML_BINDING_TYPE
        Public Desc As IntPtr

        Public Sub New(type As DML_BINDING_TYPE, desc As IntPtr)
            Me.Type = type
            Me.Desc = desc
        End Sub
    End Structure

    ''' <summary>绑定表描述。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_BINDING_TABLE_DESC
        ''' <summary>IDMLDispatchable*</summary>
        Public Dispatchable As IntPtr
        Public CPUDescriptorHandle As D3D12_CPU_DESCRIPTOR_HANDLE
        Public GPUDescriptorHandle As D3D12_GPU_DESCRIPTOR_HANDLE
        Public SizeInDescriptors As UInteger
    End Structure

    ''' <summary>编译后算子需要的绑定信息。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_BINDING_PROPERTIES
        Public RequiredDescriptorCount As UInteger
        Public TemporaryResourceSize As ULong
        Public PersistentResourceSize As ULong
    End Structure

    ''' <summary>元素级算子的可选缩放/偏移。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_SCALE_BIAS
        Public Scale As Single
        Public Bias As Single

        Public Sub New(scale As Single, bias As Single)
            Me.Scale = scale
            Me.Bias = bias
        End Sub
    End Structure

    ''' <summary>二维尺寸。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_SIZE_2D
        Public Width As UInteger
        Public Height As UInteger

        Public Sub New(width As UInteger, height As UInteger)
            Me.Width = width
            Me.Height = height
        End Sub
    End Structure

    ' ------------------------------------------------------------------
    ' 特性查询
    ' ------------------------------------------------------------------

    ''' <summary>特性级别查询的输入。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_FEATURE_QUERY_FEATURE_LEVELS
        Public RequestedFeatureLevelCount As UInteger
        Public RequestedFeatureLevels As IntPtr
    End Structure

    ''' <summary>特性级别查询的输出。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_FEATURE_DATA_FEATURE_LEVELS
        Public MaxSupportedFeatureLevel As DML_FEATURE_LEVEL
    End Structure

    ''' <summary>数据类型支持查询的输入。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_FEATURE_QUERY_TENSOR_DATA_TYPE_SUPPORT
        Public DataType As DML_TENSOR_DATA_TYPE
    End Structure

    ''' <summary>数据类型支持查询的输出。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_FEATURE_DATA_TENSOR_DATA_TYPE_SUPPORT
        Public IsSupported As Integer
    End Structure

End Namespace
