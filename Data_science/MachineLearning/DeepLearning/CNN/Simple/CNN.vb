Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN.Util
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports layerTypes = Microsoft.VisualBasic.MachineLearning.Convolutional.LayerTypes
Imports std = System.Math

Namespace CNN

    Public Class CNN

        Protected Friend ALPHA As Double = 0.85
        Protected Friend LAMBDA As Double = 0

        Private layers As List(Of Layer)
        Private divide_batchSize As [Operator]
        Private multiply_alpha As [Operator]
        Private multiply_lambda As [Operator]

        Public ReadOnly Property batchSize As Integer

        ''' <summary>
        ''' The layer numbers
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property layerNum As Integer

        Default Public ReadOnly Property Layer(i As Integer) As Layer
            Get
                Return layers(i)
            End Get
        End Property

        Public Sub New(layerBuilder As LayerBuilder, batchSize As Integer,
                       Optional alpha As Double = 0.85,
                       Optional lambda As Double = 1.0E-20)

            Me.layers = layerBuilder
            Me.layerNum = layers.Count
            Me.batchSize = batchSize
            Me.ALPHA = alpha
            Me.LAMBDA = lambda

            If Not layerBuilder.Initialized Then
                Call setup(batchSize)
            End If

            Call initPerator()
        End Sub

        Private Sub initPerator()
            divide_batchSize = Function(value) value / _batchSize
            multiply_alpha = Function(value) value * ALPHA
            multiply_lambda = Function(value) value * (1 - LAMBDA * ALPHA)
        End Sub

        Public Function predict(record As SampleData) As Double()
            Dim outputLayer = layers(layerNum - 1)
            Dim mapNum = outputLayer.OutMapNum
            Dim out = New Double(mapNum - 1) {}
            Dim outmap As Double()()

            Call forward(record)

            For m = 0 To mapNum - 1
                outmap = outputLayer.getMap(m)
                out(m) = outmap(0)(0)
            Next

            Return out
        End Function

        Friend Function train(record As SampleData) As Boolean
            forward(record)
            Return backPropagation(record)
        End Function

        Private Function backPropagation(record As SampleData) As Boolean
            Dim result = setOutLayerErrors(record)
            setHiddenLayerErrors()
            Return result
        End Function

        Friend Sub updateParas()
            For l = 1 To layerNum - 1
                Dim layer = layers(l)
                Dim lastLayer = layers(l - 1)
                Select Case layer.Type
                    Case layerTypes.Convolution, layerTypes.Output
                        updateKernels(layer, lastLayer)
                        updateBias(layer, lastLayer)
                    Case Else
                End Select
            Next
        End Sub

        Const start = 0

        Private Sub updateBias(layer As Layer, lastLayer As Layer)
            Dim errors = layer.Errors
            Dim mapNum = layer.OutMapNum

            For j = start To mapNum - 1
                Dim [error] = Util.sum(errors, j)
                Dim deltaBias = Util.sum([error]) / _batchSize
                Dim bias = layer.getBias(j) + ALPHA * deltaBias

                layer.setBias(j, bias)
            Next
        End Sub

        Private Sub updateKernels(layer As Layer, lastLayer As Layer)
            Dim mapNum = layer.OutMapNum
            Dim lastMapNum = lastLayer.OutMapNum

            For j = start To mapNum - 1
                For i = 0 To lastMapNum - 1
                    Dim deltaKernel As Double()() = Nothing

                    For r = 0 To _batchSize - 1
                        Dim [error] = layer.getError(r, j)
                        If deltaKernel Is Nothing Then
                            deltaKernel = Util.convnValid(lastLayer.getMap(r, i), [error])
                        Else
                            deltaKernel = Util.matrixOp(Util.convnValid(lastLayer.getMap(r, i), [error]), deltaKernel, Nothing, Nothing, Util.plus)
                        End If
                    Next

                    deltaKernel = Util.matrixOp(deltaKernel, divide_batchSize)

                    Dim kernel = layer.getKernel(i, j)
                    deltaKernel = Util.matrixOp(kernel, deltaKernel, multiply_lambda, multiply_alpha, Util.plus)
                    layer.setKernel(i, j, deltaKernel)
                Next
            Next

        End Sub

        Private Sub setHiddenLayerErrors()
            For l = layerNum - 2 To 1 Step -1
                Dim layer = layers(l)
                Dim nextLayer = layers(l + 1)
                Select Case layer.Type
                    Case layerTypes.Pool
                        setPoolErrors(layer, nextLayer)
                    Case layerTypes.Convolution
                        setConvErrors(layer, nextLayer)
                    Case layerTypes.ReLU
                        setReLUErrors(layer, nextLayer)
                    Case layerTypes.SoftMax
                        setSoftMaxErrors(layer, nextLayer)
                    Case Else
                End Select
            Next
        End Sub
        Private Sub setSoftMaxErrors(layer As Layer, nextLayer As Layer)
            Dim mapNum = layer.OutMapNum

            For m = start To mapNum - 1
                Dim nextError = nextLayer.getError(m)
                Dim map = layer.getMap(m)
                Dim outMatrix = Util.matrixOp(map, nextError, Nothing, Nothing, Util.multiply)
                layer.setError(m, outMatrix)
            Next
        End Sub

        Private Sub setReLUErrors(layer As Layer, nextLayer As Layer)
            Dim mapNum = layer.OutMapNum

            For m = start To mapNum - 1
                Dim nextError = nextLayer.getError(m)
                Dim map = layer.getMap(m)
                Dim outMatrix = Util.matrixOp(map, nextError, Nothing, Nothing, Util.multiply)
                layer.setError(m, outMatrix)
            Next
        End Sub

        Private Sub setPoolErrors(layer As Layer, nextLayer As Layer)
            Dim mapNum = layer.OutMapNum
            Dim nextMapNum = nextLayer.OutMapNum

            For i = start To mapNum - 1
                Dim sum As Double()() = Nothing
                For j = 0 To nextMapNum - 1
                    Dim nextError = nextLayer.getError(j)
                    Dim kernel = nextLayer.getKernel(i, j)

                    If sum Is Nothing Then
                        sum = Util.convnFull(nextError, Util.rot180(kernel))
                    Else
                        sum = Util.matrixOp(Util.convnFull(nextError, Util.rot180(kernel)), sum, Nothing, Nothing, Util.plus)
                    End If
                Next
                layer.setError(i, sum)
            Next

        End Sub

        Private Sub setConvErrors(layer As Layer, nextLayer As Layer)
            Dim mapNum = layer.OutMapNum

            For m = start To mapNum - 1
                Dim scale = nextLayer.ScaleSize
                Dim nextError = nextLayer.getError(m)
                Dim map = layer.getMap(m)
                Dim outMatrix = Util.matrixOp(map, Util.cloneMatrix(map), Nothing, Util.one_value, Util.multiply)
                outMatrix = Util.matrixOp(outMatrix, Util.kronecker(nextError, scale), Nothing, Nothing, Util.multiply)
                layer.setError(m, outMatrix)
            Next

        End Sub

        Private Function setOutLayerErrors(record As SampleData) As Boolean
            Dim outputLayer = layers(layerNum - 1)
            Dim mapNum = outputLayer.OutMapNum
            Dim target = New Double(mapNum - 1) {}
            Dim outmaps = New Double(mapNum - 1) {}
            For m = 0 To mapNum - 1
                Dim outmap = outputLayer.getMap(m)
                outmaps(m) = outmap(0)(0)

            Next
            Dim lable As Integer = record.labels(0)
            target(lable) = 1

            For m = 0 To mapNum - 1
                outputLayer.setError(m, 0, 0, outmaps(m) * (1 - outmaps(m)) * (target(m) - outmaps(m)))
            Next
            Return lable = which.Max(outmaps)
        End Function

        Private Sub forward(record As SampleData)
            Call setInLayerOutput(record)

            For l = 1 To layers.Count - 1
                Dim layer = layers(l)
                Dim lastLayer = layers(l - 1)
                Select Case layer.Type
                    Case layerTypes.Convolution
                        setConvOutput(layer, lastLayer)
                    Case layerTypes.Pool
                        setPoolOutput(layer, lastLayer)
                    Case layerTypes.Output
                        setNNOutput(layer, lastLayer)
                    Case layerTypes.ReLU
                        setReLUOutput(layer, lastLayer)
                    Case layerTypes.SoftMax
                        setSoftMaxOutput(layer, lastLayer)
                    Case Else
                End Select
            Next
        End Sub

        Private Sub setInLayerOutput(value As SampleData)
            Dim inputLayer = layers(0)
            Dim mapSize = inputLayer.MapSize
            Dim attr = value.features
            If attr.Length <> mapSize.x * mapSize.y Then
                Throw New Exception()
            End If
            For i = 0 To mapSize.x - 1
                For j = 0 To mapSize.y - 1
                    inputLayer.setMapValue(0, i, j, attr(mapSize.x * i + j))
                Next
            Next
        End Sub

        Private Sub setConvOutput(layer As Layer, lastLayer As Layer)
            Dim mapNum = layer.OutMapNum
            Dim lastMapNum = lastLayer.OutMapNum

            For j = start To mapNum - 1
                Dim sum As Double()() = Nothing
                For i = 0 To lastMapNum - 1
                    Dim lastMap = lastLayer.getMap(i)
                    Dim kernel = layer.getKernel(i, j)
                    If sum Is Nothing Then
                        sum = Util.convnValid(lastMap, kernel)
                    Else
                        sum = Util.matrixOp(Util.convnValid(lastMap, kernel), sum, Nothing, Nothing, Util.plus)
                    End If
                Next

                Dim bias = layer.getBias(j)

                sum = Util.matrixOp(sum, Function(value) Sigmoid.doCall(value + bias))
                layer.setMapValue(j, sum)
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="layer">is the output layer</param>
        ''' <param name="lastLayer"></param>
        Private Sub setNNOutput(layer As Layer, lastLayer As Layer)
            Dim mapNum = layer.OutMapNum
            Dim lastMapNum = lastLayer.OutMapNum

            For j = start To mapNum - 1
                Dim sum As Double()() = Nothing
                For i = 0 To lastMapNum - 1
                    Dim lastMap = lastLayer.getMap(i)
                    Dim kernel = layer.getKernel(i, j)
                    If sum Is Nothing Then
                        sum = Util.convnValid(lastMap, kernel, w:=layer.MapSize.x)
                    Else
                        sum = Util.matrixOp(Util.convnValid(lastMap, kernel, w:=layer.MapSize.x), sum, Nothing, Nothing, Util.plus)
                    End If
                Next

                Dim bias = layer.getBias(j)

                sum = Util.matrixOp(sum, Function(value) Sigmoid.doCall(value + bias))
                layer.setMapValue(j, sum)
            Next
        End Sub


        Private Sub setSoftMaxOutput(layer As Layer, lastLayer As Layer)
            Dim lastMapNum = lastLayer.OutMapNum

            For i = start To lastMapNum - 1
                Dim lastMap = lastLayer.getMap(i)
                Dim bias = layer.getBias(i)
                Dim max As Double = lastMap.IteratesALL.Max
                Dim outMatrix = Util.matrixOp(lastMap, Function(value) std.Exp((value + bias) - max))
                layer.setMapValue(i, outMatrix)
            Next
        End Sub

        Private Sub setReLUOutput(layer As Layer, lastLayer As Layer)
            Dim lastMapNum = lastLayer.OutMapNum

            For i = start To lastMapNum - 1
                Dim lastMap = lastLayer.getMap(i)
                Dim bias = layer.getBias(i)
                Dim outMatrix = Util.matrixOp(lastMap, Function(value) ReLU.ReLU(value + bias))
                layer.setMapValue(i, outMatrix)
            Next
        End Sub

        Private Sub setPoolOutput(layer As Layer, lastLayer As Layer)
            Dim lastMapNum = lastLayer.OutMapNum

            For i = start To lastMapNum - 1
                Dim lastMap = lastLayer.getMap(i)
                Dim scaleSize = layer.ScaleSize
                Dim sampMatrix = Util.scaleMatrix(lastMap, scaleSize)
                layer.setMapValue(i, sampMatrix)
            Next
        End Sub

        Public Overridable Sub setup(batchSize As Integer)
            Call layers(0).initOutmaps(batchSize)

            For i = 1 To layers.Count - 1
                Dim layer = layers(i)
                Dim frontLayer = layers(i - 1)
                Dim frontMapNum = frontLayer.OutMapNum

                Select Case layer.Type
                    Case layerTypes.Input
                    Case layerTypes.Convolution
                        layer.MapSize = frontLayer.MapSize.subtract(layer.KernelSize, 1)
                        layer.initKernel(frontMapNum)
                        layer.initBias(frontMapNum)
                        layer.initErros(batchSize)
                        layer.initOutmaps(batchSize)
                    Case layerTypes.Pool
                        layer.OutMapNum = frontMapNum
                        layer.MapSize = frontLayer.MapSize.divide(layer.ScaleSize)
                        layer.initErros(batchSize)
                        layer.initOutmaps(batchSize)
                    Case layerTypes.Output
                        layer.initOutputKerkel(frontMapNum, frontLayer.MapSize)
                        layer.initBias(frontMapNum)
                        layer.initErros(batchSize)
                        layer.initOutmaps(batchSize)
                    Case layerTypes.ReLU, layerTypes.SoftMax
                        layer.OutMapNum = frontMapNum
                        layer.MapSize = frontLayer.MapSize
                        layer.initBias(frontMapNum)
                        layer.initErros(batchSize)
                        layer.initOutmaps(batchSize)
                End Select
            Next
        End Sub
    End Class
End Namespace
