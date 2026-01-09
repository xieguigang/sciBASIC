Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.Repository
Imports _rng = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace ComponentModel.DataSourceModel.Repository

    ''' <summary>
    ''' 提供布谷鸟过滤器的实现。
    ''' 布谷鸟过滤器是一种基于布谷鸟哈希的紧凑型概率数据结构，支持插入、删除和成员查询。
    ''' 相比布隆过滤器，它支持动态删除元素，且在中等假阳性率下通常具有更高的空间效率。
    ''' </summary>
    Public Class CuckooFilter

        ''' <summary>
        ''' 存储桶的数组。使用一维数组模拟二维结构以提升性能。
        ''' 索引计算公式：index = bucketIndex * bucketSize + slotIndex
        ''' </summary>
        Private _buckets As UInteger()

        ''' <summary>
        ''' 桶的数量
        ''' </summary>
        Public ReadOnly Property BucketCount As Integer

        ''' <summary>
        ''' 每个桶中包含的槽位数量（即每个桶能存多少个指纹）。
        ''' </summary>
        Public ReadOnly Property BucketSize As Integer

        ''' <summary>
        ''' 插入失败时的最大重试（踢出）次数。
        ''' 如果超过此次数仍无法插入，则认为过滤器已满。
        ''' </summary>
        Public ReadOnly Property MaxKicks As Integer

        Default Public ReadOnly Property CheckItem(item As String) As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Contains(item)
            End Get
        End Property

        ''' <summary>
        ''' 使用指定的期望容量和桶参数初始化 CuckooFilter 类的新实例。
        ''' </summary>
        ''' <param name="capacity">期望插入的元素数量。</param>
        ''' <param name="bucketSize">每个桶的槽位数。默认为 4，推荐的值为 4 或 8。</param>
        ''' <param name="maxKicks">最大踢出重试次数。默认为 500。</param>
        Public Sub New(capacity As Integer, Optional bucketSize As Integer = 4, Optional maxKicks As Integer = 500)
            If capacity <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(capacity), "容量必须为正数。")
            If bucketSize <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(bucketSize), "桶大小必须为正数。")

            _BucketSize = bucketSize
            _MaxKicks = maxKicks

            ' 布谷鸟过滤器在负载因子较高（如 95%）时仍能保持良好性能
            ' 计算桶数量 m = capacity / (bucketSize * loadFactor)
            Dim loadFactor As Double = 0.95
            _BucketCount = CInt(std.Ceiling(capacity / (CDbl(bucketSize) * loadFactor)))

            ' 初始化数组，UInteger 默认值为 0，表示空槽
            _buckets = New UInteger(_BucketCount * bucketSize - 1) {}
        End Sub

        ''' <summary>
        ''' 将指定的元素添加到布谷鸟过滤器中。
        ''' </summary>
        ''' <param name="item">要添加的元素。不能为 Nothing。</param>
        ''' <returns>如果添加成功则为 true；如果过滤器已满且插入失败则为 false。</returns>
        Public Function Add(item As String) As Boolean
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))

            ' 1. 计算哈希值
            Dim data As Byte() = Encoding.UTF8.GetBytes(item)
            Dim hashVal As UInteger = MurmurHash.MurmurHashCode3_x86_32(data, 0)

            ' 2. 提取指纹和计算索引
            ' 指纹：取哈希值的低 16 位作为指纹 (也可以取更多位以降低误判)
            Dim fp As UInteger = hashVal And &HFFFFUI
            ' 指纹不能为 0，因为 0 用于表示空槽
            If fp = 0 Then fp = 1

            ' 索引 i1：直接使用哈希值取模
            Dim i1 As Integer = CInt(hashVal Mod CUInt(_BucketCount))
            ' 索引 i2：使用 i1 和 fp 计算得到
            Dim i2 As Integer = GetAltIndex(i1, fp)

            ' 3. 尝试直接插入到 i1 或 i2
            If TryInsert(i1, fp) OrElse TryInsert(i2, fp) Then
                Return True
            End If

            ' 4. 如果两个桶都满了，执行“踢出”流程
            ' 随机选择一个桶作为起始踢出位置
            Dim currIndex As Integer = If(_rng.NextDouble() < 0.5, i1, i2)

            For i As Integer = 0 To _MaxKicks - 1
                ' 随机选择当前桶中的一个槽位
                Dim slot As Integer = _rng.Next(_BucketSize)
                Dim arrIndex As Integer = currIndex * _BucketSize + slot

                ' 获取被踢出的旧指纹
                Dim oldFp As UInteger = _buckets(arrIndex)

                ' 将新指纹放入该位置（踢出旧的）
                _buckets(arrIndex) = fp

                ' 更新当前变量：被踢出的指纹现在需要去往它的另一个位置
                fp = oldFp
                currIndex = GetAltIndex(currIndex, fp)

                ' 尝试将被踢出的 fp 插入到新计算出的位置
                If TryInsert(currIndex, fp) Then
                    Return True
                End If
            Next

            ' 如果循环结束仍未插入成功，说明表太拥挤了
            Return False
        End Function

        ''' <summary>
        ''' 检查布谷鸟过滤器中是否可能包含指定的元素。
        ''' </summary>
        ''' <param name="item">要检查的元素。不能为 Nothing。</param>
        ''' <returns>如果元素可能存在，则为 true；如果元素绝对不存在，则为 false。</returns>
        Public Function Contains(item As String) As Boolean
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))

            Dim data As Byte() = Encoding.UTF8.GetBytes(item)
            Dim hashVal As UInteger = MurmurHash.MurmurHashCode3_x86_32(data, 0)

            Dim fp As UInteger = hashVal And &HFFFFUI
            If fp = 0 Then fp = 1

            Dim i1 As Integer = CInt(hashVal Mod CUInt(_BucketCount))
            Dim i2 As Integer = GetAltIndex(i1, fp)

            ' 只要两个桶中任意一个包含该指纹，即认为可能存在
            Return Lookup(i1, fp) OrElse Lookup(i2, fp)
        End Function

        ''' <summary>
        ''' 从过滤器中删除指定的元素。
        ''' </summary>
        ''' <param name="item">要删除的元素。不能为 Nothing。</param>
        ''' <returns>如果元素存在并被删除，则为 true；如果元素不存在，则为 false。</returns>
        Public Function Delete(item As String) As Boolean
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))

            Dim data As Byte() = Encoding.UTF8.GetBytes(item)
            Dim hashVal As UInteger = MurmurHash.MurmurHashCode3_x86_32(data, 0)

            Dim fp As UInteger = hashVal And &HFFFFUI
            If fp = 0 Then fp = 1

            Dim i1 As Integer = CInt(hashVal Mod CUInt(_BucketCount))
            Dim i2 As Integer = GetAltIndex(i1, fp)

            ' 尝试从两个桶中删除该指纹
            Return TryDelete(i1, fp) OrElse TryDelete(i2, fp)
        End Function

        ''' <summary>
        ''' 计算备用索引位置。
        ''' 公式：i2 = i1 XOR hash(fp)
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function GetAltIndex(i1 As Integer, fp As UInteger) As Integer
            ' 需要对指纹进行哈希以计算偏移量。
            ' 这里再次调用 MurmurHash，将指纹转换回字节数组。
            ' 注意：频繁的 BitConverter.GetBytes 会产生微小垃圾，但在 .NET 中通常可接受。
            ' 也可以使用简单的位混淆函数替代以提高极致性能。
            Dim fpHash As UInteger = MurmurHash.MurmurHashCode3_x86_32(BitConverter.GetBytes(fp), &HFFFFFFFFUI)

            ' 使用异或生成 i2，并取模确保在范围内
            Return (i1 Xor CInt(fpHash)) Mod _BucketCount
        End Function

        ''' <summary>
        ''' 尝试将指纹插入到指定桶的任意空槽位中。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function TryInsert(bucketIndex As Integer, fp As UInteger) As Boolean
            Dim startIdx As Integer = bucketIndex * _BucketSize
            For i As Integer = 0 To _BucketSize - 1
                If _buckets(startIdx + i) = 0 Then
                    _buckets(startIdx + i) = fp
                    Return True
                End If
            Next
            Return False
        End Function

        ''' <summary>
        ''' 在指定桶中查找是否存在特定指纹。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Lookup(bucketIndex As Integer, fp As UInteger) As Boolean
            Dim startIdx As Integer = bucketIndex * _BucketSize
            For i As Integer = 0 To _BucketSize - 1
                If _buckets(startIdx + i) = fp Then
                    Return True
                End If
            Next
            Return False
        End Function

        ''' <summary>
        ''' 尝试从指定桶中删除特定指纹。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function TryDelete(bucketIndex As Integer, fp As UInteger) As Boolean
            Dim startIdx As Integer = bucketIndex * _BucketSize
            For i As Integer = 0 To _BucketSize - 1
                If _buckets(startIdx + i) = fp Then
                    _buckets(startIdx + i) = 0 ' 清空槽位
                    Return True
                End If
            Next
            Return False
        End Function

    End Class

End Namespace