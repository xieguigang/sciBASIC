'#Region "Microsoft.VisualBasic::5f8c9d9ea3f5a6cb5065c9ac054dbef1, Microsoft.VisualBasic.Core\test\test\wmfTest.vb"

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


'    ' Code Statistics:

'    '   Total Lines: 16
'    '    Code Lines: 13 (81.25%)
'    ' Comment Lines: 0 (0.00%)
'    '    - Xml Docs: 0.00%
'    ' 
'    '   Blank Lines: 3 (18.75%)
'    '     File Size: 659 B


'    ' Module wmfTest
'    ' 
'    '     Sub: Main1
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports System.Drawing
'Imports Microsoft.VisualBasic.Imaging

'Module wmfTest

'    Sub Main1()
'        Using wmf As New Wmf(New Size(1200, 500), "./test.wmf", backgroundColor:="white")
'            Call wmf.DrawString("Hello world", New Font("Microsoft YaHei", 64, FontStyle.Bold), Brushes.Red, New PointF(100, 100))
'        End Using

'        Using png As Graphics2D = New Size(1200, 500).CreateGDIDevice(Color.White)
'            Call png.DrawString("Hello world", New Font("Microsoft YaHei", 64, FontStyle.Bold), Brushes.Red, New PointF(100, 100))
'            Call png.ImageResource.SaveAs("./test.png")
'        End Using
'    End Sub
'End Module
