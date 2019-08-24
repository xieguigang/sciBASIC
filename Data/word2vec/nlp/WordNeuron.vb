Imports System
Imports System.Collections.Generic
Imports HuffmanNode = org.nlp.util.HuffmanNode
Imports HuffmanTree = org.nlp.util.HuffmanTree

Namespace org.nlp.vec


    ''' <summary>
    ''' Created by fangy on 13-12-17.
    ''' 词神经元
    ''' </summary>
    Public Class WordNeuron
        Inherits HuffmanNeuron

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: Fields cannot have the same name as methods:
        Private name_Renamed As String
        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: Fields cannot have the same name as methods:
        Private pathNeurons_Renamed As IList(Of HuffmanNode)

        Public Sub New(ByVal name As String, ByVal freq As Integer, ByVal vectorSize As Integer)
            MyBase.New(freq, vectorSize)
            name_Renamed = name
            Dim random As Random = New Random()

            For i = 0 To vector.Length - 1
                vector(i) = (random.NextDouble() - 0.5) / vectorSize
            Next
        End Sub

        Public Overridable Property name As String
            Get
                Return name_Renamed
            End Get
            Set(ByVal value As String)
                name_Renamed = value
            End Set
        End Property

        Public Overridable ReadOnly Property pathNeurons As IList(Of HuffmanNode)
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
