Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module SchemasAPI

    <Extension>
    Public Function SaveData(Of T As Class)(source As IEnumerable(Of T), DIR As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean
        Dim schema As Schema = Schema.GetSchema(Of T)
        Dim files = (From x As NamedValue(Of String)
                     In schema.Members
                     Select x
                     Group x By x.x Into Group).ToArray

        Dim type As Type = GetType(T)
        Dim IO As [Class] = [Class].GetSchema(type)

        For Each obj As SeqValue(Of T) In source.SeqIterator

        Next

        Return schema.GetJson.SaveTo(DIR & "/" & Schema.DefaultName)
    End Function
End Module
