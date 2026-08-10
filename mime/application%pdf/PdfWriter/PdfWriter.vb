#Region "Microsoft.VisualBasic::6b8a554f5c5056eae3efa3b044d4837c, mime\application%pdf\PdfWriter\PdfWriter.vb"

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

    '   Total Lines: 315
    '    Code Lines: 248 (78.73%)
    ' Comment Lines: 34 (10.79%)
    '    - Xml Docs: 14.71%
    ' 
    '   Blank Lines: 33 (10.48%)
    '     File Size: 13.96 KB


    ' Structure PdfWriteMeta
    ' 
    ' 
    ' 
    ' Class PdfWriter
    ' 
    '     Function: BuildFontObject, BuildImageObject, BuildInfo, BuildResourceDict, BuildSMaskObject
    '               CompressZlib, Fmt, IsStreamBody, PdfText
    ' 
    '     Sub: Save, WriteTo
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' PdfWriter.vb - PDF 文件序列化器
'
' 负责间接对象编号分配、对象体写出、/Catalog + /Pages 页面树 + 各 /Page
' （MediaBox / Resources / Contents）、/Info 文档信息字典、内容流 Flate
' 压缩、xref 交叉引用表与 trailer、%%EOF。
'
' 写入过程中记录各对象字节偏移供 xref 使用，避免二次扫描。
' 注意：新增类型名不与 PdfReader 侧既有读取类型冲突（同根命名空间）。
' ============================================================================

Imports System.IO
Imports System.IO.Compression
Imports System.Text

