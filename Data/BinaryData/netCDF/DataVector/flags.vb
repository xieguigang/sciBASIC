#Region "Microsoft.VisualBasic::ae8ee92ae0b6ee173857192f42e19ca3, Data\BinaryData\netCDF\DataVector\flags.vb"

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

    '   Total Lines: 39
    '    Code Lines: 30
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.35 KB


    '     Class flags
    ' 
    '         Properties: cdfDataType
    ' 
    '         Function: ToFactors, ToFloat, ToInteger, ToLong, ToNumeric
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data

Namespace DataVector

    Public Class flags : Inherits CDFData(Of Boolean)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.BOOLEAN
            End Get
        End Property

        Public Overrides Function ToNumeric() As Double()
            Return (From i In buffer Select If(i, 1.0, 0.0)).ToArray
        End Function

        Public Overrides Function ToFloat() As Single()
            Return (From i In buffer Select If(i, 1.0F, 0.0F)).ToArray
        End Function

        Public Overrides Function ToFactors() As String()
            Return (From i In buffer Select CStr(i)).ToArray
        End Function

        Public Overrides Function ToInteger() As Integer()
            Return (From i In buffer Select If(i, 1, 0)).ToArray
        End Function

        Public Overrides Function ToLong() As Long()
            Return (From i In buffer Select If(i, 1L, 0L)).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Boolean()) As flags
            Return New flags With {.buffer = data}
        End Operator
    End Class
End Namespace
