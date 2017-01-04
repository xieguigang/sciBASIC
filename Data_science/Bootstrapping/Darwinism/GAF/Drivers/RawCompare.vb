Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.Calculus

Namespace Darwinism.GAF

    ''' <summary>
    ''' 不像<see cref="GAFFitness"/>接受的是经过插值处理的原始数据，
    ''' 这个``fitness``驱动程序接受的是未经过任何处理的原始数据
    ''' </summary>
    Public Class RawCompare
        Implements Fitness(Of ParameterVector)

        ''' <summary>
        ''' TIME
        ''' </summary>
        Dim X#()
        Dim n%
        Dim a, b As Double
        Dim time As New Dictionary(Of NamedValue(Of IndexOf(Of Double)))
        Dim observation As NamedValue(Of TimeValue())()

        ''' <summary>
        ''' <see cref="MonteCarlo.Model"/>
        ''' </summary>
        Dim model As Type
        Dim y0 As Dictionary(Of String, Double)

        Sub New(observation As NamedValue(Of TimeValue())(), n%, a#, b#, y0 As Dictionary(Of String, Double))
            With Me
                .n = n
                .a = a
                .b = b
                .X = ODEs.TimePopulator(n, a, b).ToArray
                .observation = observation
                .y0 = y0
            End With

            For Each var As NamedValue(Of TimeValue()) In observation
                time += New NamedValue(Of IndexOf(Of Double)) With {
                    .Name = var.Name,
                    .Value = TimeValue.BuildIndex(X, var.Value)
                }
            Next
        End Sub

        Public Function Calculate(chromosome As ParameterVector) As Double Implements Fitness(Of ParameterVector).Calculate
            Dim result As ODEsOut = MonteCarlo.Model.RunTest(model, y0, chromosome.vars, n, a, b)
            Dim fitness As New List(Of Double)

            For Each var As NamedValue(Of TimeValue()) In observation
                Dim y = result.y(var.Name)
                Dim index As IndexOf(Of Double) = time(var.Name).Value
                Dim indices%() = var.Value _
                    .Select(Function(t) index(t.Time)) _
                    .ToArray
                Dim cData#() = indices _
                    .Select(Function(i) y.Value(i)) _
                    .ToArray

                fitness += Math.Sqrt(
                    FitnessHelper.Calculate(
                    var.Value.Select(Function(t) t.Y).ToArray,
                    cData))
            Next

            Dim out# = fitness.Average
            Return out
        End Function
    End Class
End Namespace