''' <summary>PDF 文档信息字典元数据。</summary>
Public Structure PdfWriteMeta
    Public Author As String
    Public Title As String
    Public Subject As String
    Public Keywords As String
    Public Creator As String
End Structure

''' <summary>PDF 文件序列化器。</summary>
Public Class PdfWriter

    ''' <summary>将排版结果序列化为完整 PDF 并写入文件。写入前自动创建输出目录。</summary>
    Public Shared Sub Save(filePath As String,
                          render As PdfRenderResult,
                          pageWidthPt As Double, pageHeightPt As Double,
                          fonts As PdfFontResource,
                          meta As PdfWriteMeta)
        Dim dir = Path.GetDirectoryName(Path.GetFullPath(filePath))
        If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
            Directory.CreateDirectory(dir)
        End If

        Using fs As New FileStream(filePath, FileMode.Create, FileAccess.Write)
            WriteTo(fs, render, pageWidthPt, pageHeightPt, fonts, meta)
        End Using
    End Sub

    Private Shared Sub WriteTo(fs As FileStream,
                               render As PdfRenderResult,
                               pageW As Double, pageH As Double,
                               fonts As PdfFontResource,
                               meta As PdfWriteMeta)
        Dim enc As Encoding = Encoding.ASCII
        Dim objects As New List(Of (num As Integer, body As Byte()))

        Dim nextNum = 1
        Dim catalogNum = nextNum : nextNum += 1
        Dim pagesNum = nextNum : nextNum += 1

        ' 每页：内容流对象 + 页对象
        Dim pageContentNums As New List(Of Integer)()
        Dim pageNums As New List(Of Integer)()
        For Each content In render.Pages
            Dim cNum = nextNum : nextNum += 1
            Dim pNum = nextNum : nextNum += 1
            pageContentNums.Add(cNum)
            pageNums.Add(pNum)
        Next

        ' 字体对象
        Dim fontNums As New Dictionary(Of String, Integer)()
        For Each f In fonts.GetRegistry()
            fontNums(f.Name) = nextNum
            nextNum += 1
        Next

        ' 图片对象（含 SMask）
        Dim imgNums As New Dictionary(Of String, Integer)()
        Dim smaskNums As New Dictionary(Of String, Integer)()
        For Each img In render.Images
            imgNums(img.Name) = nextNum
            nextNum += 1
            If img.SMask IsNot Nothing Then
                smaskNums(img.Name) = nextNum
                nextNum += 1
            End If
        Next

        ' ---- 组装对象体 ----
        ' Catalog
        objects.Add((catalogNum, enc.GetBytes($"<< /Type /Catalog /Pages {pagesNum} 0 R >>")))

        ' Pages
        Dim kids As New StringBuilder()
        For Each p In pageNums
            kids.Append($"{p} 0 R ")
        Next
        objects.Add((pagesNum, enc.GetBytes($"<< /Type /Pages /Kids [{kids.ToString().Trim()}] /Count {pageNums.Count} >>")))

        ' 资源字典（字体 + 图片，所有页共用同一资源集）
        Dim resDict = BuildResourceDict(fontNums, imgNums)

        ' 页对象与内容流
        For i = 0 To render.Pages.Count - 1
            Dim contentBytes = CompressZlib(Encoding.UTF8.GetBytes(render.Pages(i)))
            Dim contentObj = enc.GetBytes(
                $"<< /Length {contentBytes.Length} /Filter /FlateDecode >>" & vbCrLf &
                "stream" & vbCrLf)
            ' 内容对象 = 头部 + 流数据 + 尾部
            Dim cBody As New List(Of Byte)()
            cBody.AddRange(contentObj)
            cBody.AddRange(contentBytes)
            cBody.AddRange(enc.GetBytes(vbCrLf & "endstream" & vbCrLf & "endobj" & vbCrLf))
            objects.Add((pageContentNums(i), cBody.ToArray()))

            Dim pageObj = enc.GetBytes(
                $"<< /Type /Page /Parent {pagesNum} 0 R " &
                $"/MediaBox [0 0 {Fmt(pageW)} {Fmt(pageH)}] " &
                $"/Contents {pageContentNums(i)} 0 R " &
                $"/Resources {resDict} >>")
            objects.Add((pageNums(i), pageObj))
        Next

        ' 字体对象
        For Each f In fonts.GetRegistry()
            objects.Add((fontNums(f.Name), enc.GetBytes(BuildFontObject(f, fontNums, smaskNums, imgNums))))
        Next

        ' 图片对象（需要先写描述符对象号，故图片在字体后）
        For Each img In render.Images
            Dim body = BuildImageObject(img, imgNums, smaskNums)
            objects.Add((imgNums(img.Name), body))
            If img.SMask IsNot Nothing Then
                objects.Add((smaskNums(img.Name), BuildSMaskObject(img)))
            End If
        Next

        ' Info 字典
        Dim infoNum = nextNum : nextNum += 1
        objects.Add((infoNum, enc.GetBytes(BuildInfo(meta))))

        ' ---- 写出 ----
        Dim header = enc.GetBytes("%PDF-1.4" & vbCrLf & "%âãÏÓ" & vbCrLf)
        fs.Write(header, 0, header.Length)

        Dim offsets(objects.Count - 1) As Long
        For i = 0 To objects.Count - 1
            offsets(i) = fs.Position
            Dim body = objects(i).body
            Dim objHeader = enc.GetBytes($"{objects(i).num} 0 obj ")
            fs.Write(objHeader, 0, objHeader.Length)
            fs.Write(body, 0, body.Length)
            ' 非流对象补 endobj
            If Not IsStreamBody(objects(i).body) Then
                Dim eo = enc.GetBytes("endobj" & vbCrLf)
                fs.Write(eo, 0, eo.Length)
            End If
        Next

        ' xref
        Dim xrefOffset = fs.Position
        Dim total = objects.Count + 1
        Dim xref As New StringBuilder()
        xref.Append($"xref" & vbCrLf)
        xref.Append($"0 {total}" & vbCrLf)
        xref.Append("0000000000 65535 f " & vbCrLf)
        For i = 0 To objects.Count - 1
            xref.Append(offsets(i).ToString("0000000000") & " 00000 n " & vbCrLf)
        Next
        Dim xrefBytes = enc.GetBytes(xref.ToString())
        fs.Write(xrefBytes, 0, xrefBytes.Length)

        ' trailer
        Dim trailer = enc.GetBytes(
            "trailer" & vbCrLf &
            $"<< /Size {total} /Root {catalogNum} 0 R /Info {infoNum} 0 R >>" & vbCrLf &
            "startxref" & vbCrLf &
            $"{xrefOffset}" & vbCrLf &
            "%%EOF" & vbCrLf)
        fs.Write(trailer, 0, trailer.Length)
    End Sub

    Private Shared Function IsStreamBody(body As Byte()) As Boolean
        ' 内容流对象体以 "stream" 结尾（其后紧跟原始数据）
        Dim tail = Encoding.ASCII.GetString(body, System.Math.Max(0, body.Length - 8), System.Math.Min(8, body.Length))
        Return tail.Contains("stream")
    End Function

    Private Shared Function BuildResourceDict(fontNums As Dictionary(Of String, Integer),
                                              imgNums As Dictionary(Of String, Integer)) As String
        Dim sb As New StringBuilder()
        sb.Append("<< /Font << ")
        For Each kv In fontNums
            sb.Append($"/{kv.Key} {kv.Value} 0 R ")
        Next
        sb.Append(">> ")
        If imgNums.Count > 0 Then
            sb.Append("/XObject << ")
            For Each kv In imgNums
                sb.Append($"/{kv.Key} {kv.Value} 0 R ")
            Next
            sb.Append(">> ")
        End If
        sb.Append(">>")
        Return sb.ToString()
    End Function

    Private Shared Function BuildFontObject(f As PdfFontEntry,
                                            fontNums As Dictionary(Of String, Integer),
                                            smaskNums As Dictionary(Of String, Integer),
                                            imgNums As Dictionary(Of String, Integer)) As String
        If f.IsType0 Then
            ' 需要额外的 CIDFontType0 后代字体对象号
            ' 这里为简化：直接内联后代字体（不拆分对象）
            Return "<< /Type /Font /Subtype /Type0 /BaseFont /Adobe-GB1 " &
                   "/Encoding /UniGB-UCS2-H " &
                   "/DescendantFonts [ << /Type /Font /Subtype /CIDFontType0 " &
                   "/BaseFont /Adobe-GB1 " &
                   "/CIDSystemInfo << /Registry (Adobe) /Ordering (GB1) /Supplement 2 >> " &
                   "/FontDescriptor << /FontName /Adobe-GB1 /Flags 4 " &
                   "/FontBBox [-100 -100 1000 1000] /ItalicAngle 0 " &
                   "/Ascent 880 /Descent -120 /CapHeight 880 /StemV 80 >> >> ] >>"
        End If
        Return $"<< /Type /Font /Subtype /Type1 /BaseFont /{f.BaseFont} /Encoding {f.Encoding} >>"
    End Function

    Private Shared Function BuildImageObject(img As PdfImageObject,
                                             imgNums As Dictionary(Of String, Integer),
                                             smaskNums As Dictionary(Of String, Integer)) As Byte()
        Dim enc As Encoding = Encoding.ASCII
        Dim header As String
        If img.IsJpeg Then
            header = $"<< /Type /XObject /Subtype /Image /Width {img.Width} /Height {img.Height} " &
                     "/ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode " &
                     $"/Length {img.Data.Length}"
        Else
            header = $"<< /Type /XObject /Subtype /Image /Width {img.Width} /Height {img.Height} " &
                     "/ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /FlateDecode " &
                     $"/Length {img.Data.Length}"
        End If
        If img.SMask IsNot Nothing Then
            header &= $" /SMask {smaskNums(img.Name)} 0 R"
        End If
        header &= " >>" & vbCrLf & "stream" & vbCrLf

        Dim out As New List(Of Byte)()
        out.AddRange(enc.GetBytes(header))
        out.AddRange(img.Data)
        out.AddRange(enc.GetBytes(vbCrLf & "endstream" & vbCrLf & "endobj" & vbCrLf))
        Return out.ToArray()
    End Function

    Private Shared Function BuildSMaskObject(img As PdfImageObject) As Byte()
        Dim enc As Encoding = Encoding.ASCII
        Dim header = $"<< /Type /XObject /Subtype /Image /Width {img.Width} /Height {img.Height} " &
                     "/ColorSpace /DeviceGray /BitsPerComponent 8 /Filter /FlateDecode " &
                     $"/Length {img.SMask.Length} >>" & vbCrLf & "stream" & vbCrLf
        Dim out As New List(Of Byte)()
        out.AddRange(enc.GetBytes(header))
        out.AddRange(img.SMask)
        out.AddRange(enc.GetBytes(vbCrLf & "endstream" & vbCrLf & "endobj" & vbCrLf))
        Return out.ToArray()
    End Function

    Private Shared Function BuildInfo(meta As PdfWriteMeta) As String
        Dim sb As New StringBuilder()
        sb.Append("<< ")
        If Not String.IsNullOrEmpty(meta.Title) Then sb.Append($"/Title ({PdfText(meta.Title)}) ")
        If Not String.IsNullOrEmpty(meta.Author) Then sb.Append($"/Author ({PdfText(meta.Author)}) ")
        If Not String.IsNullOrEmpty(meta.Subject) Then sb.Append($"/Subject ({PdfText(meta.Subject)}) ")
        If Not String.IsNullOrEmpty(meta.Keywords) Then sb.Append($"/Keywords ({PdfText(meta.Keywords)}) ")
        If Not String.IsNullOrEmpty(meta.Creator) Then sb.Append($"/Creator ({PdfText(meta.Creator)}) ")
        sb.Append(">>")
        Return sb.ToString()
    End Function

    ''' <summary>把元数据文本转为 PDF 字面字符串（转义括号与反斜杠，非 ASCII 以 ? 替代）。</summary>
    Private Shared Function PdfText(s As String) As String
        Dim sb As New StringBuilder()
        For Each c As Char In s
            If c = "\"c OrElse c = "("c OrElse c = ")"c Then
                sb.Append("\"c & c)
            ElseIf AscW(c) < 128 Then
                sb.Append(c)
            Else
                sb.Append("?"c)
            End If
        Next
        Return sb.ToString()
    End Function

    ''' <summary>zlib 压缩（deflate + zlib 头 + adler32），供内容流与图片流复用。</summary>
    Private Shared Function CompressZlib(data As Byte()) As Byte()
        Using ms As New MemoryStream()
            ms.WriteByte(&H78)
            ms.WriteByte(&H1)
            Using ds As New DeflateStream(ms, CompressionMode.Compress, leaveOpen:=True)
                ds.Write(data, 0, data.Length)
            End Using
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

    Private Shared Function Fmt(v As Double) As String
        If System.Math.Abs(v) < 0.001 Then Return "0"
        Return v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
    End Function

End Class
