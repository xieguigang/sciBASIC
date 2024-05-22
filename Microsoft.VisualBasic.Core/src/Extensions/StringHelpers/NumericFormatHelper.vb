#Region "Microsoft.VisualBasic::758f84c692424368c312c6876083878d, Microsoft.VisualBasic.Core\src\Extensions\StringHelpers\NumericFormatHelper.vb"

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

    '   Total Lines: 168
    '    Code Lines: 80 (47.62%)
    ' Comment Lines: 71 (42.26%)
    '    - Xml Docs: 80.28%
    ' 
    '   Blank Lines: 17 (10.12%)
    '     File Size: 4.28 KB


    ' Module NumericFormatHelper
    ' 
    '     Function: [Decimal], Float, SafeToString, SafeToStrings, (+9 Overloads) ToHexString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

''' <summary>
''' ###### ``C``货币
'''
''' ```vbnet
''' 2.5.ToString("C")
''' ' ￥2.50
''' ```
'''
''' ###### ``D``十进制数
'''
''' ```vbnet
''' 25.ToString("D5")
''' ' 00025
''' ```
''' 
''' ###### ``E``科学型
'''
''' ```vbnet
''' 25000.ToString("E")
''' ' 2.500000E+005
''' ```
''' 
''' ###### ``F``固定点
'''
''' ```vbnet
''' 25.ToString("F2")
''' ' 25.00
''' ```
''' 
''' ###### ``G``常规
'''
''' ```vbnet
''' 2.5.ToString("G")
''' ' 2.5
''' ```
'''
''' ###### ``N``数字
'''
''' ```vbnet
''' 2500000.ToString("N")
''' ' 2,500,000.00
''' ```
''' 
''' ###### ``X``十六进制
'''
''' ```vbnet
''' 255.ToString("X")
''' ' FF
''' ```
''' </summary>
Public Module NumericFormatHelper

    ''' <summary>
    ''' ``D&lt;n>``
    ''' </summary>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    Public Function [Decimal](n%) As String
        Return "D" & n
    End Function

    ''' <summary>
    ''' ``F&lt;n>``
    ''' </summary>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    Public Function Float(n%) As String
        Return "F" & n
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x#"></param>
    ''' <param name="NaN_imaginary$">Default using R language style default numeric value</param>
    ''' <returns></returns>
    <Extension>
    Public Function SafeToString(x#, Optional NaN_imaginary$ = "NA") As String
        If x.IsNaNImaginary Then
            Return NaN_imaginary
        Else
            Return CStr(x)
        End If
    End Function

    <Extension>
    Public Function SafeToStrings(v As IEnumerable(Of Double), Optional NaN_imaginary$ = "NA") As String()
        Return v _
            .Select(Function(x) x.SafeToString(NaN_imaginary)) _
            .ToArray
    End Function

#Region "PrimitiveExtensions"

    <Extension>
    Public Function ToHexString(this As Byte) As String
        Return this.ToString("x2")
    End Function

    <Extension>
    Public Function ToHexString(this As Short) As String
        Dim value As UShort = CUShort(this)
        Return ToHexString(value)
    End Function

    <Extension>
    Public Function ToHexString(this As UShort) As String
        Dim reverse As UShort = CUShort(((&HFF00 And this) >> 8) Or ((&HFF And this) << 8))
        Return reverse.ToString("x4")
    End Function

    ''' <summary>
    ''' 转换为16进制
    ''' </summary>
    ''' <param name="this"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ToHexString(this As Integer) As String
        Return ToHexString(CUInt(this))
    End Function

    <Extension>
    Public Function ToHexString(this As UInteger) As String
        Dim reverse As UInteger = ((&HFF000000UI And this) >> 24) Or
            ((&HFF0000 And this) >> 8) Or
            ((&HFF00 And this) << 8) Or
            ((&HFF And this) << 24)

        Return reverse.ToString("x8")
    End Function

    <Extension>
    Public Function ToHexString(this As Long) As String
        Dim value As ULong = CULng(this)
        Return ToHexString(value)
    End Function

    <Extension>
    Public Function ToHexString(this As ULong) As String
        Dim reverse As ULong = (this And &HFFUL) << 56 Or
            (this And &HFF00UL) << 40 Or
            (this And &HFF0000UL) << 24 Or
            (this And &HFF000000UL) << 8 Or
            (this And &HFF00000000UL) >> 8 Or
            (this And &HFF0000000000UL) >> 24 Or
            (this And &HFF000000000000UL) >> 40 Or
            (this And &HFF00000000000000UL) >> 56

        Return reverse.ToString("x16")
    End Function

    <Extension>
    Public Function ToHexString(this As Single) As String
        Dim value As UInteger = Numeric.ToUInt32(this)
        Return ToHexString(value)
    End Function

    <Extension>
    Public Function ToHexString(this As Double) As String
        Dim value As ULong = Numeric.ToUInt64(this)
        Return ToHexString(value)
    End Function
#End Region
End Module
