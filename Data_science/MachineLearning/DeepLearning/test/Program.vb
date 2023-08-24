Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports ds = Microsoft.VisualBasic.MachineLearning.CNN.Dataset.Dataset

Public Class RunCNN

    Public Shared Sub runCnn()
        Dim builder As LayerBuilder = New LayerBuilder()
        Dim output_width = 10

        builder.buildInputLayer(New Size(28, 28))
        builder.buildConvLayer(6, New Size(5, 5))
        builder.buildSampLayer(New Size(2, 2))
        builder.buildConvLayer(12, New Size(5, 5))
        builder.buildSampLayer(New Size(2, 2))
        builder.buildOutputLayer(output_width)
        Dim cnn As CNN = New CNN(builder, 50)

        Dim fileName = "\GCModeller\src\R-sharp\test\demo\machineLearning\umap\NIST-text\train.format"
        Dim dataset As ds = ds.load(fileName, ",", 784)
        cnn.train(dataset, 5)
        Dim modelName = "model/model.cnn"
        cnn.saveModel(modelName)
        dataset.clear()
        dataset = Nothing

        ' CNN cnn = CNN.loadModel(modelName);	
        Dim testset = ds.load("\GCModeller\src\R-sharp\test\demo\machineLearning\umap\NIST-text\test.format", ",", -1)
        cnn.predict(testset, "\GCModeller\src\R-sharp\test\demo\machineLearning\umap\NIST-text\test.predict")
    End Sub
End Class

Module Program
    Sub Main(args As String())
        RunCNN.runCnn()
    End Sub
End Module
