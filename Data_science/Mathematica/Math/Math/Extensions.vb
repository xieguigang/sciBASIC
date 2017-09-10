#Region "Microsoft.VisualBasic::c13cabceef670e31d0caf5b4d0b4feed, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports sys = System.Math

''' <summary>
''' 向量以及统计函数拓展
''' </summary>
Public Module Extensions

    ''' <summary>
    ''' 计算两个离散信号之间的相似度
    ''' </summary>
    ''' <param name="q"></param>
    ''' <param name="s"></param>
    ''' <returns></returns>
    Public Function SSM(q As Vector, s As Vector) As Double
        Return Sum(q * s) / Sqrt(Sum(q ^ 2) * Sum(s ^ 2))
    End Function

    <Extension>
    Public Function AsVector(data As IEnumerable(Of Double)) As Vector
        Return New Vector(data)
    End Function

    ''' <summary>
    ''' ``FDR = length(pvalue)*pvalue/rank(pvalue)``
    ''' </summary>
    ''' <param name="pvalue"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FDR(pvalue As IEnumerable(Of Double)) As Vector
        Dim x As New Vector(pvalue)
        Dim fdr_result = (x.Dim * x) / x.FractionalRanking
        Return fdr_result
    End Function

    ''' <summary>
    ''' Tuple range iterates
    ''' </summary>
    ''' <param name="range">Number values iterates from value ``from`` to value ``to``.</param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function Iterates(range As (From%, To%)) As IEnumerable(Of Integer)
        Dim step% = sys.Sign(range.To - range.From)

        For i As Integer = range.From To range.To Step [step]
            Yield i
        Next
    End Function

    <Extension>
    Public Function Range(data As IEnumerable(Of Double)) As (min#, max#)
        With data.ToArray
            Return (.Min, .Max)
        End With
    End Function

    <Extension>
    Public Function Range(data As IEnumerable(Of Integer)) As (min#, max#)
        With data.ToArray
            Return (.Min, .Max)
        End With
    End Function

    <Extension>
    Public Function IntRange(range As (From%, To%)) As IntRange
        With range
            Return New IntRange(.From, .To)
        End With
    End Function

    <Extension>
    Public Function DoubleRange(range As (From#, To#)) As DoubleRange
        With range
            Return New DoubleRange(.From, .To)
        End With
    End Function

    ''' <summary>
    ''' 返回数值序列之中的首次出现符合条件的减少的位置
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="ratio"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FirstDecrease(data As IEnumerable(Of Double), Optional ratio As Double = 10) As Integer
        Dim pre As Double = data.First
        Dim pr As Double = 1000000

        For Each x In data.SeqIterator
            Dim d = (pre - x.value)

            If d / pr > ratio Then
                Return x.i
            Else
                pr = d
                pre = x.value
            End If
        Next

        Return -1 ' 没有找到符合条件的点
    End Function

    ''' <summary>
    ''' 只对单调递增的那一部分曲线有效
    ''' </summary>
    ''' <param name="data">y值</param>
    ''' <param name="alpha"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FirstIncrease(data As IEnumerable(Of Double), dx As Double, Optional alpha As Double = 30) As Integer
        Dim pre As Double = data.First
        Dim pr As Double = 1000000

        For Each x In data.Skip(1).SeqIterator(offset:=1)
            Dim dy = (x.value - pre) ' 对边
            Dim tanX As Double = dy / dx
            Dim a As Double = Atn(tanX)

            If a >= alpha Then
                Return x.i
            End If
        Next

        Return -1 ' 没有找到符合条件的点
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="n"></param>
    ''' <param name="offset">距离目标数据点<paramref name="n"/>的正负偏移量</param>
    ''' <returns></returns>
    <Extension>
    Public Function Reach(data As IEnumerable(Of Double), n As Double, Optional offset As Double = 0) As Integer
        For Each x In data.SeqIterator
            If sys.Abs(x.value - n) <= offset Then
                Return x.i
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' [Sequence Generation] Generate regular sequences. seq is a standard generic with a default method.
    ''' </summary>
    ''' <param name="From">
    ''' the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.
    ''' </param>
    ''' <param name="To">
    ''' the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.
    ''' </param>
    ''' <param name="By">number: increment of the sequence</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function seq2(from#, to#, Optional by# = 0.1) As Vector
        Return New Vector(seq(from, [to], by))
    End Function

    ''' <summary>
    ''' 余弦相似度
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Sim(x As Vector, y As Vector) As Double
        Return (x * y).Sum / (x.Mod * y.Mod)
    End Function

    ''' <summary>
    ''' 这是x和y所共有的属性个数与x或y所具有的属性个数之间的比率。这个函数被称为Tanimoto系数或Tanimoto距离，
    ''' 它经常用在信息检索和生物学分类中。(余弦度量的一个简单的变种)
    ''' 当属性是二值属性时，余弦相似性函数可以用共享特征或属性解释。假设如果xi=1，则对象x具有第i个属性。于是，
    ''' x·y是x和y共同具有的属性数，而xy是x具有的属性数与y具有的属性数的几何均值。于是，sim(x,y)是公共属性相
    ''' 对拥有的一种度量。
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' http://xiao5461.blog.163.com/blog/static/22754562201211237567238/
    ''' </remarks>
    <Extension>
    Public Function Tanimoto(x As Vector, y As Vector) As Double
        Return (x * y).Sum / ((x * x).Sum + (y * y).Sum - (x * y).Sum)
    End Function
End Module
