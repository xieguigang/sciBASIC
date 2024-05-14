#Region "Microsoft.VisualBasic::7c6335af5f985a73d4262c05d072226e, Microsoft.VisualBasic.Core\src\Language\Value\TripleVariant.vb"

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


    ' Code Statistics:

    '   Total Lines: 74
    '    Code Lines: 60
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 2.47 KB


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
