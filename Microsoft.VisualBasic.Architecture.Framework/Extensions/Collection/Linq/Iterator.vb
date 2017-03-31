#Region "Microsoft.VisualBasic::9e8834e8230661102662b6c7c387833f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Collection\Linq\Iterator.vb"

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
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization
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

        <Extension>
        Public Iterator Function SeqIterator(Of T1, T2)(seqFrom As IEnumerable(Of T1),
                                                        follows As IEnumerable(Of T2),
                                                        Optional offset% = 0) As IEnumerable(Of SeqValue(Of T1, T2))
            Dim x As T1() = seqFrom.ToArray
            Dim y As T2() = follows.ToArray

            For i As Integer = 0 To x.Length - 1
                Yield New SeqValue(Of T1, T2)(i + offset, x(i), y.Get(i))
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
        <Extension>
        Public Function ValueArray(Of T)(source As IEnumerable(Of Value(Of T).IValueOf)) As T()
            Return source.Select(Function(o) o.value).ToArray
        End Function
    End Module

    Public Structure SeqValue(Of T1, T2) : Implements IAddressHandle

        Public Property i As Integer
        Public Property value As T1
        Public Property Follows As T2

        Private Property Address As Integer Implements IAddressHandle.Address
            Get
                Return CLng(i)
            End Get
            Set(value As Integer)
                i = CInt(value)
            End Set
        End Property

        Sub New(i%, x As T1, y As T2)
            Me.i = i
            value = x
            Follows = y
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub
    End Structure

    Public Structure SeqValue(Of T) : Implements IAddressHandle

        ''' <summary>
        ''' The position of this object value in the original sequence.
        ''' </summary>
        ''' <returns></returns>
        Public Property i%
        ''' <summary>
        ''' The Object data
        ''' </summary>
        ''' <returns></returns>
        Public Property value As T

        Private Property Address As Integer Implements IAddressHandle.Address
            Get
                Return CLng(i)
            End Get
            Set
                i = CInt(Value)
            End Set
        End Property

        Sub New(i%, x As T)
            Me.i = i
            value = x
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson(False)
        End Function

        Public Shared Narrowing Operator CType(x As SeqValue(Of T)) As T
            Return x.value
        End Operator

        Public Shared Narrowing Operator CType(x As SeqValue(Of T)) As Integer
            Return x.i
        End Operator

        Public Shared Operator +(list As System.Collections.Generic.List(Of T), x As SeqValue(Of T)) As System.Collections.Generic.List(Of T)
            Call list.Add(x.value)
            Return list
        End Operator

        ''' <summary>
        ''' Get value from <see cref="value"/> property.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator +(x As SeqValue(Of T)) As T
            Return x.value
        End Operator

        Public Sub Dispose() Implements IDisposable.Dispose
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
