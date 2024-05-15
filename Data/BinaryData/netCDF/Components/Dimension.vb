#Region "Microsoft.VisualBasic::0abb9aa55c74ad64bcfa750035af29d6, Data\BinaryData\netCDF\Components\Dimension.vb"

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

    '   Total Lines: 54
    '    Code Lines: 33
    ' Comment Lines: 10
    '   Blank Lines: 11
    '     File Size: 1.64 KB


    '     Structure Dimension
    ' 
    '         Properties: name
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: FromVector, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.DataStorage.netCDF.DataVector

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

        Sub New(chrs As chars)
            Me.size = chrs.Length
        End Sub

        Public Overrides Function ToString() As String
            Return $"{name}(size={size})"
        End Function

        Public Shared Function FromVector(Of T)(data As CDFData(Of T), Optional dimName As String = Nothing) As Dimension
            Dim dimSize As Integer = data.Length

            If dimName.StringEmpty Then
                dimName = $"{App.GetNextUniqueName("sizeof_")}_{GetType(T).Name.ToLower}"
            End If

            Return New Dimension With {
                .name = dimName,
                .size = dimSize
            }
        End Function
    End Structure

End Namespace
