
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

Namespace HDF5.dataset

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
