Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Public Module NumericSequence

    <Extension>
    Public Function InRange(Of T As IComparable)(x As T, min As T, max As T) As Boolean
        If x.LessThanOrEquals(max) AndAlso x.GreaterThanOrEquals(min) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Iterator Function Range(from%, to%, Optional step% = 1) As IEnumerable(Of Integer)
        For i As Integer = from To [to] Step [step]
            Yield i
        Next
    End Function

    Public Iterator Function Range(from#, to#, Optional step# = 1) As IEnumerable(Of Double)
        For i As Double = from To [to] Step [step]
            Yield i
        Next
    End Function

    Public Iterator Function Range(from!, to!, Optional step! = 1) As IEnumerable(Of Single)
        For i As Single = from To [to] Step [step]
            Yield i
        Next
    End Function

    Public Iterator Function Range(from&, to&, Optional step& = 1) As IEnumerable(Of Long)
        For i As Long = from To [to] Step [step]
            Yield i
        Next
    End Function

    Public Iterator Function Range(from@, to@, Optional step@ = 1) As IEnumerable(Of Decimal)
        For i As Decimal = from To [to] Step [step]
            Yield i
        Next
    End Function

    Public Iterator Function Range(from As Short, [to] As Short, Optional [step] As Short = 1) As IEnumerable(Of Short)
        For i As Short = from To [to] Step [step]
            Yield i
        Next
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="from"></param>
    ''' <param name="[to]"></param>
    ''' <param name="step%">Milliseconds</param>
    ''' <returns></returns>
    Public Iterator Function Range(from As Date, [to] As Date, Optional step# = 1) As IEnumerable(Of Date)
        Dim t As TimeSpan = TimeSpan.FromMilliseconds([step])

        ' 由于后面的Do循环之中from是放在加减法之后的，所以需要在这里把最开始的值返回，要不然第一个值会被丢失的。。。
        Yield from

        If from < [to] Then
            Do While from < [to]
                from = from + t
                Yield from
            Loop
        Else
            Do While from > [to]
                from = from - t
                Yield from
            Loop
        End If
    End Function
End Module
