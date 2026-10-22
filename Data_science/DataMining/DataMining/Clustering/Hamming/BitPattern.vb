Imports System.Numerics
Imports System.Runtime.CompilerServices

Namespace Hamming

    ''' <summary>
    ''' 定长二进制模式（位串）的位打包值对象。
    ''' 
    ''' 每一位占用一个比特：第 ``i`` 位保存在第 ``i \ 64`` 个机器字的
    ''' 第 ``i Mod 64`` 位上（低位起）。这样的布局可以保证 <see cref="ToString"/>
    ''' 逐字回显原始的 ``"10110010"`` 形式，同时让距离计算走
    ''' 「异或 + popcount」的硬件级快速路径。
    ''' </summary>
    Public NotInheritable Class BitPattern

        Private ReadOnly _words As ULong()
        Private ReadOnly _bitLength As Integer
        Private _hash As Integer = Integer.MinValue

        ''' <summary>
        ''' 二进制位的总长度（也就是特征维数 ``d``）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BitLength As Integer
            Get
                Return _bitLength
            End Get
        End Property

        ''' <summary>
        ''' 位打包之后的机器字数组。返回的是内部缓冲区，调用方**不得**修改其内容。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Words As ULong()
            Get
                Return _words
            End Get
        End Property

        Private Sub New(words As ULong(), bitLength As Integer)
            _words = words
            _bitLength = bitLength
        End Sub

#Region "Factory"

        ''' <summary>
        ''' 从 ``"10110010"`` 形式的位串构造位模式对象。
        ''' </summary>
        ''' <param name="bits">只允许包含 ``0`` 与 ``1`` 的非空字符串</param>
        ''' <returns></returns>
        ''' <exception cref="ArgumentException">位串为空或者包含非法字符</exception>
        Public Shared Function FromString(bits As String) As BitPattern
            If String.IsNullOrEmpty(bits) Then
                Throw New ArgumentException("the binary string cannot be null or empty!", NameOf(bits))
            End If

            Dim words As ULong() = Allocate(bits.Length)

            For i As Integer = 0 To bits.Length - 1
                Select Case bits(i)
                    Case "0"c
                        ' 保持为 0
                    Case "1"c
                        SetBit(words, i)
                    Case Else
                        Throw New ArgumentException($"invalid binary character '{bits(i)}' at position {i}, only '0' and '1' are allowed!", NameOf(bits))
                End Select
            Next

            Return New BitPattern(words, bits.Length)
        End Function

        ''' <summary>
        ''' 从布尔序列构造位模式对象，``True`` 记为 1。
        ''' </summary>
        ''' <param name="values">布尔序列</param>
        ''' <returns></returns>
        Public Shared Function FromBooleans(values As Boolean()) As BitPattern
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Dim words As ULong() = Allocate(values.Length)

            For i As Integer = 0 To values.Length - 1
                If values(i) Then
                    SetBit(words, i)
                End If
            Next

            Return New BitPattern(words, values.Length)
        End Function

        ''' <summary>
        ''' 从数值向量构造位模式对象，大于 <paramref name="threshold"/> 的位置记为 1。
        ''' 
        ''' 缺省阈值 ``0.5`` 对应 ``0/1`` 二值数据的自然二值化；``NaN`` 一律记为 0。
        ''' </summary>
        ''' <param name="values">数值向量</param>
        ''' <param name="threshold">二值化的阈值（严格大于阈值才记为 1）</param>
        ''' <returns></returns>
        Public Shared Function FromValues(values As Double(), Optional threshold As Double = 0.5) As BitPattern
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Dim words As ULong() = Allocate(values.Length)

            For i As Integer = 0 To values.Length - 1
                If values(i) > threshold Then
                    SetBit(words, i)
                End If
            Next

            Return New BitPattern(words, values.Length)
        End Function

        ''' <summary>
        ''' 从整数向量构造位模式对象，非 0 的位置记为 1。
        ''' </summary>
        ''' <param name="values">整数向量</param>
        ''' <returns></returns>
        Public Shared Function FromIntegers(values As Integer()) As BitPattern
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Dim words As ULong() = Allocate(values.Length)

            For i As Integer = 0 To values.Length - 1
                If values(i) <> 0 Then
                    SetBit(words, i)
                End If
            Next

            Return New BitPattern(words, values.Length)
        End Function

        ''' <summary>
        ''' 从已经打包好的机器字数组构造位模式对象。
        ''' </summary>
        ''' <param name="words">机器字数组，长度必须等于 ``⌈bitLength / 64⌉``</param>
        ''' <param name="bitLength">二进制位的总长度</param>
        ''' <returns></returns>
        Public Shared Function FromWords(words As ULong(), bitLength As Integer) As BitPattern
            If words Is Nothing Then
                Throw New ArgumentNullException(NameOf(words))
            End If
            If bitLength <= 0 Then
                Throw New ArgumentException($"the bit length must be a positive integer, but {bitLength}!", NameOf(bitLength))
            End If

            Dim expected As Integer = WordCount(bitLength)

            If words.Length <> expected Then
                Throw New ArgumentException($"the words array has {words.Length} elements, but {expected} elements are required for {bitLength} bits!", NameOf(words))
            End If

            Dim copy As ULong() = DirectCast(words.Clone(), ULong())

            MaskTail(copy, bitLength)

            Return New BitPattern(copy, bitLength)
        End Function

