#Region "Microsoft.VisualBasic::59ea8d660ffcb6907c7ba949c82b16f1, Data_science\MachineLearning\Bootstrapping\Darwinism\GAF\ODEs\Dump.vb"

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

    '     Class Dump
    ' 
    '         Sub: Update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Text

Namespace Darwinism.GAF.ODEs

    Public Class Dump

        Public model As Type
        Public n%, a%, b%
        Public y0 As Dictionary(Of String, Double)

        Dim i As New Uid(caseSensitive:=False)

        Public Sub Update(iteration%, fitness#, environment As GeneticAlgorithm(Of ParameterVector))
            Dim best As ParameterVector = environment.Best
            Dim vars As Dictionary(Of String, Double) =
                best _
                   .vars _
                   .ToDictionary(Function(var) var.Name,
                                 Function(var) var.value)
            Dim out As ODEsOut = MonteCarlo.Model.RunTest(model, y0, vars, n, a, b)  ' 通过拟合的参数得到具体的计算数据
            Dim path = App.CurrentProcessTemp & $"\{GetHashCode()}\debug_{+i}.csv"

            Call out.DataFrame("#TIME").Save(path$, Encodings.ASCII)
        End Sub
    End Class
End Namespace
