#Region "Microsoft.VisualBasic::d1cad56db0c2e7f28cea1ee865e2d2aa, Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\SeqValue.vb"

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

    '   Total Lines: 198
    '    Code Lines: 97 (48.99%)
    ' Comment Lines: 77 (38.89%)
    '    - Xml Docs: 94.81%
    ' 
    '   Blank Lines: 24 (12.12%)
    '     File Size: 6.72 KB


    '     Structure SeqValue
    ' 
    '         Properties: i, IsEmpty, value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) CompareTo, ToString
    ' 
    '         Sub: Assign
    ' 
    '         Operators: (+2 Overloads) -, (+3 Overloads) +, (+2 Overloads) <>, (+2 Overloads) =, (+2 Overloads) Mod
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
        Public Property i As Integer Implements IAddress(Of Integer).Address
        ''' <summary>
        ''' The Object data
        ''' </summary>
        ''' <returns></returns>
        Public Property value As T Implements Value(Of T).IValueOf.Value

        ''' <summary>
        ''' This indexed value have no value.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean Implements IsEmpty.IsEmpty
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return i = 0 AndAlso value Is Nothing
            End Get
        End Property

        <DebuggerStepThrough>
        Sub New(i%, x As T)
            Me.i = i
            value = x
        End Sub

        ''' <summary>
        ''' create index value with default index zero
        ''' </summary>
        ''' <param name="value"></param>
        Sub New(value As T)
            Me.i = 0
            Me.value = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overrides Function ToString() As String
            Return $"[{i}] {Me.value.GetJson(False)}"
        End Function

        ''' <summary>
        ''' get value from the indexed object
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(x As SeqValue(Of T)) As T
            Return x.value
        End Operator

        ''' <summary>
        ''' get ordinal index offset from the given indexed object.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(x As SeqValue(Of T)) As Integer
            Return x.i
        End Operator

        Public Shared Operator +(list As System.Collections.Generic.List(Of T), x As SeqValue(Of T)) As System.Collections.Generic.List(Of T)
            Call list.Add(x.value)
            Return list
        End Operator

        ''' <summary>
        ''' Calculation of: ``<see cref="SeqValue(Of T).i"/> Mod <paramref name="n"/>``
        ''' </summary>
        ''' <param name="i"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Mod(i As SeqValue(Of T), n%) As Integer
            Return i.i Mod n
        End Operator

        ''' <summary>
        ''' Not equals to index i
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="i%"></param>
        ''' <returns></returns>
        Public Shared Operator <>(v As SeqValue(Of T), i%) As Boolean
            Return v.i <> i
        End Operator

        ''' <summary>
        ''' Equals to index i
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="i%"></param>
        ''' <returns></returns>
        Public Shared Operator =(v As SeqValue(Of T), i%) As Boolean
            Return v.i = i
        End Operator

        Public Shared Operator =(v As SeqValue(Of T), x As T) As Boolean
            Return v.value.Equals(x)
        End Operator

        Public Shared Operator <>(v As SeqValue(Of T), x As T) As Boolean
            Return Not v.value.Equals(x)
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
        ''' <see cref="i"/> + <paramref name="i"/>
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Shared Operator +(x As SeqValue(Of T), i As Integer) As Integer
            Return x.i + i
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(x As SeqValue(Of T), n As Integer) As Integer
            Return x.i - n
        End Operator

        ''' <summary>
        ''' Compares the index value <see cref="i"/>.
        ''' </summary>
        ''' <param name="other"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
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

        <DebuggerStepThrough>
        Private Sub Assign(address As Integer) Implements IAddress(Of Integer).Assign
            i = address
        End Sub
    End Structure
End Namespace
