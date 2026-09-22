#Region "Microsoft.VisualBasic::541b819a4093237abc5fc66c675c0040, Data_science\MachineLearning\MachineLearning\SVM\StorageProcedure\Models\SvmModelJSON.vb"

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

    '   Total Lines: 196
    '    Code Lines: 128 (65.31%)
    ' Comment Lines: 34 (17.35%)
    '    - Xml Docs: 97.06%
    ' 
    '   Blank Lines: 34 (17.35%)
    '     File Size: 7.29 KB


    '     Class supportNodeVector
    ' 
    '         Properties: index, value
    ' 
    '         Function: CreateNodes, CreateVector
    ' 
    '     Class Model
    ' 
    '         Properties: classLabels, dimensionNames, numberOfClasses, numberOfSVPerClass, pairwiseProbabilityA
    '                     pairwiseProbabilityB, parameter, rho, supportVectorCoefficients, supportVectorCount
    '                     supportVectorIndices, supportVectors, trainingSize
    ' 
    '         Function: CreateJSONModel, CreateModel
    ' 
    '     Class SvmModelJSON
    ' 
    '         Properties: factors, gaussianTransform, model, rangeTransform
    ' 
    '         Function: CreateJSONModel, CreateSVMModel, ToString
    ' 
    '     Class SVMMultipleSetJSON
    ' 
    '         Properties: dimensionNames, topics
    ' 
    '         Function: CreateJSONModel, CreateSVMModel, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM.StorageProcedure

    ''' <summary>
    ''' The JSON serializable data model of a support vector: the sparse 
    ''' <see cref="Node"/> array is stored as two separated index/value arrays.
    ''' </summary>
    Public Class supportNodeVector

        ''' <summary>
        ''' The index value of each element of the support vector.
        ''' </summary>
        ''' <returns>An array of the feature index values.</returns>
        Public Property index As Integer()
        ''' <summary>
        ''' The feature value of each element of the support vector.
        ''' </summary>
        ''' <returns>An array of the feature values.</returns>
        Public Property value As Double()

        ''' <summary>
        ''' Restore the sparse <see cref="Node"/> array of this support vector.
        ''' </summary>
        ''' <returns>
        ''' A sequence of the <see cref="Node"/> objects which are produced by 
        ''' zipping the <see cref="index"/> array and the <see cref="value"/> array.
        ''' </returns>
        Public Iterator Function CreateNodes() As IEnumerable(Of Node)
            For i As Integer = 0 To index.Length - 1
                Yield New Node With {
                    .index = index(i),
                    .value = value(i)
                }
            Next
        End Function

        ''' <summary>
        ''' Create the JSON data model from a sparse <see cref="Node"/> array.
        ''' </summary>
        ''' <param name="nodes">The source support vector.</param>
        ''' <returns>A <see cref="supportNodeVector"/> object.</returns>
        Public Shared Function CreateVector(nodes As Node()) As supportNodeVector
            Dim index As Integer() = nodes.Select(Function(n) n.index).ToArray
            Dim value As Double() = nodes.Select(Function(n) n.value).ToArray

            Return New supportNodeVector With {
                .index = index,
                .value = value
            }
        End Function

    End Class

    ''' <summary>
    ''' The JSON serializable data model of a trained LibSVM model.
    ''' </summary>
    Public Class Model

        ''' <summary>
        ''' Parameter object.
        ''' </summary>
        Public Property parameter As Parameter

        ''' <summary>
        ''' Number of classes in the model.
        ''' </summary>
        Public Property numberOfClasses As Integer

        ''' <summary>
        ''' Total number of support vectors.
        ''' </summary>
        Public Property supportVectorCount As Integer

        ''' <summary>
        ''' The support vectors.
        ''' </summary>
        Public Property supportVectors As supportNodeVector()

        ''' <summary>
        ''' The coefficients for the support vectors.
        ''' </summary>
        Public Property supportVectorCoefficients As Double()()

        ''' <summary>
        ''' Values in [1,...,num_training_data] to indicate SVs in the training set
        ''' </summary>
        Public Property supportVectorIndices As Integer()

        ''' <summary>
        ''' Constants in decision functions
        ''' </summary>
        Public Property rho As Double()

        ''' <summary>
        ''' First pairwise probability.
        ''' </summary>
        Public Property pairwiseProbabilityA As Double()

        ''' <summary>
        ''' Second pairwise probability.
        ''' </summary>
        Public Property pairwiseProbabilityB As Double()

        ' for classification only

        ''' <summary>
        ''' Class labels.
        ''' </summary>
        Public Property classLabels As Integer()

        ''' <summary>
        ''' Number of support vectors per class.
        ''' </summary>
        Public Property numberOfSVPerClass As Integer()
        ''' <summary>
        ''' The names of the feature dimensions of the training data.
        ''' </summary>
        ''' <returns>An array of the dimension names.</returns>
        Public Property dimensionNames As String()
        ''' <summary>
        ''' The number of the samples which was used for training this model.
        ''' </summary>
        ''' <returns>An <see cref="Integer"/> value.</returns>
        Public Property trainingSize As Integer

        ''' <summary>
        ''' Create the JSON data model from a trained LibSVM model object.
        ''' </summary>
        ''' <param name="svm">The trained <see cref="SVM.Model"/> object.</param>
        ''' <returns>A <see cref="Model"/> data model which is ready for JSON serialization.</returns>
        Public Shared Function CreateJSONModel(svm As SVM.Model) As Model
            Return New Model With {
                .classLabels = svm.classLabels,
                .dimensionNames = svm.dimensionNames,
                .numberOfClasses = svm.numberOfClasses,
                .numberOfSVPerClass = svm.numberOfSVPerClass,
                .pairwiseProbabilityA = svm.pairwiseProbabilityA,
                .pairwiseProbabilityB = svm.pairwiseProbabilityB,
                .parameter = svm.parameter,
                .rho = svm.rho,
                .supportVectorCoefficients = svm.supportVectorCoefficients,
                .supportVectorCount = svm.supportVectorCount,
                .supportVectorIndices = svm.supportVectorIndices,
                .supportVectors = svm.supportVectors _
                    .Select(AddressOf supportNodeVector.CreateVector) _
                    .ToArray,
                .trainingSize = svm.trainingSize
            }
        End Function

        ''' <summary>
        ''' Restore the trained LibSVM model object from this JSON data model.
        ''' </summary>
        ''' <returns>A <see cref="SVM.Model"/> object which is reconstructed from the JSON data.</returns>
        Public Function CreateModel() As SVM.Model
            Return New SVM.Model With {
                .classLabels = classLabels,
                .dimensionNames = dimensionNames,
                .numberOfClasses = numberOfClasses,
                .numberOfSVPerClass = numberOfSVPerClass,
                .pairwiseProbabilityA = pairwiseProbabilityA,
                .pairwiseProbabilityB = pairwiseProbabilityB,
                .parameter = parameter,
                .rho = rho,
                .supportVectorCoefficients = supportVectorCoefficients,
                .supportVectorCount = supportVectorCount,
                .supportVectorIndices = supportVectorIndices,
                .supportVectors = supportVectors _
                    .Select(Function(v) v.CreateNodes.ToArray) _
                    .ToArray,
                .trainingSize = trainingSize
            }
        End Function
    End Class

    ''' <summary>
    ''' The JSON serializable storage model of a complete <see cref="SVMModel"/> 
    ''' object, which contains the inner model, the range transform and the 
    ''' class label factors.
    ''' </summary>
    Public Class SvmModelJSON

        ''' <summary>
        ''' The JSON data model of the inner LibSVM model.
        ''' </summary>
        ''' <returns>A <see cref="Model"/> object.</returns>
        Public Property model As Model
        ''' <summary>
        ''' The JSON data model of the range transform, it is ``Nothing`` when 
        ''' the transform of the source model is not a <see cref="RangeTransform"/>.
        ''' </summary>
        ''' <returns>A <see cref="RangeTransformModel"/> object.</returns>
        Public Property rangeTransform As RangeTransformModel
        ''' <summary>
        ''' The JSON data model of the gaussian transform, it is ``Nothing`` when 
        ''' the transform of the source model is not a <see cref="GaussianTransform"/>.
        ''' </summary>
        ''' <returns>A <see cref="GaussianTransformModel"/> object.</returns>
        Public Property gaussianTransform As GaussianTransformModel
        ''' <summary>
        ''' The color definition of each class label.
        ''' </summary>
        ''' <returns>An array of the <see cref="ColorClass"/> objects.</returns>
        Public Property factors As ColorClass()

        ''' <summary>
        ''' Create the JSON storage model from a trained <see cref="SVMModel"/> object.
        ''' </summary>
        ''' <param name="svm">The trained support vector machine model.</param>
        ''' <returns>A <see cref="SvmModelJSON"/> object which is ready for JSON serialization.</returns>
        Public Shared Function CreateJSONModel(svm As SVMModel) As SvmModelJSON
            Return New SvmModelJSON With {
                .model = Model.CreateJSONModel(svm.model),
                .factors = svm.factors.Colors,
                .gaussianTransform = If(TypeOf svm.transform Is GaussianTransform, New GaussianTransformModel(svm.transform), Nothing),
                .rangeTransform = If(TypeOf svm.transform Is RangeTransform, New RangeTransformModel(svm.transform), Nothing)
            }
        End Function

        ''' <summary>
        ''' Display this storage model as a json string.
        ''' </summary>
        ''' <returns>A json text which describes this model.</returns>
        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' Restore the trained <see cref="SVMModel"/> object from this JSON 
        ''' storage model.
        ''' </summary>
        ''' <returns>A <see cref="SVMModel"/> object which is reconstructed from the JSON data.</returns>
        Public Function CreateSVMModel() As SVMModel
            Return New SVMModel With {
                .factors = New ClassEncoder(factors),
                .model = model.CreateModel,
                .transform = If(rangeTransform Is Nothing, gaussianTransform.GetTransform, rangeTransform.GetTransform)
            }
        End Function

    End Class

    ''' <summary>
    ''' The JSON serializable storage model of a <see cref="SVMMultipleSet"/> object.
    ''' </summary>
    Public Class SVMMultipleSetJSON

        ''' <summary>
        ''' The names of the feature dimensions of the model set.
        ''' </summary>
        ''' <returns>An array of the dimension names.</returns>
        Public Property dimensionNames As String()
        ''' <summary>
        ''' The JSON storage model of each model in the set, the dictionary key 
        ''' is the name of the target class.
        ''' </summary>
        ''' <returns>A dictionary which maps the class name to its <see cref="SvmModelJSON"/>.</returns>
        Public Property topics As Dictionary(Of String, SvmModelJSON)

        ''' <summary>
        ''' Create the JSON storage model from a <see cref="SVMMultipleSet"/> object.
        ''' </summary>
        ''' <param name="svm">The trained multiple model set.</param>
        ''' <returns>A <see cref="SVMMultipleSetJSON"/> object which is ready for JSON serialization.</returns>
        Public Shared Function CreateJSONModel(svm As SVMMultipleSet) As SVMMultipleSetJSON
            Return New SVMMultipleSetJSON With {
                .dimensionNames = svm.dimensionNames,
                .topics = svm.topics _
                    .ToDictionary(Function(a) a.Key,
                                  Function(a)
                                      Return SvmModelJSON.CreateJSONModel(a.Value)
                                  End Function)
            }
        End Function

        ''' <summary>
        ''' Display this storage model as a json string.
        ''' </summary>
        ''' <returns>A json text which describes this model set.</returns>
        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' Restore the <see cref="SVMMultipleSet"/> object from this JSON 
        ''' storage model.
        ''' </summary>
        ''' <returns>A <see cref="SVMMultipleSet"/> object which is reconstructed from the JSON data.</returns>
        Public Function CreateSVMModel() As SVMMultipleSet
            Return New SVMMultipleSet With {
                .dimensionNames = dimensionNames,
                .topics = topics _
                    .ToDictionary(Function(a) a.Key,
                                  Function(a)
                                      Return a.Value.CreateSVMModel()
                                  End Function)
            }
        End Function

    End Class
End Namespace
