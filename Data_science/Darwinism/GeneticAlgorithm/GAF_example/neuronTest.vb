#Region "Microsoft.VisualBasic::e24685b885d52f7567834306fbebcb13, Data_science\Darwinism\GeneticAlgorithm\GAF_example\neuronTest.vb"

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

    ' Module neuronTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Accelerator
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module neuronTest

    Sub Main()

        Dim test As New TrainingUtils(6, {12, 80, 24}, 3)

        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({0, 1, 1, 0, 0, 0}, {1, 1, 1})

        ' Call test.Train()
        Call test.NeuronNetwork.RunGAAccelerator(test.TrainingSet)

        Dim classes1 = test.NeuronNetwork.Compute(0, 0, 0, 1, 1, 1)
        Dim classes2 = test.NeuronNetwork.Compute(1, 0, 0, 1, 1, 1)
        Dim classes3 = test.NeuronNetwork.Compute(0, 1, 1, 0, 0, 0)

        Call classes1.AsVector.ToString.__DEBUG_ECHO
        Call classes2.AsVector.ToString.__DEBUG_ECHO
        Call classes3.AsVector.ToString.__DEBUG_ECHO

        Call StoreProcedure.NeuralNetwork.Snapshot(test.NeuronNetwork).GetXml.SaveTo("./network.xml")


        Pause()
    End Sub
End Module
