#Region "Microsoft.VisualBasic::6d1283cc28a958992c638bb70836a719, Microsoft.VisualBasic.Core\src\My\RlangInterop.vb"

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

    '   Total Lines: 61
    '    Code Lines: 41 (67.21%)
    ' Comment Lines: 11 (18.03%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (14.75%)
    '     File Size: 2.29 KB


    '     Module RlangInterop
    ' 
    '         Function: ProcessingRRawUniCode, ProcessingRUniCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace My

    Public Module RlangInterop

        ''' <summary>
        ''' processing a unicode char like ``&lt;U+767D>`` 
        ''' </summary>
        ''' <param name="output"></param>
        ''' <returns></returns>
        Public Function ProcessingRUniCode(output As String) As String
            Dim unicodes As String() = output.Matches("[<]U[+][A-H0-9]+[>]").Distinct.ToArray
            Dim charCode As Integer
            Dim [char] As Char
            Dim str As New StringBuilder(output)

            For Each code As String In unicodes
                charCode = code.GetStackValue("<", ">").Split("+"c).Last.DoCall(AddressOf i32.GetHexInteger)
                [char] = Strings.ChrW(charCode)
                str.Replace(code, [char])
            Next

            Return str.ToString
        End Function

        ''' <summary>
        ''' processing a unicode char like ``&lt;aa>``
        ''' </summary>
        ''' <param name="output"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Function ProcessingRRawUniCode(output As String, Optional encoding As Encodings = Encodings.Unicode) As String
            Dim raw As String() = output.Matches("([<][a-z0-9]{1,2}[>])+").ToArray
            Dim str As New StringBuilder(output)
            Dim bytes As Byte()
            Dim unicodeStr As String
            Dim charset As Encoding = encoding.CodePage

            For Each part As String In raw
                bytes = part _
                    .Matches("[<][a-z0-9]{1,2}[>]") _
                    .Select(Function(strVal)
                                Return strVal.GetStackValue("<", ">")
                            End Function) _
                    .Select(Function(hex)
                                Return CByte(i32.GetHexInteger(hex))
                            End Function) _
                    .ToArray
                unicodeStr = charset.GetString(bytes)

                str.Replace(part, unicodeStr)
            Next

            Return str.ToString
        End Function
    End Module
End Namespace
