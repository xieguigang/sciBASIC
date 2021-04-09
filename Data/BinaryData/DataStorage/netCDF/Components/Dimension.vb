#Region "Microsoft.VisualBasic::65b185c557111e3d3cb56423411cf418, Data\BinaryData\DataStorage\netCDF\Components\Dimension.vb"

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

    '     Structure Dimension
    ' 
    '         Properties: name
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace netCDF.Components

    ''' <summary>
    ''' ``[name => size]``
    ''' </summary>
    ''' 
    <XmlType("dim", [Namespace]:=Xml.netCDF)>
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

        Public Overrides Function ToString() As String
            Return $"{name}(size={size})"
        End Function
    End Structure

End Namespace
