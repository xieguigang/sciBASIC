Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.Repository
Imports Microsoft.VisualBasic.Language.Java

Namespace HashMaps.MinHash

    Public Module MinHash

        ''' <summary>
        ''' 生成MinHash签名
        ''' </summary>
        ''' <param name="shingles"></param>
        ''' <param name="Num_HashFunctions">
        ''' MinHash签名长度
        ''' </param>
        ''' <returns></returns>
        Public Function GenerateMinHashSignature(shingles As HashSet(Of String), Num_HashFunctions As Integer) As UInteger()
            Dim signature As UInteger() = New UInteger(Num_HashFunctions - 1) {}.fill(UInteger.MaxValue)

            ' 3. 计算 MinHash
            '    遍历每一个 Shingle 的字节数据
            For Each shingle As String In shingles
                Dim bytesData As Byte() = Encoding.UTF8.GetBytes(shingle)

                ' 遍历每一个哈希函数 (由 seed 0 到 N-1 代表)
                For i As Integer = 0 To Num_HashFunctions - 1
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
        ''' <param name="items">对字符串序列进行n-gram切片后的结果</param>
        ''' <param name="id">sequence id</param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateSequenceData(items As IEnumerable(Of String), id As Integer, Optional Num_HashFunctions As Integer = 100) As SequenceItem
            Dim shingles As New HashSet(Of String)

            For Each item As String In items
                Call shingles.Add(item)
            Next

            Return New SequenceItem With {
                .ID = id,
                .Signature = MinHash.GenerateMinHashSignature(shingles, Num_HashFunctions)
            }
        End Function
    End Module
End Namespace