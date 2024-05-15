#Region "Microsoft.VisualBasic::526b174882318115de7c206d8a4f6f0f, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\FileIO\SortedMap.vb"

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

    '   Total Lines: 66
    '    Code Lines: 29
    ' Comment Lines: 28
    '   Blank Lines: 9
    '     File Size: 2.11 KB


    '     Class SortedMap
    ' 
    '         Properties: Count, Keys
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace XLSX.FileIO

    ''' <summary>
    ''' Class to manage key value pairs (string / string). The entries are in the order how they were added
    ''' </summary>
    Public Class SortedMap

        ''' <summary>
        ''' Defines the keyEntries
        ''' </summary>
        Private ReadOnly keyEntries As List(Of String)

        ''' <summary>
        ''' Defines the valueEntries
        ''' </summary>
        Private ReadOnly valueEntries As List(Of String)

        ''' <summary>
        ''' Defines the index
        ''' </summary>
        Private ReadOnly index As Dictionary(Of String, Integer)

        ''' <summary>
        ''' Gets the Count
        ''' Number of map entries
        ''' </summary>
        Public ReadOnly Property Count As Integer

        ''' <summary>
        ''' Gets the keys of the map as list
        ''' </summary>
        Public ReadOnly Property Keys As IEnumerable(Of String)
            Get
                Return keyEntries
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SortedMap"/> class
        ''' </summary>
        Public Sub New()
            keyEntries = New List(Of String)()
            valueEntries = New List(Of String)()
            index = New Dictionary(Of String, Integer)()
            Count = 0
        End Sub

        ''' <summary>
        ''' Method to add a key value pair
        ''' </summary>
        ''' <param name="key">Key as string.</param>
        ''' <param name="value">Value as string.</param>
        ''' <returns>Returns the resolved string (either added or returned from an existing entry).</returns>
        Public Function Add(key As String, value As String) As String
            If index.ContainsKey(key) Then
                Return valueEntries(index(key))
            End If
            index.Add(key, Count)
            _Count += 1
            keyEntries.Add(key)
            valueEntries.Add(value)
            Return value
        End Function
    End Class

End Namespace
