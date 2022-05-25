Namespace Drawing2D.Text.Nudge

    Public Class ConflictIndexTuple

        ''' <summary>
        ''' [0] item 1
        ''' </summary>
        ''' <returns></returns>
        Public Property i As Integer
        ''' <summary>
        ''' [1] item 2
        ''' </summary>
        ''' <returns></returns>
        Public Property j As Integer

        Public Overrides Function ToString() As String
            Return $"[{i} == {j}]"
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Not TypeOf obj Is ConflictIndexTuple Then
                Return False
            ElseIf obj Is Me Then
                Return True
            Else
                With DirectCast(obj, ConflictIndexTuple)
                    Return i = .i AndAlso j = .j
                End With
            End If
        End Function

        Public Shared Function [In](conflicts As ConflictIndexTuple(), parent As IEnumerable(Of ConflictIndexTuple)) As Boolean
            Throw New NotImplementedException
        End Function
    End Class
End Namespace