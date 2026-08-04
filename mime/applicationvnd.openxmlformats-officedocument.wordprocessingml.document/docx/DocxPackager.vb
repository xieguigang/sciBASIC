' ============================================================================
' DocxPackager.vb - .docx 文件打包器
'
' 将 WordDocument 的内部状态序列化为符合 Office Open XML (OOXML) 规范的
' .docx 文件。.docx 本质上是一个 ZIP 包，包含以下 XML 文件：
'
'   [Content_Types].xml          - 内容类型声明
'   _rels/.rels                  - 根关系
'   word/document.xml            - 文档主体
'   word/styles.xml              - 样式定义
'   word/settings.xml            - 文档设置 (含 TOC 自动更新)
'   word/_rels/document.xml.rels - 文档关系 (图片等)
'   word/media/imageN.ext        - 图片文件
'   docProps/core.xml            - 核心属性 (作者、标题等)
'   docProps/app.xml             - 应用属性
' ============================================================================

Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Globalization

''' <summary>
''' .docx 文件打包器。
''' </summary>
Public Class DocxPackager

    ' OOXML XML 命名空间
    Private Const NS_W As String = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
    Private Const NS_R As String = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
    Private Const NS_WP As String = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
    Private Const NS_A As String = "http://schemas.openxmlformats.org/drawingml/2006/main"
    Private Const NS_PIC As String = "http://schemas.openxmlformats.org/drawingml/2006/picture"

    ''' <summary>
    ''' 将 WordDocument 保存为 .docx 文件。
    ''' </summary>
    Public Sub Save(doc As WordDocument, filePath As String)
        ' 确保输出目录存在
        Dim dir As String = Path.GetDirectoryName(filePath)
        If dir <> "" AndAlso Not Directory.Exists(dir) Then
            Directory.CreateDirectory(dir)
        End If

        Using fs As New FileStream(filePath, FileMode.Create)
            Using archive As New ZipArchive(fs, ZipArchiveMode.Create)
                ' 1. [Content_Types].xml
                WriteEntry(archive, "[Content_Types].xml", BuildContentTypes(doc))

                ' 2. _rels/.rels
                WriteEntry(archive, "_rels/.rels", BuildRootRels())

                ' 3. word/document.xml
                WriteEntry(archive, "word/document.xml", BuildDocumentXml(doc))

                ' 4. word/styles.xml
                WriteEntry(archive, "word/styles.xml", BuildStylesXml(doc))

                ' 5. word/settings.xml
                WriteEntry(archive, "word/settings.xml", BuildSettingsXml())

                ' 6. word/_rels/document.xml.rels
                WriteEntry(archive, "word/_rels/document.xml.rels", BuildDocumentRels(doc))

                ' 7. docProps/core.xml
                WriteEntry(archive, "docProps/core.xml", BuildCoreProps(doc))

                ' 8. docProps/app.xml
                WriteEntry(archive, "docProps/app.xml", BuildAppProps(doc))

                ' 9. 图片文件
                For Each img As WordDocument.ImageEntry In doc.GetImages()
                    Dim contentType As String = GetImageContentType(img.Extension)
                    ' 根据关系 ID 确定文件名
                    Dim relNum As Integer = Integer.Parse(img.RelId.Replace("rId", ""))
                    Dim imgPath As String = $"word/media/image{relNum - 2}.{img.Extension}"
                    Dim entry As ZipArchiveEntry = archive.CreateEntry(imgPath)
                    Using es As Stream = entry.Open()
                        es.Write(img.Data, 0, img.Data.Length)
                    End Using
                Next
            End Using
        End Using
    End Sub

    ' ========================================================================
    ' XML 生成
    ' ========================================================================

    ''' <summary>[Content_Types].xml</summary>
    Private Function BuildContentTypes(doc As WordDocument) As String
        Dim sb As New StringBuilder()
        sb.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
        sb.Append("<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">")
        sb.Append("<Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>")
        sb.Append("<Default Extension=""xml"" ContentType=""application/xml""/>")

        ' 根据实际包含的图片扩展名添加
        Dim exts As New HashSet(Of String)()
        For Each img As WordDocument.ImageEntry In doc.GetImages()
            exts.Add(img.Extension.ToLower())
        Next
        For Each ext As String In exts
            sb.Append($"<Default Extension=""{ext}"" ContentType=""{GetImageContentType(ext)}""/>")
        Next

        sb.Append("<Override PartName=""/word/document.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml""/>")
        sb.Append("<Override PartName=""/word/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml""/>")
        sb.Append("<Override PartName=""/word/settings.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml""/>")
        sb.Append("<Override PartName=""/docProps/core.xml"" ContentType=""application/vnd.openxmlformats-package.core-properties+xml""/>")
        sb.Append("<Override PartName=""/docProps/app.xml"" ContentType=""application/vnd.openxmlformats-officedocument.extended-properties+xml""/>")
        sb.Append("</Types>")
        Return sb.ToString()
    End Function

    ''' <summary>_rels/.rels</summary>
    Private Function BuildRootRels() As String
        Dim sb As New StringBuilder()
        sb.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
        sb.Append("<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">")
        sb.Append("<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""word/document.xml""/>")
        sb.Append("<Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"" Target=""docProps/core.xml""/>")
        sb.Append("<Relationship Id=""rId3"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties"" Target=""docProps/app.xml""/>")
        sb.Append("</Relationships>")
        Return sb.ToString()
    End Function

    ''' <summary>word/_rels/document.xml.rels</summary>
    Private Function BuildDocumentRels(doc As WordDocument) As String
        Dim sb As New StringBuilder()
        sb.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
        sb.Append("<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">")
        sb.Append("<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"" Target=""styles.xml""/>")
        sb.Append("<Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings"" Target=""settings.xml""/>")

        ' 图片关系
        For Each img As WordDocument.ImageEntry In doc.GetImages()
            Dim relNum As Integer = Integer.Parse(img.RelId.Replace("rId", ""))
            sb.Append($"<Relationship Id=""{img.RelId}"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/image"" Target=""media/image{relNum - 2}.{img.Extension}""/>")
        Next

        sb.Append("</Relationships>")
        Return sb.ToString()
    End Function

    ''' <summary>word/document.xml</summary>
    Private Function BuildDocumentXml(doc As WordDocument) As String
        Dim sb As New StringBuilder()
        sb.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
        sb.Append($"<w:document xmlns:w=""{NS_W}"" xmlns:r=""{NS_R}"" xmlns:wp=""{NS_WP}"" xmlns:a=""{NS_A}"" xmlns:pic=""{NS_PIC}"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"">")
        sb.Append("<w:body>")
        sb.Append(doc.GetBodyXml())

        ' 节属性 (页面大小和边距)
        Dim margins = doc.GetMargins()
        sb.Append("<w:sectPr>")
        sb.Append($"<w:pgSz w:w=""{doc.GetPageWidth()}"" w:h=""{doc.GetPageHeight()}""/>")
        sb.Append($"<w:pgMar w:top=""{margins.Top}"" w:right=""{margins.Right}"" w:bottom=""{margins.Bottom}"" w:left=""{margins.Left}"" w:header=""720"" w:footer=""720"" w:gutter=""0""/>")
        sb.Append("</w:sectPr>")

        sb.Append("</w:body>")
        sb.Append("</w:document>")
        Return sb.ToString()
    End Function

    ''' <summary>word/styles.xml</summary>
    Private Function BuildStylesXml(doc As WordDocument) As String
        Dim sb As New StringBuilder()
        sb.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
        sb.Append($"<w:styles xmlns:w=""{NS_W}"">")

        ' 文档默认值
        Dim defStyle As WordStyle = doc.GetDefaultStyle()
        sb.Append("<w:docDefaults><w:rPrDefault><w:rPr>")
        sb.Append($"<w:rFonts w:ascii=""{defStyle.FontName}"" w:eastAsia=""{defStyle.FontNameEastAsia}"" w:hAnsi=""{defStyle.FontName}"" w:cs=""{defStyle.FontName}""/>")
        sb.Append($"<w:sz w:val=""{CInt(defStyle.Size * 2)}""/>")
        sb.Append($"<w:szCs w:val=""{CInt(defStyle.Size * 2)}""/>")
        sb.Append($"<w:lang w:val=""zh-CN"" w:eastAsia=""zh-CN""/>")
        sb.Append("</w:rPr></w:rPrDefault></w:docDefaults>")

        ' Normal 样式
        sb.Append("<w:style w:type=""paragraph"" w:default=""1"" w:styleId=""Normal"">")
        sb.Append("<w:name w:val=""Normal""/>")
        sb.Append("<w:qFormat/>")
        sb.Append("</w:style>")

        ' 标题样式 (Title)
        Dim ts As WordStyle = doc.GetTitleStyle()
        sb.Append("<w:style w:type=""paragraph"" w:styleId=""Title"">")
        sb.Append("<w:name w:val=""Title""/>")
        sb.Append("<w:basedOn w:val=""Normal""/>")
        sb.Append("<w:qFormat/>")
        sb.Append("<w:pPr>")
        sb.Append($"<w:spacing w:before=""{PtToTwip(ts.SpaceBefore)}"" w:after=""{PtToTwip(ts.SpaceAfter)}"" w:line=""{CInt(ts.LineSpacing * 240)}"" w:lineRule=""auto""/>")
        sb.Append($"<w:jc w:val=""{ts.Alignment}""/>")
        sb.Append("</w:pPr><w:rPr>")
        sb.Append($"<w:rFonts w:ascii=""{ts.FontName}"" w:eastAsia=""{ts.FontNameEastAsia}"" w:hAnsi=""{ts.FontName}""/>")
        If ts.Bold Then sb.Append("<w:b/>")
        sb.Append($"<w:color w:val=""{ts.ForeColor}""/>")
        sb.Append($"<w:sz w:val=""{CInt(ts.Size * 2)}""/>")
        sb.Append("</w:rPr></w:style>")

        ' 标题 1-6
        Dim headings As WordStyle() = doc.GetHeadingStyles()
        For i As Integer = 0 To 5
            Dim hs As WordStyle = headings(i)
            Dim level As Integer = i + 1
            sb.Append($"<w:style w:type=""paragraph"" w:styleId=""Heading{level}"">")
            sb.Append($"<w:name w:val=""heading {level}""/>")
            sb.Append("<w:basedOn w:val=""Normal""/>")
            sb.Append("<w:next w:val=""Normal""/>")
            sb.Append("<w:qFormat/>")
            sb.Append("<w:pPr>")
            sb.Append($"<w:keepNext/>")
            sb.Append($"<w:spacing w:before=""{PtToTwip(hs.SpaceBefore)}"" w:after=""{PtToTwip(hs.SpaceAfter)}"" w:line=""{CInt(hs.LineSpacing * 240)}"" w:lineRule=""auto""/>")
            sb.Append($"<w:outlineLvl w:val=""{i}""/>")
            sb.Append("</w:pPr><w:rPr>")
            sb.Append($"<w:rFonts w:ascii=""{hs.FontName}"" w:eastAsia=""{hs.FontNameEastAsia}"" w:hAnsi=""{hs.FontName}""/>")
            If hs.Bold Then sb.Append("<w:b/>")
            If hs.Italic Then sb.Append("<w:i/>")
            sb.Append($"<w:color w:val=""{hs.ForeColor}""/>")
            sb.Append($"<w:sz w:val=""{CInt(hs.Size * 2)}""/>")
            sb.Append($"<w:szCs w:val=""{CInt(hs.Size * 2)}""/>")
            sb.Append("</w:rPr></w:style>")
        Next

        ' TOC 标题样式
        sb.Append("<w:style w:type=""paragraph"" w:styleId=""TOCHeading"">")
        sb.Append("<w:name w:val=""TOC Heading""/>")
        sb.Append("<w:basedOn w:val=""Normal""/>")
        sb.Append("<w:next w:val=""Normal""/>")
        sb.Append("<w:qFormat/>")
        sb.Append("<w:pPr><w:spacing w:before=""240"" w:after=""120""/></w:pPr>")
        sb.Append("<w:rPr><w:b/><w:sz w:val=""28""/><w:color w:val=""1F4D78""/></w:rPr>")
        sb.Append("</w:style>")

        sb.Append("</w:styles>")
        Return sb.ToString()
    End Function

    ''' <summary>word/settings.xml</summary>
    Private Function BuildSettingsXml() As String
        Dim sb As New StringBuilder()
        sb.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
        sb.Append($"<w:settings xmlns:w=""{NS_W}"">")
        ' 自动更新域（让 Word 打开时自动更新 TOC）
        sb.Append("<w:updateFields w:val=""true""/>")
        sb.Append("</w:settings>")
        Return sb.ToString()
    End Function

    ''' <summary>docProps/core.xml</summary>
    Private Function BuildCoreProps(doc As WordDocument) As String
        Dim sb As New StringBuilder()
        sb.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
        sb.Append("<cp:coreProperties xmlns:cp=""http://schemas.openxmlformats.org/package/2006/metadata/core-properties""")
        sb.Append(" xmlns:dc=""http://purl.org/dc/elements/1.1/""")
        sb.Append(" xmlns:dcterms=""http://purl.org/dc/terms/""")
        sb.Append(" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">")
        If doc.Title <> "" Then sb.Append($"<dc:title>{XEsc(doc.Title)}</dc:title>")
        If doc.Author <> "" Then sb.Append($"<dc:creator>{XEsc(doc.Author)}</dc:creator>")
        If doc.Subject <> "" Then sb.Append($"<dc:subject>{XEsc(doc.Subject)}</dc:subject>")
        If doc.Description <> "" Then sb.Append($"<dc:description>{XEsc(doc.Description)}</dc:description>")
        If doc.Tags IsNot Nothing AndAlso doc.Tags.Length > 0 Then
            sb.Append($"<cp:keywords>{XEsc(String.Join("; ", doc.Tags))}</cp:keywords>")
        End If
        Dim now As String = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
        sb.Append($"<dcterms:created xsi:type=""dcterms:W3CDTF"">{now}</dcterms:created>")
        sb.Append($"<dcterms:modified xsi:type=""dcterms:W3CDTF"">{now}</dcterms:modified>")
        sb.Append("</cp:coreProperties>")
        Return sb.ToString()
    End Function

    ''' <summary>docProps/app.xml</summary>
    Private Function BuildAppProps(doc As WordDocument) As String
        Dim sb As New StringBuilder()
        sb.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
        sb.Append("<Properties xmlns=""http://schemas.openxmlformats.org/officeDocument/2006/extended-properties""")
        sb.Append(" xmlns:vt=""http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes"">")
        sb.Append($"<Application>{XEsc(doc.ApplicationName)}</Application>")
        sb.Append("</Properties>")
        Return sb.ToString()
    End Function

    ' ========================================================================
    ' 辅助函数
    ' ========================================================================

    Private Sub WriteEntry(archive As ZipArchive, entryPath As String, content As String)
        Dim entry As ZipArchiveEntry = archive.CreateEntry(entryPath)
        Using es As Stream = entry.Open()
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(content)
            es.Write(bytes, 0, bytes.Length)
        End Using
    End Sub

    Private Function GetImageContentType(ext As String) As String
        Select Case ext.ToLower()
            Case "png" : Return "image/png"
            Case "jpg", "jpeg" : Return "image/jpeg"
            Case "bmp" : Return "image/bmp"
            Case "gif" : Return "image/gif"
            Case "emf" : Return "image/x-emf"
            Case "wmf" : Return "image/x-wmf"
            Case "tiff", "tif" : Return "image/tiff"
            Case Else : Return "application/octet-stream"
        End Select
    End Function

    Private Shared Function PtToTwip(pt As Double) As Integer
        Return CInt(pt * 20)
    End Function

    Private Shared Function XEsc(text As String) As String
        If String.IsNullOrEmpty(text) Then Return ""
        Return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("""", "&quot;")
    End Function

End Class
