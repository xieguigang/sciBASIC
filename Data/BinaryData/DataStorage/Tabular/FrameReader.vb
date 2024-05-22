#Region "Microsoft.VisualBasic::acff783762d9d4faa3a67794cd8e5177, Data\BinaryData\DataStorage\Tabular\FrameReader.vb"

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

    '   Total Lines: 47
    '    Code Lines: 35 (74.47%)
    ' Comment Lines: 5 (10.64%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (14.89%)
    '     File Size: 1.57 KB


    ' Module FrameReader
    ' 
    '     Function: ReadFeather
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataStorage
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.DataFrame
Imports any = Microsoft.VisualBasic.Scripting

Public Module FrameReader

    ''' <summary>
    ''' read the feather file as dataframe
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    Public Function ReadFeather(file As String) As DataFrame
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

End Module
