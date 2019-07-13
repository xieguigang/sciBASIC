#Region "Microsoft.VisualBasic::bafe1b916b2181cb9bca634d2782c5cd, Data_science\Mathematica\Math\Math\Scripting\Factors\Factor.vb"

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

    '     Class Factor
    ' 
    '         Properties: FactorValue
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Factors
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetFactors
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language

Namespace Scripting

    ''' <summary>
    ''' R language like string factor
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Factor(Of T As IComparable(Of T)) : Inherits VBDouble
        Implements Value(Of T).IValueOf

        Public Property FactorValue As T Implements Value(Of T).IValueOf.Value

        Sub New()
        End Sub

        Sub New(value As T, factor#)
            Me.Value = factor
            Me.FactorValue = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return FactorValue.ToString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(factor As Factor(Of T)) As T
            Return factor.FactorValue
        End Operator
    End Class

    Public Class Factors(Of T As IComparable(Of T)) : Inherits Index(Of T)

        Sub New(ParamArray list As T())
            Call MyBase.New(list)
        End Sub

        Public Iterator Function GetFactors() As IEnumerable(Of Factor(Of T))
            For Each i In MyBase.Map
                Yield New Factor(Of T) With {
                    .FactorValue = i.Key,
                    .Value = i.Value
                }
            Next
        End Function
    End Class
End Namespace
