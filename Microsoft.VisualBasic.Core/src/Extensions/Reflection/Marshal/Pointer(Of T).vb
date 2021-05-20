#Region "Microsoft.VisualBasic::59bb251d4d5fe61aa37da49576a3ca5e, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Marshal\Pointer(Of T).vb"

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

    '     Class Pointer
    ' 
    '         Properties: Current, EndRead, Length, NullEnd, Position
    '                     RawBuffer, UBound
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: GetLeftsAll, MoveNext, PeekNext, stackalloc, ToString
    '         Operators: (+2 Overloads) -, (+2 Overloads) +, <<, (+2 Overloads) <=, <>
    '                    =, (+2 Overloads) >=, >>, (+2 Overloads) IsFalse, (+2 Overloads) IsTrue
    '                    (+2 Overloads) Not
    ' 
    '     Structure SwapHelper
    ' 
    '         Sub: Swap
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq

Namespace Emit.Marshal

    ''' <summary>
    ''' <see cref="Array"/> index helper.(在数组的索引基础上封装了数组本身)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Pointer(Of T) : Inherits DataStructures.Pointer(Of T)

        Protected buffer As T()

        ''' <summary>
        ''' <see cref="Position"/> -> its current value
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 当前的位置是指相对于当前的位置offset为0的位置就是当前的位置
        ''' </remarks>
        Public Property Current As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Value(Scan0)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Protected Friend Set(value As T)
                Me.Value(Scan0) = value
            End Set
        End Property

        ''' <summary>
        ''' Memory block size
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Length
            End Get
        End Property

        ''' <summary>
        ''' 返回指定维度的一个数组，最高可用的下标。
        ''' </summary>
        ''' <returns>
        ''' <see cref="Integer"/>。 指定维度的下标可以包含的最大值。 如果 Array 只有一个元素， UBound ，则返回 0。 如果 Array 不包含任何元素，例如，如果它是零长度字符串，
        ''' UBound 返回-1。</returns>
        Public ReadOnly Property UBound As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Information.UBound(buffer)
            End Get
        End Property

        ''' <summary>
        ''' 相对于当前的指针的位置而言的
        ''' </summary>
        ''' <param name="p">相对于当前的位置的offset偏移量</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个属性并不会移动当前的指针的位置
        ''' </remarks>
        Default Public Property Value(p As Integer) As T
            Get
                p += index

                If p < 0 OrElse p >= buffer.Length Then
                    Return Nothing
                Else
                    Return buffer(p)
                End If
            End Get
            Set(value As T)
                p += index

                If p < 0 OrElse p >= buffer.Length Then
                    Throw New MemberAccessException(p & " reference to invalid memory region!")
                Else
                    buffer(p) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Raw memory of this pointer
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RawBuffer As T()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer
            End Get
        End Property

        Public ReadOnly Property NullEnd(Optional offset As Integer = 0) As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return index >= (buffer.Length - 1 - offset)
            End Get
        End Property

        ''' <summary>
        ''' Is read to end?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EndRead As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return index >= buffer.Length
            End Get
        End Property

        ''' <summary>
        ''' Current read position
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Position As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return index
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="array"></param>
        ''' <remarks>
        ''' 为了保持原来的对象引用，在这里就不进行ToArray数组复制来打破这种引用关系了
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(ByRef array As T())
            buffer = array
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(array As List(Of T))
            buffer = array.ToArray
        End Sub

        ''' <summary>
        ''' Create a collection wrapper from a <paramref name="source"/> buffer.
        ''' </summary>
        ''' <param name="source">The collection source buffer</param>
        Sub New(source As IEnumerable(Of T))
            buffer = source.ToArray
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' Pointer move to next and then returns is <see cref="EndRead"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function MoveNext() As Boolean
            index += 1
            Return Not EndRead
        End Function

        ''' <summary>
        ''' 获取得到下一个对象而不移动当前的内部指针
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function PeekNext() As T
            Return buffer(index + 1)
        End Function

        Public Overrides Function ToString() As String
            Return $"*{GetType(T).Name}: ({index}) {Current}"
        End Function

        ''' <summary>
        ''' 获取当前指针位置后面的所有元素
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLeftsAll() As T()
            Return buffer.Skip(index).ToArray
        End Function

        '''' <summary>
        '''' 前移<paramref name="offset"/>个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置
        '''' </summary>
        '''' <param name="p"></param>
        '''' <param name="offset"></param>
        '''' <returns></returns>
        'Public Overloads Shared Operator <=(p As Pointer(Of T), offset As Integer) As T
        '    Return p(-offset)
        'End Operator

        '''' <summary>
        '''' 后移<paramref name="offset"/>个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置
        '''' </summary>
        '''' <param name="p"></param>
        '''' <param name="offset"></param>
        '''' <returns></returns>
        'Public Overloads Shared Operator >=(p As Pointer(Of T), offset As Integer) As T
        '    Return p(offset)
        'End Operator

        Public Overloads Shared Operator IsTrue(p As Pointer(Of T)) As Boolean
            Return Not p.EndRead
        End Operator

        Public Overloads Shared Operator IsFalse(p As Pointer(Of T)) As Boolean
            Return p.EndRead
        End Operator

        Public Overloads Shared Operator Not(p As Pointer(Of T)) As Boolean
            Return Not p.EndRead
        End Operator

        ''' <summary>
        ''' 获取得到当前的元素
        ''' </summary>
        ''' <param name="p"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(p As Pointer(Of T)) As T
            Return p.Current
        End Operator

        ''' <summary>
        ''' 前移<paramref name="offset"/>个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <<(p As Pointer(Of T), offset As Integer) As T
            Return p(-offset)
        End Operator

        ''' <summary>
        ''' 后移<paramref name="offset"/>个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator >>(p As Pointer(Of T), offset As Integer) As T
            Return p(offset)
        End Operator

        Public Overloads Shared Widening Operator CType(raw As T()) As Pointer(Of T)
            Return New Pointer(Of T)(raw)
        End Operator

        ''' <summary>
        ''' Move steps and returns this pointer
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <param name="d"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(ptr As Pointer(Of T), d As Integer) As Pointer(Of T)
            ptr.index += d
            Return ptr
        End Operator

        ''' <summary>
        ''' move back current read position index
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <param name="d"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator -(ptr As Pointer(Of T), d As Integer) As Pointer(Of T)
            ptr.index -= d
            Return ptr
        End Operator

        ''' <summary>
        ''' Pointer move to next and then returns the previous value
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator +(ptr As Pointer(Of T)) As SeqValue(Of T)
            Dim i% = ptr.index

            If ptr.EndRead Then
                Return Nothing
            Else
                ' 2019-04-22 在这里会需要将index的位移放在endread判断的后面, 因为
                ' 假设当前的读取位置是最后一个元素的话,则会因为在endread之前已经移动了指针导致
                ' 被判断为endread,从而在前面的代码提前返回,出现不应该存在的空值bug 

                ' move pointer forward
                ptr.index += 1

                Return New SeqValue(Of T) With {
                    .i = i,
                    .value = ptr.buffer(i)
                }
            End If
        End Operator

        ''' <summary>
        ''' The stack trace back operator.
        ''' (指针的位置往回移动一个单位，然后返回原来的位置的元素的值)
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator -(ptr As Pointer(Of T)) As SeqValue(Of T)
            Dim i% = ptr.index

            ' move pointer back one unit.
            ptr.index -= 1

            Return New SeqValue(Of T) With {
                .i = i,
                .value = ptr.buffer.ElementAtOrDefault(i, Nothing)
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <=(a As Pointer(Of T), b As Pointer(Of T)) As SwapHelper(Of T)
            Return New SwapHelper(Of T) With {.a = a, .b = b}
        End Operator

        Public Overloads Shared Operator >=(a As Pointer(Of T), b As Pointer(Of T)) As SwapHelper(Of T)
            Throw New NotSupportedException
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <=(a As Pointer(Of T), b As Integer) As SwapHelper(Of T)
            Return New SwapHelper(Of T) With {.a = a, .i = b}
        End Operator

        Public Overloads Shared Operator >=(a As Pointer(Of T), b As Integer) As SwapHelper(Of T)
            Throw New NotSupportedException
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function stackalloc(len As Integer) As Pointer(Of T)
            Return New Pointer(Of T)(New T(len - 1) {})
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(p As Pointer(Of T), count%) As Boolean
            Return p.Length = count
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(p As Pointer(Of T), count%) As Boolean
            Return Not p = count
        End Operator
    End Class

    Public Structure SwapHelper(Of T)

        Public a As Pointer(Of T)
        Public b As Pointer(Of T)
        Public i As Integer

        Public Sub Swap()
            Dim tmp As T = a.Current

            If b Is Nothing Then
                a.Current = a(i)
                a(i) = tmp
            Else
                a.Current = b.Current
                b.Current = tmp
            End If
        End Sub
    End Structure
End Namespace
