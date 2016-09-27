Namespace Mathematical

    ''' <summary>
    ''' 针对负数到正数的range随机数
    ''' </summary>
    Public Class RandomRange

        ReadOnly __sign As Random

        ReadOnly negative As PreciseRandom
        ReadOnly positive As PreciseRandom

        ''' <summary>
        ''' from 和 to都是负数？？？
        ''' </summary>
        Dim negativeRanges As Boolean = False
        Dim negativeParts As Boolean = False

        Sub New(from As Double, [to] As Double, Optional ZERO As Integer = -50)
            If from < 0 Then
                If [to] < 0 Then
                    ' 负数比较大，绝对值会较小，所以to被放在from的位置了
                    positive = New PreciseRandom(Math.Abs([to]), Math.Abs(from))
                    negativeRanges = True
                Else
                    from = Math.Abs(from)
                    negative = New PreciseRandom(ZERO, Math.Log10(from))
                    positive = New PreciseRandom(ZERO, Math.Log10([to]))
                    __sign = New Random
                    negativeParts = True
                End If
            Else
                ' 两个都是正整数
                positive = New PreciseRandom(from, [to])
            End If
        End Sub

        Public Function NextNumber() As Double
            If negativeRanges Then  ' 两个都是负数，则只去一个值然后乘以-1即可
                Return positive.NextNumber * -1
            ElseIf negativeParts Then  ' 只有一边是负值，则有百分之50的概率会出现负值
                If __sign.NextDouble >= 0.5 Then
                    Return -1 * negative.NextNumber
                Else
                    Return positive.NextNumber
                End If
            Else
                Return positive.NextNumber
            End If
        End Function

        Public Function GetHandle() As INextRandomNumber
            Return AddressOf NextNumber
        End Function
    End Class
End Namespace