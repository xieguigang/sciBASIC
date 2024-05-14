#Region "Microsoft.VisualBasic::0442cf63a9796676bc934dac75b3a7f0, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\TemplateAttribute.vb"

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

    '   Total Lines: 17
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 375 B


    '     Class TemplateAttribute
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.DataSourceModel

    Public Class TemplateAttribute : Inherits Attribute

        ReadOnly schema As Type
        ReadOnly schemaName As String

        Sub New(schema As Type)
            Me.schema = schema
        End Sub

        Sub New(name As String)
            Me.schemaName = name
        End Sub

    End Class
End Namespace
