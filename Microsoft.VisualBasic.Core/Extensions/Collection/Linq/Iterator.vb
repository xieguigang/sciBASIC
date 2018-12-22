#Region "Microsoft.VisualBasic::27a6e726bdd07af572a40c4625b66bd9, Microsoft.VisualBasic.Core\Extensions\Collection\Linq\Iterator.vb"

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

    '     Module IteratorExtensions
    ' 
    '         Function: [Next], (+2 Overloads) Indices, Ordinals, Previous, (+2 Overloads) SeqIterator
    '                   (+2 Overloads) SeqTuple, ValueArray
    ' 
    '     Structure SeqValue
    ' 
    '         Properties: i, IsEmpty, value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) CompareTo, ToString
    ' 
    '         Sub: Assign
    ' 
    '         Operators: -, (+2 Overloads) +, <>, =, (+2 Overloads) Mod
    ' 
    '     Interface IIterator
    ' 
    '         Function: GetEnumerator, IGetEnumerator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Linq

    Public Module IteratorExtensions

        <Extension>
        Public Iterator Function SeqIterator(source As IEnumerable, Optional offset% = 0) As IEnumerable(Of SeqValue(Of Object))
            Dim i As Integer = offset

            For Each o As Object In source
                Yield New SeqValue(Of Object)(i, o)
                i += 1
            Next
        End Function

        ''' <summary>
        ''' Iterates all of the objects in the source sequence with collection index position.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source">the source sequence</param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function SeqIterator(Of T)(source As IEnumerable(Of T), Optional offset% = 0) As IEnumerable(Of SeqValue(Of T))
            If Not source Is Nothing Then
                Dim idx% = offset

                For Each x As T In source
                    Yield New SeqValue(Of T)(idx, x)
                    idx += 1
                Next
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function SeqTuple(Of T1, T2)(tuple As (a As IEnumerable(Of T1), b As IEnumerable(Of T2)), Optional offset% = 0) As IEnumerable(Of SeqValue(Of (a As T1, b As T2)))
            Return (tuple.a.ToArray, tuple.b.ToArray).SeqTuple(offset)
        End Function

        <Extension>
        Public Iterator Function SeqTuple(Of T1, T2)(tuple As (x As T1(), y As T2()), Optional offset% = 0) As IEnumerable(Of SeqValue(Of (a As T1, b As T2)))
            Dim value As (T1, T2)
            Dim length% = Math.Max(tuple.x.Length, tuple.y.Length)

            For i As Integer = 0 To length - 1
                value = (
                    tuple.x.ElementAtOrDefault(i),
                    tuple.y.ElementAtOrDefault(i)
                )
                Yield New SeqValue(Of (T1, T2))(i + offset, value)
            Next
        End Function

        ''' <summary>
        ''' Move the enumerator pointer to next and get next value, if the pointer is reach the end, then will returns nothing
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function [Next](Of T)(source As IEnumerator(Of T)) As T
            If source.MoveNext() Then
                Return source.Current
            Else
                Return Nothing
            End If
        End Function

        <Extension>
        Public Function Previous(Of T)(source As IEnumerator(Of T)) As T
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' Creates an array from property <see cref="Value(Of T).IValueOf.value"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ValueArray(Of T)(source As IEnumerable(Of Value(Of T).IValueOf)) As T()
            Return source.Select(Function(o) o.Value).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Indices(Of T)(source As IEnumerable(Of SeqValue(Of T))) As Integer()
            Return source.Ordinals.ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Ordinals(Of T)(source As IEnumerable(Of SeqValue(Of T))) As IEnumerable(Of Integer)
            Return source.Select(Function(o) o.i)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Indices(Of T)(source As IEnumerable(Of T), assert As Func(Of T, Boolean)) As Integer()
            Return source _
                .SeqIterator _
                .Where(Function(x) True = assert(x.value)) _
                .Indices
        End Function
    End Module

    ''' <summary>
    ''' Value <typeparamref name="T"/> with sequence index <see cref="i"/>.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure SeqValue(Of T) : Implements IAddressOf
        Implements IComparable(Of Integer)
        Implements IComparable
        Implements Value(Of T).IValueOf
        Implements IsEmpty

        ''' <summary>
        ''' The position of this object value in the original sequence.
        ''' </summary>
        ''' <returns></returns>
        Public Property i As Integer Implements IAddressOf.Address
        ''' <summary>
        ''' The Object data
        ''' </summary>
        ''' <returns></returns>
        Public Property value As T Implements Value(Of T).IValueOf.Value

        Public ReadOnly Property IsEmpty As Boolean Implements IsEmpty.IsEmpty
            Get
                Return i = 0 AndAlso value Is Nothing
            End Get
        End Property

        Sub New(i%, x As T)
            Me.i = i
            value = x
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return Me.value.GetJson(False)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(x As SeqValue(Of T)) As T
            Return x.value
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(x As SeqValue(Of T)) As Integer
            Return x.i
        End Operator

        Public Shared Operator +(list As System.Collections.Generic.List(Of T), x As SeqValue(Of T)) As System.Collections.Generic.List(Of T)
            Call list.Add(x.value)
            Return list
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Mod(i As SeqValue(Of T), n%) As Integer
            Return i.i Mod n
        End Operator

        Public Shared Operator <>(v As SeqValue(Of T), i%) As Boolean
            Return v.i <> i
        End Operator

        Public Shared Operator =(v As SeqValue(Of T), i%) As Boolean
            Return v.i = i
        End Operator

        ''' <summary>
        ''' Get value from <see cref="value"/> property.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Syntax helper for the <see cref="Pointer(Of T)"/>:
        ''' 
        ''' ```vbnet
        ''' Dim p As Pointer(Of T) = T()
        ''' Dim x As T = ++p
        ''' ```
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(x As SeqValue(Of T)) As T
            Return x.value
        End Operator

        ''' <summary>
        ''' Get value from <see cref="value"/> property.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Syntax helper for the <see cref="Pointer(Of T)"/>:
        ''' 
        ''' ```vbnet
        ''' Dim p As Pointer(Of T) = T()
        ''' Dim x As T = --p
        ''' ```
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(x As SeqValue(Of T)) As T
            Return x.value
        End Operator

        ''' <summary>
        ''' Compares the index value <see cref="i"/>.
        ''' </summary>
        ''' <param name="other"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CompareTo(other As Integer) As Integer Implements IComparable(Of Integer).CompareTo
            Return i.CompareTo(other)
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            If obj Is Nothing Then
                Return 1
            End If

            If Not obj.GetType Is Me.GetType Then
                Return 10
            End If

            Return i.CompareTo(DirectCast(obj, SeqValue(Of T)).i)
        End Function

        Private Sub Assign(address As Integer) Implements IAddress(Of Integer).Assign
            i = address
        End Sub
    End Structure

    ''' <summary>
    ''' Exposes the enumerator, which supports a simple iteration over a collection of
    ''' a specified type.To browse the .NET Framework source code for this type, see
    ''' the Reference Source.
    ''' (使用这个的原因是系统自带的<see cref="IEnumerable(Of T)"/>在Xml序列化之中的支持不太好)
    ''' </summary>
    ''' <typeparam name="T">The type of objects to enumerate.This type parameter is covariant. That is, you
    ''' can use either the type you specified or any type that is more derived. For more
    ''' information about covariance and contravariance, see Covariance and Contravariance
    ''' in Generics.</typeparam>
    Public Interface IIterator(Of T)

        ''' <summary>
        ''' Returns an enumerator that iterates through the collection.
        ''' </summary>
        ''' <returns>An enumerator that can be used to iterate through the collection.</returns>
        Function GetEnumerator() As IEnumerator(Of T)

        ''' <summary>
        ''' Returns an enumerator that iterates through a collection.
        ''' </summary>
        ''' <returns>An System.Collections.IEnumerator object that can be used to iterate through
        ''' the collection.</returns>
        Function IGetEnumerator() As IEnumerator
    End Interface
End Namespace
