Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq

Public Module SQL

    ''' <summary>
    ''' 加入是以Dump模型运行的话，标题只会被解析一次，其他的行数据的标题都会使用第一行的数据的标题，
    ''' 这个函数只适用于``INSERT INTO``数据插入语句
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <param name="dumpMode"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function SQLData(path$, Optional dumpMode As Boolean = True) As IEnumerable(Of Dictionary(Of String, String))
        Dim firstHeader As IndexOf(Of String) = path.ReadFirstLine.SQLFields
        Dim getHeader = Function(SQL$) firstHeader

        If Not dumpMode Then
            getHeader = Function(SQL$) SQL.SQLFields
        End If

        For Each rowSQL As String In path.IterateAllLines
            Dim fields As IndexOf(Of String) = getHeader(SQL:=rowSQL)
            Dim values As String() = rowSQL.SQLValues
            Dim row As New Dictionary(Of String, String)

            For Each map As SeqValue(Of String) In fields
                row.Add(map.value, values(map.i))
            Next

            Yield row
        Next
    End Function

    <Extension>
    Public Function SQLValues(insertSQL$) As String()
        Dim values$ = Regex.Split(insertSQL, "\)\s*VALUES\s*\(", RegexICSng).Last
        Dim t$() = IO.CharsParser(values).ToArray
        Return t
    End Function

    <Extension>
    Public Function SQLFields(insertSQL$) As IndexOf(Of String)
        Dim fields$ = Regex _
            .Match(insertSQL, "INSERT INTO .+?\)\s*VALUES\s*\(", RegexICSng) _
            .Value _
            .StringSplit("\s*VALUES\s*\(") _
            .First _
            .GetStackValue("(", ")")
        Dim names$() = IO.CharsParser(fields).ToArray
        Return New IndexOf(Of String)(names)
    End Function
End Module
