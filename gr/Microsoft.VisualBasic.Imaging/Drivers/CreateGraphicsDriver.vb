#Region "Microsoft.VisualBasic::11561d3e69ce9064a9aede1360732f9b, gr\Microsoft.VisualBasic.Imaging\Drivers\CreateGraphicsDriver.vb"

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

    '   Total Lines: 30
    '    Code Lines: 18 (60.00%)
    ' Comment Lines: 6 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (20.00%)
    '     File Size: 977 B


    '     Module ImageDriver
    ' 
    '         Function: GraphicsPlot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D

#If NET48 Then
Imports Microsoft.VisualBasic.Drawing
#End If

Namespace Driver

    Public Module ImageDriver

        ''' <summary>
        ''' a unify method for create graphics plot
        ''' </summary>
        ''' <param name="desc">the graphics drawing context description</param>
        ''' <param name="plot"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GraphicsPlot(desc As DeviceDescription, plot As IPlot) As GraphicsData
            Dim device = DriverLoad.UseGraphicsDevice(desc.driverUsed)

            Using g As IGraphics = device.CreateGraphic(desc.size, desc.background, desc.dpi)
                Call g.Clear(desc.background)
                Call plot(g, desc.GetRegion)

                Return device.GetData(g, desc.padding)
            End Using
        End Function
    End Module
End Namespace
