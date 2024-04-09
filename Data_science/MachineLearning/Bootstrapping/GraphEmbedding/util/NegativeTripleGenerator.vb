Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.struct
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace GraphEmbedding.util

    Public Class NegativeTripleGenerator

        ReadOnly positiveTriple As Triple
        ReadOnly numOfEntities As Integer
        ReadOnly numOfRelation As Integer

        Public Sub New(inPositiveTriple As Triple, inNumberOfEntities As Integer, inNumberOfRelation As Integer)
            positiveTriple = inPositiveTriple
            numOfEntities = inNumberOfEntities
            numOfRelation = inNumberOfRelation
        End Sub

        Public Overridable Function generateHeadNegTriple(m_NumNeg As Integer) As HashSet(Of Triple)
            Dim iPosHead As Integer = positiveTriple.head()
            Dim iPosTail As Integer = positiveTriple.tail()
            Dim iPosRelation As Integer = positiveTriple.relation()
            Dim NegativeTripleSet As New HashSet(Of Triple)()

            While NegativeTripleSet.Count < m_NumNeg
                Dim iNegHead = iPosHead
                Dim NegativeTriple As New Triple(iNegHead, iPosTail, iPosRelation)

                While iNegHead = iPosHead
                    iNegHead = std.Floor(randf.NextDouble * numOfEntities)
                    NegativeTriple = New Triple(iNegHead, iPosTail, iPosRelation)
                End While

                NegativeTripleSet.Add(NegativeTriple)
            End While

            Return NegativeTripleSet
        End Function

        Public Overridable Function generateTailNegTriple(m_NumNeg As Integer) As HashSet(Of Triple)
            Dim iPosHead As Integer = positiveTriple.head()
            Dim iPosTail As Integer = positiveTriple.tail()
            Dim iPosRelation As Integer = positiveTriple.relation()
            Dim NegativeTripleSet As New HashSet(Of Triple)()

            While NegativeTripleSet.Count < m_NumNeg
                Dim iNegTail = iPosTail
                Dim NegativeTriple As New Triple(iPosHead, iNegTail, iPosRelation)

                While iNegTail = iPosTail
                    iNegTail = std.Floor(randf.NextDouble * numOfEntities)
                    NegativeTriple = New Triple(iPosHead, iNegTail, iPosRelation)
                End While

                NegativeTripleSet.Add(NegativeTriple)
            End While

            Return NegativeTripleSet
        End Function

        Public Overridable Function generateRelNegTriple() As Triple
            Dim iPosHead As Integer = positiveTriple.head()
            Dim iPosTail As Integer = positiveTriple.tail()
            Dim iPosRelation As Integer = positiveTriple.relation()
            Dim iNegRelation = iPosRelation
            Dim NegativeTriple As New Triple(iPosHead, iPosTail, iNegRelation)

            While iNegRelation = iPosRelation
                iNegRelation = std.Floor(randf.NextDouble * numOfRelation)
                NegativeTriple = New Triple(iPosHead, iPosTail, iNegRelation)
            End While

            Return NegativeTriple
        End Function
    End Class

End Namespace
