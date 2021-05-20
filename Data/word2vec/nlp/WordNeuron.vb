Imports Microsoft.VisualBasic.Data.NLP.Word2Vec.utils

Namespace NlpVec

    ''' <summary>
    ''' Created by fangy on 13-12-17.
    ''' 词神经元
    ''' </summary>
    Public Class WordNeuron
        Inherits HuffmanNeuron

        Private name_Renamed As String
        Private pathNeurons_Renamed As IList(Of HuffmanNode)

        Public Sub New(name As String, freq As Integer, vectorSize As Integer)
            MyBase.New(freq, vectorSize)
            name_Renamed = name
            Dim random As Random = New Random()

            For i = 0 To vector.Length - 1
                vector(i) = (random.NextDouble() - 0.5) / vectorSize
            Next
        End Sub

        Public Property name As String
            Get
                Return name_Renamed
            End Get
            Set(value As String)
                name_Renamed = value
            End Set
        End Property

        Public ReadOnly Property pathNeurons As IList(Of HuffmanNode)
            Get

                If pathNeurons_Renamed IsNot Nothing Then
                    Return pathNeurons_Renamed
                End If

                pathNeurons_Renamed = HuffmanTree.getPath(Me)
                Return pathNeurons_Renamed
            End Get
        End Property
    End Class
End Namespace
