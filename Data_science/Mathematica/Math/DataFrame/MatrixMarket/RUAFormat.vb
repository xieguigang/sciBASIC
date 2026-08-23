' 
' Copyright (c) 2020 sciBASIC# development team
' 
' This composition is a part of the sciBASIC# project.
' 
' TEST LICENSE HERE 
' 
' </license>
' -----------------------------------------------------------------------
' 
' AUTHOR: xieguigang (https://github.com/xieguigang)
' 
' 2020-02-14
' 
' </author>
' -----------------------------------------------------------------------
' 
' 

Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace MatrixMarket

    ''' <summary>
    ''' Reader and writer for the Harwell-Boeing sparse matrix format,
    ''' specifically the <c>RUA</c> (Real, Unsymmetric, Assembled) variant.
    ''' 
    ''' The file is text based and uses a fixed column width layout:
    ''' 
    '''  - Line 1 : 72 chars title + 8 chars identifier
    '''  - Line 2 : totcrd ptrcrd indcrd valcrd rhscrd  (5 integers)
    '''  - Line 3 : mxtype(14) N M nz nrhs
    '''  - Line 4 : (ptrfmt) (indfmt) (valfmt)
    '''  - ptrcrd lines  : column pointers   (N + 1 integers, N = #columns)
    '''  - indcrd lines  : row indices       (nz integers)
    '''  - valcrd lines  : numerical values  (nz reals)
    ''' 
    ''' The stored matrix is column oriented (CSC). The internal
    ''' <see cref="SparseMatrix"/> representation is row oriented (CSR), so a
    ''' transposed index mapping is applied on both read and write.
    ''' </summary>
    Public Class RUAFormat

        ''' <summary>
        ''' The matrix type identifier that is written into the header.
        ''' </summary>
        Public Const MatrixType As String = "RUA"

        Const ptrPerLine As Integer = 10
        Const ptrWidth As Integer = 8
        Const indPerLine As Integer = 10
        Const indWidth As Integer = 8
        Const valPerLine As Integer = 4
        Const valWidth As Integer = 20

        ' +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        '  READ
        ' +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        Public Shared Function ReadMatrix(filepath As String) As SparseMatrix
            Using file As Stream = filepath.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Return ReadMatrix(file)
            End Using
        End Function

        Public Shared Function ReadMatrix(file As Stream) As SparseMatrix
            Using reader As New StreamReader(file)
                Return ReadMatrix(reader)
            End Using
        End Function

        Public Shared Function ReadMatrix(reader As StreamReader) As SparseMatrix
            ' ---- header (first 4 lines) --------------------------------------
            Dim titleLine As String = reader.ReadLine()
            Dim countLine As String = reader.ReadLine()
            Dim typeLine As String = reader.ReadLine()
            Dim fmtLine As String = reader.ReadLine()

            Dim counts As String() = countLine.Trim().Split(New Char() {" "c, vbTab}, StringSplitOptions.RemoveEmptyEntries)
            ' totcrd ptrcrd indcrd valcrd rhscrd
            Dim ptrcrd As Integer = Integer.Parse(counts(1))
            Dim indcrd As Integer = Integer.Parse(counts(2))
            Dim valcrd As Integer = Integer.Parse(counts(3))

            Dim typeTokens As String() = typeLine.Trim().Split(New Char() {" "c, vbTab}, StringSplitOptions.RemoveEmptyEntries)
            Dim N As Integer = Integer.Parse(typeTokens(1))  ' columns
            Dim M As Integer = Integer.Parse(typeTokens(2))  ' rows
            Dim nz As Integer = Integer.Parse(typeTokens(3)) ' non zeros

            ' ---- column pointers (N + 1) -------------------------------------
            Dim colptr As Integer() = ReadIntegers(reader, ptrcrd, ptrPerLine, ptrWidth, N + 1)

            ' ---- row indices (nz) --------------------------------------------
            Dim rowind As Integer() = ReadIntegers(reader, indcrd, indPerLine, indWidth, nz)

            ' ---- numerical values (nz) ---------------------------------------
            Dim values As Double() = ReadReals(reader, valcrd, valPerLine, valWidth, nz)

            ' ---- build sparse matrix (column major -> row major) -------------
            Dim matrix As New SparseMatrix(M, N)

            For c As Integer = 0 To N - 1
                Dim start As Integer = colptr(c) - 1
                Dim [end] As Integer = colptr(c + 1) - 1

                For k As Integer = start To [end] - 1
                    Dim r As Integer = rowind(k) - 1
                    matrix(r, c) = values(k)
                Next
            Next

            Return matrix
        End Function

        ''' <summary>
        ''' Reads fixed width integer fields from <paramref name="count"/> lines,
        ''' each holding <paramref name="perLine"/> fields of <paramref name="width"/>
        ''' characters, and returns exactly <paramref name="expected"/> values.
        ''' </summary>
        Private Shared Function ReadIntegers(reader As StreamReader, count As Integer, perLine As Integer, width As Integer, expected As Integer) As Integer()
            Dim list As New List(Of Integer)
            Dim sb As New StringBuilder(perLine * width)

            For i As Integer = 0 To count - 1
                sb.Clear()
                sb.Append(reader.ReadLine())

                For f As Integer = 0 To perLine - 1
                    Dim start As Integer = f * width

                    If start + width > sb.Length Then
                        Exit For
                    End If

                    Dim token As String = sb.ToString(start, width).Trim()

                    If token.Length > 0 Then
                        list.Add(Integer.Parse(token))
                    End If
                Next
            Next

            ' pad / truncate to the exact expected length
            If list.Count < expected Then
                For k As Integer = list.Count To expected - 1
                    list.Add(0)
                Next
            ElseIf list.Count > expected Then
                Call list.RemoveRange(expected, list.Count - expected)
            End If

            Return list.ToArray()
        End Function

        ''' <summary>
        ''' Reads fixed width real fields from <paramref name="count"/> lines,
        ''' each holding <paramref name="perLine"/> fields of <paramref name="width"/>
        ''' characters, and returns exactly <paramref name="expected"/> values.
        ''' </summary>
        Private Shared Function ReadReals(reader As StreamReader, count As Integer, perLine As Integer, width As Integer, expected As Integer) As Double()
            Dim list As New List(Of Double)
            Dim sb As New StringBuilder(perLine * width)

            For i As Integer = 0 To count - 1
                sb.Clear()
                sb.Append(reader.ReadLine())

                For f As Integer = 0 To perLine - 1
                    Dim start As Integer = f * width

                    If start + width > sb.Length Then
                        Exit For
                    End If

                    Dim token As String = sb.ToString(start, width).Trim()

                    If token.Length > 0 Then
                        list.Add(Double.Parse(token, CultureInfo.InvariantCulture))
                    End If
                Next
            Next

            If list.Count < expected Then
                For k As Integer = list.Count To expected - 1
                    list.Add(0.0)
                Next
            ElseIf list.Count > expected Then
                Call list.RemoveRange(expected, list.Count - expected)
            End If

            Return list.ToArray()
        End Function

        ' +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        '  WRITE
        ' +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        Public Shared Sub WriteMatrix(matrix As SparseMatrix, filepath As String)
            Using file As Stream = filepath.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                Call WriteMatrix(matrix, New StreamWriter(file))
            End Using
        End Sub

        Public Shared Sub WriteMatrix(matrix As SparseMatrix, writer As StreamWriter)
            Dim M As Integer = matrix.RowDimension
            Dim N As Integer = matrix.ColumnDimension

            ' convert row major (CSR) -> column major (CSC)
            Dim colptr(N) As Integer
            Dim rowind As New List(Of Integer)
            Dim values As New List(Of Double)

            colptr(0) = 1

            For c As Integer = 0 To N - 1
                Dim nnzInCol As Integer = 0

                For r As Integer = 0 To M - 1
                    Dim v As Double = matrix(r, c)

                    If v <> 0.0 Then
                        rowind.Add(r + 1)
                        values.Add(v)
                        nnzInCol += 1
                    End If
                Next

                colptr(c + 1) = colptr(c) + nnzInCol
            Next

            Dim nz As Integer = values.Count
            Dim ptrcrd As Integer = CInt(System.Math.Ceiling((N + 1) / CDbl(ptrPerLine)))
            Dim indcrd As Integer = CInt(System.Math.Ceiling(nz / CDbl(indPerLine)))
            Dim valcrd As Integer = CInt(System.Math.Ceiling(nz / CDbl(valPerLine)))
            Dim totcrd As Integer = 4 + ptrcrd + indcrd + valcrd

            ' ---- header -------------------------------------------------------
            writer.WriteLine("RUA sparse matrix (sciBASIC# writer)".PadRight(72) & "RUAWRITER")
            writer.WriteLine(String.Format("{0,14}{1,14}{2,14}{3,14}{4,14}", totcrd, ptrcrd, indcrd, valcrd, 0))
            writer.WriteLine(String.Format("{0,-14}{1,14}{2,14}{3,14}{4,14}", MatrixType, N, M, nz, 0))
            writer.WriteLine("(10I8)          (10I8)          (4E20.12)")

            ' ---- column pointers ---------------------------------------------
            Call WriteIntegers(writer, colptr, ptrPerLine, ptrWidth)
            ' ---- row indices -------------------------------------------------
            Call WriteIntegers(writer, rowind.ToArray(), indPerLine, indWidth)
            ' ---- numerical values --------------------------------------------
            Call WriteReals(writer, values.ToArray(), valPerLine, valWidth)

            writer.Flush()
        End Sub

        Private Shared Sub WriteIntegers(writer As StreamWriter, data As Integer(), perLine As Integer, width As Integer)
            For i As Integer = 0 To data.Length - 1 Step perLine
                Dim line As New StringBuilder(perLine * width)

                For f As Integer = 0 To perLine - 1
                    If i + f >= data.Length Then
                        Exit For
                    End If

                    line.Append(String.Format("{0," & width & "}", data(i + f)))
                Next

                writer.WriteLine(line.ToString())
            Next
        End Sub

        Private Shared Sub WriteReals(writer As StreamWriter, data As Double(), perLine As Integer, width As Integer)
            For i As Integer = 0 To data.Length - 1 Step perLine
                Dim line As New StringBuilder(perLine * width)

                For f As Integer = 0 To perLine - 1
                    If i + f >= data.Length Then
                        Exit For
                    End If

                    line.Append(FormatE20_12(data(i + f)))
                Next

                writer.WriteLine(line.ToString())
            Next
        End Sub

        ''' <summary>
        ''' Formats a double the way a Fortran <c>E20.12</c> field would:
        ''' 12 decimal digits, a 2 digit exponent, left padded to 20 columns.
        ''' </summary>
        Private Shared Function FormatE20_12(value As Double) As String
            ' "0.000000000000E+00" -> padded to 20 characters
            Dim s As String = value.ToString("0.000000000000E+00", CultureInfo.InvariantCulture)
            Return s.PadLeft(valWidth)
        End Function
    End Class
End Namespace
