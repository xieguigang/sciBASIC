
Imports System.Runtime.InteropServices

Namespace HashMaps

    ''' <summary>
    ''' <see cref="UInt64"/>
    ''' </summary>
    <StructLayout(LayoutKind.Explicit)> Public Structure UncheckedUInt64

        <FieldOffset(0)> Private longValue As UInt64
        <FieldOffset(0)> Private intValueLo As UInt32
        <FieldOffset(4)> Private intValueHi As UInt32

        Sub New(newLongValue As UInt64)
            longValue = newLongValue
        End Sub

        Public Overrides Function ToString() As String
            Return longValue
        End Function

        Public Overloads Shared Widening Operator CType(value As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(value)
        End Operator

        Public Overloads Shared Widening Operator CType(value As UncheckedUInt64) As UInt64
            Return value.longValue
        End Operator

        Public Overloads Shared Operator *(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue * y)
        End Operator

        Public Overloads Shared Operator /(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue / y)
        End Operator

        Public Overloads Shared Operator ^(x As UncheckedUInt64, y As Double) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue ^ y)
        End Operator

        Public Overloads Shared Operator Xor(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue Xor y)
        End Operator

        Public Overloads Shared Operator +(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue + y)
        End Operator

        Public Overloads Shared Operator -(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue - y)
        End Operator

        Public Overloads Shared Operator <<(x As UncheckedUInt64, y As Int32) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue << y)
        End Operator

        Public Overloads Shared Operator >>(x As UncheckedUInt64, y As Int32) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue >> y)
        End Operator

        Public Overloads Shared Operator And(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue And y)
        End Operator

        Public Overloads Shared Operator =(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue)
        End Operator

        Public Overloads Shared Operator <>(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue <> y)
        End Operator

        Public Overloads Shared Operator Not(x As UncheckedUInt64) As UncheckedUInt64
            Return New UncheckedUInt64(Not x.longValue)
        End Operator
    End Structure
End Namespace