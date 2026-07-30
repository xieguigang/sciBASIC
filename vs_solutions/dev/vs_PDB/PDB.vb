#Region "Microsoft.VisualBasic::00000000000000000000000000000000, sciBASIC#\vs_solutions\dev\vs_PDB\PDB.vb"

    ' Copyright (c) 2018 GPL3 Licensed
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

#End Region

Imports System.IO

Namespace sciBASIC.PDB

    ''' <summary>
    ''' Unified entry point for reading PDB debug-symbol files. <see cref="Open"/> inspects the file
    ''' header and dispatches to the classic MSF reader (SuperBlock magic) or the Portable PDB reader
    ''' (DOS <c>MZ</c> header), then aggregates the result into one uniform debug-information model.
    ''' </summary>
    Public Class PDB

        ''' <summary>The physical container format of the parsed file.</summary>
        Public Enum FormatKind
            Classic
            Portable
        End Enum

        ''' <summary>Container format of the source file.</summary>
        Public Property Format As FormatKind

        ''' <summary>GUID / signature / age (classic MSF only; empty for Portable PDB).</summary>
        Public Property PdbInfo As PdbStreamInfo

        ''' <summary>Source files referenced by the symbols.</summary>
        Public ReadOnly Property SourceDocuments As New List(Of SourceDocument)()

        ''' <summary>Line-number / sequence-point records.</summary>
        Public ReadOnly Property LineNumbers As New List(Of LineInfo)()

        ''' <summary>Public / global symbols (classic MSF only; empty for Portable PDB).</summary>
        Public ReadOnly Property Symbols As New List(Of Symbol)()

        ''' <summary>Type records (classic MSF only; empty for Portable PDB).</summary>
        Public ReadOnly Property TypeRecords As New List(Of TypeRecord)()

        ''' <summary>
        ''' Open and parse a PDB file, auto-detecting its format.
        ''' </summary>
        ''' <exception cref="InvalidDataException">When the file is neither a Portable PE nor a classic MSF.</exception>
        Public Shared Function Open(path As String) As PDB
            Dim head(3) As Byte

            Using fs As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                If fs.Read(head, 0, 4) < 4 Then
                    Throw New InvalidDataException("File is too small to be a PDB.")
                End If
            End Using

            If head(0) = &H4D AndAlso head(1) = &H5A Then
                ' "MZ" -> managed PE that carries the CLI metadata.
                Return OpenPortable(path)
            ElseIf BitConverter.ToUInt32(head, 0) = &H424A5342UI Then
                ' "BSJB" -> standalone Portable PDB (metadata root at offset 0).
                Return OpenPortable(path)
            ElseIf MSFReader.IsMagic(path) Then
                Return OpenClassic(path)
            End If

            Throw New InvalidDataException("Not a recognized PDB file (neither Portable PDB nor classic MSF).")
        End Function

        Private Shared Function OpenPortable(path As String) As PDB
            Dim pdb As New PDB() With {.Format = FormatKind.Portable}

            Using reader As New PortablePdbReader(path)
                pdb.SourceDocuments.AddRange(reader.Documents)
                pdb.LineNumbers.AddRange(reader.LineNumbers)
            End Using

            Return pdb
        End Function

        Private Shared Function OpenClassic(path As String) As PDB
            Dim pdb As New PDB() With {.Format = FormatKind.Classic}

            Using msf As New MSFReader(path)
                pdb.PdbInfo = msf.PdbInfo

                ' DBI stream (#3): modules, source documents, line numbers.
                Dim dbiStream As Stream = msf.GetStream(MSFReader.StreamDbi)
                Dim dbi As DbiReader = Nothing

                If dbiStream IsNot Nothing Then
                    dbi = New DbiReader(dbiStream)
                    pdb.SourceDocuments.AddRange(dbi.SourceDocuments)
                    pdb.LineNumbers.AddRange(dbi.LineNumbers)
                End If

                ' TPI stream (#2): type records.
                Dim tpiStream As Stream = msf.GetStream(MSFReader.StreamTpi)

                If tpiStream IsNot Nothing Then
                    Dim tpi As New TpiReader(tpiStream)
                    pdb.TypeRecords.AddRange(tpi.TypeRecords)
                End If

                ' Public symbol stream (via DBI header).
                If dbi IsNot Nothing Then
                    Dim syms As New PublicSymbolReader(msf, dbi)
                    pdb.Symbols.AddRange(syms.Symbols)
                End If
            End Using

            Return pdb
        End Function
    End Class
End Namespace
