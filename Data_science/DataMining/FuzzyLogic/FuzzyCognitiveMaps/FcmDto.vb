Option Strict On
Option Explicit On

Imports System
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FuzzyCognitiveMaps

    ''' <summary>FCM 概念节点的 JSON 传输对象。</summary>
    Public NotInheritable Class FcmConceptDto
        ''' <summary>概念名称。</summary>
        Public Property Name As String
        ''' <summary>初始激活值。</summary>
        Public Property InitialValue As Double

        ''' <summary>由概念构造 DTO。</summary>
        Public Shared Function FromModel(concept As FcmConcept) As FcmConceptDto
            If concept Is Nothing Then
                Throw New ArgumentNullException(NameOf(concept))
            End If

            Return New FcmConceptDto With {
                .Name = concept.Name,
                .InitialValue = concept.InitialValue
            }
        End Function

        ''' <summary>还原为概念。</summary>
        Public Function ToModel() As FcmConcept
            Return New FcmConcept(Name, InitialValue)
        End Function
    End Class

    ''' <summary>FCM 的 JSON 传输对象，含格式版本与方阵校验。</summary>
    Public NotInheritable Class FcmDto

        ''' <summary>当前格式版本。</summary>
        Public Const CurrentVersion As String = "1.0"
        ''' <summary>激活函数类型（sigmoid / binary / clamped）。</summary>
        Public Const FunctionSigmoid As String = "sigmoid"
        ''' <summary>二值激活函数标识。</summary>
        Public Const FunctionBinary As String = "binary"
        ''' <summary>裁剪线性激活函数标识。</summary>
        Public Const FunctionClamped As String = "clamped"

        ''' <summary>格式版本。</summary>
        Public Property FormatVersion As String = CurrentVersion
        ''' <summary>概念列表。</summary>
        Public Property Concepts As System.Collections.Generic.List(Of FcmConceptDto)
        ''' <summary>权重矩阵（按行展开）。</summary>
        Public Property Weights As Double(,)
        ''' <summary>激活函数类型。</summary>
        Public Property ActivationKind As String = FunctionSigmoid
        ''' <summary>Sigmoid 斜率。</summary>
        Public Property SigmoidAlpha As Double = 1.0
        ''' <summary>二值阈值。</summary>
        Public Property BinaryThreshold As Double = 0.5
        ''' <summary>裁剪下界。</summary>
        Public Property LowerBound As Double = 0.0
        ''' <summary>裁剪上界。</summary>
        Public Property UpperBound As Double = 1.0
        ''' <summary>是否包含自环。</summary>
        Public Property IncludeSelfLoop As Boolean

        ''' <summary>由 FCM 构造 DTO。</summary>
        Public Shared Function FromModel(map As FuzzyCognitiveMap) As FcmDto
            If map Is Nothing Then
                Throw New ArgumentNullException(NameOf(map))
            End If

            Dim conceptDtos As New System.Collections.Generic.List(Of FcmConceptDto)()

            For Each concept As FcmConcept In map.Concepts
                Call conceptDtos.Add(FcmConceptDto.FromModel(concept))
            Next

            Dim weights(map.Count - 1, map.Count - 1) As Double

            For i As Integer = 0 To map.Count - 1
                For j As Integer = 0 To map.Count - 1
                    weights(i, j) = map.GetWeight(i, j)
                Next
            Next

            Dim dto As New FcmDto With {
                .FormatVersion = CurrentVersion,
                .Concepts = conceptDtos,
                .Weights = weights,
                .IncludeSelfLoop = map.IncludeSelfLoop
            }

            Dim activation As IActivationFunction = map.ActivationFunction

            If TypeOf activation Is SigmoidActivation Then
                dto.ActivationKind = FunctionSigmoid
                dto.SigmoidAlpha = DirectCast(activation, SigmoidActivation).Alpha
            ElseIf TypeOf activation Is BinaryActivation Then
                dto.ActivationKind = FunctionBinary
                dto.BinaryThreshold = DirectCast(activation, BinaryActivation).Threshold
                dto.LowerBound = activation.LowerBound
                dto.UpperBound = activation.UpperBound
            Else
                dto.ActivationKind = FunctionClamped
                dto.LowerBound = activation.LowerBound
                dto.UpperBound = activation.UpperBound
            End If

            Return dto
        End Function

        ''' <summary>序列化为 JSON。</summary>
        Public Function ToJson(Optional indent As Boolean = False) As String
            Return JsonContract.GetJson(Of FcmDto)(Me, indent)
        End Function

        ''' <summary>从 JSON 还原 FCM。</summary>
        Public Shared Function FromJson(json As String) As FuzzyCognitiveMap
            If String.IsNullOrWhiteSpace(json) Then
                Throw New ArgumentNullException(NameOf(json))
            End If

            Dim dto As FcmDto = json.LoadJSON(Of FcmDto)()

            Return dto.ToModel()
        End Function

        ''' <summary>还原为 FCM 对象。</summary>
        Public Function ToModel() As FuzzyCognitiveMap
            If Concepts Is Nothing OrElse Concepts.Count = 0 Then
                Throw New InvalidOperationException("FCM DTO has no concepts")
            End If
            If Weights Is Nothing Then
                Throw New InvalidOperationException("FCM DTO has no weight matrix")
            End If
            If Weights.GetLength(0) <> Concepts.Count OrElse Weights.GetLength(1) <> Concepts.Count Then
                Throw New InvalidOperationException("weight matrix must be square with concept count")
            End If
            If Not String.IsNullOrEmpty(FormatVersion) AndAlso FormatVersion <> CurrentVersion Then
                Throw New NotSupportedException($"unsupported format version '{FormatVersion}'")
            End If

            Dim modelConcepts(Concepts.Count - 1) As FcmConcept

            For i As Integer = 0 To Concepts.Count - 1
                modelConcepts(i) = Concepts(i).ToModel()
            Next

            Dim activation As IActivationFunction

            Select Case ActivationKind
                Case FunctionSigmoid
                    activation = New SigmoidActivation(SigmoidAlpha)
                Case FunctionBinary
                    activation = New BinaryActivation(BinaryThreshold, LowerBound, UpperBound)
                Case FunctionClamped
                    activation = New ClampedLinearActivation(LowerBound, UpperBound)
                Case Else
                    Throw New NotSupportedException($"unknown activation function '{ActivationKind}'")
            End Select

            Return New FuzzyCognitiveMap(modelConcepts, Weights, activation, IncludeSelfLoop)
        End Function
    End Class

End Namespace
