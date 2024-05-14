#Region "Microsoft.VisualBasic::35ac5ce56edf3247ba658ffd654a23e9, Microsoft.VisualBasic.Core\src\Data\Repository\BKDRHash.vb"

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

    '   Total Lines: 83
    '    Code Lines: 49
    ' Comment Lines: 19
    '   Blank Lines: 15
    '     File Size: 3.01 KB


    '     Class BKDRHash
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GenerateVersion2, GenerateVersion3, ToUTF8, ToUTF8Bytes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Data.Repository

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/fernandezja/ColorHashSharp/blob/master/src/ColorHashSharp/BKDRHash.cs
    ''' </remarks>
    Public NotInheritable Class BKDRHash

        Const PADDING_CHAR As Char = "x"c

        ''' <summary>
        ''' The Number.MAX_SAFE_INTEGER constant represents the maximum safe integer in JavaScript  
        ''' https://developer.mozilla.org/es/docs/Web/JavaScript/Referencia/Objetos_globales/Number/MAX_SAFE_INTEGER
        ''' </summary>
        Const JAVASCRIPT_MAX_SAFE_INTEGER As ULong = 9007199254740991
        Const SEED As ULong = 131
        Const SEED2 As ULong = 137

        Private Sub New()
        End Sub

        Public Shared Function GenerateVersion2(value As String) As ULong
            Dim hashcode As Long = 0
            Dim bytes As Byte()
            ' Make hash more sensitive for short string like 'a', 'b', 'c'
            Dim valueWithPadding = $"{value}{PADDING_CHAR}"
            Dim valueUtf8 = ToUTF8(valueWithPadding)
            Dim max = Long.MaxValue / SEED

            For i As Integer = 0 To valueUtf8.Length - 1
                If hashcode > max Then
                    hashcode = hashcode / SEED2
                End If

                bytes = ToUTF8Bytes(valueUtf8(i).ToString())
                hashcode = hashcode * CLng(SEED) + bytes(0)
            Next

            Return hashcode
        End Function

        ''' <summary>
        ''' BKDR Hash (modified version). Idem  original code.
        ''' https://github.com/zenozeng/color-hash/blob/master/lib/bkdr-hash.js
        ''' Example values nodejs https://repl.it/@Jose_AA/BKDR-Hash
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Shared Function GenerateVersion3(value As String) As ULong
            Dim hash As ULong = 0
            Dim bytes As Byte()
            ' Make hash more sensitive for short string like 'a', 'b', 'c'
            Dim valueWithPadding = $"{value}{PADDING_CHAR}"
            Dim valueUtf8 = ToUTF8(valueWithPadding)
            Dim max = JAVASCRIPT_MAX_SAFE_INTEGER / SEED

            For i As Integer = 0 To valueUtf8.Length - 1
                If hash > max Then
                    hash = hash / SEED2
                End If

                bytes = ToUTF8Bytes(valueUtf8(i).ToString())
                hash = hash * SEED + bytes(0)
            Next

            Return hash
        End Function

        Private Shared Function ToUTF8(value As String) As String
            Dim bytes = Encoding.Default.GetBytes(value)
            Return Encoding.UTF8.GetString(bytes)
        End Function

        Private Shared Function ToUTF8Bytes(value As String) As Byte()
            Dim bytes = Encoding.UTF8.GetBytes(value)
            Return bytes
        End Function
    End Class
End Namespace
