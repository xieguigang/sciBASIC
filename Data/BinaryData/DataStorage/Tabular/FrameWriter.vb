#Region "Microsoft.VisualBasic::c1eaccce035d47d732c6b880e75f8f14, Data\BinaryData\DataStorage\Tabular\FrameWriter.vb"

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

    '   Total Lines: 95
    '    Code Lines: 70 (73.68%)
    ' Comment Lines: 8 (8.42%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 17 (17.89%)
    '     File Size: 3.44 KB


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
Imports Microsoft.VisualBasic.ValueTypes

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
        Call wr.Write(metadata.GetJson)
        Call wr.Write(offset)
        Call wr.Flush()
        Call wr.Seek(magic.Count, SeekOrigin.Begin)
        Call wr.Write(offset)
        Call wr.Flush()

        Return True
    End Function

    Private Sub WriteVector(wr As BinaryDataWriter, v As FeatureVector, code As TypeCode)
        Call wr.Write(v.size)

        Select Case code
            Case TypeCode.Boolean : wr.Write(v.TryCast(Of Boolean).Select(Function(f) CByte(f)).ToArray)

            Case Else
                Throw New NotImplementedException(code.ToString)
        End Select
    End Sub

    Private Sub WriteScalar(wr As BinaryDataWriter, obj As Object, code As TypeCode)
        If obj Is Nothing OrElse code = TypeCode.DBNull OrElse code = TypeCode.Empty Then
            Call wr.Write(0%)
            Return
        Else
            Call wr.Write(1%)
        End If

        Select Case code
            Case TypeCode.Boolean : If CBool(obj) Then wr.Write(CByte(1)) Else wr.Write(CByte(0))
            Case TypeCode.Byte : wr.Write(CByte(obj))
            Case TypeCode.Char : wr.Write(AscW(CChar(obj)))
            Case TypeCode.DateTime : wr.Write(CDate(obj).UnixTimeStamp)
            Case TypeCode.Decimal : wr.Write(CDec(obj))
            Case TypeCode.Double : wr.Write(CDbl(obj))
            Case TypeCode.Int16 : wr.Write(CShort(obj))
            Case TypeCode.Int32 : wr.Write(CInt(obj))
            Case TypeCode.Int64 : wr.Write(CLng(obj))
            Case TypeCode.SByte : wr.Write(CSByte(obj))
            Case TypeCode.Single : wr.Write(CSng(obj))
            Case TypeCode.String : wr.Write(CStr(obj))
            Case TypeCode.UInt16 : wr.Write(CUShort(obj))
            Case TypeCode.UInt32 : wr.Write(CUInt(obj))
            Case TypeCode.UInt64 : wr.Write(CULng(obj))

            Case Else
                Throw New NotImplementedException(code.ToString)
        End Select
    End Sub

End Module
