Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.Models

Module Program
    Sub Main(args As String())
        If args.Length < 1 Then
            Console.WriteLine("usage: test <path-to.pdb> [userName] [repoName] [commit] [localRoot] [diag]")
            Return
        End If

        Dim pdbPath As String = args(0)
        Dim userName As String = If(args.Length > 1, args(1), "xieguigang")
        Dim repoName As String = If(args.Length > 2, args(2), "sciBASIC")
        Dim commit As String = If(args.Length > 3, args(3), "master")
        Dim localRoot As String = If(args.Length > 4, args(4), Nothing)
        Dim diag As Boolean = args.Length > 5 AndAlso args(5) = "diag"

        Dim pdb As PDB

        Try
            pdb = pdb.Open(pdbPath)
        Catch ex As Exception
            Console.Error.WriteLine("Failed to open PDB: " & ex.Message)
            Return
        End Try

        Console.WriteLine($"Format        : {pdb.Format}")

        If pdb.PdbInfo IsNot Nothing Then
            Console.WriteLine($"PDB GUID      : {pdb.PdbInfo.Guid}")
            Console.WriteLine($"Signature/Age : {pdb.PdbInfo.Signature} / {pdb.PdbInfo.Age}")
        End If

        Console.WriteLine($"Documents     : {pdb.SourceDocuments.Count}")
        Console.WriteLine($"Line numbers  : {pdb.LineNumbers.Count}")
        Console.WriteLine($"Symbols       : {pdb.Symbols.Count}")
        Console.WriteLine($"Type records  : {pdb.TypeRecords.Count}")

        If localRoot Is Nothing Then
            pdb.PointLocal2Github(userName, repoName, commit)
        Else
            pdb.PointLocal2Github(userName, repoName, commit, localRoot)
        End If

        Console.WriteLine()
        Console.WriteLine("Sample source documents (local -> github):")

        Dim shown As Integer = 0

        For Each doc As SourceDocument In pdb.SourceDocuments
            Console.WriteLine($"  {doc.FilePath}")

            If Not String.IsNullOrEmpty(doc.GitHubUrl) Then
                Console.WriteLine($"    -> {doc.GitHubUrl}")
            End If

            shown += 1
            If shown >= 5 Then Exit For
        Next

        If pdb.TypeRecords.Count > 0 Then
            Console.WriteLine()
            Console.WriteLine("Sample type records:")

            For i As Integer = 0 To Math.Min(4, pdb.TypeRecords.Count - 1)
                Dim t As TypeRecord = pdb.TypeRecords(i)
                Console.WriteLine($"  [{t.Kind}] {t.Name} (size={t.Size})")
            Next
        End If

        If pdb.Symbols.Count > 0 Then
            Console.WriteLine()
            Console.WriteLine("Sample symbols:")

            For i As Integer = 0 To Math.Min(4, pdb.Symbols.Count - 1)
                Dim s As Symbol = pdb.Symbols(i)
                Console.WriteLine($"  [{s.Kind}] {s.Name} @ {s.Section}:{s.Offset}")
            Next
        End If

        If pdb.LineNumbers.Count > 0 Then
            Console.WriteLine()
            Console.WriteLine("Sample line numbers:")

            For i As Integer = 0 To Math.Min(4, pdb.LineNumbers.Count - 1)
                Dim ln As LineInfo = pdb.LineNumbers(i)
                Console.WriteLine($"  {ln.Document.FilePath}: {ln.StartLine},{ln.StartColumn} - {ln.EndLine},{ln.EndColumn} (IL {ln.Offset})")
            Next
        End If

        If diag AndAlso pdb.Format = PDB.FormatKind.Classic Then
            DumpClassic(pdbPath)
        End If
    End Sub

    Private Sub DumpClassic(pdbPath As String)
        Console.WriteLine()
        Console.WriteLine("==== classic internals ====")

        Using msf As New MSFReader(pdbPath)
            Console.WriteLine("StreamCount: " & msf.StreamCount)

            Dim dbiStream As Stream = msf.GetStream(MSFReader.StreamDbi)
            Dim dbi As New DbiReader(dbiStream)

            Console.WriteLine("PublicStreamIndex    : " & dbi.Header.PublicStreamIndex)
            Console.WriteLine("SymRecordStreamIndex : " & dbi.Header.SymRecordStreamIndex)
            Console.WriteLine("ModInfoSize          : " & dbi.Header.ModInfoSize)
            Console.WriteLine("SectionContribution  : " & dbi.Header.SectionContributionSize)
            Console.WriteLine("SectionMap           : " & dbi.Header.SectionMapSize)
            Console.WriteLine("SourceInfoSize       : " & dbi.Header.SourceInfoSize)
            Console.WriteLine("TypeServerMapSize    : " & dbi.Header.TypeServerMapSize)
            Console.WriteLine("OptionalDbgHdrSize   : " & dbi.Header.OptionalDbgHdrSize)
            Console.WriteLine("ECSubstreamSize      : " & dbi.Header.ECSubstreamSize)
            Console.WriteLine("Modules              : " & dbi.Modules.Count)

            For i As Integer = 0 To Math.Min(2, dbi.Modules.Count - 1)
                Dim m As ModuleInfo = dbi.Modules(i)
                Console.WriteLine($"  Module[{i}] name={m.ModuleName} obj={m.ObjFileName} files={m.FileIndices.Length} c13size={m.C13Size}")
            Next

            Dim pub As Stream = msf.GetStream(dbi.Header.PublicStreamIndex)
            Console.WriteLine("Public stream bytes  : " & If(pub Is Nothing, -1, pub.GetBytes().Length))

            Dim tpi As Stream = msf.GetStream(MSFReader.StreamTpi)
            If tpi IsNot Nothing Then
                Dim td As Byte() = tpi.GetBytes()
                Console.WriteLine("TPI bytes            : " & td.Length)
                Console.Write("TPI head  : ")
                For i As Integer = 0 To Math.Min(31, td.Length - 1)
                    Console.Write(td(i).ToString("X2") & " ")
                Next
                Console.WriteLine()
            End If

            ' Tail debug substream size (after all fixed substreams).
            Dim pos As Integer = 64 + dbi.Header.ModInfoSize + dbi.Header.SectionContributionSize +
                dbi.Header.SectionMapSize + dbi.Header.SourceInfoSize + dbi.Header.TypeServerMapSize +
                dbi.Header.OptionalDbgHdrSize + dbi.Header.ECSubstreamSize
            Dim dbiBytes As Byte() = dbiStream.GetBytes()
            Console.WriteLine("DBI bytes            : " & dbiBytes.Length)
            Console.WriteLine("Tail debug substream : " & (dbiBytes.Length - pos) & " (pos=" & pos & ")")
        End Using
    End Sub
End Module
