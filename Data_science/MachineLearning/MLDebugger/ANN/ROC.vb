#Region "Microsoft.VisualBasic::ed282dcae4dd55571c496b2e613e4d84, Data_science\MachineLearning\MLDebugger\ANN\ROC.vb"

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

    '   Total Lines: 55
    '    Code Lines: 47 (85.45%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (14.55%)
    '     File Size: 2.28 KB


    ' Module ROC
    ' 
    '     Function: AUC, CreateValidateResult, ROC
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Module ROC

    <Extension>
    Public Iterator Function CreateValidateResult(network As Network, validateSet As IEnumerable(Of TrainingSample)) As IEnumerable(Of Validate)
        For Each sample As TrainingSample In validateSet
            Dim predicts = network.Compute(sample.sample)
            Dim actuals = sample.classify

            Yield New Validate With {
                .actuals = actuals,
                .predicts = predicts
            }
        Next
    End Function

    <Extension>
    Public Function ROC(result As IEnumerable(Of Validate), range As DoubleRange, attribute%, Optional n% = 20) As Validation()
        Dim thresholdSeq As New Sequence With {.n = n, .range = range}

        Return Validation.ROC(Of Validate)(
            entity:=result,
            getValidate:=Function(x, threshold) x.actuals(attribute) >= threshold,
            getPredict:=Function(x, threshold) x.predicts(attribute) >= threshold,
            threshold:=thresholdSeq
        ).Where(Function(threshold)
                    Return Not threshold.Specificity.IsNaNImaginary AndAlso
                        Not threshold.Sensibility.IsNaNImaginary
                End Function) _
         .ToArray
    End Function

    <Extension>
    Public Function AUC(training As TrainingUtils) As Double()
        Dim network As Network = training.NeuronNetwork
        Dim result As Validate() = network.CreateValidateResult(training.TrainingSet).ToArray
        Dim attributes As Double() = result(Scan0).actuals
        Dim evalAUC = Function(null As Double, i As Integer) As Double
                          Dim validations = result.ROC(New Double() {0, 1}, attribute:=i)
                          Dim AUCValue = Validation.AUC(validations)

                          Return AUCValue
                      End Function
        Dim validateAUCs = attributes _
            .Select(evalAUC) _
            .ToArray

        Return validateAUCs
    End Function
End Module
