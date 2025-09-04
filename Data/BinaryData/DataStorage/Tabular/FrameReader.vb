#Region "Microsoft.VisualBasic::825c03582e8ac98cbacea3fd7ec1445e, Data\BinaryData\DataStorage\Tabular\FrameReader.vb"

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

    '   Total Lines: 127
    '    Code Lines: 94 (74.02%)
    ' Comment Lines: 16 (12.60%)
    '    - Xml Docs: 93.75%
    ' 
    '   Blank Lines: 17 (13.39%)
    '     File Size: 5.14 KB


    ' Module FrameReader
    ' 
    '     Function: ReadFeatures, (+2 Overloads) ReadSasXPT
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
Imports any = Microsoft.VisualBasic.Scripting

Public Module FrameReader

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
