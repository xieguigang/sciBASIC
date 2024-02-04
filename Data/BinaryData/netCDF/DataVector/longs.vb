﻿#Region "Microsoft.VisualBasic::8943d6bbbb801f5619d6508b59b4c057, sciBASIC#\Data\BinaryData\netCDF\DataVector\longs.vb"

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

'   Total Lines: 19
'    Code Lines: 15
' Comment Lines: 0
'   Blank Lines: 4
'     File Size: 596 B


'     Class longs
' 
'         Properties: cdfDataType
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data

Namespace DataVector

    Public Class longs : Inherits CDFData(Of Long)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.NC_INT64
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(i64 As IEnumerable(Of Long))
            buffer = i64.ToArray
        End Sub

        Sub New(iu32 As IEnumerable(Of UInteger))
            Call Me.New(From i As UInteger In iu32 Select CLng(i))
        End Sub

        Sub New(i32 As IEnumerable(Of Integer))
            Call Me.New(From i As Integer In i32 Select CLng(i))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Long()) As longs
            Return New longs With {.buffer = data}
        End Operator
    End Class
End Namespace
