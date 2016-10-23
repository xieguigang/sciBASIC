#Region "Microsoft.VisualBasic::f9bd35570337e1480e7a3c97cb6683f8, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\Random\PreciseRandom.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Mathematical

    ''' <summary>
    ''' 主要针对的是非常小的小数（仅适用于Positive Number）
    ''' </summary>
    Public Class PreciseRandom

        ReadOnly __rnd As New Random(Now.Millisecond)
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
        Sub New(digitMin!, digitMax!)
            __digits = New DoubleRange(digitMin, digitMax + 1)  ' 假若max是1e10的话，则最高的位数是10，这时候由于计算公式的原因最多只能够到9所以在这里需要手动添加一来避免这个问题
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="from">最小的精度为<see cref="System.Double.Epsilon"/></param>
        ''' <param name="[to]"></param>
        Sub New(from#, to#)
            Call Me.New(
                CSng(If(from = 0R, 0F, Math.Log10(from))), ' 避免出现log(0)的情况
                CSng(If([to] = 0R, 0F, Math.Log10([to]))))
        End Sub

        Public Overrides Function ToString() As String
            Return __digits.GetJson
        End Function

        Public Function NextNumber() As Double
            Dim d As Integer = __rnd.NextDouble * __digits.Length + __digits.Min      ' generates the digits
            Dim digits# = 10 ^ d
            Dim r# = __rnd.NextDouble
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
