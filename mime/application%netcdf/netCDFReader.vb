#Region "Microsoft.VisualBasic::744d821b68c4d06cbdde77dd4d9e2937, mime\application%netcdf\netCDF.vb"

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

' Class netCDF
' 
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language

Public Class netCDFReader

    Dim buffer As BinaryDataReader
    Dim header As Header

    Const Magic$ = "CDF"

    Sub New(buffer As BinaryDataReader)
        Dim version As Value(Of Byte) = Scan0

        buffer.ByteOrder = ByteOrder.BigEndian
        ' Test if file in support format
        Utils.notNetcdf(buffer.ReadString(3) <> Magic, $"should start with {Magic}")
        Utils.notNetcdf((version = buffer.ReadByte) > 2, "unknown version")

        Me.header = New Header(buffer, version)
        Me.buffer = buffer
    End Sub

End Class
