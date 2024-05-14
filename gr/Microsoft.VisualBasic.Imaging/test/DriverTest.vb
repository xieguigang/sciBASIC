#Region "Microsoft.VisualBasic::fd4808c8c16daef020fc32e5569ac257, gr\Microsoft.VisualBasic.Imaging\test\DriverTest.vb"

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

    '   Total Lines: 60
    '    Code Lines: 30
    ' Comment Lines: 13
    '   Blank Lines: 17
    '     File Size: 2.01 KB


    ' Module DriverTest
    ' 
    '     Function: testPlot
    ' 
    '     Sub: Main1
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Driver.CSS
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS
Imports VB = Microsoft.VisualBasic.Language.Runtime

Module DriverTest
    Sub Main1()

        Dim cssfile As CSSFile = CssParser.GetTagWithCSS("G:\GCModeller\src\runtime\sciBASIC#\gr\SVG\demo.css".ReadAllText)


        Dim css As New CSSFile With {
            .Selectors = {
                New Selector With {.Selector = "@canvas", .Properties = New Dictionary(Of String, String) From {{"bg", "123"}}},
                New Selector With {.Selector = "#testfont", .Properties = New Dictionary(Of String, String) From {{"font-size", "15px"}}}
            }
        }

        With New VB

            ' Call (AddressOf testPlot).RunPlot(css, !A = 99, !B = 123, !CSS = "dertfff")

            Call GetType(DriverTest).LoadDriver("test.plot").RunPlot(Nothing, css, !A = 99, !B = 123, !bg = "from reflection: 1234567890")

#Region "Not working"
            'Dim driver As Func(Of Single, Single, String, GraphicsData) = AddressOf testPlot

            'Call driver.RunPlot(
            '    css, !A = 99,
            '         !B = 123,
            '         !CSS = "from delegate: 1234567890",
            '         !testfont = "12345")
#End Region



            Call testPlot(A:=666, b:=4444, bg:="direct calls")

        End With


        Pause()
    End Sub

    ''' <summary>
    ''' Test target
    ''' </summary>
    ''' <param name="A!"></param>
    ''' <param name="b!"></param>
    ''' <returns></returns>
    Public Function testPlot(A!, b!, Optional bg$ = "1234", Optional testFont$ = CSSFont.Win7LargerBold) As GraphicsData
        Call Console.WriteLine(A)
        Call Console.WriteLine(b)
        Call Console.WriteLine(bg)
        Call Console.WriteLine(testFont)

        Call Console.WriteLine(New String("+"c, 20))
    End Function
End Module
