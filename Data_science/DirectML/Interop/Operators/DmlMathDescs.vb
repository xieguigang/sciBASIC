' ------------------------------------------------------------------------
' 矩阵乘、归约与归一化算子的原生描述符
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>DML_GEMM_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_GEMM_OPERATOR_DESC
        Public ATensor As IntPtr
        Public BTensor As IntPtr
        Public CTensor As IntPtr
        Public OutputTensor As IntPtr
        Public TransA As DML_MATRIX_TRANSFORM
        Public TransB As DML_MATRIX_TRANSFORM
        Public Alpha As Single
        Public Beta As Single
        Public FusedActivation As IntPtr
    End Structure

    ''' <summary>DML_REDUCE_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_REDUCE_OPERATOR_DESC
        Public [Function] As DML_REDUCE_FUNCTION
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public AxisCount As UInteger
        Public Axes As IntPtr
    End Structure

    ''' <summary>DML_LOCAL_RESPONSE_NORMALIZATION_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_LOCAL_RESPONSE_NORMALIZATION_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public CrossChannel As Integer
        Public LocalSize As UInteger
        Public Alpha As Single
        Public Beta As Single
        Public Bias As Single
    End Structure

    ''' <summary>DML_LP_NORMALIZATION_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_LP_NORMALIZATION_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Axis As UInteger
        Public Epsilon As Single
        Public P As UInteger
    End Structure

    ''' <summary>DML_MEAN_VARIANCE_NORMALIZATION1_OPERATOR_DESC（按轴归一化，用于 LayerNormalization）</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_MEAN_VARIANCE_NORMALIZATION1_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public ScaleTensor As IntPtr
        Public BiasTensor As IntPtr
        Public OutputTensor As IntPtr
        Public AxisCount As UInteger
        Public Axes As IntPtr
        Public NormalizeVariance As Integer
        Public Epsilon As Single
        Public FusedActivation As IntPtr
    End Structure

    ''' <summary>DML_MEAN_VARIANCE_NORMALIZATION_OPERATOR_DESC（跨通道归一化，用于 InstanceNormalization）</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_MEAN_VARIANCE_NORMALIZATION_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public ScaleTensor As IntPtr
        Public BiasTensor As IntPtr
        Public OutputTensor As IntPtr
        Public CrossChannel As Integer
        Public NormalizeVariance As Integer
        Public Epsilon As Single
        Public FusedActivation As IntPtr
    End Structure

    ''' <summary>DML_BATCH_NORMALIZATION_OPERATOR_DESC（推理期：均值/方差为常量输入）</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_BATCH_NORMALIZATION_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public MeanTensor As IntPtr
        Public VarianceTensor As IntPtr
        Public ScaleTensor As IntPtr
        Public BiasTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Spatial As Integer
        Public Epsilon As Single
        Public FusedActivation As IntPtr
    End Structure

    ''' <summary>DML_MATRIX_MULTIPLY_INTEGER_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_MATRIX_MULTIPLY_INTEGER_OPERATOR_DESC
        Public ATensor As IntPtr
        Public AZeroPointTensor As IntPtr
        Public BTensor As IntPtr
        Public BZeroPointTensor As IntPtr
        Public OutputTensor As IntPtr
    End Structure

End Namespace
