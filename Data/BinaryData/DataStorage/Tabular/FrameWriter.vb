#Region "Microsoft.VisualBasic::f3b17e1d3d181b1b6b2f5fa7ae9125ce, Data\BinaryData\DataStorage\Tabular\FrameWriter.vb"

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

    '   Total Lines: 69
    '    Code Lines: 47 (68.12%)
    ' Comment Lines: 9 (13.04%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 13 (18.84%)
    '     File Size: 2.30 KB


    ' Module FrameWriter
    ' 
    '     Properties: magic
    ' 
    '     Function: WriteFrame
    ' 
    '     Sub: WriteScalar, WriteVector
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module FrameWriter

    Public ReadOnly Property magic As IReadOnlyCollection(Of Byte) = Encoding.ASCII.GetBytes("scibasic.net/dataframe")

    ''' <summary>
    ''' write dataframe object as the binary file
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <Extension>
    Public Function WriteFrame(df As DataFrame, file As Stream) As Boolean
        Dim wr As New BinaryDataWriter(file) With {.ByteOrder = ByteOrder.BigEndian}
        Dim offset As Long
        Dim metadata As New Schema(df)

        Call wr.Write(DirectCast(magic, Byte()))
        ' offsets for the metadata
        Call wr.Write(0&)

        For Each name As String In metadata.ordinals
            Dim v As FeatureVector = df.features(name)

            offset = wr.Position
            metadata(name).offset = offset

            If v.isScalar Then
                Call WriteScalar(wr, v.GetScalarValue, metadata(name).type)
            Else
                Call WriteVector(wr, v, metadata(name).type)
            End If
        Next

        offset = wr.Position

        ' write metadata offset at the begining and ends of stream
        Call wr.Write(metadata.GetJson, BinaryStringFormat.DwordLengthPrefix)
        Call wr.Write(offset)
        Call wr.Flush()
        ' jump to the start of the file data
        Call wr.Seek(magic.Count, SeekOrigin.Begin)
        Call wr.Write(offset)
        Call wr.Flush()

        Return True
    End Function

    Private Sub WriteVector(wr As BinaryDataWriter, v As FeatureVector, code As TypeCode)
        Call wr.Write(v.size)
        Call VectorStream.WriteVector(wr, v.vector, code)
    End Sub

    Private Sub WriteScalar(wr As BinaryDataWriter, obj As Object, code As TypeCode)
        If obj Is Nothing OrElse code = TypeCode.DBNull OrElse code = TypeCode.Empty Then
            Call wr.Write(0%)
            Return
        Else
            Call wr.Write(1%)
            Call VectorStream.WriteScalar(wr, obj, code)
        End If
    End Sub

End Module
