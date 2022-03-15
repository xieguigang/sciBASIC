#Region "Microsoft.VisualBasic::556b9055e7ff96051eee251d947f7ecf, sciBASIC#\gr\network-visualization\test\Test.vb"

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

    '   Total Lines: 83
    '    Code Lines: 0
    ' Comment Lines: 69
    '   Blank Lines: 14
    '     File Size: 2.57 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::4345d30a5cdf32fc5b787ecdb1353bce, gr\network-visualization\test\Test.vb"

'    ' Author:
'    ' 
'    '       asuka (amethyst.asuka@gcmodeller.org)
'    '       xie (genetics@smrucc.org)
'    '       xieguigang (xie.guigang@live.com)
'    ' 
'    ' Copyright (c) 2018 GPL3 Licensed
'    ' 
'    ' 
'    ' GNU GENERAL PUBLIC LICENSE (GPL3)
'    ' 
'    ' 
'    ' This program is free software: you can redistribute it and/or modify
'    ' it under the terms of the GNU General Public License as published by
'    ' the Free Software Foundation, either version 3 of the License, or
'    ' (at your option) any later version.
'    ' 
'    ' This program is distributed in the hope that it will be useful,
'    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
'    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    ' GNU General Public License for more details.
'    ' 
'    ' You should have received a copy of the GNU General Public License
'    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



'    ' /********************************************************************************/

'    ' Summaries:

'    ' Module Test
'    ' 
'    '     Sub: Main, TestStyling
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.Data.visualize.Network.Styling
'Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

'Module Test

'    Sub Main()
'        Call TestStyling()
'        ' Call TestPageRank()

'        Pause()
'    End Sub



'    Sub TestStyling()
'        Dim json As New StyleJSON With {
'            .nodes = New Dictionary(Of String, NodeStyle) From {
'            {
'                "*", New NodeStyle With {
'                    .fill = "black",
'                    .size = "size",
'                    .stroke = Stroke.AxisStroke
'                }
'            },
'            {
'                "type = example", New NodeStyle With {
'                    .fill = "red",
'                    .size = "scale(size, 5, 30)"
'                }
'            }
'            },
'            .labels = New Dictionary(Of String, LabelStyle) From {
'            {
'                "degree > 2", New LabelStyle With {
'                    .fill = "brown"
'                }
'            }
'            }
'        }
'        Dim styles As StyleMapper = StyleMapper.FromJSON(json)
'    End Sub
'End Module
