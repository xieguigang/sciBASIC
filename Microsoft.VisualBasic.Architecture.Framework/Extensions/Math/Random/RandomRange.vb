Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Linq

Namespace Mathematical

    ''' <summary>
    ''' 针对负数到正数的range随机数，小数位最多精确到1E-4
    ''' </summary>
    Public Module RandomRange

        ''' <summary>
        ''' 返回零表示比较小的常数
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="INF"></param>
        ''' <returns></returns>
        Public Function Log(x As Double, Optional INF As Integer = 5) As Single
            Dim p As Double = Math.Log10(Math.Abs(x))

            If p < -4 Then
                Return p
            End If
            If p > 5 Then
                Return p
            End If

            Return 0
        End Function

        Public Function GetRandom(from As Double, [to] As Double, Optional INF As Integer = 5) As INextRandomNumber
            Dim pf As Single = Log(from, INF)
            Dim pt As Single = Log([to], INF)

            If from > 0 Then
                If [to] > 0 Then ' from 是正数，则to也必须是正数
                    If pf <> 0F Then  ' 如果from不是常数（极大数或者极小数），则整体当做极值数来看待
                        Return AddressOf New PreciseRandom(pf, CSng(Math.Log10([to]))).NextNumber
                    Else ' from 是常数  
                        If pt > 0 Then ' 同样的，当to也是极值数的时候，整体也将被当做极值数来看待
                            Return AddressOf New PreciseRandom(CSng(Math.Log10(from)), pt).NextNumber
                        Else
                            ' to 也是常数
                            Dim rnd As New Random
                            Dim range As New DoubleRange(from, [to])

                            Return Function() rnd.NextDouble(range)  ' 假若二者都是常数，则返回常数随机区间
                        End If
                    End If
                Else
                    Throw New InvalidConstraintException(
                        $"Can not creates a range as min is positive but max is negative! (from:={from}, to:={[to]})")
                End If
            Else ' from是负数
                If [to] > 0 Then ' to 是正数
                    If pf <> 0F OrElse pt <> 0F Then  ' from是极值数，则整体当做极值数来看待

                        pf = Math.Log10(Math.Abs(from))
                        pt = Math.Log10(Math.Abs([to]))

                        Dim c As Single() = {0F, pf}
                        Dim rf As New PreciseRandom(c.Min, c.Max)
                        c = {0F, pt}
                        Dim rt As New PreciseRandom(c.Min, c.Max)
                        Dim ppf = Math.Abs(pf) / (Math.Abs(pf) + Math.Abs(pt))
                        Dim rnd As New Random

                        Return Function()
                                   If rnd.NextDouble < ppf Then
                                       Return -1 * rf.NextNumber
                                   Else
                                       Return rt.NextNumber
                                   End If
                               End Function
                    Else
                        Dim range As New DoubleRange(from, [to])
                        Dim rnd As New Random
                        Return Function() rnd.NextDouble(range)
                    End If
                Else  ' to 同样也是负数的情况
                    If pf <> 0F OrElse pt <> 0F Then ' 两个都是极值数

                        pf = Math.Log10(Math.Abs(from))
                        pt = Math.Log10(Math.Abs([to]))

                        Dim c = {pf, pt}
                        Dim rnd As New PreciseRandom(c.Min, c.Max)   ' 由于from要小于to
                        Return Function() -1 * rnd.NextNumber
                    Else  ' from 和 to 都是负实数
                        Dim range As New DoubleRange(from, [to])
                        Dim rnd As New Random
                        Return Function() rnd.NextDouble(range)
                    End If
                End If
            End If
        End Function

        Public Function Testing(from As Double, [to] As String) As Double()
            Dim rnd As INextRandomNumber = GetRandom(from, [to])
            Dim bufs As New List(Of Double)

            For Each i As Integer In 1000.Sequence
                bufs += rnd()
            Next

            Return bufs
        End Function
    End Module
End Namespace