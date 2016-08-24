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
    Public Class Int : Implements IComparable

        Public Property value As Integer

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
            ElseIf type.Equals(GetType(Int)) Then
                Return value.CompareTo(DirectCast(obj, Int).value)
            Else
                Throw New Exception($"Miss-match of type:  {GetType(Int).FullName} --> {type.FullName}")
            End If
        End Function

        ''' <summary>
        ''' n &lt; value &lt;= n2
        ''' 假若n 大于value，则返回最大值，上面的表达式肯定不成立
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator <(n As Integer, x As Int) As Int
            If n >= x.value Then
                Return New Int(Integer.MaxValue)
            Else
                Return x
            End If
        End Operator

        Public Shared Operator <=(x As Int, n As Integer) As Boolean
            Return x.value <= n
        End Operator

        Public Shared Operator >=(x As Int, n As Integer) As Boolean
            Return x.value >= n
        End Operator

        Public Shared Operator >(n As Integer, x As Int) As Int
            Return x
        End Operator

        Public Shared Widening Operator CType(n As Integer) As Int
            Return New Int(n)
        End Operator

        Public Shared Narrowing Operator CType(n As Int) As Integer
            Return n.value
        End Operator

        Public Shared Operator +(x As Int) As Integer
            Dim i As Integer = x.value
            x.value += 1
            Return i
        End Operator

        Public Shared Operator >(source As IEnumerable, handle As Int) As Boolean
            Dim file As FileHandle = FileHandles.__getHandle(handle.value)
            Return CollectionIO.DefaultHandle()(source, file.FileName, file.encoding)
        End Operator

        Public Shared Operator <(source As IEnumerable, handle As Int) As Boolean
            Throw New NotSupportedException
        End Operator
    End Class

    Public Structure Float
        Implements IComparable

        Dim value As Double

        Sub New(x As Double)
            value = x
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Dim type As Type = obj.GetType

            If type.Equals(GetType(Double)) Then
                Return value.CompareTo(DirectCast(obj, Double))
            ElseIf type.Equals(GetType(Float)) Then
                Return value.CompareTo(DirectCast(obj, Float).value)
            Else
                Throw New Exception($"Miss-match of type:  {GetType(Float).FullName} --> {type.FullName}")
            End If
        End Function

        ''' <summary>
        ''' n &lt; value &lt;= n2
        ''' 假若n 大于value，则返回最大值，上面的表达式肯定不成立
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator <(n As Double, x As Float) As Float
            If n >= x.value Then
                Return New Float(Double.MaxValue)
            Else
                Return x
            End If
        End Operator

        Public Shared Operator <=(x As Float, n As Double) As Boolean
            Return x.value <= n
        End Operator

        Public Shared Operator >=(x As Float, n As Double) As Boolean
            Return x.value >= n
        End Operator

        Public Shared Operator >(n As Double, x As Float) As Float
            Return x
        End Operator
    End Structure
End Namespace
