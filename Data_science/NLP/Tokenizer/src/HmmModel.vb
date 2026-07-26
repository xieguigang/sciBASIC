Imports std = System.Math

Namespace ChineseTokenizer

    ''' <summary>
    ''' 基于 HMM（隐马尔可夫模型）的未登录词识别模块。
    ''' 采用 BMES 四态标注体系：
    ''' <list type="bullet">
    ''' <item><term>B</term><description>词首字符（Begin）</description></item>
    ''' <item><term>M</term><description>词中字符（Middle）</description></item>
    ''' <item><term>E</term><description>词尾字符（End）</description></item>
    ''' <item><term>S</term><description>单字成词（Single）</description></item>
    ''' </list>
    ''' 模型参数：
    ''' <list type="bullet">
    ''' <item><term>初始概率 π</term><description>句子首字符处于各状态的概率</description></item>
    ''' <item><term>转移概率 A</term><description>状态 i 到状态 j 的转移概率</description></item>
    ''' <item><term>发射概率 B</term><description>状态 i 下观测到字符 c 的概率</description></item>
    ''' </list>
    ''' 解码采用 Viterbi 算法，时间复杂度 O(n * S^2)，其中 n 为序列长度，S = 4。
    ''' </summary>
    Public NotInheritable Class HmmModel

        ' BMES 四个状态
        Public Const STATE_B As Integer = 0
        Public Const STATE_M As Integer = 1
        Public Const STATE_E As Integer = 2
        Public Const STATE_S As Integer = 3
        Private Const STATE_COUNT As Integer = 4

        Private Shared ReadOnly STATE_NAMES() As String = {"B", "M", "E", "S"}

        ' 模型参数（对数概率空间，避免下溢）
        Private _pi(STATE_COUNT - 1) As Double
        Private _trans(STATE_COUNT - 1, STATE_COUNT - 1) As Double
        Private _emit As New Dictionary(Of Char, Double())()
        Private _totalEmit(STATE_COUNT - 1) As Long

        ' 默认发射平滑值（对数空间，对应一个极小概率）
        Private Const DEFAULT_EMIT_LOG_PROB As Double = -15.0

        Public Sub New()
            InitializeDefaults()
        End Sub

        ''' <summary>
        ''' 使用经验值初始化 HMM 参数（适用于无训练语料的场景）。
        ''' 这些数值基于公开中文分词语料的统计规律。
        ''' </summary>
        Private Sub InitializeDefaults()
            ' 初始概率：句首极少为 M 或 E
            _pi(STATE_B) = std.Log(0.6)
            _pi(STATE_M) = std.Log(0.05)
            _pi(STATE_E) = std.Log(0.05)
            _pi(STATE_S) = std.Log(0.3)

            ' 转移概率矩阵（行=前状态，列=后状态）
            ' 合法转移：B->M, B->E, M->M, M->E, E->B, E->S, S->B, S->S
            _trans(STATE_B, STATE_M) = std.Log(0.15)
            _trans(STATE_B, STATE_E) = std.Log(0.85)
            _trans(STATE_M, STATE_M) = std.Log(0.15)
            _trans(STATE_M, STATE_E) = std.Log(0.85)
            _trans(STATE_E, STATE_B) = std.Log(0.55)
            _trans(STATE_E, STATE_S) = std.Log(0.45)
            _trans(STATE_S, STATE_B) = std.Log(0.55)
            _trans(STATE_S, STATE_S) = std.Log(0.45)
            ' 非法转移赋极小值
            For i As Integer = 0 To STATE_COUNT - 1
                For j As Integer = 0 To STATE_COUNT - 1
                    If _trans(i, j) = 0.0 Then _trans(i, j) = -100.0
                Next
            Next
        End Sub

        ''' <summary>
        ''' 从已分词语料训练 HMM 参数。语料格式：每行一句，词以空格分隔。
        ''' </summary>
        Public Sub Train(corpusPath As String, Optional encoding As System.Text.Encoding = Nothing)
            If Not IO.File.Exists(corpusPath) Then Throw New IO.FileNotFoundException("语料文件未找到", corpusPath)
            If encoding Is Nothing Then encoding = System.Text.Encoding.UTF8

            ' 计数器
            Dim piCount(STATE_COUNT - 1) As Long
            Dim transCount(STATE_COUNT - 1, STATE_COUNT - 1) As Long
            Dim emitCount As New Dictionary(Of Char, Long())()
            Dim stateTotal(STATE_COUNT - 1) As Long
            Dim piTotal As Long = 0

            Using reader As New IO.StreamReader(corpusPath, encoding)
                Dim line As String = reader.ReadLine()
                Do While line IsNot Nothing
                    If String.IsNullOrWhiteSpace(line) Then
                        line = reader.ReadLine()
                        Continue Do
                    End If
                    Dim words() As String = line.Split(New Char() {" "c, ControlChars.Tab}, StringSplitOptions.RemoveEmptyEntries)
                    If words.Length = 0 Then
                        line = reader.ReadLine()
                        Continue Do
                    End If

                    Dim prevState As Integer = -1
                    Dim isFirstChar As Boolean = True

                    For Each word As String In words
                        If String.IsNullOrEmpty(word) Then Continue For
                        For idx As Integer = 0 To word.Length - 1
                            Dim ch As Char = word(idx)
                            Dim st As Integer
                            If word.Length = 1 Then
                                st = STATE_S
                            ElseIf idx = 0 Then
                                st = STATE_B
                            ElseIf idx = word.Length - 1 Then
                                st = STATE_E
                            Else
                                st = STATE_M
                            End If

                            ' 初始概率
                            If isFirstChar Then
                                piCount(st) += 1
                                piTotal += 1
                                isFirstChar = False
                            End If

                            ' 转移概率
                            If prevState >= 0 Then
                                transCount(prevState, st) += 1
                            End If

                            ' 发射概率
                            Dim counts As Long() = Nothing
                            If Not emitCount.TryGetValue(ch, counts) Then
                                counts = New Long(STATE_COUNT - 1) {}
                                emitCount(ch) = counts
                            End If
                            counts(st) += 1
                            stateTotal(st) += 1

                            prevState = st
                        Next
                    Next

                    ' 读取下一行
                    line = reader.ReadLine()
                Loop
            End Using

            ' 转换为对数概率（带加 1 平滑）
            For i As Integer = 0 To STATE_COUNT - 1
                _pi(i) = If(piTotal > 0, std.Log((piCount(i) + 1.0) / (piTotal + STATE_COUNT)), std.Log(1.0 / STATE_COUNT))
            Next

            For i As Integer = 0 To STATE_COUNT - 1
                Dim rowSum As Long = 0
                For j As Integer = 0 To STATE_COUNT - 1
                    rowSum += transCount(i, j)
                Next
                For j As Integer = 0 To STATE_COUNT - 1
                    _trans(i, j) = If(rowSum > 0, std.Log((transCount(i, j) + 1.0) / (rowSum + STATE_COUNT)), -100.0)
                Next
            Next

            _emit.Clear()
            For Each kv As KeyValuePair(Of Char, Long()) In emitCount
                Dim probs(STATE_COUNT - 1) As Double
                For s As Integer = 0 To STATE_COUNT - 1
                    probs(s) = If(stateTotal(s) > 0, std.Log((kv.Value(s) + 1.0) / (stateTotal(s) + emitCount.Count)), DEFAULT_EMIT_LOG_PROB)
                Next
                _emit(kv.Key) = probs
            Next
        End Sub

        ''' <summary>
        ''' 使用 Viterbi 算法对给定字符序列进行解码，返回每个字符的 BMES 状态标签。
        ''' </summary>
        Public Function Decode(text As String) As List(Of String)
            Dim result As New List(Of String)()
            If String.IsNullOrEmpty(text) Then Return result

            Dim n As Integer = text.Length
            Dim viterbi(n - 1, STATE_COUNT - 1) As Double
            Dim backptr(n - 1, STATE_COUNT - 1) As Integer

            ' 初始化（t = 0）
            For s As Integer = 0 To STATE_COUNT - 1
                viterbi(0, s) = _pi(s) + GetEmitLogProb(text(0), s)
                backptr(0, s) = -1
            Next

            ' 递推
            For t As Integer = 1 To n - 1
                For s As Integer = 0 To STATE_COUNT - 1
                    Dim bestLogProb As Double = Double.NegativeInfinity
                    Dim bestPrev As Integer = 0
                    For p As Integer = 0 To STATE_COUNT - 1
                        Dim lp As Double = viterbi(t - 1, p) + _trans(p, s)
                        If lp > bestLogProb Then
                            bestLogProb = lp
                            bestPrev = p
                        End If
                    Next
                    viterbi(t, s) = bestLogProb + GetEmitLogProb(text(t), s)
                    backptr(t, s) = bestPrev
                Next
            Next

            ' 回溯
            Dim bestLast As Integer = 0
            Dim bestFinalLogProb As Double = Double.NegativeInfinity
            For s As Integer = 0 To STATE_COUNT - 1
                If viterbi(n - 1, s) > bestFinalLogProb Then
                    bestFinalLogProb = viterbi(n - 1, s)
                    bestLast = s
                End If
            Next

            Dim path(n - 1) As Integer
            path(n - 1) = bestLast
            For t As Integer = n - 2 To 0 Step -1
                path(t) = backptr(t + 1, path(t + 1))
            Next

            For t As Integer = 0 To n - 1
                result.Add(STATE_NAMES(path(t)))
            Next
            Return result
        End Function

        ''' <summary>
        ''' 根据 BMES 标签序列将字符序列切分为词。
        ''' </summary>
        Public Shared Function TagsToWords(text As String, tags As List(Of String)) As List(Of String)
            Dim words As New List(Of String)()
            If String.IsNullOrEmpty(text) OrElse tags Is Nothing OrElse tags.Count = 0 Then Return words

            Dim buf As New System.Text.StringBuilder()
            For i As Integer = 0 To std.Min(text.Length, tags.Count) - 1
                Dim tag As String = tags(i)
                buf.Append(text(i))
                Select Case tag
                    Case "S"
                        words.Add(buf.ToString())
                        buf.Clear()
                    Case "E"
                        words.Add(buf.ToString())
                        buf.Clear()
                    Case "B", "M"
                        ' 继续累积
                End Select
            Next
            If buf.Length > 0 Then words.Add(buf.ToString())
            Return words
        End Function

        ' 获取指定字符在指定状态下的发射对数概率（带平滑）
        Private Function GetEmitLogProb(ch As Char, state As Integer) As Double
            Dim probs As Double() = Nothing
            If _emit.TryGetValue(ch, probs) Then
                Return probs(state)
            End If
            Return DEFAULT_EMIT_LOG_PROB
        End Function

    End Class

End Namespace
