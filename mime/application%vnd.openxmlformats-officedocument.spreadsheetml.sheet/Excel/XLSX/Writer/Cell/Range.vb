Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer.Cell

Namespace XLSX.Writer

    ''' <summary>
    ''' Struct representing a cell range with a start and end address
    ''' </summary>
    Public Structure Range
        ''' <summary>
        ''' End address of the range
        ''' </summary>
        Public EndAddress As Address

        ''' <summary>
        ''' Start address of the range
        ''' </summary>
        Public StartAddress As Address

        ''' <summary>
        ''' Initializes a new instance of the <see cref=""/> class
        ''' </summary>
        ''' <param name="start">Start address of the range.</param>
        ''' <param name="end">End address of the range.</param>
        Public Sub New(start As Address, [end] As Address)
            If start.CompareTo([end]) < 0 Then
                StartAddress = start
                EndAddress = [end]
            Else
                StartAddress = [end]
                EndAddress = start
            End If
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref=""/> class
        ''' </summary>
        ''' <param name="range">Address range (e.g. 'A1:B12').</param>
        Public Sub New(range As String)
            Dim r = ResolveCellRange(range)
            If r.StartAddress.CompareTo(r.EndAddress) < 0 Then
                StartAddress = r.StartAddress
                EndAddress = r.EndAddress
            Else
                StartAddress = r.EndAddress
                EndAddress = r.StartAddress
            End If
        End Sub

        ''' <summary>
        ''' Gets a list of all addresses between the start and end address
        ''' </summary>
        ''' <returns>List of Addresses.</returns>
        Public Function ResolveEnclosedAddresses() As IReadOnlyList(Of Address)
            Dim startColumn, endColumn, startRow, endRow As Integer
            If StartAddress.Column <= EndAddress.Column Then
                startColumn = StartAddress.Column
                endColumn = EndAddress.Column
            Else
                endColumn = StartAddress.Column
                startColumn = EndAddress.Column
            End If
            If StartAddress.Row <= EndAddress.Row Then
                startRow = StartAddress.Row
                endRow = EndAddress.Row
            Else
                endRow = StartAddress.Row
                startRow = EndAddress.Row
            End If
            Dim addresses As List(Of Address) = New List(Of Address)()
            For c = startColumn To endColumn
                For r = startRow To endRow
                    addresses.Add(New Address(c, r))
                Next
            Next
            Return addresses
        End Function

        ''' <summary>
        ''' Overwritten ToString method
        ''' </summary>
        ''' <returns>Returns the range (e.g. 'A1:B12').</returns>
        Public Overrides Function ToString() As String
            Return StartAddress.ToString() & ":" & EndAddress.ToString()
        End Function

        ''' <summary>
        ''' Creates a (dereferenced, if applicable) deep copy of this range
        ''' </summary>
        ''' <returns>Copy of this range.</returns>
        Friend Function Copy() As Range
            Return New Range(StartAddress.Copy(), EndAddress.Copy())
        End Function

        ''' <summary>
        ''' Compares two objects whether they are ranges and equal. The cell types (possible $ prefix) are considered
        ''' </summary>
        ''' <param name="obj">Other object to compare.</param>
        ''' <returns>True if the two objects are the same range.</returns>
        Public Overrides Function Equals(obj As Object) As Boolean
            If Not (TypeOf obj Is Range) Then
                Return False
            End If
            Dim other As Range = obj
            Return StartAddress.Equals(other.StartAddress) AndAlso EndAddress.Equals(other.EndAddress)
        End Function

        ''' <summary>
        ''' Gets the hash code of the range object according to its string representation
        ''' </summary>
        ''' <returns>Hash code of the range.</returns>
        Public Overrides Function GetHashCode() As Integer
            Return ToString().GetHashCode()
        End Function


        ' Operator overloads
        Public Shared Operator =(range1 As Range, range2 As Range) As Boolean
            Return range1.Equals(range2)
        End Operator

        Public Shared Operator <>(range1 As Range, range2 As Range) As Boolean
            Return Not range1.Equals(range2)
        End Operator
    End Structure

End Namespace