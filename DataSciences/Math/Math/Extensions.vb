Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Public Module Extensions

    ''' <summary>
    ''' 返回数值序列之中的首次出现符合条件的减少的位置
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="ratio"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FirstDecrease(data As IEnumerable(Of Double), Optional ratio As Double = 10) As Integer
        Dim pre As Double = data.First
        Dim pr As Double = 1000000

        For Each x In data.SeqIterator
            Dim d = (pre - x.obj)

            If d / pr > ratio Then
                Return x.i
            Else
                pr = d
                pre = x.obj
            End If
        Next

        Return -1 ' 没有找到符合条件的点
    End Function

    <Extension>
    Public Function FirstIncrease(data As IEnumerable(Of Double), Optional ratio As Double = 10) As Integer
        Dim pre As Double = data.First
        Dim pr As Double = 1000000

        For Each x In data.SeqIterator
            Dim d = (x.obj - pre)

            If d / pr > ratio Then
                Return x.i
            Else
                pr = d
                pre = x.obj
            End If
        Next

        Return -1 ' 没有找到符合条件的点
    End Function

    <Extension>
    Public Function Reach(data As IEnumerable(Of Double), n As Double, Optional offset As Double = 0) As Integer
        For Each x In data.SeqIterator
            If Math.Abs(x.obj - n) <= offset Then
                Return x.i
            End If
        Next

        Return -1
    End Function
End Module
