#Region "Microsoft.VisualBasic::edba9a6676c776e13e81d6a4bbed73bd, mime\application%pdf\test\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 107
    '    Code Lines: 58
    ' Comment Lines: 29
    '   Blank Lines: 20
    '     File Size: 4.55 KB


    '     Class Program
    ' 
    '         Sub: ListIndirectObjects, LoadImmediately, LoadOnDemand, Main, ShowFirstPageContent
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
