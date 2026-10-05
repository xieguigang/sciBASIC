' ============================================================================
'  AxisTicks.vb - 坐标轴刻度生成辅助
'
'  从旧 Plots 项目的 g/Axis/AxisScalling.vb（CreateAxisTicks）迁移而来的简化版：
'  采用 "nice step"（1/2/5/10 幂次）算法生成可读刻度。
' ============================================================================

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports std = System.Math

''' <summary>坐标轴刻度生成辅助模块</summary>
Public Module AxisTicksHelper

    <Extension>
    Public Function CreateAxisTicks(range As DoubleRange,
                                    Optional ticks As Integer = 10,
                                    Optional decimalDigits As Integer = 2) As Double()

        If Double.IsNaN(range.Min) OrElse Double.IsNaN(range.Max) OrElse
            Double.IsInfinity(range.Min) OrElse Double.IsInfinity(range.Max) Then
            Return {0, 1}
        End If

        If range.Min = 0 AndAlso range.Max = 0 Then
            Return {0, 1}
        End If

        If range.Length = 0 Then
            Dim delta As Double = std.Abs(range.Max) * 0.25 + 0.5

            range = New DoubleRange(range.Min - delta, range.Max + delta)
        End If

        Return CreateAxisTicks(range.Min, range.Max, ticks, decimalDigits)
    End Function

    <Extension>
    Public Function CreateAxisTicks(range As (min As Double, max As Double),
                                    Optional ticks As Integer = 10,
                                    Optional decimalDigits As Integer = 2) As Double()

        Return New DoubleRange(range.min, range.max).CreateAxisTicks(ticks, decimalDigits)
    End Function

    <Extension>
    Public Function CreateAxisTicks(data As IEnumerable(Of Double),
                                    Optional ticks As Integer = 10,
                                    Optional decimalDigits As Integer = 2) As Double()

        Dim values As Double() = data.Where(Function(x) Not Double.IsNaN(x) AndAlso Not Double.IsInfinity(x)).ToArray

        If values.Length = 0 Then
            Return {0, 1}
        End If

        Dim r As New DoubleRange(values)

        Return r.CreateAxisTicks(ticks, decimalDigits)
    End Function

    <Extension>
    Public Function CreateAxisTicks(min As Double, max As Double,
                                    Optional ticks As Integer = 10,
                                    Optional decimalDigits As Integer = 2) As Double()

        If min = max AndAlso min + max <> 0 Then
            Return {0, max}
        ElseIf min = max Then
            Return {0, 1}
        ElseIf Double.IsNaN(min) OrElse Double.IsNaN(max) OrElse
            Double.IsInfinity(min) OrElse Double.IsInfinity(max) Then
            Return {0, 1}
        End If

        Dim range As Double = max - min
        Dim zeroFlag As Boolean = min <= 0 AndAlso max >= 0
        Dim [step] As Double = NiceStep(range, ticks)
        Dim list As New List(Of Double)
        Dim v As Double

        If zeroFlag Then
            v = std.Ceiling(std.Abs(min) / [step]) * -[step]
        Else
            v = std.Floor(min / [step]) * [step]
        End If

        Dim digits As Integer = If(decimalDigits < 0, 15, decimalDigits)

        Do While v <= max + [step] * 0.001
            Call list.Add(std.Round(v, digits))
            v += [step]
        Loop

        If list.Count = 0 Then
            Call list.Add(std.Round(min, digits))
            Call list.Add(std.Round(max, digits))
        End If

        Return list.ToArray
    End Function

    ''' <summary>
    ''' 按固定的刻度步长生成坐标轴序列（从旧 Plots 项目的 AxisScalling.GetAxisByTick 迁移）
    ''' </summary>
    <Extension>
    Public Function GetAxisByTick(range As DoubleRange, tick As Double) As Double()
        Return GetAxisByTick(range.Max, tick, range.Min).ToArray
    End Function

    Public Function GetAxisByTick(max As Double, tick As Double, Optional min As Double = 0R) As List(Of Double)
        Dim l As New List(Of Double)
        Dim i As Double = min

        If tick = 0R Then
            Throw New ArgumentException($"Tick can not be ZERO! min={min}, max={max}")
        End If

        Do Until i >= max
            Call l.Add(i)
            i += tick
        Loop

        Call l.Add(max)

        Return l
    End Function

    Private Function NiceStep(range As Double, tickCount As Integer) As Double
        If range <= 0 Then Return 1

        Dim rough As Double = range / std.Max(tickCount - 1, 1)
        Dim pow As Double = std.Pow(10, std.Floor(std.Log10(rough)))
        Dim norm As Double = rough / pow
        Dim [step] As Double

        If norm < 1.5 Then
            [step] = 1
        ElseIf norm < 3.5 Then
            [step] = 2
        ElseIf norm < 7.5 Then
            [step] = 5
        Else
            [step] = 10
        End If

        Return [step] * pow
    End Function
End Module