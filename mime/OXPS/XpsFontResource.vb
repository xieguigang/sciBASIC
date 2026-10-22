' ============================================================================
' XpsFontResource.vb - 字体资源（ODTTF 混淆嵌入 / 无字体回退）
'
' XPS 的 <Glyphs> 通过 FontUri 引用包内的字体部件。为了既能被阅读器正确渲染，
' 又不在缺少字体的平台上让整篇文档生成失败，本类实现「可选嵌入 + 自动回退」：
'
'   1. 显式指定的 FontFile；
'   2. 系统字体目录中的候选单一 TTF（simhei / segoeui / arial / consola / DejaVu）；
'   3. 都找不到（或显式关闭嵌入、或命中 .ttc 字体集合）→ 回退为空资源：
'      Glyphs 不写 FontUri，包结构依旧合法、文本依旧可提取。
'
' 嵌入格式为 ODTTF（Obfuscated OpenType）：部件名形如 Resources/Fonts/{GUID}.odttf，
' 字体文件前 32 字节与「由该 GUID 反序得到的 16 字节密钥」循环 XOR。
' ============================================================================

Imports System.IO

''' <summary>
''' XPS 字体资源：负责查找可用字体、按 ODTTF 规范混淆，并在没有可用字体时安全回退。
''' </summary>
''' <remarks>
''' 嵌入的是**完整**字体文件（不做子集化），因此使用中文字体时文档包体会达到数 MB；
''' 对体积敏感的场景可以关闭 <see cref="XpsDocument.EmbedFont"/>（回退输出的包只有几 KB，
''' 文本仍可提取），或通过 <see cref="XpsDocument.FontFile"/> 指定更小的字体。
''' </remarks>
Public Class XpsFontResource

    ''' <summary>包内字体部件所在目录。</summary>
    Public Const FontsFolder As String = "Resources/Fonts"

    ''' <summary>字体部件名（如 ``Resources/Fonts/8F5D2C1A-....odttf``）；回退模式下为空。</summary>
    Public Property PartName As String = ""

    ''' <summary>混淆后的字体字节；回退模式下为 Nothing。</summary>
    Public Property Data As Byte()

    ''' <summary>是否成功嵌入字体。</summary>
    Public Property IsEmbedded As Boolean = False

    ''' <summary>字体来源文件路径（回退模式下为 Nothing）。</summary>
    Public Property SourcePath As String

    ''' <summary>
    ''' Glyphs/@FontUri 使用值（以 ``/`` 开头的绝对部件名）；回退模式下为空字符串，
    ''' 调用方据此决定是否输出该属性。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property FontUri As String
        Get
            If String.IsNullOrEmpty(PartName) Then
                Return ""
            End If

            Return "/" & PartName
        End Get
    End Property

    ''' <summary>
    ''' 创建字体资源。
    ''' </summary>
    ''' <param name="fontFile">可选的字体文件路径（.ttf）；为空时自动查找系统字体。</param>
    ''' <param name="embed">是否嵌入字体；False 时直接返回回退资源。</param>
    ''' <returns>字体资源实例（永不返回 Nothing，回退时 <see cref="IsEmbedded"/> 为 False）。</returns>
    Public Shared Function Create(Optional fontFile As String = Nothing, Optional embed As Boolean = True) As XpsFontResource
        Dim resource As New XpsFontResource()

        If Not embed Then
            Return resource
        End If

        Dim path As String = If(String.IsNullOrEmpty(fontFile), FindSystemFont(), fontFile)

        If String.IsNullOrEmpty(path) OrElse Not File.Exists(path) Then
            Return resource
        End If

        ' .ttc 是字体集合，需要 FontFaceIndex 才能正确引用；不做猜测，直接回退
        If path.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase) Then
            Return resource
        End If

        Dim bytes As Byte()

        Try
            bytes = File.ReadAllBytes(path)
        Catch ex As Exception
            ' 字体读取失败只影响渲染外观，不影响文档生成
            Call Console.Error.WriteLine($"[警告] 字体读取失败，已回退为不嵌入字体: {path} ({ex.Message})")
            Return resource
        End Try

        If bytes Is Nothing OrElse bytes.Length < 32 Then
            Return resource
        End If

        Dim guid As Guid = Guid.NewGuid()

        resource.SourcePath = path
        resource.PartName = FontsFolder & "/" & guid.ToString("D") & ".odttf"
        resource.Data = Obfuscate(bytes, guid)
        resource.IsEmbedded = True

        Return resource
    End Function

    ''' <summary>
    ''' 按 ODTTF 规范混淆字体字节：以 GUID 的 16 字节（反序）为密钥，对前 32 字节做循环 XOR。
    ''' </summary>
    ''' <param name="font">原始字体字节。</param>
    ''' <param name="guid">字体部件名中使用的 GUID。</param>
    ''' <returns>混淆后的新字节数组（不修改入参）。</returns>
    Public Shared Function Obfuscate(font As Byte(), guid As Guid) As Byte()
        Dim key As Byte() = GuidKey(guid)
        Dim data As Byte() = CType(font.Clone(), Byte())
        Dim count As Integer = If(data.Length < 32, data.Length, 32)

        For i As Integer = 0 To count - 1
            data(i) = CType(data(i) Xor key(i Mod key.Length), Byte)
        Next

        Return data
    End Function

    ''' <summary>
    ''' 由 GUID 生成 ODTTF 混淆密钥：取 GUID 的 16 字节并反序。
    ''' </summary>
    ''' <param name="guid"></param>
    ''' <returns></returns>
    Public Shared Function GuidKey(guid As Guid) As Byte()
        Dim hex As String = guid.ToString("N")
        Dim key(15) As Byte

        For i As Integer = 0 To 15
            key(15 - i) = Convert.ToByte(hex.Substring(i * 2, 2), 16)
        Next

        Return key
    End Function

    ''' <summary>
    ''' 查找系统中可用的单一 TTF 字体（跳过 .ttc 集合）。
    ''' </summary>
    ''' <returns>字体文件路径；找不到时返回 Nothing。</returns>
    Public Shared Function FindSystemFont() As String
        Dim folders As New List(Of String)()
        Dim fontsFolder As String = ""

        Try
            fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts)
        Catch ex As Exception
            fontsFolder = ""
        End Try

        If Not String.IsNullOrEmpty(fontsFolder) Then
            Call folders.Add(fontsFolder)
        End If
        If Environment.OSVersion.Platform = PlatformID.Win32NT Then
            Call folders.Add("C:\Windows\Fonts")
        End If

        Call folders.Add("/usr/share/fonts/truetype/dejavu")
        Call folders.Add("/usr/share/fonts/truetype/liberation")
        Call folders.Add("/usr/share/fonts")
        Call folders.Add("/System/Library/Fonts")
        Call folders.Add("/System/Library/Fonts/Supplemental")
        Call folders.Add("/Library/Fonts")

        Dim names As String() = {
            "simhei.ttf",
            "simkai.ttf",
            "segoeui.ttf",
            "arial.ttf",
            "DejaVuSans.ttf",
            "LiberationSans-Regular.ttf",
            "NotoSans-Regular.ttf"
        }

        For Each folder As String In folders
            If String.IsNullOrEmpty(folder) OrElse Not Directory.Exists(folder) Then
                Continue For
            End If

            For Each name As String In names
                ' 变量不能命名为 path —— VB 大小写不敏感，会遮蔽 Path 类型
                Dim fontPath As String = Path.Combine(folder, name)

                If File.Exists(fontPath) Then
                    Return fontPath
                End If
            Next
        Next

        Return Nothing
    End Function

End Class
