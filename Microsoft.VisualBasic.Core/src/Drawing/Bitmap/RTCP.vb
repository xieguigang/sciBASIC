#Region "Microsoft.VisualBasic::b66fe7762a77257dad24a7bbf543d984, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\RTCP.vb"

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

    '   Total Lines: 227
    '    Code Lines: 165 (72.69%)
    ' Comment Lines: 29 (12.78%)
    '    - Xml Docs: 68.97%
    ' 
    '   Blank Lines: 33 (14.54%)
    '     File Size: 8.90 KB


    '     Module RTCP
    ' 
    '         Function: copy, MeasureGlobalWeight, RTCPGray, RTCPGrayGlobalWeightIndex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports ran = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' .NET Implement of Real-time Contrast Preserving Decolorization
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/IntPtrZero/RTCPRGB2Gray/tree/master
    ''' </remarks>
    Public Module RTCP

        Private Function copy(inBitmap As Bitmap) As Bitmap
            '-----缩放64*64-----
            Dim scale As Single = 64 / std.Sqrt(inBitmap.Width * inBitmap.Height)
            Dim bpw = CInt(inBitmap.Width * scale)
            Dim bph = CInt(inBitmap.Height * scale)

#If NET48 Then
            Dim bp As New Bitmap(bpw, bph)
            Dim g As Graphics = Graphics.FromImage(bp)
            g.InterpolationMode = InterpolationMode.NearestNeighbor
            g.DrawImage(inBitmap, New Rectangle(0, 0, bp.Width, bp.Height))
            g.Dispose()

            Return bp
#Else
            Return inBitmap.Resize(bpw, bph)
#End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bpBuffer"></param>
        ''' <param name="bp"></param>
        ''' <param name="stride"></param>
        ''' <param name="W"></param>
        ''' <param name="sigma"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RTCPGrayGlobalWeightIndex(bpBuffer() As Byte, bp As Size, stride As Integer, W As List(Of Vector3), Optional sigma As Single = 0.05) As Integer
            Dim sigma_pow As Double = sigma ^ 2

            '-----打乱像素-----
            Dim temp(bpBuffer.Length / 4 - 1) As Integer

            For i = 0 To temp.Length - 1
                temp(i) = i
            Next

            Dim shuffleBuffer(bpBuffer.Length - 1) As Byte

            For i = 0 To temp.Length - 1
                Dim pos As Integer = ran.NextInteger(0, temp.Length - i)

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

            Return index
        End Function

        Public Function MeasureGlobalWeight(inBitmap As Bitmap, Optional sigma As Single = 0.05!) As (r As Single, g As Single, b As Single)
            '-----灰度权重-----
            Dim sigma_pow As Single = sigma ^ 2
            Dim W As New List(Of Vector3)

            For i As Integer = 0 To 10
                For j As Integer = 0 To 10 - i
                    Dim k = 10 - i - j
                    W.Add(New Vector3(i / 10.0!, j / 10.0!, k / 10.0!))
                Next
            Next

            Dim bpBuffer() As Byte
            Dim stride As Integer
            Dim bp As Bitmap = copy(inBitmap)

#If NET48 Then
            Dim bpData As BitmapData
            bpData = bp.LockBits(New Rectangle(0, 0, bp.Width, bp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
            stride = std.Abs(bpData.Stride)
            ReDim bpBuffer(stride * bpData.Height - 1)
            Marshal.Copy(bpData.Scan0, bpBuffer, 0, bpBuffer.Length)
            bp.UnlockBits(bpData)
#Else
            bpBuffer = inBitmap.MemoryBuffer.RawBuffer
            stride = inBitmap.MemoryBuffer.Stride
#End If

            Call VBDebugger.EchoLine($"image_src_dims: [{inBitmap.Width},{inBitmap.Height}]")
            Call VBDebugger.EchoLine($"image_copy_dims: [{bp.Width},{bp.Height}]")

            Dim index = bpBuffer.RTCPGrayGlobalWeightIndex(New Size(bp.Width, bp.Height), stride, W, sigma)

            Call VBDebugger.EchoLine($"global_weight: [r:{W(index).X},g:{W(index).Y},b:{W(index).Z}]")

            With W(index)
                Return (.X, .Y, .Z)
            End With
        End Function

        ''' <summary>
        ''' .NET Implement of Real-time Contrast Preserving Decolorization
        ''' </summary>
        ''' <param name="inBitmap"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RTCPGray(inBitmap As Bitmap, Optional sigma As Single = 0.05!) As Bitmap
#If net48 Then

            Dim bp As Bitmap = inBitmap.Clone
            Dim bpData = bp.LockBits(New Rectangle(0, 0, bp.Width, bp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
            Dim w = MeasureGlobalWeight(inBitmap, sigma)

            Using rgbValues As Emit.Marshal.Byte = New Emit.Marshal.Byte(
                p:=bpData.Scan0,
                chunkSize:=std.Abs(bpData.Stride) * bpData.Height
            )
                ' Calls unmanaged memory write when this 
                ' memory pointer was disposed
                Call rgbValues.scanInternal(w.r, w.g, w.b)
            End Using

            Call bp.UnlockBits(bpData)

            Return bp
#Else
            Throw New NotImplementedException
#End If
        End Function
    End Module
End Namespace
