#Region "Microsoft.VisualBasic::9e163c2e5ad7d901123b9f102821598d, sciBASIC#\Data\BinaryData\netCDF\Components\Dimension.vb"

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

    '   Total Lines: 36
    '    Code Lines: 19
    ' Comment Lines: 10
    '   Blank Lines: 7
    '     File Size: 1017 B


    '     Structure Dimension
    ' 
    '         Properties: name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace Components

    ''' <summary>
    ''' ``[name => size]``
    ''' </summary>
    ''' 
    <XmlType("dim", [Namespace]:=Data.Xml.netCDF)>
    Public Structure Dimension : Implements INamedValue

        ''' <summary>
        ''' String with the name of the dimension
        ''' </summary>
        <XmlAttribute>
        Public Property name As String Implements IKeyedEntity(Of String).Key

        ''' <summary>
        ''' Number with the size of the dimension
        ''' </summary>
        <XmlText>
        Dim size As Integer

        Sub New(name As String, size As Integer)
            Me.name = name
            Me.size = size
        End Sub

        Public Overrides Function ToString() As String
            Return $"{name}(size={size})"
        End Function
    End Structure

End Namespace
