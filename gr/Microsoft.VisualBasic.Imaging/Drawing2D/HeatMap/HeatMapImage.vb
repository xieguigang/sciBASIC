Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Namespace HeatMap

	' https://github.com/RainkLH/HeatMapSharp
    Public Class HeatMapImage
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
        ''' </summary>
        Private heatValsField As Double(,)

        ''' <summary>
        ''' Color map matrix
        ''' </summary>
        Private ColorMaps As Color()

        ''' <summary>
        ''' gaussian kernel
        ''' </summary>
        Private kernelField As Double(,)

        ''' <summary>
        ''' color numbers
        ''' </summary>
        Private Const NUMCOLORS As Integer = 1000

        ''' <summary>
        ''' width of img
        ''' </summary>
        Public Property W As Integer
            Get
                Return wField
            End Get
            Set(ByVal value As Integer)
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
            Set(ByVal value As Integer)
                hField = value
            End Set
        End Property

        ''' <summary>
        ''' gaussian kernel
        ''' </summary>
        Public ReadOnly Property Kernel As Double(,)
            Get
                Return kernelField
            End Get
        End Property

        ''' <summary>
        ''' Two dimensional matrix corresponding to data list
        ''' </summary>
        Public ReadOnly Property HeatVals As Double(,)
            Get
                Return heatValsField
            End Get
        End Property

        ''' <summary>
        ''' construction
        ''' </summary>
        ''' <paramname="width">image width</param>
        ''' <paramname="height">image height</param>
        ''' <paramname="gSize">gaussian kernel size</param>
        ''' <paramname="gSigma">gaussian kernel sigma</param>
        Public Sub New(ByVal width As Integer, ByVal height As Integer, ByVal gSize As Integer, ByVal gSigma As Double)
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
            kernelField = New Double(Me.gSize - 1, Me.gSize - 1) {}
            gaussiankernel()
            '初始化高斯累加图
            heatValsField = New Double(hField - 1, wField - 1) {}
        End Sub

        Private Sub gaussiankernel()
            Dim y = -r, i = 0

            While i < gSize
                Dim x = -r, j = 0

                While j < gSize
                    kernelField(i, j) = Math.Exp((x * x + y * y) / (-2 * gSigma * gSigma)) / (2 * Math.PI * gSigma * gSigma)
                    x += 1
                    j += 1
                End While

                y += 1
                i += 1
            End While
        End Sub

        Private Function MultiplyKernel(ByVal weight As Double) As Double(,)
            Dim wKernel As Double(,) = CType(kernelField.Clone(), Double(,))
            For i = 0 To gSize - 1
                For j = 0 To gSize - 1
                    wKernel(i, j) *= weight
                Next
            Next
            Return wKernel
        End Function

        Private Sub RescaleArray()
            Dim max As Single = 0
            For Each value As Single In heatValsField
                If value > max Then
                    max = value
                End If
            Next

            For i = 0 To heatValsField.GetLength(0) - 1
                For j = 0 To heatValsField.GetLength(1) - 1
                    heatValsField(i, j) *= (NUMCOLORS - 1) / max
                    If heatValsField(i, j) > NUMCOLORS - 1 Then
                        heatValsField(i, j) = NUMCOLORS - 1
                    End If
                Next
            Next
        End Sub

        Public Sub SetDatas(ByVal datas As List(Of DataType))
            For Each data As DataType In datas
                Dim i, j, tx, ty, ir, jr As Integer
                Dim radius = gSize >> 1

                Dim x = data.X
                Dim y = data.Y
                Dim kernelMultiplied = MultiplyKernel(data.Weight)

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
                            heatValsField(ty, tx) += kernelMultiplied(i, j)
                        End If
                    Next
                Next
            Next

        End Sub

        Public Sub SetAData(ByVal data As DataType)
            Dim i, j, tx, ty, ir, jr As Integer
            Dim radius = gSize >> 1

            Dim x = data.X
            Dim y = data.Y
            Dim kernelMultiplied = MultiplyKernel(data.Weight)

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
                        heatValsField(ty, tx) += kernelMultiplied(i, j)
                    End If
                Next
            Next
        End Sub

        Public Sub ProcessUsingLockbits(ByVal processedBitmap As Bitmap)
            Dim rect As Rectangle = New Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height)

            Dim heatMapData = processedBitmap.LockBits(rect, ImageLockMode.WriteOnly, processedBitmap.PixelFormat)

            Dim ptrw = heatMapData.Scan0

            Dim wbytes = Math.Abs(heatMapData.Stride) * processedBitmap.Height

            Dim argbValuesW = New Byte(wbytes - 1) {}

            Marshal.Copy(ptrw, argbValuesW, 0, wbytes)


            For i = 0 To hField - 1
                For j = 0 To wField - 1
                    Dim colorIndex = If(Double.IsNaN(heatValsField(i, j)), 0, CInt(heatValsField(i, j)))
                    Dim index = (i * processedBitmap.Width + j) * 4
                    argbValuesW(index) = ColorMaps(4 * colorIndex)
                    argbValuesW(index + 1) = ColorMaps(4 * colorIndex + 1)
                    argbValuesW(index + 2) = ColorMaps(4 * colorIndex + 2)
                    argbValuesW(index + 3) = ColorMaps(4 * colorIndex + 3)
                Next
            Next
            Marshal.Copy(argbValuesW, 0, ptrw, wbytes)
            processedBitmap.UnlockBits(heatMapData)
        End Sub
        Public Function GetHeatMap() As Bitmap
            RescaleArray()
            Dim heatMap As Bitmap = New Bitmap(W, H, PixelFormat.Format32bppArgb)
            ProcessUsingLockbits(heatMap)
            Return heatMap
        End Function
    End Class
End Namespace
