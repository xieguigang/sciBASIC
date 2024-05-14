#Region "Microsoft.VisualBasic::ef16285bcf1dd1e43c4720a98796fe49, Microsoft.VisualBasic.Core\src\Language\Value\Pointer.vb"

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

    '   Total Lines: 143
    '    Code Lines: 71
    ' Comment Lines: 52
    '   Blank Lines: 20
    '     File Size: 4.74 KB


    '     Class Pointer
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    '         Operators: (+2 Overloads) -, (+4 Overloads) +, <, <<, <=
    '                    >, >=
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language

    ''' <summary>
    ''' Type of <see cref="Int32"/> pointer class to the <see cref="Array"/> class.
    ''' (<see cref="Int32"/>类型，一般用来进行数组操作的)
    ''' </summary>
    Public Class Pointer

        ''' <summary>
        ''' Current read position
        ''' </summary>
        Protected index As Integer
        ''' <summary>
        ''' 指针移动的步进值
        ''' </summary>
        Protected ReadOnly [step] As Integer

        ''' <summary>
        ''' Construct a pointer class and then assign a initial <see cref="Int32"/> value.
        ''' (构造一个指针对象，并且赋值其初始值)
        ''' </summary>
        ''' <param name="n">The initial value.</param>
        Sub New(n As Integer)
            index = n
            [step] = 1
        End Sub

        Public Sub New(n As Integer, [step] As Integer)
            index = n
            Me.step = [step]
        End Sub

        ''' <summary>
        ''' Creates a new <see cref="Integer"/> type pointer object in VisualBasic with its initial value is ZERO.(构造一个初始值为零的整形数指针对象)
        ''' </summary>
        Sub New()
            Call Me.New(Scan0)
        End Sub

        Public Overrides Function ToString() As String
            Return index
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(n As Integer) As Pointer
            Return New Pointer(n)
        End Operator

        Public Shared Narrowing Operator CType(n As Pointer) As Integer
            Return n.index
        End Operator

        Public Overloads Shared Operator +(n As Pointer, x As Integer) As Pointer
            n.index += x
            Return n
        End Operator

        Public Overloads Shared Operator +(x As Integer, n As Pointer) As Pointer
            n.index += x
            Return n
        End Operator

        Public Overloads Shared Operator +(x As Pointer, n As Pointer) As Pointer
            Return New Pointer(n.index + x.index)
        End Operator

        ''' <summary>
        ''' ``<see cref="index"/> &lt; <paramref name="n"/>``
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator <(x As Pointer, n As Integer) As Boolean
            Return x.index < n
        End Operator

        Public Overloads Shared Operator >(x As Pointer, n As Integer) As Boolean
            Return x.index > n
        End Operator

        Public Shared Operator -(x As Pointer, n As Integer) As Integer
            Dim p As Integer = x.index
            x.index -= n
            Return p
        End Operator

        ''' <summary>
        ''' 移动n，然后返回之前的数值
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Shared Operator <<(x As Pointer, n As Integer) As Integer
            Dim value As Integer = x.index
            x.index += n
            Return value
        End Operator

        ''' <summary>
        ''' Automatically increasing self +1 and then returns the previous value.(自增1，然后返回之前的数值)
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(x As Pointer) As Integer
            Dim p As Integer = x.index
            x.index += x.step
            Return p
        End Operator

        ''' <summary>
        ''' 自减1，然后返回之前的数值
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator -(x As Pointer) As Integer
            Dim p As Integer = x.index
            x.index -= x.step
            Return p
        End Operator

        ''' <summary>
        ''' Less than or equals
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Shared Operator <=(x As Pointer, n As Integer) As Boolean
            Return x.index <= n
        End Operator

        ''' <summary>
        ''' Greater than or equals
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Shared Operator >=(x As Pointer, n As Integer) As Boolean
            Return x.index >= n
        End Operator
    End Class
End Namespace
