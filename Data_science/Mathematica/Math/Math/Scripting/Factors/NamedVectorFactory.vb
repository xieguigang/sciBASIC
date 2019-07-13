#Region "Microsoft.VisualBasic::663448477e335b82a8ba508c3452e1f7, Data_science\Mathematica\Math\Math\Scripting\Factors\NamedVectorFactory.vb"

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

    '     Class NamedVectorFactory
    ' 
    '         Properties: Keys
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) AsVector, EmptyVector, ToString, Translate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting

    ''' <summary>
    ''' Factory for <see cref="Dictionary(Of String, Double)"/> to <see cref="Vector"/>
    ''' </summary>
    Public Class NamedVectorFactory

        Public ReadOnly Property Keys As String()

        ReadOnly factors As Factor(Of String)()

        Sub New(factors As IEnumerable(Of String))
            Me.Keys = factors.ToArray
            Me.factors = FactorExtensions.factors(Keys)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function EmptyVector() As Vector
            Return New Vector(factors.Length - 1)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AsVector(data As Dictionary(Of String, Integer)) As Vector
            Return AsVector(data.ToDictionary(Function(k) k.Key, Function(t) CDbl(t.Value)))
        End Function

        Public Function AsVector(data As Dictionary(Of String, Double)) As Vector
            Dim vector#() = New Double(factors.Length - 1) {}

            For Each factor As Factor(Of String) In factors
                vector(CInt(factor.Value)) = data.TryGetValue(factor)
            Next

            Return vector.AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(vector As Vector) As Dictionary(Of String, Double)
            Return factors.ToDictionary(
                Function(factor) factor.FactorValue,
                Function(i) vector(CInt(i.Value)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return factors _
                .Select(Function(factor) factor.FactorValue) _
                .ToArray _
                .GetJson
        End Function
    End Class
End Namespace
