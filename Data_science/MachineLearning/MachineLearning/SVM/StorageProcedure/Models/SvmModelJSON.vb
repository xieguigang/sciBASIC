Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder

Namespace SVM.StorageProcedure

    Public Class SvmModelJSON

        Public Property model As Model
        Public Property rangeTransform As RangeTransformModel
        Public Property gaussianTransform As GaussianTransformModel
        Public Property factors As ColorClass()

    End Class

    Public Class SVMMultipleSetJSON

        Public Property dimensionNames As String()
        Public Property topics As Dictionary(Of String, SvmModelJSON)

    End Class
End Namespace