'
' Copyright 2011 Mozilla Foundation And contributors
' Licensed under the New BSD license. See LICENSE Or:
' http//opensource.org/licenses/BSD-3-Clause
'
' Based on the Base 64 VLQ implementation in Closure Compiler:
' https//code.google.com/p/closure-compiler/source/browse/trunk/src/com/google/debugging/sourcemap/Base64VLQ.java
'
' Copyright 2011 The Closure Compiler Authors. All rights reserved.
' Redistribution And use in source And binary forms, with Or without
' modification, are permitted provided that the following conditions are
' met:
'
'  * Redistributions of source code must retain the above copyright
'    notice, this list of conditions And the following disclaimer.
'  * Redistributions in binary form must reproduce the above
'    copyright notice, this list of conditions And the following
'    disclaimer in the documentation And/Or other materials provided
'    with the distribution.
'  * Neither the name of Google Inc. nor the names of its
'    contributors may be used to endorse Or promote products derived
'    from this software without specific prior written permission.
'
' THIS SOFTWARE Is PROVIDED BY THE COPYRIGHT HOLDERS And CONTRIBUTORS
' "AS IS" And ANY EXPRESS Or IMPLIED WARRANTIES, INCLUDING, BUT Not
' LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY And FITNESS FOR
' A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
' OWNER Or CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
' SPECIAL, EXEMPLARY, Or CONSEQUENTIAL DAMAGES (INCLUDING, BUT Not
' LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS Or SERVICES; LOSS OF USE,
' DATA, Or PROFITS; Or BUSINESS INTERRUPTION) HOWEVER CAUSED And ON ANY
' THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, Or TORT
' (INCLUDING NEGLIGENCE Or OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
' OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace SourceMap

    ''' <summary>
    ''' VLQ编码
    ''' 
    ''' > https://github.com/mozilla/source-map/blob/master/lib/base64-vlq.js
    ''' </summary>
    Public Module base64VLQ

        ''' <summary>
        ''' A single base 64 digit can contain 6 bits of data. For the base 64 variable
        ''' length quantities we use in the source map spec, the first bit Is the sign,
        ''' the next four bits are the actual value, And the 6th bit Is the
        ''' continuation bit. The continuation bit tells us whether there are more
        ''' digits in this value following this digit.
        '''
        ''' ```
        '''   Continuation
        '''   |    Sign
        '''   |    |
        '''   V    V
        '''   101011
        ''' ```  
        ''' </summary>
        Const VLQ_BASE_SHIFT = 5

        ''' <summary>
        ''' binary 100000
        ''' </summary>        
        Const VLQ_BASE = 1 << VLQ_BASE_SHIFT

        ''' <summary>
        ''' binary 011111
        ''' </summary>
        Const VLQ_BASE_MASK = VLQ_BASE - 1

        ''' <summary>
        ''' binary 100000
        ''' </summary>        
        Const VLQ_CONTINUATION_BIT = VLQ_BASE

        Const Base64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" &
                       "abcdefghijklmnopqrstuvwxyz" &
                       "0123456789+/"

        ReadOnly fromBase64 As New Dictionary(Of Char, Integer)

        Sub New()
            For Each c As SeqValue(Of Char) In Base64.SeqIterator
                fromBase64(c.value) = c
            Next
        End Sub

        ''' <summary>
        ''' Converts from a two-complement value to a value where the sign bit Is
        ''' placed in the least significant bit.  
        ''' 
        ''' For example, as decimals:
        ''' 
        ''' + 1 becomes 2 (10 binary), -1 becomes 3 (11 binary)
        ''' + 2 becomes 4 (100 binary), -2 becomes 5 (101 binary)
        ''' 
        ''' </summary>
        ''' <param name="a"></param>
        ''' <returns></returns>
        Function toVLQSigned(a As Integer) As Integer
            If a < 0 Then
                Return (-a << 1) + 1
            Else
                Return (a << 1) + 0
            End If
        End Function

        ''' <summary>
        ''' Converts to a two-complement value from a value where the sign bit Is
        ''' Is placed in the least significant bit.  
        ''' 
        ''' For example, as decimals:
        ''' 
        ''' + 2 (10 binary) becomes 1, 3 (11 binary) becomes -1
        ''' + 4 (100 binary) becomes 2, 5 (101 binary) becomes -2
        ''' 
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Private Function fromVLQSigned(value As Integer) As Integer
            Dim negate = (value And 1) = 1
            value = value >> 1
            Return If(negate, -value, value)
        End Function

        Public Function base64VLQ_encode(aValue As Integer) As String
            Dim encoded As New List(Of Char)
            Dim digit As Integer
            Dim vlq As Integer = toVLQSigned(aValue)

            Do
                digit = vlq And VLQ_BASE_MASK
                vlq >>= VLQ_BASE_SHIFT

                If vlq > 0 Then
                    ' There are still more digits in this value, so we must make sure the
                    ' continuation bit Is marked.
                    digit = digit Or VLQ_CONTINUATION_BIT
                End If

                encoded += Base64(digit)

                If vlq <= 0 Then
                    Exit Do
                End If
            Loop

            Return encoded.CharString
        End Function

        ''' <summary>
        ''' Decodes the next VLQValue from the provided chars.
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <returns></returns>
        Public Function base64VLQ_decode([in] As String) As Integer
            Dim chars As CharEnumerator = [in].GetEnumerator
            Call chars.MoveNext()
            Return base64VLQ_decode(chars)
        End Function

        Public Iterator Function getIntegers([in] As String) As IEnumerable(Of Integer)
            Dim chars As CharEnumerator = [in].GetEnumerator

            Do While chars.MoveNext()
                Yield base64VLQ_decode(chars)
            Loop
        End Function

        ''' <summary>
        ''' Decodes the next VLQValue from the provided chars.
        ''' </summary>
        ''' <returns></returns>
        Public Function base64VLQ_decode(chars As CharEnumerator) As Integer
            Dim result As Integer = 0
            Dim shift As Integer = 0
            Dim continuation As Boolean
            Dim c As Char
            Dim digit As Integer

            Do
                c = chars.Current
                digit = fromBase64(c)
                continuation = (digit And VLQ_CONTINUATION_BIT) <> 0
                digit = digit And VLQ_BASE_MASK
                result = result + (digit << shift)
                shift = shift + VLQ_BASE_SHIFT

                If Not continuation Then
                    Exit Do
                Else
                    chars.MoveNext()
                End If
            Loop

            Return fromVLQSigned(result)
        End Function
    End Module
End Namespace