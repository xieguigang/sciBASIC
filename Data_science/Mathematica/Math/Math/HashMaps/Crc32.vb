#Region "Microsoft.VisualBasic::870564f55daf9d9f6c41a05626b4e5cc, Data_science\Mathematica\Math\Math\HashMaps\Crc32.vb"

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

    '   Total Lines: 116
    '    Code Lines: 98
    ' Comment Lines: 8
    '   Blank Lines: 10
    '     File Size: 5.57 KB


    ' Class Crc32
    ' 
    '     Properties: CheckSum
    ' 
    '     Function: (+2 Overloads) AddToCRC32, CRC32Bytes, CRC32String, UPDC32
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

''' <summary>
''' Crc32校验码
''' </summary>
Public Class Crc32

    ''' <summary>
    ''' crc32
    ''' </summary>
    ''' <returns></returns>
    Public Property CheckSum As Integer

    Shared ReadOnly crc_32_tab%() = {
        0,
        &H77073096, &HEE0E612C, &H990951BA, &H76DC419, &H706AF48F,
        &HE963A535, &H9E6495A3, &HEDB8832, &H79DCB8A4, &HE0D5E91E,
        &H97D2D988, &H9B64C2B, &H7EB17CBD, &HE7B82D07, &H90BF1D91,
        &H1DB71064, &H6AB020F2, &HF3B97148, &H84BE41DE, &H1ADAD47D,
        &H6DDDE4EB, &HF4D4B551, &H83D385C7, &H136C9856, &H646BA8C0,
        &HFD62F97A, &H8A65C9EC, &H14015C4F, &H63066CD9, &HFA0F3D63,
        &H8D080DF5, &H3B6E20C8, &H4C69105E, &HD56041E4, &HA2677172,
        &H3C03E4D1, &H4B04D447, &HD20D85FD, &HA50AB56B, &H35B5A8FA,
        &H42B2986C, &HDBBBC9D6, &HACBCF940, &H32D86CE3, &H45DF5C75,
        &HDCD60DCF, &HABD13D59, &H26D930AC, &H51DE003A, &HC8D75180,
        &HBFD06116, &H21B4F4B5, &H56B3C423, &HCFBA9599, &HB8BDA50F,
        &H2802B89E, &H5F058808, &HC60CD9B2, &HB10BE924, &H2F6F7C87,
        &H58684C11, &HC1611DAB, &HB6662D3D, &H76DC4190, &H1DB7106,
        &H98D220BC, &HEFD5102A, &H71B18589, &H6B6B51F, &H9FBFE4A5,
        &HE8B8D433, &H7807C9A2, &HF00F934, &H9609A88E, &HE10E9818,
        &H7F6A0DBB, &H86D3D2D, &H91646C97, &HE6635C01, &H6B6B51F4,
        &H1C6C6162, &H856530D8, &HF262004E, &H6C0695ED, &H1B01A57B,
        &H8208F4C1, &HF50FC457, &H65B0D9C6, &H12B7E950, &H8BBEB8EA,
        &HFCB9887C, &H62DD1DDF, &H15DA2D49, &H8CD37CF3, &HFBD44C65,
        &H4DB26158, &H3AB551CE, &HA3BC0074, &HD4BB30E2, &H4ADFA541,
        &H3DD895D7, &HA4D1C46D, &HD3D6F4FB, &H4369E96A, &H346ED9FC,
        &HAD678846, &HDA60B8D0, &H44042D73, &H33031DE5, &HAA0A4C5F,
        &HDD0D7CC9, &H5005713C, &H270241AA, &HBE0B1010, &HC90C2086,
        &H5768B525, &H206F85B3, &HB966D409, &HCE61E49F, &H5EDEF90E,
        &H29D9C998, &HB0D09822, &HC7D7A8B4, &H59B33D17, &H2EB40D81,
        &HB7BD5C3B, &HC0BA6CAD, &HEDB88320, &H9ABFB3B6, &H3B6E20C,
        &H74B1D29A, &HEAD54739, &H9DD277AF, &H4DB2615, &H73DC1683,
        &HE3630B12, &H94643B84, &HD6D6A3E, &H7A6A5AA8, &HE40ECF0B,
        &H9309FF9D, &HA00AE27, &H7D079EB1, &HF00F9344, &H8708A3D2,
        &H1E01F268, &H6906C2FE, &HF762575D, &H806567CB, &H196C3671,
        &H6E6B06E7, &HFED41B76, &H89D32BE0, &H10DA7A5A, &H67DD4ACC,
        &HF9B9DF6F, &H8EBEEFF9, &H17B7BE43, &H60B08ED5, &HD6D6A3E8,
        &HA1D1937E, &H38D8C2C4, &H4FDFF252, &HD1BB67F1, &HA6BC5767,
        &H3FB506DD, &H48B2364B, &HD80D2BDA, &HAF0A1B4C, &H36034AF6,
        &H41047A60, &HDF60EFC3, &HA867DF55, &H316E8EEF, &H4669BE79,
        &HCB61B38C, &HBC66831A, &H256FD2A0, &H5268E236, &HCC0C7795,
        &HBB0B4703, &H220216B9, &H5505262F, &HC5BA3BBE, &HB2BD0B28,
        &H2BB45A92, &H5CB36A04, &HC2D7FFA7, &HB5D0CF31, &H2CD99E8B,
        &H5BDEAE1D, &H9B64C2B0, &HEC63F226, &H756AA39C, &H26D930A,
        &H9C0906A9, &HEB0E363F, &H72076785, &H5005713, &H95BF4A82,
        &HE2B87A14, &H7BB12BAE, &HCB61B38, &H92D28E9B, &HE5D5BE0D,
        &H7CDCEFB7, &HBDBDF21, &H86D3D2D4, &HF1D4E242, &H68DDB3F8,
        &H1FDA836E, &H81BE16CD, &HF6B9265B, &H6FB077E1, &H18B74777,
        &H88085AE6, &HFF0F6A70, &H66063BCA, &H11010B5C, &H8F659EFF,
        &HF862AE69, &H616BFFD3, &H166CCF45, &HA00AE278, &HD70DD2EE,
        &H4E048354, &H3903B3C2, &HA7672661, &HD06016F7, &H4969474D,
        &H3E6E77DB, &HAED16A4A, &HD9D65ADC, &H40DF0B66, &H37D83BF0,
        &HA9BCAE53, &HDEBB9EC5, &H47B2CF7F, &H30B5FFE9, &HBDBDF21C,
        &HCABAC28A, &H53B39330, &H24B4A3A6, &HBAD03605, &HCDD70693,
        &H54DE5729, &H23D967BF, &HB3667A2E, &HC4614AB8, &H5D681B02,
        &H2A6F2B94, &HB40BBE37, &HC30C8EA1, &H5A05DF1B, &H2D02EF8D
    }

    Public Function AddToCRC32(c As Integer) As UInt32
        Return AddToCRC32(CUShort(c))
    End Function

    Public Function AddToCRC32(c As UInt16) As UInt32
        Dim octet As Byte = CByte((c And &HFF))
        Dim num2 As Byte = CByte((c >> 8))
        _CheckSum = Crc32.UPDC32(num2, _CheckSum)
        _CheckSum = Crc32.UPDC32(octet, _CheckSum)
        Return Not _CheckSum
    End Function

    Public Shared Function CRC32Bytes(bytes As Byte()) As UInt32
        Dim maxValue As UInt32 = UInt32.MaxValue
        Dim length As Integer = bytes.Length
        Dim i As Integer = 0
        Do While (length > 0)
            length -= 1
            maxValue = Crc32.UPDC32(bytes(length), maxValue)
            i += 1
        Loop
        Return Not maxValue
    End Function

    Public Shared Function CRC32String([text] As String) As UInt32
        Dim maxValue As UInt32 = UInt32.MaxValue
        Dim length As Integer = [text].Length
        Dim i As Integer = 0

        Do While (length > 0)
            length -= 1
            Dim num3 As UInt16 = AscW([text].Chars(length))
            Dim octet As Byte = CByte((num3 And &HFF))
            Dim num5 As Byte = CByte((num3 >> 8))
            maxValue = Crc32.UPDC32(num5, maxValue)
            maxValue = Crc32.UPDC32(octet, maxValue)
            i += 1
        Loop

        Return Not maxValue
    End Function

    ' #define UPDC32(octet,crc) (crc_32_tab[((crc) ^ ((uint8_t)octet)) & 0xff] ^ ((crc) >> 8))
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function UPDC32(octet As Byte, crc As UInt32) As UInt32
        Return (Crc32.crc_32_tab(CInt((((crc) Xor octet) And &HFF))) Xor (crc >> 8))
    End Function
End Class
