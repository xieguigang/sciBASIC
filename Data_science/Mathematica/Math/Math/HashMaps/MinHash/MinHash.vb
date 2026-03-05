Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.Repository

Namespace HashMaps.MinHash

    Public Module MinHash

        ''' <summary>
        ''' 生成MinHash签名
        ''' </summary>
        ''' <param name="shingles"></param>
        ''' <returns></returns>
        Public Function GenerateMinHashSignature(shingles As HashSet(Of String)) As UInteger()
            Dim signature As UInteger() = New UInteger(Config.Num_HashFunctions - 1) {}
            Dim shingleBytesList As New List(Of Byte())()
            For Each shingle In shingles
                shingleBytesList.Add(Encoding.UTF8.GetBytes(shingle))
            Next
            ' 3. 计算 MinHash
            '    遍历每一个 Shingle 的字节数据
            For Each bytesData In shingleBytesList
                ' 遍历每一个哈希函数 (由 seed 0 到 N-1 代表)
                For i As Integer = 0 To Config.Num_HashFunctions - 1
                    ' 使用索引 i 作为种子，相当于第 i 个哈希函数
                    Dim hashVal As UInteger = MurmurHash.MurmurHashCode3_x86_32(bytesData, CUInt(i))

                    ' 更新签名中对应位置的最小值
                    If hashVal < signature(i) Then
                        signature(i) = hashVal
                    End If
                Next
            Next

            Return signature
        End Function

        ''' <summary>
        ''' Compute minhash for a given sequence
        ''' </summary>
        ''' <param name="items">the items inside a sequence</param>
        ''' <param name="id">sequence id</param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateSequenceData(items As IEnumerable(Of String), id As Integer) As SequenceItem
            Dim shingles As New HashSet(Of String)

            For Each item As String In items
                Call shingles.Add(item)
            Next

            Return New SequenceItem With {
                .ID = id,
                .Signature = MinHash.GenerateMinHashSignature(shingles)
            }
        End Function
    End Module
End Namespace