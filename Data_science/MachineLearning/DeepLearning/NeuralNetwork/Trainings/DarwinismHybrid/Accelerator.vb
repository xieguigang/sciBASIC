#Region "Microsoft.VisualBasic::338b7bc57993fb50447580f82018a9d0, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Trainings\DarwinismHybrid\Accelerator.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 57
    '    Code Lines: 44 (77.19%)
    ' Comment Lines: 4 (7.02%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 9 (15.79%)
    '     File Size: 2.50 KB


    '     Module Accelerator
    ' 
    '         Function: GetSynapseGroups, RunGATrainer
    ' 
    '         Sub: doPrint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Population

Namespace NeuralNetwork.DarwinismHybrid

    ''' <summary>
    ''' not working as expected?
    ''' </summary>
    Public Module Accelerator

        <Extension>
        Public Function GetSynapseGroups(network As Network) As NamedCollection(Of Synapse)()
            Return network.PopulateAllSynapses _
                .GroupBy(Function(s) s.ToString) _
                .Select(Function(sg)
                            Return New NamedCollection(Of Synapse)(sg.Key, sg.ToArray)
                        End Function) _
                .ToArray
        End Function

        <Extension>
        Public Function RunGATrainer(network As Network, trainingSet As Sample(), Optional mutationRate# = 0.2, Optional populationSize% = 1000, Optional iterations% = 10000) As Network
            Dim population As New Population(Of NetworkIndividual)(New PopulationList(Of NetworkIndividual), parallel:=True) With {
                .capacitySize = populationSize
            }

            population = New NetworkIndividual(network) With {
                .MutationRate = mutationRate
            }.InitialPopulation(population)

            Dim fitness As Fitness(Of NetworkIndividual) = New Fitness(trainingSet)
            Dim ga As New GeneticAlgorithm(Of NetworkIndividual)(population, fitness)
            Dim engine As New EnvironmentDriver(Of NetworkIndividual)(
                ga:=ga,
                takeBestSnapshot:=Sub(null, nullErr)
                                      ' do nothing
                                  End Sub) With {
                .Iterations = iterations,
                .Threshold = 0.005
            }

            Call "Run GA helper!".__DEBUG_ECHO
            Call engine.AttachReporter(AddressOf doPrint)
            Call engine.Train(parallel:=True)

            Return ga.Best.target
        End Function

        Private Sub doPrint(i%, e#, g As GeneticAlgorithm(Of NetworkIndividual))
            Call EnvironmentDriver(Of NetworkIndividual).CreateReport(i, e, g).ToString.__DEBUG_ECHO
        End Sub
    End Module
End Namespace
