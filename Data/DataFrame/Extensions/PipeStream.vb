#Region "Microsoft.VisualBasic::c5efae1cbf748563abcd7540f8b47eeb, ..\sciBASIC#\Data\DataFrame\Extensions\PipeStream.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO

Public Module PipeStream

    <Extension>
    Public Iterator Function LoadStream(Of T As Class)(input As StreamReader, Optional strict As Boolean = False, Optional maps As Dictionary(Of String, String) = Nothing) As IEnumerable(Of T)

    End Function

    <Extension>
    Public Function AsDataSet(source As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double)))) As DataSet()
        Return source.Select(
            Function(x)
                Return New DataSet With {
                    .ID = x.Name,
                    .Properties = x.Value
                }
            End Function).ToArray
    End Function

    <Extension>
    Public Function Vectors(data As IEnumerable(Of DataSet)) As NamedValue(Of Double())()
        Return data.Select(
            Function(d)
                Return New NamedValue(Of Double()) With {
                    .Name = d.ID,
                    .Value = d.Properties.Values.ToArray
                }
            End Function).ToArray
    End Function
End Module
