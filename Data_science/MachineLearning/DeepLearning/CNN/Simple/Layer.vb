Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.MachineLearning.Convolutional
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace CNN

    Public Class Layer

        Friend m_kernel As Double()()()()
        Friend m_outmaps As Double()()()()
        Friend m_errors As Double()()()()
        Friend bias As Double()

        Private Shared recordInBatch As Integer = 0

        Public Overridable Property MapSize As Dimension
        Public Overridable ReadOnly Property Type As LayerTypes
        Public Overridable Property OutMapNum As Integer
        Public Overridable ReadOnly Property KernelSize As Dimension
        Public Overridable ReadOnly Property ScaleSize As Dimension
        Public Overridable ReadOnly Property ClassNum As Integer = -1

        Public Overridable ReadOnly Property Maps As Double()()()()
            Get
                Return m_outmaps
            End Get
        End Property

        Public Overridable ReadOnly Property Errors As Double()()()()
            Get
                Return m_errors
            End Get
        End Property

        Public Overridable ReadOnly Property Kernel As Double()()()()
            Get
                Return m_kernel
            End Get
        End Property

        Private Sub New()
        End Sub

        ''' <summary>
        ''' the constructor for the model binary reader
        ''' </summary>
        ''' <param name="mapsize"></param>
        ''' <param name="kernel"></param>
        ''' <param name="scale"></param>
        ''' <param name="type"></param>
        ''' <param name="outmapSize"></param>
        ''' <param name="classNum"></param>
        Friend Sub New(mapsize As Dimension,
                       kernel As Dimension,
                       scale As Dimension,
                       type As LayerTypes,
                       outmapSize As Integer,
                       classNum As Integer)

            Me.MapSize = mapsize
            Me.Type = type
            Me.KernelSize = kernel
            Me.ScaleSize = scale
            Me.OutMapNum = outmapSize
            Me.ClassNum = classNum
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Type.Description}()"
        End Function

        Public Shared Sub prepareForNewBatch()
            recordInBatch = 0
        End Sub

        Public Shared Sub prepareForNewRecord()
            recordInBatch += 1
        End Sub

        Public Overridable Sub initKernel(frontMapNum As Integer)
            m_kernel = RectangularArray.CubicMatrix(Of Double)(frontMapNum, _OutMapNum, _KernelSize.x, _KernelSize.y)

            For i = 0 To frontMapNum - 1
                For j = 0 To _OutMapNum - 1
                    m_kernel(i)(j) = Util.randomMatrix(_KernelSize.x, _KernelSize.y, True)
                Next
            Next
        End Sub

        Public Overridable Sub initOutputKerkel(frontMapNum As Integer, size As Dimension)
            _KernelSize = size
            m_kernel = RectangularArray.CubicMatrix(Of Double)(frontMapNum, _OutMapNum, _KernelSize.x, _KernelSize.y)

            For i = 0 To frontMapNum - 1
                For j = 0 To _OutMapNum - 1
                    m_kernel(i)(j) = Util.randomMatrix(_KernelSize.x, _KernelSize.y, False)
                Next
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub initBias(frontMapNum As Integer)
            bias = Vector.Zero(_OutMapNum)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub initOutmaps(batchSize As Integer)
            m_outmaps = RectangularArray.CubicMatrix(Of Double)(batchSize, _OutMapNum, _MapSize.x, _MapSize.y)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub setMapValue(mapNo As Integer, mapX As Integer, mapY As Integer, value As Double)
            m_outmaps(recordInBatch)(mapNo)(mapX)(mapY) = value
        End Sub

        Friend Shared count As Integer = 0

        Public Overridable Sub setMapValue(mapNo As Integer, outMatrix As Double()())
            ' Log.i(type.toString());
            ' Util.printMatrix(outMatrix);
            m_outmaps(recordInBatch)(mapNo) = outMatrix
        End Sub

        Public Overridable Function getMap(index As Integer) As Double()()
            Return m_outmaps(recordInBatch)(index)
        End Function

        Public Overridable Function getKernel(i As Integer, j As Integer) As Double()()
            Return m_kernel(i)(j)
        End Function

        Public Overridable Sub setError(mapNo As Integer, mapX As Integer, mapY As Integer, value As Double)
            m_errors(recordInBatch)(mapNo)(mapX)(mapY) = value
        End Sub

        Public Overridable Sub setError(mapNo As Integer, matrix As Double()())
            ' Log.i(type.toString());
            ' Util.printMatrix(matrix);
            m_errors(recordInBatch)(mapNo) = matrix
        End Sub

        Public Overridable Function getError(mapNo As Integer) As Double()()
            Return m_errors(recordInBatch)(mapNo)
        End Function

        Public Overridable Sub initErros(batchSize As Integer)
            m_errors = RectangularArray.CubicMatrix(Of Double)(batchSize, _OutMapNum, _MapSize.x, _MapSize.y)
        End Sub

        Public Overridable Sub setKernel(lastMapNo As Integer, mapNo As Integer, kernel As Double()())
            m_kernel(lastMapNo)(mapNo) = kernel
        End Sub

        Public Overridable Function getBias(mapNo As Integer) As Double
            Return bias(mapNo)
        End Function

        Public Overridable Sub setBias(mapNo As Integer, value As Double)
            bias(mapNo) = value
        End Sub

        Public Overridable Function getError(recordId As Integer, mapNo As Integer) As Double()()
            Return m_errors(recordId)(mapNo)
        End Function

        Public Overridable Function getMap(recordId As Integer, mapNo As Integer) As Double()()
            Return m_outmaps(recordId)(mapNo)
        End Function
    End Class
End Namespace
