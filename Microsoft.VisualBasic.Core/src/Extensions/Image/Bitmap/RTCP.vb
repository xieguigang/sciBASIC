Imports System.Drawing
Imports System.Numerics
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports std = System.Math

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' .NET Implement of Real-time Contrast Preserving Decolorization
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/IntPtrZero/RTCPRGB2Gray/tree/master
    ''' </remarks>
    Public Module RTCP

        ''' <summary>
        ''' .NET Implement of Real-time Contrast Preserving Decolorization
        ''' </summary>
        ''' <param name="inBitmap"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RTCPGray(ByVal inBitmap As Bitmap) As Bitmap
            '-----缩放64*64-----
            Dim scale As Single = 64 / std.Sqrt(inBitmap.Width * inBitmap.Height)
            Dim bp As New Bitmap(CInt(inBitmap.Width * scale), CInt(inBitmap.Height * scale))
            Dim g As Graphics = Graphics.FromImage(bp)
            g.InterpolationMode = InterpolationMode.NearestNeighbor
            g.DrawImage(inBitmap, New Rectangle(0, 0, bp.Width, bp.Height))
            g.Dispose()

            '-----灰度权重-----
            Dim sigma = 0.05!
            Dim sigma_pow As Single = sigma ^ 2
            Dim W As New List(Of Vector3)
            For i = 0 To 10
                For j = 0 To 10 - i
                    Dim k = 10 - i - j
                    W.Add(New Vector3(i / 10.0!, j / 10.0!, k / 10.0!))
                Next
            Next

            Dim bpData As BitmapData
            Dim bpBuffer() As Byte
            Dim stride As Integer
            bpData = bp.LockBits(New Rectangle(0, 0, bp.Width, bp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
            stride = std.Abs(bpData.Stride)
            ReDim bpBuffer(stride * bpData.Height - 1)
            Marshal.Copy(bpData.Scan0, bpBuffer, 0, bpBuffer.Length)
            bp.UnlockBits(bpData)

            '-----打乱像素-----
            Dim ran As Random
            Dim temp(bpBuffer.Length / 4 - 1) As Integer
            For i = 0 To temp.Length - 1
                temp(i) = i
            Next
            Dim shuffleBuffer(bpBuffer.Length - 1) As Byte
            For i = 0 To temp.Length - 1
                ran = New Random
                Dim pos = ran.Next(0, temp.Length - i)
                shuffleBuffer(i * 4) = bpBuffer(temp(pos) * 4)
                shuffleBuffer(i * 4 + 1) = bpBuffer(temp(pos) * 4 + 1)
                shuffleBuffer(i * 4 + 2) = bpBuffer(temp(pos) * 4 + 2)
                temp(pos) = temp(temp.Length - i - 1)
            Next

            '-----全局对比度-----
            Dim Delta As New List(Of Single)
            Dim P As New List(Of Vector3)
            For i = 0 To bpBuffer.Length - 1 Step 4
                Dim bB = CInt(bpBuffer(i)) - shuffleBuffer(i)
                Dim bG = CInt(bpBuffer(i + 1)) - shuffleBuffer(i + 1)
                Dim bR = CInt(bpBuffer(i + 2)) - shuffleBuffer(i + 2)
                Dim gB = bB / 255.0!
                Dim gG = bG / 255.0!
                Dim gR = bR / 255.0!
                Dim d = std.Sqrt((bB ^ 2 + bG ^ 2 + bR ^ 2) >> 1) / 255.0!
                If d < sigma Then
                    Continue For
                End If
                P.Add(New Vector3(gR, gG, gB))
                Delta.Add(d)
            Next

            '-----X轴对比度-----
            For j = 0 To bp.Height - 1
                For i = 0 To bp.Width - 2
                    Dim bB = CInt(bpBuffer(j * stride + i * 4)) - bpBuffer(j * stride + (i + 1) * 4)
                    Dim bG = CInt(bpBuffer(j * stride + i * 4 + 1)) - bpBuffer(j * stride + (i + 1) * 4 + 1)
                    Dim bR = CInt(bpBuffer(j * stride + i * 4 + 2)) - bpBuffer(j * stride + (i + 1) * 4 + 2)
                    Dim xB = bB / 255.0!
                    Dim xG = bG / 255.0!
                    Dim xR = bR / 255.0!
                    Dim d = std.Sqrt((bB ^ 2 + bG ^ 2 + bR ^ 2) >> 1) / 255.0!
                    If d < sigma Then
                        Continue For
                    End If
                    P.Add(New Vector3(xR, xG, xB))
                    Delta.Add(d)
                Next
            Next

            '-----Y轴对比度-----
            For i = 0 To bp.Width - 1
                For j = 0 To bp.Height - 2
                    Dim bB = CInt(bpBuffer(j * stride + i * 4)) - bpBuffer((j + 1) * stride + i * 4)
                    Dim bG = CInt(bpBuffer(j * stride + i * 4 + 1)) - bpBuffer((j + 1) * stride + i * 4 + 1)
                    Dim bR = CInt(bpBuffer(j * stride + i * 4 + 2)) - bpBuffer((j + 1) * stride + i * 4 + 2)
                    Dim yB = bB / 255.0!
                    Dim yG = bG / 255.0!
                    Dim yR = bR / 255.0!
                    Dim d = std.Sqrt((bB ^ 2 + bG ^ 2 + bR ^ 2) >> 1) / 255.0!
                    If d < sigma Then
                        Continue For
                    End If
                    P.Add(New Vector3(yR, yG, yB))
                    Delta.Add(d)
                Next
            Next

            Dim L(P.Count - 1, W.Count - 1) As Single
            Dim M1(P.Count - 1, W.Count - 1), M2(P.Count - 1, W.Count - 1) As Single
            For i = 0 To P.Count - 1
                For j = 0 To W.Count - 1
                    L(i, j) = P(i).X * W(j).X + P(i).Y * W(j).Y + P(i).Z * W(j).Z
                    M1(i, j) = L(i, j) + Delta(i)
                    M2(i, j) = L(i, j) - Delta(i)
                Next
            Next

            Dim U(P.Count - 1, W.Count - 1) As Single
            For i = 0 To P.Count - 1
                For j = 0 To W.Count - 1
                    U(i, j) = std.Log(std.E ^ (-(M1(i, j) ^ 2 / sigma_pow)) + std.E ^ (-(M2(i, j) ^ 2 / sigma_pow)), 2)
                Next
            Next

            Dim E(W.Count - 1) As Single
            For j = 0 To W.Count - 1
                For i = 0 To P.Count - 1
                    E(j) += U(i, j)
                Next
                E(j) /= P.Count
            Next

            Dim maxE As Single = E(0)
            Dim index As Integer = 0
            For i = 1 To E.Length - 1
                If maxE < E(i) Then
                    maxE = E(i)
                    index = i
                End If
            Next

            bp = inBitmap.Clone
            bpData = bp.LockBits(New Rectangle(0, 0, bp.Width, bp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
            ReDim bpBuffer(std.Abs(bpData.Stride) * bpData.Height - 1)
            Marshal.Copy(bpData.Scan0, bpBuffer, 0, bpBuffer.Length)
            For i = 0 To bpBuffer.Length - 1 Step 4
                Dim gray = W(index).X * bpBuffer(i + 2) + W(index).Y * bpBuffer(i + 1) + W(index).Z * bpBuffer(i)
                bpBuffer(i) = gray
                bpBuffer(i + 1) = gray
                bpBuffer(i + 2) = gray
            Next
            Marshal.Copy(bpBuffer, 0, bpData.Scan0, bpBuffer.Length)
            bp.UnlockBits(bpData)
            Return bp
        End Function
    End Module
End Namespace