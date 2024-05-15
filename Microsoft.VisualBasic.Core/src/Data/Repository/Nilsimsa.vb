#Region "Microsoft.VisualBasic::f83b8643978d9664198dce48c04097be, Microsoft.VisualBasic.Core\src\Data\Repository\Nilsimsa.vb"

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

    '   Total Lines: 313
    '    Code Lines: 169
    ' Comment Lines: 100
    '   Blank Lines: 44
    '     File Size: 11.91 KB


    '     Class Nilsimsa
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: bitCount, bitwiseDifference, compare, (+3 Overloads) digest, (+2 Overloads) getHash
    '                   (+3 Overloads) hexdigest, reset, tran3, (+2 Overloads) update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Java
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Data.Repository

    ''' <summary>
    ''' Computes the Nilsimsa hash for the given string.
    ''' 
    ''' <para>
    ''' This class is based on the Python implementation by Michael Itz
    ''' http://code.google.com/p/py-nilsimsa
    ''' 
    ''' </para>
    ''' <para>
    ''' Original C nilsimsa-0.2.4 implementation by cmeclax:
    ''' http://ixazon.dynip.com/~cmeclax/nilsimsa.html
    ''' 
    ''' @author Albert Weichselbraun
    ''' </para>
    ''' </summary>
    Public Class Nilsimsa

        ''' <summary>
        ''' num characters seen
        ''' </summary>
        Dim count As Integer = 0
        ''' <summary>
        ''' accumulators for computing the digest
        ''' </summary>
        Dim acc As Integer() = New Integer(255) {}
        ''' <summary>
        ''' the last four seen characters
        ''' </summary>
        Dim lastch As Integer() = New Integer(3) {}
        ''' <summary>
        ''' the Nilsimsa digest
        ''' </summary>
        Dim m_digest As Byte() = Nothing

        ''' <summary>
        ''' pre-defined transformation array
        ''' </summary>
        Shared ReadOnly TRAN As Byte() = New Byte() {
            &H2, &HD6, &H9E, &H6F, &HF9, &H1D, &H4, &HAB, &HD0,
            &H22, &H16, &H1F, &HD8, &H73, &HA1, &HAC, &H3B, &H70,
            &H62, &H96, &H1E, &H6E, &H8F, &H39, &H9D, &H5, &H14,
            &H4A, &HA6, &HBE, &HAE, &HE, &HCF, &HB9, &H9C, &H9A,
            &HC7, &H68, &H13, &HE1, &H2D, &HA4, &HEB, &H51, &H8D,
            &H64, &H6B, &H50, &H23, &H80, &H3, &H41, &HEC, &HBB,
            &H71, &HCC, &H7A, &H86, &H7F, &H98, &HF2, &H36, &H5E,
            &HEE, &H8E, &HCE, &H4F, &HB8, &H32, &HB6, &H5F, &H59,
            &HDC, &H1B, &H31, &H4C, &H7B, &HF0, &H63, &H1, &H6C,
            &HBA, &H7, &HE8, &H12, &H77, &H49, &H3C, &HDA, &H46,
            &HFE, &H2F, &H79, &H1C, &H9B, &H30, &HE3, &H0, &H6,
            &H7E, &H2E, &HF, &H38, &H33, &H21, &HAD, &HA5, &H54,
            &HCA, &HA7, &H29, &HFC, &H5A, &H47, &H69, &H7D, &HC5,
            &H95, &HB5, &HF4, &HB, &H90, &HA3, &H81, &H6D, &H25,
            &H55, &H35, &HF5, &H75, &H74, &HA, &H26, &HBF, &H19,
            &H5C, &H1A, &HC6, &HFF, &H99, &H5D, &H84, &HAA, &H66,
            &H3E, &HAF, &H78, &HB3, &H20, &H43, &HC1, &HED, &H24,
            &HEA, &HE6, &H3F, &H18, &HF3, &HA0, &H42, &H57, &H8,
            &H53, &H60, &HC3, &HC0, &H83, &H40, &H82, &HD7, &H9,
            &HBD, &H44, &H2A, &H67, &HA8, &H93, &HE0, &HC2, &H56,
            &H9F, &HD9, &HDD, &H85, &H15, &HB4, &H8A, &H27, &H28,
            &H92, &H76, &HDE, &HEF, &HF8, &HB2, &HB7, &HC9, &H3D,
            &H45, &H94, &H4B, &H11, &HD, &H65, &HD5, &H34, &H8B,
            &H91, &HC, &HFA, &H87, &HE9, &H7C, &H5B, &HB1, &H4D,
            &HE5, &HD4, &HCB, &H10, &HA2, &H17, &H89, &HBC, &HDB,
            &HB0, &HE2, &H97, &H88, &H52, &HF7, &H48, &HD3, &H61,
            &H2C, &H3A, &H2B, &HD1, &H8C, &HFB, &HF1, &HCD, &HE4,
            &H6A, &HE7, &HA9, &HFD, &HC4, &H37, &HC8, &HD2, &HF6,
            &HDF, &H58, &H72, &H4E
        }

        Public Sub New()
            reset()
        End Sub

        ''' <summary>
        ''' Updates the Nilsimsa digest with the given byte array.
        ''' </summary>
        ''' <param name="data"> the data to consider in the update. </param>
        ''' <returns> The updated Nilsimsa object. </returns>
        Public Function update(data As Byte()) As Nilsimsa
            Dim t0, t1, t2, t3, t4, t5, t6, t7 As Integer

            For Each ch As Integer In data
                ch = ch And &HFF
                count += 1

                ' incr accumulators for triplets
                If lastch(1) > -1 Then
                    t0 = tran3(ch, lastch(0), lastch(1), 0)

                    acc(t0) += 1
                End If

                If lastch(2) > -1 Then
                    t1 = tran3(ch, lastch(0), lastch(2), 1)
                    t2 = tran3(ch, lastch(1), lastch(2), 2)

                    acc(t1) += 1
                    acc(t2) += 1
                End If

                If lastch(3) > -1 Then
                    t3 = tran3(ch, lastch(0), lastch(3), 3)
                    t4 = tran3(ch, lastch(1), lastch(3), 4)
                    t5 = tran3(ch, lastch(2), lastch(3), 5)
                    t6 = tran3(lastch(3), lastch(0), ch, 6)
                    t7 = tran3(lastch(3), lastch(2), ch, 7)

                    acc(t3) += 1
                    acc(t4) += 1
                    acc(t5) += 1
                    acc(t6) += 1
                    acc(t7) += 1
                End If

                ' adjust lastch
                For i As Integer = 3 To 1 Step -1
                    lastch(i) = lastch(i - 1)
                Next

                lastch(0) = ch
            Next

            m_digest = Nothing

            Return Me
        End Function

        ''' <summary>
        ''' Update the current Nilsimsa object with the given String s.
        ''' </summary>
        ''' <param name="s"> the String to add to the hash. </param>
        ''' <returns> The updated Nilsimsa object. </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function update(s As String) As Nilsimsa
            Return update(Encoding.UTF8.GetBytes(s))
        End Function

        ''' <summary>
        ''' Reset the Hash computation.
        ''' </summary>
        ''' <returns> A reset (i.e., empty) Nilsimsa object. </returns>
        Public Function reset() As Nilsimsa
            count = 0
            acc.fill(0)
            lastch.fill(-1)
            m_digest = Nothing
            Return Me
        End Function

        ''' <summary>
        ''' Accumulator for a transition n between the chars a, b, c.
        ''' </summary>
        Private Shared Function tran3(a As Integer, b As Integer, c As Integer, n As Integer) As Integer
            Dim i = c Xor TRAN(n)
            Return (TRAN(a + n And 255) Xor TRAN(b And &HFF) * (n + n + 1)) + TRAN(i And &HFF) And 255
        End Function

        ''' <summary>
        ''' Return the digest for the current Nilsimsa object.
        ''' </summary>
        ''' <returns> The digest of the current Nilsimsa object. </returns>
        Public Function digest() As Byte()
            If m_digest Is Nothing Then
                Dim total = 0

                m_digest = New Byte(31) {}
                m_digest.fill(0)

                If count = 3 Then
                    total = 1
                ElseIf count = 4 Then
                    total = 4
                ElseIf count > 4 Then
                    total = 8 * count - 28
                End If

                Dim threshold As Integer = total / 256

                For i As Integer = 0 To 255
                    If acc(i) > threshold Then
                        m_digest(31 - (i >> 3)) += CByte(1 << (i And 7))
                    End If
                Next
            End If

            Return m_digest
        End Function


        ''' <summary>
        ''' Compute the Nilsimsa digest for the given String.
        ''' </summary>
        ''' <param name="s"> the String to hash </param>
        ''' <returns> The Nilsimsa digest. </returns>
        Public Function digest(s As String) As Byte()
            Return digest(Encoding.UTF8.GetBytes(s))
        End Function


        ''' <summary>
        ''' Compute the Nilsimsa digest for the given String.
        ''' </summary>
        ''' <param name="data"> an array of bytes to hash </param>
        ''' <returns> The Nilsimsa digest. </returns>
        Public Function digest(data As Byte()) As Byte()
            reset()
            update(data)
            Return digest()
        End Function

        ''' <summary>
        ''' Compute the Nilsimsa digest for the given byte array.
        ''' </summary>
        ''' <param name="data"> to hash </param>
        ''' <returns> The byte array's Nilsimsa hash. </returns>
        Public Shared Function getHash(data As Byte()) As Nilsimsa
            Return (New Nilsimsa()).update(data)
        End Function

        ''' <summary>
        ''' Compute the Nilsimsa digest for the given String.
        ''' </summary>
        ''' <param name="s"> the String to hash </param>
        ''' <returns> The String's Nilsimsa hash. </returns>
        Public Shared Function getHash(s As String) As Nilsimsa
            Return getHash(Encoding.UTF8.GetBytes(s))
        End Function

        ''' <summary>
        ''' Return the hex digest of the current Nilsimsa object.
        ''' </summary>
        ''' <returns> A String representation of the current state of the Nilsimsa object. </returns>
        Public Function hexdigest() As String
            Dim s As New StringBuilder()

            For Each b As Byte In digest()
                s.Append(String.Format("{0:x2}", b).ToUpper())
            Next

            Return s.ToString()
        End Function

        ''' <summary>
        ''' Compute the Nilsimsa hexDigest for the given String.
        ''' </summary>
        ''' <param name="data"> an array of bytes to hash </param>
        ''' <returns> The Nilsimsa hexdigest. </returns>
        Public Function hexdigest(data As Byte()) As String
            digest(data)
            Return hexdigest()
        End Function

        ''' <summary>
        ''' Compute the Nilsimsa hexDigest for the given String.
        ''' </summary>
        ''' <param name="s"> the String to hash </param>
        ''' <returns> The Nilsimsa hexdigest. </returns>
        Public Function hexdigest(s As String) As String
            digest(s)
            Return hexdigest()
        End Function

        ''' <summary>
        ''' Compare a Nilsimsa object to the current one and return the number of bits that differ.
        ''' </summary>
        ''' <param name="cmp"> the comparison Nilsimsa object </param>
        ''' <returns> The number of bits in which the Nilsimsa digests differ. </returns>
        Public Function bitwiseDifference(cmp As Nilsimsa) As Integer
            Dim distance = 0
            Dim h1 As Integer
            Dim h2 As Integer

            Dim n1 As Byte() = digest()
            Dim n2 As Byte() = cmp.digest()

            For i As Integer = 0 To 31 Step 4
                h1 = n1(i) And &HFF Or (n1(i + 1) And &HFF) << 8 Or (n1(i + 2) And &HFF) << 16 Or (n1(i + 3) And &HFF) << 24
                h2 = n2(i) And &HFF Or (n2(i + 1) And &HFF) << 8 Or (n2(i + 2) And &HFF) << 16 Or (n2(i + 3) And &HFF) << 24
                distance += bitCount(h1 Xor h2)
            Next

            Return distance
        End Function

        Public Shared Function bitCount(i As Integer) As Integer
            ' HD, Figure 5-2
            i = i - (i >> 1 And &H55555555)
            i = (i And &H33333333) + (i >> 2 And &H33333333)
            i = i + (i >> 4) And &HF0F0F0F
            i = i + (i >> 8)
            i = i + (i >> 16)

            Return i And &H3F
        End Function

        ''' <summary>
        ''' Return a value between -128 and + 128 that indicates the difference between the nilsimsa digest
        ''' of the current object and cmp.
        ''' </summary>
        ''' <param name="cmp"> comparison Nilsimsa object </param>
        ''' <returns> A value between -128 (no matching bits) and 128 (all bits match; both hashes are equal) </returns>
        Public Function compare(cmp As Nilsimsa) As Integer
            Return 128 - bitwiseDifference(cmp)
        End Function
    End Class
End Namespace
