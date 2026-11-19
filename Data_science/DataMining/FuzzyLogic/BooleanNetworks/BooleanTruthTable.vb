Option Strict On
Option Explicit On

Imports System
Imports System.Collections
Imports System.Text

Namespace BooleanNetworks

    ''' <summary>
    ''' 以位掩码存储的布尔真值表。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <c>k</c> 个父节点对应 <c>2^k</c> 个输入组合，每个组合一位。输入布尔向量按小端解释：
    ''' <c>inputs(0)</c> 是索引的最低位，<c>inputs(k-1)</c> 是最高位。
    ''' </para>
    ''' <para>
    ''' 父节点数不超过 <see cref="MaxMaskParents"/> 时用 <c>ULong()</c> 位掩码，单步求值 O(1)；
    ''' 更大时回退 <see cref="BitArray"/>。上界 <see cref="MaxBitArrayParents"/> 用于阻止组合爆炸。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class BooleanTruthTable
        Implements IBooleanRule

        ''' <summary>位掩码路径允许的最大父节点数（2^20 位 = 128 KiB）。</summary>
        Public Const MaxMaskParents As Integer = 20
        ''' <summary>BitArray 回退路径允许的最大父节点数。</summary>
        Public Const MaxBitArrayParents As Integer = 26

        Private ReadOnly _inputCount As Integer
        Private ReadOnly _mask As ULong()
        Private ReadOnly _bits As BitArray

        ''' <summary>创建所有输出均为假的真值表。</summary>
        Public Sub New(inputCount As Integer)
            Call ValidateInputCount(inputCount)

            _inputCount = inputCount

            If inputCount <= MaxMaskParents Then
                Dim length As Integer = 1 << inputCount
                _mask = New ULong((length + 63) \ 64 - 1) {}
                _bits = Nothing
            Else
                _mask = Nothing
                _bits = New BitArray(1 << inputCount, False)
            End If
        End Sub

        ''' <summary>由显式的布尔数组创建真值表。</summary>
        Public Sub New(inputCount As Integer, values As Boolean())
            Call Me.New(inputCount)

            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If
            If values.Length <> 1 << inputCount Then
                Throw New ArgumentException($"truth table must have {1 << inputCount} entries, got {values.Length}", NameOf(values))
            End If

            For i As Integer = 0 To values.Length - 1
                Call SetValue(i, values(i))
            Next
        End Sub

        ''' <summary>由位掩码数组创建真值表（仅允许位掩码路径）。</summary>
        Public Sub New(inputCount As Integer, mask As ULong())
            Call ValidateInputCount(inputCount)

            If mask Is Nothing Then
                Throw New ArgumentNullException(NameOf(mask))
            End If
            Dim words As Integer = ((1 << inputCount) + 63) \ 64

            If mask.Length <> words Then
                Throw New ArgumentException($"mask must have {words} ULong word(s), got {mask.Length}", NameOf(mask))
            End If

            _inputCount = inputCount
            _mask = DirectCast(mask.Clone(), ULong())
            _bits = Nothing
        End Sub

        Private Shared Sub ValidateInputCount(inputCount As Integer)
            If inputCount < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(inputCount), inputCount, "input count cannot be negative")
            End If
            If inputCount > MaxBitArrayParents Then
                Throw New ArgumentOutOfRangeException(NameOf(inputCount), inputCount,
                                                      $"more than {MaxBitArrayParents} parents would explode the truth table")
            End If
        End Sub

        ''' <summary>输入个数。</summary>
        Public ReadOnly Property InputCount As Integer Implements IBooleanRule.InputCount
            Get
                Return _inputCount
            End Get
        End Property

        ''' <summary>规则类型。</summary>
        Public ReadOnly Property Kind As BooleanRuleKind Implements IBooleanRule.Kind
            Get
                Return BooleanRuleKind.TruthTable
            End Get
        End Property

        ''' <summary>读取指定输入组合的输出。</summary>
        Public Function GetValue(index As Integer) As Boolean
            If index < 0 OrElse index >= 1 << _inputCount Then
                Throw New ArgumentOutOfRangeException(NameOf(index), index, "index outside truth table")
            End If

            If _mask IsNot Nothing Then
                Dim word As Integer = index >> 6
                Dim bit As Integer = index And 63

                Return ((_mask(word) >> bit) And 1UL) <> 0UL
            Else
                Return _bits(index)
            End If
        End Function

        ''' <summary>写入指定输入组合的输出。</summary>
        Public Sub SetValue(index As Integer, value As Boolean)
            If index < 0 OrElse index >= 1 << _inputCount Then
                Throw New ArgumentOutOfRangeException(NameOf(index), index, "index outside truth table")
            End If

            If _mask IsNot Nothing Then
                Dim word As Integer = index >> 6
                Dim bit As Integer = index And 63
                Dim flag As ULong = 1UL << bit

                If value Then
                    _mask(word) = _mask(word) Or flag
                Else
                    _mask(word) = _mask(word) And Not flag
                End If
            Else
                _bits(index) = value
            End If
        End Sub

        ''' <summary>把输入布尔向量解释为小端位模式并求值。</summary>
        Public Function Evaluate(inputs As Boolean()) As Boolean Implements IBooleanRule.Evaluate
            If inputs Is Nothing Then
                Throw New ArgumentNullException(NameOf(inputs))
            End If
            If inputs.Length <> _inputCount Then
                Throw New ArgumentException($"expected {_inputCount} inputs, got {inputs.Length}", NameOf(inputs))
            End If

            Dim index As Integer = 0

            For i As Integer = 0 To inputs.Length - 1
                If inputs(i) Then
                    index = index Or (1 << i)
                End If
            Next

            Return GetValue(index)
        End Function

        ''' <summary>导出现式真值表。</summary>
        Public Function ToTruthTable() As Boolean() Implements IBooleanRule.ToTruthTable
            Dim values((1 << _inputCount) - 1) As Boolean

            For i As Integer = 0 To values.Length - 1
                values(i) = GetValue(i)
            Next

            Return values
        End Function

        ''' <summary>按位与：两个规则都为真时输出真。</summary>
        Public Function AndWith(other As BooleanTruthTable) As BooleanTruthTable
            Call RequireSameShape(other)

            Dim result As New BooleanTruthTable(_inputCount)

            For i As Integer = 0 To (1 << _inputCount) - 1
                Call result.SetValue(i, GetValue(i) AndAlso other.GetValue(i))
            Next

            Return result
        End Function

        ''' <summary>按位或：任一规则为真时输出真。</summary>
        Public Function OrWith(other As BooleanTruthTable) As BooleanTruthTable
            Call RequireSameShape(other)

            Dim result As New BooleanTruthTable(_inputCount)

            For i As Integer = 0 To (1 << _inputCount) - 1
                Call result.SetValue(i, GetValue(i) OrElse other.GetValue(i))
            Next

            Return result
        End Function

        ''' <summary>输出取反。</summary>
        Public Function Negate() As BooleanTruthTable
            Dim result As New BooleanTruthTable(_inputCount)

            For i As Integer = 0 To (1 << _inputCount) - 1
                Call result.SetValue(i, Not GetValue(i))
            Next

            Return result
        End Function

        Private Sub RequireSameShape(other As BooleanTruthTable)
            If other Is Nothing Then
                Throw New ArgumentNullException(NameOf(other))
            End If
            If other._inputCount <> _inputCount Then
                Throw New ArgumentException("truth tables must have the same input count", NameOf(other))
            End If
        End Sub

        ''' <summary>创建与门真值表。</summary>
        Public Shared Function CreateAnd(inputCount As Integer) As BooleanTruthTable
            If inputCount < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(inputCount), inputCount, "AND gate needs at least one input")
            End If

            Dim table As New BooleanTruthTable(inputCount)

            Call table.SetValue((1 << inputCount) - 1, True)

            Return table
        End Function

        ''' <summary>创建或门真值表。</summary>
        Public Shared Function CreateOr(inputCount As Integer) As BooleanTruthTable
            If inputCount < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(inputCount), inputCount, "OR gate needs at least one input")
            End If

            Dim table As New BooleanTruthTable(inputCount)

            For i As Integer = 1 To (1 << inputCount) - 1
                Call table.SetValue(i, True)
            Next

            Return table
        End Function

        ''' <summary>创建单输入非门真值表。</summary>
        Public Shared Function CreateNot() As BooleanTruthTable
            Dim table As New BooleanTruthTable(1)

            Call table.SetValue(0, True)

            Return table
        End Function

        ''' <summary>创建异或门真值表（真输入个数为奇数时输出真）。</summary>
        Public Shared Function CreateXor(inputCount As Integer) As BooleanTruthTable
            If inputCount < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(inputCount), inputCount, "XOR gate needs at least one input")
            End If

            Dim table As New BooleanTruthTable(inputCount)

            For i As Integer = 0 To (1 << inputCount) - 1
                Dim bits As Integer = i
                Dim parity As Integer = 0

                While bits <> 0
                    parity = parity Xor (bits And 1)
                    bits = bits >> 1
                End While

                If parity = 1 Then
                    Call table.SetValue(i, True)
                End If
            Next

            Return table
        End Function

        ''' <summary>由显式真值表数组创建，长度必须为 2^k。</summary>
        Public Shared Function FromValues(inputCount As Integer, ParamArray values As Boolean()) As BooleanTruthTable
            Return New BooleanTruthTable(inputCount, values)
        End Function

        ''' <summary>每行一个输入组合的紧凑文本表示。</summary>
        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder()

            For i As Integer = 0 To (1 << _inputCount) - 1
                If i > 0 Then
                    Call sb.Append(" ")
                End If
                Call sb.Append(If(GetValue(i), "1", "0"))
            Next

            Return sb.ToString()
        End Function
    End Class

End Namespace
