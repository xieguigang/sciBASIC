#Region "Microsoft.VisualBasic::b9525e9664556a1340bf245a9d2a538f, Microsoft.VisualBasic.Core\src\Language\Value\Numeric\Numeric.vb"

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

    '   Total Lines: 190
    '    Code Lines: 85
    ' Comment Lines: 80
    '   Blank Lines: 25
    '     File Size: 6.44 KB


    '     Module Numeric
    ' 
    '         Function: Equals, GreaterThan, GreaterThanOrEquals, LessThan, LessThanOrEquals
    '                   MaxIndex, MinIndex, NextInteger, (+2 Overloads) Reverse, ToUInt32
    '                   ToUInt64
    ' 
    '     Class Precise
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

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

#Region "VB type char helper"
        Public Const yes! = 1
        Public Const no! = 0
#End Region

        ''' <summary>
        ''' *<see cref="Numeric.MaxIndex"/>* The max element its index in the source collection.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns>
        ''' -1 means empty collection
        ''' </returns>
        <Extension>
        Public Function MaxIndex(Of T As IComparable)(source As IEnumerable(Of T)) As Integer
            Dim i As Integer = 0
            Dim max As T
            Dim maxInd As Integer = 0
            Dim loops As T()

            If source Is Nothing Then
                Return -1
            Else
                loops = If(TypeOf source Is T(), DirectCast(source, T()), source.ToArray)

                If loops.Length = 0 Then
                    Return -1
                Else
                    max = loops(0)
                End If
            End If

            For Each x As T In loops.Skip(1)
                i += 1

                If x.CompareTo(max) > 0 Then
                    max = x
                    maxInd = i
                End If
            Next

            Return maxInd
        End Function

        <Extension>
        Public Function MinIndex(Of T As IComparable)(source As IEnumerable(Of T)) As Integer
            Dim i As Integer
            Dim min As T = source.First
            Dim minInd As Integer = 0

            For Each x As T In source.Skip(1)
                i += 1

                If x.CompareTo(min) < 0 Then
                    min = x
                    minInd = i
                End If
            Next

            Return minInd
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
        ''' a > b
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <Extension> Public Function GreaterThan(Of T As IComparable)(a As T, b As T) As Boolean
            Return a.CompareTo(b) > 0
        End Function

        ''' <summary>
        ''' ``<paramref name="a"/> &lt;= <paramref name="b"/>``
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
        <Extension> Public Function NextInteger(rnd As Random, max As Integer) As i32
            Return New i32(rnd.Next(max))
        End Function

        Public Function ToUInt32(value As Single) As UInteger
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            Return BitConverter.ToUInt32(bytes, 0)
        End Function

        Public Function ToUInt64(value As Double) As ULong
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            Return BitConverter.ToUInt64(bytes, 0)
        End Function

        Public Function Reverse(value As UInteger) As UInteger
            Dim reversed As UInteger = (value And &HFF) << 24 Or (value And &HFF00) << 8 Or (value And &HFF0000) >> 8 Or (value And &HFF000000UI) >> 24
            Return reversed
        End Function

        Public Function Reverse(value As Integer) As Integer
            Dim uvalue As UInteger = CUInt(value)
            Dim reversed As UInteger = Reverse(uvalue)
            Return CInt(reversed)
        End Function
    End Module

    Public Class Precise : Inherits Value(Of Decimal)

    End Class
End Namespace
