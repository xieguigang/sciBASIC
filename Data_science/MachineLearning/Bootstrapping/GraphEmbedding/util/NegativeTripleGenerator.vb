Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.struct
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace GraphEmbedding.util

    Public Class NegativeTripleGenerator
        Public PositiveTriple As Triple
        Public iNumberOfEntities As Integer
        Public iNumberOfRelation As Integer

        Public Sub New(inPositiveTriple As Triple, inNumberOfEntities As Integer, inNumberOfRelation As Integer)
            PositiveTriple = inPositiveTriple
            iNumberOfEntities = inNumberOfEntities
            iNumberOfRelation = inNumberOfRelation
        End Sub

        Public Overridable Function generateHeadNegTriple(m_NumNeg As Integer) As HashSet(Of Triple)
            Dim iPosHead As Integer = PositiveTriple.head()
            Dim iPosTail As Integer = PositiveTriple.tail()
            Dim iPosRelation As Integer = PositiveTriple.relation()


            Dim NegativeTripleSet As HashSet(Of Triple) = New HashSet(Of Triple)()
            While NegativeTripleSet.Count < m_NumNeg
                Dim iNegHead = iPosHead
                Dim NegativeTriple As Triple = New Triple(iNegHead, iPosTail, iPosRelation)
                While iNegHead = iPosHead
                    iNegHead = CInt(randf.NextDouble * iNumberOfEntities)
                    NegativeTriple = New Triple(iNegHead, iPosTail, iPosRelation)
                End While
                NegativeTripleSet.Add(NegativeTriple)
            End While
            Return NegativeTripleSet
        End Function

        Public Overridable Function generateTailNegTriple(m_NumNeg As Integer) As HashSet(Of Triple)
            Dim iPosHead As Integer = PositiveTriple.head()
            Dim iPosTail As Integer = PositiveTriple.tail()
            Dim iPosRelation As Integer = PositiveTriple.relation()


            Dim NegativeTripleSet As HashSet(Of Triple) = New HashSet(Of Triple)()

            While NegativeTripleSet.Count < m_NumNeg
                Dim iNegTail = iPosTail
                Dim NegativeTriple As Triple = New Triple(iPosHead, iNegTail, iPosRelation)
                While iNegTail = iPosTail
                    iNegTail = CInt(randf.NextDouble * iNumberOfEntities)
                    NegativeTriple = New Triple(iPosHead, iNegTail, iPosRelation)
                End While
                NegativeTripleSet.Add(NegativeTriple)
            End While

            Return NegativeTripleSet
        End Function

        Public Overridable Function generateRelNegTriple() As Triple
            Dim iPosHead As Integer = PositiveTriple.head()
            Dim iPosTail As Integer = PositiveTriple.tail()
            Dim iPosRelation As Integer = PositiveTriple.relation()

            Dim iNegRelation = iPosRelation
            Dim NegativeTriple As Triple = New Triple(iPosHead, iPosTail, iNegRelation)
            While iNegRelation = iPosRelation
                iNegRelation = CInt(randf.NextDouble * iNumberOfRelation)
                NegativeTriple = New Triple(iPosHead, iPosTail, iNegRelation)
            End While
            Return NegativeTriple
        End Function
    End Class

End Namespace
