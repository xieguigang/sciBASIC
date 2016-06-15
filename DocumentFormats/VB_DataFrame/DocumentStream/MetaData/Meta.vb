Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal

Namespace DocumentStream

    Module Meta

        Public Function TryGetMetaData(Of T)(reader As File, ByRef i As Integer) As T
            Dim [in] As Dictionary(Of String, String) = TryGetMetaData(reader, i)
            Dim schema = DataFrameColumnAttribute.LoadMapping(Of T)(mapsAll:=True)
            Dim x As Object = Activator.CreateInstance(Of T)
            Dim value As String = Nothing

            For Each prop In schema
                If [in].TryGetValue(prop.Key, value) Then
                    Dim o As Object = Scripting.CTypeDynamic(value, prop.Value.Type)
                    Call prop.Value.SetValue(x, o)
                End If
            Next

            Return DirectCast(x, T)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="reader"></param>
        ''' <param name="i">下一行是标题行</param>
        ''' <returns></returns>
        Public Function TryGetMetaData(reader As File, ByRef i As Integer) As Dictionary(Of String, String)
            Dim p As New Pointer(Of RowObject)(reader)
            Dim out As New Dictionary(Of String, String)
            Dim name As String

            Do While (++p).IsMetaRow
                Dim row = p.Current.First.GetTagValue("=")
                name = row.Name
                name = Regex.Replace(name, "^#+", "", RegexOptions.Multiline)

                Call out.Add(name, row.x)
            Loop

            i = p.Pointer

            Return out
        End Function

        <Extension>
        Public Function DataFrameWithMeta(Of T)(x As T) As File
            Return New File(x.ToCsvMeta)
        End Function

        <Extension>
        Public Function IsMetaRow(row As RowObject) As Boolean
            If row.First <> "#"c Then
                Return False
            Else
                Return row.Count <= 2
            End If
        End Function

        <Extension>
        Public Function ToCsvMeta(Of T)(x As T) As RowObject()
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