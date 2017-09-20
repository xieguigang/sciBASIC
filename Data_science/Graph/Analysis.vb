Imports System.Runtime.CompilerServices

Public Module AnalysisExtensions

    ' Simple 'drawing' routines
    <Extension> Public Function Build(Of T)(tree As Tree(Of T)) As String
        If tree Is Nothing Then
            Return "()"
        End If

        If tree.isleaf Then
            Return tree.ID
        Else
            Dim children = tree _
            .childs _
            .select(Function(t) t.build) _
            .joinby(", ")

            Return $"{tree.ID}({children})"
        End If
    End Function

    ' summary this tree model its nodes as csv table
    <Extension> Public Iterator Function Summary(Of T)(tree As tree(Of T), Optional schema As Propertyinfo() = Nothing) As IEnumerable(Of NamedValue(Of Dictionary(Of String, String)))

        If schema.isnullorempty Then
            schema = dataframework.schema(Of T)(index:=False, primitive:=True, access:=Readable)
        End If

        Yield tree.SummaryMe(schema)

        For Each c As Tree(Of T) In tree.Childs.SafeQuery
            Yield c.Summary(schema)
        Next
    End Function

    ' 这个函数不会对childs进行递归
    Private Function SummaryMe(Of T)(this As Tree(Of T), schema As Propertyinfo()) As namedValue(Of Dictionary(Of String, String))
        Dim name = this.label
        Dim values = schema.ToDictionary(Function(key) key.name, Function(read) safeCStr(read.getValue(this.data)))
        values.Add("tree.ID", this.ID)
        values.add("tree.Label", this.label)

        Return New namedvalue(Of Dictionary(Of String, String)) With {.name = name, .value = values}
    End Function
End Module
