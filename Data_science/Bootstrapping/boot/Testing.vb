Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.diffEq

Module Testing

    Public Sub Run()
        Dim model As Model = New TestModel
        Dim result = GAF.Protocol.Fitting(model, 100, 0, 10, evolIterations:=20000)
        Pause()
    End Sub

    Public Class TestModel : Inherits Model

        Dim y As var
        Dim a As Double

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            dy(y) = a * Math.Sin(dx + a)
        End Sub

        Public Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
            Throw New NotImplementedException()
        End Function

        Public Overrides Function params() As VariableModel()
            Return {
                New VariableModel(-100, 200) With {
                    .Name = NameOf(a)
                }
            }
        End Function

        Public Overrides Function yinit() As VariableModel()
            Return {
                New VariableModel(-100, 300) With {
                    .Name = NameOf(y)
                }
            }
        End Function
    End Class
End Module
