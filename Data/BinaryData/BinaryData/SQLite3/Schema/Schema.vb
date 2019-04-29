Imports System.Collections.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ManagedSqlite.Core

    Public Class Schema

        Public Property Columns As NamedValue(Of String)()

        Sub New(columns As String())
            Me.Columns = ParseColumns(columns).ToArray
        End Sub

        Private Iterator Function ParseColumns(columns As String()) As IEnumerable(Of NamedValue(Of String))
            Dim tokens As String()
            Dim field As NamedValue(Of String)

            For Each column As String In columns
                tokens = column.StringSplit("\s+")
                field = New NamedValue(Of String) With {
                    .Name = tokens(Scan0),
                    .Value = tokens(1)
                }

                Yield field
            Next
        End Function
    End Class
End Namespace