Imports System
Imports System.Runtime.CompilerServices


Public Interface IProvideRandomValues
    ReadOnly Property IsThreadSafe As Boolean

    ''' <summary>
    ''' Generates a random float. Values returned are from 0.0 up to but not including 1.0.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Function NextFloat() As Single

    ''' <summary>
    ''' Fills the elements of a specified array of bytes with random numbers.
    ''' </summary>
    ''' <paramname="buffer">An array of bytes to contain random numbers.</param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub NextFloats(ByVal buffer As Single())

    ''' <summary>
    ''' Returns a random integer that is within a specified range.
    ''' </summary>
    ''' <paramname="minValue">The inclusive lower bound of the random number returned.</param>
    ''' <paramname="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
    ''' <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue</returns>        '  equals maxValue, minValue is returned.</returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Function [Next](ByVal minValue As Integer, ByVal maxValue As Integer) As Integer
End Interface
