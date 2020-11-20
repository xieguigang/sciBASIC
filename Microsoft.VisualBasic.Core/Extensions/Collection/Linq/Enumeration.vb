#Region "Microsoft.VisualBasic::3c4b5e020dd25b44ba41feb2c4e20222, Microsoft.VisualBasic.Core\Extensions\Collection\Linq\Enumeration.vb"

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

    '     Interface Enumeration
    ' 
    '         Function: GenericEnumerator, GetEnumerator
    ' 
    '     Module EnumerationExtensions
    ' 
    '         Function: AsEnumerable, (+2 Overloads) AsObjectEnumerator
    '         Class Enumerator
    ' 
    '             Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Linq

    ''' <summary>
    ''' Exposes the enumerator, which supports a simple iteration over a collection of
    ''' a specified type.To browse the .NET Framework source code for this type, see
    ''' the Reference Source.
    ''' (使用这个的原因是系统自带的<see cref="IEnumerable(Of T)"/>在Xml序列化之中的支持不太友好，
    ''' 实现这个接口之后可以通过<see cref="EnumerationExtensions.AsEnumerable(Of T)(Enumeration(Of T))"/>
    ''' 拓展来转换为查询操作的数据源)
    ''' </summary>
    ''' <typeparam name="T">The type of objects to enumerate.This type parameter is covariant. That is, you
    ''' can use either the type you specified or any type that is more derived. For more
    ''' information about covariance and contravariance, see Covariance and Contravariance
    ''' in Generics.</typeparam>
    Public Interface Enumeration(Of T)

        ''' <summary>
        ''' Returns an enumerator that iterates through the collection.
        ''' </summary>
        ''' <returns>An enumerator that can be used to iterate through the collection.</returns>
        Function GenericEnumerator() As IEnumerator(Of T)

        ''' <summary>
        ''' Returns an enumerator that iterates through a collection.
        ''' </summary>
        ''' <returns>An System.Collections.IEnumerator object that can be used to iterate through
        ''' the collection.</returns>
        Function GetEnumerator() As IEnumerator
    End Interface

    <HideModuleName>
    Public Module EnumerationExtensions

        Private Class Enumerator(Of T) : Implements IEnumerable(Of T)

            Public Enumeration As Enumeration(Of T)

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
                Return Enumeration.GenericEnumerator
            End Function

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Return Enumeration.GetEnumerator
            End Function
        End Class

        ''' <summary>
        ''' 将一个<see cref="Array"/>对象转换为一个<see cref="Object"/>对象的枚举序列
        ''' </summary>
        ''' <param name="enums"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 使用这个拓展函数的原因是<see cref="Array"/>对象不能够产生对象的枚举序列用于Linq拓展函数
        ''' </remarks>
        <Extension>
        Public Iterator Function AsObjectEnumerator(enums As Array) As IEnumerable(Of Object)
            For i As Integer = 0 To enums.Length - 1
                Yield enums.GetValue(i)
            Next
        End Function

        ''' <summary>
        ''' 将一个<see cref="Array"/>对象转换为一个<see cref="Object"/>对象的枚举序列
        ''' </summary>
        ''' <param name="enums"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 使用这个拓展函数的原因是<see cref="Array"/>对象不能够产生对象的枚举序列用于Linq拓展函数
        ''' </remarks>
        <Extension>
        Public Iterator Function AsObjectEnumerator(Of T)(enums As Array) As IEnumerable(Of T)
            For i As Integer = 0 To enums.Length - 1
                Yield DirectCast(enums.GetValue(i), T)
            Next
        End Function

        ''' <summary>
        ''' Returns the input typed as <see cref="IEnumerable(Of T)"/>.
        ''' </summary>
        ''' <typeparam name="T">The type of the elements of source.</typeparam>
        ''' <param name="enums">The sequence to type as <see cref="IEnumerable(Of T)"/></param>
        ''' <returns>The input sequence typed as <see cref="IEnumerable(Of T)"/>.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsEnumerable(Of T)(enums As Enumeration(Of T)) As IEnumerable(Of T)
            If enums Is Nothing Then
                Return {}
            Else
                Return New Enumerator(Of T) With {
                    .Enumeration = enums
                }
            End If
        End Function
    End Module
End Namespace
