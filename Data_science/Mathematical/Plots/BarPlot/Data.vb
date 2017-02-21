#Region "Microsoft.VisualBasic::a94e34d765a2a7b4581d977fd50b3db6, ..\sciBASIC#\Data_science\Mathematical\Plots\BarPlot\Data.vb"

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

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class BarDataSample

    Public Property Tag As String
    Public Property data As Double()

    ''' <summary>
    ''' The sum of <see cref="data"/>
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property StackedSum As Double
        Get
            Return data.Sum
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class BarDataGroup : Inherits ProfileGroup

    ''' <summary>
    ''' 与<see cref="BarDataSample.data"/>里面的数据顺序是一致的
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Property Serials As NamedValue(Of Color)()
    Public Property Samples As BarDataSample()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Shared Function FromDistributes(data As IEnumerable(Of Double), Optional base! = 10.0F, Optional color$ = "darkblue") As BarDataGroup
        Dim source = data.Distributes(base!)
        Dim bg As Color = color.ToColor(onFailure:=Drawing.Color.DarkBlue)
        Dim values As New List(Of Double)
        Dim serials = LinqAPI.Exec(Of NamedValue(Of Color)) <=
            From lv As Integer
            In source.Keys
            Select New NamedValue(Of Color) With {
                .Name = lv.ToString,
                .Value = bg
            }

        For Each x In serials
            values += source(CInt(x.Name)).value
        Next

        Return New BarDataGroup With {
            .Serials = serials,
            .Samples = {
                New BarDataSample With {
                    .Tag = "Distribution",
                    .data = values
                }
            }
        }
    End Function
End Class
