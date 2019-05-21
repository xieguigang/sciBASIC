#Region "Microsoft.VisualBasic::0643358e0848cab314fd791e9176d16f, mime\application%netcdf\HDF5\structure\LayoutField.vb"

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

    ' 	Class LayoutField
    ' 
    ' 	    Properties: byteLength, dataType, name, nDims, offset
    ' 
    ' 	    Constructor: (+1 Overloads) Sub New
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

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {dataType.ToString} = [&{offset}, {byteLength}]"
        End Function
    End Class

End Namespace
