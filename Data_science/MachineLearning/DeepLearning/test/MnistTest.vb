#Region "Microsoft.VisualBasic::b923ebf59b652c6a4a68d753e864bf6e, Data_science\MachineLearning\DeepLearning\test\MnistTest.vb"

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

    '   Total Lines: 149
    '    Code Lines: 99 (66.44%)
    ' Comment Lines: 28 (18.79%)
    '    - Xml Docs: 17.86%
    ' 
    '   Blank Lines: 22 (14.77%)
    '     File Size: 6.59 KB


    '     Class MnistTest
    ' 
    '         Sub: Main, printPredictions
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Linq
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
            layers.buildReLULayer()
            layers.buildPoolLayer(2, 2, 0)
            layers.buildConvLayer(5, 64, 1, 2)
            layers.buildReLULayer()
            layers.buildPoolLayer(2, 2, 0)
            'layers.buildFullyConnectedLayer(1024)
            'layers.buildLocalResponseNormalizationLayer(5)
            ' layers.buildDropoutLayer()
            layers.buildConv2DTransposeLayer(10, 1, 0)
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
            Dim trainer As TrainerAlgorithm = New AdaGradTrainer(20, 0.001F).SetKernel(net)

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
                Dim n As Integer = 300
                Dim d As Integer = n / 25
                Dim check As New PerformanceCounter

                For Each img In mr.ExtractVectors.Take(n)
                    db.addImageData(img.value, img.value.Max)
                    tr = trainer.train(db, {Val(img.description)}, check.Set)
                    loss += tr.Loss

                    If (++i) Mod d = 0 Then
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
                    db.addImageData(img.value, img.value.Max)
                    Dim correct As Integer = img.description
                    Dim prediction = which.Max(net.predict(db))
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
