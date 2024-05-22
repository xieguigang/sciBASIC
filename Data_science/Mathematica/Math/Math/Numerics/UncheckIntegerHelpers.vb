#Region "Microsoft.VisualBasic::a962fbe21e07df9f2aea4908fab4d346, Data_science\Mathematica\Math\Math\Numerics\UncheckIntegerHelpers.vb"

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
    '    Code Lines: 29 (74.36%)
    ' Comment Lines: 3 (7.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (17.95%)
    '     File Size: 1.23 KB


    '     Module UncheckIntegerExtensions
    ' 
    '         Function: unchecked, uncheckedInteger, uncheckedLong, uncheckedULong
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices

Namespace Numerics

    ''' <summary>
    ''' unchecked arithmetic
    ''' </summary>
    Public Module UncheckIntegerExtensions

        Public Function unchecked(u&) As BigInteger
            Return New BigInteger(u)
        End Function

        <Extension>
        Public Function uncheckedULong(bytes As BigInteger) As ULong
            Dim data As Byte() = bytes.ToByteArray
            If data.Length < 8 Then
                data = data.Join({0, 0, 0, 0, 0, 0, 0, 0}).ToArray
            End If
            Return BitConverter.ToUInt64(data, Scan0)
        End Function

        <Extension>
        Public Function uncheckedLong(bytes As BigInteger) As Long
            Dim data As Byte() = bytes.ToByteArray
            If data.Length < 8 Then
                data = data.Join({0, 0, 0, 0, 0, 0, 0, 0}).ToArray
            End If
            Return BitConverter.ToInt64(data, Scan0)
        End Function

        <Extension>
        Public Function uncheckedInteger(bytes As BigInteger) As Integer
            Return BitConverter.ToInt32(bytes.ToByteArray, Scan0)
        End Function
    End Module

End Namespace
