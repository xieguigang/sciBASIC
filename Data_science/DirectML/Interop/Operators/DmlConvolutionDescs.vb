' ------------------------------------------------------------------------
' 卷积、反卷积与池化算子的原生描述符
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>DML_CONVOLUTION_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_CONVOLUTION_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public FilterTensor As IntPtr
        Public BiasTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Mode As DML_CONVOLUTION_MODE
        Public Direction As DML_CONVOLUTION_DIRECTION
        Public DimensionCount As UInteger
        Public Strides As IntPtr
        Public Dilations As IntPtr
        Public StartPadding As IntPtr
        Public EndPadding As IntPtr
        Public OutputPadding As IntPtr
        Public GroupCount As UInteger
        Public FusedActivation As IntPtr
    End Structure

    ''' <summary>DML_AVERAGE_POOLING_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_AVERAGE_POOLING_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public DimensionCount As UInteger
        Public Strides As IntPtr
        Public WindowSize As IntPtr
        Public StartPadding As IntPtr
        Public EndPadding As IntPtr
        Public IncludePadding As Integer
    End Structure

    ''' <summary>DML_MAX_POOLING_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_MAX_POOLING_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public DimensionCount As UInteger
        Public Strides As IntPtr
        Public WindowSize As IntPtr
        Public StartPadding As IntPtr
        Public EndPadding As IntPtr
    End Structure

    ''' <summary>DML_MAX_POOLING2_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_MAX_POOLING2_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public OutputIndicesTensor As IntPtr
        Public DimensionCount As UInteger
        Public Strides As IntPtr
        Public WindowSize As IntPtr
        Public StartPadding As IntPtr
        Public EndPadding As IntPtr
        Public Dilations As IntPtr
    End Structure

    ''' <summary>DML_LP_POOLING_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_LP_POOLING_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public DimensionCount As UInteger
        Public Strides As IntPtr
        Public WindowSize As IntPtr
        Public StartPadding As IntPtr
        Public EndPadding As IntPtr
        Public P As UInteger
    End Structure

    ''' <summary>DML_MAX_UNPOOLING_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_MAX_UNPOOLING_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public IndicesTensor As IntPtr
        Public OutputTensor As IntPtr
    End Structure

End Namespace
