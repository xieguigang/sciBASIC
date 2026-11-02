Imports std = System.Math

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 离散因子(潜势函数)：一张定义在一组离散随机变量之上的表。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 因子是变量消除、junction tree 与 loopy belief propagation 这三种推断算法的公共代数结构。
    ''' 表内的数值始终以 <b>对数</b> 形式保存(见 <see cref="NumericLogSpace"/>)，因此
    ''' 「因子相乘」在对数空间之中就是加法，「求和消元」则是 log-sum-exp。
    ''' </para>
    ''' <para>
    ''' 索引约定：<see cref="Variables"/> 按照升序排列，扁平下标为
    ''' <c>Σ value(k) · Π_(m&lt;k) Cardinalities(m)</c>，即第一个变量的变化最快。
    ''' </para>
    ''' </remarks>
    Public Class Factor

        Private ReadOnly _variables As Integer()
        Private ReadOnly _cardinalities As Integer()
        Private ReadOnly _strides As Integer()
        Private ReadOnly _values As Double()

        ''' <summary>
        ''' 构建一个新的因子
        ''' </summary>
        ''' <param name="variables">因子定义域上的变量下标，必须各不相同</param>
        ''' <param name="cardinalities">与 <paramref name="variables"/> 对齐的取值个数</param>
        ''' <param name="values">扁平化的对数潜势值，长度必须等于各取值个数的乘积</param>
        Public Sub New(variables As Integer(), cardinalities As Integer(), values As Double())
            If variables Is Nothing OrElse cardinalities Is Nothing OrElse values Is Nothing Then
                Throw New ArgumentNullException(NameOf(variables), "the variables, cardinalities and values of a factor can not be null")
            End If
            If variables.Length <> cardinalities.Length Then
                Throw New ArgumentException("the variables and cardinalities arrays must have the same length")
            End If

            Dim size As Integer = 1

            For i As Integer = 0 To cardinalities.Length - 1
                If cardinalities(i) < 2 Then
                    Throw New ArgumentOutOfRangeException(NameOf(cardinalities), $"the cardinality of variable {variables(i)} must be at least 2")
                End If

                size *= cardinalities(i)
            Next

            If values.Length <> size Then
                Throw New ArgumentException($"expected {size} values but got {values.Length}", NameOf(values))
            End If

            _variables = DirectCast(variables.Clone(), Integer())
            _cardinalities = DirectCast(cardinalities.Clone(), Integer())
            _values = DirectCast(values.Clone(), Double())
            _strides = ComputeStrides(_cardinalities)
        End Sub

        ''' <summary>
        ''' 因子定义域上的变量下标(升序)
        ''' </summary>
        Public ReadOnly Property Variables As Integer()
            Get
                Return _variables
            End Get
        End Property

        ''' <summary>
        ''' 与 <see cref="Variables"/> 对齐的取值个数
        ''' </summary>
        Public ReadOnly Property Cardinalities As Integer()
            Get
                Return _cardinalities
            End Get
        End Property

        ''' <summary>
        ''' 扁平化之后的对数潜势值
        ''' </summary>
        Public ReadOnly Property Values As Double()
            Get
                Return _values
            End Get
        End Property

        ''' <summary>
        ''' 表的元素个数
        ''' </summary>
        Public ReadOnly Property Size As Integer
            Get
                Return _values.Length
            End Get
        End Property

        ''' <summary>
        ''' 因子定义域之中变量的个数
        ''' </summary>
        Public ReadOnly Property Rank As Integer
            Get
                Return _variables.Length
            End Get
        End Property

        ''' <summary>
        ''' 判断某个变量是否属于本因子的定义域
        ''' </summary>
        Public Function Contains(variable As Integer) As Boolean
            Return Array.IndexOf(_variables, variable) >= 0
        End Function

        ''' <summary>
        ''' 判断本因子的定义域是否是另一个因子定义域的子集
        ''' </summary>
        Public Function IsSubsetOf(other As Factor) As Boolean
            For Each v As Integer In _variables
                If Not other.Contains(v) Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 读取一组赋值所对应的对数潜势值
        ''' </summary>
        ''' <param name="assignment">按照 <see cref="Variables"/> 顺序排列的取值</param>
        Public Function LogValue(assignment As Integer()) As Double
            Return _values(Encode(assignment, _strides))
        End Function

        ''' <summary>
        ''' 固定一个变量的取值，返回降低了秩的新因子
        ''' </summary>
        Public Function Restrict(variable As Integer, value As Integer) As Factor
            Dim position As Integer = Array.IndexOf(_variables, variable)

            If position < 0 Then
                Return Me
            End If

            Dim remaining As Integer() = _variables.Where(Function(v) v <> variable).ToArray()
            Dim cards As Integer() = RemainingCards(remaining)
            Dim newStride As Integer() = ComputeStrides(cards)
            Dim size As Integer = If(cards.Length = 0, 1, newStride(cards.Length - 1) * cards(cards.Length - 1))
            Dim values As Double() = New Double(size - 1) {}
            Dim codes As Integer() = New Integer(remaining.Length - 1) {}

            For idx As Integer = 0 To size - 1
                Call Decode(idx, cards, newStride, codes)

                ' 把剩余变量的取值填回原来的秩
                Dim full As Integer() = New Integer(_variables.Length - 1) {}

                For k As Integer = 0 To remaining.Length - 1
                    full(Array.IndexOf(_variables, remaining(k))) = codes(k)
                Next

                full(position) = value
                values(idx) = _values(Encode(full, _strides))
            Next

            Return New Factor(remaining, cards, values)
        End Function

        ''' <summary>
        ''' 两个因子相乘(在对数空间之中是逐元素相加)
        ''' </summary>
        Public Function Multiply(other As Factor) As Factor
            Dim vars As Integer() = UnionVariables(_variables, other._variables)
            Dim cards As Integer() = CardsOf(vars, other)
            Dim strides As Integer() = ComputeStrides(cards)
            Dim size As Integer = If(cards.Length = 0, 1, strides(cards.Length - 1) * cards(cards.Length - 1))
            Dim values As Double() = New Double(size - 1) {}
            Dim codes As Integer() = New Integer(vars.Length - 1) {}
            Dim positionsSelf As Integer() = Positions(vars, _variables)
            Dim positionsOther As Integer() = Positions(vars, other._variables)
            Dim bufferSelf As Integer() = New Integer(_variables.Length - 1) {}
            Dim bufferOther As Integer() = New Integer(other._variables.Length - 1) {}

            For idx As Integer = 0 To size - 1
                Call Decode(idx, cards, strides, codes)

                For k As Integer = 0 To bufferSelf.Length - 1
                    bufferSelf(k) = codes(positionsSelf(k))
                Next
                For k As Integer = 0 To bufferOther.Length - 1
                    bufferOther(k) = codes(positionsOther(k))
                Next

                values(idx) = LogMultiply(_values(Encode(bufferSelf, _strides)), other._values(Encode(bufferOther, other._strides)))
            Next

            Return New Factor(vars, cards, values)
        End Function

        ''' <summary>
        ''' 对一个变量求和消元
        ''' </summary>
        Public Function SumOut(variable As Integer) As Factor
            Return Eliminate(variable, sum:=True)
        End Function

        ''' <summary>
        ''' 对一个变量取最大值消元(用于 MAP 查询)
        ''' </summary>
        Public Function MaxOut(variable As Integer) As Factor
            Return Eliminate(variable, sum:=False)
        End Function

        ''' <summary>
        ''' 依次消去不在目标变量集合之中的变量，得到目标变量上的边缘因子
        ''' </summary>
        Public Function MarginalizeTo(targets As Integer()) As Factor
            Dim current As Factor = Me

            For Each v As Integer In _variables
                If Array.IndexOf(targets, v) < 0 Then
                    current = current.SumOut(v)
                End If
            Next

            Return current
        End Function

        ''' <summary>
        ''' 归一化为概率分布，返回与 <see cref="Values"/> 等长的线性值数组
        ''' </summary>
        Public Function ToProbabilities() As Double()
            Return NumericLogSpace.NormalizeLog(_values)
        End Function

        ''' <summary>
        ''' 本因子所代表的(未归一化)概率质量的对数值，即 <c>ln Σ e^(values)</c>
        ''' </summary>
        Public Function LogMass() As Double
            Return NumericLogSpace.LogSumExp(_values)
        End Function

        Private Function Eliminate(variable As Integer, sum As Boolean) As Factor
            Dim position As Integer = Array.IndexOf(_variables, variable)

            If position < 0 Then
                Return Me
            End If

            Dim remaining As Integer() = _variables.Where(Function(v) v <> variable).ToArray()
            Dim cards As Integer() = RemainingCards(remaining)
            Dim newStride As Integer() = ComputeStrides(cards)
            Dim size As Integer = If(cards.Length = 0, 1, newStride(cards.Length - 1) * cards(cards.Length - 1))
            Dim values As Double() = New Double(size - 1) {}
            Dim codes As Integer() = New Integer(remaining.Length - 1) {}
            Dim full As Integer() = New Integer(_variables.Length - 1) {}
            Dim range As Integer = _cardinalities(position)
            Dim buffer As Double() = New Double(range - 1) {}

            For idx As Integer = 0 To size - 1
                Call Decode(idx, cards, newStride, codes)

                For k As Integer = 0 To remaining.Length - 1
                    full(Array.IndexOf(_variables, remaining(k))) = codes(k)
                Next

                For v As Integer = 0 To range - 1
                    full(position) = v
                    buffer(v) = _values(Encode(full, _strides))
                Next

                If sum Then
                    values(idx) = NumericLogSpace.LogSumExp(buffer)
                Else
                    Dim max As Double = NumericLogSpace.LogZero

                    For Each b As Double In buffer
                        If b > max Then
                            max = b
                        End If
                    Next

                    values(idx) = max
                End If
            Next

            Return New Factor(remaining, cards, values)
        End Function

        Private Function RemainingCards(remaining As Integer()) As Integer()
            Dim cards As Integer() = New Integer(remaining.Length - 1) {}

            For k As Integer = 0 To remaining.Length - 1
                cards(k) = _cardinalities(Array.IndexOf(_variables, remaining(k)))
            Next

            Return cards
        End Function

        Private Function CardsOf(vars As Integer(), other As Factor) As Integer()
            Dim cards As Integer() = New Integer(vars.Length - 1) {}

            For k As Integer = 0 To vars.Length - 1
                Dim position As Integer = Array.IndexOf(_variables, vars(k))

                If position >= 0 Then
                    cards(k) = _cardinalities(position)
                Else
                    position = Array.IndexOf(other._variables, vars(k))
                    cards(k) = other._cardinalities(position)
                End If
            Next

            Return cards
        End Function

