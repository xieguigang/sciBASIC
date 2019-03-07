#Region "Microsoft.VisualBasic::144e9dc4b325a83ee5d7db5bca3d06cc, mime\application%netcdf\HDF5\structure\Layout.vb"

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

Imports System.Collections.Generic

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Namespace HDF5.[Structure]

	Public Class Layout

		Private m_dataAddress As Long
		Private m_numberOfDimensions As Integer
		Private m_dimensionLength As Integer()
		Private m_maxDimensionLength As Integer()
		Private m_chunkSize As Integer()
		Private m_fields As List(Of LayoutField)

		Public Sub New()
			Me.m_dataAddress = 0
			Me.m_numberOfDimensions = 0
			Me.m_dimensionLength = Nothing
			Me.m_maxDimensionLength = Nothing
			Me.m_chunkSize = Nothing
			Me.m_fields = New List(Of LayoutField)()
		End Sub

		Public Overridable Property dataAddress() As Long
			Get
				Return Me.m_dataAddress
			End Get
			Set
				Me.m_dataAddress = value
			End Set
		End Property


		Public Overridable Property chunkSize() As Integer()
			Get
				Return Me.m_chunkSize
			End Get
			Set
				Me.m_chunkSize = value
			End Set
		End Property


		Public Overridable Property numberOfDimensions() As Integer
			Get
				Return Me.m_numberOfDimensions
			End Get
			Set
				Me.m_numberOfDimensions = value
			End Set
		End Property


		Public Overridable Property dimensionLength() As Integer()
			Get
				Return Me.m_dimensionLength
			End Get
			Set
				Me.m_dimensionLength = value
			End Set
		End Property


		Public Overridable Property maxDimensionLength() As Integer()
			Get
				Return Me.m_maxDimensionLength
			End Get
			Set
				Me.m_maxDimensionLength = value
			End Set
		End Property


		Public Overridable Sub addField(name As String, offset As Integer, ndims As Integer, dataType As Integer, byteLength As Integer)
			Dim lf As New LayoutField(name, offset, ndims, dataType, byteLength)
			Me.m_fields.Add(lf)
		End Sub

		Public Overridable ReadOnly Property fields() As List(Of LayoutField)
			Get
				Return Me.m_fields
			End Get
		End Property
	End Class

End Namespace

