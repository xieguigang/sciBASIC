Imports System.Runtime.InteropServices

' Module:   128-bit Unsigned Integer Class
' Author:   James Merrill
' Note:     I am using uxl as the variable name prefix for this class
'           (Unsigned eXtra Large)

Public Class UInt128
    ' High- and Low-order QWords
    Private _hi As ULong
    Private _lo As ULong

    ' Treat these as constants
    ' Minimum Value for this class
    Public Shared ReadOnly MinValue As UInt128 = 0
    ' Maximum value for this class
    Public Shared ReadOnly MaxValue As UInt128 = Not MinValue

    ' Outside access to _hi and _lo
    Public Property Hi As ULong
        Get
            Return _hi
        End Get
        Set(value As ULong)
            _hi = value
        End Set
    End Property
    Public Property Lo As ULong
        Get
            Return _lo
        End Get
        Set(value As ULong)
            _lo = value
        End Set
    End Property

    ' Default constructor
    Public Sub New()
        _hi = 0
        _lo = 0
    End Sub

    ''' <summary>
    ''' <see cref="UInt64"/>, 8 bytes
    ''' </summary>
    <StructLayout(LayoutKind.Explicit)> Public Structure UncheckedUInt64

        <FieldOffset(0)> Private longValue As ULong
        <FieldOffset(0)> Dim intValueLo As ULong
        <FieldOffset(4)> Dim intValueHi As ULong

        Sub New(newLongValue As ULong)
            longValue = newLongValue
        End Sub

        Sub New(l&)
            longValue = l
        End Sub

        Public Overrides Function ToString() As String
            Return longValue.ToString
        End Function

        Public Overloads Shared Widening Operator CType(value As ULong) As UncheckedUInt64
            Return New UncheckedUInt64(value)
        End Operator

        Public Overloads Shared Widening Operator CType(value As UncheckedUInt64) As ULong
            Return value.longValue
        End Operator
    End Structure

    Sub New(ul As ULong)
        Dim l As UncheckedUInt64 = ul
        _hi = l.intValueHi
        _lo = l.intValueLo
    End Sub

    Sub New(l&)
        Dim ul As New UncheckedUInt64(l)
        _hi = ul.intValueHi
        _lo = ul.intValueLo
    End Sub

    ' Combine high-order and low-order parts
    ' Dim uxlVariable As New UInt128(&HFFFFFFFFFFFFFFFFUL, &HFFFFFFFFFFFFFFFFUL)
    Public Sub New(ByVal argHi As ULong, ByVal argLo As ULong)
        ' save values internally
        _hi = argHi
        _lo = argLo
    End Sub
    ' Copy values from existing UInt128
    ' Can use CType operator to feed value into this Sub
    Public Sub New(ByRef arg128 As UInt128)
        ' Take values from arg and save internally
        _hi = arg128.Hi
        _lo = arg128.Lo
    End Sub

    ' Always gives either 1 or 0
    ' When setting, any non-zero value evaluates to 1
    Public Property Bit(ByVal position As Integer) As Integer
        ' Position is 0-based, with bit(0) being the lowest-order bit
        Get ' Retrieve the requested bit
            If position > 63 Then
                ' Return bit from High part
                Return CInt((_hi >> (position - 64)) And 1UL)
            Else ' Return bit from Low part
                Return CInt((_lo >> position) And 1UL)
            End If
        End Get
        ' Set the specified bit
        Set(value As Integer)
            If position > 63 Then
                ' Operate on High QWord
                If value <> 0 Then
                    ' Set bit
                    _hi = _hi Or (1UL << (position - 64))
                Else ' Clear bit
                    _hi = _hi And Not (1UL << (position - 64))
                End If
            Else ' Operate on Low QWord
                If value <> 0 Then
                    ' Set bit
                    _lo = _lo Or (1UL << position)
                Else ' Clear bit
                    _lo = _lo And Not (1UL << position)
                End If
            End If
        End Set
    End Property

    ' Returns the number of bits used (1-based)
    Public ReadOnly Property Length As Integer
        Get
            ' Set testing variables for High part
            Dim intReturn As Integer = 128
            If _hi = 0 Then
                ' No bits used - 0 length
                If _lo = 0 Then Return 0
                ' Set variable for Low part
                intReturn = 64
            End If
            ' Check each bit until a 1 is reached
            Do Until Bit(intReturn - 1) = 1
                intReturn -= 1
            Loop
            ' Return Length
            Return intReturn
        End Get
    End Property

    ' Return Hex string
    Public ReadOnly Property ToHex As String
        Get
            ' Gives full 128 character hex string, left padded with 0's.
            Return Hex(Hi).PadLeft(16, "0"c) + Hex(Lo).PadLeft(16, "0"c)
        End Get
    End Property

    ' Return Integer String
    Public Shadows ReadOnly Property ToString As String
        Get
            Dim strOut As String = ""
            Dim uxlHold As UInt128 = (CType(_hi, UInt128) << 64) Or _lo
            Do While uxlHold > 0
                ' Convert each bit (lowest to highest) to a string and add it
                ' to the front of the output string
                strOut = strOut.Insert(0, CType(uxlHold Mod 10, UInteger).ToString)
                uxlHold \= 10
            Loop
            ' If nothing has been put into strOut, return "0"
            If strOut.Length = 0 Then strOut = "0"
            Return strOut
        End Get
    End Property

    ' TryParse converts a string to UInt128
    Public Shared Function TryParse(value As String, ByRef arg128 As UInt128) As Boolean
        ' Prime the return value
        arg128 = 0
        ' Check for bad characters
        For i = 0 To value.Length - 1
            If "0123456789".Contains(value.Substring(i, 1)) Then
                ' Add the next digit - Use CUInt because we know it's a digit here
                arg128 = arg128 * 10 + CUInt(value.Substring(i, 1))
            Else ' Bad character - Toss the whole thing out
                arg128 = 0
                Return False
            End If
        Next
        ' If it gets here, everything is ok
        Return True
    End Function

    ' Widen all Int/UInt types to UInt128. Reduces the number of overloads required for operators
    Public Shared Widening Operator CType(ByVal value As ULong) As UInt128
        Return New UInt128(0, value)
    End Operator

    ' Narrow UInt128 to ULong
    Public Shared Narrowing Operator CType(arg128 As UInt128) As ULong
        ' Only valid if High QWord is 0
        If arg128.Hi = 0 Then
            Return arg128.Lo
        Else ' Overflow error
            Throw New OverflowException()
        End If
    End Operator

    ' Bit-wise And operator
    Public Shared Operator And(ByVal argLeft As UInt128, ByVal argRight As UInt128) As UInt128
        ' Perform And on each section pair and return result
        Return New UInt128(argLeft.Hi And argRight.Hi, argLeft.Lo And argRight.Lo)
    End Operator

    ' Bit-wise Or operator
    Public Shared Operator Or(ByVal argLeft As UInt128, ByVal argRight As UInt128) As UInt128
        ' Perform Or on each section pair and return result
        Return New UInt128(argLeft.Hi Or argRight.Hi, argLeft.Lo Or argRight.Lo)
    End Operator

    ' Bit-wise Not operator
    Public Shared Operator Not(ByVal argLeft As UInt128) As UInt128
        ' Perform Not on each section and return result
        Return New UInt128(Not argLeft.Hi, Not argLeft.Lo)
    End Operator

    ' Bit-wise Xor operator
    Public Shared Operator Xor(ByVal argLeft As UInt128, ByVal argRight As UInt128) As UInt128
        ' Perform Xor on each section pair and return result
        Return New UInt128(argLeft.Hi Xor argRight.Hi, argLeft.Lo Xor argRight.Lo)
    End Operator

    ' Return Additive inverse of arguement
    Public Shared Operator -(ByVal arg128 As UInt128) As UInt128
        ' Add 1 to the inverse of the starting value
        Return (Not arg128) + 1
    End Operator

    ' Subtraction - Take additive inverse and pass to addition
    ' Uses the fact that addition drops overflow
    Public Shared Operator -(ByVal argLeft As UInt128, ByVal argRight As UInt128) As UInt128
        Return argLeft + (-argRight)
    End Operator

    ' Addition - Overflow is dropped
    Public Shared Operator +(ByVal argLeft As UInt128, ByVal argRight As UInt128) As UInt128
        Dim uxlResult As UInt128 = 0
        Dim blnCarry As Boolean = False
        ' Check for and prevent overflow
        If argLeft.Lo > ULong.MaxValue - argRight.Lo Then
            ' Subtract difference between 2^64 and argRight.lo from argLeft.lo
            uxlResult.Lo = argLeft.Lo - (ULong.MaxValue - argRight.Lo + 1UL)
            ' Set carry flag
            blnCarry = True
        Else ' Add lo QWord
            uxlResult.Lo = argLeft.Lo + argRight.Lo
        End If
        ' Check for overflow
        If argLeft.Hi > ULong.MaxValue - argRight.Hi Then
            ' Subtract difference between 2^64 and argRight.hi from argLeft.hi
            uxlResult.Hi = argLeft.Hi - (ULong.MaxValue - argRight.Hi + 1UL)
        Else ' Add hi QWord
            uxlResult.Hi = argLeft.Hi + argRight.Hi
        End If
        ' If carry flag is set, add 1 to hi
        If blnCarry Then
            ' Avoid overflow error - If Hi is at max, roll it to 0
            If uxlResult.Hi = ULong.MaxValue Then
                uxlResult.Hi = 0
            Else ' Add 1 for carry
                uxlResult.Hi += 1UL
            End If
        End If
        Return uxlResult
    End Operator

    ' Multiply - Overflow is dropped
    ' Speed test shows 1000x faster than bitwise
    Public Shared Operator *(ByVal argLeft As UInt128, ByVal argRight As UInt128) As UInt128
        ' Split each arg into 4 32-bit parts
        Dim intLeftParts() As UInteger = {CUInt(argLeft.Lo And &HFFFFFFFFUL), CUInt(argLeft.Lo >> 32), CUInt(argLeft.Hi And &HFFFFFFFFUL), CUInt(argLeft.Hi >> 32)}
        Dim intRightParts() As UInteger = {CUInt(argRight.Lo And &HFFFFFFFFUL), CUInt(argRight.Lo >> 32), CUInt(argRight.Hi And &HFFFFFFFFUL), CUInt(argRight.Hi >> 32)}
        ' Result registers - Use 8 to avoid runtime errors
        Dim lngResults(7) As ULong
        For i = 0 To 3 ' Cycle through Right arg parts
            For j = 0 To 3 ' Cycle through Left arg parts
                lngResults(i + j) += CULng(intRightParts(i)) * CULng(intLeftParts(j))
                For k = i + j To 6 ' Move overflow into next one up
                    lngResults(k + 1) += lngResults(k) >> 32
                    lngResults(k) = lngResults(k) And &HFFFFFFFFUL
                Next ' k
            Next ' j 
        Next ' i 
        ' Put result together and return it - Overflow is dropped
        Return (CType(lngResults(3), UInt128) << 96) Or (CType(lngResults(2), UInt128) << 64) Or (CType(lngResults(1), UInt128) << 32) Or lngResults(0)
    End Operator

    ' Division - Shift algorithm
    ' Defined first as Integer division (\) and then referred to by standard division (/)
    ' Adapted from https://en.wikipedia.org/wiki/Division_algorithm
    Public Shared Operator \(ByVal argLeft As UInt128, ByVal argRight As UInt128) As UInt128
        ' Return division by zero error
        If argRight = 0 Then Throw New DivideByZeroException
        ' Return 0
        If argRight > argLeft Then Return 0
        ' Quotient and Remainder variables
        Dim uxlQuotient As UInt128 = 0
        Dim uxlRemainder As UInt128 = 0
        ' Bit length of stored numerator
        Dim intBits As Integer = argLeft.Length
        ' Loop to process each bit
        For i = intBits - 1 To 0 Step -1
            ' Shift remainder
            uxlRemainder = uxlRemainder << 1
            ' Copy bit i into remainder's low bit
            uxlRemainder.Bit(0) = argLeft.Bit(i)
            ' Check to see if remainder is higher than divisor
            If uxlRemainder >= argRight Then
                ' Subtract divisor from remainder
                uxlRemainder -= argRight
                ' Set current bit of quotient
                uxlQuotient.Bit(i) = 1
            End If
        Next
        ' Return quotient
        Return uxlQuotient
    End Operator

    ' / Operator - added for completeness
    ' Only refers arguements to \ operator
    Public Shared Operator /(ByVal argLeft As UInt128, ByVal argRight As UInt128) As UInt128
        Return argLeft \ argRight
    End Operator

    ' Mod operator - Shift algorithm
    ' Same algorithm as division, but return the remainder
    ' Adapted from https://en.wikipedia.org/wiki/Division_algorithm
    Public Shared Operator Mod(ByVal argLeft As UInt128, ByVal argRight As UInt128) As UInt128
        ' Return division by zero error
        If argRight = 0 Then Throw New DivideByZeroException
        If argRight > argLeft Then Return argLeft
        ' Quotient and Remainder variables
        Dim uxlQuotient As UInt128 = 0
        Dim uxlRemainder As UInt128 = 0
        ' Bit length of stored numerator
        Dim intBits As Integer = argLeft.Length
        ' Loop to process each bit
        For i = intBits - 1 To 0 Step -1
            ' Shift remainder
            uxlRemainder = uxlRemainder << 1
            ' Copy bit i into remainder's low bit
            uxlRemainder.Bit(0) = argLeft.Bit(i)
            ' Check to see if remainder is higher than divisor
            If uxlRemainder >= argRight Then
                ' Subtract divisor from remainder
                uxlRemainder -= argRight
                ' Set current bit of quotient
                uxlQuotient.Bit(i) = 1
            End If
        Next
        ' Return Modulus
        Return uxlRemainder
    End Operator

    ' Exponent
    Public Shared Operator ^(argLeft As UInt128, argRight As Integer) As UInt128
        ' Prime output to 1
        Dim uxlExponent As UInt128 = 1
        ' Loop argRight times
        For i = 1 To argRight
            ' Multiply accumulator with argLeft
            uxlExponent *= argLeft
        Next ' i
        ' Return result
        Return uxlExponent
    End Operator

    ' Shift right
    Public Shared Operator >>(ByVal argLeft As UInt128, ByVal argRight As Integer) As UInt128
        ' Never shift more than 127 bits
        Dim intShift As Integer = argRight Mod 128
        If argRight = 0 Then
            Return argLeft
        ElseIf argRight < 0 Then
            ' Negative shift? Shift left instead
            Return argLeft << -argRight
        ElseIf intShift < 64 Then
            ' Shift bits into the low-order QWord
            Return New UInt128(argLeft.Hi >> intShift, (argLeft.Lo >> intShift) Or (argLeft.Hi << (64 - intShift)))
        Else
            ' High-order QWord is zeroed and remainder of shift moves its bits into the Low-order QWord
            Return argLeft.Hi >> (intShift - 64)
        End If
    End Operator

    ' Shift left
    Public Shared Operator <<(ByVal argLeft As UInt128, ByVal argRight As Integer) As UInt128
        ' Never shift more than 127 bits
        Dim intShift As Integer = argRight Mod 128
        If intShift = 0 Then
            Return argLeft
        ElseIf argRight < 0 Then
            ' Negative shift? Shift right instead
            Return argLeft >> -argRight
        ElseIf intShift < 64 Then
            ' Shift bits into high-or
            Return New UInt128((argLeft.Hi << intShift) Or (argLeft.Lo >> (64 - intShift)), argLeft.Lo << intShift)
        Else
            ' Low order QWord is 0, shifts into high order QWord
            Return New UInt128(argLeft.Lo << (intShift - 64), 0)
        End If
    End Operator

    ' Comparisons
    ' Equality
    Public Shared Operator =(ByVal argLeft As UInt128, ByVal argRight As UInt128) As Boolean
        ' If both halves are equal then return True
        Return argLeft.Hi = argRight.Hi AndAlso argLeft.Lo = argRight.Lo
    End Operator

    ' Inequality
    Public Shared Operator <>(ByVal argLeft As UInt128, ByVal argRight As UInt128) As Boolean
        ' Test for equality and reverse the answer
        Return Not (argLeft = argRight)
    End Operator

    ' Less than
    Public Shared Operator <(ByVal argLeft As UInt128, ByVal argRight As UInt128) As Boolean
        ' Test the high part first. Then if the High parts are equal, test the low parts.
        Return argLeft.Hi < argRight.Hi OrElse (argLeft.Hi = argRight.Hi AndAlso argLeft.Lo < argRight.Lo)
    End Operator

    ' Greater than
    Public Shared Operator >(ByVal argLeft As UInt128, ByVal argRight As UInt128) As Boolean
        ' Test the high part first. Then if the High parts are equal, test the low parts.
        Return argLeft.Hi > argRight.Hi OrElse (argLeft.Hi = argRight.Hi AndAlso argLeft.Lo > argRight.Lo)
    End Operator

    ' Greater than or Equal
    Public Shared Operator >=(ByVal argLeft As UInt128, ByVal argRight As UInt128) As Boolean
        ' Test for less than and return the opposite
        Return Not (argLeft < argRight)
    End Operator

    ' Less than or Equal
    Public Shared Operator <=(ByVal argLeft As UInt128, ByVal argRight As UInt128) As Boolean
        ' Test for greater than and return the opposite
        Return Not (argLeft > argRight)
    End Operator

    ' IsTrue and IsFalse - used for "AndAlso" and "OrElse" shortcutting
    Public Shared Operator IsFalse(ByVal arg128 As UInt128) As Boolean
        ' If the value is 0, all subsequent -And- operators will result in 0
        Return arg128 = MinValue
    End Operator

    Public Shared Operator IsTrue(ByVal arg128 As UInt128) As Boolean
        ' If the value is MaxValue, all subsequent -Or- operators will result in MaxValue
        Return arg128 = MaxValue
    End Operator
End Class