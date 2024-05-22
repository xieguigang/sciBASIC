#Region "Microsoft.VisualBasic::e534213f9f72a1239833c2353c43a2c4, mime\application%rdf+xml\test\Module1.vb"

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

    '   Total Lines: 72
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 57 (79.17%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (20.83%)
    '     File Size: 2.22 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::b2d749f01506c88d078a05e62622c4d4, mime\application%rdf+xml\TestProject\Module1.vb"

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

'    ' Module Module1
'    ' 
'    '     Sub: Main
'    ' 
'    ' Class RDF
'    ' 
'    '     Properties: Entity
'    ' 
'    ' Class ee
'    ' 
'    '     Properties: author, homepage
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.DocumentFormat.RDF

'Module Module1

'    Sub Main()

'        Dim xeee As New ee With {.Resource = "http://www.w3school.com.cn/RDF", .author = "David", .homepage = "http://www.w3school.com.cn"}

'        Call New RDF With {.Entity = xeee}.SaveAsXml("x:\test_rdf.xml")

'        Dim x As New RDFD With {.CDList = {New RDFD.CD With {.Artist = ""}}}
'        Call x.SaveAsXml("x:\test2.rdf.xml")
'    End Sub
'End Module

'Public Class RDF
'    Public Property Entity As ee
'End Class

'Public Class ee : Inherits RDFEntity
'    Public Property author As String
'    Public Property homepage As String
'End Class
