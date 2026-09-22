#Region "Microsoft.VisualBasic::b69ba4e4ac35918f3c3eac583e24357e, Data_science\MachineLearning\MachineLearning\SVM\LibSVM.vb"

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

    '   Total Lines: 42
    '    Code Lines: 18 (42.86%)
    ' Comment Lines: 18 (42.86%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (14.29%)
    '     File Size: 1.67 KB


    '     Class LibSVM
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: getSvmModel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder

Namespace SVM

    ''' <summary>
    ''' The LibSVM style high level api for training a support vector machine 
    ''' model from a raw problem definition.
    ''' </summary>
    ''' <remarks>
    ''' This api applies the range transform (data normalization) on the input 
    ''' problem automatically, so that the returned <see cref="SVMModel"/> object 
    ''' is ready for prediction directly.
    ''' </remarks>
    Public NotInheritable Class LibSVM

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Train a support vector machine model from a raw problem definition.
        ''' </summary>
        ''' <param name="problem">The training data, which contains the feature vectors and the label values.</param>
        ''' <param name="par">The training parameters of the support vector machine.</param>
        ''' <returns>
        ''' A <see cref="SVMModel"/> object which contains the trained model, the 
        ''' range transform and the label encoder.
        ''' </returns>
        Public Shared Function getSvmModel(problem As Problem, par As Parameter) As SVMModel
            Dim transform As RangeTransform = RangeTransform.Compute(problem)
            Dim scale = transform.Scale(problem)
            Dim model As SVM.Model = Training.Train(scale, par)

            Call Logging.flush()

            Return New SVMModel With {
                .transform = transform,
                .model = model,
                .factors = New ClassEncoder(problem.Y)
            }
        End Function
    End Class
End Namespace
