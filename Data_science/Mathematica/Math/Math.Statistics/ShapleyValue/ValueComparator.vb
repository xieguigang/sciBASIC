Namespace ShapleyValue

    ''' 
    ''' <summary>
    ''' Comparator to sort an HashMap
    ''' 
    ''' </summary>
    Public Class ValueComparator
        Implements IComparer(Of String)
        Friend base As IDictionary(Of String, Double)

        Public Sub New(base As IDictionary(Of String, Double))
            Me.base = base
        End Sub

        ' Note: this comparator imposes orderings that are inconsistent with
        ' equals.
        Public Overridable Function Compare(a As String, b As String) As Integer Implements IComparer(Of String).Compare
            If base(a) >= base(b) Then
                Return -1
            Else
                Return 1
            End If ' returning 0 would merge keys
        End Function
    End Class


End Namespace
