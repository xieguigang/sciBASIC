#Region "Microsoft.VisualBasic::ccad36780681f41eafaab177d6e947fd, Microsoft.VisualBasic.Core\src\Extensions\Math\Random\PreciseRandom.vb"

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

    '   Total Lines: 81
    '    Code Lines: 41
    ' Comment Lines: 29
    '   Blank Lines: 11
    '     File Size: 3.06 KB


    '     Class PreciseRandom
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: NextDouble, NextNumber, rand, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Serialization.JSON
Imports sys = System.Math

Namespace Math

    ''' <summary>
    ''' 主要针对的是非常小的小数（仅适用于Positive Number）
    ''' </summary>
    Public Class PreciseRandom

        ReadOnly __rnd As Random
        ReadOnly __digits As DoubleRange

        ''' <summary>
        ''' 4.94065645841247E-324
        ''' </summary>
        Public Const Epsilon As Double = Double.Epsilon

        ''' <summary>
        ''' 最小的精度为``<see cref="System.Double.Epsilon"/>=4.94065645841247E-324``
        ''' </summary>
        ''' <param name="digitMin">``10^?``</param>
        ''' <param name="digitMax">``10^?``</param>
        Sub New(digitMin!, digitMax!, Optional seeds As IRandomSeeds = Nothing)
            ' 假若max是1e10的话，则最高的位数是10，
            ' 这时候由于计算公式的原因最多只能够到9所以在这里需要手动添加一来避免这个问题
            __digits = New DoubleRange(digitMin, digitMax + 1)

            If seeds Is Nothing Then
                __rnd = New Random
            Else
                __rnd = seeds()
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="from">最小的精度为<see cref="System.Double.Epsilon"/></param>
        ''' <param name="[to]"></param>
        Sub New(from#, to#, Optional seeds As IRandomSeeds = Nothing)
            Call Me.New(
                CSng(If(from = 0R, 0F, sys.Log10(from))), ' 避免出现log(0)的情况
                CSng(If([to] = 0R, 0F, sys.Log10([to]))),
                seeds)
        End Sub

        Public Overrides Function ToString() As String
            Return __digits.GetJson & " --> " & NextNumber()
        End Function

        Private Function rand() As Double
            SyncLock __rnd
                ' 线程不安全，所以需要加锁，不然无法得到随机数
                ' 因为多线程的时候不加锁在不同的线程之间同时调用会得到相同的数
                Return __rnd.NextDouble
            End SyncLock
        End Function

        ''' <summary>
        ''' 获取一个在给定的小数位范围内的随机的数
        ''' </summary>
        ''' <returns></returns>
        Public Function NextNumber() As Double
            Dim d% = rand() * __digits.Length + __digits.Min      ' generates the digits
            Dim digits# = 10 ^ d
            Dim r# = rand()
            Return r * digits
        End Function

        ''' <summary>
        ''' 这个方法可能只适用于很小的数，例如1e-100到1e-10这样子的
        ''' </summary>
        ''' <param name="range"></param>
        ''' <returns></returns>
        Public Function NextDouble(range As DoubleRange) As Double
            Return range.Min + range.Length * NextNumber()
        End Function
    End Class
End Namespace
