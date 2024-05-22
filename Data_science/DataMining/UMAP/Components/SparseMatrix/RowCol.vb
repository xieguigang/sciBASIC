#Region "Microsoft.VisualBasic::572bfff23f1ca1f98a0fc5d9875d3869, Data_science\DataMining\UMAP\Components\SparseMatrix\RowCol.vb"

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

    '   Total Lines: 45
    '    Code Lines: 27 (60.00%)
    ' Comment Lines: 10 (22.22%)
    '    - Xml Docs: 40.00%
    ' 
    '   Blank Lines: 8 (17.78%)
    '     File Size: 1.45 KB


    ' Structure RowCol
    ' 
    '     Properties: Col, Row
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: (+2 Overloads) Equals, GetHashCode, ToString
    ' 
    ' /********************************************************************************/

#End Region

Friend Structure RowCol : Implements IEquatable(Of RowCol)

    Public ReadOnly Property Row As Integer
    Public ReadOnly Property Col As Integer

    Public Sub New(row As Integer, col As Integer)
        Me.Row = row
        Me.Col = col
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{Row}, {Col}]"
    End Function

    ' 2019-06-24 DWR: Structs get default Equals and GetHashCode
    ' implementations but they can be slow - having these versions
    ' makes the code run much quicker and it seems a good practice
    ' to throw in IEquatable<RowCol> to avoid boxing when Equals
    ' is called
    Public Overloads Function Equals(other As RowCol) As Boolean Implements IEquatable(Of RowCol).Equals
        Return other.Row = Row AndAlso other.Col = Col
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean
        If Not TypeOf obj Is RowCol Then
            Return False
        Else
            Return DirectCast(obj, RowCol).Equals(Me)
        End If
    End Function

    ''' <summary>
    ''' Courtesy of https://stackoverflow.com/a/263416/3813189
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function GetHashCode() As Integer
        Dim hash = 17

        ' Overflow is fine, just wrap
        hash = hash * 23 + Row
        hash = hash * 23 + Col

        Return hash
    End Function
End Structure
