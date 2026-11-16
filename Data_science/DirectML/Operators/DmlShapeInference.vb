' ------------------------------------------------------------------------
' 形状推导：卷积与池化的输出尺寸
'
' 公式与 ONNX 规范一致：
'   output = floor((input + padStart + padEnd - dilation * (kernel - 1) - 1) / stride) + 1
' auto_pad 语义：
'   NOTSET  -> 使用显式 pads
'   SAME_UPPER / SAME_LOWER -> 输出 = ceil(input / stride)，padding 自动补齐
'   VALID   -> 不填充
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Operators

    ''' <summary>卷积/池化的形状推导。</summary>
    Public Module DmlShapeInference

        ''' <summary>
        ''' 计算卷积/池化在一个维度上的输出长度。
        ''' </summary>
        Public Function OutputSize(inputSize As Integer, kernel As Integer, stride As Integer,
                                   dilation As Integer, padStart As Integer, padEnd As Integer) As Integer
            If stride <= 0 Then
                stride = 1
            End If

            If dilation <= 0 Then
                dilation = 1
            End If

            Dim effective As Integer = dilation * (kernel - 1) + 1
            Dim value As Integer = (inputSize + padStart + padEnd - effective) \ stride + 1

            Return System.Math.Max(0, value)
        End Function

        ''' <summary>
        ''' 按 auto_pad 计算实际 padding（返回 (start, end) 对应的两个数组）。
        ''' </summary>
        ''' <param name="autoPad">NOTSET / SAME_UPPER / SAME_LOWER / VALID。</param>
        ''' <param name="inputSizes">空间维度输入尺寸。</param>
        ''' <param name="kernel">卷积核尺寸。</param>
        ''' <param name="strides">步长。</param>
        ''' <param name="dilations">膨胀。</param>
        ''' <param name="startPadding">输出：起点 padding。</param>
        ''' <param name="endPadding">输出：终点 padding。</param>
        Public Sub ResolveAutoPadding(autoPad As String,
                                      inputSizes As Integer(),
                                      kernel As Integer(),
                                      strides As Integer(),
                                      dilations As Integer(),
                                      ByRef startPadding As Integer(),
                                      ByRef endPadding As Integer())

            Dim n As Integer = inputSizes.Length

            startPadding = New Integer(n - 1) {}
            endPadding = New Integer(n - 1) {}

            Dim mode As String = If(autoPad, "NOTSET").ToUpperInvariant()

            If mode = "VALID" Then
                Return
            End If

            If mode <> "SAME_UPPER" AndAlso mode <> "SAME_LOWER" Then
                Return
            End If

            For i As Integer = 0 To n - 1
                Dim stride As Integer = If(i < strides.Length AndAlso strides(i) > 0, strides(i), 1)
                Dim dilation As Integer = If(i < dilations.Length AndAlso dilations(i) > 0, dilations(i), 1)
                Dim effective As Integer = dilation * (kernel(i) - 1) + 1
                Dim outSize As Integer = (inputSizes(i) + stride - 1) \ stride
                Dim total As Integer = System.Math.Max(0, (outSize - 1) * stride + effective - inputSizes(i))

                If mode = "SAME_UPPER" Then
                    startPadding(i) = total \ 2
                    endPadding(i) = total - startPadding(i)
                Else
                    endPadding(i) = total \ 2
                    startPadding(i) = total - endPadding(i)
                End If
            Next
        End Sub

        ''' <summary>把 <see cref="Integer"/> 数组转成 <see cref="UInteger"/> 数组（负数按 0 处理）。</summary>
        Public Function ToUnsigned(values As Integer()) As UInteger()
            If values Is Nothing Then
                Return New UInteger() {}
            End If

            Dim result As UInteger() = New UInteger(values.Length - 1) {}

            For i As Integer = 0 To values.Length - 1
                result(i) = CUInt(System.Math.Max(0, values(i)))
            Next

            Return result
        End Function

        ''' <summary>计算元素总数。</summary>
        Public Function ElementCount(dims As UInteger()) As ULong
            Dim n As ULong = 1UL

            For Each d As UInteger In dims
                n *= d
            Next

            Return n
        End Function

        ''' <summary>把形状转成调试文本。</summary>
        Public Function ShapeText(dims As UInteger()) As String
            Return "[" & String.Join(", ", dims) & "]"
        End Function

    End Module

End Namespace
