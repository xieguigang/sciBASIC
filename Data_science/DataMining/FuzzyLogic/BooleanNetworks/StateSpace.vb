Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Text

Namespace BooleanNetworks

    ''' <summary>布尔网络状态的编码、解码与轨迹分析工具。</summary>
    Public NotInheritable Class StateSpace

        Private Sub New()
        End Sub

        ''' <summary>把不超过 64 位的状态编码为无符号整数（第 i 位对应第 i 个基因）。</summary>
        Public Shared Function Encode(state As Boolean()) As ULong
            If state Is Nothing Then
                Throw New ArgumentNullException(NameOf(state))
            End If
            If state.Length > 64 Then
                Throw New ArgumentException("cannot encode more than 64 bits into ULong", NameOf(state))
            End If

            Dim code As ULong = 0UL

            For i As Integer = 0 To state.Length - 1
                If state(i) Then
                    code = code Or (1UL << i)
                End If
            Next

            Return code
        End Function

        ''' <summary>把无符号整数解码为指定长度的状态向量。</summary>
        Public Shared Function Decode(code As ULong, geneCount As Integer) As Boolean()
            If geneCount < 0 OrElse geneCount > 64 Then
                Throw New ArgumentOutOfRangeException(NameOf(geneCount), geneCount, "gene count must be in [0,64]")
            End If

            Dim state(geneCount - 1) As Boolean

            For i As Integer = 0 To geneCount - 1
                state(i) = ((code >> i) And 1UL) <> 0UL
            Next

            Return state
        End Function

        ''' <summary>生成任意长度状态的字符串键，用于吸引子检测。</summary>
        Public Shared Function Key(state As Boolean()) As String
            If state Is Nothing Then
                Throw New ArgumentNullException(NameOf(state))
            End If

            Dim sb As New StringBuilder(state.Length)

            For i As Integer = 0 To state.Length - 1
                Call sb.Append(If(state(i), "1"c, "0"c))
            Next

            Return sb.ToString()
        End Function

        ''' <summary>复制状态向量。</summary>
        Public Shared Function Copy(state As Boolean()) As Boolean()
            If state Is Nothing Then
                Throw New ArgumentNullException(NameOf(state))
            End If

            Return DirectCast(state.Clone(), Boolean())
        End Function
    End Class

    ''' <summary>逐步记录的状态轨迹，可分析吸引子与周期。</summary>
    Public NotInheritable Class StateTrajectory

        Private ReadOnly _states As New List(Of Boolean())

        ''' <summary>追加一个状态（自动复制）。</summary>
        Public Sub Append(state As Boolean())
            If state Is Nothing Then
                Throw New ArgumentNullException(NameOf(state))
            End If

            Call _states.Add(StateSpace.Copy(state))
        End Sub

        ''' <summary>已记录的状态数。</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return _states.Count
            End Get
        End Property

        ''' <summary>按步序读取状态。</summary>
        Public Default ReadOnly Property Item(stepIndex As Integer) As Boolean()
            Get
                Return StateSpace.Copy(_states(stepIndex))
            End Get
        End Property

        ''' <summary>所有状态的防御性副本。</summary>
        Public Function ToArray() As Boolean()()
            Return _states.ConvertAll(Function(s) StateSpace.Copy(s)).ToArray()
        End Function

        ''' <summary>
        ''' 检测轨迹中第一个重复状态所构成的吸引子。
        ''' </summary>
        ''' <returns>找到循环时返回吸引子信息，否则返回 <c>Nothing</c>。</returns>
        Public Function DetectAttractor() As AttractorResult
            Dim seen As New Dictionary(Of String, Integer)()

            For i As Integer = 0 To _states.Count - 1
                Dim key As String = StateSpace.Key(_states(i))
                Dim first As Integer = 0

                If seen.TryGetValue(key, first) Then
                    Dim cycleLength As Integer = i - first
                    Dim cycle(cycleLength - 1)() As Boolean

                    For j As Integer = 0 To cycleLength - 1
                        cycle(j) = StateSpace.Copy(_states(first + j))
                    Next

                    Return New AttractorResult(first, cycleLength, cycle)
                End If

                seen(key) = i
            Next

            Return Nothing
        End Function
    End Class

    ''' <summary>吸引子（不动点或极限环）的描述。</summary>
    Public NotInheritable Class AttractorResult

        ''' <summary>以首次出现步序、周期长度和循环状态构造结果。</summary>
        Public Sub New(startStep As Integer, period As Integer, cycleStates As Boolean()())
            If startStep < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(startStep))
            End If
            If period <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(period), period, "period must be positive")
            End If
            If cycleStates Is Nothing OrElse cycleStates.Length <> period Then
                Throw New ArgumentException("cycle state count must match period", NameOf(cycleStates))
            End If

            _startStep = startStep
            _period = period
            _cycleStates = DirectCast(cycleStates.Clone(), Boolean()())
        End Sub

        Private ReadOnly _startStep As Integer
        Private ReadOnly _period As Integer
        Private ReadOnly _cycleStates As Boolean()()

        ''' <summary>吸引子首次出现的步序。</summary>
        Public ReadOnly Property StartStep As Integer
            Get
                Return _startStep
            End Get
        End Property

        ''' <summary>吸引子周期；1 表示不动点。</summary>
        Public ReadOnly Property Period As Integer
            Get
                Return _period
            End Get
        End Property

        ''' <summary>是否为不动点。</summary>
        Public ReadOnly Property IsFixedPoint As Boolean
            Get
                Return _period = 1
            End Get
        End Property

        ''' <summary>构成循环的状态（防御性副本）。</summary>
        Public ReadOnly Property CycleStates As Boolean()()
            Get
                Return DirectCast(_cycleStates.Clone(), Boolean()())
            End Get
        End Property
    End Class

End Namespace
