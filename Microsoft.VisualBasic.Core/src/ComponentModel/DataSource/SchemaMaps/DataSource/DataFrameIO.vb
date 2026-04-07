#Region "Microsoft.VisualBasic::fd20d19aea859283cc20b3ac7373e763, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\SchemaMaps\DataSource\DataFrameIO.vb"

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

    '   Total Lines: 13
    '    Code Lines: 5 (38.46%)
    ' Comment Lines: 5 (38.46%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 3 (23.08%)
    '     File Size: 408 B


    '     Class DataFrameIO
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.DataSourceModel.SchemaMaps

    Public MustInherit Class DataFrameIO(Of TAttributeType As DataFrameColumnAttribute)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected MustOverride Function InitializeSchema(Of TEntityType As Class)() As TAttributeType()

    End Class
End Namespace
