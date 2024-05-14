#Region "Microsoft.VisualBasic::dc1a137222af92febb8a654d8ad985ef, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Cell\Address.vb"

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

    '   Total Lines: 131
    '    Code Lines: 56
    ' Comment Lines: 59
    '   Blank Lines: 16
    '     File Size: 5.12 KB


    '     Structure Address
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CompareTo, Copy, (+2 Overloads) Equals, GetAddress, GetColumn
    '                   GetHashCode, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace XLSX.Writer

    ''' <summary>
    ''' Struct representing the cell address as column and row (zero based)
    ''' </summary>
    Public Structure Address
        Implements IEquatable(Of Address), IComparable(Of Address)
        ''' <summary>
        ''' Column number (zero based)
        ''' </summary>
        Public Column As Integer

        ''' <summary>
        ''' Row number (zero based)
        ''' </summary>
        Public Row As Integer

        ''' <summary>
        ''' Referencing type of the address
        ''' </summary>
        Public Type As AddressType

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Address"/> class
        ''' </summary>
        ''' <param name="column">Column number (zero based).</param>
        ''' <param name="row">Row number (zero based).</param>
        ''' <param name="type">Optional referencing type of the address.</param>
        Public Sub New(column As Integer, row As Integer, Optional type As AddressType = AddressType.Default)
            Me.Column = column
            Me.Row = row
            Me.Type = type
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Address"/> class
        ''' </summary>
        ''' <param name="address">Address string (e.g. 'A1:B12').</param>
        ''' <param name="type">Optional referencing type of the address.</param>
        Public Sub New(address As String, Optional type As AddressType = AddressType.Default)
            Me.Type = type
            Cell.ResolveCellCoordinate(address, Column, Row, type)
        End Sub

        ''' <summary>
        ''' Returns the combined Address
        ''' </summary>
        ''' <returns>Address as string in the format A1 - XFD1048576.</returns>
        Public Function GetAddress() As String
            Return Cell.ResolveCellAddress(Column, Row, Type)
        End Function

        ''' <summary>
        ''' Gets the column address (A - XFD)
        ''' </summary>
        ''' <returns>Column address as letter(s).</returns>
        Public Function GetColumn() As String
            Return Cell.ResolveColumnAddress(Column)
        End Function

        ''' <summary>
        ''' Overwritten ToString method
        ''' </summary>
        ''' <returns>Returns the cell address (e.g. 'A15').</returns>
        Public Overrides Function ToString() As String
            Return GetAddress()
        End Function

        ''' <summary>
        ''' Compares two addresses whether they are equal
        ''' </summary>
        ''' <param name="o"> Other address.</param>
        ''' <returns>True if equal.</returns>
        Public Overloads Function Equals(o As Address) As Boolean Implements IEquatable(Of Address).Equals
            If Row = o.Row AndAlso Column = o.Column Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Compares two objects whether they are addresses and equal
        ''' </summary>
        ''' <param name="obj"> Other address.</param>
        ''' <returns>True if not null, of the same type and equal.</returns>
        Public Overrides Function Equals(obj As Object) As Boolean
            If Not (TypeOf obj Is Address) Then
                Return False
            End If
            Return Equals(CType(obj, Address))
        End Function

        ''' <summary>
        ''' Gets the hash code based on the string representation of the address
        ''' </summary>
        ''' <returns>Hash code of the address.</returns>
        Public Overrides Function GetHashCode() As Integer
            Return ToString().GetHashCode()
        End Function


        ' Operator overloads
        Public Shared Operator =(address1 As Address, address2 As Address) As Boolean
            Return address1.Equals(address2)
        End Operator

        Public Shared Operator <>(address1 As Address, address2 As Address) As Boolean
            Return Not address1.Equals(address2)
        End Operator
        ''' <summary>
        ''' Compares two addresses using the column and row numbers
        ''' </summary>
        ''' <param name="other"> Other address.</param>
        ''' <returns>-1 if the other address is greater, 0 if equal and 1 if smaller.</returns>
        Public Function CompareTo(other As Address) As Integer Implements IComparable(Of Address).CompareTo
            Dim thisCoordinate = Column * CLng(Worksheet.MAX_ROW_NUMBER) + Row
            Dim otherCoordinate = other.Column * CLng(Worksheet.MAX_ROW_NUMBER) + other.Row
            Return thisCoordinate.CompareTo(otherCoordinate)
        End Function

        ''' <summary>
        ''' Creates a (dereferenced, if applicable) deep copy of this address
        ''' </summary>
        ''' <returns>Copy of this range.</returns>
        Friend Function Copy() As Address
            Return New Address(Column, Row, Type)
        End Function
    End Structure

End Namespace
