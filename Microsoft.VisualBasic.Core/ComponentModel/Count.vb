#Region "Microsoft.VisualBasic::dfbbdb353c3faead10950620e0ceb0b5, Microsoft.VisualBasic.Core\ComponentModel\Count.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Counter
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Hit
    ' 
    '         Sub: Add
    ' 
    '     Module CounterExtensions
    ' 
    '         Function: AsInteger, AsNumeric
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    ''' <summary>
    ''' The object counter
    ''' </summary>
    Public Class Counter : Inherits VBInteger

        ''' <summary>
        ''' Create a new integer counter start from ZERO.(新建一个计数器)
        ''' </summary>
        Sub New()
            MyBase.New(0)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(hits As Integer)
            Call MyBase.New(x:=hits)
        End Sub

        Public Sub Add(n As Integer)
            Value += n
        End Sub

        ''' <summary>
        ''' ``++i``
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Hit() As Integer
            Return ++Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(c As Integer) As Counter
            Return New Counter(hits:=c)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(c As Counter) As Integer
            Return c.Value
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(c As Counter) As Double
            Return CDbl(c.Value)
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
