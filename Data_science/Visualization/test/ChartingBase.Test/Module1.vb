#Region "Microsoft.VisualBasic::acc141cfecb7f55dc2f37fd233596d3c, Data_science\Visualization\test\ChartingBase.Test\Module1.vb"

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

    '   Total Lines: 151
    '    Code Lines: 97 (64.24%)
    ' Comment Lines: 12 (7.95%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 42 (27.81%)
    '     File Size: 6.49 KB


    ' Module Module1
    ' 
    '     Sub: __3dPie, alignment, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.StyledBarplot
Imports Microsoft.VisualBasic.Data.ChartPlots.Dendrogram
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub alignment()

        Dim query = "62.108932,0.00725163 72.802757,0.00619083 79.623337,0.00616974 86.093651,0.01244469 86.094704,0.03735146 86.096313,1 96.304543,0.00640275 111.7593,0.00621461 132.101593,0.04109047 141.125336,0.0062846 143.391052,0.0066127 191.200211,0.0065964 231.914474,0.0068113 276.960571,0.00623346 288.77359,0.00646979 291.571564,0.00708362 291.585419,0.00957037 299.200439,0.01098496 299.209106,0.02269272 299.217865,0.0179758"
        Dim subject = "58.2126541137695,0.00202304243002261 69.0839080810547,0.0027746131613088 72.3085479736328,0.00351824931796731 74.1845703125,0.00183006668460307 86.2995376586914,1 88.2241973876953,0.00181392999438157 90.0654754638672,0.0465517525169149 91.25732421875,0.00447813866495691 100.635070800781,0.00430299461123474 101.251541137695,0.00134531628492794 104.090896606445,0.00205209116509168 111.06867980957,0.00235594572548659 111.698974609375,0.00352326562477906 114.136413574219,0.0178042154127458 114.951431274414,0.00671197938392144 119.864685058594,0.0450199075334234 123.235458374023,0.00265892982840261 128.705490112305,0.00477850915154426 129.651931762695,0.00204028708505812 132.230041503906,0.000792703342457297 138.270462036133,0.00626707705356575 140.036727905273,0.00364165198447873 140.723754882812,0.00321131803646168"

        Dim qdata = query.Split.Select(Function(s) s.Split(","c)).Select(Function(m) (Val(m(0)), Val(m(1)) * 100))
        Dim sdata = subject.Split.Select(Function(s) s.Split(","c)).Select(Function(m) (Val(m(0)), Val(m(1)) * 100))

        Call AlignmentPlot.PlotAlignment(qdata, sdata, yrange:="0,100").Save("x:\test.png")

        Pause()
    End Sub

    Sub __3dPie()
        Dim pie = {
            New NamedValue(Of Integer) With {
                .Name = "p1", .Value = 100
            },
            New NamedValue(Of Integer) With {
                .Name = "p2", .Value = 12
            },
              New NamedValue(Of Integer) With {
                .Name = "p2", .Value = 22
            },
              New NamedValue(Of Integer) With {
                .Name = "p2", .Value = 32
            },
                   New NamedValue(Of Integer) With {
                .Name = "p2", .Value = 42
            },
                   New NamedValue(Of Integer) With {
                .Name = "p2", .Value = 72
            },
                 New NamedValue(Of Integer) With {
                .Name = "p2", .Value = 72
            },
                  New NamedValue(Of Integer) With {
                .Name = "p2", .Value = 72
            }
        }

        Call pie.Plot3D(
            New Camera With {
                .ViewDistance = -2,
                .screen = New Size(1200, 1200),
                .angleZ = 120,
                .angleX = 1,
                .angleY = 2
            }).Save("X:\test3D.png")

        Pause()
    End Sub

    Sub Main()

        Call __3dPie()
        Call alignment()

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
