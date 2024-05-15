#Region "Microsoft.VisualBasic::ff530451ecfec01ffc904ad8819f8615, Data\BinaryData\SQLite3\Objects\ColumnDataMeta.vb"

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

    '   Total Lines: 28
    '    Code Lines: 15
    ' Comment Lines: 7
    '   Blank Lines: 6
    '     File Size: 822 B


    '     Class ColumnDataMeta
    ' 
    '         Properties: name, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums

Namespace ManagedSqlite.Core.Objects

    Public Class ColumnDataMeta

        Public ReadOnly Property type As SqliteDataType
        ''' <summary>
        ''' Field name of current column meta data
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String

        ''' <summary>
        ''' 对于文本或者blob类型,长度是可变的
        ''' </summary>
        Public length As UShort

        Sub New(name$, type As SqliteDataType)
            Me.type = type
            Me.name = name
        End Sub

        Public Overrides Function ToString() As String
            Return $"{name}: {type.Description} ~ {length}"
        End Function
    End Class
End Namespace
