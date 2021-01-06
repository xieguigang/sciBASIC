#Region "Microsoft.VisualBasic::8c68ddf0bb4e89991a94bbd8809b55aa, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) GetScaler, (+2 Overloads) RangeTransform, SymmetricalRange, Union
    ' 
    '         Sub: Parser
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports r = System.Text.RegularExpressions.Regex
Imports stdNum = System.Math

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

        Const RegexpFloatRange$ = RegexpFloat & "\s*,\s*" & RegexpFloat

        ''' <summary>
        ''' + ``min -> max``
        ''' + ``min—max``
        ''' + ``min~max``
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
            ElseIf InStr(exp, "—") > 0 Then
                ' 使用的是中文的分隔符
                t = Strings.Split(exp, "—")
            ElseIf InStr(exp, "~") > 0 Then
                t = Strings.Split(exp, "~")
            ElseIf exp.IsPattern(RegexpDouble & "\s*[-]\s*" & RegexpDouble) Then
                ' 使用的是英文的分隔符
                ' 因为可能会和负号弄混，所以在这里需要使用正则表达式来匹配出这个分隔符
                ' 因为是这种格式的range： dd-dd
                ' 故而分隔符的pattern肯定是数字加连接符本身，将这个pattern匹配出来，然后利用这个pattern进行分割即可
                Dim del$ = r.Match(exp, "\d\s*[-]").Value
                t = Strings.Split(exp, del)
                ' 需要将存在于del的pattern之中的前面的数字的最后一个数值补回来
                t(0) = t(0) & del.Trim("-"c)
            Else
                exp = r _
                    .Match(exp, RegexpFloatRange, RegexOptions.Singleline) _
                    .Value

                If String.IsNullOrEmpty(exp) Then
                    exp = $"'{raw}' is not a valid expression format!"
                    Throw New FormatException(exp)
                Else
                    t = exp.Split(","c)
                End If
            End If

            t = t _
                .Select(AddressOf Trim) _
                .ToArray

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
            If from Is Nothing Then
                Return {}
            End If

            Dim vector#() = from.ToArray

            If vector.Length = 1 Then
                Return {[to].Max}
            End If

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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function RangeTransform(from As IEnumerable(Of Integer), [to] As IntRange) As Integer()
            Return from _
                   .Select(Function(x) CDbl(x)) _
                   .RangeTransform(New DoubleRange([to])) _
                   .Select(Function(x) CInt(x)) _
                   .ToArray
        End Function

        <Extension>
        Public Function Union(fragments As IEnumerable(Of IntRange)) As IEnumerable(Of IntRange)
            Dim unions As New List(Of IntRange)

            For Each f In fragments.OrderBy(Function(r) r.Min)
                If unions = 0 Then
                    unions += New IntRange(f.Min, f.Max)
                Else
                    Dim isUnion As Boolean = False

                    For Each region In unions
                        If region.IsOverlapping(f) OrElse region.IsInside(f) Then
                            region.Min = stdNum.Min(region.Min, f.Min)
                            region.Max = stdNum.Max(region.Max, f.Max)

                            isUnion = True
                        End If
                    Next

                    If Not isUnion Then
                        unions += New IntRange(f.Min, f.Max)
                    End If
                End If
            Next

            Return unions
        End Function
    End Module
End Namespace
