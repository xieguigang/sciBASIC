#Region "Microsoft.VisualBasic::aef095a0cceef03b321955f6389f1fda, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\struct\TripleSet.vb"

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

    '   Total Lines: 104
    '    Code Lines: 89 (85.58%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (14.42%)
    '     File Size: 4.32 KB


    '     Class TripleSet
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [get], entities, relations, triples
    ' 
    '         Sub: load, randomShuffle
    '         Class CSharpImpl
    ' 
    '             Function: __Assign
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.util
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace GraphEmbedding.struct

    Public Class TripleSet

        Private iNumberOfEntities As Integer
        Private iNumberOfRelations As Integer
        Private iNumberOfTriples As Integer
        Public pTriple As List(Of Triple) = Nothing

        Public Sub New()
        End Sub

        Public Sub New(iEntities As Integer, iRelations As Integer)
            iNumberOfEntities = iEntities
            iNumberOfRelations = iRelations
        End Sub

        Public Overridable Function entities() As Integer
            Return iNumberOfEntities
        End Function

        Public Overridable Function relations() As Integer
            Return iNumberOfRelations
        End Function

        Public Overridable Function triples() As Integer
            Return iNumberOfTriples
        End Function

        Public Overridable Function [get](iID As Integer) As Triple
            If iID < 0 OrElse iID >= iNumberOfTriples Then
                Throw New Exception("getTriple error in TripleSet: ID out of range")
            End If
            Return pTriple(iID)
        End Function

        Public Overridable Sub load(fnInput As String, iSize As Integer)
            Dim reader As StreamReader = New StreamReader(New FileStream(fnInput, FileMode.Open, FileAccess.Read), Encoding.UTF8)
            pTriple = New List(Of Triple)()

            Dim line As Value(Of String) = ""
            Dim iCnt = 0

            While Not (line = reader.ReadLine()) Is Nothing
                Dim tokens = StringSplitter.RemoveEmptyEntries(StringSplitter.split(vbTab & " ", line))
                If tokens.Length <> 3 Then
                    Throw New Exception("load error in TripleSet: data format incorrect")
                End If
                Dim iHead = Integer.Parse(tokens(0))
                Dim iTail = Integer.Parse(tokens(2))
                Dim iRelation = Integer.Parse(tokens(1))
                If iHead < 0 OrElse iHead >= iNumberOfEntities Then
                    Throw New Exception("load error in TripleSet: head entity ID out of range")
                End If
                If iTail < 0 OrElse iTail >= iNumberOfEntities Then
                    Throw New Exception("load error in TripleSet: tail entity ID out of range")
                End If
                If iRelation < 0 OrElse iRelation >= iNumberOfRelations Then
                    Throw New Exception("load error in TripleSet: relation ID out of range")
                End If
                pTriple.Add(New Triple(iHead, iTail, iRelation))
                iCnt += 1
                If iSize <> -1 AndAlso iCnt >= iSize Then
                    Exit While
                End If
            End While
            iNumberOfTriples = pTriple.Count
            reader.Close()
        End Sub

        Public Overridable Sub randomShuffle()
            Dim tmpMap As SortedDictionary(Of Double, Triple) = New SortedDictionary(Of Double, Triple)()
            For iID = 0 To iNumberOfTriples - 1
                Dim i As Integer = pTriple(iID).head()
                Dim j As Integer = pTriple(iID).tail()
                Dim k As Integer = pTriple(iID).relation()
                tmpMap(randf.NextDouble) = New Triple(i, j, k)
            Next

            pTriple = New List(Of Triple)()
            Dim iterValues As IEnumerator(Of Double) = tmpMap.Keys.GetEnumerator()
            While iterValues.MoveNext()
                Dim dRand = iterValues.Current
                Dim trip = tmpMap(dRand)
                pTriple.Add(New Triple(trip.head(), trip.tail(), trip.relation()))
            End While
            iNumberOfTriples = pTriple.Count
            tmpMap.Clear()
        End Sub
    End Class

End Namespace
