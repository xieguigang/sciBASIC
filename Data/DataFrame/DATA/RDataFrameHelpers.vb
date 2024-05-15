#Region "Microsoft.VisualBasic::1a94179f171cb5cf078e77708f9af12d, Data\DataFrame\DATA\RDataFrameHelpers.vb"

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

    '   Total Lines: 71
    '    Code Lines: 48
    ' Comment Lines: 11
    '   Blank Lines: 12
    '     File Size: 2.73 KB


    '     Module RDataFrameHelpers
    ' 
    '         Function: InvalidsAsRLangNA, processValue, StripNaN
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace DATA

    ''' <summary>
    ''' Helpers function for R dataframe data imports
    ''' </summary>
    Public Module RDataFrameHelpers

        Private Function processValue(map As KeyValuePair(Of String, Double), replaceAs$) As String
            Dim s As String = map.Value.ToString

            If isNaN(s) Then
                Return replaceAs
            Else
                Return s
            End If
        End Function

        <Extension>
        Public Iterator Function InvalidsAsRLangNA(source As IEnumerable(Of DataSet), Optional replaceAs$ = "NA") As IEnumerable(Of EntityObject)
            For Each data As DataSet In source
                Dim values = data _
                    .Properties _
                    .ToDictionary(Function(map) map.Key,
                                  Function(map)
                                      Return processValue(map, replaceAs)
                                  End Function)

                Yield New EntityObject With {
                    .ID = data.ID,
                    .Properties = values
                }
            Next
        End Function

        ''' <summary>
        ''' 对于一些数学计算的数值结果，无穷大，无穷小或者非实数会被转换为中文，导致R程序无法识别
        ''' 则需要使用这个函数来将这些数值替换为目标字符串<paramref name="replaceAs"/>
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="replaceAs$">默认为R之中可以识别的``NA``常数值</param>
        ''' <returns></returns>
        Public Function StripNaN(path$, Optional replaceAs$ = "NA") As Boolean
            Dim csv As IO.File = IO.File.Load(path)
            Dim invalids As Index(Of String) = {"正无穷大", "负无穷大", "非数字"}.Indexing
            Dim file As New List(Of RowObject)

            ' 因为第一行一般都是标题行，所以在这里直接跳过了
            For Each row In csv.Skip(1)
                Dim buffer = row.ToArray

                For i As Integer = 0 To buffer.Length - 1
                    If invalids.IndexOf(buffer(i)) > -1 Then
                        buffer(i) = replaceAs
                    End If
                Next

                file += New RowObject(buffer)
            Next

            csv = csv.First + file

            Return csv.Save(path, Encodings.UTF8)
        End Function
    End Module
End Namespace
