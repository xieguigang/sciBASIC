#Region "Microsoft.VisualBasic::55b7048f639616105ec34961ebfdeb43, Data\BinaryData\HDF5\types\FixedPoint.vb"

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

    '   Total Lines: 44
    '    Code Lines: 39 (88.64%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (11.36%)
    '     File Size: 1.61 KB


    '     Class FixedPoint
    ' 
    '         Properties: bitOffset, bitPrecision, byteOrder, highPadding, lowPadding
    '                     signed, typeInfo
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics

Namespace type

    Public Class FixedPoint : Inherits DataType

        Public Property byteOrder As ByteOrder
        Public Property lowPadding As Boolean
        Public Property highPadding As Boolean
        Public Property signed As Boolean
        Public Property bitOffset As Short
        Public Property bitPrecision As Short

        Public Overrides ReadOnly Property typeInfo As System.Type
            Get
                If signed Then
                    Select Case bitPrecision
                        Case 8 : Return GetType(SByte)
                        Case 16 : Return GetType(Short)
                        Case 32 : Return GetType(Integer)
                        Case 64 : Return GetType(Long)
                        Case Else
                            Throw New NotSupportedException
                    End Select
                Else
                    Select Case bitPrecision
                        Case 8, 16
                            Return GetType(Integer)
                        Case 32
                            Return GetType(Long)
                        Case 64
                            Return GetType(BigInteger)
                        Case Else
                            Throw New NotSupportedException
                    End Select
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"({byteOrder.ToString} {Me.GetType.Name}) {typeInfo.FullName}"
        End Function
    End Class
End Namespace
