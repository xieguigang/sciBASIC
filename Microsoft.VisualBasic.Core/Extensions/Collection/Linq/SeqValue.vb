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
End Namespace
