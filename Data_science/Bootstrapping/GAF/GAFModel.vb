Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Mathematical.BasicR

Namespace GAF

    Public MustInherit Class Model : Inherits MonteCarlo.Model

        Protected MustOverride Overrides Sub func(dx As Double, ByRef dy As Vector)

#Region "Not Required"
        Public NotOverridable Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
            Return Nothing
        End Function

        Public NotOverridable Overrides Function params() As VariableModel()
            Return Nothing
        End Function

        Public NotOverridable Overrides Function yinit() As VariableModel()
            Return Nothing
        End Function
#End Region

    End Class
End Namespace