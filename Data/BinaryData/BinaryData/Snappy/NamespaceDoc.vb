' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Snappy
'  Data\BinaryData\BinaryData\Snappy\NamespaceDoc.vb
' /********************************************************************************/

Namespace Snappy

    ''' <summary>
    ''' # Snappy 压缩算法
    ''' Snappy 是 Google 提出的快速压缩算法，属于 LZ77 家族：它用“距离 + 长度”的
    ''' 反向引用替换重复内容，不做熵编码，因此压缩率中等但解压速度极高。本命名空间
    ''' 提供两套互相关联的格式：
    ''' ---
    ''' ## 一、原始块格式（raw block）
    ''' 一段原始块以 varint（LEB128）记录未压缩长度，随后是一串指令。每条指令的
    ''' 首字节低 2 位决定类型：
    ''' ```
    ''' 00  literal       高 6 位 = 长度 - 1；若 >= 60，则高 6 位 = 59 + 后续字节数(1..4)，
    '''                   后续字节小端存放“长度 - 1”
    ''' 01  copy 1-byte   长度 4..11（3 位），偏移 11 位（高 3 位在 tag，低 8 位在后续字节）
    ''' 10  copy 2-byte   长度 1..64（6 位），偏移 16 位小端
    ''' 11  copy 4-byte   长度 1..64（6 位），偏移 32 位小端
    ''' ```
    ''' 复制指令引用的是“已经解压出来的输出”，因此偏移必须落在已输出区域内；
    ''' 当偏移小于复制长度时（如偏移 1、长度 15），源区域会被写入过程不断改写，
    ''' 必须逐字节复制——这也是 Snappy 表达行程编码的方式。
    ''' 原始块常用于 Parquet、Hadoop 等文件内部的独立压缩字段，对应 API：
    ''' `Snappy.Compress` / `Snappy.Uncompress` / `Snappy.UncompressedLength`。
    ''' ---
    ''' ## 二、分块流格式（framing format）
    ''' 当需要顺序读写大文件时，原始块之外还要包一层分块容器：
    ''' ```
    ''' FF 06 00 00 "sNaPpY"       stream identifier（必须是第一个分块）
    ''' 00 &lt;len:3&gt; &lt;crc:4&gt; &lt;block&gt; 压缩数据分块（block 为原始块）
    ''' 01 &lt;len:3&gt; &lt;crc:4&gt; &lt;data&gt;  未压缩数据分块
    ''' 80..FD &lt;len:3&gt; &lt;...&gt;       可跳过分块
    ''' FE &lt;len:3&gt; &lt;...&gt;           填充分块
    ''' ```
    ''' 分块头 = 1 字节类型 + 3 字节小端长度；数据分块的载荷前 4 字节是
    ''' **masked CRC32C**，校验对象是该分块解压后的数据。掩码规则：
    ''' ```
    ''' masked = ((crc &gt;&gt; 15) | (crc &lt;&lt; 17)) + 0xA282EAD8   (32 位回绕)
    ''' ```
    ''' 0x02..0x7F 是保留的“不可跳过”类型，遇到时应拒绝；0x80..0xFD 与 0xFE
    ''' 可以安全跳过。单个分块的未压缩数据不得超过 65536 字节。
    ''' 对应 API：`SnappyOutputStream`（写）/ `SnappyInputStream`（读）。
    ''' ---
    ''' ## 三、实现说明
    ''' - 压缩器按 64 KiB 分片处理，每片使用 4 字节哈希表查找匹配；窗口不超过
    '''   65535，单条复制指令不超过 64 字节，超长匹配会被拆成多条复制指令。
    ''' - 解压器线性扫描指令，字面量用 `Buffer.BlockCopy` 批量复制，重叠匹配置为
    '''   逐字节复制；所有长度、偏移与输入边界都有显式校验。
    ''' - CRC32C 使用 Castagnoli 反射多项式 0x82F63B78，与 BZip2 使用的 IEEE
    '''   CRC32 不同，不能混用。
    ''' </summary>
    Module NamespaceDoc
    End Module
End Namespace
