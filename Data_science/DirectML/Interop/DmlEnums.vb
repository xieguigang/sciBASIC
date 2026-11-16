' ------------------------------------------------------------------------
' DirectML 原生枚举
'
' 全部取值来自 Windows SDK 的 DirectML.h（10.0.26100.0），已逐项核对，
' 不要凭记忆修改：这些整数会直接传给 DMLCreateDevice / CreateOperator。
' ------------------------------------------------------------------------

Namespace Interop

    ''' <summary>张量元素的数据类型。</summary>
    Public Enum DML_TENSOR_DATA_TYPE As Integer
        UNKNOWN = 0
        FLOAT32 = 1
        FLOAT16 = 2
        UINT32 = 3
        UINT16 = 4
        UINT8 = 5
        INT32 = 6
        INT16 = 7
        INT8 = 8
        FLOAT64 = 9
        UINT64 = 10
        INT64 = 11
    End Enum

    ''' <summary>张量描述的标志位。</summary>
    <Flags>
    Public Enum DML_TENSOR_FLAGS As UInteger
        NONE = 0
        ''' <summary>张量数据由 DML 持有，必须在算子初始化时通过持久资源绑定上传。</summary>
        OWNED_BY_DML = 1
    End Enum

    ''' <summary>张量描述的类型。</summary>
    Public Enum DML_TENSOR_TYPE As Integer
        INVALID = 0
        BUFFER = 1
    End Enum

    ''' <summary>绑定描述的类型。</summary>
    Public Enum DML_BINDING_TYPE As Integer
        NONE = 0
        BUFFER = 1
        BUFFER_ARRAY = 2
    End Enum

    ''' <summary>归约函数。</summary>
    Public Enum DML_REDUCE_FUNCTION As Integer
        ARGMAX = 0
        ARGMIN = 1
        AVERAGE = 2
        L1 = 3
        L2 = 4
        LOG_SUM = 5
        LOG_SUM_EXP = 6
        MAX = 7
        MIN = 8
        MULTIPLY = 9
        SUM = 10
        SUM_SQUARE = 11
    End Enum

    ''' <summary>卷积模式。</summary>
    Public Enum DML_CONVOLUTION_MODE As Integer
        CONVOLUTION = 0
        CROSS_CORRELATION = 1
    End Enum

    ''' <summary>卷积方向。</summary>
    Public Enum DML_CONVOLUTION_DIRECTION As Integer
        FORWARD = 0
        BACKWARD = 1
    End Enum

    ''' <summary>填充模式。</summary>
    Public Enum DML_PADDING_MODE As Integer
        CONSTANT = 0
        EDGE = 1
        REFLECTION = 2
        SYMMETRIC = 3
        WRAP = 4
    End Enum

    ''' <summary>矩阵变换（是否转置）。</summary>
    Public Enum DML_MATRIX_TRANSFORM As Integer
        NONE = 0
        TRANSPOSE = 1
    End Enum

    ''' <summary>重采样插值模式。</summary>
    Public Enum DML_INTERPOLATION_MODE As Integer
        NEAREST_NEIGHBOR = 0
        LINEAR = 1
    End Enum

    ''' <summary>轴方向。</summary>
    Public Enum DML_AXIS_DIRECTION As Integer
        INCREASING = 0
        DECREASING = 1
    End Enum

    ''' <summary>DepthToSpace / SpaceToDepth 的排列顺序。</summary>
    Public Enum DML_DEPTH_SPACE_ORDER As Integer
        DEPTH_COLUMN_ROW = 0
        COLUMN_ROW_DEPTH = 1
    End Enum

    ''' <summary>取整模式。</summary>
    Public Enum DML_ROUNDING_MODE As Integer
        HALVES_TO_NEAREST_EVEN = 0
        TOWARD_ZERO = 1
        TOWARD_INFINITY = 2
    End Enum

    ''' <summary>特性查询类型。</summary>
    Public Enum DML_FEATURE As Integer
        TENSOR_DATA_TYPE_SUPPORT = 0
        FEATURE_LEVELS = 1
    End Enum

    ''' <summary>DirectML 特性级别（每个级别对应一批算子）。</summary>
    Public Enum DML_FEATURE_LEVEL As Integer
        LEVEL_1_0 = &H1000
        LEVEL_1_1 = &H1100
        LEVEL_2_0 = &H2000
        LEVEL_2_1 = &H2100
        LEVEL_2_2 = &H2200
        LEVEL_2_3 = &H2300
        LEVEL_2_4 = &H2400
        LEVEL_3_0 = &H3000
        LEVEL_3_1 = &H3100
        LEVEL_4_0 = &H4000
        LEVEL_4_1 = &H4100
        LEVEL_5_0 = &H5000
        LEVEL_5_1 = &H5100
        LEVEL_6_0 = &H6000
        LEVEL_6_1 = &H6100
    End Enum

    ''' <summary>DML 设备创建标志。</summary>
    <Flags>
    Public Enum DML_CREATE_DEVICE_FLAGS As UInteger
        NONE = 0
        DEBUG = 1
    End Enum

    ''' <summary>算子编译（执行）标志。</summary>
    <Flags>
    Public Enum DML_EXECUTION_FLAGS As UInteger
        NONE = 0
        ''' <summary>允许 DML 在内部使用 FP16 计算（更快，但误差更大）。</summary>
        ALLOW_HALF_PRECISION_COMPUTATION = 1
        DISABLE_META_COMMANDS = 2
        DESCRIPTORS_VOLATILE = 4
    End Enum

    ''' <summary>
    ''' DirectML 算子类型。
    ''' </summary>
    ''' <remarks>
    ''' 只列出本框架实际会用到的算子；序号与 DirectML.h 中的声明顺序完全一致
    ''' （该枚举在头文件里没有显式赋值，序号即声明顺序）。
    ''' </remarks>
    Public Enum DML_OPERATOR_TYPE As Integer
        INVALID = 0

        ELEMENT_WISE_IDENTITY = 1
        ELEMENT_WISE_ABS = 2
        ELEMENT_WISE_ACOS = 3
        ELEMENT_WISE_ADD = 4
        ELEMENT_WISE_ASIN = 5
        ELEMENT_WISE_ATAN = 6
        ELEMENT_WISE_CEIL = 7
        ELEMENT_WISE_CLIP = 8
        ELEMENT_WISE_COS = 9
        ELEMENT_WISE_DIVIDE = 10
        ELEMENT_WISE_EXP = 11
        ELEMENT_WISE_FLOOR = 12
        ELEMENT_WISE_LOG = 13
        ELEMENT_WISE_LOGICAL_AND = 14
        ELEMENT_WISE_LOGICAL_EQUALS = 15
        ELEMENT_WISE_LOGICAL_GREATER_THAN = 16
        ELEMENT_WISE_LOGICAL_LESS_THAN = 17
        ELEMENT_WISE_LOGICAL_NOT = 18
        ELEMENT_WISE_LOGICAL_OR = 19
        ELEMENT_WISE_LOGICAL_XOR = 20
        ELEMENT_WISE_MAX = 21
        ELEMENT_WISE_MEAN = 22
        ELEMENT_WISE_MIN = 23
        ELEMENT_WISE_MULTIPLY = 24
        ELEMENT_WISE_POW = 25
        ELEMENT_WISE_CONSTANT_POW = 26
        ELEMENT_WISE_RECIP = 27
        ELEMENT_WISE_SIN = 28
        ELEMENT_WISE_SQRT = 29
        ELEMENT_WISE_SUBTRACT = 30
        ELEMENT_WISE_TAN = 31
        ELEMENT_WISE_THRESHOLD = 32
        ELEMENT_WISE_QUANTIZE_LINEAR = 33
        ELEMENT_WISE_DEQUANTIZE_LINEAR = 34
        ACTIVATION_ELU = 35
        ACTIVATION_HARDMAX = 36
        ACTIVATION_HARD_SIGMOID = 37
        ACTIVATION_IDENTITY = 38
        ACTIVATION_LEAKY_RELU = 39
        ACTIVATION_LINEAR = 40
        ACTIVATION_LOG_SOFTMAX = 41
        ACTIVATION_PARAMETERIZED_RELU = 42
        ACTIVATION_PARAMETRIC_SOFTPLUS = 43
        ACTIVATION_RELU = 44
        ACTIVATION_SCALED_ELU = 45
        ACTIVATION_SCALED_TANH = 46
        ACTIVATION_SIGMOID = 47
        ACTIVATION_SOFTMAX = 48
        ACTIVATION_SOFTPLUS = 49
        ACTIVATION_SOFTSIGN = 50
        ACTIVATION_TANH = 51
        ACTIVATION_THRESHOLDED_RELU = 52
        CONVOLUTION = 53
        GEMM = 54
        REDUCE = 55
        AVERAGE_POOLING = 56
        LP_POOLING = 57
        MAX_POOLING = 58
        ROI_POOLING = 59
        SLICE = 60
        CAST = 61
        SPLIT = 62
        JOIN = 63
        PADDING = 64
        VALUE_SCALE_2D = 65
        UPSAMPLE_2D = 66
        GATHER = 67
        SPACE_TO_DEPTH = 68
        DEPTH_TO_SPACE = 69
        TILE = 70
        TOP_K = 71
        BATCH_NORMALIZATION = 72
        MEAN_VARIANCE_NORMALIZATION = 73
        LOCAL_RESPONSE_NORMALIZATION = 74
        LP_NORMALIZATION = 75
        RNN = 76
        LSTM = 77
        GRU = 78
        ELEMENT_WISE_SIGN = 79
        ELEMENT_WISE_IS_NAN = 80
        ELEMENT_WISE_ERF = 81
        ELEMENT_WISE_SINH = 82
        ELEMENT_WISE_COSH = 83
        ELEMENT_WISE_TANH = 84
        ELEMENT_WISE_ASINH = 85
        ELEMENT_WISE_ACOSH = 86
        ELEMENT_WISE_ATANH = 87
        ELEMENT_WISE_IF = 88
        ELEMENT_WISE_ADD1 = 89
        ACTIVATION_SHRINK = 90
        MAX_POOLING1 = 91
        MAX_UNPOOLING = 92
        DIAGONAL_MATRIX = 93
        SCATTER_ELEMENTS = 94
        ONE_HOT = 95
        RESAMPLE = 96
        ELEMENT_WISE_BIT_SHIFT_LEFT = 97
        ELEMENT_WISE_BIT_SHIFT_RIGHT = 98
        ELEMENT_WISE_ROUND = 99
        ELEMENT_WISE_IS_INFINITY = 100
        ELEMENT_WISE_MODULUS_TRUNCATE = 101
        ELEMENT_WISE_MODULUS_FLOOR = 102
        FILL_VALUE_CONSTANT = 103
        FILL_VALUE_SEQUENCE = 104
        CUMULATIVE_SUMMATION = 105
        REVERSE_SUBSEQUENCES = 106
        GATHER_ELEMENTS = 107
        GATHER_ND = 108
        SCATTER_ND = 109
        MAX_POOLING2 = 110
        SLICE1 = 111
        TOP_K1 = 112
        DEPTH_TO_SPACE1 = 113
        SPACE_TO_DEPTH1 = 114
        MEAN_VARIANCE_NORMALIZATION1 = 115
        RESAMPLE1 = 116
        MATRIX_MULTIPLY_INTEGER = 117
        QUANTIZED_LINEAR_MATRIX_MULTIPLY = 118
        CONVOLUTION_INTEGER = 119
        QUANTIZED_LINEAR_CONVOLUTION = 120
        ELEMENT_WISE_BIT_AND = 121
        ELEMENT_WISE_BIT_OR = 122
        ELEMENT_WISE_BIT_XOR = 123
        ELEMENT_WISE_BIT_NOT = 124
        ELEMENT_WISE_BIT_COUNT = 125
        ELEMENT_WISE_LOGICAL_GREATER_THAN_OR_EQUAL = 126
        ELEMENT_WISE_LOGICAL_LESS_THAN_OR_EQUAL = 127
        ACTIVATION_CELU = 128
        RANDOM_GENERATOR = 132
        NONZERO_COORDINATES = 133
        ARGMIN = 137
        ARGMAX = 138
        ROI_ALIGN = 139
        GATHER_ND1 = 140
        ELEMENT_WISE_ATAN_YX = 141
        ELEMENT_WISE_DIFFERENCE_SQUARE = 143
        CUMULATIVE_PRODUCT = 145
        DYNAMIC_QUANTIZE_LINEAR = 148
        ROI_ALIGN1 = 149
        BATCH_NORMALIZATION_TRAINING = 151
        ELEMENT_WISE_CLIP1 = 153
        PADDING1 = 155
        ELEMENT_WISE_NEGATE = 156
        ACTIVATION_GELU = 157
        ACTIVATION_SOFTMAX1 = 158
        ACTIVATION_LOG_SOFTMAX1 = 159
        ACTIVATION_HARDMAX1 = 160
        RESAMPLE2 = 161
        DIAGONAL_MATRIX1 = 163
        MULTIHEAD_ATTENTION = 164
        LP_POOLING1 = 165
        AVERAGE_POOLING1 = 166
        ACTIVATION_SWISH = 167
        ACTIVATION_HARD_SWISH = 168
        QUANTIZED_LINEAR_AVERAGE_POOLING = 169
        MATRIX_MULTIPLY_INTEGER_TO_FLOAT = 170
        MEAN_VARIANCE_NORMALIZATION2 = 171
        MULTIHEAD_ATTENTION1 = 172
        QUANTIZE = 173
        DEQUANTIZE = 174
        RESAMPLE3 = 175
    End Enum

    ''' <summary>DML 数据类型与元素字节数的换算。</summary>
    Public Module DmlTensorDataTypeHelper

        ''' <summary>
        ''' 取得元素字节数；未知类型返回 0。
        ''' </summary>
        Public Function GetElementSize(dataType As DML_TENSOR_DATA_TYPE) As Integer
            Select Case dataType
                Case DML_TENSOR_DATA_TYPE.FLOAT32,
                     DML_TENSOR_DATA_TYPE.UINT32,
                     DML_TENSOR_DATA_TYPE.INT32
                    Return 4

                Case DML_TENSOR_DATA_TYPE.FLOAT16,
                     DML_TENSOR_DATA_TYPE.UINT16,
                     DML_TENSOR_DATA_TYPE.INT16
                    Return 2

                Case DML_TENSOR_DATA_TYPE.UINT8, DML_TENSOR_DATA_TYPE.INT8
                    Return 1

                Case DML_TENSOR_DATA_TYPE.FLOAT64,
                     DML_TENSOR_DATA_TYPE.UINT64,
                     DML_TENSOR_DATA_TYPE.INT64
                    Return 8

                Case Else
                    Return 0
            End Select
        End Function

        ''' <summary>取得数据类型名称。</summary>
        Public Function GetName(dataType As DML_TENSOR_DATA_TYPE) As String
            Return dataType.ToString()
        End Function

    End Module

End Namespace
