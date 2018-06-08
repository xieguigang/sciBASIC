Imports System.Runtime.CompilerServices

Namespace Language.Default

    Public Structure BooleanAssert

        Dim bool As Boolean

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return bool.ToString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(b As Boolean) As BooleanAssert
            Return New BooleanAssert With {.bool = b}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(b As BooleanAssert) As Boolean
            Return b.bool
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator IsTrue(b As BooleanAssert) As Boolean
            Return b.bool
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator IsFalse(b As BooleanAssert) As Boolean
            Return b.bool = False
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(b As BooleanAssert, bool As Boolean) As Boolean
            Return b.bool = bool
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(b As BooleanAssert, bool As Boolean) As Boolean
            Return Not b = bool
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(bool As Boolean, b As BooleanAssert) As Boolean
            Return b = bool
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(bool As Boolean, b As BooleanAssert) As Boolean
            Return Not b = bool
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Or(assert As BooleanAssert, ifFailure As Object()) As Object
            Return If(assert, ifFailure(0), ifFailure(1))
        End Operator
    End Structure
End Namespace