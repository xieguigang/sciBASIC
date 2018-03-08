Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON

Module SerializationIO

    <Extension>
    Public Function SolveListStream(path$, Optional encoding As Encoding = Nothing) As IEnumerable(Of String)
        Select Case path.ExtensionSuffix.ToLower
            Case "", "txt"
                Return path.IterateAllLines
            Case "json"
                Return path.LoadObject(Of String())
            Case "xml"
                Return path.LoadXml(Of String())
            Case Else
                Throw New NotImplementedException(path)
        End Select
    End Function
End Module
