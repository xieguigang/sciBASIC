#Region "Microsoft.VisualBasic::365ebc0a4b334d6af889b143a8067968, Data_science\Visualization\Plots\g\Axis\AxisScalling.vb"

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

    '     Module AxisScalling
    ' 
    '         Function: __fix, __max, AxisExpression, (+3 Overloads) CreateAxisTicks, (+2 Overloads) GetAxisByTick
    '                   (+2 Overloads) GetAxisValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

Namespace Graphic.Axis

    Public Module AxisScalling

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CreateAxisTicks(data As IEnumerable(Of Double), Optional ticks% = 10, Optional decimalDigits% = 2) As Double()
            Return data.Range.CreateAxisTicks(ticks, decimalDigits)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CreateAxisTicks(range As DoubleRange,
                                        Optional ticks% = 10,
                                        Optional decimalDigits% = 2,
                                        Optional w_steps# = 0.8,
                                        Optional w_min# = 0.1,
                                        Optional w_max# = 0.1) As Double()
            With range
                If .Min.IsNaNImaginary AndAlso .Max.IsNaNImaginary Then
                    Return {0, 1}
                Else
                    Return AxisScalling.CreateAxisTicks(.Min, .Max, ticks, decimalDigits, w_steps, w_max, w_min)
                End If
            End With
        End Function

        ''' <summary>
        ''' <see cref="AxisProvider"/>
        ''' </summary>
        ''' <param name="ticks#"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AxisExpression(ticks#()) As String
            Return $"({ticks.Min},{ticks.Max}),n={ticks.Length}"
        End Function

        ''' <summary>
        ''' ### An Algorithm for Creating and Selecting Graph Axes
        ''' > http://austinclemens.com/blog/2016/01/09/an-algorithm-for-creating-a-graphs-axes/
        ''' </summary>
        ''' <param name="ticks%"></param>
        ''' <param name="decimalDigits%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateAxisTicks(min#, max#,
                                        Optional ticks% = 10,
                                        Optional decimalDigits% = 2,
                                        Optional w_steps As Double = 0.8,
                                        Optional w_max As Double = 0.1,
                                        Optional w_min As Double = 0.1) As Double()

            ' First, get the minimum and maximum of the series, toggle the zero_flag variable 
            ' if 0 Is between Then the min And max, And Get the range Of the data.
            Dim zeroFlag As Boolean = False
            Dim range = max - min
            Dim inputRange As New DoubleRange(min, max)

            If min = max AndAlso min + max <> 0 Then
                Return {0, max}
            End If

            If range = 0R Then
                Return {}
            End If
            If (min <= 0 AndAlso max >= 0) Then
                zeroFlag = True
            End If

            ' Next, define ‘nice’ numbers. You could change this if you’d like to include other 
            ' possibilities, but I decided I would allow counting by 
            ' 1s, 2s, 5s, 10s, 15s, 25s, and 75s. This will make a bit more sense below.
            Dim niceTicks#() = {0.1, 0.2, 0.5, 1, 0.15, 0.25, 0.75}

            ' This next part is a bit of path dependence – I had an algorithm where the number of 
            ' ticks was more central and I kept that framework but I probably wouldn’t do it this 
            ' way again. I get a naive value for the distance between ticks and determine the place 
            ' value of this distance.
            Dim steps = range / (ticks - 1)
            Dim rounded As Double
            Dim digits As Integer

            If (steps >= 1) Then
                rounded = stdNum.Round(steps)
                digits = rounded.ToString().Length
            Else
                Dim places = steps.ToString().Split("."c)(1)
                Dim firstPlace% = 0

                For i = 0 To places.Length - 1
                    If (places(i) <> "0" AndAlso firstPlace = 0) Then
                        firstPlace = i
                        Exit For
                    End If
                Next

                digits = -firstPlace
            End If

            ' Now using the value of digits (the place value of steps), generate a list of candidate steps. 
            ' These are just the values from nice_steps above multiplied by powers of 10 according to the 
            ' place value of digits. Because computation doesn’t matter to me, 
            ' I check 10^place value+1, 10^place value, and 10^place value-1. Most of these candidate step 
            ' lengths will be terrible but it doesn’t matter – they will get weeded out in the next step. 
            ' If our initial step length was 13, candidate steps will be generated by taking 1, 10 and 100 * all values 
            ' of nice_ticks. So 13 would result in candidate steps: 
            ' [.1,.2,.5,1,.15,.25,.75,1,2,5,10,2.5,2.5,7.5,20,50,100,15,25,75]
            Dim candidateSteps As New List(Of Double)

            For i As Integer = 0 To niceTicks.Length - 1
                candidateSteps.Add(niceTicks(i) * stdNum.Pow(10, digits))
                candidateSteps.Add(niceTicks(i) * stdNum.Pow(10, digits - 1))
                candidateSteps.Add(niceTicks(i) * stdNum.Pow(10, digits + 1))
            Next

            Dim minSteps As Double
            Dim stepArray As New List(Of Double)
            Dim candidateArray As New List(Of Double())

            ' Loop through candidate steps and generate an axis based on each step length.
            For i As Integer = 0 To candidateSteps.Count - 1
                steps = candidateSteps(i)

                ' starting value depends on whether Or Not 0 Is in the array
                If (zeroFlag) Then
                    minSteps = stdNum.Ceiling(stdNum.Abs(min) / steps)
                    stepArray = {-minSteps * steps}.AsList
                Else
                    stepArray = {stdNum.Floor(min / steps) * steps}.AsList
                End If

                Dim stepnum% = 1

                Do While (stepArray(stepArray.Count - 1) < max)
                    stepArray.Add((stepArray(0) + steps * stepnum))
                    stepnum += 1
                Loop

                ' this arbitrarily enforces step_arrays of length between 4 And 10

                ' 2017-9-12 假若在这里直接使用个数来限制最终的结果的话，很可能会出现candidateArray为空的情况
                ' 所以为了避免出现这个问题，在这里就不进行限制了，直接添加结果到候选的数据集之中
                ' If (stepArray.Count < 11 AndAlso stepArray.Count > 4) Then

                ' All that remains is to score all the candidate arrays. 
                ' I’m not going to include my scorer, because there are a 
                ' lot of arbitrary choices involved, but basically I look at 
                ' how much space each array wastes compared to the data use 
                ' that as a starting value. Each array gets the score 10^percent 
                ' wasted space – then I further penalize the array for large 
                ' values of ticks, tick values that I don’t like as much 
                ' (.15 for example, is great in certain cases, but probably 
                ' shouldn’t be liked as much by the function as .1). 
                ' The array with the lowest score ‘wins’.
                candidateArray += stepArray.ToArray
                ' End If
            Next

            ' 通过分别计算ticks的数量差值，是否容纳了输入的[min,max]范围来判断是否合适
            Dim maxSteps = candidateArray.Max(Function(candidate) candidate.Length)
            Dim dSteps = maxSteps - candidateArray.Select(Function(candidate) stdNum.Abs(candidate.Length - ticks)).AsVector
            Dim dMin = inputRange.Length - candidateArray.Select(Function(candidate) stdNum.Abs(candidate.Min - inputRange.Min)).AsVector
            Dim dMax = inputRange.Length - candidateArray.Select(Function(candidate) stdNum.Abs(candidate.Max - inputRange.Max)).AsVector

            dSteps = dSteps / dSteps.Max
            dMin = dMin / dMin.Max
            dMax = dMax / dMax.Max

            Dim scores As Vector = dSteps * w_steps + dMin * w_min + dMax * w_max
            Dim tickArray#() = candidateArray(Which.Max(scores))

            ' 2018-2-1
            ' 如果数值是 1E-10 这样子的小数的话，在这里直接使用Round或导致返回的ticks全部都是零的bugs
            ' 在这里加个开关，如果小于零就不在进行round了
            If decimalDigits >= 0 Then
                For i As Integer = 0 To tickArray.Length - 1
                    tickArray(i) = stdNum.Round(tickArray(i), decimalDigits)
                Next
            End If

            Return tickArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="max#"></param>
        ''' <param name="parts%"></param>
        ''' <param name="min#"></param>
        ''' <param name="absoluteScalling">
        ''' 这个主要是相对于<paramref name="min"/>的计算而言的，当这个不为真的视乎，scalling函数会将较小的min值直接设置为0，反之不会
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' + 0-10
        ''' + 0-100
        ''' + 0-1000
        ''' + 0-1E30
        ''' + 0-1E-30
        ''' + 0-1
        ''' + 0-0.1
        ''' </remarks>
        Public Function GetAxisValues(max#, Optional parts% = 10, Optional min# = 0R, Optional t# = 5 / 6, Optional decimal% = -1, Optional absoluteScalling As Boolean = False) As Double()
            Dim vmax# = __max(max, min)
            Dim vmin#

            If min < 0 Then
                vmin = -__max(stdNum.Abs(min), 0)
            Else
                vmin = If(min < t * max, If(absoluteScalling, min, 0), min - (max - min) / 20)
            End If

            Dim d = __fix(vmax, True) - __fix(vmin, False)
            Dim p = stdNum.Round(stdNum.Log10(d), 0)
            Dim tick# = 2 * ((10 ^ p) / parts)
            Dim out As List(Of Double) = GetAxisByTick(vmax, tick, vmin)

            If out(-2) > max Then
                Call out.RemoveLast
            End If

            If 0 <= [decimal] Then
                out = New List(Of Double)(out.Select(Function(x) stdNum.Round(x, [decimal])))
            End If

            Return out.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="max#">必须始终是正数</param>
        ''' <param name="min#"></param>
        ''' <returns></returns>
        Private Function __max(max#, min#) As Double
            'Dim p% = Fix(Math.Log10(max)) ' max.ToString.Split("."c).First.Length - 1
            'Dim value#
            'Dim upbound% = (CInt(Val(CStr(max.ToString.First)) + 1)) * 10 ^ p

            'If max < upbound Then
            '    value = upbound
            'Else
            '    value = max + (max - min) / 20
            'End If

            'Return value
            Return max + (max - min) / 20
        End Function

        Private Function __fix(ByRef n#, enlarge As Boolean) As Double
            If n = 0R Then
                Return n
            End If

            'If enlarge Then
            '    If n > 0 Then
            '        n += 1
            '    End If
            'Else
            '    If n < 0 Then
            '        n -= 1
            '    End If
            'End If

            Dim p% = stdNum.Round(stdNum.Log10(stdNum.Abs(n)), 0) ' Fix(Math.Log10(Math.Abs(n))) ' stdNum.Round(Math.Log10(Math.Abs(n)), 0)
            Dim d = 10 ^ (p - 1)
            Dim v#
            Dim s = stdNum.Sign(n)
            Dim l% = CInt(Val(stdNum.Abs(n).ToString.First))

            If Not enlarge Then
                p = 10 ^ (p - 1)
            Else
                p = 10 ^ p
            End If

            p *= l

            For i As Double = 0 To 10 Step 0.5
                v = s * p + s * i * d

                If enlarge Then
                    If n <= v Then
                        n = v
                        Exit For
                    End If
                Else
                    If (n > 0 AndAlso n <= v) OrElse (n < 0 AndAlso n > v) Then
                        n = v

                        'If enlarge Then
                        '    ' 由于v已经是比原先的数要大的值，所以在这里可以直接跳过了
                        '    Exit For
                        'End If

                        n -= 0.5 * d

                        Exit For
                    End If
                End If
            Next

            Return n
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="range"></param>
        ''' <param name="ticks%"></param>
        ''' <param name="absoluteScalling">
        ''' 从最小值开始还是从零开始计算？
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function GetAxisValues(range As DoubleRange, Optional ticks% = 10, Optional absoluteScalling As Boolean = False) As Double()
            ' Return GetAxisValues(range.Max, parts, range.Min, absoluteScalling:=absoluteScalling)
            If absoluteScalling Then
                Return New DoubleRange(stdNum.Min(0, range.Min), range.Max).CreateAxisTicks(ticks)
            Else
                Return range.CreateAxisTicks(ticks)
            End If
        End Function

        <Extension>
        Public Function GetAxisByTick(range As DoubleRange, tick#) As Double()
            Return GetAxisByTick(range.Max, tick, range.Min).ToArray
        End Function

        Public Function GetAxisByTick(max#, tick#, Optional min# = 0R) As List(Of Double)
            Dim l As New List(Of Double)
            Dim i# = min

            If tick = 0# Then
                Throw New InvalidExpressionException($"Tick can not be ZERO! min={min}, max={max}")
            End If

            Try
                Do Until i >= max
                    l.Add(i)
                    i += tick
                Loop

                l += max
            Catch ex As Exception
                Dim debug As New Dictionary(Of String, Double) From {
                    {NameOf(min), min},
                    {NameOf(max), max},
                    {NameOf(tick), tick}
                }
                Throw New Exception(debug.GetJson, ex)
            End Try

            Return l
        End Function
    End Module
End Namespace