#Region "Shared helpers"

        ''' <summary>
        ''' 计算混合进制展开的步长
        ''' </summary>
        Public Shared Function ComputeStrides(cardinalities As Integer()) As Integer()
            Dim strides As Integer() = New Integer(cardinalities.Length - 1) {}
            Dim stride As Integer = 1

            For i As Integer = 0 To cardinalities.Length - 1
                strides(i) = stride
                stride *= cardinalities(i)
            Next

            Return strides
        End Function

        ''' <summary>
        ''' 把一组取值编码为扁平下标
        ''' </summary>
        Public Shared Function Encode(assignment As Integer(), strides As Integer()) As Integer
            Dim index As Integer = 0

            For i As Integer = 0 To strides.Length - 1
                index += assignment(i) * strides(i)
            Next

            Return index
        End Function

        ''' <summary>
        ''' 把扁平下标解码为一组取值
        ''' </summary>
        Public Shared Sub Decode(index As Integer, cardinalities As Integer(), strides As Integer(), ByRef codes As Integer())
            For i As Integer = 0 To cardinalities.Length - 1
                codes(i) = (index \ strides(i)) Mod cardinalities(i)
            Next
        End Sub

        ''' <summary>
        ''' 求两个有序变量集合的并集(保持升序)
        ''' </summary>
        Public Shared Function UnionVariables(a As Integer(), b As Integer()) As Integer()
            Dim union As New SortedSet(Of Integer)

            For Each v As Integer In a
                Call union.Add(v)
            Next
            For Each v As Integer In b
                Call union.Add(v)
            Next

            Return union.ToArray()
        End Function

        Private Shared Function Positions(vars As Integer(), subset As Integer()) As Integer()
            Dim slots As Integer() = New Integer(subset.Length - 1) {}

            For k As Integer = 0 To subset.Length - 1
                slots(k) = Array.IndexOf(vars, subset(k))
            Next

            Return slots
        End Function

        ''' <summary>
        ''' 对数空间之中的乘法：任意一个因子为 0(对数负无穷)的时候结果为 0
        ''' </summary>
        Public Shared Function LogMultiply(a As Double, b As Double) As Double
            If Double.IsNegativeInfinity(a) OrElse Double.IsNegativeInfinity(b) Then
                Return Double.NegativeInfinity
            Else
                Return a + b
            End If
        End Function

#End Region
    End Class
End Namespace
