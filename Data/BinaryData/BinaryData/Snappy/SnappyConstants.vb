Option Strict On
Option Explicit On

Namespace Snappy

    ''' <summary>
    ''' Snappy 原始块格式与分块流格式（framing format）共用的常量。
    ''' </summary>
    Friend Module SnappyConstants

        ''' <summary>单个压缩分块的最大未压缩长度（64 KiB）。</summary>
        Friend Const BlockSize As Integer = 65536

        ''' <summary>压缩器哈希表的位数（表大小 = 1 &lt;&lt; HashTableBits）。</summary>
        Friend Const HashTableBits As Integer = 14

        ''' <summary>压缩器哈希表的大小。</summary>
        Friend Const HashTableSize As Integer = 1 << HashTableBits

        ''' <summary>两字节回退偏移能够表示的最大距离。</summary>
        Friend Const MaxTwoByteOffset As Integer = &HFFFF

        ''' <summary>单个复制指令能够表示的最大长度。</summary>
        Friend Const MaxCopyLength As Integer = 64

        ''' <summary>分块流的分块长度字段最多 3 字节。</summary>
        Friend Const MaxChunkLength As Integer = &HFFFFFF

        ''' <summary>分块流中单个分块允许的最大未压缩数据长度。</summary>
        Friend Const MaxChunkUncompressedData As Integer = 65536

        ''' <summary>压缩数据分块。</summary>
        Friend Const ChunkTypeCompressed As Byte = &H0

        ''' <summary>未压缩数据分块。</summary>
        Friend Const ChunkTypeUncompressed As Byte = &H1

        ''' <summary>填充分块，内容应被忽略。</summary>
        Friend Const ChunkTypePadding As Byte = &HFE

        ''' <summary>流标识分块。</summary>
        Friend Const ChunkTypeStreamIdentifier As Byte = &HFF

        ''' <summary>可跳过分块类型范围的下界。</summary>
        Friend Const SkippableChunkMin As Byte = &H80

        ''' <summary>可跳过分块类型范围的上界。</summary>
        Friend Const SkippableChunkMax As Byte = &HFD

        ''' <summary>分块流的流标识载荷，ASCII 文本 "sNaPpY"。</summary>
        Friend ReadOnly StreamIdentifier As Byte() = {&H73, &H4E, &H61, &H50, &H70, &H59}

    End Module
End Namespace
