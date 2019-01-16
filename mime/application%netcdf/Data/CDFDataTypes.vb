#Region "Microsoft.VisualBasic::0c9b05f09bf9f51e59576d0f689cc1d6, mime\application%netcdf\CDFDataTypes.vb"

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

' Enum CDFDataTypes
' 
' 
'  
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel

''' <summary>
''' The enum values of the CDF data types.
''' </summary>
Public Enum CDFDataTypes As Integer
    <Description("undefined")> undefined = -1

    <Description("byte")> [BYTE] = 1
    <Description("char")> [CHAR] = 2
    <Description("short")> [SHORT] = 3
    <Description("int")> [INT] = 4
    <Description("float")> [FLOAT] = 5
    <Description("double")> [DOUBLE] = 6
End Enum
