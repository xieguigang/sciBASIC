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
