Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Linq.JoinExtensions

Namespace AprioriRules

    Public Structure ItemSet : Implements IComparable(Of ItemSet)

        Public Items As Item()

        Public ReadOnly Property Length As Integer
            Get
                Return Items.Length
            End Get
        End Property

        Default Public ReadOnly Property Item(offset As Integer) As Item
            Get
                Return Items(offset)
            End Get
        End Property

        Sub New(scalar As Item)
            Items = {scalar}
        End Sub

        Sub New(items As IEnumerable(Of Item))
            Me.Items = items.ToArray
        End Sub

        Public Overrides Function GetHashCode() As Integer
            If IsNullOrEmpty() Then
                Return 0
            End If

            Dim hash As Integer = 1
            Dim el_hash As Integer

            For Each element As Item In Items
                el_hash = element.Code Xor (element.Code >> 32)
                hash = 31 * hash + el_hash
            Next

            Return hash
        End Function

        Public Function IsNullOrEmpty() As Boolean
            Return Items Is Nothing OrElse Items.Length = 0
        End Function

        Public Function SorterSortTokens() As ItemSet
            Return New ItemSet With {.Items = Items.OrderBy(Function(a) a).ToArray}
        End Function

        Public Overrides Function ToString() As String
            Return "{" & Items.Select(Function(i) i.Item).JoinBy(", ") & "}"
        End Function

        Public Function Contains(i As Item) As Boolean
            For Each i32 As Item In Items
                If i32 = i Then
                    Return True
                End If
            Next

            Return False
        End Function

        Public Function PopLast() As ItemSet
            Return New ItemSet(scalar:=Items.Last)
        End Function

        Public Shared Function Empty() As ItemSet
            Return New ItemSet With {.Items = {}}
        End Function

        ''' <summary>
        ''' GetRemaining: removes all child elements from parent
        ''' </summary>
        ''' <param name="child"></param>
        ''' <returns></returns>
        Public Function Remove(child As ItemSet) As ItemSet
            Dim remaining As New List(Of Item)

            For Each i As Item In Items
                If Not child.Contains(i) Then
                    Call remaining.Add(i)
                End If
            Next

            Return New ItemSet(remaining)
        End Function

        Public Function Slice(start As Integer, count As Integer) As ItemSet
            Return New ItemSet With {.Items = Items.Skip(start).Take(count).ToArray}
        End Function

        Public Overloads Shared Operator &(a As ItemSet, b As ItemSet) As ItemSet
            Return New ItemSet With {.Items = a.Items.JoinIterates(b.Items).ToArray}
        End Operator

        Public Overloads Shared Operator =(a As ItemSet, b As ItemSet) As Boolean
            Return a.Items.SequenceEqual(b.Items)
        End Operator

        Public Overloads Shared Operator <>(a As ItemSet, b As ItemSet) As Boolean
            Return Not a = b
        End Operator

        Private Function GetCompareValue() As ULong
            Dim p As ULong = 0

            For i As Integer = 0 To Items.Length - 1
                p += Items(i).Code * (10 ^ i)
            Next

            Return p
        End Function

        Public Function CompareTo(other As ItemSet) As Integer Implements IComparable(Of ItemSet).CompareTo
            Dim val1 As ULong = GetCompareValue()
            Dim val2 As ULong = other.GetCompareValue

            Return val1.CompareTo(val2)
        End Function
    End Structure

End Namespace