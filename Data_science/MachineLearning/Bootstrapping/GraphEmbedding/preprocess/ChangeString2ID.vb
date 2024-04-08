Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Text

Namespace GraphEmbedding.preprocess

    Public Class ChangeString2ID
        Public m_EntityIDMap As Dictionary(Of String, Integer) = Nothing
        Public m_RelationIDMap As Dictionary(Of String, Integer) = Nothing

        Public Sub New()
        End Sub

        Public Overridable Sub LoadIDMaps(fnEntID As String, fnRelID As String)
            m_EntityIDMap = New Dictionary(Of String, Integer)()
            m_RelationIDMap = New Dictionary(Of String, Integer)()
            Dim ent As StreamReader = New StreamReader(New FileStream(fnEntID, FileMode.Open, FileAccess.Read), Encoding.UTF8)
            Dim rel As StreamReader = New StreamReader(New FileStream(fnRelID, FileMode.Open, FileAccess.Read), Encoding.UTF8)

            Dim line = ""
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, ent.ReadLine())), Nothing)
                Dim tokens = line.Split(ASCII.TAB)
                Dim iID As Integer? = Integer.Parse(tokens(0))
                m_EntityIDMap(tokens(1)) = iID.Value
            End While
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, rel.ReadLine())), Nothing)
                Dim tokens = line.Split(ASCII.TAB)
                Dim iID As Integer? = Integer.Parse(tokens(0))
                m_RelationIDMap(tokens(1)) = iID.Value
            End While
            ent.Close()
            rel.Close()
        End Sub

        Public Overridable Sub ChangeFormat(fnInput As String, fnOutput As String)
            Dim sr As StreamReader = New StreamReader(New FileStream(fnInput, FileMode.Open, FileAccess.Read), Encoding.UTF8)
            Dim sw As StreamWriter = New StreamWriter(New FileStream(fnOutput, FileMode.Create, FileAccess.Write), Encoding.UTF8)

            Dim line = ""
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, sr.ReadLine())), Nothing)
                Dim tokens = line.Split(ASCII.TAB)
                Dim iHead = m_EntityIDMap(tokens(0))
                Dim iTail = m_EntityIDMap(tokens(2))
                Dim iRelation = m_RelationIDMap(tokens(1))
                sw.Write(iHead.ToString() & vbTab & iRelation.ToString() & vbTab & iTail.ToString() & vbLf)
            End While
            sr.Close()
            sw.Close()
        End Sub

        Public Shared Sub Main(args As String())
            Dim fnEntID = "D:\MyExperiments\ComplExWithRules\WN18\EntityIDMap.tsv"
            Dim fnRelID = "D:\MyExperiments\ComplExWithRules\WN18\RelationIDMap.tsv"
            Dim fnInput = "D:\MyExperiments\ComplExWithRules\WN18\wordnet-mlj12-train.txt"
            Dim fnOutput = "D:\MyExperiments\ComplExWithRules\WN18\WN18-train.txt"
            Dim converter As ChangeString2ID = New ChangeString2ID()
            converter.LoadIDMaps(fnEntID, fnRelID)
            converter.ChangeFormat(fnInput, fnOutput)
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
