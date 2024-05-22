#Region "Microsoft.VisualBasic::b725190fb9af50c3834f0ee40a938bb4, Data_science\MachineLearning\MachineLearning\test\activeTest.vb"

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

    '   Total Lines: 40
    '    Code Lines: 36 (90.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (10.00%)
    '     File Size: 1.72 KB


    ' Module activeTest
    ' 
    '     Function: runTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations

Module activeTest

    Sub Main()
        Call runTest(-100, 100).ToArray.SaveTo("./ANN_actives.csv")
    End Sub

    Private Iterator Function runTest(min#, max#) As IEnumerable(Of DataSet)
        Dim BipolarSigmoid As New BipolarSigmoid
        Dim HyperbolicTangent As New HyperbolicTangent
        Dim ReLU As New ReLU
        Dim Sigmoid As New Sigmoid
        Dim SigmoidFunction As New SigmoidFunction
        Dim Threshold As New Threshold

        For x As Double = min To max Step 0.1
            Yield New DataSet With {
                .ID = x.ToHexString,
                .Properties = New Dictionary(Of String, Double) From {
                    {"input", x},
                    {"BipolarSigmoid", BipolarSigmoid(x)},
                    {"BipolarSigmoid/dt", BipolarSigmoid.CalculateDerivative(x)},
                    {"HyperbolicTangent", HyperbolicTangent(x)},
                    {"HyperbolicTangent/dt", HyperbolicTangent.CalculateDerivative(x)},
                    {"ReLU", ReLU(x)},
                    {"ReLU/dt", ReLU.CalculateDerivative(x)},
                    {"Sigmoid", Sigmoid(x)},
                    {"Sigmoid/dt", Sigmoid.CalculateDerivative(x)},
                    {"SigmoidFunction", SigmoidFunction(x)},
                    {"SigmoidFunction/dt", SigmoidFunction.CalculateDerivative(x)},
                    {"Threshold", Threshold(x)},
                    {"Threshold/dt", Threshold.CalculateDerivative(x)}
                }
            }
        Next
    End Function
End Module
