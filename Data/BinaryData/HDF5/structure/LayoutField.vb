#Region "Microsoft.VisualBasic::60d39a262667e6eaaf9b15e3277d4962, Data\BinaryData\HDF5\structure\LayoutField.vb"

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

    '   Total Lines: 34
    '    Code Lines: 20 (58.82%)
    ' Comment Lines: 6 (17.65%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (23.53%)
    '     File Size: 1.03 KB


    '     Class LayoutField
    ' 
    '         Properties: byteLength, dataType, name, nDims, offset
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
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


Imports Microsoft.VisualBasic.Data.IO.HDF5.type

Namespace struct

    Public Class LayoutField

        Public ReadOnly Property name As String
        Public ReadOnly Property offset As Integer
        Public ReadOnly Property nDims As Integer
        Public ReadOnly Property dataType As DataTypes
        Public ReadOnly Property byteLength As Integer

        Public Sub New(name As String, offset As Integer, ndims As Integer, dataType As Integer, byteLength As Integer)
            Me.name = name
            Me.offset = offset
            Me.nDims = ndims
            Me.dataType = CType(dataType, DataTypes)
            Me.byteLength = byteLength
        End Sub

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {dataType.ToString} = [&{offset}, {byteLength}]"
        End Function
    End Class

End Namespace
