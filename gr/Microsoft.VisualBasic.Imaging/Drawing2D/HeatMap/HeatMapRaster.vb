Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap
Imports stdNum = System.Math

Namespace HeatMap

    ''' <summary>
    ''' A helper class or produce heatmap raster matrix data
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/RainkLH/HeatMapSharp
    ''' </remarks>
    Public Class HeatMapRaster(Of T As Pixel)

        ''' <summary>
        ''' width of img
        ''' </summary>
        Private wField As Integer

        ''' <summary>
        ''' height of img
        ''' </summary>
        Private hField As Integer

        ''' <summary>
        ''' gaussian kernel size
        ''' </summary>
        Private gSize As Integer

        ''' <summary>
        ''' gaussian kernel sigma
        ''' </summary>
        Private gSigma As Double

        ''' <summary>
        ''' radius
        ''' </summary>
        Private r As Integer

        ''' <summary>
        ''' Two dimensional matrix corresponding to data list
        ''' 
        ''' the heatmap cells data, transform color scale from
        ''' this matrix data
        ''' </summary>
        Private m_heatMatrix As Double(,)

        ''' <summary>
        ''' gaussian kernel
        ''' </summary>
        Private m_kernelField As Double(,)

        ''' <summary>
        ''' width of img
        ''' </summary>
        Public Property W As Integer
            Get
                Return wField
            End Get
            Set(value As Integer)
                wField = value
            End Set
        End Property

        ''' <summary>
        ''' height of img
        ''' </summary>
        Public Property H As Integer
            Get
                Return hField
            End Get
            Set(value As Integer)
                hField = value
            End Set
        End Property

        ''' <summary>
        ''' gaussian kernel
        ''' </summary>
        Public ReadOnly Property Kernel As Double(,)
            Get
                Return m_kernelField
            End Get
        End Property

        ''' <summary>
        ''' Two dimensional matrix corresponding to data list
        ''' </summary>
        Public ReadOnly Property HeatMatrix As Double(,)
            Get
                Return m_heatMatrix
            End Get
        End Property

        ''' <summary>
        ''' construction
        ''' </summary>
        ''' <param name="width">image width</param>
        ''' <param name="height">image height</param>
        ''' <param name="gSize">gaussian kernel size</param>
        ''' <param name="gSigma">gaussian kernel sigma</param>
        Public Sub New(width As Integer, height As Integer, gSize As Integer, gSigma As Double)
            wField = width
            hField = height

            '对高斯核尺寸进行判断
            If gSize < 3 OrElse gSize > 400 Then
                Throw New Exception("Kernel size is invalid")
            End If
            Me.gSize = If(gSize Mod 2 = 0, gSize + 1, gSize)
            '高斯的sigma值，计算半径r
            r = Me.gSize / 2
            Me.gSigma = gSigma
            '计算高斯核
            m_kernelField = New Double(Me.gSize - 1, Me.gSize - 1) {}
            gaussiankernel()
            '初始化高斯累加图
            m_heatMatrix = New Double(hField - 1, wField - 1) {}
        End Sub

        Private Sub gaussiankernel()
            Dim y = -r, i = 0

            While i < gSize
                Dim x = -r, j = 0

                While j < gSize
                    m_kernelField(i, j) = stdNum.Exp((x * x + y * y) / (-2 * gSigma * gSigma)) / (2 * stdNum.PI * gSigma * gSigma)
                    x += 1
                    j += 1
                End While

                y += 1
                i += 1
            End While
        End Sub

        Private Function MultiplyKernel(weight As Double) As Double(,)
            Dim wKernel As Double(,) = CType(m_kernelField.Clone(), Double(,))
            For i = 0 To gSize - 1
                For j = 0 To gSize - 1
                    wKernel(i, j) *= weight
                Next
            Next
            Return wKernel
        End Function

        ''' <summary>
        ''' set imaging raster pixels data to generate heatmap matrix
        ''' </summary>
        ''' <param name="datas"></param>
        Public Function SetDatas(datas As List(Of T)) As HeatMapRaster(Of T)
            For Each data As Pixel In datas
                Dim i, j, tx, ty, ir, jr As Integer
                Dim radius = gSize >> 1

                Dim x = data.X
                Dim y = data.Y
                Dim kernelMultiplied = MultiplyKernel(data.Scale)

                For i = 0 To gSize - 1
                    ir = i - radius
                    ty = y + ir

                    ' skip row
                    If ty < 0 Then
                        Continue For
                    End If

                    ' break Height
                    If ty >= hField Then
                        Exit For
                    End If

                    ' for each kernel column
                    For j = 0 To gSize - 1
                        jr = j - radius
                        tx = x + jr

                        ' skip column
                        If tx < 0 Then
                            Continue For
                        End If

                        If tx < wField Then
                            m_heatMatrix(ty, tx) += kernelMultiplied(i, j)
                        End If
                    Next
                Next
            Next

            Return Me
        End Function

        Public Iterator Function GetRasterPixels() As IEnumerable(Of Pixel)
            For i = 0 To m_heatMatrix.GetLength(0) - 1
                For j = 0 To m_heatMatrix.GetLength(1) - 1
                    Yield New PixelData With {
                        .X = i,
                        .Y = j,
                        .Scale = m_heatMatrix(i, j)
                    }
                Next
            Next
        End Function
    End Class
End Namespace
