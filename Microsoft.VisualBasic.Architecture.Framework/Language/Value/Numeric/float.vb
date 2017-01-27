Namespace Language

    ''' <summary>
    ''' <see cref="System.Double"/>
    ''' </summary>
    Public Class float : Inherits Value(Of Double)
        Implements IComparable

        Sub New(x#)
            value = x#
        End Sub

        Sub New()
            Me.New(0R)
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Dim type As Type = obj.GetType

            If type.Equals(GetType(Double)) Then
                Return value.CompareTo(DirectCast(obj, Double))
            ElseIf type.Equals(GetType(float)) Then
                Return value.CompareTo(DirectCast(obj, float).value)
            Else
                Throw New Exception($"Miss-match of type:  {GetType(float).FullName} --> {type.FullName}")
            End If
        End Function

        ''' <summary>
        ''' n &lt; value &lt;= n2
        ''' 假若n 大于value，则返回最大值，上面的表达式肯定不成立
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator <(n#, x As float) As float
            If n >= x.value Then
                Return New float(Double.MaxValue)
            Else
                Return x
            End If
        End Operator

        Public Shared Operator *(n#, x As float) As Double
            Return n * x.value
        End Operator

        Public Overloads Shared Operator +(x As float, y As float) As Double
            Return x.value + y.value
        End Operator

        Public Overloads Shared Operator /(x As float, y As float) As Double
            Return x.value / y.value
        End Operator

        Public Overloads Shared Operator +(x#, y As float) As Double
            Return x + y.value
        End Operator

        Public Overloads Shared Widening Operator CType(x#) As float
            Return New float(x)
        End Operator

        Public Overloads Shared Operator <=(x As float, n As Double) As Boolean
            Return x.value <= n
        End Operator

        Public Overloads Shared Operator >=(x As float, n As Double) As Boolean
            Return x.value >= n
        End Operator

        Public Overloads Shared Operator /(x As float, n As Double) As Double
            Return x.value / n
        End Operator

        Public Shared Operator >(n As Double, x As float) As float
            Return x
        End Operator

        Public Shared Operator ^(x As float, power As Double) As Double
            Return x.value ^ power
        End Operator

        Public Overloads Shared Narrowing Operator CType(f As float) As Double
            Return f.value
        End Operator

        Public Overloads Shared Operator -(a As float, b As float) As Double
            Return a.value - b.value
        End Operator

        Public Overloads Shared Operator *(a As float, b As float) As Double
            Return a.value * b.value
        End Operator
    End Class
End Namespace