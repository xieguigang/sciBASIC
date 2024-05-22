#Region "Microsoft.VisualBasic::1c9870413b86fc3373ffd903d1a2c90e, Data\BinaryData\Feather\RowMap.vb"

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

    '   Total Lines: 51
    '    Code Lines: 33 (64.71%)
    ' Comment Lines: 11 (21.57%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 7 (13.73%)
    '     File Size: 1.68 KB


    ' Class RowMap
    ' 
    '     Properties: Count
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System

''' <summary>
''' Utility class for addressing a dataframe's rows.
''' </summary>
Public Class RowMap
    Private Parent As DataFrame

    ''' <summary>
    ''' Number of rows in the dataframe
    ''' </summary>
    Public ReadOnly Property Count As Long
        Get
            Return Parent.RowCount
        End Get
    End Property

    ''' <summary>
    ''' Returns the row at the given index (in the dataframe's basis).
    ''' 
    ''' Throws if the index is out of range.
    ''' </summary>
    Default Public ReadOnly Property Item(index As Long) As Row
        Get
            Dim translatedIndex = Parent.TranslateIndex(index)

            If translatedIndex < 0 OrElse translatedIndex >= Parent.Metadata.NumRows Then
                Dim minLegal As Long
                Dim maxLegal As Long
                Select Case Parent.Basis
                    Case BasisType.One
                        minLegal = 1
                        maxLegal = Parent.Metadata.NumRows
                    Case BasisType.Zero
                        minLegal = 0
                        maxLegal = Parent.Metadata.NumRows - 1
                    Case Else
                        Throw New InvalidOperationException($"Unexpected Basis: {Parent.Basis}")
                End Select

                Throw New ArgumentOutOfRangeException(NameOf(index), $"Row index out of range, valid between [{minLegal}, {maxLegal}] found {index}")
            End If

            Return New Row(Parent, translatedIndex)
        End Get
    End Property

    Friend Sub New(parent As DataFrame)
        Me.Parent = parent
    End Sub
End Class
