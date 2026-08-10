#Region "Microsoft.VisualBasic::c3d4e5f60718293a4b5c6d7e8f90123, mime\application%pdf\PdfWriter\PdfImageXObject.vb"

    ' Author:
    ' 
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2026 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

    ' Class PdfImageXObject
    ' 
    '     Function: GetOrCreate, Width, Height, HasAlpha
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' PdfImageXObject.vb - 图片转 PDF XObject
'
' JPEG 走 /DCTDecode 原样嵌入（零解码开销）；
' PNG 解 zlib 后逆向五种行过滤器（None/Sub/Up/Average/Paeth）得 raw RGB，
' 再用 zlib 重新压缩为 /FlateDecode；含 alpha 时拆出 /SMask。
'
' 按文件路径缓存已编码 XObject，避免同图重复解码与重复嵌入导致文件膨胀。
' 不支持的格式打印 [警告] 并跳过（同一文件仅告警一次）。
' ============================================================================

Imports System.IO
Imports System.IO.Compression
Imports std = System.Math

''' <summary>
''' 一个已编码好的图片 XObject，供 <see cref="PdfWriter"/> 作为 /XObject 资源嵌入。
''' </summary>
Public Class PdfImageObject

    ''' <summary>资源名（如 Img1）。</summary>
    Public Name As String
    ''' <summary>图片像素宽。</summary>
    Public Width As Integer
    ''' <summary>图片像素高。</summary>
    Public Height As Integer
    ''' <summary>图像数据（JPEG 原样或 Flate 压缩后的 raw RGB）。</summary>
    Public Data As Byte()
    ''' <summary>是否为 JPEG（决定滤镜类型）。</summary>
    Public IsJpeg As Boolean
    ''' <summary>每分量位数（Flate 用，通常 8）。</summary>
    Public BitsPerComponent As Integer = 8
    ''' <summary>颜色分量数（Flate 用，3=RGB）。</summary>
    Public Components As Integer = 3
    ''' <summary>透明通道 SMask 数据（Flate，可能为空）。</summary>
    Public SMask As Byte() = Nothing

End Class

''' <summary>
''' 图片 XObject 编码与缓存管理。
''' </summary>
Public Class PdfImageXObject

    Private Shared ReadOnly cache As New Dictionary(Of String, PdfImageObject)()
    Private Shared warnedFiles As New HashSet(Of String)()

    ''' <summary>
    ''' 取得或创建指定路径图片的 XObject。失败时返回 Nothing（调用方应跳过并提示）。
    ''' 同一文件仅告警一次。
    ''' </summary>
    Public Shared Function GetOrCreate(filePath As String) As PdfImageObject
        SyncLock cache
            If cache.ContainsKey(filePath) Then
                Return cache(filePath)
            End If

            Dim obj As PdfImageObject = Nothing
            Try
                Dim data = File.ReadAllBytes(filePath)
                Dim ext = Path.GetExtension(filePath).TrimStart("."c).ToLower()

                If ext = "jpg" OrElse ext = "jpeg" Then
                    obj = EncodeJpeg(filePath, data)
                ElseIf ext = "png" Then
                    obj = EncodePng(filePath, data)
                Else
                    Warn(filePath, $"不支持的图片格式，已跳过: {filePath}")
                    Return Nothing
                End If
            Catch ex As Exception
                Warn(filePath, $"图片编码失败，已跳过: {filePath} - {ex.Message}")
                Return Nothing
            End Try

            If obj IsNot Nothing Then
                cache(filePath) = obj
            End If
            Return obj
        End SyncLock
    End Function

    Private Shared Sub Warn(filePath As String, msg As String)
        SyncLock warnedFiles
            If Not warnedFiles.Contains(filePath) Then
                warnedFiles.Add(filePath)
                Console.Error.WriteLine($"[警告] {msg}")
            End If
        End SyncLock
    End Sub

    ''' <summary>封装 JPEG：直接以 /DCTDecode 原样嵌入。</summary>
    Private Shared Function EncodeJpeg(filePath As String, data As Byte()) As PdfImageObject
        Dim dims = Microsoft.VisualBasic.MIME.Office.WordDocument.ImageHelper.ReadImageDimensions(filePath)
        Dim w = If(dims.IsEmpty, 0, dims.Width)
        Dim h = If(dims.IsEmpty, 0, dims.Height)
        Return New PdfImageObject() With {
            .Name = "",
            .Width = w,
            .Height = h,
            .Data = data,
            .IsJpeg = True,
            .BitsPerComponent = 8,
            .Components = 3
        }
    End Function

    ''' <summary>解码 PNG 为 raw RGB（含逆滤波），再以 Flate 压缩嵌入。</summary>
    Private Shared Function EncodePng(filePath As String, data As Byte()) As PdfImageObject
        ' 解析 IHDR
        If data.Length < 33 Then Return Nothing
        Dim w = BE32(data, 16)
        Dim h = BE32(data, 20)
        Dim bitDepth = data(24)
        Dim colorType = data(25)
        If bitDepth <> 8 Then
            Warn(filePath, $"仅支持 8 位 PNG，已跳过: {filePath}")
            Return Nothing
        End If

        Dim channels As Integer
        Select Case colorType
            Case 0 : channels = 1 ' 灰度
            Case 2 : channels = 3 ' RGB
            Case 3 : channels = 1 ' 调色板（转灰度近似）
            Case 4 : channels = 2 ' 灰+alpha
            Case 6 : channels = 4 ' RGBA
            Case Else
                Warn(filePath, $"不支持的 PNG 颜色类型: {colorType}，已跳过: {filePath}")
                Return Nothing
        End Select

        ' 收集 IDAT
        Dim idat As New List(Of Byte)()
        Dim pos = 8
        Do While pos < data.Length
            If pos + 8 > data.Length Then Exit Do
            Dim len = BE32(data, pos)
            Dim chunkType = System.Text.Encoding.ASCII.GetString(data, pos + 4, 4)
            If chunkType = "IDAT" Then
                For i = 0 To len - 1
                    idat.Add(data(pos + 8 + i))
                Next
            End If
            pos += 12 + len
            If chunkType = "IEND" Then Exit Do
        Loop

        ' zlib 解压（复用读取侧 FlateDecode 工具）
        Dim raw = FlateDecode.Decode(idat.ToArray())
        ' 逆滤波得到原始像素（channels 通道）
        Dim stride = w * channels
        Dim unfiltered = Unfilter(raw, w, h, channels)

        ' 构造输出 RGB（丢弃 alpha），并单独抽出 alpha 行
        Dim outRgb As Byte()
        Dim alphaData As Byte() = Nothing
        Dim outStride = w * 3
        ReDim outRgb(w * h * 3 - 1)
        Dim outPos = 0
        Dim alphaStride = w
        If channels = 4 Then
            ReDim alphaData(w * h - 1)
        End If

        For y = 0 To h - 1
            For x = 0 To w - 1
                Dim src = y * stride + x * channels
                Select Case channels
                    Case 1 ' 灰度 -> 复制到 RGB
                        outRgb(outPos) = unfiltered(src)
                        outRgb(outPos + 1) = unfiltered(src)
                        outRgb(outPos + 2) = unfiltered(src)
                    Case 2 ' 灰+alpha -> 灰度 + alpha
                        outRgb(outPos) = unfiltered(src)
                        outRgb(outPos + 1) = unfiltered(src)
                        outRgb(outPos + 2) = unfiltered(src)
                        alphaData(y * alphaStride + x) = unfiltered(src + 1)
                    Case 3 ' 调色板近似为灰度
                        outRgb(outPos) = unfiltered(src)
                        outRgb(outPos + 1) = unfiltered(src)
                        outRgb(outPos + 2) = unfiltered(src)
                    Case 4 ' RGBA
                        outRgb(outPos) = unfiltered(src)
                        outRgb(outPos + 1) = unfiltered(src + 1)
                        outRgb(outPos + 2) = unfiltered(src + 2)
                        alphaData(y * alphaStride + x) = unfiltered(src + 3)
                    Case Else ' 3 RGB
                        outRgb(outPos) = unfiltered(src)
                        outRgb(outPos + 1) = unfiltered(src + 1)
                        outRgb(outPos + 2) = unfiltered(src + 2)
                End Select
                outPos += 3
            Next
        Next

        ' 重新 zlib 压缩（写回 FlateDecode 友好的 zlib 流）
        Dim compressed = ZlibCompress(outRgb)
        Dim aCompressed As Byte() = Nothing
        If alphaData IsNot Nothing Then
            aCompressed = ZlibCompress(alphaData)
        End If

        Return New PdfImageObject() With {
            .Name = "",
            .Width = w,
            .Height = h,
            .Data = compressed,
            .IsJpeg = False,
            .BitsPerComponent = 8,
            .Components = 3,
            .SMask = aCompressed
        }
    End Function

    ''' <summary>逆向 PNG 行滤波器（None/Sub/Up/Average/Paeth）。</summary>
    Private Shared Function Unfilter(raw As Byte(), w As Integer, h As Integer, channels As Integer) As Byte()
        Dim stride = w * channels
        Dim result(stride * h - 1) As Byte
        Dim prevRow(stride - 1) As Byte
        Dim pos = 0
        For y = 0 To h - 1
            If pos >= raw.Length Then Exit For
            Dim filterType = raw(pos)
            pos += 1
            Dim curRow(stride - 1) As Byte
            For x = 0 To stride - 1
                If pos >= raw.Length Then Exit For
                Dim rawByte = raw(pos)
                pos += 1
                Dim left = If(x >= channels, curRow(x - channels), CByte(0))
                Dim up = prevRow(x)
                Dim upLeft = If(x >= channels, prevRow(x - channels), CByte(0))
                Dim val As Integer
                Select Case filterType
                    Case 0 ' None
                        val = rawByte
                    Case 1 ' Sub
                        val = (rawByte + left) And &HFF
                    Case 2 ' Up
                        val = (rawByte + up) And &HFF
                    Case 3 ' Average
                        val = (rawByte + ((CInt(left) + CInt(up)) \ 2)) And &HFF
                    Case 4 ' Paeth
                        val = (rawByte + Paeth(left, up, upLeft)) And &HFF
                    Case Else
                        val = rawByte
                End Select
                curRow(x) = CByte(val)
            Next
            Array.Copy(curRow, 0, result, y * stride, stride)
            prevRow = curRow
        Next
        Return result
    End Function

    Private Shared Function Paeth(a As Byte, b As Byte, c As Byte) As Integer
        Dim ai = CInt(a), bi = CInt(b), ci = CInt(c)
        Dim p = ai + bi - ci
        Dim pa = std.Abs(p - ai)
        Dim pb = std.Abs(p - bi)
        Dim pc = std.Abs(p - ci)
        If pa <= pb AndAlso pa <= pc Then Return ai
        If pb <= pc Then Return bi
        Return ci
    End Function

    ''' <summary>zlib 压缩（与 ImageHelper.ZlibCompress 等价）。</summary>
    Private Shared Function ZlibCompress(data As Byte()) As Byte()
        Using ms As New MemoryStream()
            ms.WriteByte(&H78)
            ms.WriteByte(&H1)
            Using ds As New DeflateStream(ms, CompressionMode.Compress, leaveOpen:=True)
                ds.Write(data, 0, data.Length)
            End Using
            ' Adler32
            Dim a As UInteger = 1, b As UInteger = 0
            For Each c As Byte In data
                a = (a + c) Mod 65521
                b = (b + a) Mod 65521
            Next
            Dim adler = (b << 16) Or a
            ms.WriteByte(CByte((adler >> 24) And &HFF))
            ms.WriteByte(CByte((adler >> 16) And &HFF))
            ms.WriteByte(CByte((adler >> 8) And &HFF))
            ms.WriteByte(CByte(adler And &HFF))
            Return ms.ToArray()
        End Using
    End Function

    Private Shared Function BE32(data As Byte(), offset As Integer) As Integer
        Return (CInt(data(offset)) << 24) Or
               (CInt(data(offset + 1)) << 16) Or
               (CInt(data(offset + 2)) << 8) Or
               CInt(data(offset + 3))
    End Function

End Class
