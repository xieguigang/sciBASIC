#Region "Microsoft.VisualBasic::3d9cd202001758e90f5bb8a732d149ce, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\Unit\ByteSize.vb"

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

    '   Total Lines: 53
    '    Code Lines: 36 (67.92%)
    ' Comment Lines: 10 (18.87%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (13.21%)
    '     File Size: 1.65 KB


    '     Enum ByteSize
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module ParserExtensions
    ' 
    '         Function: ParseByteSize, ParseByteUnit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Ranges.Unit

    ''' <summary>
    ''' the unit for binary data file size
    ''' </summary>
    Public Enum ByteSize As Long
        B = 1
        KB = 1024
        MB = KB * 1024
        GB = MB * 1024
        TB = GB * 1024
        PB = TB * 1024
    End Enum

    <HideModuleName>
    Public Module ParserExtensions

        ''' <summary>
        ''' Parse the byte size in bytes
        ''' </summary>
        ''' <param name="desc">
        ''' 1111B means 1111 byte
        ''' 1KB means 1024 byte
        ''' 1MB means 1024*1024 byte
        ''' etc
        ''' </param>
        ''' <returns></returns>
        Public Function ParseByteSize(desc As String) As Long
            desc = Strings.Trim(desc)

            If desc = "" Then
                Return 0
            ElseIf desc.IsSimpleNumber Then
                Return CLng(Val(desc))
            End If

            Dim num As String = desc.Match(SimpleNumberPattern)
            Dim unit As String = desc.Replace(num, "").Trim.ToUpper
            Dim unitFlag As ByteSize = ParseByteUnit(unit)
            Dim bytes As Long = CLng(Val(num) * CLng(unitFlag))

            Return bytes
        End Function

        Public Function ParseByteUnit(desc As String) As ByteSize
            Select Case Strings.Trim(desc).ToLower
                Case "" : Return ByteSize.B
                Case "k", "kb" : Return ByteSize.KB
                Case "m", "mb" : Return ByteSize.MB
                Case "g", "gb" : Return ByteSize.GB
                Case "t", "tb" : Return ByteSize.TB
                Case "p", "pb" : Return ByteSize.PB
                Case Else
                    Return ByteSize.B
            End Select
        End Function
    End Module
End Namespace
