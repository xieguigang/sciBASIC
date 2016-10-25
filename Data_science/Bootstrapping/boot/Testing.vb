Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.DataMining.GAF.Helper.ListenerHelper
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON

Module Testing

    Public Sub Run()

        'Call mutate(2)
        'Call mutate(20)
        'Call mutate(200)
        'Call mutate(200000)
        'Call mutate(2 * 10 ^ -30)
        'Call mutate(2 * 10 ^ -8)
        'Call mutate(2 * 10 ^ 99)
        'Call mutate(2 * 10 ^ 10)

        Dim model As Model = New TestModel
        Dim outPrint As List(Of outPrint) = Nothing
        Dim result = GAF.Protocol.Fitting(model, 2000, 0, 100, popSize:=1000, evolIterations:=150000, outPrint:=outPrint)

        Call outPrint.SaveTo("x:\test_debug.csv")
        Call result.GetJson.__DEBUG_ECHO

        Pause()
    End Sub

    Public Class TestModel : Inherits Model

        Dim y As var
        Dim y2 As var
        Dim a As Double

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            dy(y) = a * Math.Sin(dx + a)
            dy(y2) = 2 * a + dx
        End Sub

        Public Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
            Throw New NotImplementedException()
        End Function

        Public Overrides Function params() As VariableModel()
            Return {
                New VariableModel(-10000, 20000) With {
                    .Name = NameOf(a)
                }
            }
        End Function

        Public Overrides Function yinit() As VariableModel()
            Return {
                New VariableModel(-10000, 3300) With {
                    .Name = NameOf(y)
                },
                New VariableModel(-10000, 3300) With {
                    .Name = NameOf(y2)
                }
            }
        End Function
    End Class
End Module