#End Region

        ''' <summary>
        ''' 读取指定位下标上的二进制值。
        ''' </summary>
        ''' <param name="index">位下标（0 基）</param>
        ''' <returns></returns>
        Public Function BitAt(index As Integer) As Boolean
            If index < 0 OrElse index >= _bitLength Then
                Throw New ArgumentOutOfRangeException(NameOf(index), $"the bit index must be within [0, {_bitLength - 1}], but {index}!")
            End If

            Return ((_words(index \ 64) >> (index Mod 64)) And 1UL) = 1UL
        End Function

        ''' <summary>
        ''' 值为 1 的二进制位个数（Hamming 权重）。
        ''' </summary>
        ''' <returns></returns>
        Public Function CountOnes() As Integer
            Return HammingDistance.Ones(_words)
        End Function

        ''' <summary>
        ''' 计算当前模式和另一个等长模式之间的 Hamming 距离。
        ''' </summary>
        ''' <param name="other">另一个位模式</param>
        ''' <returns>取值不同的位数</returns>
        ''' <exception cref="ArgumentException">两个模式的长度不一致</exception>
        Public Function DistanceTo(other As BitPattern) As Integer
            If other Is Nothing Then
                Throw New ArgumentNullException(NameOf(other))
            End If
            If other._bitLength <> _bitLength Then
                Throw New ArgumentException($"the two patterns must have the same length, but {_bitLength} <> {other._bitLength}!", NameOf(other))
            End If

            Return HammingDistance.Between(_words, other._words)
        End Function

        ''' <summary>
        ''' 计算与另一个等长模式的相似度（``1 - distance / length``）。
        ''' </summary>
        ''' <param name="other">另一个位模式</param>
        ''' <returns></returns>
        Public Function SimilarityTo(other As BitPattern) As Double
            Return HammingDistance.Similarity(DistanceTo(other), _bitLength)
        End Function

        ''' <summary>
        ''' 按位异或，返回新的位模式对象。
        ''' </summary>
        ''' <param name="other">另一个等长模式</param>
        ''' <returns></returns>
        Public Function XorWith(other As BitPattern) As BitPattern
            Return Binary(other, Function(x, y) x Xor y, NameOf(other))
        End Function

        ''' <summary>
        ''' 按位与，返回新的位模式对象。
        ''' </summary>
        ''' <param name="other">另一个等长模式</param>
        ''' <returns></returns>
        Public Function AndWith(other As BitPattern) As BitPattern
            Return Binary(other, Function(x, y) x And y, NameOf(other))
        End Function

        ''' <summary>
        ''' 按位取反（超出位长的尾部比特会被清零），返回新的位模式对象。
        ''' </summary>
        ''' <returns></returns>
        Public Function NotOf() As BitPattern
            Dim copy As ULong() = DirectCast(_words.Clone(), ULong())

            For i As Integer = 0 To copy.Length - 1
                copy(i) = Not copy(i)
            Next

            MaskTail(copy, _bitLength)

            Return New BitPattern(copy, _bitLength)
        End Function

        ''' <summary>
        ''' 回显为 ``"10110010"`` 形式的位串（与 <see cref="FromString"/> 互逆）。
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim chars As Char() = New Char(_bitLength - 1) {}

            For i As Integer = 0 To _bitLength - 1
                chars(i) = If(((_words(i \ 64) >> (i Mod 64)) And 1UL) = 1UL, "1"c, "0"c)
            Next

            Return New String(chars)
        End Function

        ''' <summary>
        ''' 位长与全部机器字都相同才认为两个模式相等。
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other As BitPattern = TryCast(obj, BitPattern)

            If other Is Nothing Then
                Return False
            End If
            If other._bitLength <> _bitLength Then
                Return False
            End If

            For i As Integer = 0 To _words.Length - 1
                If _words(i) <> other._words(i) Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 使用 <see cref="System.HashCode"/> 组合位长与全部机器字，避免短模式的哈希碰撞。
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetHashCode() As Integer
            If _hash <> Integer.MinValue Then
                Return _hash
            End If

            Dim hash As New HashCode()

            hash.Add(_bitLength)

            For i As Integer = 0 To _words.Length - 1
                hash.Add(_words(i))
            Next

            _hash = hash.ToHashCode()

            Return _hash
        End Function

        ''' <summary>
        ''' 位模式相等比较。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Operator =(x As BitPattern, y As BitPattern) As Boolean
            If x Is Nothing Then
                Return y Is Nothing
            End If

            Return x.Equals(y)
        End Operator

        ''' <summary>
        ''' 位模式不等比较。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Operator <>(x As BitPattern, y As BitPattern) As Boolean
            If x Is Nothing Then
                Return y IsNot Nothing
            End If

            Return Not x.Equals(y)
        End Operator

        Private Function Binary(other As BitPattern, [op] As Func(Of ULong, ULong, ULong), argument As String) As BitPattern
            If other Is Nothing Then
                Throw New ArgumentNullException(NameOf(other))
            End If
            If other._bitLength <> _bitLength Then
                Throw New ArgumentException($"the two patterns must have the same length, but {_bitLength} <> {other._bitLength}!", argument)
            End If

            Dim result As ULong() = New ULong(_words.Length - 1) {}

            For i As Integer = 0 To result.Length - 1
                result(i) = [op](_words(i), other._words(i))
            Next

            Return New BitPattern(result, _bitLength)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function WordCount(bitLength As Integer) As Integer
            Return (bitLength - 1) \ 64 + 1
        End Function

        Private Shared Function Allocate(bitLength As Integer) As ULong()
            If bitLength <= 0 Then
                Throw New ArgumentException($"the bit length must be a positive integer, but {bitLength}!")
            End If

            Return New ULong(WordCount(bitLength) - 1) {}
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub SetBit(words As ULong(), index As Integer)
            words(index \ 64) = words(index \ 64) Or (1UL << (index Mod 64))
        End Sub

        ''' <summary>
        ''' 把最后一个机器字中超出位长的尾部比特清零。
        ''' </summary>
        Private Shared Sub MaskTail(words As ULong(), bitLength As Integer)
            Dim used As Integer = bitLength Mod 64

            If used = 0 Then
                Return
            End If

            Dim mask As ULong = (1UL << used) - 1UL

            words(words.Length - 1) = words(words.Length - 1) And mask
        End Sub
    End Class
End Namespace
