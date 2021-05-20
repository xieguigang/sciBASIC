Public Class Chain

    ''' <summary>
    ''' 一般是在这里使用一个短的字符串来引用目标对象
    ''' </summary>
    ''' <returns></returns>
    Public Property obSequence As String()

    ''' <summary>
    ''' 可能会存在通过所引用的对象的相似度阈值来表示相等
    ''' </summary>
    Friend ReadOnly equalsTo As Func(Of String, String, Boolean)

    Default Public ReadOnly Property Item(i As Integer) As String
        Get
            Return _obSequence(i)
        End Get
    End Property

    Public ReadOnly Property length As Integer
        Get
            Return obSequence.Length
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="equalsTo">
    ''' 因为可能会存在通过所引用的对象的相似度阈值来表示相等
    ''' 所以在这里会多出来一个额外的函数来进行等价性计算
    ''' </param>
    Sub New(Optional equalsTo As Func(Of String, String, Boolean) = Nothing)
        If equalsTo Is Nothing Then
            Me.equalsTo = Function(str1, str2) str1 = str2
        Else
            Me.equalsTo = equalsTo
        End If
    End Sub

    Public Function IndexOf(obj As String) As Integer
        Dim i As Integer = 0

        For Each ref As String In obSequence
            If equalsTo(obj, ref) Then
                Return i
            End If

            i += 1
        Next

        Return -1
    End Function

    Public Overrides Function ToString() As String
        Return obSequence.JoinBy("->")
    End Function
End Class
