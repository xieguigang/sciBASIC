'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Namespace HDF5.[Structure]
	Public Class LayoutField

		Private m_name As String
		Private m_offset As Integer
		Private m_ndims As Integer
		Private m_dataType As Integer
		Private m_byteLength As Integer

		Public Sub New(name As String, offset As Integer, ndims As Integer, dataType As Integer, byteLength As Integer)
			Me.m_name = name
			Me.m_offset = offset
			Me.m_ndims = ndims
			Me.m_dataType = dataType
			Me.m_byteLength = byteLength
		End Sub

		Public Overridable ReadOnly Property name() As String
			Get
				Return Me.m_name
			End Get
		End Property

		Public Overridable ReadOnly Property offset() As Integer
			Get
				Return Me.m_offset
			End Get
		End Property

		Public Overridable ReadOnly Property nDims() As Integer
			Get
				Return Me.m_ndims
			End Get
		End Property

		Public Overridable ReadOnly Property dataType() As Integer
			Get
				Return Me.m_dataType
			End Get
		End Property

		Public Overridable ReadOnly Property byteLength() As Integer
			Get
				Return Me.m_byteLength
			End Get
		End Property
	End Class

End Namespace
