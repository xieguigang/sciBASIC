#Region "Microsoft.VisualBasic::6f74918bf8d85e426efe71f02723b038, Data_science\MachineLearning\MLDebugger\ANN\ROC.vb"

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

    ' Module ROC
    ' 
    '     Function: AUC
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Evaluation
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Module ROC

    <Extension>
    Public Function AUC(training As TrainingUtils) As Double()
        Dim network = training.NeuronNetwork
        Dim result = training.TrainingSet _
            .Select(Function(sample)
                        Dim predicts = network.Compute(sample.sample)
                        Dim actuals = sample.classify

                        Return New Validate With {
                            .actuals = actuals,
                            .predicts = predicts
                        }
                    End Function) _
            .ToArray
        Dim attributes = result(Scan0).actuals
        Dim thresholdSeq As New Sequence With {.n = 100, .range = {0, 1}}
        Dim evalAUC = Function(null As Double, i As Integer) As Double
                          Dim validations = Validation.ROC(Of Validate)(
                              entity:=result,
                              getValidate:=Function(x, threshold) x.actuals(i) >= threshold,
                              getPredict:=Function(x, threshold) x.predicts(i) >= threshold,
                              threshold:=thresholdSeq
                          ).Where(Function(threshold)
                                      Return Not threshold.Specificity.IsNaNImaginary AndAlso
                                             Not threshold.Sensibility.IsNaNImaginary
                                  End Function) _
                           .ToArray
                          Dim AUCValue = Validation.AUC(validations)

                          Return AUCValue
                      End Function
        Dim validateAUCs = attributes _
            .Select(evalAUC) _
            .ToArray

        Return validateAUCs
    End Function
End Module
