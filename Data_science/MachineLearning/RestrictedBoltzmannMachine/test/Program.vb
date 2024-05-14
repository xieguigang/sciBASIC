#Region "Microsoft.VisualBasic::3b8cecbb169edb1b399b0411035f9bbd, Data_science\MachineLearning\RestrictedBoltzmannMachine\test\Program.vb"

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

    '   Total Lines: 32
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 1.61 KB


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.factory
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.learn
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.utils
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Module Program
    Sub Main(args As String())
        Dim rbm = New RandomRBMFactory().build(6, 3)
        Dim contrastiveDivergence = New ContrastiveDivergence(New LearningParameters().setEpochs(25000))
        Dim training As Double()() = {
            New Double() {1.0, 1.0, 1.0, 0.0, 0.0, 0.0},
            New Double() {1.0, 0.0, 1.0, 0.0, 0.0, 0.0},
            New Double() {1.0, 1.0, 1.0, 0.0, 0.0, 0.0},
            New Double() {0.0, 0.0, 1.0, 1.0, 1.0, 0.0},
            New Double() {0.0, 0.0, 1.0, 1.0, 0.0, 0.0},
            New Double() {0.0, 0.0, 1.0, 1.0, 1.0, 0.0}
        }
        Dim buildBetterSampleTrainingData As New DenseMatrix(New NumericMatrix(training))

        contrastiveDivergence.learn(rbm, buildBetterSampleTrainingData)

        Dim testData = DenseMatrix.make(New Double()() {New Double() {0, 0, 0, 1, 1, 0}, New Double() {0, 0, 1, 1, 0, 0}})
        Dim hidden = contrastiveDivergence.runVisible(rbm, testData)
        Dim visual = contrastiveDivergence.runHidden(rbm, hidden)

        Call Console.WriteLine(PrettyPrint.ToString(hidden.toArray))
        Call Console.WriteLine(PrettyPrint.ToString(visual.toArray))

        Pause()
    End Sub
End Module
