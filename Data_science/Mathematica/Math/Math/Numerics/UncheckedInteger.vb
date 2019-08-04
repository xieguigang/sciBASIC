Imports System.Numerics
Imports System.Runtime.CompilerServices

Namespace Numerics

    Public Structure UncheckedInteger

        Dim Value As BigInteger

        Sub New(i%)
            Value = New BigInteger(i)
        End Sub

        Sub New(l&)
            Value = New BigInteger(l)
        End Sub

        Sub New(s As Short)
            Value = New BigInteger(s)
        End Sub

        Public Overrides Function ToString() As String
            Return Value.ToString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(unchecked As UncheckedInteger, i%) As UncheckedInteger
            Return New UncheckedInteger((unchecked.Value + i).uncheckedInteger)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(i%, unchecked As UncheckedInteger) As UncheckedInteger
            Return New UncheckedInteger((unchecked.Value + i).uncheckedInteger)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(unchecked As UncheckedInteger, i%) As UncheckedInteger
            Return New UncheckedInteger((unchecked.Value - i).uncheckedInteger)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(i%, unchecked As UncheckedInteger) As UncheckedInteger
            Return New UncheckedInteger((i - unchecked.Value).uncheckedInteger)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(unchecked As UncheckedInteger, i%) As UncheckedInteger
            Return New UncheckedInteger((unchecked.Value * i).uncheckedInteger)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(i%, unchecked As UncheckedInteger) As UncheckedInteger
            Return New UncheckedInteger((i * unchecked.Value).uncheckedInteger)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator /(unchecked As UncheckedInteger, i%) As UncheckedInteger
            Return New UncheckedInteger((unchecked.Value / i).uncheckedInteger)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator /(i%, unchecked As UncheckedInteger) As UncheckedInteger
            Return New UncheckedInteger((i / unchecked.Value).uncheckedInteger)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator ^(unchecked As UncheckedInteger, i%) As UncheckedInteger
            Return New UncheckedInteger(BigInteger.Pow(unchecked.Value, i).uncheckedInteger)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator ^(i%, unchecked As UncheckedInteger) As UncheckedInteger
            Return New UncheckedInteger(BigInteger.Pow(i, unchecked.Value).uncheckedInteger)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(unchecked As UncheckedInteger) As Integer
            Return unchecked.Value.uncheckedInteger
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(unchecked As UncheckedInteger) As Long
            Return unchecked.Value.uncheckedLong
        End Operator

    End Structure
End Namespace