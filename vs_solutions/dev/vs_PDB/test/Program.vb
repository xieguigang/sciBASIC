Imports vs_PDB.sciBASIC.PDB

Module Program
    Sub Main(args As String())
        If args.Length < 1 Then
            Console.WriteLine("usage: test <path-to.pdb> [userName] [repoName] [commit] [localRoot]")
            Return
        End If

        Dim pdbPath As String = args(0)
        Dim userName As String = If(args.Length > 1, args(1), "xieguigang")
        Dim repoName As String = If(args.Length > 2, args(2), "sciBASIC")
        Dim commit As String = If(args.Length > 3, args(3), "master")
        Dim localRoot As String = If(args.Length > 4, args(4), Nothing)

        Dim pdb As PDB

        Try
            pdb = PDB.Open(pdbPath)
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
    End Sub
End Module
