Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

Namespace FuzzyCognitiveMaps

    ''' <summary>FCM 中的概念节点（基因、表型或调控因子）。</summary>
    Public NotInheritable Class FcmConcept

        Private ReadOnly _name As String
        Private _initialValue As Double
        Private ReadOnly _incoming As New Dictionary(Of Integer, Double)()

        ''' <summary>以名称与初始激活值创建概念节点。</summary>
        Public Sub New(name As String, Optional initialValue As Double = 0.0)
            If String.IsNullOrWhiteSpace(name) Then
                Throw New ArgumentException("concept name cannot be empty", NameOf(name))
            End If

            _name = name
            _initialValue = initialValue
        End Sub

        ''' <summary>概念名称。</summary>
        Public ReadOnly Property Name As String
            Get
                Return _name
            End Get
        End Property

        ''' <summary>初始激活值。</summary>
        Public Property InitialValue As Double
            Get
                Return _initialValue
            End Get
            Set(value As Double)
                If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                    Throw New ArgumentException("initial value must be finite", NameOf(value))
                End If

                _initialValue = value
            End Set
        End Property

        ''' <summary>入边（源节点索引 → 权重）的只读视图。</summary>
        Public ReadOnly Property IncomingWeights As IReadOnlyDictionary(Of Integer, Double)
            Get
                Return _incoming
            End Get
        End Property

        Friend Sub ClearIncoming()
            _incoming.Clear()
        End Sub

        Friend Sub SetIncoming(sourceIndex As Integer, weight As Double)
            _incoming(sourceIndex) = weight
        End Sub

        ''' <summary>文本表示。</summary>
        Public Overrides Function ToString() As String
            Return $"{_name} (initial={_initialValue})"
        End Function
    End Class

End Namespace
