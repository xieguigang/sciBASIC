Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Namespace DocumentStream

    Module Meta

        <Extension>
        Public Function IsMetaRow(row As RowObject) As Boolean
            If row.First <> "#"c Then
                Return False
            Else
                Return row.Count <= 2
            End If
        End Function

        <Extension>
        Public Function ToCsvMeta(Of T As Class)(x As T) As RowObject()
            Return ToCsvMeta(x, GetType(T))
        End Function

        Public Function ToCsvMeta(o As Object, type As Type) As RowObject()
            Dim schema = DataFrameColumnAttribute.LoadMapping(type, mapsAll:=True)
            Dim source = schema.Select(Function(x) New NamedValue(Of Object)(x.Key, x.Value.GetValue(o)))
            Dim out As RowObject() = ToCsvMeta(source).ToArray
            Return out
        End Function

        <Extension>
        Public Iterator Function ToCsvMeta(Of T)(source As IEnumerable(Of NamedValue(Of T))) As IEnumerable(Of RowObject)
            Dim s As String

            For Each x As NamedValue(Of T) In source
                s = Scripting.ToString(x.x)
                Yield New RowObject({$"##{x.Name}={s}"})
            Next
        End Function

        <Extension>
        Public Iterator Function ToCsvMeta(source As IEnumerable(Of KeyValuePair(Of String, String))) As IEnumerable(Of RowObject)
            For Each x In source
                Yield New RowObject({$"##{x.Key}={x.Value}"})
            Next
        End Function
    End Module
End Namespace