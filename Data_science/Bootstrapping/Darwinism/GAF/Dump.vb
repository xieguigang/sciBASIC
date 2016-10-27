Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Text

Namespace GAF

    Public Class Dump
        Implements IterartionListener(Of ParameterVector, Double)

        Public model As Type
        Public n%, a%, b%
        Public y0 As Dictionary(Of String, Double)

        Dim i As New Uid(caseSensitive:=False)

        Public Sub Update(environment As GeneticAlgorithm(Of ParameterVector, Double)) Implements IterartionListener(Of ParameterVector, Double).Update
            Dim best As ParameterVector = environment.Best
            Dim vars As Dictionary(Of String, Double) =
                best _
                   .vars _
                   .ToDictionary(Function(var) var.Name,
                                 Function(var) var.value)
            Dim out As ODEsOut =
                MonteCarlo.Model.RunTest(model, y0, vars, n, a, b)  ' 通过拟合的参数得到具体的计算数据
            Dim path = App.CurrentProcessTemp & $"\debug_{+i}.csv"

            Call out.DataFrame("#TIME").Save(path$, Encodings.ASCII)
        End Sub
    End Class
End Namespace