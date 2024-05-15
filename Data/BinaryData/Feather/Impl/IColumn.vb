#Region "Microsoft.VisualBasic::958c568fbe6501264a7d00fb55c968f5, Data\BinaryData\Feather\Impl\IColumn.vb"

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

    '   Total Lines: 33
    '    Code Lines: 26
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.58 KB


    '     Interface IColumn
    ' 
    '         Properties: Index, Length, Name, Type
    ' 
    '         Function: GetRange, ToArray, (+2 Overloads) TryGetValue, TryGetValueCell
    ' 
    '         Sub: (+4 Overloads) GetRange, (+2 Overloads) GetRangeValue, (+2 Overloads) ToArray, ToArrayValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Runtime.InteropServices

Namespace Impl
    Friend Interface IColumn(Of T)
        Inherits IList(Of T) ' implement so Linq's ElementAt is as fast as you'd expect
        ReadOnly Property Index As Long
        ReadOnly Property Name As String
        ReadOnly Property Type As Type
        ReadOnly Property Length As Long

        Function GetRange(rowIndex As Long, length As Integer) As T()

        Function ToArray() As T()

        Sub ToArray(ByRef array As T())
        Sub ToArray(Of V)(ByRef array As V())
        Sub ToArrayValue(ByRef array As Value())

        Sub GetRange(rowSourceIndex As Long, length As Integer, ByRef array As T())
        Sub GetRange(Of V)(rowSourceIndex As Long, length As Integer, ByRef array As V())
        Sub GetRangeValue(rowSourceIndex As Long, length As Integer, ByRef array As Value())

        Sub GetRange(rowSourceIndex As Long, length As Integer, ByRef array As T(), destinationIndex As Integer)
        Sub GetRange(Of V)(rowSourceIndex As Long, length As Integer, ByRef array As V(), destinationIndex As Integer)
        Sub GetRangeValue(rowSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer)

        Function TryGetValue(rowIndex As Long, <Out> ByRef value As T) As Boolean
        Function TryGetValue(Of V)(rowIndex As Long, <Out> ByRef value As V) As Boolean
        Function TryGetValueCell(rowIndex As Long, <Out> ByRef value As Value) As Boolean
    End Interface
End Namespace

