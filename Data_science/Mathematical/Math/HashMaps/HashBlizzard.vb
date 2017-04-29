Imports System.Numerics

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

        ReadOnly __hashBlizzard_seed2 As BigInteger = &HEEEEEEEE

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
        ''' <param name="KeyByte"></param>
        ''' <param name="HasType">HasType =[0 ,1 ,2] </param>
        ''' <returns></returns>
        Public Function HashBlizzard(KeyByte() As Byte, Optional HasType As dwHashTypes = dwHashTypes.Position) As ULong
            Dim L% = KeyByte.Length - 1
            Dim seed1 As BigInteger = &H7FED7FED
            Dim seed2 As BigInteger = __hashBlizzard_seed2
            Dim LoopID% = 0

            While (LoopID < L)
                Dim ascCode% = KeyByte(LoopID)
                seed1 = cryptTable((HasType << 8) + ascCode) Xor (seed1 + seed2)
                seed2 = ascCode + seed1 + seed2 + (seed2 << 5) + 3
                LoopID += 1
            End While

            Return seed1.uncheckedULong
        End Function
    End Class
End Namespace