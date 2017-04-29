Namespace HashMaps

    Public Class HashBlizzard

        ReadOnly cryptTable(&H100 * 5 - 1) As UInt64

        Sub New()
            Call HashBlizzardInit()
        End Sub

        Private Sub HashBlizzardInit()
            Dim seed As UInt64 = &H100001
            Dim index1 As UInt64 = 0
            Dim index2 As UInt64 = 0
            Dim I As UInt64
            Dim KKK As UInt64 = 0

            For index1 = 0 To &H100 - 1
                index2 = index1
                For I = 0 To 4
                    Dim temp1, temp2 As UInt64
                    seed = (seed * 125 + 3) Mod &H2AAAAB
                    temp1 = (seed And &HFFFF) << &H10
                    seed = (seed * 125 + 3) Mod &H2AAAAB
                    temp2 = (seed And &HFFFF)
                    cryptTable(index2) = (temp1 Or temp2) '//|
                    index2 += &H100
                Next
            Next
        End Sub

        Const __hashBlizzard_seed2& = &HEEEEEEEE

        ''' <summary>
        ''' 暴雪公司出名的哈希码.
        ''' 测试了 二千万 GUID, 没有重复.但运算量比较大。
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="HasType">HasType =0 ,1 ,2 </param>
        ''' <returns></returns>
        Public Function HashBlizzard(Key As String, Optional HasType As Long = 0) As UInt64
            Dim L As Int32 = Key.Length - 1
            Dim KeyCharArr() As Char = Key.ToArray
            Dim seed1 As UncheckedUInt64 = &H7FED7FED
            Dim seed2 As New UncheckedUInt64(__hashBlizzard_seed2)
            Dim LoopID As Int32 = 0

            While (LoopID < L)
                Dim ascCode As Int32 = Asc(KeyCharArr(LoopID))
                seed1 = cryptTable((HasType << 8) + ascCode) Xor (seed1 + seed2)
                seed2 = ascCode + seed1 + seed2 + (seed2 << 5) + 3
                LoopID += 1
            End While

            Return seed1
        End Function

        ''' <summary>
        ''' 暴雪公司著名的 HashMap .
        ''' 测试了 二千万 GUID, 没有重复.但运算量比较大。
        ''' </summary>
        ''' <param name="KeyByte"></param>
        ''' <param name="HasType">HasType =[0 ,1 ,2] </param>
        ''' <returns></returns>
        Public Function HashBlizzard(KeyByte() As Byte, Optional HasType As Long = 0) As UInt64
            Dim L As Int32 = KeyByte.Length - 1
            Dim seed1 As UncheckedUInt64 = &H7FED7FED
            Dim seed2 As New UncheckedUInt64(__hashBlizzard_seed2)
            Dim LoopID As Int32 = 0

            While (LoopID < L)
                Dim ascCode As Int32 = KeyByte(LoopID)
                seed1 = cryptTable((HasType << 8) + ascCode) Xor (seed1 + seed2)
                seed2 = ascCode + seed1 + seed2 + (seed2 << 5) + 3
                LoopID += 1
            End While

            Return seed1
        End Function
    End Class
End Namespace