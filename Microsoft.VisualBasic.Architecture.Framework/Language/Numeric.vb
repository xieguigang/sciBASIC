#Region "Microsoft.VisualBasic::40096691d3aba60411e91f03f7971883, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Language\Numeric.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace Language

    ''' <summary>
    ''' Defines a generalized type-specific comparison method that a value type or class
    ''' implements to order or sort its instances.
    ''' </summary>
    ''' <remarks>
    '''
    ''' Summary:
    '''
    '''     Compares the current instance with another object of the same type and returns
    '''     an integer that indicates whether the current instance precedes, follows, or
    '''     occurs in the same position in the sort order as the other object.
    '''
    ''' Returns:
    '''
    '''     A value that indicates the relative order of the objects being compared. The
    '''     return value has these meanings:
    '''
    '''     1. Value Meaning Less than zero
    '''        This instance precedes obj in the sort order.
    '''
    '''     2. Zero
    '''        This instance occurs in the same position in the sort order as obj.
    '''
    '''     3. Greater than zero
    '''        This instance follows obj in the sort order.
    '''
    ''' Exceptions:
    '''
    '''   T:System.ArgumentException:
    '''     obj is not the same type as this instance.
    ''' </remarks>
    Public Module Numeric

        ''' <summary>
        ''' The max element its index in the source collection.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MaxIndex(Of T As IComparable)(source As IEnumerable(Of T)) As Integer
            Dim i As Integer
            Dim max As T = source.First
            Dim maxInd As Integer = 0

            For Each x As T In source.Skip(1)
                i += 1

                If x.CompareTo(max) > 0 Then
                    max = x
                    maxInd = i
                End If
            Next

            Return maxInd
        End Function

        ''' <summary>
        ''' =
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Function Equals(Of T As IComparable)(a As T, b As T) As Boolean
            Return a.CompareTo(b) = 0
        End Function

        ''' <summary>
        ''' &lt;
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <Extension> Public Function LessThan(Of T As IComparable)(a As T, b As T) As Boolean
            Return a.CompareTo(b) < 0
        End Function

        ''' <summary>
        ''' >
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <Extension> Public Function GreaterThan(Of T As IComparable)(a As T, b As T) As Boolean
            Return a.CompareTo(b) > 0
        End Function

        ''' <summary>
        ''' &lt;=
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <Extension> Public Function LessThanOrEquals(Of T As IComparable)(a As T, b As T) As Boolean
            Return a.LessThan(b) OrElse Equals(a, b)
        End Function

        ''' <summary>
        ''' =>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <Extension> Public Function GreaterThanOrEquals(Of T As IComparable)(a As T, b As T) As Boolean
            Return a.GreaterThan(b) OrElse Equals(a, b)
        End Function

        ''' <summary>
        ''' <see cref="Random"/> get next integer in the specific range <paramref name="max"/>
        ''' </summary>
        ''' <param name="rnd"></param>
        ''' <param name="max"></param>
        ''' <returns></returns>
        <Extension> Public Function NextInteger(rnd As Random, max As Integer) As Int
            Return New Int(rnd.Next(max))
        End Function
    End Module

    ''' <summary>
    ''' Alias of <see cref="Int32"/>
    ''' </summary>
    Public Class int : Inherits Value(Of Integer)
        Implements IComparable

        Sub New(Optional x As Integer = Scan0)
            value = x
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function

        ''' <summary>
        ''' Compare <see cref="Int"/> or <see cref="Int32"/>
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Dim type As Type = obj.GetType

            If type.Equals(GetType(Integer)) Then
                Return value.CompareTo(DirectCast(obj, Integer))
            ElseIf type.Equals(GetType(int)) Then
                Return value.CompareTo(DirectCast(obj, int).value)
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
            If n >= x.value Then
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
        Public Shared Operator <(x As int, n As Integer) As Boolean
            Return x.value < n
        End Operator

        ''' <summary>
        ''' ``x.value > n``
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Shared Operator >(x As int, n As Integer) As Boolean
            Return x.value > n
        End Operator

        Public Overloads Shared Operator <=(x As int, n As Integer) As Boolean
            Return x.value <= n
        End Operator

        Public Overloads Shared Operator >=(x As int, n As Integer) As Boolean
            Return x.value >= n
        End Operator

        Public Shared Operator >(n As Integer, x As int) As int
            Return x
        End Operator

        ''' <summary>
        ''' 必须要overloads这个方法，否则会出现无法将Value(Of Integer)转换为int的错误
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Overloads Shared Widening Operator CType(n As Integer) As int
            Return New int(n)
        End Operator

        ''' <summary>
        ''' 自增1然后返回之前的值
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(x As int) As Integer
            Dim i As Integer = x.value
            x.value += 1
            Return i
        End Operator

        Public Shared Operator >(source As IEnumerable, handle As int) As Boolean
            Dim file As FileHandle = FileHandles.__getHandle(handle.value)
            Return CollectionIO.DefaultHandle()(source, file.FileName, file.encoding)
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
        Public Shared Operator <<(p As int, x As Integer) As Integer
            Dim i As Integer = p.value
            p.value += x
            Return i
        End Operator
    End Class

    ''' <summary>
    ''' <see cref="System.Double"/>
    ''' </summary>
    Public Class float : Inherits Value(Of Double)
        Implements IComparable

        Sub New(x As Double)
            value = x
        End Sub

        Sub New()
            Me.New(0R)
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Dim type As Type = obj.GetType

            If type.Equals(GetType(Double)) Then
                Return value.CompareTo(DirectCast(obj, Double))
            ElseIf type.Equals(GetType(float)) Then
                Return value.CompareTo(DirectCast(obj, float).value)
            Else
                Throw New Exception($"Miss-match of type:  {GetType(float).FullName} --> {type.FullName}")
            End If
        End Function

        ''' <summary>
        ''' n &lt; value &lt;= n2
        ''' 假若n 大于value，则返回最大值，上面的表达式肯定不成立
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator <(n As Double, x As float) As float
            If n >= x.value Then
                Return New float(Double.MaxValue)
            Else
                Return x
            End If
        End Operator

        Public Shared Operator *(n As Double, x As float) As Double
            Return n * x.value
        End Operator

        Public Overloads Shared Widening Operator CType(x As Double) As float
            Return New float(x)
        End Operator

        Public Overloads Shared Operator <=(x As float, n As Double) As Boolean
            Return x.value <= n
        End Operator

        Public Overloads Shared Operator >=(x As float, n As Double) As Boolean
            Return x.value >= n
        End Operator

        Public Shared Operator >(n As Double, x As float) As float
            Return x
        End Operator
    End Class
End Namespace
