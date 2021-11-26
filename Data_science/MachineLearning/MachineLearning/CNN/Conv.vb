Imports System.Threading
Imports stdNum = System.Math

Namespace Convolutional

    Public Class Conv : Inherits Layer

        Public stride As Integer()
        Public weights As Tensor
        Public biases As Tensor

        Public Sub New(inputTensorDims As Integer(), pad As Integer())
            MyBase.New(inputTensorDims, pad)
            type = "Convolution"
            stride = New Integer(1) {}
        End Sub

        Public Overloads Sub setOutputDims()
            Dim newHeight = CInt(stdNum.Floor((InputTensorDims(0) - weights.Dimensions(0)) / stride(0))) + 1
            Dim newWidth = CInt(stdNum.Floor((InputTensorDims(1) - weights.Dimensions(1)) / stride(1))) + 1
            outputDims = New Integer() {newHeight, newWidth, weights.Dimensions(3)}
        End Sub

        Public Overrides Sub feedNext()
            outputTensorMemAlloc()
            Dim inputHeight = InputTensorDims(0)
            Dim inputWidth = InputTensorDims(1)
            Dim filterHeight = weights.Dimensions(0)
            Dim filterWidth = weights.Dimensions(1)
            Dim channelCount = weights.Dimensions(2)
            Dim filterCount = weights.Dimensions(3)
            Dim mCountH = inputHeight - filterHeight + 1
            Dim mCountW = inputWidth - filterWidth + 1
            Dim possibleH As Tensor = New Tensor(New Integer() {outputDims(0), 1})
            Dim j = 0
            Dim i = 0

            While i < mCountH
                possibleH.memPtr(stdNum.Min(Threading.Interlocked.Increment(j), j - 1)) = i
                i += stride(0)
            End While

            Dim possibleW As Tensor = New Tensor(New Integer() {1, outputDims(1)})

            j = 0
            i = 0

            While i < mCountW
                possibleW.memPtr(stdNum.Min(Threading.Interlocked.Increment(j), j - 1)) = i
                i += stride(1)
            End While

            Dim startingIndexes = possibleW + possibleH * inputWidth
            possibleH.Dispose()
            possibleW.Dispose()
            possibleH = New Tensor(New Integer() {filterHeight, 1})

            For i = 0 To filterHeight - 1
                possibleH.memPtr(i) = i
            Next

            possibleW = New Tensor(New Integer() {1, filterWidth})

            For i = 0 To filterWidth - 1
                possibleW.memPtr(i) = i
            Next

            Dim offsets = possibleW + possibleH * inputWidth
            possibleH.Dispose()
            possibleW.Dispose()
            startingIndexes.reshape(New Integer() {startingIndexes.TotalLength, 1})
            offsets.reshape(New Integer() {1, offsets.TotalLength})
            Dim allIndexes = startingIndexes + offsets
            startingIndexes.Dispose()
            offsets.Dispose()
            Dim outputH_W = outputDims(0) * outputDims(1)
            Dim allInOne As Tensor = New Tensor(New Integer() {outputH_W, filterHeight * filterWidth * channelCount})
            Dim h_W = inputHeight * inputWidth
            Dim fH_fW = filterHeight * filterWidth
            Dim h_w_fH_fW = h_W * fH_fW
            Dim tmp As Integer

            'int[] aiInd = new int[] { 0, 0 };
            'int[] aioInd = new int[] { 0, 0 };
            'for (int ch = 0; ch < channelCount; ch++)
            '{
            '    for (int k = 0; k < outputH_W; k++)
            '    {
            '        aioInd[0] = k;
            '        aiInd[0] = k;
            '        for (int m = 0; m < fH_fW; m++)
            '        {
            '            aioInd[1] = ch * fH_fW + m;
            '            aiInd[1] = m;
            '            tmp = (int)allIndexes[aiInd] + h_W * ch;
            '            allInOne[aioInd] = inputTensor.memPtr[tmp];
            '        }
            '    }
            '}

            ' A bit faster:
            Dim aiInd, aioInd As Integer

            For ch = 0 To channelCount - 1
                Dim fH_fW_ch = fH_fW * ch
                Dim h_W_ch = h_W * ch

                For m = 0 To fH_fW - 1
                    aiInd = m * outputH_W
                    aioInd = (fH_fW_ch + m) * outputH_W

                    For k = 0 To outputH_W - 1
                        tmp = CInt(allIndexes.memPtr(stdNum.Min(Threading.Interlocked.Increment(aiInd), aiInd - 1))) + h_W_ch
                        allInOne.memPtr(stdNum.Min(Threading.Interlocked.Increment(aioInd), aioInd - 1)) = inputTensor.memPtr(tmp)
                    Next
                Next
            Next

            allIndexes.Dispose()
            nextLayer.inputTensor.reshape(New Integer() {allInOne.Dimensions(0), filterCount})

            Dim x = allInOne.Dimensions(1)
            Dim y = filterCount
            Dim z = allInOne.Dimensions(0)
            Dim po As ParallelOptions = New ParallelOptions()

            po.MaxDegreeOfParallelism = Environment.ProcessorCount
            Tasks.Parallel.For(0, y, po, Sub(f)
                                             Dim aioInd_ As Integer
                                             Dim outputInd_ As Integer
                                             Dim weightsInd_ As Integer

                                             For g = 0 To z - 1
                                                 Dim sum As Single = 0

                                                 For h = 0 To x - 1
                                                     aioInd_ = h * z + g
                                                     weightsInd_ = f * x + h
                                                     sum += weights.memPtr(weightsInd_) * allInOne.memPtr(aioInd_)
                                                 Next

                                                 outputInd_ = f * z + g
                                                 nextLayer.inputTensor.memPtr(outputInd_) = sum + biases.memPtr(f)
                                             Next
                                         End Sub)

            nextLayer.inputTensor.reshape(outputDims)
            allInOne.Dispose()
            disposeInputTensor()
        End Sub
    End Class
End Namespace
