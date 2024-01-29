'Imports System.IO
'Imports Microsoft.VisualBasic.Linq
'Imports Microsoft.VisualBasic.MachineLearning.CNN
'Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
'Imports ds = test.Dataset

'Public Class RunCNN

'    Shared log As Action(Of String) = AddressOf VBDebugger.EchoLine

'    Public Shared Sub runCnn()
'        Dim builder As LayerBuilder = New LayerBuilder()
'        Dim output_width = 10

'        builder.buildInputLayer(New Dimension(28, 28))
'        builder.buildConvLayer(6, New Dimension(5, 5))
'        builder.buildPoolLayer(New Dimension(2, 2))
'        builder.buildConvLayer(12, New Dimension(5, 5))
'        builder.buildPoolLayer(New Dimension(2, 2))
'        builder.buildOutputLayer(output_width)
'        Dim cnn As CNN = New CNN(builder, 50)

'        Dim fileName = "\GCModeller\src\R-sharp\test\demo\machineLearning\umap\NIST-text\train.format"
'        Dim dataset As ds = ds.load(fileName, ",", 784)
'        Dim trainer As New Trainer
'        trainer.train(cnn, dataset.records.ToArray, 5)
'        Dim modelName = "model/model.cnn"
'        ' cnn.saveModel(modelName)
'        dataset.clear()
'        dataset = Nothing

'        ' CNN cnn = CNN.loadModel(modelName);	
'        Dim testset = ds.load("\GCModeller\src\R-sharp\test\demo\machineLearning\umap\NIST-text\test.format", ",", -1)
'        predict(cnn, testset, "\GCModeller\src\R-sharp\test\demo\machineLearning\umap\NIST-text\test.predict")
'    End Sub

'    Public Shared Sub predict(cnn As CNN, testset As ds, fileName As String)
'        log("begin predict")
'        Try
'            Dim max = cnn(cnn.layerNum - 1).ClassNum
'            Dim writer As StreamWriter = New StreamWriter(fileName.Open(FileMode.OpenOrCreate, doClear:=True))
'            Dim iter As IEnumerator(Of SampleData) = testset.iter()

'            Call Layer.prepareForNewBatch()

'            While iter.MoveNext()
'                Dim record = iter.Current
'                ' int lable =
'                ' Util.binaryArray2int(out);
'                Dim lable = which.Max(cnn.predict(record))
'                ' if (lable >= max)
'                ' lable = lable - (1 << (out.length -
'                ' 1));
'                writer.WriteLine(lable.ToString())
'            End While
'            writer.Flush()
'            writer.Close()
'        Catch e As IOException
'            ' throw new Exception(e);
'        End Try
'        log("end predict")
'    End Sub
'End Class

'Module Program
'    Sub Main2(args As String())
'        RunCNN.runCnn()
'    End Sub
'End Module
