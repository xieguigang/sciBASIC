#Region "Microsoft.VisualBasic::a1d630ebc36065be0d4cd70b8cdf131c, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Ranges\Extensions.vb"

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
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace ComponentModel.Ranges

    Public Module Extensions

        ''' <summary>
        ''' 对称的的范围，假若X为正数，那么其为max，而-x为min。假若x为负数，那么-x为max
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <returns></returns>
        <Extension>
        Public Function SymmetricalRange(x#) As DoubleRange
            If x > 0 Then
                Return {-x, x}
            Else
                Return {x, -x}
            End If
        End Function

        ''' <summary>
        ''' + ``min -> max``
        ''' + ``[min,max]``
        ''' + ``{min,max}``
        ''' + ``(min,max)``
        ''' + ``min,max``
        ''' </summary>
        ''' <param name="exp$"></param>
        ''' <param name="min#"></param>
        ''' <param name="max#"></param>
        <Extension> Public Sub Parser(exp$, ByRef min#, ByRef max#)
            Dim t$()
            Dim raw$ = exp

            If InStr(exp, "->") > 0 Then
                t = Strings.Split(exp, "->")
            Else
                exp = Regex.Match(exp, RegexpFloat & "\s*,\s*" & RegexpFloat).Value

                If String.IsNullOrEmpty(exp) Then
                    exp = $"'{raw}' is not a valid expression format!"
                    Throw New FormatException(exp)
                Else
                    t = exp.Split(","c)
                End If
            End If

            t = t.Select(AddressOf Trim).ToArray

            min = Casting.ParseNumeric(t(Scan0))
            max = Casting.ParseNumeric(t(1))
        End Sub

        ''' <summary>
        ''' 返回一个实数区间的范围百分比的生成函数：``[0-1]``之间
        ''' </summary>
        ''' <param name="range"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetScaler(range As DoubleRange) As Func(Of Double, Double)
            Dim length# = range.Length
            Dim min# = range.Min

            Return Function(x#)
                       Return (x - min) / length
                   End Function
        End Function

        <Extension>
        Public Function GetScaler(vector As IEnumerable(Of Double)) As Func(Of Double, Double)
            With vector.ToArray
                Return New DoubleRange(.Min, .Max).GetScaler
            End With
        End Function

        ''' <summary>
        ''' 将目标区间内的任意实数全部转换为<paramref name="to"/>区间内的实数
        ''' </summary>
        ''' <param name="from"></param>
        ''' <param name="[to]"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RangeTransform(from As IEnumerable(Of Double), [to] As DoubleRange) As Double()
            If from.IsNullOrEmpty Then
                Return {}
            End If

            Dim vector#() = from.ToArray
            Dim scale = New DoubleRange(vector).GetScaler
            Dim percentages#() = vector.Select(scale).ToArray
            Dim length# = [to].Length
            Dim min# = [to].Min
            Dim maps#() = percentages.Select(Function(x) x * length + min).ToArray
            Return maps
        End Function

        ''' <summary>
        ''' 将目标区间内的任意实数全部转换为<paramref name="to"/>区间内的实数
        ''' </summary>
        ''' <param name="from"></param>
        ''' <param name="[to]"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RangeTransform(from As IEnumerable(Of Integer), [to] As IntRange) As Integer()
            Return from _
                   .Select(Function(x) CDbl(x)) _
                   .RangeTransform(New DoubleRange([to])) _
                   .Select(Function(x) CInt(x)) _
                   .ToArray
        End Function
    End Module
End Namespace
