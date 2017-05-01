Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace CommandLine.InteropService.SharedORM

    Public Class VisualBasic : Inherits CodeGenerator

        Public Sub New(CLI As Type)
            MyBase.New(CLI)
        End Sub

        Public Overrides Function GetSourceCode() As String
            Dim vb As New StringBuilder

            For Each API In Me.EnumeratesAPI
                Call __calls(vb, API)
            Next

            Return vb.ToString
        End Function

        Private Sub __calls(vb As StringBuilder, API As NamedValue(Of CommandLine))

        End Sub
    End Class
End Namespace