#Region "Microsoft.VisualBasic::1f93735a29d102046e03b3eb29eeb5ba, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\FileIO\DynamicRow.vb"

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

    '   Total Lines: 35
    '    Code Lines: 15
    ' Comment Lines: 15
    '   Blank Lines: 5
    '     File Size: 1.11 KB


    '     Class DynamicRow
    ' 
    '         Properties: CellDefinitions, RowNumber
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer

Namespace XLSX.FileIO

    ''' <summary>
    ''' Class representing a row that is either empty or containing cells. Empty rows can also carry information about height or visibility
    ''' </summary>
    Friend Class DynamicRow
        ''' <summary>
        ''' Defines the cellDefinitions
        ''' </summary>
        Private cellDefinitionsField As List(Of Cell)

        ''' <summary>
        ''' Gets or sets the row number (zero-based)
        ''' </summary>
        Public Property RowNumber As Integer

        ''' <summary>
        ''' Gets the List of cells if not empty
        ''' </summary>
        Public ReadOnly Property CellDefinitions As List(Of Cell)
            Get
                Return cellDefinitionsField
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="DynamicRow"/> class
        ''' </summary>
        Public Sub New()
            cellDefinitionsField = New List(Of Cell)()
        End Sub
    End Class
End Namespace
