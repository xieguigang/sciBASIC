#Region "Microsoft.VisualBasic::1d25732ebab32c6eb0b2c2c6a178c01e, ..\sciBASIC#\Data_science\Mathematical\Plots\Testing\ChartingBase.Test\Module1.vb"

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
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.StyledBarplot
Imports Microsoft.VisualBasic.Data.ChartPlots.Dendrogram
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()

        For Each i% In (1, 1000).Iterates
            println("%s = %X", i, i)
        Next


        Pause()

        Dim t As New Tree("root")

        Call t.ChildNodes.Add(New Tree("123"))
        Call t.ChildNodes.Add(New Tree("333"))
        Call t.ChildNodes.Last.AddChild(New Tree("233"))
        Call t.ChildNodes.Last.AddChild(New Tree("abc"))
        Call t.ChildNodes.Last.AddChild(New Tree("666"))

        Dim layout = t.HorizontalLayout(New Rectangle(100, 100, 1000, 2000))


        Pause()

        Dim s = {
            New BarSerial With {.Brush = "red", .Label = "label<sub>1</sub>", .Value = 123},
            New BarSerial With {.Brush = "red", .Label = "label<sub>1</sub>", .Value = 123},
            New BarSerial With {.Brush = "cyan", .Label = "label<sub>1</sub>", .Value = 123},
            New BarSerial With {.Brush = "red", .Label = "label<sub>1</sub>", .Value = 123},
            New BarSerial With {.Brush = "orange", .Label = "label<sub>1</sub>", .Value = 13},
            New BarSerial With {.Brush = "blue", .Label = "label<sub>1</sub>", .Value = 223},
            New BarSerial With {.Brush = "yellow", .Label = "label<sub>1</sub>", .Value = 223},
            New BarSerial With {.Brush = "black", .Label = "label<sub>1</sub>", .Value = 223},
            New BarSerial With {.Brush = "red", .Label = "label<sub>1</sub>", .Value = 223}
        }

        Call s.Plot().Save("x:/test.png")

        Pause()

        'Dim x = "-60,23"
        'Dim y = "-0.5,2"

        'Dim mapper As New Mapper(x, y)

        'Using g As GDIPlusDeviceHandle = New Size(1600, 1200).CreateGDIDevice

        '    Call g.Graphics.DrawAxis(g.Size, "padding: 50 50 50 50", mapper, True,
        '                             xlabel:="<span style=""color:green"">log<sub>2</sub>(Fold Change)</span>",
        '                             ylabel:="-log<sub>10</sub>(P-value)",
        '                             ylayout:=YAxisLayoutStyles.Centra)

        '    Call g.Save("x:\test.png", ImageFormats.Png)

        'End Using

        'Pause()


        Call AxisScalling.GetAxisValues(98,, -83.4023).GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(198,, -83.4023).GetJson.__DEBUG_ECHO

        '   Pause()

        Call AxisScalling.GetAxisValues(98,, 83.4023).GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(98,, 3).GetJson.__DEBUG_ECHO

        Call AxisScalling.GetAxisValues(7.9,, 1.3).GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(-7.9,, -21.3).GetJson.__DEBUG_ECHO

        Call AxisScalling.GetAxisValues(0.98,, 0.03, [decimal]:=2).GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(2.98,, 0.03, [decimal]:=2).GetJson.__DEBUG_ECHO

        Call AxisScalling.GetAxisValues(-0.0198,, -0.903, [decimal]:=2).GetJson.__DEBUG_ECHO

        Pause()




    End Sub

End Module

