#Region "Microsoft.VisualBasic::9d4e9607afc9e8796d990df89034e490, Data\BinaryData\Feather\Impl\IRow.vb"

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

    '   Total Lines: 21
    '    Code Lines: 16
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 949 B


    '     Interface IRow
    ' 
    '         Properties: Index, Length
    ' 
    '         Function: GetRange, ToArray, (+4 Overloads) TryGetValue
    ' 
    '         Sub: (+2 Overloads) GetRange, ToArray
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices

Namespace Impl
    Friend Interface IRow
        ReadOnly Property Index As Long
        ReadOnly Property Length As Long

        Function TryGetValue(columnIndex As Long, <Out> ByRef value As Value) As Boolean
        Function TryGetValue(Of T)(columnIndex As Long, <Out> ByRef value As T) As Boolean

        Function TryGetValue(columnName As String, <Out> ByRef value As Value) As Boolean
        Function TryGetValue(Of T)(columnName As String, <Out> ByRef value As T) As Boolean

        Function ToArray() As Value()
        Function GetRange(columnIndex As Long, length As Integer) As Value()

        Sub ToArray(ByRef array As Value())
        Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value())
        Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer)
    End Interface
End Namespace
