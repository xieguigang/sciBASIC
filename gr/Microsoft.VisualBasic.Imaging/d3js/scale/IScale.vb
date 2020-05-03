#Region "Microsoft.VisualBasic::01b78c11acae13f368db7b19f86f17bb, gr\Microsoft.VisualBasic.Imaging\d3js\scale\IScale.vb"

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

    '     Class IScale
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

    Public MustInherit Class IScale(Of T As IScale(Of T))

        Default Public MustOverride ReadOnly Property Value(x#) As Double
        Default Public MustOverride ReadOnly Property Value(term$) As Double

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

        ''' <summary>
        ''' If range is specified, sets the range of the ordinal scale to the specified array of values. 
        ''' The first element in the domain will be mapped to the first element in range, the second 
        ''' domain value to the second range value, and so on. If there are fewer elements in the range 
        ''' than in the domain, the scale will reuse values from the start of the range. If range is 
        ''' not specified, this method returns the current range.
        ''' (设置绘图的实际的像素区间)
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function range(Optional values As IEnumerable(Of Double) = Nothing) As T
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
