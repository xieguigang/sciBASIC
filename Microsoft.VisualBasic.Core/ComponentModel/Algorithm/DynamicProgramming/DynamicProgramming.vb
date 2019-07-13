#Region "Microsoft.VisualBasic::1f506608694faa5f51d75c94347722b9, Microsoft.VisualBasic.Core\ComponentModel\Algorithm\DynamicProgramming\DynamicProgramming.vb"

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

    '     Class Cost
    ' 
    '         Function: DefaultCost, DefaultSubstituteCost
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Default

Namespace ComponentModel.Algorithm.DynamicProgramming

    Public Class Cost(Of T)

        Public insert As Func(Of T, Double)
        Public delete As Func(Of T, Double)
        Public substitute As Func(Of T, T, Double)
        Public transpose As Func(Of T, T, Double)

        Public Shared Function DefaultSubstituteCost(a As T, b As T, equals As GenericLambda(Of T).IEquals) As Double
            Return If(equals(a, b), 0, 1)
        End Function

        Public Shared Function DefaultCost(cost As Double) As [Default](Of Cost(Of T))
            Return New Cost(Of T) With {
                .insert = Function(x) cost,
                .delete = Function(x) cost,
                .substitute = Function(a, b) cost
            }
        End Function
    End Class

    Public Delegate Function ISimilarity(Of T)(x As T, y As T) As Double
    Public Delegate Function IEquals(Of T)(x As T, y As T) As Boolean

End Namespace
