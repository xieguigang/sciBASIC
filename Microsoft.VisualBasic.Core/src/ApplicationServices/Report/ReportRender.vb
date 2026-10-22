Namespace ApplicationServices.Report

    ''' <summary>
    ''' The base class of the report renderer: converts the markdown content model
    ''' <see cref="Block"/> into a target document format. Both the plain text based
    ''' format (such as the markdown or html text, see <see cref="TextReportRender"/>)
    ''' and the binary document format (such as the microsoft office word document)
    ''' should implements this abstract class.
    ''' </summary>
    Public MustInherit Class ReportRender

        ''' <summary>
        ''' render the given <see cref="Block"/> document into the target document format
        ''' </summary>
        ''' <param name="report">the markdown content blocks of the target document</param>
        Public MustOverride Sub Render(report As IEnumerable(Of Block))

    End Class
End Namespace
