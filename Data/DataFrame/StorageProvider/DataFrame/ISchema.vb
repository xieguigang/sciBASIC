#Region "Microsoft.VisualBasic::952d1693ae01ce2a2546fec66b3a7595, Data\DataFrame\StorageProvider\DataFrame\ISchema.vb"

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

    '   Total Lines: 12
    '    Code Lines: 6 (50.00%)
    ' Comment Lines: 4 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (16.67%)
    '     File Size: 367 B


    '     Interface ISchema
    ' 
    '         Properties: SchemaOridinal
    ' 
    '         Function: GetOrdinal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace StorageProvider

    Public Interface ISchema

        ''' <summary>
        ''' 从数据源之中解析出来得到的域列表
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer)
        Function GetOrdinal(name As String) As Integer
    End Interface
End Namespace
