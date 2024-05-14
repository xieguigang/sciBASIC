#Region "Microsoft.VisualBasic::81a5c2d22a24481ea46d0f46e0f319a9, Data\BinaryData\netCDF\DataVector\integers.vb"

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

    '   Total Lines: 46
    '    Code Lines: 35
    ' Comment Lines: 0
    '   Blank Lines: 11
    '     File Size: 1.47 KB


    '     Class integers
    ' 
    '         Properties: cdfDataType
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToFactors, ToFloat, ToInteger, ToLong, ToNumeric
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data

Namespace DataVector

    Public Class integers : Inherits CDFData(Of Integer)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.NC_INT
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(data As IEnumerable(Of Integer))
            buffer = data.ToArray
        End Sub

        Public Overrides Function ToNumeric() As Double()
            Return (From i In buffer Select CDbl(i)).ToArray
        End Function

        Public Overrides Function ToFloat() As Single()
            Return (From i In buffer Select CSng(i)).ToArray
        End Function

        Public Overrides Function ToFactors() As String()
            Return (From i In buffer Select CStr(i)).ToArray
        End Function

        Public Overrides Function ToInteger() As Integer()
            Return (From i In buffer Select CInt(i)).ToArray
        End Function

        Public Overrides Function ToLong() As Long()
            Return (From i In buffer Select CLng(i)).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Integer()) As integers
            Return New integers With {.buffer = data}
        End Operator
    End Class
End Namespace
