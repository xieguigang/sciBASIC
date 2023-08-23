Imports System.IO
Imports Microsoft.VisualBasic.MachineLearning.CNN.Dataset
Imports Microsoft.VisualBasic.MachineLearning.CNN.Util
Imports std = System.Math

Namespace CNN

    Public Class CNN

        Private Shared ALPHA As Double = 0.85
        Protected Friend Const LAMBDA As Double = 0
        Private layers As IList(Of Layer)
        Private layerNum As Integer
        Private batchSize As Integer
        Private [end] As Integer
        Private divide_batchSize As [Operator]
        Private multiply_alpha As [Operator]
        Private multiply_lambda As [Operator]

        Public Sub New(layerBuilder As LayerBuilder, batchSize As Integer, classNumber As Integer)
            layers = layerBuilder.mLayers
            layerNum = layers.Count
            Me.batchSize = batchSize
            Me.end = classNumber
            setup(batchSize)
            initPerator()
        End Sub

        Private Sub initPerator()
            divide_batchSize = Function(value) value / batchSize
            multiply_alpha = Function(value) value * ALPHA
            multiply_lambda = Function(value) value * (1 - LAMBDA * ALPHA)
        End Sub

        Public Overridable Sub train(trainset As Dataset.Dataset, repeat As Integer)
            Dim t = 0

            While t < repeat AndAlso Not stopTrain
                Dim epochsNum As Integer = trainset.size() / batchSize
                If trainset.size() Mod batchSize <> 0 Then
                    epochsNum += 1
                End If
                Log.i("")
                Call Log.i(t.ToString() & "th iter epochsNum:" & epochsNum.ToString())
                Dim right = 0
                Dim count = 0
                For i = 0 To epochsNum - 1
                    Dim randPerm As Integer() = Util.randomPerm(trainset.size(), batchSize)
                    Call Layer.prepareForNewBatch()

                    For Each index In randPerm
                        Dim isRight = train(trainset.getRecord(index))
                        If isRight Then
                            right += 1
                        End If
                        count += 1
                        Call Layer.prepareForNewRecord()
                    Next

                    updateParas()
                    If i Mod 50 = 0 Then
                        Console.Write("..")
                        If i + 50 > epochsNum Then
                            Console.WriteLine()
                        End If
                    End If
                Next
                Dim p = 1.0 * right / count
                If t Mod 10 = 1 AndAlso p > 0.96 Then
                    ALPHA = 0.001 + ALPHA * 0.9
                    Call Log.i("Set alpha = " & ALPHA.ToString())
                End If
                Call Log.i("precision " & right.ToString() & "/" & count.ToString() & "=" & p.ToString())
                t += 1
            End While
        End Sub

        Private Shared stopTrain As Boolean

        Public Overridable Function test(trainset As Dataset.Dataset) As Double
            Call Layer.prepareForNewBatch()
            Dim iter As IEnumerator(Of Record) = trainset.iter()
            Dim right = 0
            While iter.MoveNext()
                Dim record = iter.Current
                forward(record)
                Dim outputLayer = layers(layerNum - 1)
                Dim mapNum = outputLayer.OutMapNum
                Dim out = New Double(mapNum - 1) {}
                For m = 0 To mapNum - 1
                    Dim outmap = outputLayer.getMap(m)
                    out(m) = outmap(0)(0)
                Next
                If record.Lable.Value = Util.getMaxIndex(out) Then
                    right += 1
                End If
            End While
            Dim p As Double = 1.0 * right / trainset.size()
            Call Log.i("precision", p.ToString() & "")
            Return p
        End Function

        Public Overridable Sub predict(testset As Dataset.Dataset, fileName As String)
            Log.i("begin predict")
            Try
                Dim max = layers(layerNum - 1).ClassNum
                Dim writer As StreamWriter = New StreamWriter(File.OpenRead(fileName))
                Call Layer.prepareForNewBatch()
                Dim iter As IEnumerator(Of Record) = testset.iter()
                While iter.MoveNext()
                    Dim record = iter.Current
                    forward(record)
                    Dim outputLayer = layers(layerNum - 1)

                    Dim mapNum = outputLayer.OutMapNum
                    Dim out = New Double(mapNum - 1) {}
                    For m = 0 To mapNum - 1
                        Dim outmap = outputLayer.getMap(m)
                        out(m) = outmap(0)(0)
                    Next
                    ' int lable =
                    ' Util.binaryArray2int(out);
                    Dim lable = Util.getMaxIndex(out)
                    ' if (lable >= max)
                    ' lable = lable - (1 << (out.length -
                    ' 1));
                    writer.WriteLine(lable.ToString() & vbLf)
                End While
                writer.Flush()
                writer.Close()
            Catch e As IOException
                ' throw new Exception(e);
            End Try
            Log.i("end predict")
        End Sub

        Private Function isSame(output As Double(), target As Double()) As Boolean
            Dim r = True
            For i = 0 To output.Length - 1
                If std.Abs(output(i) - target(i)) > 0.5 Then
                    r = False
                    Exit For
                End If
            Next

            Return r
        End Function

        Private Function train(record As Record) As Boolean
            forward(record)
            Dim result = backPropagation(record)
            Return result
            ' System.exit(0);
        End Function

        Private Function backPropagation(record As Record) As Boolean
            Dim result = setOutLayerErrors(record)
            setHiddenLayerErrors()
            Return result
        End Function

        Private Sub updateParas()
            For l = 1 To layerNum - 1
                Dim layer = layers(l)
                Dim lastLayer = layers(l - 1)
                Select Case layer.Type
                    Case Layer.LayerType.conv, Layer.LayerType.output
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

            For j = start To [end] - 1
                Dim [error] = Util.sum(errors, j)
                ' ����ƫ��
                Dim deltaBias = Util.sum([error]) / batchSize
                Dim bias = layer.getBias(j) + ALPHA * deltaBias
                layer.setBias(j, bias)
            Next

        End Sub


        Private Sub updateKernels(layer As Layer, lastLayer As Layer)
            Dim mapNum = layer.OutMapNum
            Dim lastMapNum = lastLayer.OutMapNum

            For j = start To [end] - 1
                For i = 0 To lastMapNum - 1
                    Dim deltaKernel As Double()() = Nothing
                    For r = 0 To batchSize - 1
                        Dim [error] = layer.getError(r, j)
                        If deltaKernel Is Nothing Then
                            deltaKernel = Util.convnValid(lastLayer.getMap(r, i), [error]) ' �ۻ����
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
                    Case Layer.LayerType.samp
                        setSampErrors(layer, nextLayer)
                    Case Layer.LayerType.conv
                        setConvErrors(layer, nextLayer)
                    Case Else
                End Select
            Next
        End Sub

        Private Sub setSampErrors(layer As Layer, nextLayer As Layer)
            Dim mapNum = layer.OutMapNum
            Dim nextMapNum = nextLayer.OutMapNum

            For i = start To [end] - 1
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

            For m = start To [end] - 1
                Dim scale = nextLayer.ScaleSize
                Dim nextError = nextLayer.getError(m)
                Dim map = layer.getMap(m)
                Dim outMatrix = Util.matrixOp(map, Util.cloneMatrix(map), Nothing, Util.one_value, Util.multiply)
                outMatrix = Util.matrixOp(outMatrix, Util.kronecker(nextError, scale), Nothing, Nothing, Util.multiply)
                layer.setError(m, outMatrix)
            Next

        End Sub

        Private Function setOutLayerErrors(record As Record) As Boolean

            Dim outputLayer = layers(layerNum - 1)
            Dim mapNum = outputLayer.OutMapNum
            ' double[] target =
            ' record.getDoubleEncodeTarget(mapNum);
            ' double[] outmaps = new double[mapNum];
            ' for (int m = 0; m < mapNum; m++) {
            ' double[][] outmap = outputLayer.getMap(m);
            ' double output = outmap[0][0];
            ' outmaps[m] = output;
            ' double errors = output * (1 - output) *
            ' (target[m] - output);
            ' outputLayer.setError(m, 0, 0, errors);
            ' }
            ' // ��ȷ
            ' if (isSame(outmaps, target))
            ' return true;
            ' return false;

            Dim target = New Double(mapNum - 1) {}
            Dim outmaps = New Double(mapNum - 1) {}
            For m = 0 To mapNum - 1
                Dim outmap = outputLayer.getMap(m)
                outmaps(m) = outmap(0)(0)

            Next
            Dim lable As Integer = record.Lable.Value
            target(lable) = 1
            ' Log.i(record.getLable() + "outmaps:" +
            ' Util.fomart(outmaps)
            ' + Arrays.toString(target));
            For m = 0 To mapNum - 1
                outputLayer.setError(m, 0, 0, outmaps(m) * (1 - outmaps(m)) * (target(m) - outmaps(m)))
            Next
            Return lable = Util.getMaxIndex(outmaps)
        End Function

        Private Sub forward(record As Record)
            InLayerOutput = record
            For l = 1 To layers.Count - 1
                Dim layer = layers(l)
                Dim lastLayer = layers(l - 1)
                Select Case layer.Type
                    Case Layer.LayerType.conv
                        setConvOutput(layer, lastLayer)
                    Case Layer.LayerType.samp
                        setSampOutput(layer, lastLayer)
                    Case Layer.LayerType.output
                        setConvOutput(layer, lastLayer)
                    Case Else
                End Select
            Next
        End Sub

        Private WriteOnly Property InLayerOutput As Record
            Set(value As Record)
                Dim inputLayer = layers(0)
                Dim mapSize = inputLayer.MapSize
                Dim attr = value.Attrs
                If attr.Length <> mapSize.x * mapSize.y Then
                    Throw New Exception()
                End If
                For i = 0 To mapSize.x - 1
                    For j = 0 To mapSize.y - 1
                        inputLayer.setMapValue(0, i, j, attr(mapSize.x * i + j))
                    Next
                Next
            End Set
        End Property

        Private Sub setConvOutput(layer As Layer, lastLayer As Layer)
            Dim mapNum = layer.OutMapNum
            Dim lastMapNum = lastLayer.OutMapNum

            For j = start To [end] - 1
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

                sum = Util.matrixOp(sum, Function(value) Util.sigmod(value + bias))
                layer.setMapValue(j, sum)
            Next

        End Sub

        Private Sub setSampOutput(layer As Layer, lastLayer As Layer)
            Dim lastMapNum = lastLayer.OutMapNum

            For i = start To [end] - 1
                Dim lastMap = lastLayer.getMap(i)
                Dim scaleSize = layer.ScaleSize
                Dim sampMatrix = Util.scaleMatrix(lastMap, scaleSize)
                layer.setMapValue(i, sampMatrix)
            Next

        End Sub

        Public Overridable Sub setup(batchSize As Integer)
            Dim inputLayer = layers(0)

            inputLayer.initOutmaps(batchSize)
            For i = 1 To layers.Count - 1
                Dim layer = layers(i)
                Dim frontLayer = layers(i - 1)
                Dim frontMapNum = frontLayer.OutMapNum
                Select Case layer.Type
                    Case Layer.LayerType.input
                    Case Layer.LayerType.conv
                        ' ����map�Ĵ�С
                        layer.MapSize = frontLayer.MapSize.subtract(layer.KernelSize, 1)
                        ' ��ʼ������ˣ�����frontMapNum*outMapNum�������

                        layer.initKernel(frontMapNum)
                        ' ��ʼ��ƫ�ã�����frontMapNum*outMapNum��ƫ��
                        layer.initBias(frontMapNum)
                        ' batch��ÿ����¼��Ҫ����һ�ݲв�
                        layer.initErros(batchSize)
                        ' ÿһ�㶼��Ҫ��ʼ�����map
                        layer.initOutmaps(batchSize)
                    Case Layer.LayerType.samp
                        ' �������map��������һ����ͬ
                        layer.OutMapNum = frontMapNum
                        ' ������map�Ĵ�С����һ��map�Ĵ�С����scale��С
                        layer.MapSize = frontLayer.MapSize.divide(layer.ScaleSize)
                        ' batch��ÿ����¼��Ҫ����һ�ݲв�
                        layer.initErros(batchSize)
                        ' ÿһ�㶼��Ҫ��ʼ�����map
                        layer.initOutmaps(batchSize)
                    Case Layer.LayerType.output
                        ' ��ʼ��Ȩ�أ�����ˣ��������ľ���˴�СΪ��һ���map��С
                        layer.initOutputKerkel(frontMapNum, frontLayer.MapSize)
                        ' ��ʼ��ƫ�ã�����frontMapNum*outMapNum��ƫ��
                        layer.initBias(frontMapNum)
                        ' batch��ÿ����¼��Ҫ����һ�ݲв�
                        layer.initErros(batchSize)
                        ' ÿһ�㶼��Ҫ��ʼ�����map
                        layer.initOutmaps(batchSize)
                End Select
            Next
        End Sub

        Public Class LayerBuilder
            Friend mLayers As IList(Of Layer)

            Public Sub New()
                mLayers = New List(Of Layer)()
            End Sub

            Public Sub New(layer As Layer)
                Me.New()
                mLayers.Add(layer)
            End Sub

            Public Overridable Function addLayer(layer As Layer) As LayerBuilder
                mLayers.Add(layer)
                Return Me
            End Function
        End Class

        Public Overridable Sub saveModel(fileName As String)
            'ObjectOutputStream oos = new ObjectOutputStream(new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write));
            'oos.writeObject(this);
            'oos.flush();
            'oos.close();
            Try
            Catch e As IOException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
            End Try

        End Sub

        Public Shared Function loadModel(fileName As String) As CNN
            'ObjectInputStream @in = new ObjectInputStream(new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read));
            'CNN cnn = (CNN) @in.readObject();
            '@in.close();
            'return cnn;
            Try
            Catch e As Exception
                'e.printStackTrace();
            End Try
            Return Nothing
        End Function
    End Class

End Namespace
