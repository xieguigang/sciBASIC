﻿#Region "Microsoft.VisualBasic::82133f0590bbadd27af2edfad0d3d049, Data\BinaryData\Feather\Impl\IDataFrame.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 885 B


    '     Interface IDataFrame
    ' 
    '         Properties: Basis, ColumnCount, (+2 Overloads) Item, RowCount
    ' 
    '         Function: (+4 Overloads) TryGetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices

Namespace Impl
    Friend Interface IDataFrame
        ReadOnly Property RowCount As Long
        ReadOnly Property ColumnCount As Long

        ReadOnly Property Basis As BasisType

        Default ReadOnly Property Item(rowIndex As Long, columnIndex As Long) As Value
        Default ReadOnly Property Item(rowIndex As Long, columnName As String) As Value

        Function TryGetValue(rowIndex As Long, columnIndex As Long, <Out> ByRef value As Value) As Boolean
        Function TryGetValue(Of T)(rowIndex As Long, columnIndex As Long, <Out> ByRef value As T) As Boolean

        Function TryGetValue(rowIndex As Long, columnName As String, <Out> ByRef value As Value) As Boolean
        Function TryGetValue(Of T)(rowIndex As Long, columnName As String, <Out> ByRef value As T) As Boolean
    End Interface
End Namespace
