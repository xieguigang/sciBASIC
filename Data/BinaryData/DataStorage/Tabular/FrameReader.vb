#Region "Microsoft.VisualBasic::695cd5df8683449754febbeef4ccda74, Data\BinaryData\DataStorage\Tabular\FrameReader.vb"

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

    '   Total Lines: 177
    '    Code Lines: 134 (75.71%)
    ' Comment Lines: 16 (9.04%)
    '    - Xml Docs: 93.75%
    ' 
    '   Blank Lines: 27 (15.25%)
    '     File Size: 7.11 KB


    ' Module FrameReader
    ' 
    '     Function: ReadFeatures, (+2 Overloads) ReadFrame, (+2 Overloads) ReadSasXPT
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.IO.Xpt
Imports Microsoft.VisualBasic.Data.IO.Xpt.Types
Imports Microsoft.VisualBasic.DataStorage
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports any = Microsoft.VisualBasic.Scripting

Public Module FrameReader

    Public Function ReadFrame(file As String) As DataFrame
        Using s As Stream = file.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
            Return ReadFrame(s)
        End Using
    End Function

    Public Function ReadFrame(file As Stream) As DataFrame
        Dim df As New Dictionary(Of String, FeatureVector)
        Dim bin As New BinaryDataReader(file) With {
            .ByteOrder = ByteOrder.BigEndian
        }

        If Not FrameWriter.magic.SequenceEqual(bin.ReadBytes(FrameWriter.magic.Count)) Then
            Throw New InvalidDataException("invalid data magic header for binary dataframe file!")
        Else
            Call bin.Seek(bin.ReadInt64, Scan0)
        End If

        Dim metadata As Schema = bin.ReadString(BinaryStringFormat.DwordLengthPrefix).LoadJSON(Of Schema)

        For Each name As String In metadata.ordinals
            Call bin.Seek(metadata(name).offset, SeekOrigin.Begin)

            If metadata(name).isScalar Then
                Dim flag As Integer = bin.ReadInt32

                If flag = 0 Then
                    df.Add(name, FeatureVector.FromGeneral(name, metadata(name).CreateEmpty))
                Else
                    df.Add(name, FeatureVector.FromScalar(name, VectorStream.ReadScalar(bin, metadata(name).type)))
                End If
            Else
                Dim size As Integer = bin.ReadInt32
                Dim vec As Array = VectorStream.ReadVector(bin, metadata(name).type, size)

                Call df.Add(name, FeatureVector.FromGeneral(name, vec))
            End If

            df(name).attributes = metadata(name).attrs
        Next

        Return New DataFrame With {
            .name = metadata.name,
            .description = metadata.description,
            .features = df,
            .rownames = metadata.rownames
        }
    End Function

    ''' <summary>
    ''' read the feather file as dataframe
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    Public Function ReadFeatures(file As String) As DataFrame
        Dim df As New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector)
        }
        Dim type As Type
        Dim data As Array
        Dim feature As FeatureVector
        Dim value As FeatherFormat.Value

        Using untyped = FeatherFormat.ReadFromFile(file)
            For Each col As FeatherFormat.Column In untyped.AllColumns.AsEnumerable
                type = col.Type
                data = Array.CreateInstance(type, col.Length)

                For i As Integer = 0 To data.Length - 1
                    value = col(i)
                    data.SetValue(value, i)
                Next

                If col.Name = "row.names" Then
                    data = data.AsObjectEnumerator _
                        .Select(Function(o) any.ToString(o)) _
                        .ToArray
                    df.rownames = DirectCast(data, String())
                Else
                    feature = FeatureVector.FromGeneral(col.Name, data)
                    df.add(feature)
                End If
            Next
        End Using

        Return df
    End Function

    ''' <summary>
    ''' read sas xpt file as dataframe
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ReadSasXPT(file As String) As DataFrame
        Return ReadSasXPT(file.Open(FileMode.Open, doClear:=False, [readOnly]:=True), filename:=file)
    End Function

    ''' <summary>
    ''' read sas xpt file as dataframe
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <returns></returns>
    Public Function ReadSasXPT(buffer As Stream, Optional filename As String = Nothing) As DataFrame
        Using iterator As New SASXportFileIterator(buffer)
            Dim cols As List(Of Object)() = New List(Of Object)(iterator.MetaData.var_count - 1) {}
            Dim row As Object()
            Dim tableName = If(
                iterator.MetaData.table_name.StringEmpty(, True),
                filename.BaseName(allowEmpty:=True),
                iterator.MetaData.table_name)

            For i As Integer = 0 To cols.Length - 1
                cols(i) = New List(Of Object)
            Next

            While iterator.hasNext()
                row = iterator.next().ToArray

                For i As Integer = 0 To cols.Length - 1
                    Call cols(i).Add(row(i))
                Next
            End While

            Dim features As New Dictionary(Of String, FeatureVector)
            Dim desc As New StringBuilder

            For i As Integer = 0 To cols.Length - 1
                Dim field As ReadStatVariable = iterator.MetaData.variables(i)

                Select Case field.type
                    Case ReadstatType.READSTAT_TYPE_DOUBLE
                        Call features.Add(field.name, New FeatureVector(field.name, cols(i).Select(Function(s) CDbl(s))))
                    Case ReadstatType.READSTAT_TYPE_FLOAT
                        Call features.Add(field.name, New FeatureVector(field.name, cols(i).Select(Function(s) CSng(s))))
                    Case ReadstatType.READSTAT_TYPE_INT16
                        Call features.Add(field.name, New FeatureVector(field.name, cols(i).Select(Function(s) CShort(s))))
                    Case ReadstatType.READSTAT_TYPE_INT32
                        Call features.Add(field.name, New FeatureVector(field.name, cols(i).Select(Function(s) CInt(s))))
                    Case ReadstatType.READSTAT_TYPE_INT8
                        Call features.Add(field.name, New FeatureVector(field.name, cols(i).Select(Function(s) CByte(s))))
                    Case ReadstatType.READSTAT_TYPE_STRING
                        Call features.Add(field.name, New FeatureVector(field.name, cols(i).Select(Function(s) CStr(s))))
                    Case Else
                        Throw New NotImplementedException(field.type.ToString)
                End Select

                Call desc.AppendLine(field.ToString)
            Next

            Return New DataFrame With {
                .features = features,
                .name = tableName,
                .description = desc.ToString,
                .rownames = Enumerable _
                    .Range(1, iterator.RowCount - 1) _
                    .Select(Function(i) $"#{i}") _
                    .ToArray
            }
        End Using
    End Function

End Module
