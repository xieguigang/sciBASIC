Imports System.Runtime.CompilerServices

Namespace Language

    Public Class [Variant](Of A, B) : Inherits Value(Of Object)

        Public Overrides Function GetUnderlyingType() As Type
            If Value Is Nothing Then
                Return GetType(Void)
            Else
                Return Value.GetType
            End If
        End Function

        Public ReadOnly Property VA As A
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Value
            End Get
        End Property

        Public ReadOnly Property VB As B
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Value
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(obj As [Variant](Of A, B)) As Type
            Return obj.GetUnderlyingType
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(obj As [Variant](Of A, B)) As A
            Return obj.VA
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(obj As [Variant](Of A, B)) As B
            Return obj.VB
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(a As A) As [Variant](Of A, B)
            Return New [Variant](Of A, B) With {.Value = a}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(b As B) As [Variant](Of A, B)
            Return New [Variant](Of A, B) With {.Value = b}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator =(obj As [Variant](Of A, B), a As A) As Boolean
            Return obj.VA.Equals(a)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <>(obj As [Variant](Of A, B), a As A) As Boolean
            Return Not obj = a
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator =(obj As [Variant](Of A, B), b As B) As Boolean
            Return obj.VB.Equals(b)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <>(obj As [Variant](Of A, B), b As B) As Boolean
            Return Not obj = b
        End Operator
    End Class

    Public Class [Variant](Of A, B, C) : Inherits [Variant](Of A, B)

        Public ReadOnly Property VC As C
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Value
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(obj As [Variant](Of A, B, C)) As C
            Return obj.VC
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(obj As [Variant](Of A, B, C)) As A
            Return obj.VA
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(obj As [Variant](Of A, B, C)) As B
            Return obj.VB
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(c As C) As [Variant](Of A, B, C)
            Return New [Variant](Of A, B, C) With {.Value = c}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(a As A) As [Variant](Of A, B, C)
            Return New [Variant](Of A, B, C) With {.Value = a}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(b As B) As [Variant](Of A, B, C)
            Return New [Variant](Of A, B, C) With {.Value = b}
        End Operator
    End Class
End Namespace