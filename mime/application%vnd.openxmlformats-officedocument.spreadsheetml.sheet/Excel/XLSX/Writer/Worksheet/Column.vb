#Region "Microsoft.VisualBasic::db15e3e2d6d70444c057f8ea84d8703a, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Worksheet\Column.vb"

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

    '   Total Lines: 121
    '    Code Lines: 61 (50.41%)
    ' Comment Lines: 47 (38.84%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (10.74%)
    '     File Size: 4.21 KB


    '     Class Column
    ' 
    '         Properties: ColumnAddress, HasAutoFilter, IsHidden, Number, Width
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Copy
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace XLSX.Writer

    ''' <summary>
    ''' Class representing a column of a worksheet
    ''' </summary>
    Public Class Column

        ''' <summary>
        ''' Defines the number
        ''' </summary>
        Private numberField As Integer

        ''' <summary>
        ''' Defines the columnAddress
        ''' </summary>
        Private columnAddressField As String

        ''' <summary>
        ''' Defines the width
        ''' </summary>
        Private widthField As Single

        ''' <summary>
        ''' Gets or sets the ColumnAddress
        ''' Column address (A to XFD)
        ''' </summary>
        Public Property ColumnAddress As String
            Get
                Return columnAddressField
            End Get
            Set(value As String)
                If String.IsNullOrEmpty(value) Then
                    Throw New RangeException("A general range exception occurred", "The passed address was null or empty")
                End If
                numberField = Cell.ResolveColumn(value)
                columnAddressField = value.ToUpper()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether HasAutoFilter
        ''' If true, the column has auto filter applied, otherwise not
        ''' </summary>
        Public Property HasAutoFilter As Boolean

        ''' <summary>
        ''' Gets or sets a value indicating whether IsHidden
        ''' If true, the column is hidden, otherwise visible
        ''' </summary>
        Public Property IsHidden As Boolean

        ''' <summary>
        ''' Gets or sets the Number
        ''' Column number (0 to 16383)
        ''' </summary>
        Public Property Number As Integer
            Get
                Return numberField
            End Get
            Set(value As Integer)
                columnAddressField = Cell.ResolveColumnAddress(value)
                numberField = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the Width
        ''' Width of the column
        ''' </summary>
        Public Property Width As Single
            Get
                Return widthField
            End Get
            Set(value As Single)
                If value < Worksheet.MIN_COLUMN_WIDTH OrElse value > Worksheet.MAX_COLUMN_WIDTH Then
                    Throw New RangeException("A general range exception occurred", "The passed column width is out of range (" & Worksheet.MIN_COLUMN_WIDTH.ToString() & " to " & Worksheet.MAX_COLUMN_WIDTH.ToString() & ")")
                End If
                widthField = value
            End Set
        End Property

        ''' <summary>
        ''' Prevents a default instance of the <see cref="Column"/> class from being created
        ''' </summary>
        Private Sub New()
            Width = Worksheet.DEFAULT_COLUMN_WIDTH
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Column"/> class
        ''' </summary>
        ''' <param name="columnCoordinate">Column number (zero-based, 0 to 16383).</param>
        Public Sub New(columnCoordinate As Integer)
            Me.New()
            Number = columnCoordinate
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Column"/> class
        ''' </summary>
        ''' <param name="columnAddress">Column address (A to XFD).</param>
        Public Sub New(columnAddress As String)
            Me.New()
            Me.ColumnAddress = columnAddress
        End Sub

        ''' <summary>
        ''' Creates a deep copy of this column
        ''' </summary>
        ''' <returns>Copy of this column.</returns>
        Friend Function Copy() As Column
            Dim lCopy As Column = New Column()
            lCopy.IsHidden = IsHidden
            lCopy.Width = widthField
            lCopy.HasAutoFilter = HasAutoFilter
            lCopy.columnAddressField = columnAddressField
            lCopy.numberField = numberField
            Return lCopy
        End Function
    End Class
End Namespace
