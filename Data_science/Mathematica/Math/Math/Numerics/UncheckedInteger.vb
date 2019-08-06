Imports System.Runtime.InteropServices

Namespace Numerics

    ''' <summary>
    ''' > https://stackoverflow.com/questions/2403154/fastest-way-to-do-an-unchecked-integer-addition-in-vb-net
    ''' </summary>
    <StructLayout(LayoutKind.Explicit)>
    Public Structure UncheckedInteger

        <FieldOffset(0)>
        Private longValue As Long
        <FieldOffset(0)>
        Private intValueLo As Integer
        <FieldOffset(4)>
        Private intValueHi As Integer

        ''' <summary>
        ''' The integer value
        ''' </summary>
        ''' <returns></returns>
        Public Property Value As Integer
            Get
                Return intValueLo
            End Get
            Set(value As Integer)
                longValue = value
            End Set
        End Property

        Public ReadOnly Property UncheckUInt32 As UInt32
            Get
                Dim bytes As Byte() = BitConverter.GetBytes(intValueLo)
                Dim uint As UInt32 = BitConverter.ToUInt32(bytes, Scan0)

                Return uint
            End Get
        End Property

        Private Sub New(newLongValue As Long)
            longValue = newLongValue
        End Sub

        Public Overrides Function ToString() As String
            Return Value
        End Function

        Public Overloads Shared Widening Operator CType(value As Integer) As UncheckedInteger
            Return New UncheckedInteger(CLng(value))
        End Operator

        Public Overloads Shared Widening Operator CType(value As Long) As UncheckedInteger
            Return New UncheckedInteger(value)
        End Operator

        Public Overloads Shared Widening Operator CType(value As UncheckedInteger) As Long
            Return value.longValue
        End Operator

        Public Overloads Shared Widening Operator CType(value As UncheckedInteger) As Integer
            Return value.intValueLo
        End Operator

        Public Overloads Shared Operator +(a As UncheckedInteger, b As Integer) As UncheckedInteger
            Return New UncheckedInteger(a.longValue + b)
        End Operator

        Public Overloads Shared Operator *(x As UncheckedInteger, y As Integer) As UncheckedInteger
            Return New UncheckedInteger(x.longValue * y)
        End Operator

        Public Overloads Shared Operator Xor(x As UncheckedInteger, y As Integer) As UncheckedInteger
            Return New UncheckedInteger(x.longValue Xor y)
        End Operator
    End Structure
End Namespace