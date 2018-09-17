#Region "Microsoft.VisualBasic::05e59390398e54608cf7ea8de00ebadf, Microsoft.VisualBasic.Core\Language\Value\Numeric\int.vb"

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

    '     Class int
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) CompareTo, Equals, (+2 Overloads) ToString
    '         Operators: (+2 Overloads) -, (+2 Overloads) /, (+2 Overloads) +, (+4 Overloads) <, <<
    '                    <=, (+4 Overloads) >, >=
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem

Namespace Language

    ''' <summary>
    ''' Alias of <see cref="Int32"/>
    ''' </summary>
    Public Class int : Inherits Value(Of Integer)
        Implements IComparable
        Implements IComparable(Of Integer)
        Implements IEquatable(Of Integer)
        Implements IFormattable

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(Optional x% = Scan0)
            Value = x
        End Sub

        Sub New()
            Call Me.New(0)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return Value
        End Function

        ''' <summary>
        ''' Compare <see cref="Int"/> or <see cref="Int32"/>
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Dim type As Type = obj.GetType

            If type.Equals(GetType(Integer)) Then
                Return Value.CompareTo(DirectCast(obj, Integer))
            ElseIf type.Equals(GetType(int)) Then
                Return Value.CompareTo(DirectCast(obj, int).Value)
            Else
                Throw New Exception($"Miss-match of type:  {GetType(int).FullName} --> {type.FullName}")
            End If
        End Function

        ''' <summary>
        ''' n &lt; value &lt;= n2
        ''' 假若n 大于value，则返回最大值，上面的表达式肯定不成立
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator <(n As Integer, x As int) As int
            If n >= x.Value Then
                Return New int(Integer.MaxValue)
            Else
                Return x
            End If
        End Operator

        ''' <summary>
        ''' ``x.value &lt; n``
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <(x As int, n As Integer) As Boolean
            Return x.Value < n
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <(n As Double, x As int) As Boolean
            Return n < x.Value
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >(n As Double, x As int) As Boolean
            Return n > x.Value
        End Operator

        ''' <summary>
        ''' ``x.value > n``
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >(x As int, n As Integer) As Boolean
            Return x.Value > n
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <=(x As int, n As Integer) As Boolean
            Return x.Value <= n
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator >=(x As int, n As Integer) As Boolean
            Return x.Value >= n
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >(n As Integer, x As int) As int
            Return x
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator -(x As int, n As Integer) As int
            x.Value -= n
            Return x
        End Operator

        ''' <summary>
        ''' 正常的减法四则运算
        ''' </summary>
        ''' <param name="x%"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator -(x%, n As int) As Integer
            Return x - n.Value
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator /(x As int, b As Integer) As Double
            Return x.Value / b
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator /(x As Integer, b As int) As Double
            Return x / b.Value
        End Operator

        ''' <summary>
        ''' 必须要overloads这个方法，否则会出现无法将Value(Of Integer)转换为int的错误
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(n As Integer) As int
            Return New int(n)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(n As int) As Double
            Return CDbl(n.Value)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(n As int) As Integer
            Return n.Value
        End Operator

        ''' <summary>
        ''' Auto increment value with step 1 and then returns the previous value.
        ''' (自增1然后返回之前的值)
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(x As int) As Integer
            Dim i As Integer = x.Value
            x.Value += 1
            Return i
        End Operator

        ''' <summary>
        ''' 位移<paramref name="n"/>个单位然后返回位移之后的结果值
        ''' 
        ''' 对于<see cref="int"/>类型而言，其更加侧重于迭代器中的位移，所以这个加法运算是符合
        ''' ```vbnet
        ''' x += n
        ''' ```
        ''' 
        ''' 但是对于<see cref="float"/>类型而言，其更加侧重于模型计算，所以其加法不符合上述的语法，
        ''' 不会修改源变量的值，返回的是一个单纯的<see cref="Double"/>值类型
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n%"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(x As int, n%) As int
            x.Value += n
            Return x
        End Operator

        'Public Overloads Shared Operator =(x As int, a As Integer) As int

        'End Operator

        Public Shared Operator >(source As IEnumerable, handle As int) As Boolean
            Dim file As FileHandle = FileHandles.__getHandle(handle.Value)
            Return IOHandler.DefaultHandle()(source, file.FileName, file.encoding)
        End Operator

        Public Shared Operator <(source As IEnumerable, handle As int) As Boolean
            Throw New NotSupportedException
        End Operator

        ''' <summary>
        ''' p的值增加x，然后返回之前的值
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator <<(p As int, x%) As Integer
            Dim i As Integer = p.Value
            p.Value += x
            Return i
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CompareTo(other As Integer) As Integer Implements IComparable(Of Integer).CompareTo
            Return Value.CompareTo(other)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Equals(other As Integer) As Boolean Implements IEquatable(Of Integer).Equals
            Return Value.Equals(other)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function ToString(format As String, formatProvider As IFormatProvider) As String Implements IFormattable.ToString
            Return Value.ToString(format, formatProvider)
        End Function
    End Class
End Namespace
