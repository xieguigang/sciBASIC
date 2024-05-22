#Region "Microsoft.VisualBasic::717f847d6c1b2c6fd4b86f535731fa35, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\util\NegativeTripleGenerator.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 77
    '    Code Lines: 59 (76.62%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (23.38%)
    '     File Size: 3.18 KB


    '     Class NegativeTripleGenerator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: generateHeadNegTriple, generateRelNegTriple, generateTailNegTriple
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
