Namespace ApplicationServices.Report

    Public MustInherit Class ReportRender

        Public MustOverride Sub Render(report As IEnumerable(Of Block))

    End Class
End Namespace