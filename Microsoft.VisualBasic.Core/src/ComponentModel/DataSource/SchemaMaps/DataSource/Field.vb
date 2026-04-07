#Region "Microsoft.VisualBasic::9b834a7328705fe9dc52379a1c184fee, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\SchemaMaps\DataSource\Field.vb"

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

    '   Total Lines: 24
    '    Code Lines: 12 (50.00%)
    ' Comment Lines: 8 (33.33%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 4 (16.67%)
    '     File Size: 711 B


    '     Class Field
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.DataSourceModel.SchemaMaps

    ''' <summary>
    ''' <see cref="DataFrameColumnAttribute"/>属性的别称
    ''' </summary>
    Public Class Field : Inherits DataFrameColumnAttribute

        ''' <summary>
        ''' Initializes a new instance by name.
        ''' </summary>
        ''' <param name="FieldName">The name.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(fieldName As String)
            Call MyBase.New(fieldName)
        End Sub

        Sub New(ordinal As Integer)
            Call MyBase.New(ordinal)
        End Sub
    End Class
End Namespace
