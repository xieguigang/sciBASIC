' ------------------------------------------------------------------------
' 形状/布局/数据搬运类算子的原生描述符
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>DML_SLICE_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_SLICE_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public DimensionCount As UInteger
        Public Offsets As IntPtr
        Public Sizes As IntPtr
        Public Strides As IntPtr
    End Structure

    ''' <summary>DML_CAST_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_CAST_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
    End Structure

    ''' <summary>DML_JOIN_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_JOIN_OPERATOR_DESC
        Public InputCount As UInteger
        Public InputTensors As IntPtr
        Public OutputTensor As IntPtr
        Public Axis As UInteger
    End Structure

    ''' <summary>DML_SPLIT_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_SPLIT_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputCount As UInteger
        Public OutputTensors As IntPtr
        Public Axis As UInteger
    End Structure

    ''' <summary>DML_PADDING_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_PADDING_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public PaddingMode As DML_PADDING_MODE
        Public PaddingValue As Single
        Public DimensionCount As UInteger
        Public StartPadding As IntPtr
        Public EndPadding As IntPtr
    End Structure

    ''' <summary>DML_TILE_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_TILE_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public RepeatsCount As UInteger
        Public Repeats As IntPtr
    End Structure

    ''' <summary>DML_GATHER_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_GATHER_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public IndicesTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Axis As UInteger
        Public IndexDimensions As UInteger
    End Structure

    ''' <summary>DML_GATHER_ELEMENTS_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_GATHER_ELEMENTS_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public IndicesTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Axis As UInteger
    End Structure

    ''' <summary>DML_GATHER_ND_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_GATHER_ND_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public IndicesTensor As IntPtr
        Public OutputTensor As IntPtr
        Public InputDimensionCount As UInteger
        Public IndicesDimensionCount As UInteger
    End Structure

    ''' <summary>DML_SCATTER_ND_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_SCATTER_ND_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public IndicesTensor As IntPtr
        Public UpdatesTensor As IntPtr
        Public OutputTensor As IntPtr
        Public InputDimensionCount As UInteger
        Public IndicesDimensionCount As UInteger
    End Structure

    ''' <summary>DML_TOP_K1_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_TOP_K1_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputValueTensor As IntPtr
        Public OutputIndexTensor As IntPtr
        Public Axis As UInteger
        Public K As UInteger
        Public AxisDirection As DML_AXIS_DIRECTION
    End Structure

    ''' <summary>DML_ARGMAX_OPERATOR_DESC / DML_ARGMIN</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ARGMIN_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public AxisCount As UInteger
        Public Axes As IntPtr
        Public AxisDirection As DML_AXIS_DIRECTION
    End Structure

    ''' <summary>DML_DEPTH_TO_SPACE_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_DEPTH_TO_SPACE_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public BlockSize As UInteger

    End Structure

    ''' <summary>DML_DEPTH_TO_SPACE1_OPERATOR_DESC / SPACE_TO_DEPTH1</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_DEPTH_TO_SPACE1_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public BlockSize As UInteger
        Public Order As DML_DEPTH_SPACE_ORDER
    End Structure

    ''' <summary>DML_SPACE_TO_DEPTH_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_SPACE_TO_DEPTH_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public BlockSize As UInteger
    End Structure

    ''' <summary>DML_RESAMPLE1_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_RESAMPLE1_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public InterpolationMode As DML_INTERPOLATION_MODE
        Public DimensionCount As UInteger
        Public Scales As IntPtr
        Public InputPixelOffsets As IntPtr
        Public OutputPixelOffsets As IntPtr
    End Structure

    ''' <summary>DML_ONE_HOT_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ONE_HOT_OPERATOR_DESC
        Public IndicesTensor As IntPtr
        Public ValuesTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Axis As UInteger
    End Structure

    ''' <summary>DML_DIAGONAL_MATRIX1_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_DIAGONAL_MATRIX1_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Offset As Integer
        Public Value As Single
    End Structure

    ''' <summary>DML_CUMULATIVE_SUMMATION_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_CUMULATIVE_SUMMATION_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Axis As UInteger
        Public AxisDirection As DML_AXIS_DIRECTION
        Public HasExclusiveSum As Integer
    End Structure

    ''' <summary>DML_REVERSE_SUBSEQUENCES_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_REVERSE_SUBSEQUENCES_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public SequenceLengthsTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Axis As UInteger
    End Structure

End Namespace
