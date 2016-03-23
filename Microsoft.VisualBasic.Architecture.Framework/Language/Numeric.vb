Imports System.Runtime.CompilerServices

Namespace Language

    ''' <summary>
    ''' Defines a generalized type-specific comparison method that a value type or class
    ''' implements to order or sort its instances.
    ''' </summary>
    ''' <remarks>
    ''' 
    ''' Summary:
    ''' 
    '''     Compares the current instance with another object of the same type and returns
    '''     an integer that indicates whether the current instance precedes, follows, or
    '''     occurs in the same position in the sort order as the other object.
    '''
    ''' Returns:
    ''' 
    '''     A value that indicates the relative order of the objects being compared. The
    '''     return value has these meanings: 
    '''     
    '''     1. Value Meaning Less than zero 
    '''        This instance precedes obj in the sort order. 
    '''     
    '''     2. Zero 
    '''        This instance occurs in the same position in the sort order as obj. 
    '''        
    '''     3. Greater than zero 
    '''        This instance follows obj in the sort order.
    '''
    ''' Exceptions:
    ''' 
    '''   T:System.ArgumentException:
    '''     obj is not the same type as this instance.
    ''' </remarks>
    Public Module Numeric

        Public Function Equals(Of T As IComparable)(a As T, b As T) As Boolean
            Return a.CompareTo(b) = 0
        End Function

        <Extension> Public Function LessThan(Of T As IComparable)(a As T, b As T) As Boolean
            Return a.CompareTo(b) < 0
        End Function

        <Extension> Public Function GreaterThan(Of T As IComparable)(a As T, b As T) As Boolean
            Return a.CompareTo(b) > 0
        End Function

        <Extension> Public Function LessThanOrEquals(Of T As IComparable)(a As T, b As T) As Boolean
            Return a.LessThan(b) OrElse Equals(a, b)
        End Function

        <Extension> Public Function GreaterThanOrEquals(Of T As IComparable)(a As T, b As T) As Boolean
            Return a.GreaterThan(b) OrElse Equals(a, b)
        End Function

        <Extension> Public Function NextInteger(rnd As Random, max As Integer) As Int
            Return New Int(rnd.Next(max))
        End Function
    End Module

    Public Structure Int : Implements IComparable

        Dim value As Integer

        Sub New(x As Integer)
            value = x
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Dim type As Type = obj.GetType

            If type.Equals(GetType(Integer)) Then
                Return value.CompareTo(DirectCast(obj, Integer))
            ElseIf type.Equals(GetType(Int)) Then
                Return value.CompareTo(DirectCast(obj, Int).value)
            Else
                Throw New Exception($"Miss-match of type:  {GetType(Int).FullName} --> {type.FullName}")
            End If
        End Function

        ''' <summary>
        ''' n &lt; value &lt;= n2
        ''' 假若n 大于value，则返回最大值，上面的表达式肯定不成立
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator <(n As Integer, x As Int) As Int
            If n >= x.value Then
                Return New Int(Integer.MaxValue)
            Else
                Return x
            End If
        End Operator

        Public Shared Operator <=(x As Int, n As Integer) As Boolean
            Return x.value <= n
        End Operator

        Public Shared Operator >=(x As Int, n As Integer) As Boolean
            Return x.value >= n
        End Operator

        Public Shared Operator >(n As Integer, x As Int) As Int
            Return x
        End Operator
    End Structure

    Public Structure Float
        Implements IComparable

        Dim value As Double

        Sub New(x As Double)
            value = x
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Dim type As Type = obj.GetType

            If type.Equals(GetType(Double)) Then
                Return value.CompareTo(DirectCast(obj, Double))
            ElseIf type.Equals(GetType(Float)) Then
                Return value.CompareTo(DirectCast(obj, Float).value)
            Else
                Throw New Exception($"Miss-match of type:  {GetType(Float).FullName} --> {type.FullName}")
            End If
        End Function

        ''' <summary>
        ''' n &lt; value &lt;= n2
        ''' 假若n 大于value，则返回最大值，上面的表达式肯定不成立
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator <(n As Double, x As Float) As Float
            If n >= x.value Then
                Return New Float(Double.MaxValue)
            Else
                Return x
            End If
        End Operator

        Public Shared Operator <=(x As Float, n As Double) As Boolean
            Return x.value <= n
        End Operator

        Public Shared Operator >=(x As Float, n As Double) As Boolean
            Return x.value >= n
        End Operator

        Public Shared Operator >(n As Double, x As Float) As Float
            Return x
        End Operator
    End Structure
End Namespace