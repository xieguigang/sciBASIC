#Region "Microsoft.VisualBasic::de2ee2c1301bfbe106deb21938fa0887, Data\BinaryData\Feather\ColumnMap.vb"

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

    '   Total Lines: 71
    '    Code Lines: 46 (64.79%)
    ' Comment Lines: 16 (22.54%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 9 (12.68%)
    '     File Size: 2.39 KB


    ' Class ColumnMap
    ' 
    '     Properties: Count
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic

''' <summary>
''' Utility class for addressing a dataframes columns.
''' </summary>
Public Class ColumnMap
    Private Parent As DataFrame

    ''' <summary>
    ''' Number of columns in the dataframe
    ''' </summary>
    Public ReadOnly Property Count As Long
        Get
            Return Parent.ColumnCount
        End Get
    End Property

    ''' <summary>
    ''' Returns the column at the given index (in the dataframe's basis).
    ''' 
    ''' Throws if the index is out of range.
    ''' </summary>
    Default Public ReadOnly Property Item(index As Long) As Column
        Get
            Dim translatedIndex = Parent.TranslateIndex(index)

            If translatedIndex < 0 OrElse translatedIndex >= Parent.Metadata.Columns.Length Then
                Dim minLegal As Long
                Dim maxLegal As Long
                Select Case Parent.Basis
                    Case BasisType.One
                        minLegal = 1
                        maxLegal = Parent.Metadata.Columns.Length
                    Case BasisType.Zero
                        minLegal = 0
                        maxLegal = Parent.Metadata.Columns.Length - 1
                    Case Else
                        Throw New InvalidOperationException($"Unexpected Basis: {Parent.Basis}")
                End Select

                Throw New ArgumentOutOfRangeException(NameOf(index), $"Column index out of range, valid between [{minLegal}, {maxLegal}] found {index}")
            End If

            Return New Column With {
.Parent = Parent,
.TranslatedColumnIndex = translatedIndex
}
        End Get
    End Property

    ''' <summary>
    ''' Returns the column with the given name.
    ''' 
    ''' Throws if no column has the given name.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Column
        Get
            Dim translatedIndex As Long
            If Not Parent.TryLookupTranslatedColumnIndex(columnName, translatedIndex) Then
                Throw New KeyNotFoundException($"Could not find column with name ""{columnName}""")
            End If

            Return New Column(Parent, translatedIndex)
        End Get
    End Property

    Friend Sub New(parent As DataFrame)
        Me.Parent = parent
    End Sub
End Class
