Imports System.Text

Namespace CNN

    Public Class Layer
        ''' 

        Private typeField As LayerType
        Private outMapNumField As Integer
        Private mapSizeField As Size
        Private kernelSizeField As Size
        Private scaleSizeField As Size
        Private kernelField As Double()()()()
        Private bias As Double()

        Private outmaps As Double()()()()

        Private errorsField As Double()()()()

        Private Shared recordInBatch As Integer = 0

        Private classNumField As Integer = -1

        Private Sub New()

        End Sub

        Public Shared Sub prepareForNewBatch()
            recordInBatch = 0
        End Sub

        Public Shared Sub prepareForNewRecord()
            recordInBatch += 1
        End Sub

        Public Shared Function buildInputLayer(mapSize As Size) As Layer
            Dim layer As Layer = New Layer()
            layer.typeField = LayerType.input
            layer.outMapNumField = 1
            layer.MapSize = mapSize
            Return layer
        End Function

        Public Shared Function buildConvLayer(outMapNum As Integer, kernelSize As Size) As Layer
            Dim layer As Layer = New Layer()
            layer.typeField = LayerType.conv
            layer.outMapNumField = outMapNum
            layer.kernelSizeField = kernelSize
            Return layer
        End Function

        Public Shared Function buildSampLayer(scaleSize As Size) As Layer
            Dim layer As Layer = New Layer()
            layer.typeField = LayerType.samp
            layer.scaleSizeField = scaleSize
            Return layer
        End Function

        Public Shared Function buildOutputLayer(classNum As Integer) As Layer
            Dim layer As Layer = New Layer()
            layer.classNumField = classNum
            layer.typeField = LayerType.output
            layer.mapSizeField = New Size(1, 1)
            layer.outMapNumField = classNum
            ' int outMapNum = 1;
            ' while ((1 << outMapNum) < classNum)
            ' outMapNum += 1;
            ' layer.outMapNum = outMapNum;
            Call Log.i("outMapNum:" & layer.outMapNumField.ToString())
            Return layer
        End Function

        Public Overridable Property MapSize As Size
            Get
                Return mapSizeField
            End Get
            Set(value As Size)
                mapSizeField = value
            End Set
        End Property

        Public Overridable ReadOnly Property Type As LayerType
            Get
                Return typeField
            End Get
        End Property

        Public Overridable Property OutMapNum As Integer
            Get
                Return outMapNumField
            End Get
            Set(value As Integer)
                outMapNumField = value
            End Set
        End Property

        Public Overridable ReadOnly Property KernelSize As Size
            Get
                Return kernelSizeField
            End Get
        End Property

        Public Overridable ReadOnly Property ScaleSize As Size
            Get
                Return scaleSizeField
            End Get
        End Property

        Public Enum LayerType
            input
            output
            conv
            samp
        End Enum

        Public Class Size

            Friend Const serialVersionUID As Long = -209157832162004118L
            Public ReadOnly x As Integer
            Public ReadOnly y As Integer

            Public Sub New(x As Integer, y As Integer)
                Me.x = x
                Me.y = y
            End Sub

            Public Overrides Function ToString() As String
                Dim s As StringBuilder = (New StringBuilder("Size(")).Append(" x = ").Append(x).Append(" y= ").Append(y).Append(")")
                Return s.ToString()
            End Function

            Public Overridable Function divide(scaleSize As Size) As Size
                Dim x As Integer = Me.x / scaleSize.x
                Dim y As Integer = Me.y / scaleSize.y
                If x * scaleSize.x <> Me.x OrElse y * scaleSize.y <> Me.y Then
                    Throw New Exception(ToString() & "��������" & scaleSize.ToString())
                End If
                Return New Size(x, y)
            End Function

            Public Overridable Function subtract(size As Size, append As Integer) As Size
                Dim x = Me.x - size.x + append
                Dim y = Me.y - size.y + append
                Return New Size(x, y)
            End Function
        End Class

        Public Overridable Sub initKernel(frontMapNum As Integer)
            kernelField = ReturnRectangularDoubleArray(frontMapNum, outMapNumField, kernelSizeField.x, kernelSizeField.y)
            For i = 0 To frontMapNum - 1
                For j = 0 To outMapNumField - 1
                    kernelField(i)(j) = Util.randomMatrix(kernelSizeField.x, kernelSizeField.y, True)
                Next
            Next
        End Sub

        Public Overridable Sub initOutputKerkel(frontMapNum As Integer, size As Size)
            kernelSizeField = size
            kernelField = ReturnRectangularDoubleArray(frontMapNum, outMapNumField, kernelSizeField.x, kernelSizeField.y)

            For i = 0 To frontMapNum - 1
                For j = 0 To outMapNumField - 1
                    kernelField(i)(j) = Util.randomMatrix(kernelSizeField.x, kernelSizeField.y, False)
                Next
            Next
        End Sub

        Public Overridable Sub initBias(frontMapNum As Integer)
            bias = Util.randomArray(outMapNumField)
        End Sub

        Public Overridable Sub initOutmaps(batchSize As Integer)
            outmaps = ReturnRectangularDoubleArray(batchSize, outMapNumField, mapSizeField.x, mapSizeField.y)
        End Sub

        Public Overridable Sub setMapValue(mapNo As Integer, mapX As Integer, mapY As Integer, value As Double)
            outmaps(recordInBatch)(mapNo)(mapX)(mapY) = value
        End Sub

        Friend Shared count As Integer = 0

        Public Overridable Sub setMapValue(mapNo As Integer, outMatrix As Double()())
            ' Log.i(type.toString());
            ' Util.printMatrix(outMatrix);
            outmaps(recordInBatch)(mapNo) = outMatrix
        End Sub

        Public Overridable Function getMap(index As Integer) As Double()()
            Return outmaps(recordInBatch)(index)
        End Function

        Public Overridable Function getKernel(i As Integer, j As Integer) As Double()()
            Return kernelField(i)(j)
        End Function

        Public Overridable Sub setError(mapNo As Integer, mapX As Integer, mapY As Integer, value As Double)
            errorsField(recordInBatch)(mapNo)(mapX)(mapY) = value
        End Sub

        Public Overridable Sub setError(mapNo As Integer, matrix As Double()())
            ' Log.i(type.toString());
            ' Util.printMatrix(matrix);
            errorsField(recordInBatch)(mapNo) = matrix
        End Sub

        Public Overridable Function getError(mapNo As Integer) As Double()()
            Return errorsField(recordInBatch)(mapNo)
        End Function

        Public Overridable ReadOnly Property Errors As Double()()()()
            Get
                Return errorsField
            End Get
        End Property

        Public Overridable Sub initErros(batchSize As Integer)
            errorsField = ReturnRectangularDoubleArray(batchSize, outMapNumField, mapSizeField.x, mapSizeField.y)
        End Sub

        Public Overridable Sub setKernel(lastMapNo As Integer, mapNo As Integer, kernel As Double()())
            kernelField(lastMapNo)(mapNo) = kernel
        End Sub

        Public Overridable Function getBias(mapNo As Integer) As Double
            Return bias(mapNo)
        End Function

        Public Overridable Sub setBias(mapNo As Integer, value As Double)
            bias(mapNo) = value
        End Sub

        Public Overridable ReadOnly Property Maps As Double()()()()
            Get
                Return outmaps
            End Get
        End Property

        Public Overridable Function getError(recordId As Integer, mapNo As Integer) As Double()()
            Return errorsField(recordId)(mapNo)
        End Function

        Public Overridable Function getMap(recordId As Integer, mapNo As Integer) As Double()()
            Return outmaps(recordId)(mapNo)
        End Function

        Public Overridable ReadOnly Property ClassNum As Integer
            Get
                Return classNumField
            End Get
        End Property

        Public Overridable ReadOnly Property Kernel As Double()()()()
            Get
                Return kernelField
            End Get
        End Property

    End Class

End Namespace
