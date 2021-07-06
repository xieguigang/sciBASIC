Imports System.Runtime.CompilerServices

Namespace Language

    Public Class [Variant](Of A, B, C) : Inherits [Variant](Of A, B)

        Public ReadOnly Property VC As C
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Value
            End Get
        End Property

        Sub New()
        End Sub

        <DebuggerStepThrough>
        Sub New(a As A)
            Value = a
        End Sub

        <DebuggerStepThrough>
        Sub New(b As B)
            Value = b
        End Sub

        <DebuggerStepThrough>
        Sub New(c As C)
            Value = c
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Narrowing Operator CType(obj As [Variant](Of A, B, C)) As C
            Return obj.VC
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Narrowing Operator CType(obj As [Variant](Of A, B, C)) As A
            Return obj.VA
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Narrowing Operator CType(obj As [Variant](Of A, B, C)) As B
            Return obj.VB
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Widening Operator CType(c As C) As [Variant](Of A, B, C)
            Return New [Variant](Of A, B, C) With {.Value = c}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Widening Operator CType(a As A) As [Variant](Of A, B, C)
            Return New [Variant](Of A, B, C) With {.Value = a}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Widening Operator CType(b As B) As [Variant](Of A, B, C)
            Return New [Variant](Of A, B, C) With {.Value = b}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Operator Like(var As [Variant](Of A, B, C), type As Type) As Boolean
            Return var.GetUnderlyingType Is type
        End Operator
    End Class
End Namespace