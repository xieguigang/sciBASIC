Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream

Public Module DocumentExtensions

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="cols"><see cref="File.Columns"/> filtering results.</param>
    ''' <returns></returns>
    <Extension>
    Public Function JoinColumns(cols As IEnumerable(Of String())) As DocumentStream.File
        Dim array As String()() = cols.ToArray
        Dim out As New DocumentStream.File

        For i As Integer = 0 To array.First.Length - 1
            Dim ind As Integer = i
            out += New RowObject(array.Select(Function(x) x(ind)))
        Next

        Return out
    End Function

    <Extension>
    Public Function Apply(ByRef row As RowObject, action As Func(Of String, String), Optional skip As Integer = 0) As RowObject
        For i As Integer = skip To row._innerColumns.Count - 1
            row._innerColumns(i) = action(row._innerColumns(i))
        Next

        Return row
    End Function
End Module
