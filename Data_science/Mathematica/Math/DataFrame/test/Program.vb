Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.Matrix.MatrixMarket

Module Program

    Sub Main(args As String())
        Call Console.WriteLine("=== MatrixMarket RUA / MTX IO demo ===")
        Call Console.WriteLine()

        Dim mmDir As String = LocateMatrixMarketDir()
        If mmDir Is Nothing Then
            Call Console.WriteLine("ERROR: cannot locate MatrixMarket sample directory.")
            Call Console.ReadLine()
            Return
        End If

        Dim ruaFile As String = Path.Combine(mmDir, "west0655.rua")
        Dim mtxFile As String = Path.Combine(mmDir, "west0655.mtx")

        Call RunReadDemo(ruaFile, mtxFile)
        Call RunWriteRoundTripDemo(mmDir)
        Call RunCrossFormatDemo(mmDir, ruaFile, mtxFile)

        Call Console.WriteLine()
        Call Console.WriteLine("=== All demos finished. Press ENTER to exit. ===")
        Call Console.ReadLine()
    End Sub

    ''' <summary>
    ''' Walk up from the current directory looking for the MatrixMarket folder
    ''' that contains the west0655 sample files.
    ''' </summary>
    Private Function LocateMatrixMarketDir() As String
        Dim dir As New DirectoryInfo(Directory.GetCurrentDirectory())

        Do
            Dim candidate As String = Path.Combine(dir.FullName, "MatrixMarket")
            If Directory.Exists(candidate) AndAlso File.Exists(Path.Combine(candidate, "west0655.rua")) Then
                Return candidate
            End If

            ' also try the parent of the project (source tree layout)
            candidate = Path.Combine(dir.FullName, "Data_science", "Mathematica", "Math", "DataFrame", "MatrixMarket")
            If Directory.Exists(candidate) AndAlso File.Exists(Path.Combine(candidate, "west0655.rua")) Then
                Return candidate
            End If

            dir = dir.Parent
        Loop Until dir Is Nothing

        Return Nothing
    End Function

    Private Sub RunReadDemo(ruaFile As String, mtxFile As String)
        Call Console.WriteLine("--- 1. ReadMatrix demo ---")

        Dim fromRua As SparseMatrix = RUAFormat.ReadMatrix(ruaFile)
        Dim fromMtx As SparseMatrix = MTXFormat.ReadMatrix(mtxFile)

        Call Console.WriteLine($"RUA file : {Path.GetFileName(ruaFile)}")
        Call Console.WriteLine($"   rows={fromRua.RowDimension}, cols={fromRua.ColumnDimension}, nnz={fromRua.nnz}")
        Call Console.WriteLine($"MTX file : {Path.GetFileName(mtxFile)}")
        Call Console.WriteLine($"   rows={fromMtx.RowDimension}, cols={fromMtx.ColumnDimension}, nnz={fromMtx.nnz}")

        Dim same As Boolean = CompareMatrices(fromRua, fromMtx, "RUA vs MTX sample")
        Call Console.WriteLine($"RUA vs MTX sample identical : {same}")
        Call Console.WriteLine()
    End Sub

    Private Sub RunWriteRoundTripDemo(mmDir As String)
        Call Console.WriteLine("--- 2. Write round-trip demo ---")

        Dim src As SparseMatrix = RUAFormat.ReadMatrix(Path.Combine(mmDir, "west0655.rua"))

        Dim ruaOut As String = Path.Combine(mmDir, "west0655.out.rua")
        Dim mtxOut As String = Path.Combine(mmDir, "west0655.out.mtx")

        Call RUAFormat.WriteMatrix(src, ruaOut)
        Call MTXFormat.WriteMatrix(src, mtxOut)

        Dim ruaReload As SparseMatrix = RUAFormat.ReadMatrix(ruaOut)
        Dim mtxReload As SparseMatrix = MTXFormat.ReadMatrix(mtxOut)

        Call Console.WriteLine($"Wrote: {Path.GetFileName(ruaOut)} ({New FileInfo(ruaOut).Length} bytes)")
        Call Console.WriteLine($"Wrote: {Path.GetFileName(mtxOut)} ({New FileInfo(mtxOut).Length} bytes)")

        Dim okRua As Boolean = CompareMatrices(src, ruaReload, "RUA round-trip")
        Dim okMtx As Boolean = CompareMatrices(src, mtxReload, "MTX round-trip")
        Call Console.WriteLine($"RUA write/read round-trip identical : {okRua}")
        Call Console.WriteLine($"MTX write/read round-trip identical : {okMtx}")
        Call Console.WriteLine()
    End Sub

    Private Sub RunCrossFormatDemo(mmDir As String, ruaFile As String, mtxFile As String)
        Call Console.WriteLine("--- 3. Cross format convert demo (MTX -> RUA, RUA -> MTX) ---")

        ' RUA -> MTX
        Dim fromRua As SparseMatrix = RUAFormat.ReadMatrix(ruaFile)
        Dim mtxFromRua As String = Path.Combine(mmDir, "west0655.rua2mtx.mtx")
        Call MTXFormat.WriteMatrix(fromRua, mtxFromRua)
        Dim reloadMtx As SparseMatrix = MTXFormat.ReadMatrix(mtxFromRua)
        Dim ok1 As Boolean = CompareMatrices(fromRua, reloadMtx, "RUA -> MTX")

        ' MTX -> RUA
        Dim fromMtx As SparseMatrix = MTXFormat.ReadMatrix(mtxFile)
        Dim ruaFromMtx As String = Path.Combine(mmDir, "west0655.mtx2rua.rua")
        Call RUAFormat.WriteMatrix(fromMtx, ruaFromMtx)
        Dim reloadRua As SparseMatrix = RUAFormat.ReadMatrix(ruaFromMtx)
        Dim ok2 As Boolean = CompareMatrices(fromMtx, reloadRua, "MTX -> RUA")

        Call Console.WriteLine($"RUA -> MTX -> reload identical : {ok1}")
        Call Console.WriteLine($"MTX -> RUA -> reload identical : {ok2}")
        Call Console.WriteLine()
    End Sub

    ''' <summary>
    ''' Compares two sparse matrices element by element and reports the result.
    ''' Returns True when they carry exactly the same non-zero entries.
    ''' </summary>
    Private Function CompareMatrices(a As SparseMatrix, b As SparseMatrix, title As String) As Boolean
        If a.RowDimension <> b.RowDimension OrElse a.ColumnDimension <> b.ColumnDimension Then
            Call Console.WriteLine($"[{title}] dimension mismatch: ({a.RowDimension}x{a.ColumnDimension}) vs ({b.RowDimension}x{b.ColumnDimension})")
            Return False
        End If

        Dim diffCount As Integer = 0

        For i As Integer = 0 To a.RowDimension - 1
            For j As Integer = 0 To a.ColumnDimension - 1
                Dim va As Double = a(i, j)
                Dim vb As Double = b(i, j)

                If va = 0.0 AndAlso vb = 0.0 Then
                    Continue For
                End If

                If Math.Abs(va - vb) > 1.0E-9 Then
                    diffCount += 1
                End If
            Next
        Next

        If diffCount = 0 Then
            Call Console.WriteLine($"[{title}] OK - {a.nnz} non-zeros match exactly.")
        Else
            Call Console.WriteLine($"[{title}] FAIL - {diffCount} differing entries.")
        End If

        Return diffCount = 0
    End Function
End Module
