#Region "Microsoft.VisualBasic::92cd1e9d62c2894f83fd807be2e68a70, Microsoft.VisualBasic.Core\src\Extensions\Math\Random\RandomRange.vb"

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

    '   Total Lines: 122
    '    Code Lines: 95
    ' Comment Lines: 14
    '   Blank Lines: 13
    '     File Size: 5.73 KB


    '     Module RandomRange
    ' 
    '         Function: GetRandom, Testing
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports sys = System.Math

Namespace Math

    ''' <summary>
    ''' 针对负数到正数的range随机数，小数位最多精确到1E-4
    ''' </summary>
    Public Module RandomRange

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="from"></param>
        ''' <param name="[to]"></param>
        ''' <param name="INF"></param>
        ''' <param name="forceInit">
        ''' True的时候会通过牺牲性能来强制重新实例化随机数发生器来获取足够的随机
        ''' </param>
        ''' <returns></returns>
        Public Function GetRandom(from#, to#, Optional INF% = 5, Optional forceInit As Boolean = False) As IValueProvider
            Dim pf! = ScientificNotation.PowerLog10(from, INF), pt! = ScientificNotation.PowerLog10([to], INF)

            If from > 0 Then
                If [to] > 0 Then ' from 是正数，则to也必须是正数
                    If pf <> 0F Then  ' 如果from不是常数（极大数或者极小数），则整体当做极值数来看待
                        Return AddressOf New PreciseRandom(pf, CSng(sys.Log10([to]))).NextNumber
                    Else ' from 是常数  
                        If pt > 0 Then ' 同样的，当to也是极值数的时候，整体也将被当做极值数来看待
                            Return AddressOf New PreciseRandom(CSng(sys.Log10(from)), pt).NextNumber
                        Else
                            ' to 也是常数
                            Dim range As New DoubleRange(from, [to])

                            If forceInit Then
                                Return Function() New Random().NextDouble(range)  ' 想要通过牺牲性能来强制获取足够的随机
                            Else
                                Dim rnd As New Random
                                Return Function() rnd.NextDouble(range)  ' 假若二者都是常数，则返回常数随机区间
                            End If
                        End If
                    End If
                Else
                    Throw New InvalidConstraintException(
                        $"Can not creates a range as min is positive but max is negative! (from:={from}, to:={[to]})")
                End If
            Else ' from是负数
                If [to] > 0 Then ' to 是正数
                    If pf <> 0F OrElse pt <> 0F Then  ' from是极值数，则整体当做极值数来看待

                        pf = sys.Log10(sys.Abs(from))
                        pt = sys.Log10(sys.Abs([to]))

                        Dim c!() = {0F, pf}
                        Dim rf As New PreciseRandom(c.Min, c.Max)
                        c = {0F, pt}
                        Dim rt As New PreciseRandom(c.Min, c.Max)
                        Dim ppf = sys.Abs(pf) / (sys.Abs(pf) + sys.Abs(pt))

                        If forceInit Then
                            Return Function()
                                       If New Random().NextDouble < ppf Then
                                           Return -1 * rf.NextNumber
                                       Else
                                           Return rt.NextNumber
                                       End If
                                   End Function
                        Else
                            Dim rnd As New Random
                            Return Function()
                                       If rnd.NextDouble < ppf Then
                                           Return -1 * rf.NextNumber
                                       Else
                                           Return rt.NextNumber
                                       End If
                                   End Function
                        End If
                    Else
                        Dim range As New DoubleRange(from, [to])
                        If forceInit Then
                            Return Function() New Random().NextDouble(range)
                        Else
                            Dim rnd As New Random
                            Return Function() rnd.NextDouble(range)
                        End If
                    End If
                Else  ' to 同样也是负数的情况
                    If pf <> 0F OrElse pt <> 0F Then ' 两个都是极值数

                        pf = sys.Log10(sys.Abs(from))
                        pt = sys.Log10(sys.Abs([to]))

                        Dim c = {pf, pt}
                        Dim rnd As New PreciseRandom(c.Min, c.Max)   ' 由于from要小于to
                        Return Function() -1 * rnd.NextNumber
                    Else  ' from 和 to 都是负实数
                        Dim range As New DoubleRange(from, [to])
                        If forceInit Then
                            Return Function() New Random().NextDouble(range)
                        Else
                            Dim rnd As New Random
                            Return Function() rnd.NextDouble(range)
                        End If
                    End If
                End If
            End If
        End Function

        Public Function Testing(from#, to#) As Double()
            Dim rnd As IValueProvider = GetRandom(from, [to])
            Dim bufs As New List(Of Double)

            For Each i% In 1000%.Sequence
                bufs += rnd()
            Next

            Return bufs
        End Function
    End Module
End Namespace
