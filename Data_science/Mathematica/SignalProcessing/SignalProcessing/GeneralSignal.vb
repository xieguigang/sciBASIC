Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Public Class GeneralSignal : Implements INamedValue

    Public Property Measures As Double()
    Public Property Strength As Double()

    ''' <summary>
    ''' the unique reference
    ''' </summary>
    ''' <returns></returns>
    Public Property reference As String Implements INamedValue.Key
    Public Property measureUnit As String
    Public Property description As String
    Public Property meta As Dictionary(Of String, String)

    Public Overrides Function ToString() As String
        Return description
    End Function

    Public Function GetText() As String
        Dim sb As New StringBuilder

        For Each line As String In description.LineTokens
            Call sb.AppendLine("# " & line)
        Next

        For Each par In meta
            Call sb.AppendLine($"{par.Key}={par.Value}")
        Next

        Call sb.AppendLine(measureUnit & vbTab & "intensity")

        For i As Integer = 0 To Measures.Length - 1
            Call sb.AppendLine(Measures(i) & vbTab & Strength(i))
        Next

        Return sb.ToString
    End Function

End Class
