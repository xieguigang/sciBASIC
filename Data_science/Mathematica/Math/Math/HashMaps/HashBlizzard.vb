#Region "Microsoft.VisualBasic::70d7bfa443a67e67497f02731ee07ed9, Data_science\Mathematica\Math\Math\HashMaps\HashBlizzard.vb"

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

    '   Total Lines: 98
    '    Code Lines: 52 (53.06%)
    ' Comment Lines: 34 (34.69%)
    '    - Xml Docs: 97.06%
    ' 
    '   Blank Lines: 12 (12.24%)
    '     File Size: 3.56 KB


    '     Class HashBlizzard
    ' 
    ' 
    '         Enum dwHashTypes
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: (+2 Overloads) HashBlizzard
    ' 
    '     Sub: HashBlizzardInit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports Microsoft.VisualBasic.Math.Numerics

Namespace HashMaps

    ''' <summary>
    ''' Blizzard hash algorithm code
    ''' </summary>
    Public Class HashBlizzard

        ''' <summary>
        ''' The dwHashType: hash value types
        ''' </summary>
        Public Enum dwHashTypes As Long
            ''' <summary>
            ''' Hash ``dwHashType = 0`` calculated values are used to determine the position of the string in a hash table.
            ''' </summary>
            Position = 0
            ''' <summary>
            ''' dwHashType = 1, Hash dwHashType = 2 calculated values are used to validate the string
            ''' </summary>
            Validate1 = 1
            ''' <summary>
            ''' dwHashType = 1, Hash dwHashType = 2 calculated values are used to validate the string
            ''' </summary>
            Validate2 = 2
        End Enum

        ReadOnly cryptTable(&H100 * 5 - 1) As ULong

        Sub New()
            Call HashBlizzardInit()
        End Sub

        Private Sub HashBlizzardInit()
            Dim seed As ULong = &H100001
            Dim index1 As ULong = 0
            Dim index2 As ULong = 0
            Dim I As ULong
            Dim KKK As ULong = 0

            For index1 = 0 To &H100 - 1
                index2 = index1
                For I = 0 To 4
                    Dim temp1, temp2 As ULong
                    seed = (seed * 125 + 3) Mod &H2AAAAB
                    temp1 = (seed And &HFFFF) << &H10
                    seed = (seed * 125 + 3) Mod &H2AAAAB
                    temp2 = (seed And &HFFFF)
                    cryptTable(index2) = (temp1 Or temp2) '//|
                    index2 += &H100
                Next
            Next
        End Sub

        ReadOnly hashBlizzard_seed2 As BigInteger = &HEEEEEEEE

        ''' <summary>
        ''' 暴雪公司出名的哈希码.
        ''' 测试了 二千万 GUID, 没有重复.但运算量比较大。
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="HasType">HasType =0 ,1 ,2 </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### Testing
        ''' 
        ''' ``"unitneutralacritter.grp" -> 0xA26067F3``
        ''' </remarks>
        Public Function HashBlizzard(Key$, Optional HasType As dwHashTypes = dwHashTypes.Position) As ULong
            Return HashBlizzard(Key.Select(Function(c) CByte(Asc(c))).ToArray, HasType)
        End Function

        ''' <summary>
        ''' 暴雪公司著名的 HashMap .
        ''' 测试了 二千万 GUID, 没有重复.但运算量比较大。
        ''' </summary>
        ''' <param name="keyByte"></param>
        ''' <param name="HasType">HasType =[0 ,1 ,2] </param>
        ''' <returns></returns>
        Public Function HashBlizzard(keyByte() As Byte, Optional HasType As dwHashTypes = dwHashTypes.Position) As ULong
            Dim L% = keyByte.Length - 1
            Dim seed1 As BigInteger = &H7FED7FED
            Dim seed2 As BigInteger = hashBlizzard_seed2
            Dim loopID% = 0
            Dim ascCode%

            While (loopID < L)
                ascCode = keyByte(loopID)
                seed1 = cryptTable((HasType << 8) + ascCode) Xor (seed1 + seed2)
                seed2 = ascCode + seed1 + seed2 + (seed2 << 5) + 3
                loopID += 1
            End While

            Return seed1.uncheckedULong
        End Function
    End Class
End Namespace
