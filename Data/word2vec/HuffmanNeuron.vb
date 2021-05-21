Imports Microsoft.VisualBasic.Data.NLP.Word2Vec.utils

Namespace NlpVec

    ''' <summary>
    ''' Created by fangy on 13-12-20.
    ''' </summary>
    Public Class HuffmanNeuron
        Implements HuffmanNode

        Protected Friend parentNeuron As HuffmanNode
        Protected Friend code As Integer = 0
        Protected Friend vector As Double()

        Public Sub SetCode(value As Integer) Implements HuffmanNode.SetCode
            code = value
        End Sub

        Public Property frequency As Integer Implements HuffmanNode.frequency

        Public Property parent As HuffmanNode Implements HuffmanNode.parent
            Set(value As HuffmanNode)
                parentNeuron = value
            End Set
            Get
                Return parentNeuron
            End Get
        End Property

        Public Function merge(right As HuffmanNode) As HuffmanNode Implements HuffmanNode.merge
            Dim parent As HuffmanNode = New HuffmanNeuron(frequency + right.frequency, vector.Length)
            parentNeuron = parent
            SetCode(0)
            right.parent = parent
            right.SetCode(1)
            Return parent
        End Function

        Public Function compareTo(hn As HuffmanNode) As Integer Implements IComparable(Of HuffmanNode).CompareTo
            If frequency = hn.frequency Then
                Return 0
            ElseIf frequency > hn.frequency Then
                Return 1
            Else
                Return -1
            End If
        End Function

        Public Sub New(freq As Integer, vectorSize As Integer)
            frequency = freq
            vector = New Double(vectorSize - 1) {}
            parentNeuron = Nothing
        End Sub
    End Class
End Namespace
