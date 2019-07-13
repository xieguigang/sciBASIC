#Region "Microsoft.VisualBasic::95155070099b2ba9b2814538e446dacd, Microsoft.VisualBasic.Core\Extensions\Collection\Linq\NumericSequence.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module NumericSequence
    ' 
    '         Function: InRange, (+7 Overloads) Range
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default

Namespace Linq

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

        Public ReadOnly OneDay As [Default](Of TimeSpan) = New TimeSpan(days:=1, hours:=0, minutes:=0, seconds:=0)
        Public ReadOnly OneHour As [Default](Of TimeSpan) = New TimeSpan(hours:=1, minutes:=0, seconds:=0)
        Public ReadOnly OneMinute As [Default](Of TimeSpan) = New TimeSpan(minutes:=1, seconds:=0, hours:=0)
        Public ReadOnly OneSecond As [Default](Of TimeSpan) = New TimeSpan(seconds:=1, hours:=0, minutes:=0)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="from"></param>
        ''' <param name="[to]"></param>
        ''' <param name="step%">Milliseconds</param>
        ''' <returns></returns>
        Public Iterator Function Range(from As Date, [to] As Date, Optional [step] As TimeSpan = Nothing) As IEnumerable(Of Date)
            Dim t As TimeSpan = [step] Or OneDay

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
End Namespace
