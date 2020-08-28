Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM

    Public Class SVMModel

        Public Property model As Model
        Public Property transform As IRangeTransform

        ''' <summary>
        ''' use for get <see cref="ColorClass"/> based on 
        ''' the prediction result value
        ''' </summary>
        ''' <returns></returns>
        Public Property factors As ClassEncoder

        Public ReadOnly Property DimensionNames As String()
            Get
                Return model.DimensionNames
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return DimensionNames.GetJson
        End Function
    End Class

    Public Class SVMMultipleSet

        Public Property dimensionNames As String()
        Public Property topics As Dictionary(Of String, SVMModel)

    End Class
End Namespace