#Region "Microsoft.VisualBasic::13737a7d8a3c8579738e173c9b569448, Data\BinaryData\DataStorage\netCDF\Components\DimensionList.vb"

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

    '     Class DimensionList
    ' 
    '         Properties: dimensions, HaveRecordDimension, recordId, recordName
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace netCDF.Components

    Public Class DimensionList

        <XmlAttribute> Public Property recordId As Integer?
        <XmlAttribute> Public Property recordName As String

        Public ReadOnly Property HaveRecordDimension As Boolean
            Get
                Return Not (recordId Is Nothing AndAlso recordName = "NA")
            End Get
        End Property

        Public Property dimensions As Dimension()

        Public Overrides Function ToString() As String
            Return $"[{recordId}] {recordName}"
        End Function
    End Class

End Namespace
