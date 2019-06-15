#Region "Microsoft.VisualBasic::794fcb7210443c636a6139e0b3d779df, Data_science\Mathematica\SignalProcessing\SignalProcessing\WaveletTransform\Transform.vb"

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

    '     Module Transform
    ' 
    '         Function: (+2 Overloads) GetAllDetail, (+2 Overloads) GetDetailOfLevel, GetScaling, StepForward, StepInverse
    ' 
    '         Sub: FastForward1d, FastForward2d, FastInverse1d, FastInverse2d, FastStepForward
    '              FastStepInverse, Forward1D, Inverse1D, SetAllDetail, SetDetailofLevel
    '              SetDetailOfLevel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Marshal

Namespace WaveletTransform

    ''' <summary>
    ''' ###### WaveletTransform
    ''' 
    ''' > https://github.com/green-rail/WaveletTransform
    ''' </summary>
    Public Module Transform

        Public Sub FastForward1d(input As Double(), ByRef output As Double(), wavelet As Wavelet, levels As Integer)
            Dim Len As Integer = wavelet.DecompositionLow.Length
            Dim CircleInd As Integer
            output = New Double(input.Length - 1) {}
            Dim Buff As Double() = New Double(input.Length - 1) {}
            Buffer.BlockCopy(input, 0, output, 0, input.Length * 8)
            Dim BufScal As Double = 0
            Dim BufDet As Double = 0
            Dim DecLow As Pointer(Of Double) = Pointer(Of Double).stackalloc(Len)
            Dim DecHigh As Pointer(Of Double) = Pointer(Of Double).stackalloc(Len)
            For i As Integer = 0 To Len - 1
                DecLow(i) = wavelet.DecompositionLow(i)
                DecHigh(i) = wavelet.DecompositionHigh(i)
            Next

            Dim pout As New Pointer(Of Double)(output)
            Dim pbuf As New Pointer(Of Double)(Buff)

            For level As Integer = 0 To levels - 1
                Dim Bound As Integer = input.Length >> level
                Dim StartIndex As Integer = -((Len >> 1) - 1)
                Buffer.BlockCopy(output, 0, Buff, 0, Bound * 8)

                For i As Integer = 0 To (Bound >> 1) - 1
                    Dim j As Integer = StartIndex, k As Integer = 0
                    While k < Len
                        If (StartIndex < 0) OrElse j >= Bound Then
                            CircleInd = ((j Mod Bound) + Bound) Mod Bound
                        Else
                            CircleInd = j
                        End If
                        BufScal += DecLow(k) * pout(CircleInd)
                        BufDet += DecHigh(k) * pout(CircleInd)
                        j += 1
                        k += 1
                    End While
                    StartIndex += 2
                    pbuf(i) = BufScal
                    pbuf(i + (Bound >> 1)) = BufDet
                    BufScal = 0
                    BufDet = 0
                Next
                Buffer.BlockCopy(Buff, 0, output, 0, Bound * 8)
            Next
        End Sub

        Public Sub FastInverse1d(input As Double(), ByRef output As Double(), wavelet As Wavelet, startLevel As Integer)
            Dim Len As Integer = wavelet.ReconstructionLow.Length
            Dim CircleInd As Integer
            output = New Double(input.Length - 1) {}
            Dim Buff As Double() = New Double(input.Length - 1) {}
            Dim BufferLow As Double() = New Double(input.Length - 1) {}
            Dim BufferHigh As Double() = New Double(input.Length - 1) {}
            Buffer.BlockCopy(input, 0, output, 0, input.Length * 8)
            Dim Buf As Double = 0
            Dim RecLow As Pointer(Of Double) = Pointer(Of Double).stackalloc(Len)
            Dim RecHigh As Pointer(Of Double) = Pointer(Of Double).stackalloc(Len)
            For i As Integer = 0 To Len - 1
                RecLow(i) = wavelet.ReconstructionLow(i)
                RecHigh(i) = wavelet.ReconstructionHigh(i)
            Next

            Dim pbuf As New Pointer(Of Double)(Buff)
            Dim pLow As New Pointer(Of Double)(BufferLow)
            Dim pHigh As New Pointer(Of Double)(BufferHigh)

            For level As Integer = startLevel To 1 Step -1
                Dim Bound As Integer = input.Length >> level
                Dim StartIndex As Integer = -((Len >> 1) - 1)
                Dim i As Integer = 0, j As Integer = 0
                Dim k As Integer

                While i < Bound << 1
                    pLow(i) = 0
                    pHigh(i) = 0
                    pLow(i + 1) = output(j)
                    pHigh(i + 1) = output(Bound + j)
                    i += 2
                    j += 1
                End While

                For i = 0 To (Bound << 1) - 1
                    j = StartIndex
                    k = 0
                    While k < Len
                        If (StartIndex < 0) OrElse j >= (Bound << 1) Then
                            CircleInd = (j Mod (Bound << 1) + (Bound << 1)) Mod (Bound << 1)
                        Else
                            CircleInd = j
                        End If
                        Buf += RecLow(k) * pLow(CircleInd) + RecHigh(k) * pHigh(CircleInd)
                        j += 1
                        k += 1
                    End While
                    StartIndex += 1
                    pbuf(i) = Buf
                    Buf = 0
                Next
                Buffer.BlockCopy(Buff, 0, output, 0, Bound * 16)
            Next
        End Sub

        Private Function StepForward(input As Double(), wavelet As Wavelet, CurrentLevel As Integer) As Double()
            Dim Bound As Integer = input.Length >> CurrentLevel
            Dim Len As Integer = wavelet.DecompositionLow.Length
            Dim StartIndex As Integer = -((Len >> 1) - 1)
            Dim Output As Double() = New Double(input.Length - 1) {}
            Array.Copy(input, Bound, Output, Bound, input.Length - Bound)
            Dim BufScal As Double = 0
            Dim BufDet As Double = 0
            Dim CircleInd As Integer
            Dim i As Integer = 0, r As Integer = 0
            While i < Bound
                Dim j As Integer = StartIndex, k As Integer = 0
                While k < Len
                    If (StartIndex < 0) OrElse j >= Bound Then
                        CircleInd = ((j Mod Bound) + Bound) Mod Bound
                    Else
                        CircleInd = j
                    End If
                    BufScal += wavelet.DecompositionLow(k) * input(CircleInd)
                    BufDet += wavelet.DecompositionHigh(k) * input(CircleInd)
                    j += 1
                    k += 1
                End While
                StartIndex += 2
                Output(r) = BufScal
                Output(r + (Bound >> 1)) = BufDet
                BufScal = 0
                BufDet = 0
                i += 2
                r += 1
            End While
            Return Output
        End Function

        Public Sub Forward1D(input As Double(), ByRef output As Double(), wavelet As Wavelet, Level As Integer)
            Dim f As Boolean = (input.Length And (input.Length - 1)) = 0
            If Not f OrElse input.Length = 0 Then
                Throw New Exception("input length must be a power of two")
            End If

            output = New Double(input.Length - 1) {}
            Array.Copy(input, output, input.Length)
            For i As Integer = 0 To Level - 1
                output = StepForward(output, wavelet, i)
            Next
        End Sub

        Private Function StepInverse(input As Double(), wavelet As Wavelet, CurrentLevel As Integer) As Double()
            Dim Bound As Integer = input.Length >> CurrentLevel
            Dim Len As Integer = wavelet.ReconstructionLow.Length
            Dim StartIndex As Integer = -((Len >> 1) - 1)
            Dim Output As Double() = New Double(input.Length - 1) {}
            Dim BuffLow As Double() = New Double((Bound << 1) - 1) {}
            Dim BuffHi As Double() = New Double((Bound << 1) - 1) {}
            Array.Copy(input, 0, Output, 0, input.Length)
            Dim BufScal As Double = 0
            Dim CircleInd As Integer
            Dim i As Integer = 0, j As Integer = 0
            Dim k As Integer

            While i < Bound << 1
                BuffLow(i) = 0
                BuffHi(i) = 0
                BuffLow(i + 1) = input(j)
                BuffHi(i + 1) = input(Bound + j)
                i += 2
                j += 1
            End While

            For i = 0 To (Bound << 1) - 1
                j = StartIndex
                k = 0

                While k < Len
                    If (StartIndex < 0) OrElse j >= (Bound << 1) Then
                        CircleInd = (j Mod (Bound << 1) + (Bound << 1)) Mod (Bound << 1)
                    Else
                        CircleInd = j
                    End If
                    BufScal += wavelet.ReconstructionLow(k) * BuffLow(CircleInd) + wavelet.ReconstructionHigh(k) * BuffHi(CircleInd)
                    j += 1
                    k += 1
                End While
                StartIndex += 1
                Output(i) = BufScal
                BufScal = 0
            Next
            Return Output
        End Function

        Public Sub Inverse1D(input As Double(), ByRef output As Double(), wavelet As Wavelet, StartLevel As Integer)
            Dim f As Boolean = (input.Length And (input.Length - 1)) = 0
            If Not f OrElse input.Length = 0 Then
                Throw New Exception("input length must be a power of two")
            End If

            output = New Double(input.Length - 1) {}
            Array.Copy(input, output, input.Length)
            For i As Integer = StartLevel To 1 Step -1
                output = StepInverse(output, wavelet, i)
            Next
        End Sub


        Private Sub FastStepForward(ByRef input As Double(), buff As Double(), len As Integer, CurrentLevel As Integer, DecLow As Pointer(Of Double), DecHigh As Pointer(Of Double))
            Dim CircleInd As Integer
            Dim BufScal As Double = 0
            Dim BufDet As Double = 0
            Dim Bound As Integer = input.Length >> CurrentLevel
            Dim StartIndex As Integer = -((len >> 1) - 1)
            Dim pinp As New Pointer(Of Double)(input)
            Dim pbuf As New Pointer(Of Double)(buff)

            For i As Integer = 0 To (Bound >> 1) - 1
                Dim j As Integer = StartIndex, k As Integer = 0
                While k < len
                    If (StartIndex < 0) OrElse j >= Bound Then
                        CircleInd = ((j Mod Bound) + Bound) Mod Bound
                    Else
                        CircleInd = j
                    End If
                    BufScal += DecLow(k) * pinp(CircleInd)
                    BufDet += DecHigh(k) * pinp(CircleInd)
                    j += 1
                    k += 1
                End While
                StartIndex += 2
                pbuf(i) = BufScal
                pbuf(i + (Bound >> 1)) = BufDet
                BufScal = 0
                BufDet = 0
            Next
            Buffer.BlockCopy(buff, 0, input, 0, Bound * 8)

        End Sub

        Private Sub FastStepInverse(ByRef input As Double(), buffLow As Double(), buffHigh As Double(), len As Integer, CurrentLevel As Integer, RecLow As Pointer(Of Double),
        RecHigh As Pointer(Of Double))
            Dim Bound As Integer = input.Length >> CurrentLevel
            Dim StartIndex As Integer = -((len >> 1) - 1)
            Dim Buf As Double = 0
            Dim CircleInd As Integer
            Dim i As Integer = 0, j As Integer = 0
            Dim pinp As New Pointer(Of Double)(input)
            Dim pbufLow As New Pointer(Of Double)(buffLow)
            Dim pbufHi As New Pointer(Of Double)(buffHigh)
            Dim k As Integer

            While i < Bound << 1
                pbufLow(i) = 0
                pbufHi(i) = 0
                pbufLow(i + 1) = pinp(j)
                pbufHi(i + 1) = pinp(Bound + j)
                i += 2
                j += 1
            End While
            For i = 0 To (Bound << 1) - 1
                j = StartIndex
                k = 0
                While k < len
                    If (StartIndex < 0) OrElse j >= (Bound << 1) Then
                        CircleInd = (j Mod (Bound << 1) + (Bound << 1)) Mod (Bound << 1)
                    Else
                        CircleInd = j
                    End If
                    Buf += RecLow(k) * pbufLow(CircleInd) + RecHigh(k) * pbufHi(CircleInd)
                    j += 1
                    k += 1
                End While
                StartIndex += 1
                pinp(i) = Buf
                Buf = 0
            Next

        End Sub

        Public Sub FastForward2d(input As Double(,), ByRef output As Double(,), wavelet As Wavelet, Level As Integer)
            Dim DataLen As Integer = input.GetLength(0)
            Dim Len As Integer = wavelet.DecompositionHigh.Length
            Dim Bound As Integer
            output = New Double(DataLen - 1, DataLen - 1) {}
            Dim buff As Double() = New Double(DataLen - 1) {}
            Dim buffData As Double() = New Double(DataLen - 1) {}

            Dim DecLow As Pointer(Of Double) = Pointer(Of Double).stackalloc(Len)
            Dim DecHigh As Pointer(Of Double) = Pointer(Of Double).stackalloc(Len)
            For i As Integer = 0 To Len - 1
                DecLow(i) = wavelet.DecompositionLow(i)
                DecHigh(i) = wavelet.DecompositionHigh(i)
            Next

            For i As Integer = 0 To DataLen - 1
                For j As Integer = 0 To DataLen - 1
                    output(i, j) = input(i, j)
                Next
            Next

            For lev As Integer = 0 To Level - 1
                Bound = DataLen >> lev
                For i As Integer = 0 To Bound - 1
                    For j As Integer = 0 To Bound - 1
                        buffData(j) = output(i, j)
                    Next
                    FastStepForward(buffData, buff, Len, lev, DecLow, DecHigh)
                    For j As Integer = 0 To Bound - 1
                        output(i, j) = buffData(j)
                    Next
                Next
                For j As Integer = 0 To Bound - 1
                    For i As Integer = 0 To Bound - 1
                        buffData(i) = output(i, j)
                    Next
                    FastStepForward(buffData, buff, Len, lev, DecLow, DecHigh)
                    For i As Integer = 0 To Bound - 1
                        output(i, j) = buffData(i)
                    Next
                Next
            Next
        End Sub

        Public Sub FastInverse2d(input As Double(,), ByRef output As Double(,), wavelet As Wavelet, Level As Integer)
            Dim DataLen As Integer = input.GetLength(0)
            Dim Len As Integer = wavelet.DecompositionHigh.Length
            Dim Bound As Integer
            output = New Double(DataLen - 1, DataLen - 1) {}
            Dim buffData As Double() = New Double(DataLen - 1) {}
            Dim buffLow As Double() = New Double(DataLen - 1) {}
            Dim buffHigh As Double() = New Double(DataLen - 1) {}

            Dim RecLow As Pointer(Of Double) = Pointer(Of Double).stackalloc(Len)
            Dim RecHigh As Pointer(Of Double) = Pointer(Of Double).stackalloc(Len)
            For i As Integer = 0 To Len - 1
                RecLow(i) = wavelet.ReconstructionLow(i)
                RecHigh(i) = wavelet.ReconstructionHigh(i)
            Next

            For i As Integer = 0 To DataLen - 1
                For j As Integer = 0 To DataLen - 1
                    output(i, j) = input(i, j)
                Next
            Next

            For lev As Integer = Level To 1 Step -1
                Bound = DataLen >> lev
                For j As Integer = 0 To (Bound << 1) - 1
                    For i As Integer = 0 To (Bound << 1) - 1
                        buffData(i) = output(i, j)
                    Next
                    FastStepInverse(buffData, buffLow, buffHigh, Len, lev, RecLow,
                    RecHigh)
                    For i As Integer = 0 To (Bound << 1) - 1
                        output(i, j) = buffData(i)
                    Next
                Next
                For i As Integer = 0 To (Bound << 1) - 1
                    For j As Integer = 0 To (Bound << 1) - 1
                        buffData(j) = output(i, j)
                    Next
                    FastStepInverse(buffData, buffLow, buffHigh, Len, lev, RecLow,
                    RecHigh)
                    For j As Integer = 0 To (Bound << 1) - 1
                        output(i, j) = buffData(j)
                    Next
                Next
            Next
        End Sub

        Public Function GetAllDetail(input As Double(), currentLevel As Integer) As Double()
            Dim Shift As Integer = input.Length >> currentLevel
            Dim output As Double() = New Double(input.Length - Shift - 1) {}
            Array.Copy(input, Shift, output, 0, input.Length - Shift)
            Return output
        End Function

        Public Function GetAllDetail(input As Double(,), currentLevel As Integer) As Double()
            Dim Len As Integer = input.GetLength(0) >> currentLevel
            Dim output As Double() = New Double(input.GetLength(0) * input.GetLength(0) - (Len * Len) - 1) {}
            Dim OutIndex As Integer = 0

            For lev As Integer = currentLevel To 1 Step -1
                Dim Bound As Integer = input.GetLength(0) >> lev - 1
                Len = input.GetLength(0) >> lev
                For i As Integer = 0 To Len - 1
                    For j As Integer = Len To Bound - 1
                        output(OutIndex) = input(i, j)
                        OutIndex += 1
                    Next
                Next
                For i As Integer = Len To Bound - 1
                    For j As Integer = 0 To Len - 1
                        output(OutIndex) = input(i, j)
                        OutIndex += 1
                    Next
                Next
                For i As Integer = Len To Bound - 1
                    For j As Integer = Len To Bound - 1
                        output(OutIndex) = input(i, j)
                        OutIndex += 1
                    Next
                Next
            Next
            Return output
        End Function

        Public Function GetDetailOfLevel(input As Double(,), level As Integer) As Double()
            Dim Len As Integer = input.GetLength(0) >> level
            Dim lev As Integer = level
            Dim Bound As Integer = input.GetLength(0) >> lev - 1
            Dim output As Double() = New Double(Bound * Bound - (Len * Len) - 1) {}
            Dim OutIndex As Integer = 0

            For i As Integer = 0 To Len - 1
                For j As Integer = Len To Bound - 1
                    output(OutIndex) = input(i, j)
                    OutIndex += 1
                Next
            Next
            For i As Integer = Len To Bound - 1
                For j As Integer = 0 To Len - 1
                    output(OutIndex) = input(i, j)
                    OutIndex += 1
                Next
            Next
            For i As Integer = Len To Bound - 1
                For j As Integer = Len To Bound - 1
                    output(OutIndex) = input(i, j)
                    OutIndex += 1
                Next
            Next
            Return output
        End Function

        Public Sub SetAllDetail(input As Double(), coefs As Double(,), currentLevel As Integer)
            Dim Shift As Integer = coefs.Length >> currentLevel
            Dim Len As Integer = coefs.GetLength(0) >> currentLevel
            Dim InpIndex As Integer = 0

            For lev As Integer = currentLevel To 1 Step -1
                Dim Bound As Integer = coefs.GetLength(0) >> lev - 1
                Len = coefs.GetLength(0) >> lev
                For i As Integer = 0 To Len - 1
                    For j As Integer = Len To Bound - 1
                        coefs(i, j) = input(InpIndex)
                        InpIndex += 1
                    Next
                Next
                For i As Integer = Len To Bound - 1
                    For j As Integer = 0 To Len - 1
                        coefs(i, j) = input(InpIndex)
                        InpIndex += 1
                    Next
                Next
                For i As Integer = Len To Bound - 1
                    For j As Integer = Len To Bound - 1
                        coefs(i, j) = input(InpIndex)
                        InpIndex += 1
                    Next
                Next
            Next
        End Sub

        Public Sub SetDetailofLevel(input As Double(), coefs As Double(,), level As Integer)
            Dim Len As Integer = coefs.GetLength(0) >> level
            Dim InpIndex As Integer = 0

            Dim lev As Integer = level
            Dim Bound As Integer = coefs.GetLength(0) >> lev - 1
            Len = coefs.GetLength(0) >> lev
            For i As Integer = 0 To Len - 1
                For j As Integer = Len To Bound - 1
                    coefs(i, j) = input(InpIndex)
                    InpIndex += 1
                Next
            Next
            For i As Integer = Len To Bound - 1
                For j As Integer = 0 To Len - 1
                    coefs(i, j) = input(InpIndex)
                    InpIndex += 1
                Next
            Next
            For i As Integer = Len To Bound - 1
                For j As Integer = Len To Bound - 1
                    coefs(i, j) = input(InpIndex)
                    InpIndex += 1
                Next
            Next
        End Sub

        Public Function GetDetailOfLevel(input As Double(), currentLevel As Integer, level As Integer) As Double()
            Dim Len As Integer = input.Length >> level
            Dim output As Double() = New Double(Len - 1) {}
            Array.Copy(input, Len, output, 0, Len)
            Return output
        End Function

        Public Function GetScaling(input As Double(), currentLevel As Integer) As Double()
            Dim Len As Integer = input.Length >> currentLevel
            Dim output As Double() = New Double(Len - 1) {}
            Array.Copy(input, output, Len)
            Return output
        End Function

        Public Sub SetDetailOfLevel(input As Double(), inpDetails As Double())
            Array.Copy(inpDetails, 0, input, inpDetails.Length, inpDetails.Length)
        End Sub
    End Module
End Namespace
