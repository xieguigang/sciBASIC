#Region "Microsoft.VisualBasic::5151d3988f429ace521ea0245bec0fb4, Data\BinaryData\Feather\Impl\WriteColumnConfig.vb"

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

    '   Total Lines: 21
    '    Code Lines: 19
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 727 B


    '     Class WriteColumnConfig
    ' 
    '         Properties: Data, DotNetType, Length, Name, NullCount
    '                     OnDiskType
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace Impl
    Friend Class WriteColumnConfig
        Public Property Name As String
        Public Property DotNetType As Type
        Public Property OnDiskType As ColumnType
        Public Property Length As Long
        Public Property Data As IEnumerable
        Public Property NullCount As Long

        Public Sub New(name As String, dotNetType As Type, onDiskType As ColumnType, length As Long, data As IEnumerable, nullCount As Long)
            Me.Name = name
            Me.DotNetType = dotNetType
            Me.OnDiskType = onDiskType
            Me.Length = length
            Me.Data = data
            Me.NullCount = nullCount
        End Sub
    End Class
End Namespace
