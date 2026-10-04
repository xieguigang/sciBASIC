Imports System.Buffers.Binary
Imports System.IO
Imports System.IO.Compression
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.Win32.SafeHandles

''' <summary>
''' 带符号相关系数矩阵的磁盘编码方式
''' </summary>
Public Enum CorrelationEncodings

    ''' <summary>
    ''' 单精度浮点（4 字节/值，精度约 1e-7）。全库体积约为 <see cref="QuantizedInt16"/> 的两倍。
    ''' </summary>
    Float32 = 0

    ''' <summary>
    ''' Int16 线性量化（2 字节/值）：值域 [-1, 1] 线性映射到 [-32767, 32767]，
    ''' 量化精度约 3e-5，对相关系数完全足够；NaN 使用 <see cref="Int16.MinValue"/> 哨兵值。
    ''' </summary>
    QuantizedInt16 = 1
End Enum

''' <summary>
''' 相关系数显著性 p 值（学生 t 检验）
''' </summary>
''' <remarks>
''' 与 CellPhenotype 流水线（<c>CorrelationSignificance.PValue</c>）使用同一公式：
''' <code>
''' t = r · sqrt( df / (1 - r²) ),  df = n - 2
''' p = I_{ df / (df + t²) }( df/2, 1/2 )
''' </code>
''' 其中 I 为不完全 beta 函数（等价于 t 分布的双尾概率）。
''' 因此 <see cref="CorrelationMatrixStore"/> 只需要持久化相关系数与样本数 n，
''' p 值在查询时按需重算，存储体积直接减半。
''' </remarks>
Public Module CorrelationPValues

    ''' <summary>
    ''' 相关系数的双尾显著性 p 值
    ''' </summary>
    ''' <param name="r">相关系数（-1 ~ 1）</param>
    ''' <param name="n">计算相关时使用的样本数</param>
    ''' <returns>p 值；样本数不足或计算失败时返回 1（最保守）</returns>
    Public Function PValue(r As Double, n As Integer) As Double
        If n <= 3 OrElse Double.IsNaN(r) Then
            Return 1.0
        End If

        Dim df As Double = n - 2
        Dim rr As Double = r * r

        If rr >= 1.0 Then
            rr = 1.0 - 0.000000000001
        End If
        If rr <= 0.0 Then
            Return 1.0
        End If

        Dim t2 As Double = rr * df / (1.0 - rr)
        Dim x As Double = df / (df + t2)

        If Double.IsNaN(x) OrElse x <= 0.0 OrElse x > 1.0 Then
            Return 1.0
        End If

        Try
            Dim p As Double = Beta.betai(0.5 * df, 0.5, x, throwMaxIterError:=False)

            If Double.IsNaN(p) OrElse Double.IsInfinity(p) Then
                Return 1.0
            End If

            Return Clamp01(p)
        Catch ex As Exception
            Return 1.0
        End Try
    End Function

    Private Function Clamp01(x As Double) As Double
        If Double.IsNaN(x) Then
            Return 0.0
        End If
        If x < 0.0 Then
            Return 0.0
        End If
        If x > 1.0 Then
            Return 1.0
        End If

        Return x
    End Function
End Module

''' <summary>
''' 带符号相关矩阵的持久化写入端（边算边写）
''' </summary>
''' <remarks>
''' 面向 N = 5 万级别的稠密相关矩阵：计算端逐基因产出一行相关系数（与 bicor/WGCNA
''' 的逐行计算方式吻合），本类把每一行编码 + Brotli 压缩后顺序追加到
''' <c>{path}</c> 数据文件，并在 <see cref="Complete"/> 时把「基因表 + 行目录」
''' 原子写入 <c>{path}.index</c>。
'''
''' 存储布局（<see cref="CorrelationMatrixStore"/> 的读端依赖此布局）：
''' <code>
''' 数据文件: [row0 compressed][row1 compressed]...        （无分隔前缀，位置/长度在索引中）
''' 索引文件: Brotli( "CRST" | version | N | sampleN | encoding |
'''                   N x [len(2B)][utf8 gene id] |
'''                   N x [offset(8B)][length(4B)] )
''' </code>
'''
''' 注意：每一行保存该基因对**全部 N 个基因**的相关系数（包括自身对角线，不使用上三角压缩），
''' 这样邻域查询（"给定基因 + 阈值"）只需要一次块读。
''' </remarks>
Public Class CorrelationMatrixWriter : Implements IDisposable

    ReadOnly _path As String
    ReadOnly _indexPath As String
    Dim _genes As String()
    Dim _geneIndex As Dictionary(Of String, Integer)
    Dim _offsets As Long()
    Dim _lengths As Integer()
    Dim _encoding As CorrelationEncodings
    Dim _n As Integer
    Dim _sampleN As Integer
    Dim _compression As CompressionLevel

    Dim _stream As Stream
    Dim _writer As BinaryWriter
    Dim _nextRow As Integer = 0
    Dim _completed As Boolean = False
    Dim _disposed As Boolean = False

    ' 流模式（外部自定义 Stream）专用状态：
    ' _indexStream 非空表示流模式（索引写到该流而不是 {path}.index 文件）；
    ' _position 手工跟踪数据流的写入位置（不依赖 BaseStream.Position，非可查找流也可写入）；
    ' _ownsStream 表示数据流由本对象创建（文件模式），Dispose 时负责释放。
    ReadOnly _indexStream As Stream
    ReadOnly _ownsStream As Boolean
    Dim _position As Long = 0

    ''' <summary>
    ''' 已经完成写入的行数
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property WrittenRows As Integer
        Get
            Return _nextRow
        End Get
    End Property

    ''' <summary>
    ''' 索引是否已经写出（完成之后当前对象不可再写入）
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsCompleted As Boolean
        Get
            Return _completed
        End Get
    End Property

    ''' <summary>
    ''' 创建写入端
    ''' </summary>
    ''' <param name="path">数据文件路径（建议扩展名 <c>.corstore</c>）；索引将写在 <c>{path}.index</c></param>
    ''' <param name="geneIds">全部基因 ID（决定行序与行数 N），不允许重复（忽略大小写比较）</param>
    ''' <param name="sampleN">计算相关系数时使用的样本数（p 值重算的依据）</param>
    ''' <param name="encoding">磁盘编码方式，默认 <see cref="CorrelationEncodings.Float32"/></param>
    ''' <param name="compression">Brotli 压缩级别，默认 <see cref="CompressionLevel.Optimal"/>；
    ''' 计算耗时的场景保持默认即可（写入与计算重叠），追求写入吞吐可降为 Fastest</param>
    Sub New(path As String,
            geneIds As IEnumerable(Of String),
            sampleN As Integer,
            Optional encoding As CorrelationEncodings = CorrelationEncodings.Float32,
            Optional compression As CompressionLevel = CompressionLevel.Optimal)

        If String.IsNullOrEmpty(path) Then
            Throw New ArgumentException("数据文件路径不能为空", NameOf(path))
        End If
        If geneIds Is Nothing Then
            Throw New ArgumentNullException(NameOf(geneIds), "基因 ID 列表不能为空")
        End If

        _path = IO.Path.GetFullPath(path)
        _indexPath = _path & ".index"
        _ownsStream = True

        Call Init(geneIds, sampleN, encoding, compression)

        Call IO.Path.GetDirectoryName(_path).MakeDir
        _stream = New FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.Read)
        _writer = New BinaryWriter(_stream)
    End Sub

    ''' <summary>
    ''' 创建写入端（外部 Stream 模式）：矩阵数据与索引分别写入调用方提供的两个流
    ''' </summary>
    ''' <param name="dataOutput">
    ''' 矩阵数据的输出流。只要求可写（<see cref="Stream.CanWrite"/>），
    ''' **不要求可查找**——行偏移由写入端自行跟踪，顺序追加即可。
    ''' 本对象不会关闭该流（生命周期归调用方所有）。
    ''' </param>
    ''' <param name="indexOutput">
    ''' 索引的输出流（<see cref="Complete"/> 时写入 Brotli 压缩的索引缓冲区）。
    ''' 本对象不会关闭该流。
    ''' </param>
    ''' <param name="geneIds">全部基因 ID（决定行序与行数 N），不允许重复（忽略大小写比较）</param>
    ''' <param name="sampleN">计算相关系数时使用的样本数（p 值重算的依据）</param>
    ''' <param name="encoding">磁盘编码方式</param>
    ''' <param name="compression">Brotli 压缩级别</param>
    ''' <remarks>
    ''' 典型用途：矩阵数据与索引存入外部自定义的存储流（对象存储、加密流、网络流等）。
    ''' 配套的读端为 <see cref="CorrelationMatrixStore.Open(Stream, Stream, Integer)"/>，
    ''' 其数据流要求可读且可查找（随机访问行块）。
    ''' </remarks>
    Sub New(dataOutput As Stream,
            indexOutput As Stream,
            geneIds As IEnumerable(Of String),
            sampleN As Integer,
            Optional encoding As CorrelationEncodings = CorrelationEncodings.Float32,
            Optional compression As CompressionLevel = CompressionLevel.Optimal)

        If dataOutput Is Nothing Then
            Throw New ArgumentNullException(NameOf(dataOutput), "矩阵数据输出流不能为空")
        End If
        If Not dataOutput.CanWrite Then
            Throw New ArgumentException("矩阵数据输出流必须可写", NameOf(dataOutput))
        End If
        If indexOutput Is Nothing Then
            Throw New ArgumentNullException(NameOf(indexOutput), "索引输出流不能为空")
        End If
        If Not indexOutput.CanWrite Then
            Throw New ArgumentException("索引输出流必须可写", NameOf(indexOutput))
        End If
        If geneIds Is Nothing Then
            Throw New ArgumentNullException(NameOf(geneIds), "基因 ID 列表不能为空")
        End If

        _indexStream = indexOutput

        Call Init(geneIds, sampleN, encoding, compression)

        _stream = dataOutput
        _writer = New BinaryWriter(_stream, System.Text.Encoding.UTF8, leaveOpen:=True)
    End Sub

    ''' <summary>两种构造方式的公共初始化：基因表校验 + 行目录数组</summary>
    Private Sub Init(geneIds As IEnumerable(Of String),
                     sampleN As Integer,
                     encoding As CorrelationEncodings,
                     compression As CompressionLevel)

        _encoding = encoding
        _sampleN = sampleN
        _compression = compression
        _genes = geneIds.ToArray()
        _n = _genes.Length

        If _n < 2 Then
            Throw New ArgumentException("基因数量至少为 2", NameOf(geneIds))
        End If

        _geneIndex = New Dictionary(Of String, Integer)(_n, StringComparer.OrdinalIgnoreCase)

        For i As Integer = 0 To _n - 1
            Dim gid As String = _genes(i)

            If String.IsNullOrEmpty(gid) Then
                Throw New ArgumentException($"第 {i + 1} 个基因 ID 为空", NameOf(geneIds))
            End If
            If _geneIndex.ContainsKey(gid) Then
                Throw New ArgumentException($"基因 ID '{gid}' 重复出现", NameOf(geneIds))
            End If

            _geneIndex(gid) = i
        Next

        _offsets = New Long(_n - 1) {}
        _lengths = New Integer(_n - 1) {}
    End Sub

    ''' <summary>
    ''' 写入一行相关系数（行序必须与构造时的 <c>geneIds</c> 顺序一致）
    ''' </summary>
    ''' <param name="geneId">当前行的基因 ID（必须等于声明顺序中的下一个）</param>
    ''' <param name="row">该基因对全部 N 个基因的相关系数；NaN 表示未定义</param>
    Public Sub WriteRow(geneId As String, row As Double())
        If row Is Nothing Then
            Throw New ArgumentNullException(NameOf(row))
        End If

        Dim values(_n - 1) As Single

        For i As Integer = 0 To _n - 1
            values(i) = CSng(row(i))
        Next

        Call WriteRow(geneId, values)
    End Sub

    ''' <summary>
    ''' 写入一行相关系数（行序必须与构造时的 <c>geneIds</c> 顺序一致）
    ''' </summary>
    ''' <param name="geneId">当前行的基因 ID（必须等于声明顺序中的下一个）</param>
    ''' <param name="row">该基因对全部 N 个基因的相关系数；NaN 表示未定义</param>
    Public Sub WriteRow(geneId As String, row As Single())
        If _disposed Then
            Throw New ObjectDisposedException(NameOf(CorrelationMatrixWriter))
        End If
        If _completed Then
            Throw New InvalidOperationException("索引已经写出（Complete 已调用），不能再写入数据行")
        End If
        If _nextRow >= _n Then
            Throw New InvalidOperationException($"全部 {_n} 行已经写入完毕")
        End If

        Dim expected As String = _genes(_nextRow)

        If Not String.Equals(geneId, expected, StringComparison.OrdinalIgnoreCase) Then
            Throw New ArgumentException(
                $"行顺序错误：期望写入 '{expected}'（第 {_nextRow + 1} 行），实际收到 '{geneId}'。" &
                "WriteRow 必须严格按照构造时 geneIds 的声明顺序调用", NameOf(geneId))
        End If
        If row Is Nothing Then
            Throw New ArgumentNullException(NameOf(row))
        End If
        If row.Length <> _n Then
            Throw New ArgumentException($"行长度必须等于基因数 {_n}，实际为 {row.Length}", NameOf(row))
        End If

        Dim raw As Byte() = EncodeRow(row)
        Dim compressed As Byte() = Brotli(raw)
        ' 手工跟踪写入位置：不依赖 BaseStream.Position，非可查找的外部流也可以顺序追加
        Dim offset As Long = _position

        _writer.Write(compressed)
        _position += compressed.Length

        _offsets(_nextRow) = offset
        _lengths(_nextRow) = compressed.Length
        _nextRow += 1
    End Sub

    ''' <summary>
    ''' 完成写入：刷盘并原子写出索引文件。全部 N 行写入完毕后必须调用；
    ''' <see cref="Dispose"/> 在所有行都已写入时会自动调用本方法。
    ''' </summary>
    Public Sub Complete()
        If _completed Then
            Return
        End If
        If _nextRow < _n Then
            Throw New InvalidOperationException(
                $"只写入了 {_nextRow}/{_n} 行，索引不完整。请补齐全部数据行后再调用 Complete")
        End If

        Call _writer.Flush()

        ' 索引输出：流模式写到外部索引流；文件模式原子写 {path}.index
        If _indexStream IsNot Nothing Then
            Dim indexBytes As Byte() = BuildIndexCompressed()

            Call _indexStream.Write(indexBytes, 0, indexBytes.Length)
            Call _indexStream.Flush()

            Call _stream.Flush()
        Else
            Call _stream.Dispose()
            _writer = Nothing
            _stream = Nothing

            Dim tmp As String = _indexPath & ".tmp"

            Using raw As New MemoryStream
                Call WriteIndexBuffer(raw)
                raw.Seek(0, SeekOrigin.Begin)

                Using file As New FileStream(tmp, FileMode.Create, FileAccess.Write)
                    Using brotli As New BrotliStream(file, CompressionLevel.Optimal)
                        Call raw.CopyTo(brotli)
                    End Using
                End Using
            End Using

            ' 原子替换：防止写入中断留下损坏的索引
            If IO.File.Exists(_indexPath) Then
                Call IO.File.Replace(tmp, _indexPath, Nothing)
            Else
                Call IO.File.Move(tmp, _indexPath)
            End If
        End If

        _completed = True
    End Sub

    ''' <summary>生成 Brotli 压缩的索引缓冲区</summary>
    Private Function BuildIndexCompressed() As Byte()
        Using raw As New MemoryStream
            Call WriteIndexBuffer(raw)
            raw.Seek(0, SeekOrigin.Begin)

            Using ms As New MemoryStream
                Using bs As New BrotliStream(ms, CompressionLevel.Optimal)
                    Call raw.CopyTo(bs)
                End Using

                Return ms.ToArray()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 序列化索引缓冲区：
    ''' "CRST" | version | N | sampleN | encoding | N x [len(2B)][utf8] | N x [offset(8B)][length(4B)]
    ''' </summary>
    Private Sub WriteIndexBuffer(buf As MemoryStream)
        Using w As New BinaryWriter(buf, System.Text.Encoding.UTF8, leaveOpen:=True)
            Call w.Write({"C"c, "R"c, "S"c, "T"c})
            Call w.Write(CInt(1))
            Call w.Write(_n)
            Call w.Write(_sampleN)
            Call w.Write(CInt(_encoding))

            For i As Integer = 0 To _n - 1
                Dim gid As Byte() = System.Text.Encoding.UTF8.GetBytes(_genes(i))

                If gid.Length > Int16.MaxValue Then
                    Throw New InvalidOperationException($"基因 ID '{_genes(i)}' 过长（>{Int16.MaxValue} 字节）")
                End If

                Call w.Write(CShort(gid.Length))
                Call w.Write(gid)
            Next

            For i As Integer = 0 To _n - 1
                Call w.Write(_offsets(i))
                Call w.Write(_lengths(i))
            Next

            Call w.Flush()
        End Using
    End Sub

    Private Function EncodeRow(row As Single()) As Byte()
        If _encoding = CorrelationEncodings.Float32 Then
            Dim buf(_n * 4 - 1) As Byte

            For i As Integer = 0 To _n - 1
                Call BinaryPrimitives.WriteSingleLittleEndian(buf.AsSpan(i * 4), row(i))
            Next

            Return buf
        Else
            Dim buf(_n * 2 - 1) As Byte

            For i As Integer = 0 To _n - 1
                Call BinaryPrimitives.WriteInt16LittleEndian(buf.AsSpan(i * 2), Quantize(row(i)))
            Next

            Return buf
        End If
    End Function

    Private Shared Function Quantize(r As Single) As Short
        If Single.IsNaN(r) Then
            Return Short.MinValue
        End If

        Dim scaled As Double = CDbl(r) * 32767.0

        If scaled > 32767.0 Then
            Return 32767S
        End If
        If scaled < -32767.0 Then
            Return -32767S
        End If

        Return CShort(System.Math.Round(scaled))
    End Function

    Friend Shared Function Dequantize(v As Short) As Single
        If v = Short.MinValue Then
            Return Single.NaN
        End If

        Return CSng(v / 32767.0)
    End Function

    Friend Shared Function DecodeFloat32(buf As Byte(), n As Integer) As Single()
        Dim row(n - 1) As Single

        For i As Integer = 0 To n - 1
            row(i) = BinaryPrimitives.ReadSingleLittleEndian(buf.AsSpan(i * 4))
        Next

        Return row
    End Function

    Friend Shared Function DecodeInt16(buf As Byte(), n As Integer) As Single()
        Dim row(n - 1) As Single

        For i As Integer = 0 To n - 1
            row(i) = Dequantize(BinaryPrimitives.ReadInt16LittleEndian(buf.AsSpan(i * 2)))
        Next

        Return row
    End Function

    Private Function Brotli(raw As Byte()) As Byte()
        Using ms As New MemoryStream
            Using bs As New BrotliStream(ms, _compression)
                Call bs.Write(raw, 0, raw.Length)
            End Using

            Return ms.ToArray()
        End Using
    End Function

    ''' <summary>
    ''' 关闭写入端。若全部行已写入但尚未调用 <see cref="Complete"/>，会自动补写索引；
    ''' 若行不完整：文件模式保留数据文件供断点续写（缺少索引文件，无法被
    ''' <see cref="CorrelationMatrixStore.Open(String, Integer)"/> 打开）；
    ''' 流模式刷盘后保留外部流中的数据（同样缺少索引，不可读）。
    ''' 外部 Stream（流模式）不会被本对象关闭。
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then
            Return
        End If

        _disposed = True

        If Not _completed Then
            If _nextRow = _n Then
                Call Complete()
            Else
                ' 不完整：关闭句柄，保留已写数据（缺少索引文件，读端无法打开）
                If _indexStream IsNot Nothing Then
                    Call _stream.Flush()
                End If

                Call ($"CorrelationMatrixWriter: disposed with {_nextRow}/{_n} rows written " &
                      $"(incomplete store, index not written).").warning
            End If
        End If

        If _writer IsNot Nothing Then
            Call _writer.Dispose()
            _writer = Nothing
        End If
        If _stream IsNot Nothing AndAlso _ownsStream Then
            Call _stream.Dispose()
            _stream = Nothing
        End If
    End Sub
End Class

''' <summary>
''' 带符号相关矩阵的磁盘持久化存储（读端）
''' </summary>
''' <remarks>
''' 由 <see cref="CorrelationMatrixWriter"/> 写入、本类负责读取。针对 N = 5 万级别的
''' 稠密相关矩阵（10 GB 级原始数据）设计，支持三类查询，全部不需要把矩阵载入内存：
'''
''' <list type="bullet">
''' <item>点查 <see cref="GetCorrelation"/>：(gene1, gene2) → (cor, pvalue)，一次块读，毫秒级；</item>
''' <item>邻域查询 <see cref="Neighbors"/>：基因 + 阈值 → |cor| ≥ 阈值的邻居列表，一次块读 + 行内线性扫描；</item>
''' <item>全库流式筛选 <see cref="StreamEdges"/>：按阈值遍历全部行块，Iterator 流式产出边表，
''' 供阈值过滤实验直接生成网络，不物化中间结果。</item>
''' </list>
'''
''' p 值不持久化，由相关系数与样本数 n 按需重算（<see cref="CorrelationPValues.PValue"/>）。
''' 热行缓存为小容量 LRU（默认 64 行），多线程并发读通过 <c>RandomAccess.Read</c>
''' 显式偏移实现，无需加锁（缓存链表本身有锁保护）。
'''
''' 支持两种打开方式：
''' <list type="bullet">
''' <item><see cref="Open(String, Integer)"/>：文件路径模式（数据文件 + <c>{path}.index</c> 索引文件）；</item>
''' <item><see cref="Open(Stream, Stream, Integer)"/>：外部 Stream 模式——矩阵数据与索引分别从
''' 调用方提供的两个自定义流读取（与 <see cref="CorrelationMatrixWriter"/> 的流模式构造配套），
''' 数据流要求可读且可查找；若实际是 <see cref="FileStream"/> 会自动走无锁句柄读路径，
''' 其他流走 Seek + Read + 锁。</item>
''' </list>
''' </remarks>
Public Class CorrelationMatrixStore : Implements IDisposable

    ReadOnly _path As String
    ReadOnly _genes As String()
    ReadOnly _geneIndex As Dictionary(Of String, Integer)
    ReadOnly _offsets As Long()
    ReadOnly _lengths As Integer()
    ReadOnly _n As Integer
    ReadOnly _sampleN As Integer
    ReadOnly _encoding As CorrelationEncodings
    ReadOnly _cache As RowLruCache
    Dim _handle As SafeFileHandle
    Dim _disposed As Boolean = False

    ' 流模式（外部自定义 Stream）专用状态：
    ' _dataStream 非空表示流模式（行块通过 Seek + Read 读取，且有锁保护）；
    ' _ownsIO 表示底层句柄/流由本对象创建（文件模式），Dispose 时负责释放。
    Dim _dataStream As Stream
    ReadOnly _ioLock As New Object
    ReadOnly _ownsIO As Boolean

    ''' <summary>
    ''' 基因 ID 表（行序与写入时的声明顺序一致）
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property genes As String()
        Get
            Return _genes
        End Get
    End Property

    ''' <summary>
    ''' 矩阵规模 N（基因数量）
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property size As Integer
        Get
            Return _n
        End Get
    End Property

    ''' <summary>
    ''' 计算相关系数时使用的样本数（p 值重算依据）
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property SampleN As Integer
        Get
            Return _sampleN
        End Get
    End Property

    Private Sub New(path As String, handle As SafeFileHandle, dataStream As Stream, ownsIO As Boolean,
                    genes As String(), geneIndex As Dictionary(Of String, Integer),
                    offsets As Long(), lengths As Integer(),
                    n As Integer, sampleN As Integer, encoding As CorrelationEncodings,
                    cacheRows As Integer)

        _path = path
        _handle = handle
        _dataStream = dataStream
        _ownsIO = ownsIO
        _genes = genes
        _geneIndex = geneIndex
        _offsets = offsets
        _lengths = lengths
        _n = n
        _sampleN = sampleN
        _encoding = encoding
        _cache = New RowLruCache(cacheRows)
    End Sub

    ''' <summary>
    ''' 打开一个已完成的存储
    ''' </summary>
    ''' <param name="path">数据文件路径（索引文件为 <c>{path}.index</c>）</param>
    ''' <param name="cacheRows">热行 LRU 缓存容量（行数），默认 64 行</param>
    ''' <returns></returns>
    Public Shared Function Open(path As String, Optional cacheRows As Integer = 64) As CorrelationMatrixStore
        If String.IsNullOrEmpty(path) Then
            Throw New ArgumentException("数据文件路径不能为空", NameOf(path))
        End If

        Dim fullPath As String = IO.Path.GetFullPath(path)
        Dim indexPath As String = fullPath & ".index"

        If Not IO.File.Exists(fullPath) Then
            Throw New FileNotFoundException("相关矩阵数据文件不存在", fullPath)
        End If
        If Not IO.File.Exists(indexPath) Then
            Throw New FileNotFoundException(
                $"索引文件不存在（'{indexPath}'）。缺少索引说明写入端未正常 Complete，存储不完整", indexPath)
        End If

        Dim genes As String() = Nothing
        Dim offsets As Long() = Nothing
        Dim lengths As Integer() = Nothing
        Dim n As Integer, sampleN As Integer, encoding As Integer

        Call LoadIndex(indexPath, genes, offsets, lengths, n, sampleN, encoding)

        Dim geneIndex As New Dictionary(Of String, Integer)(n, StringComparer.OrdinalIgnoreCase)

        For i As Integer = 0 To n - 1
            geneIndex(genes(i)) = i
        Next

        Dim handle As SafeFileHandle = IO.File.OpenHandle(
            fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.RandomAccess)

        Return New CorrelationMatrixStore(fullPath, handle, Nothing, True, genes, geneIndex,
                                          offsets, lengths, n, sampleN,
                                          CType(encoding, CorrelationEncodings), cacheRows)
    End Function

    ''' <summary>
    ''' 打开一个已完成的存储（外部 Stream 模式）：矩阵数据与索引分别从调用方提供的两个流读取
    ''' </summary>
    ''' <param name="data">
    ''' 矩阵数据的输入流。**必须可读且可查找**（<see cref="Stream.CanSeek"/>）——
    ''' 行块按索引中的偏移量随机访问。本对象不会关闭该流（生命周期归调用方所有）。
    ''' 若该流实际是 <see cref="FileStream"/>，会自动使用其句柄走无锁并发读路径。
    ''' </param>
    ''' <param name="index">索引的输入流（<see cref="Complete"/> 写出的 Brotli 压缩索引），必须可读</param>
    ''' <param name="cacheRows">热行 LRU 缓存容量（行数），默认 64 行</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 与 <see cref="CorrelationMatrixWriter"/> 的
    ''' <c>Sub New(dataOutput, indexOutput, ...)</c> 构造函数配套使用：
    ''' 写入时提供的两个流，在 Complete 之后可以直接交给本方法重新打开读取。
    ''' </remarks>
    Public Shared Function Open(data As Stream, index As Stream, Optional cacheRows As Integer = 64) As CorrelationMatrixStore
        If data Is Nothing Then
            Throw New ArgumentNullException(NameOf(data), "矩阵数据输入流不能为空")
        End If
        If Not data.CanRead Then
            Throw New ArgumentException("矩阵数据输入流必须可读", NameOf(data))
        End If
        If Not data.CanSeek Then
            Throw New ArgumentException("矩阵数据输入流必须可查找（随机访问行块）", NameOf(data))
        End If
        If index Is Nothing Then
            Throw New ArgumentNullException(NameOf(index), "索引输入流不能为空")
        End If
        If Not index.CanRead Then
            Throw New ArgumentException("索引输入流必须可读", NameOf(index))
        End If

        Dim genes As String() = Nothing
        Dim offsets As Long() = Nothing
        Dim lengths As Integer() = Nothing
        Dim n As Integer, sampleN As Integer, encoding As Integer

        Call LoadIndex(index, genes, offsets, lengths, n, sampleN, encoding)

        Dim geneIndex As New Dictionary(Of String, Integer)(n, StringComparer.OrdinalIgnoreCase)

        For i As Integer = 0 To n - 1
            geneIndex(genes(i)) = i
        Next

        ' FileStream 特例：直接用句柄走 RandomAccess 无锁并发读；其他流走 Seek+Read + 锁
        Dim handle As SafeFileHandle = Nothing
        Dim dataStream As Stream = data

        If TypeOf data Is FileStream Then
            handle = DirectCast(data, FileStream).SafeFileHandle
            dataStream = Nothing
        End If

        Return New CorrelationMatrixStore(Nothing, handle, dataStream, False, genes, geneIndex,
                                          offsets, lengths, n, sampleN,
                                          CType(encoding, CorrelationEncodings), cacheRows)
    End Function

    ''' <summary>从索引文件路径加载（内部转调 Stream 版本）</summary>
    Private Shared Sub LoadIndex(indexPath As String,
                                 ByRef genes As String(),
                                 ByRef offsets As Long(),
                                 ByRef lengths As Integer(),
                                 ByRef n As Integer,
                                 ByRef sampleN As Integer,
                                 ByRef encoding As Integer)

        Using file As New FileStream(indexPath, FileMode.Open, FileAccess.Read)
            Call LoadIndex(file, genes, offsets, lengths, n, sampleN, encoding)
        End Using
    End Sub

    ''' <summary>从索引输入流加载（Brotli 压缩缓冲区，读完后流的当前位置即索引末尾）</summary>
    Private Shared Sub LoadIndex(indexStream As Stream,
                                 ByRef genes As String(),
                                 ByRef offsets As Long(),
                                 ByRef lengths As Integer(),
                                 ByRef n As Integer,
                                 ByRef sampleN As Integer,
                                 ByRef encoding As Integer)

        Using brotli As New BrotliStream(indexStream, CompressionMode.Decompress)
            Using buf As New MemoryStream
                Call brotli.CopyTo(buf)
                Call buf.Seek(0, SeekOrigin.Begin)

                Using r As New BinaryReader(buf)
                    Dim magic As Char() = r.ReadChars(4)

                    If magic(0) <> "C"c OrElse magic(1) <> "R"c OrElse magic(2) <> "S"c OrElse magic(3) <> "T"c Then
                        Throw New InvalidDataException("不是有效的相关矩阵索引文件（magic 不匹配）")
                    End If

                    Dim version As Integer = r.ReadInt32()

                    If version <> 1 Then
                        Throw New InvalidDataException($"不支持的索引版本: {version}")
                    End If

                    n = r.ReadInt32()
                    sampleN = r.ReadInt32()
                    encoding = r.ReadInt32()

                    If n < 2 Then
                        Throw New InvalidDataException($"索引中的基因数量无效: {n}")
                    End If

                    genes = New String(n - 1) {}

                    For i As Integer = 0 To n - 1
                        Dim len As Integer = r.ReadInt16()
                        genes(i) = System.Text.Encoding.UTF8.GetString(r.ReadBytes(len))
                    Next

                    offsets = New Long(n - 1) {}
                    lengths = New Integer(n - 1) {}

                    For i As Integer = 0 To n - 1
                        offsets(i) = r.ReadInt64()
                        lengths(i) = r.ReadInt32()
                    Next
                End Using
            End Using
        End Using
    End Sub

    Private Function RowOf(geneId As String) As Integer
        Dim row As Integer

        If Not _geneIndex.TryGetValue(geneId, row) Then
            Throw New KeyNotFoundException($"基因 '{geneId}' 不在相关矩阵中（共 {_n} 个基因）")
        End If

        Return row
    End Function

    ''' <summary>
    ''' 点查：(gene_id1, gene_id2) → (相关系数, p 值)
    ''' </summary>
    ''' <param name="id1">基因 ID</param>
    ''' <param name="id2">基因 ID</param>
    ''' <returns>相关系数与双尾 p 值；对角线（同一基因）返回 (1, 0)</returns>
    Public Function GetCorrelation(id1 As String, id2 As String) As (cor As Double, pvalue As Double)
        If _disposed Then
            Throw New ObjectDisposedException(NameOf(CorrelationMatrixStore))
        End If

        Dim i As Integer = RowOf(id1)
        Dim j As Integer = RowOf(id2)

        If i = j Then
            Return (1.0, 0.0)
        End If

        Dim cor As Double = ReadRow(i)(j)

        Return (cor, CorrelationPValues.PValue(cor, _sampleN))
    End Function

    ''' <summary>
    ''' 邻域查询：返回与 <paramref name="geneId"/> 相关系数绝对值超过阈值的所有基因
    ''' </summary>
    ''' <param name="geneId">查询基因</param>
    ''' <param name="minAbsCor">相关系数绝对值阈值（0 ~ 1）</param>
    ''' <returns>按 |cor| 降序排列的邻居列表（基因 ID、相关系数、双尾 p 值）</returns>
    Public Function Neighbors(geneId As String, minAbsCor As Double) As (gene As String, cor As Double, pvalue As Double)()
        If _disposed Then
            Throw New ObjectDisposedException(NameOf(CorrelationMatrixStore))
        End If

        Dim i As Integer = RowOf(geneId)
        Dim row As Single() = ReadRow(i)
        Dim hits As New List(Of (gene As String, cor As Double, pvalue As Double))

        For j As Integer = 0 To _n - 1
            If j = i Then
                Continue For
            End If

            Dim cor As Double = row(j)

            If Single.IsNaN(cor) OrElse System.Math.Abs(cor) < minAbsCor Then
                Continue For
            End If

            hits.Add((_genes(j), cor, CorrelationPValues.PValue(cor, _sampleN)))
        Next

        Return hits _
            .OrderByDescending(Function(h) System.Math.Abs(h.cor)) _
            .ToArray()
    End Function

    ''' <summary>
    ''' 全库流式筛选：遍历全部基因对，产出 |cor| ≥ 阈值（可选再过滤 p 值）的边表
    ''' </summary>
    ''' <param name="minAbsCor">相关系数绝对值阈值</param>
    ''' <param name="genes">
    ''' 可选的基因子集：只产出两端都落在子集内的边（例如某个 WGCNA 模块）。
    ''' 为 Nothing 或空时扫描全库。
    ''' </param>
    ''' <param name="pvalueCutoff">
    ''' 可选的 p 值上限；NaN（默认）表示不按 p 值过滤。
    ''' 注意按 p 值过滤会为每条候选边额外计算一次不完全 beta 函数。
    ''' </param>
    ''' <returns>流式边表 (基因 a, 基因 b, 相关系数, p 值)；每条无序基因对只产出一次</returns>
    ''' <remarks>
    ''' 每个基因对只在一行中判定（<c>j &gt; i</c>），因此即使矩阵对称存储也不会重复产出。
    ''' 枚举期间按行流式解码，内存占用 O(一行)，可直接供后续 FDR/DPI/网络生成管线消费。
    ''' </remarks>
    Public Iterator Function StreamEdges(minAbsCor As Double,
                                         Optional genes As IEnumerable(Of String) = Nothing,
                                         Optional pvalueCutoff As Double = Double.NaN
    ) As IEnumerable(Of (a As String, b As String, cor As Double, pvalue As Double))
        If _disposed Then
            Throw New ObjectDisposedException(NameOf(CorrelationMatrixStore))
        End If

        Dim subset As HashSet(Of Integer) = Nothing

        If genes IsNot Nothing Then
            subset = New HashSet(Of Integer)

            For Each gid As String In genes
                Dim row As Integer

                If _geneIndex.TryGetValue(gid, row) Then
                    subset.Add(row)
                End If
            Next

            If subset.Count = 0 Then
                Return
            End If
        End If

        Dim checkP As Boolean = Not Double.IsNaN(pvalueCutoff)

        For i As Integer = 0 To _n - 1
            If subset IsNot Nothing AndAlso Not subset.Contains(i) Then
                Continue For
            End If

            ' 流式扫描不占用 LRU 缓存，避免冲掉点查的热行
            Dim row As Single() = ReadRow(i, useCache:=False)

            For j As Integer = i + 1 To _n - 1
                If subset IsNot Nothing AndAlso Not subset.Contains(j) Then
                    Continue For
                End If

                Dim cor As Double = row(j)

                If Single.IsNaN(cor) OrElse System.Math.Abs(cor) < minAbsCor Then
                    Continue For
                End If

                Dim p As Double = CorrelationPValues.PValue(cor, _sampleN)

                If checkP AndAlso p > pvalueCutoff Then
                    Continue For
                End If

                Yield (_genes(i), _genes(j), cor, p)
            Next
        Next
    End Function

    ''' <summary>
    ''' 读取一行（全部 N 个基因的相关系数）
    ''' </summary>
    ''' <param name="row">行号</param>
    ''' <param name="useCache">是否使用/填充 LRU 缓存；流式扫描时传 False</param>
    ''' <returns>长度为 N 的相关系数数组（可直接下标访问）</returns>
    Public Function ReadRow(row As Integer, Optional useCache As Boolean = True) As Single()
        If _disposed Then
            Throw New ObjectDisposedException(NameOf(CorrelationMatrixStore))
        End If
        If row < 0 OrElse row >= _n Then
            Throw New ArgumentOutOfRangeException(NameOf(row), $"行号 {row} 超出范围 [0, {_n})")
        End If

        If useCache Then
            Dim cached As Single() = Nothing

            If _cache.TryGet(row, cached) Then
                Return cached
            End If
        End If

        Dim data As Single() = DecodeBlock(row)

        If useCache Then
            Call _cache.Add(row, data)
        End If

        Return data
    End Function

    ''' <summary>
    ''' 从底层存储读取指定长度的字节块：
    ''' 文件模式（含外部 FileStream 特例）走 <see cref="RandomAccess.Read"/> 显式偏移，
    ''' 多线程并发读无需加锁；其他外部流走 Seek + Read，用锁串行化（流不可并发寻位）。
    ''' </summary>
    Private Sub ReadRaw(offset As Long, length As Integer, buffer As Byte(), row As Integer)
        If _handle IsNot Nothing Then
            Dim total As Integer = 0

            While total < length
                Dim n As Integer = RandomAccess.Read(_handle, buffer.AsSpan(total, length - total), offset + total)

                If n <= 0 Then
                    Throw New IOException($"unexpected end of data file at offset {offset + total} (row {row})")
                End If

                total += n
            End While
        Else
            SyncLock _ioLock
                Call _dataStream.Seek(offset, SeekOrigin.Begin)

                Dim total As Integer = 0

                While total < length
                    Dim n As Integer = _dataStream.Read(buffer, total, length - total)

                    If n <= 0 Then
                        Throw New IOException($"unexpected end of data stream at offset {offset + total} (row {row})")
                    End If

                    total += n
                End While
            End SyncLock
        End If
    End Sub

    Private Function DecodeBlock(row As Integer) As Single()
        Dim length As Integer = _lengths(row)
        Dim offset As Long = _offsets(row)
        Dim compressed(length - 1) As Byte

        Call ReadRaw(offset, length, compressed, row)

        Using src As New MemoryStream(compressed)
            Using brotli As New BrotliStream(src, CompressionMode.Decompress)
                Using dst As New MemoryStream(_n * If(_encoding = CorrelationEncodings.Float32, 4, 2))
                    Call brotli.CopyTo(dst)

                    Dim raw As Byte() = dst.GetBuffer()

                    If dst.Length <> _n * If(_encoding = CorrelationEncodings.Float32, 4, 2) Then
                        Throw New InvalidDataException(
                            $"行 {row} 解压后长度 {dst.Length} 与预期 {_n * If(_encoding = CorrelationEncodings.Float32, 4, 2)} 不符（数据文件可能已损坏）")
                    End If

                    If _encoding = CorrelationEncodings.Float32 Then
                        Return CorrelationMatrixWriter.DecodeFloat32(raw, _n)
                    Else
                        Return CorrelationMatrixWriter.DecodeInt16(raw, _n)
                    End If
                End Using
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 基因是否存在于矩阵中
    ''' </summary>
    ''' <param name="geneId"></param>
    ''' <returns></returns>
    Public Function Contains(geneId As String) As Boolean
        Return _geneIndex.ContainsKey(geneId)
    End Function

    ''' <summary>
    ''' 查询基因的行号（配合 <see cref="ReadRow"/> 批量取行，例如组装模块内子矩阵）
    ''' </summary>
    ''' <param name="geneId"></param>
    ''' <returns>行号；基因不存在时返回 -1</returns>
    Public Function IndexOf(geneId As String) As Integer
        Dim row As Integer
        Return If(_geneIndex.TryGetValue(geneId, row), row, -1)
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then
            Return
        End If

        _disposed = True

        If _handle IsNot Nothing AndAlso _ownsIO Then
            Call _handle.Dispose()
        End If

        _handle = Nothing

        If _dataStream IsNot Nothing AndAlso _ownsIO Then
            Call _dataStream.Dispose()
        End If

        _dataStream = Nothing

        Call _cache.Clear()
    End Sub

    ''' <summary>小容量 LRU 热行缓存（线程安全）</summary>
    Private Class RowLruCache

        ReadOnly cap As Integer
        ReadOnly order As New LinkedList(Of KeyValuePair(Of Integer, Single()))
        ReadOnly map As New Dictionary(Of Integer, LinkedListNode(Of KeyValuePair(Of Integer, Single())))
        ReadOnly locker As New Object

        Sub New(capacity As Integer)
            cap = If(capacity < 1, 1, capacity)
        End Sub

        Function TryGet(row As Integer, ByRef data As Single()) As Boolean
            SyncLock locker
                Dim node As LinkedListNode(Of KeyValuePair(Of Integer, Single())) = Nothing

                If map.TryGetValue(row, node) Then
                    data = node.Value.Value
                    Call order.Remove(node)
                    Call order.AddFirst(node)
                    Return True
                End If

                Return False
            End SyncLock
        End Function

        Sub Add(row As Integer, data As Single())
            SyncLock locker
                Dim node As LinkedListNode(Of KeyValuePair(Of Integer, Single())) = Nothing

                If map.TryGetValue(row, node) Then
                    node.Value = New KeyValuePair(Of Integer, Single())(row, data)
                    Call order.Remove(node)
                    Call order.AddFirst(node)
                    Return
                End If

                If map.Count >= cap Then
                    Dim last As LinkedListNode(Of KeyValuePair(Of Integer, Single())) = order.Last

                    Call order.RemoveLast()
                    Call map.Remove(last.Value.Key)
                End If

                Dim fresh As New LinkedListNode(Of KeyValuePair(Of Integer, Single()))(
                    New KeyValuePair(Of Integer, Single())(row, data))

                Call order.AddFirst(fresh)
                map(row) = fresh
            End SyncLock
        End Sub

        Sub Clear()
            SyncLock locker
                Call order.Clear()
                Call map.Clear()
            End SyncLock
        End Sub
    End Class
End Class
