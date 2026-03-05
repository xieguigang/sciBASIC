Namespace HashMaps.MinHash

    Public Module MinHash

        ' 生成MinHash签名
        Public Function GenerateMinHashSignature(shingles As HashSet(Of String)) As List(Of Integer)
            Dim signature As New List(Of Integer)(Enumerable.Repeat(Integer.MaxValue, Config.Num_HashFunctions))

            ' 预定义哈希函数的系数 (实际应用中应随机生成并固定)
            ' 这里用简单的系数模拟 N 个不同的哈希函数
            Dim hashCoeffsA As Integer() = Enumerable.Range(1, Config.Num_HashFunctions).ToArray()
            Dim hashCoeffsB As Integer() = Enumerable.Range(100, Config.Num_HashFunctions).ToArray()
            Dim primeP As Integer = 2147483647 ' 一个大素数

            ' 遍历集合中的每一个 Shingle
            For Each shingle In shingles
                ' 1. 将Shingle转为整数 (作为哈希函数的输入x)
                Dim rawHash As Integer = shingle.GetHashCode() ' 简单起见用系统哈希

                ' 2. 对每一个哈希函数计算值，并更新最小值
                For i As Integer = 0 To Config.Num_HashFunctions - 1
                    ' 模拟第 i 个哈希函数: h_i(x) = (a*x + b) % p
                    Dim hashVal As Integer = (hashCoeffsA(i) * rawHash + hashCoeffsB(i)) Mod primeP

                    ' 保持取绝对值 (VB.NET Mod可能返回负数)
                    If hashVal < 0 Then hashVal += primeP

                    ' 更新签名中对应位置的最小值
                    If hashVal < signature(i) Then
                        signature(i) = hashVal
                    End If
                Next
            Next

            Return signature
        End Function
    End Module
End Namespace