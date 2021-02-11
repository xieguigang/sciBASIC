Imports System.Runtime.CompilerServices

Namespace LinearAlgebra

    Public MustInherit Class Formula

        ''' <summary>
        ''' 多项式系数向量
        ''' </summary>
        ''' <returns></returns>
        Public Property Factors As Double()

        Public MustOverride Function Evaluate(ParamArray x As Double()) As Double
        Public MustOverride Overloads Function ToString(format As String, Optional html As Boolean = False) As String
        Public MustOverride Overloads Function ToString(variables As String(), format As String, Optional html As Boolean = False) As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return ToString(format:="G3")
        End Function
    End Class
End Namespace