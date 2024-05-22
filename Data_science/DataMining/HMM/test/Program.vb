#Region "Microsoft.VisualBasic::ce1b66246f9107efec6265ec158a3227, Data_science\DataMining\HMM\test\Program.vb"

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

    '   Total Lines: 74
    '    Code Lines: 47 (63.51%)
    ' Comment Lines: 9 (12.16%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (24.32%)
    '     File Size: 3.12 KB


    ' Module Program
    ' 
    '     Sub: HMMTest, Main, MarkovChainTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models

Module Program

    Sub Main()
        ' Call MarkovChainTest()
        Call HMMTest()
    End Sub

    Sub HMMTest()

        Dim hiddenStates As StatesObject() = {
            New StatesObject With {.state = "AT-rich", .prob = {0.95, 0.05}},
            New StatesObject With {.state = "CG-rich", .prob = {0.1, 0.9}}
        }
        Dim observables As Observable() = {
              New Observable With {.obs = "A", .prob = {0.4, 0.05}},
              New Observable With {.obs = "C", .prob = {0.1, 0.45}},
              New Observable With {.obs = "G", .prob = {0.1, 0.45}},
              New Observable With {.obs = "T", .prob = {0.4, 0.05}}
        }
        Dim hiddenInit = {0.65, 0.35}

        Dim HMModel = New HMM(hiddenStates, observables, hiddenInit)
        Dim observation = "A"
        Dim hiddenState = "AT-rich"
        ' 0.9369369369369369
        Dim bayesResult = HMModel.bayesTheorem(observation, hiddenState)

        Dim obSequence As New Chain(Function(a, b) a = b) With {.obSequence = {"T", "C", "G", "G", "A"}}
        ' alphaF 0.0003171642187500001
        Dim forwardProbability = HMModel.forwardAlgorithm(obSequence)
        ' betaF 0.0003171642187500001
        Dim backwardProbability = HMModel.backwardAlgorithm(obSequence)

        obSequence = New Chain(Function(a, b) a = b) With {.obSequence = {"A", "T", "C", "G", "C", "G", "T", "C", "A", "T", "C", "G", "T", "C", "G", "T", "C", "C", "G"}}

        ' 'AT-rich', 'AT-rich', 'CG-rich', 'CG-rich', 'CG-rich', ...
        ' 1.972455831264621E-14
        Dim viterbiResult = HMModel.viterbiAlgorithm(obSequence)

        obSequence = New Chain(Function(a, b) a = b) With {.obSequence = {"A", "T", "C", "G", "C", "G", "T", "C", "A", "T", "C", "G", "T", "C", "G", "T", "C", "C", "G"}}

        ' [ [ 0.748722257770877, 0.251277742229123 ], [ 0.08173322039272721, 0.9182667796072727 ] ]
        Dim maximizedModel = HMModel.baumWelchAlgorithm(obSequence)
        Dim max = maximizedModel.GetTransMatrix

        Pause()
    End Sub

    Sub MarkovChainTest()
        Dim states As StatesObject() = {
           New StatesObject With {.state = "sunny", .prob = {0.4, 0.4, 0.2}},
           New StatesObject With {.state = "cloudy", .prob = {0.3, 0.3, 0.4}},
           New StatesObject With {.state = "rainy", .prob = {0.2, 0.5, 0.3}}
       }
        Dim init As Double() = {0.4, 0.3, 0.3}
        Dim markovChain As New MarkovChain(states, init)

        Dim stateSeq As New Chain(Function(a, b) a = b) With {.obSequence = {"sunny", "rainy", "sunny", "sunny", "cloudy"}}
        ' 0.002560000000000001
        Dim seqProbability = markovChain.SequenceProb(stateSeq)

        ' Call Console.WriteLine(markovChain.GetTransMatrix)

        Dim test2 As New Chain(Function(a, b) a = b) With {.obSequence = {"sunny", "sunny", "cloudy", "rainy"}}

        Dim prob2 = markovChain.SequenceProb(test2)

        Pause()
    End Sub

End Module
