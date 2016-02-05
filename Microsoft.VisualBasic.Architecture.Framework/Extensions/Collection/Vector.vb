Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq.Extensions

Public Module Vector

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="start">0 base</param>
    ''' <param name="length"></param>
    ''' <returns></returns>
    <Extension> Public Function Midv(Of T)(source As IEnumerable(Of T), start As Integer, length As Integer) As T()
        If source.IsNullOrEmpty Then
            Return New T() {}
        ElseIf source.Count < length Then
            Return source.ToArray
        End If

        Dim array As T() = source.ToArray
        Dim ends As Integer = start + length

        If ends > array.Length Then
            length -= array.Length - ends
        End If

        Dim buf As T() = New T(length - 1) {}
        Call System.Array.ConstrainedCopy(array, start, buf, Scan0, buf.Length)
        Return buf
    End Function

    <Extension> Public Function LoadDblArray(path As String) As Double()
        Dim array As String() = IO.File.ReadAllLines(path)
        Dim n As Double() = array.ToArray(Function(x) Val(x))
        Return n
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="delimiter">和字符串的Split函数一样，这里作为delimiter的元素都不会出现在结果之中</param>
    ''' <returns></returns>
    <Extension> Public Function Split(Of T)(source As IEnumerable(Of T), delimiter As Func(Of T, Boolean)) As T()()
        Dim array As T() = source.ToArray
        Dim list As New List(Of T())
        Dim tmp As New List(Of T)

        For i As Integer = 0 To array.Length - 1
            Dim x As T = array(i)
            If delimiter(x) = True Then
                Call list.Add(tmp.ToArray)
                Call tmp.Clear()
            Else
                Call tmp.Add(x)
            End If
        Next

        Return list.ToArray
    End Function
End Module
