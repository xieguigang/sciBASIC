#Region "Microsoft.VisualBasic::903d2eaece9cdb3baa0319f3321f967c, Data\BinaryData\netCDF\Data\StructureParser.vb"

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

    '   Total Lines: 230
    '    Code Lines: 117 (50.87%)
    ' Comment Lines: 80 (34.78%)
    '    - Xml Docs: 62.50%
    ' 
    '   Blank Lines: 33 (14.35%)
    '     File Size: 8.99 KB


    '     Module StructureParser
    ' 
    '         Function: attributesList, dimensionsList, readVariableInternal, variablesList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.Language

Namespace Data

    ''' <summary>
    ''' 在这个模块之中进行CDF文件的每一个Section的解析操作
    ''' </summary>
    Module StructureParser

        Const NC_UNLIMITED = 0
        Const ZERO = 0

        ''' <summary>
        ''' List of dimensions
        ''' </summary>
        ''' <param name="buffer">Buffer for the file data</param>
        ''' <returns>
        ''' Ojbect containing the following properties:
        ''' 
        '''  + `dimensions` that Is an array of dimension object
        '''  + `name` String with the name of the dimension
        '''  + `size`: Number with the size of the dimension dimensions: dimensions
        '''  + `recordId`: the id Of the dimension that has unlimited size Or undefined,
        '''  + `recordName`: name of the dimension that has unlimited size
        '''  
        ''' </returns>
        ''' 
        <Extension>
        Friend Function dimensionsList(buffer As BinaryDataReader) As DimensionList
            Dim dimList = buffer.ReadUInt32()

            If (dimList = ZERO) Then
                Utils.notNetcdf((buffer.ReadUInt32() <> ZERO), "wrong empty tag for list of dimensions")
                Return Nothing
            Else
                Utils.notNetcdf((dimList <> Header.NC_DIMENSION), "wrong tag for list of dimensions")
            End If

            Dim recordId%? = Nothing
            Dim recordName$ = "NA"
            ' Length of dimensions
            Dim dimensionSize = buffer.ReadUInt32()
            Dim dimensions As New List(Of Dimension)

            For [dim] As Integer = 0 To dimensionSize - 1
                ' Read name
                Dim name = Utils.readName(buffer)
                ' Read dimension size
                Dim size = buffer.ReadUInt32()

                If (size = NC_UNLIMITED) Then
                    ' In netcdf 3 one field can be Of size unlimmited
                    recordId = [dim]
                    recordName = name
                End If

                dimensions += New Dimension With {
                    .name = name,
                    .size = size
                }
            Next

            Return New DimensionList With {
                .dimensions = dimensions,
                .recordId = recordId,
                .recordName = recordName
            }
        End Function

        ''' <summary>
        ''' List of attributes
        ''' </summary>
        ''' <param name="buffer">Buffer for the file data</param>
        ''' <returns>
        ''' List of attributes with:
        ''' 
        '''  + `name` String with the name of the attribute
        '''  + `type`: String with the type of the attribute
        '''  + `value`: A number Or String With the value Of the attribute
        '''  
        ''' </returns>
        ''' 
        <Extension>
        Friend Iterator Function attributesList(buffer As BinaryDataReader) As IEnumerable(Of attribute)
            Dim gAttList = buffer.ReadUInt32()

            If (gAttList = ZERO) Then
                Utils.notNetcdf((buffer.ReadUInt32() <> ZERO), "wrong empty tag for list of attributes")
                Return
            Else
                Utils.notNetcdf((gAttList <> Header.NC_ATTRIBUTE), "wrong tag for list of attributes")
            End If

            ' Length of attributes
            Dim attributeSize = buffer.ReadUInt32()

            For gAtt As Integer = 0 To attributeSize - 1
                ' Read name
                Dim name = Utils.readName(buffer)
                ' Read type
                Dim type As CDFDataTypes = buffer.ReadUInt32()

                Call Utils.notNetcdf(type < 1, $"non valid type {type}")

                ' Read attribute
                Dim size As Integer = buffer.ReadUInt32()
                Dim val As Object = Utils.readType(buffer, type, size)

                ' Apply padding
                Call Utils.padding(buffer)

                If TypeOf val Is Boolean() Then
                    val = val(Scan0)
                End If

                Yield New attribute With {
                    .name = name,
                    .type = type,
                    .value = val
                }
            Next
        End Function

        ''' <summary>
        ''' List of variables
        ''' </summary>
        ''' <param name="buffer">Buffer for the file data</param>
        ''' <param name="recordId%">Id of the unlimited dimension (also called record dimension)
        ''' This value may be undefined if there Is no unlimited dimension</param>
        ''' <param name="version">Version of the file</param>
        ''' <returns>
        ''' Number of recordStep And list of variables with:
        ''' 
        '''  + `name`: String with the name of the variable
        '''  + `dimensions`: Array with the dimension IDs of the variable
        '''  + `attributes`: Array with the attributes of the variable
        '''  + `type`: String with the type of the variable
        '''  + `size`: Number with the size of the variable
        '''  + `offset`: Number with the offset where of the variable begins
        '''  + `record`: True if Is a record variable, false otherwise (unlimited size)
        '''  </returns>
        '''  
        <Extension>
        Friend Function variablesList(buffer As BinaryDataReader, recordId%?, version As Byte) As (variables As variable(), recordStep%)
            Dim varList = buffer.ReadUInt32()
            Dim recordStep As Integer = 0

            If (varList = ZERO) Then
                Utils.notNetcdf((buffer.ReadUInt32() <> ZERO), "wrong empty tag for list of variables")
                Return Nothing
            Else
                Utils.notNetcdf((varList <> Header.NC_VARIABLE), "wrong tag for list of variables")
            End If

            ' Length of variables
            Dim variableSize = buffer.ReadUInt32()
            Dim variables As New List(Of variable)

            For v As Integer = 0 To variableSize - 1
                variables += buffer.readVariableInternal(recordId, version, recordStep)
            Next

            Return (variables:=variables, recordStep:=recordStep)
        End Function

        ''' <summary>
        ''' var = name nelems [dimid ...] vatt_list nc_type vsize begin	
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <param name="recordId%"></param>
        ''' <param name="version"></param>
        ''' <param name="recordStep%"></param>
        ''' <returns></returns>
        <Extension>
        Private Function readVariableInternal(buffer As BinaryDataReader, recordId%?, version As Byte, ByRef recordStep%) As variable
            ' Read name
            Dim name = Utils.readName(buffer)
            ' Read dimensionality of the variable
            Dim dimensionality = buffer.ReadUInt32()
            ' Index into the list of dimensions
            Dim dimensionsIds = New Integer(dimensionality - 1) {}

            For [dim] As Integer = 0 To dimensionality - 1
                dimensionsIds([dim]) = buffer.ReadUInt32()
            Next

            ' Read variables size
            Dim attributes = attributesList(buffer).ToArray
            ' Read type
            Dim type As CDFDataTypes = buffer.ReadUInt32()

            Utils.notNetcdf(((type < 1) AndAlso (type > 6)), $"non valid type {type}")

            ' Read variable size
            ' The 32-bit varSize field Is Not large enough to contain the size of variables that require
            ' more than 2^32 - 4 bytes, so 2^32 - 1 Is used in the varSize field for such variables.
            Dim varSize = buffer.ReadUInt32()
            ' Read offset
            Dim offset = buffer.ReadUInt32()

            If (version = 2) Then
                Utils.notNetcdf((offset > 0), "offsets larger than 4GB not supported")
                offset = buffer.ReadUInt32()
            End If

            Dim record As Boolean = False

            ' Count amount of record variables
            If ((recordId IsNot Nothing) AndAlso (dimensionsIds(0) = recordId)) Then
                ' For record variables, it is the amount of space per
                ' record.
                recordStep += varSize
                record = True
            End If

            Return New variable With {
                .name = name,
                .dimensions = dimensionsIds,
                .attributes = attributes,
                .type = type,
                .size = varSize,
                .offset = offset,
                .record = record
            }
        End Function
    End Module
End Namespace
