#Region "Microsoft.VisualBasic::NumericTableBinary, Microsoft.VisualBasic.Core\src\Data\NumericTableBinary.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace Data

    ''' <summary>
    ''' <see cref="NumericTable"/> 对象的二进制文件读写。
    ''' 
    ''' 文件布局（所有的多字节数值均通过 <see cref="NetworkByteOrderBuffer"/> 编码为**大端/网络字节序**）：
    ''' 
    ''' ```
    ''' [magic]        固定的ASCII魔数 "NVTABLE"
    ''' [version]      Int32，当前的格式版本号 <see cref="FORMAT_VERSION"/>
    ''' [flags]        Byte，bit0 = 载荷数据是否采用 gzip 压缩
    ''' [nsamples]     Int32，样本数量
    ''' [nfeatures]    Int32，特征列数量
    ''' [nlabels]      Int32，标签列数量
    ''' [name]         长度前缀的UTF8字符串，表名（可以为空）
    ''' [description]  长度前缀的UTF8字符串，表的描述文本（可以为空）
    ''' [rowNames]     行名数组（数量 + 逐个长度前缀的UTF8字符串）
    ''' [featureNames] 特征列名数组
    ''' [labelNames]   标签列名数组
    ''' [features]     长度前缀的字节块，特征矩阵按行主序摊平之后的 Double 大端数据
    ''' [labels]       长度前缀的字节块，标签矩阵按行主序摊平之后的 Double 大端数据
    ''' ```
    ''' 
    ''' 其中 gzip 压缩只作用于 ``features`` 与 ``labels`` 这两个载荷块，文件头与元数据
    ''' 始终保持明文，因此可以在不完整解析文件的情况下快速识别格式与压缩状态。
    ''' </summary>
    Public Module NumericTableBinary

        ''' <summary>
        ''' 二进制文件格式的魔数文本
        ''' </summary>
        Private Const MAGIC_TEXT As String = "NVTABLE"

        ''' <summary>
        ''' 获取二进制文件格式的魔数字节
        ''' </summary>
        Private Function MagicBytes() As Byte()
            Return Encoding.ASCII.GetBytes(MAGIC_TEXT)
        End Function

        ''' <summary>
        ''' 当前的二进制文件格式版本号
        ''' </summary>
        Public Const FORMAT_VERSION As Integer = 1

        ''' <summary>
        ''' 载荷数据采用了 gzip 压缩的标志位
        ''' </summary>
        Private Const FLAG_GZIP As Byte = 1

        ''' <summary>
        ''' 大端 Int32 的字节数量
        ''' </summary>
        Private Const SIZE_INT32 As Integer = 4

        ''' <summary>
        ''' IEEE754 双精度浮点数的字节数量
        ''' </summary>
        Private Const SIZE_DOUBLE As Integer = 8

#Region "write"

        ''' <summary>
        ''' 将二维表对象保存为二进制文件。
        ''' 
        ''' 这个方法不会抛出IO异常：文件写入失败的时候会返回 False
        ''' （与 <c>FileWriter.WriteCsv(String)</c> 的约定一致）。
        ''' </summary>
        ''' <param name="table">需要保存的二维表对象</param>
        ''' <param name="file">目标文件的路径</param>
        ''' <param name="gzip">是否对矩阵载荷进行 gzip 压缩，缺省为不压缩</param>
        ''' <returns>是否保存成功</returns>
        <Extension>
        Public Function WriteBinary(table As NumericTable, file As String, Optional gzip As Boolean = False) As Boolean
            Try
                Using s As Stream = New FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None)
                    Call table.WriteBinary(s, gzip)
                End Using

                Return True
            Catch ex As Exception
                Call System.Diagnostics.Debug.WriteLine($"failed to save the NumericTable object as a binary file: {ex.Message}")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' 将二维表对象写入到指定的流之中（**不会关闭调用方传入的流**）。
        ''' </summary>
        ''' <param name="table">需要保存的二维表对象</param>
        ''' <param name="stream">目标数据流</param>
        ''' <param name="gzip">是否对矩阵载荷进行 gzip 压缩，缺省为不压缩</param>
        <Extension>
        Public Sub WriteBinary(table As NumericTable, stream As Stream, Optional gzip As Boolean = False)
            If table Is Nothing Then
                Throw New ArgumentNullException(NameOf(table))
            End If
            If stream Is Nothing Then
                Throw New ArgumentNullException(NameOf(stream))
            End If

            ' 在写出之前校验维度的一致性，避免把损坏的表写进文件
            Call table.Validate()

            Dim nbo As New NetworkByteOrderBuffer()
            Dim nsamples As Integer = table.nsamples
            Dim nfeatures As Integer = table.nfeatures
            Dim nlabels As Integer = table.nlabels
            Dim flags As Byte = If(gzip, FLAG_GZIP, CByte(0))

            Call WriteBytes(stream, MagicBytes())
            Call WriteInt32(stream, nbo, FORMAT_VERSION)
            Call stream.WriteByte(flags)
            Call WriteInt32(stream, nbo, nsamples)
            Call WriteInt32(stream, nbo, nfeatures)
            Call WriteInt32(stream, nbo, nlabels)

            Call WriteString(stream, nbo, table.name)
            Call WriteString(stream, nbo, table.description)
            Call WriteStrings(stream, nbo, table.rowNames)
            Call WriteStrings(stream, nbo, table.featureNames)
            Call WriteStrings(stream, nbo, table.labelNames)

            Call WriteBlock(stream, nbo, Compress(Flatten(nbo, table.features, nsamples, nfeatures), gzip))
            Call WriteBlock(stream, nbo, Compress(Flatten(nbo, table.labels, nsamples, nlabels), gzip))
        End Sub

