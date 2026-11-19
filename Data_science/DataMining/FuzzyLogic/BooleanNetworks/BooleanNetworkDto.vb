Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace BooleanNetworks

    ''' <summary>BN/PBN 节点的 JSON 传输对象。</summary>
    Public NotInheritable Class BooleanNodeDto
        ''' <summary>基因名。</summary>
        Public Property Name As String
        ''' <summary>父节点索引。</summary>
        Public Property Parents As Integer()
        ''' <summary>候选规则（十六进制位掩码或 01 串）。</summary>
        Public Property Rules As String()
        ''' <summary>规则类型（0=真值表）。</summary>
        Public Property Kinds As Integer()
        ''' <summary>选择概率。</summary>
        Public Property Probabilities As Double()

        ''' <summary>由节点构造 DTO。</summary>
        Public Shared Function FromModel(node As BooleanNode) As BooleanNodeDto
            If node Is Nothing Then
                Throw New ArgumentNullException(NameOf(node))
            End If

            Dim rules As IBooleanRule() = node.Rules
            Dim texts(rules.Length - 1) As String
            Dim kinds(rules.Length - 1) As Integer

            For i As Integer = 0 To rules.Length - 1
                texts(i) = InferenceHelper.TruthTableText(rules(i).ToTruthTable())
                kinds(i) = CInt(rules(i).Kind)
            Next

            Return New BooleanNodeDto With {
                .Name = node.Name,
                .Parents = node.Parents,
                .Rules = texts,
                .Kinds = kinds,
                .Probabilities = node.Probabilities
            }
        End Function

        ''' <summary>还原为节点。</summary>
        Public Function ToModel() As BooleanNode
            If String.IsNullOrWhiteSpace(Name) Then
                Throw New InvalidOperationException("node name is empty in DTO")
            End If
            If Parents Is Nothing OrElse Rules Is Nothing OrElse Probabilities Is Nothing Then
                Throw New InvalidOperationException("node DTO is incomplete")
            End If
            If Rules.Length <> Probabilities.Length Then
                Throw New InvalidOperationException("rule count does not match probability count")
            End If

            Dim functions(Rules.Length - 1) As IBooleanRule

            For i As Integer = 0 To Rules.Length - 1
                Dim values As Boolean() = ParseBits(Rules(i), 1 << Parents.Length)
                functions(i) = New BooleanTruthTable(Parents.Length, values)
            Next

            Return New BooleanNode(Name, Parents, functions, Probabilities)
        End Function

        Private Shared Function ParseBits(text As String, expected As Integer) As Boolean()
            If String.IsNullOrEmpty(text) Then
                Throw New InvalidOperationException("truth table text is empty")
            End If
            If text.Length <> expected Then
                Throw New InvalidOperationException($"truth table length must be {expected}, got {text.Length}")
            End If

            Dim values(expected - 1) As Boolean

            For i As Integer = 0 To expected - 1
                If text(i) = "1"c Then
                    values(i) = True
                ElseIf text(i) = "0"c Then
                    values(i) = False
                Else
                    Throw New InvalidOperationException($"invalid truth table character '{text(i)}'")
                End If
            Next

            Return values
        End Function
    End Class

    ''' <summary>BN/PBN 的 JSON 传输对象，含格式版本与严格校验。</summary>
    Public NotInheritable Class BooleanNetworkDto

        ''' <summary>当前格式版本。</summary>
        Public Const CurrentVersion As String = "1.0"

        ''' <summary>格式版本。</summary>
        Public Property FormatVersion As String = CurrentVersion
        ''' <summary>是否为概率网络。</summary>
        Public Property Probabilistic As Boolean
        ''' <summary>节点列表。</summary>
        Public Property Nodes As List(Of BooleanNodeDto)
        ''' <summary>初始状态（01 串）。</summary>
        Public Property InitialState As String
        ''' <summary>随机种子。</summary>
        Public Property Seed As Integer

        ''' <summary>由网络构造 DTO。</summary>
        Public Shared Function FromModel(network As BooleanNetworkBase) As BooleanNetworkDto
            If network Is Nothing Then
                Throw New ArgumentNullException(NameOf(network))
            End If

            Dim nodeDtos As New List(Of BooleanNodeDto)()

            For Each node As BooleanNode In network.Nodes
                Call nodeDtos.Add(BooleanNodeDto.FromModel(node))
            Next

            Return New BooleanNetworkDto With {
                .FormatVersion = CurrentVersion,
                .Probabilistic = TypeOf network Is ProbabilisticBooleanNetwork,
                .Nodes = nodeDtos,
                .InitialState = StateBits(network.State),
                .Seed = network.Seed
            }
        End Function

        ''' <summary>序列化为 JSON。</summary>
        Public Function ToJson(Optional indent As Boolean = False) As String
            Return JsonContract.GetJson(Of BooleanNetworkDto)(Me, indent)
        End Function

        ''' <summary>从 JSON 还原网络。</summary>
        Public Shared Function FromJson(json As String) As BooleanNetworkBase
            If String.IsNullOrWhiteSpace(json) Then
                Throw New ArgumentNullException(NameOf(json))
            End If

            Dim dto As BooleanNetworkDto = json.LoadJSON(Of BooleanNetworkDto)()

            Return dto.ToModel()
        End Function

        ''' <summary>还原为网络对象。</summary>
        Public Function ToModel() As BooleanNetworkBase
            If Nodes Is Nothing OrElse Nodes.Count = 0 Then
                Throw New InvalidOperationException("network DTO has no nodes")
            End If
            If Not String.IsNullOrEmpty(FormatVersion) AndAlso FormatVersion <> CurrentVersion Then
                Throw New NotSupportedException($"unsupported format version '{FormatVersion}'")
            End If

            Dim modelNodes(Nodes.Count - 1) As BooleanNode

            For i As Integer = 0 To Nodes.Count - 1
                modelNodes(i) = Nodes(i).ToModel()
            Next

            Dim state As Boolean() = ParseState(InitialState, modelNodes.Length)

            If Probabilistic Then
                Return New ProbabilisticBooleanNetwork(modelNodes, state, Seed)
            Else
                Return New BooleanNetwork(modelNodes, state)
            End If
        End Function

        Private Shared Function StateBits(state As Boolean()) As String
            Dim sb As New StringBuilder(state.Length)

            For Each value As Boolean In state
                Call sb.Append(If(value, "1"c, "0"c))
            Next

            Return sb.ToString()
        End Function

        Private Shared Function ParseState(text As String, length As Integer) As Boolean()
            If String.IsNullOrEmpty(text) OrElse text.Length <> length Then
                Throw New InvalidOperationException($"initial state must have {length} bits")
            End If

            Dim state(length - 1) As Boolean

            For i As Integer = 0 To length - 1
                If text(i) = "1"c Then
                    state(i) = True
                ElseIf text(i) <> "0"c Then
                    Throw New InvalidOperationException($"invalid initial state character '{text(i)}'")
                End If
            Next

            Return state
        End Function
    End Class

End Namespace
