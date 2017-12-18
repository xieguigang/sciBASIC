Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    ''' <summary>
    ''' The object counter
    ''' </summary>
    Public Class Counter : Inherits int

        Sub New()
            MyBase.New(0)
        End Sub

        Sub New(hits As Integer)
            Call MyBase.New(x:=hits)
        End Sub

        ''' <summary>
        ''' ``++i``
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Hit() As Integer
            Return ++Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(c As Counter) As Integer
            Return c.Value
        End Operator
    End Class

    Public Module CounterExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsNumeric(Of K)(counter As Dictionary(Of K, Counter)) As Dictionary(Of K, Double)
            Return counter.ToDictionary(Function(c) c.Key, Function(c) CDbl(c.Value))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsInteger(Of K)(counter As Dictionary(Of K, Counter)) As Dictionary(Of K, Integer)
            Return counter.ToDictionary(Function(c) c.Key, Function(c) CInt(c.Value))
        End Function
    End Module
End Namespace