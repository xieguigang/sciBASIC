Option Strict On
Option Explicit On

Namespace Snappy

    ''' <summary>
    ''' CRC32C（Castagnoli）校验和，以及 Snappy 分块流使用的掩码变换。
    ''' <para>
    ''' 注意：本项目 <c>Bzip2.Math.CRC32</c> 使用的是 IEEE 多项式 0x04C11DB7，
    ''' 与这里需要的 CRC32C 反射多项式 0x82F63B78 不同，不能互相替代。
    ''' </para>
    ''' </summary>
    Public NotInheritable Class Crc32C

        Private Const Polynomial As UInteger = &H82F63B78UI
        Private Const MaskDelta As ULong = &HA282EAD8UL
        Private Const Mask32 As ULong = &HFFFFFFFFUL

        Private Shared ReadOnly Table As UInteger() = CreateTable()

        Private Sub New()
        End Sub

        Private Shared Function CreateTable() As UInteger()
            Dim table(255) As UInteger

            For i As Integer = 0 To 255
                Dim crc As UInteger = CUInt(i)

                For bit As Integer = 0 To 7
                    If (crc And 1UI) <> 0UI Then
                        crc = (crc >> 1) Xor Polynomial
                    Else
                        crc >>= 1
                    End If
                Next

                table(i) = crc
            Next

            Return table
        End Function

        ''' <summary>计算整个字节数组的 CRC32C。</summary>
        Public Shared Function ComputeHash(data As Byte()) As UInteger
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            Return ComputeHash(data, 0, data.Length)
        End Function

        ''' <summary>计算字节数组指定范围的 CRC32C。</summary>
        Public Shared Function ComputeHash(data As Byte(), offset As Integer, count As Integer) As UInteger
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If offset < 0 OrElse count < 0 OrElse offset > data.Length OrElse count > data.Length - offset Then
                Throw New ArgumentOutOfRangeException("offset/count 超出了数据范围。")
            End If

            Dim crc As UInteger = &HFFFFFFFFUI

            For i As Integer = offset To offset + count - 1
                crc = (crc >> 8) Xor Table(CInt((crc Xor CUInt(data(i))) And &HFFUI))
            Next

            Return crc Xor &HFFFFFFFFUI
        End Function

        ''' <summary>
        ''' 对 CRC32C 应用 Snappy 分块流的掩码：
        ''' <c>((crc &gt;&gt; 15) Or (crc &lt;&lt; 17)) + 0xA282EAD8</c>（32 位回绕）。
        ''' </summary>
        Public Shared Function Mask(crc As UInteger) As UInteger
            Dim rotated As ULong = (CULng(crc) >> 15) Or ((CULng(crc) << 17) And Mask32)
            rotated = (rotated + MaskDelta) And Mask32
            Return CUInt(rotated)
        End Function

        ''' <summary>撤销 <see cref="Mask(UInteger)"/> 的掩码变换。</summary>
        Public Shared Function Unmask(masked As UInteger) As UInteger
            Dim rotated As ULong = (CULng(masked) - MaskDelta) And Mask32
            Dim crc As ULong = ((rotated << 15) Or (rotated >> 17)) And Mask32
            Return CUInt(crc)
        End Function

    End Class
End Namespace
