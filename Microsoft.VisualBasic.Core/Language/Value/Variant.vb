#Region "Microsoft.VisualBasic::b72f0418d1865f92e931ebec63005007, Microsoft.VisualBasic.Core\Language\Value\Variant.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class [Variant]
    ' 
    '         Properties: VA, VB
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: [TryCast], GetUnderlyingType
    '         Operators: (+2 Overloads) <>, (+2 Overloads) =, (+2 Overloads) Like
    ' 
    '     Class [Variant]
    ' 
    '         Properties: VC
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Operators: (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language

    ''' <summary>
    ''' Union type of <typeparamref name="A"/> and <typeparamref name="B"/>
    ''' </summary>
    ''' <typeparam name="A"></typeparam>
    ''' <typeparam name="B"></typeparam>
    Public Class [Variant](Of A, B) : Inherits Value(Of Object)

        Public Overrides Function GetUnderlyingType() As Type
            If Value Is Nothing Then
                Return GetType(Void)
            Else
                Return Value.GetType
            End If
        End Function

        ''' <summary>
        ''' TryCast to <typeparamref name="A"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property VA As A
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If Me Like GetType(A) OrElse GetUnderlyingType.IsInheritsFrom(GetType(A)) Then
                    Return Value
                Else
                    Return Nothing
                End If
            End Get
        End Property

        ''' <summary>
        ''' TryCast to <typeparamref name="B"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property VB As B
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If Me Like GetType(B) OrElse GetUnderlyingType.IsInheritsFrom(GetType(B)) Then
                    Return Value
                Else
                    Return Nothing
                End If
            End Get
        End Property

        <DebuggerStepThrough>
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

        ''' <summary>
        ''' do direct cast of <see cref="Value"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        <DebuggerStepThrough>
        Public Function [TryCast](Of T)() As T
            Return DirectCast(Value, T)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Widening Operator CType(obj As [Variant](Of A, B)) As Type
            Return obj.GetUnderlyingType
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Narrowing Operator CType(obj As [Variant](Of A, B)) As A
            Return obj.VA
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Narrowing Operator CType(obj As [Variant](Of A, B)) As B
            Return obj.VB
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Widening Operator CType(a As A) As [Variant](Of A, B)
            Return New [Variant](Of A, B) With {.Value = a}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Widening Operator CType(b As B) As [Variant](Of A, B)
            Return New [Variant](Of A, B) With {.Value = b}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Operator =(obj As [Variant](Of A, B), a As A) As Boolean
            Return obj.VA.Equals(a)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Operator <>(obj As [Variant](Of A, B), a As A) As Boolean
            Return Not obj = a
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Operator =(obj As [Variant](Of A, B), b As B) As Boolean
            Return obj.VB.Equals(b)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Operator <>(obj As [Variant](Of A, B), b As B) As Boolean
            Return Not obj = b
        End Operator

        ''' <summary>
        ''' 请注意Like是直接进行比较，不会比较继承关系链的？
        ''' </summary>
        ''' <param name="var"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Operator Like(var As [Variant](Of A, B), type As Type) As Boolean
            Return var.GetUnderlyingType Is type
        End Operator
    End Class

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
