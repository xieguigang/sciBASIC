Namespace Linq

    Public Structure VectorAssertor(Of T)

        Dim ALL As T
        Dim equal As Func(Of T, T, Boolean)
        Dim likes#

        Public Overrides Function ToString() As String
            Return ALL.ToString
        End Function

        Public Shared Operator Like(list As IEnumerable(Of T), assert As VectorAssertor(Of T)) As Boolean
            Dim array As T() = list.ToArray
            Dim n#

            With assert
                If .equal Is Nothing Then
                    n = array.Where(Function(x) x.Equals(.ALL)).Count
                Else
                    n = array.Where(Function(x) .equal(x, .ALL)).Count
                End If

                If .likes = 0R Then
                    Return (n / array.Length) >= 0.65
                Else
                    Return (n / array.Length) >= .likes
                End If
            End With
        End Operator

        ''' <summary>
        ''' ALL elements in target <paramref name="list"/> equals to <paramref name="assert"/> value
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="assert"></param>
        ''' <returns></returns>
        Public Shared Operator =(list As IEnumerable(Of T), assert As VectorAssertor(Of T)) As Boolean
            With assert
                If .equal Is Nothing Then
                    Return list.All(Function(x) x.Equals(.ALL))
                Else
                    Return list.All(Function(x) .equal(x, .ALL))
                End If
            End With
        End Operator

        Public Shared Operator <>(list As IEnumerable(Of T), assert As VectorAssertor(Of T)) As Boolean
            Return Not list = assert
        End Operator
    End Structure
End Namespace