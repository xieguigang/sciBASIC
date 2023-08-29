Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure

Namespace ConsoleApp1


    ''' <summary>
    ''' This a test network to try the network on the
    ''' [Mnist dataset](http://yann.lecun.com/exdb/mnist/)
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class MnistTest

        Public Shared Sub Main()
            Dim layers As New LayerBuilder

            '        Reader mr = new MnistReader("mnist/train-labels-idx1-ubyte", "mnist/train-images-idx3-ubyte");
            ' Reader mr = new PGMReader("pgmfiles/train");
            '        Reader mr = new ImageReader("pngfiles/train");
            Dim mr As New MNIST(
                "\GCModeller\src\R-sharp\test\demo\machineLearning\umap\mnist_dataset\train-images-idx3-ubyte",
                "\GCModeller\src\R-sharp\test\demo\machineLearning\umap\mnist_dataset\train-labels-idx1-ubyte"
            )

            layers.buildInputLayer(New Dimension(mr.ImageSize.Width, mr.ImageSize.Height), 1)
            layers.buildConvLayer(5, 32, 1, 2)
            layers.buildRectifiedLinearUnitsLayer()
            layers.buildPoolLayer(2, 2, 0)
            layers.buildConvLayer(5, 64, 1, 2)
            layers.buildRectifiedLinearUnitsLayer()
            layers.buildPoolLayer(2, 2, 0)
            layers.buildFullyConnectedLayer(1024)
            layers.buildLocalResponseNormalizationLayer()
            layers.buildDropoutLayer()
            layers.buildFullyConnectedLayer(10)
            layers.buildSoftmaxLayer()


            ' 
            ' 			layers.add(new InputLayer(def, mr.getSizeX(), mr.getSizeY(), 1));
            ' 			layers.add(new FullyConnectedLayer(def, 500));
            ' 			layers.add(new SoftMaxLayer(def));
            ' 			layers.add(new FullyConnectedLayer(def, 500));
            ' 			layers.add(new SoftMaxLayer(def));
            ' 			layers.add(new FullyConnectedLayer(def, 500));
            ' 			layers.add(new SoftMaxLayer(def));
            ' 			layers.add(new DropoutLayer(def));
            ' 			layers.add(new FullyConnectedLayer(def, 2));
            ' 			layers.add(new SoftMaxLayer(def));
            ' 			
            Dim net As ConvolutionalNN = New ConvolutionalNN(layers)
            Dim trainer As Trainer = New AdaGradTrainer(net, 20, 0.001F)

            'Reader mrTest = new MnistReader("mnist/t10k-labels-idx1-ubyte", "mnist/t10k-images-idx3-ubyte");
            ' Reader mrTest = new PGMReader("pgmfiles/test");
            'Reader mrTest = new ImageReader("pngfiles/test");
            Dim mrTest As MNIST = mr


            Dim start As Long = UnixTimeStamp

            Dim numberDistribution = New Integer(9) {}
            Dim correctPredictions = New Integer(9) {}

            Dim tr As TrainResult = Nothing
            Dim db As DataBlock = New DataBlock(mr.ImageSize.Width, mr.ImageSize.Height, 1, 0)
            For j = 1 To 500
                Dim loss As Double = 0
                Dim i As i32 = 1
                Dim n As Integer = 200

                For Each img In mr.ExtractVectors.Take(n)
                    db.addImageData(img.value.Select(Function(b) CInt(b)).ToArray, img.value.Max)
                    tr = trainer.train(db, img.description)
                    loss += tr.Loss

                    If (++i) Mod 5 = 0 Then
                        Console.WriteLine()
                        Console.WriteLine("Pass " & j.ToString() & " Read images: " & i.ToString())
                        Console.WriteLine("Training time: " & (CLng(UnixTimeStamp) - start).ToString())
                        Console.WriteLine("Loss: " & loss / i.ToString())

                        start = CLng(UnixTimeStamp)
                    End If
                Next


                Console.WriteLine("Loss: " & (loss / n).ToString)
                ' mr.reset()

                start = CLng(UnixTimeStamp)
                correctPredictions.fill(0)
                numberDistribution.fill(0)
                For Each img In mr.ExtractVectors.Take(n)
                    db.addImageData(img.value.Select(Function(b) CInt(b)).ToArray, img.value.Max)
                    net.forward(db, False)
                    Dim correct As Integer = img.description
                    Dim prediction = net.Prediction
                    If correct = prediction Then
                        correctPredictions(correct) += 1
                    End If
                    numberDistribution(correct) += 1
                Next

                Console.WriteLine("Last run:")
                Console.WriteLine("=================================")
                printPredictions(correctPredictions, numberDistribution, i, 10)
            Next
        End Sub

        Private Shared Sub printPredictions(ByVal correctPredictions As Integer(), ByVal numberDistribution As Integer(), ByVal totalSize As Integer, ByVal numOfClasses As Integer)
            Dim sumCorrectPredictions = 0
            For i = 0 To numOfClasses - 1
                Dim sb As StringBuilder = New StringBuilder()
                sb.Append("Number ")
                sb.Append(i)
                sb.Append(" has predictions ")
                sb.Append(correctPredictions(i))
                sb.Append("/")
                sb.Append(numberDistribution(i))
                sb.Append(vbTab & vbTab)
                sb.Append(correctPredictions(i) / CSng(numberDistribution(i)) * 100)
                sb.Append("%")
                Console.WriteLine(sb.ToString())
                sumCorrectPredictions += correctPredictions(i)
            Next
            Dim sb2 As StringBuilder = New StringBuilder()
            sb2.Append("Total correct predictions ")
            sb2.Append(sumCorrectPredictions)
            sb2.Append("/")
            sb2.Append(totalSize)
            sb2.Append(vbTab & vbTab)
            sb2.Append(sumCorrectPredictions / CSng(totalSize) * 100)
            sb2.Append("%")
            Console.WriteLine(sb2.ToString())
        End Sub
    End Class

End Namespace
