#Region "Microsoft.VisualBasic::ae34f4ddfed9d794e797806ef278642d, Data_science\Mathematica\Math\Math\Distributions\Bootstraping.vb"

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


    ' Code Statistics:

    '   Total Lines: 271
    '    Code Lines: 173
    ' Comment Lines: 70
    '   Blank Lines: 28
    '     File Size: 11.49 KB


    '     Module Bootstraping
    ' 
    '         Function: Distributes, GetBagSample, Hist, (+2 Overloads) Sample, (+2 Overloads) Samples
    '                   Sampling, TabulateBin, TabulateMode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports stdNum = System.Math
Imports randf2 = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Distributions

    ''' <summary>
    ''' Data sampling bootstrapping extensions
    ''' </summary>
    Public Module Bootstraping

        ''' <summary>
        ''' Generate a numeric <see cref="Vector"/> by <see cref="Permutation"/> <paramref name="x"/> times.
        ''' </summary>
        ''' <param name="x%"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Sample(x As Integer) As Integer()
            SyncLock seeds
                Return seeds.Permutation(x, x)
            End SyncLock
        End Function

        ''' <summary>
        ''' bootstrap是一种非参数估计方法，它用到蒙特卡洛方法。bootstrap算法如下：
        ''' 假设样本容量为N
        '''
        ''' + 有放回的从样本中随机抽取N次(所以可能x1..xn中有的值会被抽取多次)，每次抽取一个元素。并将抽到的元素放到集合S中；
        ''' + 重复**步骤1** B次（例如``B = 100``）， 得到B个集合， 记作S1, S2,…, SB;
        ''' + 对每个``Si(i=1, 2, ..., B)``，用蒙特卡洛方法估计随机变量的数字特征d，分别记作d1,d2,…,dB;
        ''' + 用d1,d2,…dB来近似d的分布；
        ''' 
        ''' 本质上，bootstrap算法是最大似然估计的一种实现，它和最大似然估计相比的优点在于，它不需要用参数来刻画总体分布。
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source">总体样本</param>
        ''' <param name="N">每一个样本的大小，即在每一个袋子中有多少个样本元素</param>
        ''' <param name="bags">采样的次数，这个函数返回多少个袋子集合？</param>
        ''' <param name="replace">
        ''' 是否为有放回的进行抽样？默认是有放回的。设置这个参数为False表示不重复的采样，即抽取过后的元素将不会再出现在后面的采样结果之中
        ''' </param>
        ''' <returns>
        ''' index of the returns value <see cref="SeqValue(Of T()).i"/> is zero based.
        ''' </returns>
        ''' <remarks>
        ''' this method can be affected by the <see cref="randf2.SetSeed(Integer)"/> method.
        ''' </remarks>
        <Extension>
        Public Iterator Function Samples(Of T)(source As IEnumerable(Of T), N As Integer,
                                               Optional bags As Integer = 100,
                                               Optional replace As Boolean = True) As IEnumerable(Of SeqValue(Of T()))

            Dim array As T() = source.ToArray
            Dim index As New List(Of Integer)(array.Sequence)

            For i As Integer = 0 To bags
                Call randf2.seeds.Next()

                Yield New SeqValue(Of T()) With {
                    .i = i,
                    .value = array _
                        .GetBagSample(N, New List(Of Integer)(index), replace) _
                        .ToArray
                }
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Sample(Of T)(source As IEnumerable(Of T), N As Integer, Optional replace As Boolean = True) As T()
            Return source.Samples(N, bags:=1, replace:=replace).First.value
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pool"></param>
        ''' <param name="N"></param>
        ''' <param name="index"></param>
        ''' <param name="replace">
        ''' if replace, then each bag sample may contains duplicated element
        ''' else, each of the data element in one bag sample is unique.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this method can be affected by the <see cref="randf2.SetSeed(Integer)"/> method.
        ''' </remarks>
        <Extension>
        Private Iterator Function GetBagSample(Of T)(pool As T(), N As Integer, index As List(Of Integer), replace As Boolean) As IEnumerable(Of T)
            If replace Then
                For k As Integer = 0 To N - 1
                    ' 在这里是有放回的随机采样
                    Yield pool(randf2.seeds.Next(pool.Length))
                Next
            ElseIf index.Count = 0 Then
                Return
            Else
                Dim i As Integer = Scan0

                ' 无放回的抽样
                For k As Integer = 0 To N - 1
                    i = randf2.seeds.Next(index.Count)
                    i = index(i)
                    index.Remove(item:=i)

                    Yield pool(i)

                    If index.Count = 0 Then
                        Return
                    End If
                Next
            End If
        End Function

        <Extension>
        Public Iterator Function Sampling(source As IEnumerable(Of Double), N%, Optional B% = 100) As IEnumerable(Of IntegerTagged(Of Vector))
            For Each x As SeqValue(Of Double()) In Samples(source, N, B)
                Yield New IntegerTagged(Of Vector) With {
                    .Tag = x.i,
                    .Value = New Vector(x.value)
                }
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Samples(Of T)(source As IEnumerable(Of T), getValue As Func(Of T, Double), N%, Optional B% = 100) As IEnumerable(Of IntegerTagged(Of Vector))
            Return source.Select(getValue).Sampling(N, B)
        End Function

        ''' <summary>
        ''' ###### 频数分布表与直方图
        ''' 
        ''' 返回来的标签数据之中的标签是在某个区间范围内的数值集合的平均值
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="base"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Distributes(data As IEnumerable(Of Double), Optional base! = 10.0F) As Dictionary(Of Integer, DoubleTagged(Of Integer))
            Dim array As DoubleTagged(Of Double)() = data _
                .Select(Function(x)
                            Return New DoubleTagged(Of Double) With {
                                .Tag = stdNum.Log(x, base),
                                .Value = x
                            }
                        End Function) _
                .ToArray
            Dim min As Integer = CInt(array.Min(Function(x) x.Tag)) - 1
            Dim max As Integer = CInt(array.Max(Function(x) x.Tag)) + 1
            Dim l As i32 = min, low As Integer = min
            Dim out As New Dictionary(Of Integer, DoubleTagged(Of Integer))

            Do While ++l < max
                Dim LQuery As DoubleTagged(Of Double)() =
                    LinqAPI.Exec(Of DoubleTagged(Of Double)) <=
                                                               _
                    From x As DoubleTagged(Of Double)
                    In array
                    Where x.Tag >= low AndAlso
                        x.Tag < l
                    Select x

                out(l) = New DoubleTagged(Of Integer) With {
                    .Tag = If(LQuery.Length = 0, 0, LQuery.Average(Function(x) x.Value)),
                    .Value = LQuery.Length
                }
                low = l
            Loop

            If out(min + 1).Value = 0 Then
                Call out.Remove(min)
            End If
            If out(max - 1).Value = 0 Then
                Call out.Remove(max)
            End If

            Return out
        End Function

        ''' <summary>
        ''' ###### 频数分布表与直方图
        ''' 
        ''' 这个函数返回来的是频数以及区间内的所有的数的平均值
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="step!"></param>
        ''' <returns>
        ''' 返回来的数据为区间的下限 -> {频数, 平均值}
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Hist(data As Double(), Optional step! = 1) As IEnumerable(Of DataBinBox(Of Double))
            If data.Length = 0 Then
                Return {}
            Else
                Return CutBins.FixedWidthBins(Of Double)(
                    v:=data.OrderBy(Function(x) x).ToArray,
                    width:=[step],
                    eval:=Function(x) x,
                    min:=data.Min,
                    max:=data.Max
                )
            End If
        End Function

        <Extension>
        Public Function TabulateMode(data As IEnumerable(Of Double), Optional topBin As Boolean = False, Optional bags As Integer = 5) As Double
            Dim resample As Double() = data.TabulateBin(topBin, bags)

            If resample.Length = 0 Then
                Return Double.NaN
            Else
                Return resample.Average
            End If
        End Function

        <Extension>
        Public Function TabulateBin(data As IEnumerable(Of Double), Optional topBin As Boolean = False, Optional bags As Integer = 5) As Double()
            With data.ToArray
                If .Length = 0 Then
                    Return {}
                ElseIf .All(AddressOf IsNaNImaginary) Then
                    Return {}
                ElseIf .Min = .Max Then
                    ' all equals to each other, no needs for calculation
                    Return .ByRef
                End If

                Dim steps As Double = New DoubleRange(.Min, .Max).Length / bags

                If steps < 0.000001 Then
                    Return .ByRef
                End If

                Dim hist = .Hist([step]:=steps).ToArray
                Dim maxN = which.Max(hist.Select(Function(bin) bin.Count))
                Dim resample As Double()

                If topBin Then
                    resample = hist(maxN).Raw
                Else
                    If maxN = 0 Then
                        resample = hist(Scan0).Raw.AsList + hist(1).Raw
                    ElseIf maxN = hist.Length - 1 Then
                        resample =
                            hist(hist.Length - 1).Raw.AsList +
                            hist(hist.Length - 2).Raw
                    Else
                        resample =
                            hist(maxN - 1).Raw.AsList +
                            hist(maxN).Raw +
                            hist(maxN + 1).Raw
                    End If
                End If

                Return resample
            End With
        End Function
    End Module
End Namespace
