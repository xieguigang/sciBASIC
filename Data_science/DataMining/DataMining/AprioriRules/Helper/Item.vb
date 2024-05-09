Namespace AprioriRules

    ''' <summary>
    ''' mapping the <see cref="Item"/> string comparision to <see cref="Code"/> comparision
    ''' </summary>
    ''' <remarks>
    ''' constant value liked, so readonly field at here
    ''' </remarks>
    Public Structure Item : Implements IComparable(Of Item)

        ''' <summary>
        ''' the hashcode of the <see cref="Item"/> string
        ''' </summary>
        ReadOnly Code As Integer
        ReadOnly Item As String

        Sub New(hashcode As Integer, item As String)
            Me.Code = hashcode
            Me.Item = item
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Return Code
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Code}]{Item}"
        End Function

        Public Function CompareTo(other As Item) As Integer Implements IComparable(Of Item).CompareTo
            Return Code.CompareTo(other.Code)
        End Function

        ''' <summary>
        ''' check equals of the <see cref="Code"/>
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =(a As Item, b As Item) As Boolean
            Return a.Code = b.Code
        End Operator

        Public Overloads Shared Operator <>(a As Item, b As Item) As Boolean
            Return a.Code <> b.Code
        End Operator

    End Structure
End Namespace