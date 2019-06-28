Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Discretion
Imports Microsoft.VisualBasic.Math.Distributions

Namespace ComponentModel.Normalizer

    Public Enum Methods
        ''' <summary>
        ''' 归一化到[0, 1]区间内
        ''' </summary>
        NormalScaler
        ''' <summary>
        ''' 直接 x / max 进行归一化, 当出现极值的时候, 此方法无效, 根据数据分布,可能会归一化到[0, 1] 或者 [-1, 1]区间内
        ''' </summary>
        RelativeScaler
        ''' <summary>
        ''' 通过对数据进行区间离散化来完成归一化
        ''' </summary>
        RangeDiscretizer
    End Enum

    Public Module Normalizations

        ReadOnly methodTable As Dictionary(Of String, Methods)

        Sub New()
            methodTable = Enums(Of Methods).ToDictionary(Function(name) name.ToString.ToLower)
        End Sub

        Public Function ParseMethod(name As String) As Methods
            Return methodTable.TryGetValue(Strings.LCase(name), [default]:=Methods.NormalScaler)
        End Function

        ReadOnly normalRange As DoubleRange = {0, 1}

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ScalerNormalize(samples As SampleDistribution, x#) As Double
            If x > samples.max Then
                Return 1
            ElseIf x < samples.min Then
                Return 0
            Else
                x = samples.GetRange.ScaleMapping(x, normalRange)
            End If

            If x.IsNaNImaginary Then
                Return samples.average
            Else
                Return x
            End If
        End Function

        ''' <summary>
        ''' 正实数和负实数是分开进行归一化的
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <param name="x#"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RelativeNormalize(samples As SampleDistribution, x#) As Double
            If x > 0 Then
                If x > samples.max Then
                    Return 1
                Else
                    Return x / samples.max
                End If
            ElseIf x = 0R Then
                Return 0
            Else
                ' 负实数需要考察一下
                If x < samples.min Then
                    Return -1
                ElseIf samples.min >= 0 Then
                    Return -1
                Else
                    Return x / Math.Abs(samples.min)
                End If
            End If
        End Function

        Public Function RangeDiscretizer(samples As SampleDistribution, x#) As Double
            Static cache As New Dictionary(Of SampleDistribution, NormalRangeDiscretizer)

            Return cache.ComputeIfAbsent(
                key:=samples,
                lazyValue:=Function(dist)
                               Return New NormalRangeDiscretizer(dist)
                           End Function) _
               .Normalize(x)
        End Function

    End Module
End Namespace