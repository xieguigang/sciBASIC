#Region "Microsoft.VisualBasic::2043b8d04dba52ac16ef96b02f499c69, Microsoft.VisualBasic.Core\src\Language\Value\Variant.vb"

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

    '   Total Lines: 227
    '    Code Lines: 142
    ' Comment Lines: 59
    '   Blank Lines: 26
    '     File Size: 8.36 KB


    '     Class [Variant]
    ' 
    '         Properties: VA, VB
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: [TryCast], GetUnderlyingType
    ' 
    '         Sub: (+2 Overloads) Dispose, TryDispose
    ' 
    '         Operators: (+2 Overloads) <>, (+2 Overloads) =, (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates

Namespace Language

    ''' <summary>
    ''' Union type of <typeparamref name="A"/> and <typeparamref name="B"/>
    ''' </summary>
    ''' <typeparam name="A"></typeparam>
    ''' <typeparam name="B"></typeparam>
    Public Class [Variant](Of A, B) : Inherits Value(Of Object)
        Implements IDisposable

        Private disposedValue As Boolean

        ''' <summary>
        ''' <see cref="System.Void"/> will be returns if the value data is nothing!
        ''' </summary>
        ''' <returns></returns>
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
        ''' <remarks>
        ''' direct cast of <see cref="Value"/> to <typeparamref name="A"/>
        ''' </remarks>
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
        ''' <remarks>
        ''' direct cast of <see cref="Value"/> to <typeparamref name="B"/>
        ''' </remarks>
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
        ''' <returns>
        ''' this function will returns nothing if the value that holds in 
        ''' this object is nothing
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function [TryCast](Of T)() As T
            If Value Is Nothing Then
                Return Nothing
            Else
                Try
                    Return DirectCast(Value, T)
                Catch ex As Exception
                    ' 20230214
                    '
                    ' ignores of the try cast error
                    ' when type can not be cast to 
                    ' target type T
                    ' just returns nothing
                    Return Nothing
                End Try
            End If
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Widening Operator CType(obj As [Variant](Of B, A)) As [Variant](Of A, B)
            Return New [Variant](Of A, B) With {
                .Value = obj.Value
            }
        End Operator

        ''' <summary>
        ''' 请注意Like是直接进行比较，不会比较继承关系链的？
        ''' </summary>
        ''' <param name="var">
        ''' 为空值的时候，仅当<paramref name="type"/>是Nothing的时候才会返回真
        ''' </param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this operator deal with the null reference error safely
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overloads Shared Operator Like(var As [Variant](Of A, B), type As Type) As Boolean
            If var Is Nothing Then
                Return type Is Nothing OrElse type Is GetType(System.Void)
            ElseIf type Is Nothing Then
                Return False
            Else
                Return var.GetUnderlyingType Is type
            End If
        End Operator

        Private Shared Sub TryDispose(obj As Object)
            If Not obj Is Nothing Then
                If obj.GetType.ImplementInterface(Of IDisposable) Then
                    Call DirectCast(obj, IDisposable).Dispose()
                End If
            End If
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call TryDispose(VA)
                    Call TryDispose(VB)
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
