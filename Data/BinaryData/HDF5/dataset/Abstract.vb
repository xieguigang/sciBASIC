#Region "Microsoft.VisualBasic::35e4873b1cea27461b46920d5de76e5a, Data\BinaryData\HDF5\dataset\Abstract.vb"

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

    '   Total Lines: 57
    '    Code Lines: 36
    ' Comment Lines: 9
    '   Blank Lines: 12
    '     File Size: 2.09 KB


    '     Class Hdf5Dataset
    ' 
    '         Properties: dataLayout, dataSpace, dataType, pipeline, scalar
    ' 
    '         Function: data, readDataSet, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'*****************************************************************************
' This file is part of jHDF. A pure Java library for accessing HDF5 files.
' 
' http://jhdf.io
' 
' Copyright 2019 James Mudd
' 
' MIT License see 'LICENSE' file
' *****************************************************************************

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.messages
Imports Microsoft.VisualBasic.Data.IO.HDF5.type

Namespace dataset

    Public MustInherit Class Hdf5Dataset

        Public Property dataType As DataType
        Public Property dataSpace As DataspaceMessage
        Public Property dataLayout As Layout
        Public Property pipeline As FilterPipelineMessage

        Public MustOverride ReadOnly Property dimensions As Integer()

        Public Overridable ReadOnly Property scalar As Boolean
            Get
                Return dataSpace.dimensionLength.Length = 0
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function data(sb As Superblock) As Object
            Return readDataSet(getBuffer(sb), sb)
        End Function

        Protected Function readDataSet(dataBuffer As MemoryStream, sb As Superblock) As Object
            Dim type As DataType = dataType

            If TypeOf type Is VariableLength Then
                Return VariableLengthDatasetReader.readDataSet(type, dimensions, sb, New MemoryReader(dataBuffer))
            Else
                Return DatasetReader.ParseDataChunk(type, dataBuffer.ToArray, dimensions)
            End If
        End Function

        Protected MustOverride Function getBuffer(sb As Superblock) As MemoryStream

        Public Overrides Function ToString() As String
            Return $"{Me.GetType.Name} {Scripting.ToString(dataSpace)} {Scripting.ToString(dataType)}"
        End Function
    End Class

End Namespace
