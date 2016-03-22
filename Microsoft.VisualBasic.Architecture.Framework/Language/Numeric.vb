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
    End Module
End Namespace