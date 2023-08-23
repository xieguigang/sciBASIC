Imports Microsoft.VisualBasic.MachineLearning.CNN.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.Layer
Imports ds = Microsoft.VisualBasic.MachineLearning.CNN.Dataset.Dataset

Namespace CNN

    Public Class RunCNN

        Public Shared Sub runCnn()
            Dim builder As LayerBuilder = New LayerBuilder()
            Dim output_width = 10

            builder.addLayer(Layer.buildInputLayer(New Size(28, 28)))
            builder.addLayer(Layer.buildConvLayer(6, New Size(5, 5)))
            builder.addLayer(Layer.buildSampLayer(New Size(2, 2)))
            builder.addLayer(Layer.buildConvLayer(12, New Size(5, 5)))
            builder.addLayer(Layer.buildSampLayer(New Size(2, 2)))
            builder.addLayer(Layer.buildOutputLayer(output_width))
            Dim cnn As CNN = New CNN(builder, 50, output_width)

            Dim fileName = "dataset/train.format"
            Dim dataset As ds = ds.load(fileName, ",", 784)
            cnn.train(dataset, 3)
            Dim modelName = "model/model.cnn"
            cnn.saveModel(modelName)
            dataset.clear()
            dataset = Nothing

            ' CNN cnn = CNN.loadModel(modelName);	
            Dim testset = ds.load("dataset/test.format", ",", -1)
            cnn.predict(testset, "dataset/test.predict")
        End Sub
    End Class
End Namespace
