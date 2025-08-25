Namespace Xpt

    Public Class PrimitiveUtils

        ''' <summary>
        ''' Byte swap a single short value.
        ''' </summary>
        ''' <param name="value"> Value to byte swap. </param>
        ''' <returns> Byte swapped representation. </returns>
        Public Shared Function swap(value As Short) As Short
            Dim b1 = value And &HFF
            Dim b2 = value >> 8 And &HFF

            Return b1 << 8 Or b2 << 0
        End Function

        ''' <summary>
        ''' Byte swap a single int value.
        ''' </summary>
        ''' <param name="value"> Value to byte swap. </param>
        ''' <returns> Byte swapped representation. </returns>
        Public Shared Function swap(value As Integer) As Integer
            Dim b1 = value >> 0 And &HFF
            Dim b2 = value >> 8 And &HFF
            Dim b3 = value >> 16 And &HFF
            Dim b4 = value >> 24 And &HFF

            Return b1 << 24 Or b2 << 16 Or b3 << 8 Or b4 << 0
        End Function

        ''' <summary>
        ''' Byte swap a single long value.
        ''' </summary>
        ''' <param name="value"> Value to byte swap. </param>
        ''' <returns> Byte swapped representation. </returns>
        Public Shared Function swap(value As Long) As Long
            Dim b1 = value >> 0 And &HFF
            Dim b2 = value >> 8 And &HFF
            Dim b3 = value >> 16 And &HFF
            Dim b4 = value >> 24 And &HFF
            Dim b5 = value >> 32 And &HFF
            Dim b6 = value >> 40 And &HFF
            Dim b7 = value >> 48 And &HFF
            Dim b8 = value >> 56 And &HFF

            Return b1 << 56 Or b2 << 48 Or b3 << 40 Or b4 << 32 Or b5 << 24 Or b6 << 16 Or b7 << 8 Or b8 << 0
        End Function

        ''' <summary>
        ''' Byte swap a single float value.
        ''' </summary>
        ''' <param name="value"> Value to byte swap. </param>
        ''' <returns> Byte swapped representation. </returns>
        Public Shared Function swap(value As Single) As Single
            Dim intValue = BitConverter.SingleToInt32Bits(value)
            intValue = swap(intValue)
            Return BitConverter.Int32BitsToSingle(intValue)
        End Function

        ''' <summary>
        ''' Byte swap a single double value.
        ''' </summary>
        ''' <param name="value"> Value to byte swap. </param>
        ''' <returns> Byte swapped representation. </returns>
        Public Shared Function swap(value As Double) As Double
            Dim longValue = BitConverter.DoubleToInt64Bits(value)
            longValue = swap(longValue)
            Return BitConverter.Int64BitsToDouble(longValue)
        End Function

        ''' <summary>
        ''' Byte swap an array of shorts. The result of the swapping is put back into the
        ''' specified array.
        ''' </summary>
        ''' <param name="array"> Array of values to swap </param>
        Public Shared Sub swap(array As Short())
            For i = 0 To array.Length - 1
                array(i) = swap(array(i))
            Next
        End Sub

        ''' <summary>
        ''' Byte swap an array of ints. The result of the swapping is put back into the
        ''' specified array.
        ''' </summary>
        ''' <param name="array"> Array of values to swap </param>
        Public Shared Sub swap(array As Integer())
            For i = 0 To array.Length - 1
                array(i) = swap(array(i))
            Next
        End Sub

        ''' <summary>
        ''' Byte swap an array of longs. The result of the swapping is put back into the
        ''' specified array.
        ''' </summary>
        ''' <param name="array"> Array of values to swap </param>
        Public Shared Sub swap(array As Long())
            For i = 0 To array.Length - 1
                array(i) = swap(array(i))
            Next
        End Sub

        ''' <summary>
        ''' Byte swap an array of floats. The result of the swapping is put back into the
        ''' specified array.
        ''' </summary>
        ''' <param name="array"> Array of values to swap </param>
        Public Shared Sub swap(array As Single())
            For i = 0 To array.Length - 1
                array(i) = swap(array(i))
            Next
        End Sub

        ''' <summary>
        ''' Byte swap an array of doubles. The result of the swapping is put back into
        ''' the specified array.
        ''' </summary>
        ''' <param name="array"> Array of values to swap </param>
        Public Shared Sub swap(array As Double())
            For i = 0 To array.Length - 1
                array(i) = swap(array(i))
            Next
        End Sub

        Public Shared Function intToBytes(num As Integer) As Byte()
            Return BitConverter.GetBytes(num)
        End Function

        Public Shared Function longToBytes(num As Long) As Byte()
            Return BitConverter.GetBytes(num)
        End Function

        Public Shared Function toDouble(bytes As Byte()) As Double
            Return ByteBuffer.wrap(bytes).order(ByteOrder.BigEndian).double()
        End Function

        Public Shared Function toLong(bytes As Byte()) As Long
            Return ByteBuffer.wrap(bytes).order(ByteOrder.BigEndian).long()
        End Function

        Public Shared Function toLongLittle(bytes As Byte()) As Long
            Return ByteBuffer.wrap(bytes).long()
        End Function

        Public Shared Sub memset(buffer As Byte(), val As Byte, len As Integer)
            For i = 0 To len - 1
                buffer(i) = val
            Next
        End Sub

        Public Shared Function memcmp(tgt As Byte(), tgt_off As Integer, src As Byte(), src_off As Integer, len As Integer) As Boolean
            For i = 0 To len - 1
                If tgt(i + tgt_off) <> src(i + src_off) Then
                    Return False
                End If
            Next
            Return True
        End Function

        Public Shared Sub memcpy(tgt As Byte(), tgt_off As Integer, src As Byte(), src_off As Integer, len As Integer)
            For i = 0 To len - 1
                tgt(i + tgt_off) = src(i + src_off)
            Next
        End Sub

        Public Shared Sub memreverse(intp As Byte(), len As Integer)
            Dim i, j As Integer
            Dim save As Byte
            j = len / 2
            For i = 0 To j - 1
                save = intp(i)
                intp(i) = intp(len - i - 1)
                intp(len - i - 1) = save
            Next
        End Sub

        Public Shared Function xpt2ieeeSimple(xport As Byte()) As Double
            Dim ibm = toLong(xport)

            Dim sign = ibm And &H8000000000000000L
            Dim exponent = (ibm And &H7F00000000000000L) >> 56
            Dim mantissa = ibm And &HFFFFFFFFFFFFFFL

            If mantissa = 0 Then
                If xport(0) = &H0 Then
                    Return 0.0R
                Else
                    Return Double.NaN
                End If
            End If
            Dim shift = 3
            If (ibm And &H80000000000000L) <> 0 Then
                shift = 3
            ElseIf (ibm And &H40000000000000L) <> 0 Then
                shift = 2
            ElseIf (ibm And &H20000000000000L) <> 0 Then
                shift = 1
            Else
                shift = 0
            End If
            mantissa >>= shift
            mantissa = mantissa And &HFFEFFFFFFFFFFFFFL

            exponent -= 65
            exponent <<= 2
            exponent += shift + 1023

            Dim ieee = sign Or exponent << 52 Or mantissa
            Dim out = longToBytes(ieee)

            Return toDouble(out)
        End Function

    End Class

End Namespace
