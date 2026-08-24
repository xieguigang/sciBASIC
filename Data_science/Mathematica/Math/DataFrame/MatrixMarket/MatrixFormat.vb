Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module MatrixFormat

    ReadOnly hostBuf As New NetworkByteOrderBuffer

    ''' <summary>
    ''' 未压缩（v1）格式的 magic 头。
    ''' </summary>
    Const magic As String = "scibasic.net/data-matrix"
    ''' <summary>
    ''' 支持压缩标志（v2）格式的 magic 头。
    ''' </summary>
    Const magicV2 As String = "scibasic.net/data-matrix/v2"

    ''' <summary>
    ''' v2 头部中的压缩标志位：0 表示未压缩，1 表示后续数据经过 gzip 压缩。
    ''' </summary>
    Enum MatrixCompression As Byte
        None = 0
        GZip = 1
    End Enum

    ''' <summary>
    ''' 以未压缩（v1）格式写入矩阵，保持对既有文件的向后兼容。
    ''' </summary>
    <Extension>
    Public Function WriteData(m As DataMatrix, file As Stream) As Boolean
        Using wr As New BinaryWriter(file)
            Call wr.Write(Encoding.ASCII.GetBytes(magic))
            Call wr.Write(m.GetLabels.GetJson)

            For Each row As Double() In m.matrix
                Call wr.Write(hostBuf.GetBytes(row))
            Next
        End Using

        Return True
    End Function

    ''' <summary>
    ''' 写入矩阵数据，可选启用 gzip 压缩。
    ''' 当 <paramref name="compress"/> 为 True 时写入 v2 格式（magic + 1 字节压缩标志 + gzip 数据块）；
    ''' 否则写入与 <see cref="WriteData(DataMatrix, Stream)"/> 完全一致的 v1 未压缩格式。
    ''' </summary>
    <Extension>
    Public Function WriteData(m As DataMatrix, file As Stream, compress As Boolean) As Boolean
        If Not compress Then
            Return m.WriteData(file)
        End If

        Using wr As New BinaryWriter(file, Encoding.ASCII, leaveOpen:=True)
            Call wr.Write(Encoding.ASCII.GetBytes(magicV2))
            ' 写入压缩标志位（位于 magic 之后、数据块之前）
            wr.Write(CByte(MatrixCompression.GZip))
        End Using

        ' 以 GZipStream 包裹剩余流，对标签 JSON 与矩阵字节整体压缩写入
        Using gzip As New GZipStream(file, CompressionMode.Compress, leaveOpen:=True)
            Using wr As New BinaryWriter(gzip)
                Call wr.Write(m.GetLabels.GetJson)

                For Each row As Double() In m.matrix
                    Call wr.Write(hostBuf.GetBytes(row))
                Next
            End Using
        End Using

        Return True
    End Function

    Public Function ReadData(file As Stream) As DataMatrix
        Using rd As New BinaryReader(file)
            Dim testMagic As String = Encoding.ASCII.GetString(rd.ReadBytes(magic.Length))

            If testMagic = magic Then
                ' v1 未压缩格式：magic 后紧跟标签 JSON 与矩阵字节
                Return rd.ReadMatrix
            End If

            If testMagic = magicV2 Then
                ' v2 格式：magic 后紧跟 1 字节压缩标志，据此决定是否解压
                Dim flag As MatrixCompression = CType(rd.ReadByte(), MatrixCompression)

                If flag = MatrixCompression.GZip Then
                    Using gzip As New GZipStream(file, CompressionMode.Decompress, leaveOpen:=True)
                        Return New BinaryReader(gzip).ReadMatrix
                    End Using
                Else
                    ' 标志为 None：按未压缩方式继续解析剩余流
                    Return rd.ReadMatrix
                End If
            End If

            Throw New InvalidDataException("Invalid magic header of the target matrix file!")
        End Using
    End Function

    <Extension>
    Private Function ReadMatrix(rd As BinaryReader) As DataMatrix
        Dim labels As String() = rd.ReadString.LoadJSON(Of String())
        Dim mat As Double()() = New Double(labels.Length - 1)() {}
        Dim width As Integer = labels.Length * RawStream.DblFloat

        For i As Integer = 0 To labels.Length - 1
            mat(i) = hostBuf.ParseDouble(rd.ReadBytes(width))
        Next

        Return New DataMatrix(labels, mat)
    End Function

End Module
