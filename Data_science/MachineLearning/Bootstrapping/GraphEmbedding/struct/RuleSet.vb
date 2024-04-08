Imports System.IO
Imports Microsoft.VisualBasic.Text

Namespace GraphEmbedding.struct


    Public Class RuleSet
        Private lstRules As List(Of Rule)

        Public Overridable Sub load(fnInput As String)
            Dim reader As StreamReader = New StreamReader(fnInput)
            lstRules = New List(Of Rule)()
            Dim line = ""
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, reader.ReadLine())), Nothing)
                Dim tokens = line.Split(ASCII.TAB)
                Dim rule As Rule = New Rule()
                Dim ruleString = tokens(0)
                rule.conf = Double.Parse(tokens(1))
                Dim ruleStrings = ruleString.Split(","c)
                rule.add(New Relation(Integer.Parse(ruleStrings(ruleStrings.Length - 1)), 1))
                For i = 0 To ruleStrings.Length - 1 - 1
                    Dim rel = Integer.Parse(ruleStrings(i))
                    Dim dir = 1
                    If rel < 0 Then
                        rel = -1 * rel
                        dir = -1
                    End If
                    rule.add(New Relation(rel, dir))
                Next
                lstRules.Add(rule)
            End While
            reader.Close()
        End Sub
        Public Overridable Function rules() As List(Of Rule)
            Return lstRules
        End Function

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class

End Namespace
