#Region "Microsoft.VisualBasic::5c316159f767edb476e5411ddd5e3ce9, Data\DataFrame\DataSet\MatrixProvider.vb"

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

    '   Total Lines: 67
    '    Code Lines: 46 (68.66%)
    ' Comment Lines: 7 (10.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (20.90%)
    '     File Size: 2.14 KB


    '     Interface MatrixProvider
    ' 
    '         Function: GetMatrix
    ' 
    '     Class DataTable
    ' 
    '         Properties: RowNames
    ' 
    '         Function: GetMatrix, NewRow, WriteCsv
    ' 
    '         Sub: (+2 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Text

Namespace DATA

    ''' <summary>
    ''' A numeric data matrix provider
    ''' </summary>
    Public Interface MatrixProvider

        ''' <summary>
        ''' populate the matrix data in row by row
        ''' </summary>
        ''' <returns></returns>
        Function GetMatrix() As IEnumerable(Of DataSet)

    End Interface

    Public Class DataTable : Implements MatrixProvider

        ReadOnly rowIndex As New Dictionary(Of String, DataSet)
        ReadOnly rowKeys As New List(Of String)

        Default Public ReadOnly Property Row(id As String) As DataSet
            Get
                Return rowIndex(id)
            End Get
        End Property

        Default Public ReadOnly Property Row(index As Integer) As DataSet
            Get
                Return rowIndex(rowKeys(index))
            End Get
        End Property

        Public ReadOnly Property RowNames As String()
            Get
                Return rowKeys.ToArray
            End Get
        End Property

        Public Sub Add(row As DataSet)
            Call rowIndex.Add(row.ID, row)
            Call rowKeys.Add(row.ID)
        End Sub

        Public Sub Add(id As String, data As Dictionary(Of String, Double))
            Call rowIndex.Add(id, New DataSet(id, data))
            Call rowKeys.Add(id)
        End Sub

        Public Function NewRow(id As String) As DataSet
            Call Add(id, New Dictionary(Of String, Double))
            Return Me(id)
        End Function

        Public Function GetMatrix() As IEnumerable(Of DataSet) Implements MatrixProvider.GetMatrix
            Return rowIndex.Values
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function WriteCsv(file As String, Optional encoding As Encodings = Encodings.UTF8WithoutBOM) As Boolean
            Return GetMatrix.SaveTo(file, encoding:=encoding.CodePage, silent:=True)
        End Function
    End Class
End Namespace
