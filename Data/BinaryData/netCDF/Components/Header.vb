﻿#Region "Microsoft.VisualBasic::25d62ff09bdcd334d5413aae5b78888c, Data\BinaryData\netCDF\Components\Header.vb"

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

    '   Total Lines: 107
    '    Code Lines: 41 (38.32%)
    ' Comment Lines: 52 (48.60%)
    '    - Xml Docs: 86.54%
    ' 
    '   Blank Lines: 14 (13.08%)
    '     File Size: 3.63 KB


    '     Class Header
    ' 
    '         Properties: dimensions, globalAttributes, recordDimension, variables, version
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: checkVariableIdConflicts
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data

Namespace Components

    ''' <summary>
    ''' Object with the fields:
    ''' 
    '''  + `recordDimension` Number with the length of record dimension
    '''  + `dimensions`: List of dimensions
    '''  + `globalAttributes`: List of global attributes
    '''  + `variables`: List of variables
    '''  
    ''' </summary>
    Public Class Header

#Region "Grammar constants"

        ''' <summary>
        ''' NC_DIMENSION = \x00 \x00 \x00 \x0A	
        ''' tag for list of dimensions
        ''' </summary>
        Public Const NC_DIMENSION = 10
        ''' <summary>
        ''' NC_VARIABLE = \x00 \x00 \x00 \x0B
        ''' tag for list of variables
        ''' </summary>
        Public Const NC_VARIABLE = 11
        ''' <summary>
        ''' NC_ATTRIBUTE = \x00 \x00 \x00 \x0C
        ''' tag for list of attributes
        ''' </summary>
        Public Const NC_ATTRIBUTE = 12
#End Region

        ''' <summary>
        ''' Number with the length of record dimension
        ''' </summary>
        ''' <returns></returns>
        Public Property recordDimension As recordDimension
        ''' <summary>
        ''' List of dimensions
        ''' </summary>
        ''' <returns></returns>
        Public Property dimensions As Dimension()
        ''' <summary>
        ''' List of global attributes
        ''' </summary>
        ''' <returns></returns>
        Public Property globalAttributes As attribute()
        ''' <summary>
        ''' List of variables
        ''' </summary>
        ''' <returns></returns>
        Public Property variables As variable()

        ''' <summary>
        ''' + \x01 classic format (CDF-1)
        ''' + \x02 64-bit offset format (CDF-2)
        ''' </summary>
        ''' <returns></returns>
        Public Property version As Byte

        Sub New()
        End Sub

        ''' <summary>
        ''' Read the header of the file
        ''' </summary>
        ''' <param name="buffer">Buffer for the file data</param>
        ''' <param name="version">Version of the file</param>
        Friend Sub New(buffer As BinaryDataReader, version As Byte)
            ' Length of record dimension
            ' sum of the varSize's of all the record variables.
            Me.recordDimension = New recordDimension With {.length = buffer.ReadUInt32}
            Me.version = version

            ' List of dimensions
            Dim dimList As DimensionList = buffer.dimensionsList()

            Me.recordDimension.id = If(dimList.recordId, dimList.recordId, -100)
            Me.recordDimension.name = dimList.recordName
            Me.dimensions = dimList.dimensions

            ' List of global attributes
            Me.globalAttributes = buffer.attributesList().ToArray

            ' List of variables
            Dim variables = buffer.variablesList(dimList.recordId, version)
            Me.variables = variables.variables
            Me.recordDimension.recordStep = variables.recordStep
        End Sub

        Public Iterator Function checkVariableIdConflicts() As IEnumerable(Of String)
            Dim uniqueId As New Index(Of String)

            For Each var As variable In variables
                If var.name Like uniqueId Then
                    Yield var.name
                Else
                    uniqueId.Add(var.name)
                End If
            Next
        End Function
    End Class
End Namespace
