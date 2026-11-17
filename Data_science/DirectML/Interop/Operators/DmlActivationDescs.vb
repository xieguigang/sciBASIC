' ------------------------------------------------------------------------
' 逐元素算子与激活算子的原生描述符
'
' 字段顺序与 DirectML.h 完全一致；可选字段（_Maybenull_）用 IntPtr 表示，
' 传 IntPtr.Zero 即表示不使用。
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Interop

#Region "ELEMENT WISE"

    ''' <summary>一元逐元素算子（带可选缩放/偏移）。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ELEMENT_WISE_UNARY_SCALE_BIAS_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public ScaleBias As IntPtr
    End Structure

    ''' <summary>一元逐元素算子（无缩放/偏移）。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ELEMENT_WISE_UNARY_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
    End Structure

    ''' <summary>二元逐元素算子。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ELEMENT_WISE_BINARY_DESC
        Public ATensor As IntPtr
        Public BTensor As IntPtr
        Public OutputTensor As IntPtr
    End Structure

    ''' <summary>DML_ELEMENT_WISE_ROUND_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ELEMENT_WISE_ROUND_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public RoundingMode As DML_ROUNDING_MODE
    End Structure

    ''' <summary>DML_ELEMENT_WISE_POW_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ELEMENT_WISE_POW_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public ExponentTensor As IntPtr
        Public OutputTensor As IntPtr
        Public ScaleBias As IntPtr
    End Structure

    ''' <summary>DML_ELEMENT_WISE_CLIP_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ELEMENT_WISE_CLIP_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public ScaleBias As IntPtr
        Public Min As Single
        Public Max As Single
    End Structure

    ''' <summary>DML_ELEMENT_WISE_THRESHOLD_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ELEMENT_WISE_THRESHOLD_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public ScaleBias As IntPtr
        Public Min As Single
    End Structure

    ''' <summary>DML_ELEMENT_WISE_IF_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ELEMENT_WISE_IF_OPERATOR_DESC
        Public ConditionTensor As IntPtr
        Public ATensor As IntPtr
        Public BTensor As IntPtr
        Public OutputTensor As IntPtr
    End Structure

    ''' <summary>DML_ELEMENT_WISE_QUANTIZE_LINEAR_OPERATOR_DESC / DEQUANTIZE_LINEAR</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ELEMENT_WISE_QUANTIZE_LINEAR_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public ScaleTensor As IntPtr
        Public ZeroPointTensor As IntPtr
        Public OutputTensor As IntPtr
    End Structure

#End Region

#Region "ACTIVATION"

    ''' <summary>只有输入与输出的激活算子（Relu/Sigmoid/Tanh/Gelu/Hardmax...）。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ACTIVATION_RELU_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
    End Structure

    ''' <summary>DML_ACTIVATION_LEAKY_RELU_OPERATOR_DESC / ELU / CELU / THRESHOLDED_RELU</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ACTIVATION_ALPHA_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Alpha As Single
    End Structure

    ''' <summary>DML_ACTIVATION_HARD_SIGMOID_OPERATOR_DESC / LINEAR / SCALED_TANH / PARAMETRIC_SOFTPLUS / HARD_SWISH</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ACTIVATION_ALPHA_BETA_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Alpha As Single
        Public Beta As Single
    End Structure

    ''' <summary>DML_ACTIVATION_SCALED_ELU_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ACTIVATION_SCALED_ELU_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Alpha As Single
        Public Gamma As Single
    End Structure

    ''' <summary>DML_ACTIVATION_SOFTMAX_OPERATOR_DESC / LOG_SOFTMAX / SOFTSIGN / GELU</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ACTIVATION_SIMPLE_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
    End Structure

    ''' <summary>DML_ACTIVATION_SOFTPLUS_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ACTIVATION_SOFTPLUS_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Steepness As Single
    End Structure

    ''' <summary>DML_ACTIVATION_SWISH_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ACTIVATION_SWISH_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public SigmoidInputScale As Single
    End Structure

    ''' <summary>DML_ACTIVATION_SHRINK_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ACTIVATION_SHRINK_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public Bias As Single
        Public Threshold As Single
    End Structure

    ''' <summary>DML_ACTIVATION_PARAMETERIZED_RELU_OPERATOR_DESC</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ACTIVATION_PARAMETERIZED_RELU_OPERATOR_DESC
        Public InputTensor As IntPtr
        Public SlopeTensor As IntPtr
        Public OutputTensor As IntPtr
    End Structure

    ''' <summary>DML_ACTIVATION_SOFTMAX1_OPERATOR_DESC / LOG_SOFTMAX1 / HARDMAX1</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DML_ACTIVATION_AXES_DESC
        Public InputTensor As IntPtr
        Public OutputTensor As IntPtr
        Public AxisCount As UInteger
        Public Axes As IntPtr
    End Structure

#End Region

End Namespace
