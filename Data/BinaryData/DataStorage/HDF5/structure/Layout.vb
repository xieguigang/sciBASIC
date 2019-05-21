#Region "Microsoft.VisualBasic::717628bbecdbdb0b720bbac8e736e373, mime\application%netcdf\HDF5\structure\Layout.vb"

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

' 	Class Layout
' 
' 	    Properties: chunkSize, dataAddress, dimensionLength, fields, maxDimensionLength
'                  numberOfDimensions
' 
' 	    Constructor: (+1 Overloads) Sub New
' 	    Sub: addField
' 
' 
' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace HDF5.[Structure]

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class Layout

        Dim fieldList As New List(Of LayoutField)

        Public Property dataAddress As Long
        Public Property chunkSize As Integer()
        Public Property numberOfDimensions As Integer
        Public Property dimensionLength As Integer()
        Public Property maxDimensionLength As Integer()

        Public ReadOnly Property fields() As IEnumerable(Of LayoutField)
            Get
                Return Me.fieldList
            End Get
        End Property

        ''' <summary>
        ''' All of the values in this <see cref="Layout"/> object is empty!
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return dataAddress <= 0 AndAlso
                    chunkSize.IsNullOrEmpty AndAlso
                    numberOfDimensions <= 0 AndAlso
                    dimensionLength.IsNullOrEmpty AndAlso
                    maxDimensionLength.IsNullOrEmpty AndAlso
                    fieldList.IsNullOrEmpty
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub addField(name As String, offset As Integer, ndims As Integer, dataType As Integer, byteLength As Integer)
            fieldList += New LayoutField(name, offset, ndims, dataType, byteLength)
        End Sub

        Public Overrides Function ToString() As String
            If IsEmpty Then
                Return "null"
            Else
                Return fieldList _
                    .Select(Function(field) field.name) _
                    .GetJson
            End If
        End Function
    End Class
End Namespace
