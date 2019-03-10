#Region "Microsoft.VisualBasic::059729a988f30b0c24ca33187beba06f, gr\Microsoft.VisualBasic.Imaging\d3js\scale\linear.vb"

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

    '     Class LinearScale
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+4 Overloads) domain, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace d3js.scale

    ''' <summary>
    ''' 连续性的映射
    ''' </summary>
    Public Class LinearScale : Inherits IScale(Of LinearScale)

        Dim _domain As DoubleRange

        ''' <summary>
        ''' Constructs a new continuous scale with the unit domain [0, 1], the unit range [0, 1], 
        ''' the default interpolator and clamping disabled. Linear scales are a good default 
        ''' choice for continuous quantitative data because they preserve proportional differences. 
        ''' Each range value y can be expressed as a function of the domain value x: ``y = mx + b``.
        ''' </summary>
        Sub New()
        End Sub

        Default Public Overrides ReadOnly Property Value(x As Double) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _domain.ScaleMapping(x, _range)
            End Get
        End Property

        Default Public Overrides ReadOnly Property Value(term As String) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me(Val(term))
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{_domain.Min}, {_domain.Max}] --> [{_range.Min}, {_range.Max}]"
        End Function

        ''' <summary>
        ''' 输入的绘图数据，建议输入由原始数据所计算出来的Ticks的结果
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        Public Overrides Function domain(values As IEnumerable(Of Double)) As LinearScale
            _domain = values.ToArray
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function domain(values As IEnumerable(Of String)) As LinearScale
            Return domain(values.Select(AddressOf Val))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function domain(values As IEnumerable(Of Integer)) As LinearScale
            Return domain(values.Select(Function(x) CDbl(x)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function domain(singles As IEnumerable(Of Single)) As LinearScale
            Return domain(singles.Select(Function(x) CDbl(x)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(scale As LinearScale) As Func(Of Double, Double)
            Return Function(x#) scale(x)
        End Operator
    End Class
End Namespace
