Imports System.IO
Imports System.Text
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

            Dim line = ""
            Dim iCnt = 0
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, reader.ReadLine())), Nothing)
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

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class

End Namespace
