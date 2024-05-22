#Region "Microsoft.VisualBasic::93aabb6720b0e557cb95331a3921247f, Data\BinaryData\HDF5\types\FloatingPoint.vb"

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

    '   Total Lines: 36
    '    Code Lines: 32 (88.89%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (11.11%)
    '     File Size: 1.39 KB


    '     Class FloatingPoint
    ' 
    '         Properties: bitOffset, bitPrecision, byteOrder, exponentBias, exponentLocation
    '                     exponentSize, highPadding, internalPadding, lowPadding, mantissaLocation
    '                     mantissaNormalization, mantissaSize, signLocation, TypeInfo
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace type

    Public Class FloatingPoint : Inherits DataType

        Public Property byteOrder As ByteOrder
        Public Property lowPadding As Boolean
        Public Property highPadding As Boolean
        Public Property internalPadding As Boolean
        Public Property mantissaNormalization As Integer
        Public Property signLocation As Integer
        Public Property bitOffset As Short
        Public Property bitPrecision As Short
        Public Property exponentLocation As SByte
        Public Property exponentSize As SByte
        Public Property mantissaLocation As SByte
        Public Property mantissaSize As SByte
        Public Property exponentBias As Integer

        Public Overrides ReadOnly Property TypeInfo As System.Type
            Get
                Select Case bitPrecision
                    Case 16, 32
                        Return GetType(Single)
                    Case 64
                        Return GetType(Double)
                    Case Else
                        Throw New NotSupportedException("Unsupported signed fixed point data type")
                End Select
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"({byteOrder.ToString} {Me.GetType.Name}) {TypeInfo.FullName}"
        End Function
    End Class
End Namespace