#End Region

#Region "read"

        ''' <summary>
        ''' 从二进制文件之中加载一个 <see cref="NumericTable"/> 对象实例。
        ''' 
        ''' 对于公开调用，请使用 <see cref="NumericTable.LoadBinary(String)"/>。
        ''' </summary>
        ''' <param name="file">二进制文件的路径</param>
        ''' <returns></returns>
        Friend Function LoadBinary(file As String) As NumericTable
            If String.IsNullOrEmpty(file) Then
                Throw New ArgumentNullException(NameOf(file))
            End If

            Using s As Stream = New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return LoadBinary(s)
            End Using
        End Function

        ''' <summary>
        ''' 从二进制数据流之中加载一个 <see cref="NumericTable"/> 对象实例。
        ''' 
        ''' 对于公开调用，请使用 <see cref="NumericTable.LoadBinary(Stream)"/>。
        ''' </summary>
        ''' <param name="stream">二进制数据流（**不会关闭调用方传入的流**）</param>
        ''' <returns></returns>
        Friend Function LoadBinary(stream As Stream) As NumericTable
            If stream Is Nothing Then
                Throw New ArgumentNullException(NameOf(stream))
            End If

            Dim nbo As New NetworkByteOrderBuffer()
            Dim magic As Byte() = ReadExactly(stream, MAGIC_TEXT.Length)

            If Not magic.SequenceEqual(MagicBytes()) Then
                Throw New InvalidDataException("invalid magic header, this is not a binary NumericTable data file!")
            End If

            Dim version As Integer = ReadInt32(stream, nbo)

            If version <> FORMAT_VERSION Then
                Throw New InvalidDataException(
                    $"unsupported binary NumericTable format version: {version}, but the current runtime only supports version {FORMAT_VERSION}!"
                )
            End If

            Dim flags As Integer = stream.ReadByte()

            If flags < 0 Then
                Throw New InvalidDataException("unexpected end of stream while reading the format flags!")
            End If

            Dim gzip As Boolean = (CByte(flags) And FLAG_GZIP) <> 0
            Dim nsamples As Integer = ReadInt32(stream, nbo)
            Dim nfeatures As Integer = ReadInt32(stream, nbo)
            Dim nlabels As Integer = ReadInt32(stream, nbo)

            If nsamples < 0 OrElse nfeatures < 0 OrElse nlabels < 0 Then
                Throw New InvalidDataException(
                    $"invalid table dimension [{nsamples} x {nfeatures} + {nlabels}] in the binary NumericTable stream!"
                )
            End If

            Dim name As String = ReadString(stream, nbo)
            Dim description As String = ReadString(stream, nbo)
            Dim rowNames As String() = ReadStrings(stream, nbo)
            Dim featureNames As String() = ReadStrings(stream, nbo)
            Dim labelNames As String() = ReadStrings(stream, nbo)

            Dim features As Double()() = If(nsamples = 0 OrElse nfeatures = 0,
                                            Nothing,
                                            Expand(nbo, Decompress(ReadBlock(stream, nbo), gzip), nsamples, nfeatures, NameOf(features)))
            Dim labels As Double()() = If(nsamples = 0 OrElse nlabels = 0,
                                          Nothing,
                                          Expand(nbo, Decompress(ReadBlock(stream, nbo), gzip), nsamples, nlabels, NameOf(labels)))

            Dim table As New NumericTable(features,
                                          If(rowNames.Length = 0, Nothing, rowNames),
                                          If(featureNames.Length = 0, Nothing, featureNames)) With {
                .labels = labels,
                .labelNames = If(labelNames.Length = 0, Nothing, labelNames),
                .name = name,
                .description = description
            }

            Call table.Validate()

            Return table
        End Function

#End Region

