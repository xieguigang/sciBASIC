#Region "Microsoft.VisualBasic::3c5b3e380d8a8ed4ddc34e8dbd47c9e9, Data\BinaryData\netCDF\Components\ICDFDataVector.vb"

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

    '   Total Lines: 15
    '    Code Lines: 10
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 386 B


    '     Interface ICDFDataVector
    ' 
    '         Properties: cdfDataType, genericValue, length
    ' 
    '         Function: GetBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data

Namespace Components

    Public Interface ICDFDataVector

        ReadOnly Property cdfDataType As CDFDataTypes
        ReadOnly Property genericValue As Array
        ReadOnly Property length As Integer

        Function GetBuffer(encoding As Encoding) As Byte()

    End Interface
End Namespace
