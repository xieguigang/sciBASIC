#Region "Microsoft.VisualBasic::150189cef7a636076806abb98e97c3e6, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Reflection\Marshal\Pointer(Of T).vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ComponentModel

Namespace Emit.Marshal

    Public Class Pointer(Of T) : Inherits DataStructures.Pointer(Of T)

        Protected __innerRaw As T()

        ''' <summary>
        ''' <see cref="Pointer"/> -> its current value
        ''' </summary>
        ''' <returns></returns>
        Public Property Current As T
            Get
                Return Value(Scan0)  ' 当前的位置是指相对于当前的位置offset为0的位置就是当前的位置
            End Get
            Protected Friend Set(value As T)
                Me.Value(Scan0) = value
            End Set
        End Property

        ''' <summary>
        ''' Memory block size
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            Get
                Return __innerRaw.Length
            End Get
        End Property

        Public ReadOnly Property UBound As Integer
            Get
                Return Information.UBound(__innerRaw)
            End Get
        End Property

        ''' <summary>
        ''' 相对于当前的指针的位置而言的
        ''' </summary>
        ''' <param name="p">相对于当前的位置的offset偏移量</param>
        ''' <returns></returns>
        Default Public Property Value(p As Integer) As T
            Get
                p += __index

                If p < 0 OrElse p >= __innerRaw.Length Then
                    Return Nothing
                Else
                    Return __innerRaw(p)
                End If
            End Get
            Set(value As T)
                p += __index

                If p < 0 OrElse p >= __innerRaw.Length Then
                    Throw New MemberAccessException(p & " reference to invalid memory region!")
                Else
                    __innerRaw(p) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Raw memory of this pointer
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Raw As T()
            Get
                Return __innerRaw
            End Get
        End Property

        Public ReadOnly Property NullEnd(Optional offset As Integer = 0) As Boolean
            Get
                Return __index >= (__innerRaw.Length - 1 - offset)
            End Get
        End Property

        ''' <summary>
        ''' Is read to end?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EndRead As Boolean
            Get
                Return __index >= __innerRaw.Length
            End Get
        End Property

        ''' <summary>
        ''' Current read position
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Pointer As Integer
            Get
                Return __index
            End Get
        End Property

        Sub New(ByRef array As T())
            __innerRaw = array
        End Sub

        Sub New(array As List(Of T))
            __innerRaw = array.ToArray
        End Sub

        Sub New(source As IEnumerable(Of T))
            __innerRaw = source.ToArray
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"* {GetType(T).Name} + {__index} --> {Current}  // {Scan0.ToString}"
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

        Public Overloads Shared Narrowing Operator CType(p As Pointer(Of T)) As T
            Return p.Current
        End Operator

        ''' <summary>
        ''' 前移<paramref name="offset"/>个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator <(p As Pointer(Of T), offset As Integer) As T
            Return p(-offset)
        End Operator

        ''' <summary>
        ''' 后移<paramref name="offset"/>个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator >(p As Pointer(Of T), offset As Integer) As T
            Return p(offset)
        End Operator

        Public Overloads Shared Widening Operator CType(raw As T()) As Pointer(Of T)
            Return New Pointer(Of T)(raw)
        End Operator

        Public Overloads Shared Operator +(ptr As Pointer(Of T), d As Integer) As Pointer(Of T)
            ptr.__index += d
            Return ptr
        End Operator

        Public Overloads Shared Operator -(ptr As Pointer(Of T), d As Integer) As Pointer(Of T)
            ptr.__index -= d
            Return ptr
        End Operator

        ''' <summary>
        ''' Pointer move to next and then returns is <see cref="EndRead"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function MoveNext() As Boolean
            __index += 1
            Return Not EndRead
        End Function

        ''' <summary>
        ''' Pointer move to next and then returns the previous value
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(ptr As Pointer(Of T)) As T
            Dim i As Integer = ptr.__index
            ptr.__index += 1
            Return ptr.__innerRaw(i)
        End Operator

        ''' <summary>
        ''' 指针的位置往回移动一个单位，然后返回原来的位置的元素的值
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator -(ptr As Pointer(Of T)) As T
            Dim i As Integer = ptr.__index
            ptr.__index -= 1
            Return ptr.__innerRaw(i)
        End Operator

        Public Overloads Shared Operator <=(a As Pointer(Of T), b As Pointer(Of T)) As SwapHelper(Of T)
            Return New SwapHelper(Of T) With {.a = a, .b = b}
        End Operator

        Public Overloads Shared Operator >=(a As Pointer(Of T), b As Pointer(Of T)) As SwapHelper(Of T)
            Throw New NotSupportedException
        End Operator

        Public Overloads Shared Operator <=(a As Pointer(Of T), b As Integer) As SwapHelper(Of T)
            Return New SwapHelper(Of T) With {.a = a, .i = b}
        End Operator

        Public Overloads Shared Operator >=(a As Pointer(Of T), b As Integer) As SwapHelper(Of T)
            Throw New NotSupportedException
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
