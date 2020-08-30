Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM.StorageProcedure

    Public Class SvmModelJSON

        Public Property model As Model
        Public Property rangeTransform As RangeTransformModel
        Public Property gaussianTransform As GaussianTransformModel
        Public Property factors As ColorClass()

        Public Shared Function CreateJSONModel(svm As SVMModel) As SvmModelJSON
            Return New SvmModelJSON With {
                .model = svm.model,
                .factors = svm.factors.Colors,
                .gaussianTransform = If(TypeOf svm.transform Is GaussianTransform, New GaussianTransformModel(svm.transform), Nothing),
                .rangeTransform = If(TypeOf svm.transform Is RangeTransform, New RangeTransformModel(svm.transform), Nothing)
            }
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function CreateSVMModel() As SVMModel
            Return New SVMModel With {
                .factors = New ClassEncoder(factors),
                .model = model,
                .transform = If(rangeTransform Is Nothing, gaussianTransform.GetTransform, rangeTransform.GetTransform)
            }
        End Function

    End Class

    Public Class SVMMultipleSetJSON

        Public Property dimensionNames As String()
        Public Property topics As Dictionary(Of String, SvmModelJSON)

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

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

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