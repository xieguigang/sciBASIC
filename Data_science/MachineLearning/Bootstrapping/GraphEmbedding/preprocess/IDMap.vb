Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Text

Namespace GraphEmbedding.preprocess

    Public Class IDMap
        Public m_lstEntities As Dictionary(Of String, Boolean) = Nothing
        Public m_lstRelations As Dictionary(Of String, Boolean) = Nothing

        Public Overridable Sub LoadEntRel(fnInput As String)
            m_lstEntities = New Dictionary(Of String, Boolean)()
            m_lstRelations = New Dictionary(Of String, Boolean)()
            Dim sr As StreamReader = New StreamReader(New FileStream(fnInput, FileMode.Open, FileAccess.Read), Encoding.UTF8)

            Dim line = ""
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, sr.ReadLine())), Nothing)
                Dim tokens = line.Split(ASCII.TAB)
                Dim strHead = tokens(0)
                Dim strRelation = tokens(1)
                Dim strTail = tokens(2)
                If Not m_lstEntities.ContainsKey(strHead) Then
                    m_lstEntities(strHead) = True
                End If
                If Not m_lstEntities.ContainsKey(strTail) Then
                    m_lstEntities(strTail) = True
                End If
                If Not m_lstRelations.ContainsKey(strRelation) Then
                    m_lstRelations(strRelation) = True
                End If
            End While
            sr.Close()
        End Sub

        Public Overridable Sub OutputIDMap(fnEntityMap As String, fnRelationMap As String)
            Dim sw1 As StreamWriter = New StreamWriter(New FileStream(fnEntityMap, FileMode.Create, FileAccess.Write), Encoding.UTF8)
            Dim sw2 As StreamWriter = New StreamWriter(New FileStream(fnRelationMap, FileMode.Create, FileAccess.Write), Encoding.UTF8)

            Dim iID = 0
            Dim iterEnt As IEnumerator(Of String) = m_lstEntities.Keys.GetEnumerator()
            While iterEnt.MoveNext()
                sw1.Write(iID.ToString() & vbTab & iterEnt.Current & vbLf)
                iID += 1
            End While
            iID = 0
            Dim iterRel As IEnumerator(Of String) = m_lstRelations.Keys.GetEnumerator()
            While iterRel.MoveNext()
                sw2.Write(iID.ToString() & vbTab & iterRel.Current & vbLf)
                iID += 1
            End While
            sw1.Close()
            sw2.Close()
        End Sub

        Public Shared Sub Main(args As String())
            Dim fnInput = "D:\MyExperiments\ComplExWithRules\WN18\wordnet-mlj12-train.txt"
            Dim fnOutput1 = "D:\MyExperiments\ComplExWithRules\WN18\EntityIDMap.tsv"
            Dim fnOutput2 = "D:\MyExperiments\ComplExWithRules\WN18\RelationIDMap.tsv"
            Dim mapper As IDMap = New IDMap()
            mapper.LoadEntRel(fnInput)
            mapper.OutputIDMap(fnOutput1, fnOutput2)
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
