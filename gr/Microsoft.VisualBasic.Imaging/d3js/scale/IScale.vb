#Region "Microsoft.VisualBasic::fefaaca1a8066ba0fc93d76f473c8efd, gr\Microsoft.VisualBasic.Imaging\d3js\scale\IScale.vb"

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

    '   Total Lines: 138
    '    Code Lines: 61
    ' Comment Lines: 58
    '   Blank Lines: 19
    '     File Size: 5.35 KB


    '     Class Scaler
    ' 
    ' 
    ' 
    '     Class IScale
    ' 
    '         Properties: rangeMax, rangeMin
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+3 Overloads) range
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default

Namespace d3js.scale

    ''' <summary>
    ''' data scaler and transform
    ''' </summary>
    Public MustInherit Class Scaler

        Public MustOverride ReadOnly Property type As scalers

        ''' <summary>
        ''' value transform
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <returns>
        ''' pixel value in plot range
        ''' </returns>
        Default Public MustOverride ReadOnly Property Value(x#) As Double
        ''' <summary>
        ''' term value transform
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <returns>
        ''' pixel value in plot range
        ''' </returns>
        Default Public MustOverride ReadOnly Property Value(term$) As Double

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="vector">
        ''' vector should be a string array or float64 numeric array
        ''' </param>
        ''' <returns></returns>
        Default Public Overridable ReadOnly Property Value(vector As Array) As Double()
            Get
                If TypeOf vector Is String() Then
                    Return DirectCast(vector, String()) _
                        .Select(Function(t) Me(term:=t)) _
                        .ToArray
                Else
                    Return DirectCast(vector, Double()) _
                        .Select(Function(xi) Me(x:=xi)) _
                        .ToArray
                End If
            End Get
        End Property

        ''' <summary>
        ''' 返回用户作图数据为零的时候的绘图位置映射结果数据
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Zero As Double

        ''' <summary>
        ''' 作图的用户数据的区间长度
        ''' </summary>
        ''' <returns>
        ''' + 对于<see cref="LinearScale"/>这个属性值为浮点数
        ''' + 对于<see cref="OrdinalScale"/>这个属性值为整形数
        ''' </returns>
        Public MustOverride ReadOnly Property domainSize As Double

        Public MustOverride ReadOnly Property rangeMax As Double
        Public MustOverride ReadOnly Property rangeMin As Double

    End Class

    Public MustInherit Class IScale(Of T As IScale(Of T)) : Inherits Scaler

        Public MustOverride Function domain(values As IEnumerable(Of Double)) As T
        Public MustOverride Function domain(values As IEnumerable(Of String)) As T
        Public MustOverride Function domain(values As IEnumerable(Of Integer)) As T

        Shared ReadOnly defaultRange As [Default](Of  IEnumerable(Of Double))

        Shared Sub New()
            defaultRange = DirectCast({0#, 1.0#}, IEnumerable(Of Double)).AsDefault
        End Sub

        ''' <summary>
        ''' 绘图的时候的实际的像素区间
        ''' </summary>
        Protected _range As DoubleRange = defaultRange.DefaultValue.Range

        Public Overrides ReadOnly Property rangeMax As Double
            Get
                Return _range.Max
            End Get
        End Property

        Public Overrides ReadOnly Property rangeMin As Double
            Get
                Return _range.Min
            End Get
        End Property

        ''' <summary>
        ''' If range is specified, sets the range of the ordinal scale to the specified array of values. 
        ''' The first element in the domain will be mapped to the first element in range, the second 
        ''' domain value to the second range value, and so on. If there are fewer elements in the range 
        ''' than in the domain, the scale will reuse values from the start of the range. If range is 
        ''' not specified, this method returns the current range.
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (设置绘图的实际的像素区间)
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function range(Optional values As IEnumerable(Of Double) = Nothing) As T
            _range = (values Or defaultRange).Range
            Return Me
        End Function

        ''' <summary>
        ''' 输入绘图的坐标轴在画布上面的X/Y值的范围(设置绘图的实际的像素区间)
        ''' </summary>
        ''' <param name="integers"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function range(Optional integers As IEnumerable(Of Integer) = Nothing) As T
            Return range(integers.Select(Function(x) CDbl(x)))
        End Function

        ''' <summary>
        ''' (设置绘图的实际的像素区间)
        ''' </summary>
        ''' <param name="singles"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function range(Optional singles As IEnumerable(Of Single) = Nothing) As T
            Return range(singles.Select(Function(x) CDbl(x)))
        End Function
    End Class
End Namespace
