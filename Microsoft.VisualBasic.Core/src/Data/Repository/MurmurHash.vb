Namespace Data.Repository

    Public Module MurmurHash

        Const c1 As UInteger = &HCC9E2D51UI
        Const c2 As UInteger = &H1B873593UI
        Const r1 As Integer = 15
        Const r2 As Integer = 13
        Const m As UInteger = 5UI
        Const n As UInteger = &HE6546B64UI

        ''' <summary>
        ''' 计算给定数据的32位MurmurHash3值。
        ''' </summary>
        ''' <param name="data">输入数据。</param>
        ''' <param name="seed">哈希种子。</param>
        ''' <returns>32位无符号哈希值。</returns>
        Public Function MurmurHash332(data As Byte(), seed As UInteger) As UInteger
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))

            Dim length As Integer = data.Length
            Dim numBlocks As Integer = length \ 4
            Dim h As UInteger = seed

            ' 处理4字节块
            Dim i As Integer = 0
            While i < numBlocks * 4
                Dim k As UInteger = CUInt(data(i)) Or
                               CUInt(data(i + 1)) << 8 Or
                               CUInt(data(i + 2)) << 16 Or
                               CUInt(data(i + 3)) << 24
                i += 4

                k *= c1
                k = (k << r1) Or (k >> (32 - r1))
                k *= c2

                h = h Xor k
                h = (h << r2) Or (h >> (32 - r2))
                h = h * m + n
            End While

            ' 处理尾部剩余字节
            Dim tailLength As Integer = length - i
            If tailLength > 0 Then
                Dim k As UInteger = 0
                Select Case tailLength
                    Case 3
                        k = k Xor CUInt(data(i + 2)) << 16
                        Exit Select
                    Case 2
                        k = k Xor CUInt(data(i + 1)) << 8
                        Exit Select
                    Case 1
                        k = k Xor data(i)
                        Exit Select
                End Select
                If tailLength > 0 Then
                    k *= c1
                    k = (k << r1) Or (k >> (32 - r1))
                    k *= c2
                    h = h Xor k
                End If
            End If

            ' 最终混合
            h = h Xor CUInt(length)
            h = h Xor (h >> 16)
            h *= &H85EBCA6BUI
            h = h Xor (h >> 13)
            h *= &HC2B2AE35UI
            h = h Xor (h >> 16)

            Return h
        End Function
    End Module
End Namespace