#Region "matrix helpers"

        ''' <summary>
        ''' 将行主序的矩阵摊平为一维数组之后编码为大端字节数据
        ''' </summary>
        Private Function Flatten(nbo As NetworkByteOrderBuffer, matrix As Double()(), rows As Integer, cols As Integer) As Byte()
            If rows = 0 OrElse cols = 0 OrElse matrix Is Nothing Then
                Return New Byte() {}
            End If

            Dim values As Double() = New Double(rows * cols - 1) {}

            For i As Integer = 0 To rows - 1
                Dim row As Double() = matrix(i)

                For j As Integer = 0 To cols - 1
                    values(i * cols + j) = row(j)
                Next
            Next

            Return nbo.GetBytes(values)
        End Function

        ''' <summary>
        ''' 将大端字节数据解码为一维数组之后再还原为行主序的矩阵
        ''' </summary>
        Private Function Expand(nbo As NetworkByteOrderBuffer, data As Byte(), rows As Integer, cols As Integer, part As String) As Double()()
            Dim expected As Integer = rows * cols * SIZE_DOUBLE

            If data.Length <> expected Then
                Throw New InvalidDataException(
                    $"the '{part}' payload size {data.Length} bytes is not equals to the expected size {expected} bytes ({rows} x {cols} x {SIZE_DOUBLE})!"
                )
            End If

            If rows = 0 OrElse cols = 0 Then
                Return Nothing
            End If

            Dim values As Double() = nbo.ParseDouble(data)
            Dim matrix As Double()() = New Double(rows - 1)() {}

            For i As Integer = 0 To rows - 1
                Dim row As Double() = New Double(cols - 1) {}

                For j As Integer = 0 To cols - 1
                    row(j) = values(i * cols + j)
                Next

                matrix(i) = row
            Next

            Return matrix
        End Function

#End Region

#Region "primitive helpers"

        Private Function Compress(data As Byte(), gzip As Boolean) As Byte()
            If Not gzip OrElse data.Length = 0 Then
                Return data
            End If

            Using ms As New MemoryStream(data)
                Return ms.GZipStream().ToArray
            End Using
        End Function

        Private Function Decompress(data As Byte(), gzip As Boolean) As Byte()
            If Not gzip OrElse data.Length = 0 Then
                Return data
            Else
                ' UnGzipStream 会自动补齐可能缺失的gzip magic标记
                Return data.UnGzipStream.ToArray
            End If
        End Function

        Private Sub WriteInt32(stream As Stream, nbo As NetworkByteOrderBuffer, value As Integer)
            Call WriteBytes(stream, nbo.GetBytes(New Integer() {value}))
        End Sub

        Private Sub WriteBytes(stream As Stream, data As Byte())
            If data IsNot Nothing AndAlso data.Length > 0 Then
                Call stream.Write(data, 0, data.Length)
            End If
        End Sub

        ''' <summary>
        ''' 写入一个以「大端Int32长度前缀 + 原始字节」构成的字节块
        ''' </summary>
        Private Sub WriteBlock(stream As Stream, nbo As NetworkByteOrderBuffer, data As Byte())
            Call WriteInt32(stream, nbo, If(data Is Nothing, 0, data.Length))
            Call WriteBytes(stream, data)
        End Sub

        Private Sub WriteString(stream As Stream, nbo As NetworkByteOrderBuffer, text As String)
            Call WriteBlock(stream, nbo, If(String.IsNullOrEmpty(text), New Byte() {}, Encoding.UTF8.GetBytes(text)))
        End Sub

        Private Sub WriteStrings(stream As Stream, nbo As NetworkByteOrderBuffer, values As String())
            Dim list As String() = If(values, New String() {})

            Call WriteInt32(stream, nbo, list.Length)

            For Each s As String In list
                Call WriteString(stream, nbo, s)
            Next
        End Sub

        Private Function ReadInt32(stream As Stream, nbo As NetworkByteOrderBuffer) As Integer
            Return nbo.ParseInteger(ReadExactly(stream, SIZE_INT32))(Scan0)
        End Function

        Private Function ReadBlock(stream As Stream, nbo As NetworkByteOrderBuffer) As Byte()
            Dim len As Integer = ReadInt32(stream, nbo)

            If len < 0 Then
                Throw New InvalidDataException($"invalid byte block length {len} in the binary NumericTable stream!")
            End If

            Return ReadExactly(stream, len)
        End Function

        Private Function ReadString(stream As Stream, nbo As NetworkByteOrderBuffer) As String
            Dim data As Byte() = ReadBlock(stream, nbo)

            If data.Length = 0 Then
                Return Nothing
            Else
                Return Encoding.UTF8.GetString(data)
            End If
        End Function

        Private Function ReadStrings(stream As Stream, nbo As NetworkByteOrderBuffer) As String()
            Dim count As Integer = ReadInt32(stream, nbo)

            If count < 0 Then
                Throw New InvalidDataException($"invalid string array size {count} in the binary NumericTable stream!")
            End If

            Dim list As String() = New String(count - 1) {}

            For i As Integer = 0 To count - 1
                list(i) = ReadString(stream, nbo)
            Next

            Return list
        End Function

        ''' <summary>
        ''' 从流之中精确地读取指定数量的字节，数据不足的时候抛出 <see cref="InvalidDataException"/>
        ''' </summary>
        Private Function ReadExactly(stream As Stream, count As Integer) As Byte()
            Dim buffer As Byte() = New Byte(count - 1) {}
            Dim offset As Integer = 0

            While offset < count
                Dim n As Integer = stream.Read(buffer, offset, count - offset)

                If n <= 0 Then
                    Throw New InvalidDataException(
                        $"unexpected end of the binary NumericTable stream: only {offset} of {count} bytes were read!"
                    )
                End If

                offset += n
            End While

            Return buffer
        End Function

#End Region
    End Module
End Namespace
