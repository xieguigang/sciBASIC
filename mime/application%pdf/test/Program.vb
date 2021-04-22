Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.application.pdf.PdfReader

Namespace ExampleConsoleApp
    Friend Class Program
        Public Shared Sub Main()
            Dim filename = "D:\biodeep\biodeep_pipeline\biodeepdb\Odor Threshold Values.pdf"
            '-----------------------------------------------
            ' Comment/Uncomment the example you want to run
            '-----------------------------------------------

            ' 1 - Show document details in compact form
            '---------------------------------------------
            ' LoadImmediately(filename, False, False)

            ' 2 - As above but also resolve references
            '---------------------------------------------
            '  LoadImmediately(filename, true, false);

            ' 3 - As above but also show stream contents
            '---------------------------------------------
            '  LoadImmediately(filename, true, true);

            ' 4 - On demand versions of the above three methods
            '---------------------------------------------
            '  LoadOnDemand(filename, false, false);
            '  LoadOnDemand(filename, true, false);
            '  LoadOnDemand(filename, true, true);

            ' 5 - Show content commands for the first page
            '---------------------------------------------
            ShowFirstPageContent(filename)

            ' 6 - List the indirect objects
            '---------------------------------------------
            '  ListIndirectObjects(filename, false, false);

            Pause()
        End Sub

        Private Shared Sub LoadImmediately(ByVal filename As String, ByVal resolve As Boolean, ByVal streamContent As Boolean)
            Dim document As PdfDocument = New PdfDocument()
            document.Load(filename, True)
            document.Close()

            ' Output an overview of the document contents
            Dim builder As PdfDebugBuilder = New PdfDebugBuilder(document)
            builder.Resolve = resolve
            builder.StreamContent = streamContent
            Console.WriteLine(builder.ToString())
        End Sub

        Private Shared Sub LoadOnDemand(ByVal filename As String, ByVal resolve As Boolean, ByVal streamContent As Boolean)
            Dim document As PdfDocument = New PdfDocument()
            document.Load(filename, False)
            Dim builder As PdfDebugBuilder = New PdfDebugBuilder(document)
            builder.Resolve = resolve
            builder.StreamContent = streamContent
            Console.WriteLine(builder.ToString())

            ' Cannot close document until finished accessing it
            document.Close()
        End Sub

        Private Shared Sub ShowFirstPageContent(ByVal filename As String)
            Dim document As PdfDocument = New PdfDocument()
            document.Load(filename, False)
            Dim contents = document.Catalog.Pages(0).Contents

            ' Get a parser for decoding the contents of the page
            Dim parser As PdfContentsParser = contents.CreateParser()

            ' Keep getting new content commands until no more left
            Dim obj As New Value(Of PdfObject)
            Dim strs As New List(Of String)


            While (obj = parser.GetObject()) IsNot Nothing
                If obj.GetUnderlyingType() Is GetType(PdfArray) Then
                    strs.Add(CType(obj, PdfArray).GetAllTextContent)
                    Console.WriteLine(strs.Last)
                Else
                    ' Console.WriteLine(obj);
                End If
            End While

            document.Close()
        End Sub

        Private Shared Sub ListIndirectObjects(ByVal filename As String, ByVal resolve As Boolean, ByVal streamContent As Boolean)
            Dim document As PdfDocument = New PdfDocument()
            document.Load(filename, True)
            document.Close()

            ' Get each indirect object identifier
            For Each id In document.IndirectObjects
                ' Get each generation for the identifier
                For Each gen In id.Value
                    Dim builder As PdfDebugBuilder = New PdfDebugBuilder(gen.Value)
                    builder.Resolve = resolve
                    builder.StreamContent = streamContent
                    Console.WriteLine(builder.ToString())
                Next
            Next
        End Sub
    End Class
End Namespace
