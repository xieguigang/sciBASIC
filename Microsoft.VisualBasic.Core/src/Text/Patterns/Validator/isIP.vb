#Region "Microsoft.VisualBasic::690724c8edefb7914a2be740adff0aad, Microsoft.VisualBasic.Core\src\Text\Patterns\Validator\isIP.vb"

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

    '   Total Lines: 118
    '    Code Lines: 68 (57.63%)
    ' Comment Lines: 36 (30.51%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (11.86%)
    '     File Size: 5.11 KB


    '     Module Validator
    ' 
    '         Function: isIP
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Text.Patterns.Validator

    Partial Module Validator

        '
        ' 11.3.  Examples
        ' 
        ' The following addresses
        '    fe80::1234 (on the 1st link of the node)
        '    ff02::5678 (on the 5th link of the node)
        '    ff08::9abc (on the 10th organization of the node)
        ' would be represented as follows:
        '    fe80::1234%1
        '    ff02::5678%5
        '    ff08::9abc%10
        ' (Here we assume a natural translation from a zone index to the
        ' <zone_id> part, where the Nth zone of any scope is translated into
        ' "N".)
        ' If we use interface names as <zone_id>, those addresses could also be
        ' represented as follows:
        '    fe80::1234%ne0
        '    ff02::5678%pvc1.3
        '    ff08::9abc%interface10
        ' where the interface "ne0" belongs to the 1st link, "pvc1.3" belongs
        ' to the 5th link, and "interface10" belongs to the 10th organization.
        ' 

        ReadOnly ipv4Maybe As New Regex("^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$", RegexICMul)
        ReadOnly ipv6Block As New Regex("^[0-9A-F]{1,4}$", RegexICMul)

        Public Function isIP(str As String, Optional version$ = "") As Boolean
            If (version.StringEmpty) Then
                Return isIP(str, 4) OrElse isIP(str, 6)
            ElseIf (version = "4") Then
                If (Not ipv4Maybe.test(str)) Then
                    Return False
                Else
                    Dim parts = str.Split("."c).sort(Function(a, b) a - b).ToArray

                    Return [String].parseInt(parts(3)) <= 255
                End If
            ElseIf (version = "6") Then
                Dim addressAndZone = {str}
                ' ipv6 addresses could have scoped architecture
                ' according to https://tools.ietf.org/html/rfc4007#section-11
                If (str.includes("%")) Then
                    addressAndZone = str.Split("%"c)
                    If (addressAndZone.Length <> 2) Then
                        ' it must be just two parts
                        Return False
                    End If
                    If (Not addressAndZone(0).includes(":"c)) Then
                        ' the first part must be the address
                        Return False
                    End If

                    If (addressAndZone(1) = "") Then
                        ' the second part must not be empty
                        Return False
                    End If
                End If

                Dim blocks = addressAndZone(0).Split(":"c)
                Dim foundOmissionBlock = False ' marker to indicate ::

                ' At least some OS accept the last 32 bits of an IPv6 address
                ' (i.e. 2 of the blocks) in IPv4 notation, and RFC 3493 says
                ' that '::ffff:a.b.c.d' is valid for IPv4-mapped IPv6 addresses,
                ' and '::a.b.c.d' is deprecated, but also valid.
                Dim foundIPv4TransitionBlock = isIP(blocks(blocks.Length - 1), 4)
                Dim expectedNumberOfBlocks = If(foundIPv4TransitionBlock, 7, 8)

                If (blocks.Length > expectedNumberOfBlocks) Then
                    Return False
                End If
                ' initial or final ::
                If (str = "::") Then
                    Return True
                ElseIf (str.substr(0, 2) = "::") Then
                    blocks.shift()
                    blocks.shift()
                    foundOmissionBlock = True
                ElseIf (str.substr(str.Length - 2) = "::") Then
                    blocks.pop()
                    blocks.pop()
                    foundOmissionBlock = True
                End If

                For i As Integer = 0 To blocks.Length - 1
                    ' test for a :: which can not be at the string start/end
                    ' since those cases have been handled above
                    If (blocks(i) = "" AndAlso i > 0 AndAlso i < blocks.Length - 1) Then
                        If (foundOmissionBlock) Then
                            Return False ' multiple :: in address
                        End If
                        foundOmissionBlock = True
                    ElseIf (foundIPv4TransitionBlock AndAlso i = blocks.Length - 1) Then
                        ' it has been checked before that the last
                        ' block is a valid IPv4 address
                    ElseIf (Not ipv6Block.test(blocks(i))) Then
                        Return False
                    End If
                Next

                If (foundOmissionBlock) Then
                    Return blocks.Length >= 1
                End If

                Return blocks.Length = expectedNumberOfBlocks
            End If

            Return False
        End Function
    End Module
End Namespace
