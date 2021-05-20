Imports Microsoft.VisualBasic.Data.NLP.Word2Vec.utils
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace NlpVec

    ''' <summary>
    ''' Created by fangy on 13-12-17.
    ''' 词神经元
    ''' </summary>
    Public Class WordNeuron : Inherits HuffmanNeuron

        Public Property name As String

        Public ReadOnly Property pathNeurons As IList(Of HuffmanNode)
            Get

                If m_pathNeurons IsNot Nothing Then
                    Return m_pathNeurons
                End If

                m_pathNeurons = HuffmanTree.getPath(Me)
                Return m_pathNeurons
            End Get
        End Property

        Dim m_pathNeurons As IList(Of HuffmanNode)

        Public Sub New(name As String, freq As Integer, vectorSize As Integer)
            MyBase.New(freq, vectorSize)

            Me.name = name

            For i = 0 To vector.Length - 1
                vector(i) = (randf.seeds.NextDouble - 0.5) / vectorSize
            Next
        End Sub
    End Class
End Namespace
