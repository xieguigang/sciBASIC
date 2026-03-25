Public Class NamedSparseMatrix

    ReadOnly data As New Dictionary(Of String, Dictionary(Of String, Double))
    ReadOnly names As New List(Of String)
    ReadOnly hash As New HashSet(Of String)

    Default Public Property Value(i As String, j As String) As Double
        Get
            Return GetDirectedValue(i, j)
        End Get
        Set
            Call SetValue(i, j, Value)
        End Set
    End Property

    Default Public Property Value(i As Integer, j As Integer) As Double
        Get
            Return Me(names(i), names(j))
        End Get
        Set
            Me(names(i), names(j)) = Value
        End Set
    End Property

    Public ReadOnly Property [Dim] As Integer
        Get
            Return names.Count
        End Get
    End Property

    ''' <summary>
    ''' Check of the edge connection is existsed inside this matrix?
    ''' </summary>
    ''' <param name="i"></param>
    ''' <param name="j"></param>
    ''' <returns></returns>
    Public Function CheckElement(i As String, j As String) As Boolean
        Return hash.Contains(i) AndAlso hash.Contains(j)
    End Function

    Public Function GetDirectedValue(i As String, j As String) As Double
        If Not data.ContainsKey(i) Then
            Return 0
        Else
            If data(i).ContainsKey(j) Then
                Return data(i)(j)
            Else
                Return 0
            End If
        End If
    End Function

    Public Sub SetValue(i As String, j As String, value As Double)
        If Not data.ContainsKey(i) Then
            data.Add(i, New Dictionary(Of String, Double))
            names.Add(i)
            hash.Add(i)
        End If
        If Not data(i).ContainsKey(j) Then
            names.Add(j)
            hash.Add(j)
        End If

        data(i)(j) = value
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{[Dim]} x {[Dim]}]"
    End Function
End Class
