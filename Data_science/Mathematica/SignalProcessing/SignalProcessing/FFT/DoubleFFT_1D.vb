#Region "Microsoft.VisualBasic::bf3100816f57b3660cc49a95d6d3a658, Data_science\Mathematica\SignalProcessing\SignalProcessing\FFT\DoubleFFT_1D.vb"

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

    '   Total Lines: 6632
    '    Code Lines: 5891
    ' Comment Lines: 370
    '   Blank Lines: 371
    '     File Size: 273.47 KB


    '     Class DoubleFFT_1D
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: cfttree, GetReminder
    ' 
    '         Sub: bitrv2, bitrv208, bitrv208neg, bitrv216, bitrv216neg
    '              bitrv2conj, bluestein_complex, bluestein_real_forward, bluestein_real_full, bluestein_real_inverse
    '              bluestein_real_inverse2, bluesteini, cfftf, (+2 Overloads) cffti, cftb040
    '              cftb1st, cftbsub, cftf040, cftf081, cftf082
    '              cftf161, cftf162, cftf1st, cftfsub, cftfx41
    '              cftleaf, cftmdl1, cftmdl2, cftrec4, cftrec4_th
    '              cftx020, cftxb020, cftxc020, (+2 Overloads) ComplexForward, (+2 Overloads) ComplexInverse
    '              makect, makeipt, makewt, passf2, passf3
    '              passf4, passf5, passfg, radb2, radb3
    '              radb4, radb5, radbg, radf2, radf3
    '              radf4, radf5, radfg, (+2 Overloads) RealForward, (+2 Overloads) RealForwardFull
    '              (+2 Overloads) RealInverse, RealInverse2, (+2 Overloads) RealInverseFull, rfftb, rfftf
    '              rffti, rftbsub, rftfsub, scale
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2017 - presented by Kei Nakai
'
' Original project is developed and published by OpenGamma Inc.
'
' Copyright (C) 2012 - present by OpenGamma Incd and the OpenGamma group of companies
'
' Please see distribution for license.
'
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'     http://www.apache.org/licenses/LICENSE-2.0
'     
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.

' **** BEGIN LICENSE BLOCK ****
' JTransforms
' Copyright (c) 2007 onward, Piotr Wendykier
' All rights reserved.
' 
' Redistribution and use in source and binary forms, with or without
' modification, are permitted provided that the following conditions are met:
' 
' 1d Redistributions of source code must retain the above copyright notice, this
'    list of conditions and the following disclaimerd 
' 2d Redistributions in binary form must reproduce the above copyright notice,
'    this list of conditions and the following disclaimer in the documentation
'    and/or other materials provided with the distribution.
'
' THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
' ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
' WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
' DISCLAIMEDd IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
' ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
' (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
' LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
' ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
' (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
' SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
'
' **** END LICENSE BLOCK ****////

Namespace FFT

    ''' <summary>
    ''' https://github.com/cobaltblueocean/Mercury.Language.Extensions
    ''' </summary>
    Public NotInheritable Class DoubleFFT_1D
#Region "Local Variables"

        Private n As Integer

        'private long nl;

        Private nBluestein As Integer

        'private long nBluesteinl;

        Private ip As Integer()

        'private long[] ipl;

        Private w As Double()

        'private double[] wl;

        Private nw As Integer

        'private long nwl;

        Private nc As Integer

        'private long ncl;

        Private wtable As Double()

        'private double[] wtablel;

        Private wtable_r As Double()

        'private double[] wtable_rl;

        Private bk1 As Double()

        'private double[] bk1l;

        Private bk2 As Double()

        'private double[] bk2l;

        Private plan As Plans

        'private Boolean useLargeArrays;

        Private Shared factors As Integer() = {4, 2, 3, 5}

        Private Shared PI As Double = 3.1415926535897931

        Private Shared TWO_PI As Double = 6.2831853071795862

#End Region

        ''' <summary>
        ''' Creates new instance of DoubleFFT_1D.
        ''' 
        ''' </summary>
        ''' <param name="n"></param>
        '''            size of data
        Public Sub New(n As Integer)
            If n < 1 Then
                Throw New ArgumentException("n must be greater than 0")
            End If
            Me.n = n

            If Not n.IsPowerOf2() Then
                If GetReminder(n, factors) >= 211 Then
                    plan = Plans.BLUESTEIN
                    nBluestein = (n * 2 - 1).NextPowerOf2()
                    bk1 = New Double(2 * nBluestein - 1) {}
                    bk2 = New Double(2 * nBluestein - 1) {}
                    ip = New Integer(2 + CInt(System.Math.Ceiling(2 + CDbl(1 << CInt(System.Math.Log(nBluestein + 0.5) / System.Math.Log(2)) / 2))) - 1) {}
                    w = New Double(nBluestein - 1) {}
                    Dim twon = 2 * nBluestein
                    nw = ip(0)
                    If twon > nw << 2 Then
                        nw = twon >> 2
                        makewt(nw)
                    End If
                    nc = ip(1)
                    If nBluestein > nc << 2 Then
                        nc = nBluestein >> 2
                        makect(nc, w, nw)
                    End If
                    bluesteini()
                Else
                    plan = Plans.MIXED_RADIX
                    wtable = New Double(4 * n + 15 - 1) {}
                    wtable_r = New Double(2 * n + 15 - 1) {}
                    cffti()
                    rffti()
                End If
            Else
                plan = Plans.SPLIT_RADIX
                ip = New Integer(2 + CInt(System.Math.Ceiling(2 + CDbl(1 << CInt(System.Math.Log(n + 0.5) / System.Math.Log(2)) / 2))) - 1) {}
                w = New Double(n - 1) {}
                Dim twon = 2 * n
                nw = ip(0)
                If twon > nw << 2 Then
                    nw = twon >> 2
                    makewt(nw)
                End If
                nc = ip(1)
                If n > nc << 2 Then
                    nc = n >> 2
                    makect(nc, w, nw)
                End If
            End If
        End Sub

#Region "ComplexForward"
        ''' <summary>
        ''' Computes 1D forward DFT of complex data leaving the result in
        ''' <code>a</code>d Complex number is stored as two double values in
        ''' sequence: the real and imaginary part, i.ed the size of the input array
        ''' must be greater or equal 2*nd The physical layout of the input data has
        ''' to be as follows:&lt;br>
        ''' 
        ''' <pre>
        ''' a[2*k] = Re[k],
        ''' a[2*k+1] = Im[k], 0&lt;=k&lt;n
        ''' </pre>
        ''' 
        ''' </summary>
        ''' <param name="a"> data to transform</param>
        '''            
        Public Sub ComplexForward(a As Double())
            ComplexForward(a, 0)
        End Sub

        ''' <summary>
        ''' Computes 1D forward DFT of complex data leaving the result in
        ''' <code>a</code>d Complex number is stored as two double values in
        ''' sequence: the real and imaginary part, i.ed the size of the input array
        ''' must be greater or equal 2*nd The physical layout of the input data has
        ''' to be as follows:&lt;br>
        ''' 
        ''' <pre>
        ''' a[offa+2*k] = Re[k],
        ''' a[offa+2*k+1] = Im[k], 0&lt;=k&lt;n
        ''' </pre>
        ''' 
        ''' </summary>
        ''' <param name="a"></param>
        '''            data to transform
        ''' <param name="offa"></param>
        '''            index of the first element in array <code>a</code>  
        Public Sub ComplexForward(a As Double(), offa As Integer)
            If n = 1 Then Return
            Select Case plan
                Case Plans.SPLIT_RADIX
                    cftbsub(2 * n, a, offa, ip, nw, w)
                Case Plans.MIXED_RADIX
                    cfftf(a, offa, -1)
                Case Plans.BLUESTEIN
                    bluestein_complex(a, offa, -1)
            End Select
        End Sub
#End Region

#Region "Complex Inverse"
        ''' <summary>
        ''' Computes 1D inverse DFT of complex data leaving the result in
        ''' <code>a</code>d Complex number is stored as two double values in
        ''' sequence: the real and imaginary part, i.ed the size of the input array
        ''' must be greater or equal 2*nd The physical layout of the input data has
        ''' to be as follows:&lt;br>
        ''' 
        ''' <pre>
        ''' a[2*k] = Re[k],
        ''' a[2*k+1] = Im[k], 0&lt;=k&lt;n
        ''' </pre>
        ''' 
        ''' </summary>
        ''' <param name="a">
        '''            data to transform</param>
        ''' <param name="scale">
        '''            if true then scaling is performed</param> 
        Public Sub ComplexInverse(a As Double(), scale As Boolean)
            ComplexInverse(a, 0, scale)
        End Sub

        ''' <summary>
        ''' Computes 1D inverse DFT of complex data leaving the result in
        ''' <code>a</code>d Complex number is stored as two double values in
        ''' sequence: the real and imaginary part, i.ed the size of the input array
        ''' must be greater or equal 2*nd The physical layout of the input data has
        ''' to be as follows:&lt;br>
        ''' 
        ''' <pre>
        ''' a[offa+2*k] = Re[k],
        ''' a[offa+2*k+1] = Im[k], 0&lt;=k&lt;n
        ''' </pre>
        ''' 
        ''' </summary>
        ''' <param name="a">
        '''            data to transform</param>
        ''' <param name="offa">
        '''            index of the first element in array <code>a</code></param>
        ''' <param name="isScale">
        '''            if true then scaling is performed    </param>
        Public Sub ComplexInverse(a As Double(), offa As Integer, isScale As Boolean)
            If n = 1 Then Return
            Select Case plan
                Case Plans.SPLIT_RADIX
                    cftfsub(2 * n, a, offa, ip, nw, w)
                Case Plans.MIXED_RADIX
                    cfftf(a, offa, +1)
                Case Plans.BLUESTEIN
                    bluestein_complex(a, offa, 1)
            End Select
            If isScale Then
                scale(n, a, offa, True)
            End If
        End Sub
#End Region

#Region "Real Forward"
        ''' <summary>
        ''' Computes 1D forward DFT of real data leaving the result in <code>a</code>
        ''' d The physical layout of the output data is as follows:&lt;br>
        ''' 
        ''' if n is even then
        ''' 
        ''' <pre>
        ''' a[2*k] = Re[k], 0&lt;=k&lt;n/2
        ''' a[2*k+1] = Im[k], 0&lt;k&lt;n/2
        ''' a[1] = Re[n/2]
        ''' </pre>
        ''' 
        ''' if n is odd then
        ''' 
        ''' <pre>
        ''' a[2*k] = Re[k], 0&lt;=k&lt;(n+1)/2
        ''' a[2*k+1] = Im[k], 0&lt;k&lt;(n-1)/2
        ''' a[1] = Im[(n-1)/2]
        ''' </pre>
        ''' 
        ''' This method computes only half of the elements of the real transformd The
        ''' other half satisfies the symmetry conditiond If you want the full real
        ''' forward transform, use <code>realForwardFull</code>d To get back the
        ''' original data, use <code>realInverse</code> on the output of this method.
        ''' 
        ''' </summary>
        ''' <param name="a">
        '''            data to transform   </param>
        Public Sub RealForward(a As Double())
            RealForward(a, 0)
        End Sub

        ''' <summary>
        ''' Computes 1D forward DFT of real data leaving the result in <code>a</code>
        ''' d The physical layout of the output data is as follows:&lt;br>
        ''' 
        ''' if n is even then
        ''' 
        ''' <pre>
        ''' a[offa+2*k] = Re[k], 0&lt;=k&lt;n/2
        ''' a[offa+2*k+1] = Im[k], 0&lt;k&lt;n/2
        ''' a[offa+1] = Re[n/2]
        ''' </pre>
        ''' 
        ''' if n is odd then
        ''' 
        ''' <pre>
        ''' a[offa+2*k] = Re[k], 0&lt;=k&lt;(n+1)/2
        ''' a[offa+2*k+1] = Im[k], 0&lt;k&lt;(n-1)/2
        ''' a[offa+1] = Im[(n-1)/2]
        ''' </pre>
        ''' 
        ''' This method computes only half of the elements of the real transformd The
        ''' other half satisfies the symmetry conditiond If you want the full real
        ''' forward transform, use <code>realForwardFull</code>d To get back the
        ''' original data, use <code>realInverse</code> on the output of this method.
        ''' 
        ''' </summary>
        ''' <param name="a">
        '''            data to transform</param>
        ''' <param name="offa">
        '''            index of the first element in array <code>a</code> </param>
        Public Sub RealForward(a As Double(), offa As Integer)
            If n = 1 Then Return

            Select Case plan
                Case Plans.SPLIT_RADIX
                    Dim xi As Double

                    If n > 4 Then
                        cftfsub(n, a, offa, ip, nw, w)
                        rftfsub(n, a, offa, nc, w, nw)
                    ElseIf n = 4 Then
                        cftx020(a, offa)
                    End If
                    xi = a(offa) - a(offa + 1)
                    a(offa) += a(offa + 1)
                    a(offa + 1) = xi
                Case Plans.MIXED_RADIX
                    rfftf(a, offa)
                    For k = n - 1 To 2 Step -1
                        Dim idx = offa + k
                        Dim tmp = a(idx)
                        a(idx) = a(idx - 1)
                        a(idx - 1) = tmp
                    Next
                Case Plans.BLUESTEIN
                    bluestein_real_forward(a, offa)
            End Select
        End Sub

        ''' <summary>
        ''' Computes 1D forward DFT of real data leaving the result in <code>a</code>
        ''' d This method computes the full real forward transform, i.ed you will get
        ''' the same result as from <code>complexForward</code> called with all
        ''' imaginary parts equal 0d Because the result is stored in <code>a</code>,
        ''' the size of the input array must greater or equal 2*n, with only the
        ''' first n elements filled with real datad To get back the original data,
        ''' use <code>complexInverse</code> on the output of this method.
        ''' 
        ''' </summary>
        ''' <param name="a"></param>
        '''            data to transform
        Public Sub RealForwardFull(a As Double())
            RealForwardFull(a, 0)
        End Sub

        ''' <summary>
        ''' Computes 1D forward DFT of real data leaving the result in <code>a</code>
        ''' d This method computes the full real forward transform, i.ed you will get
        ''' the same result as from <code>complexForward</code> called with all
        ''' imaginary part equal 0d Because the result is stored in <code>a</code>,
        ''' the size of the input array must greater or equal 2*n, with only the
        ''' first n elements filled with real datad To get back the original data,
        ''' use <code>complexInverse</code> on the output of this method.
        ''' 
        ''' </summary>
        ''' <param name="a"></param>
        '''            data to transform
        ''' <param name="offa"></param>
        '''            index of the first element in array <code>a</code>
        Public Sub RealForwardFull(a As Double(), offa As Integer)

            Dim twon = 2 * n
            Select Case plan
                Case Plans.SPLIT_RADIX
                    RealForward(a, offa)
                    Dim nthreads As Integer = Process.GetCurrentProcess().Threads.Count
                    If nthreads > 1 AndAlso n / 2 > TransformCore.THREADS_BEGIN_N_1D_FFT_2THREADS Then
                        Dim taskArray = New Task(nthreads - 1) {}
                        Dim k As Integer = n / 2 / nthreads
                        For i = 0 To nthreads - 1
                            Dim firstIdx = i * k
                            Dim lastIdx As Integer = If(i = nthreads - 1, n / 2, firstIdx + k)
                            taskArray(i) = Task.Factory.StartNew(Sub()
                                                                     Dim idx1, idx2 As Integer
                                                                     For k = firstIdx To lastIdx - 1
                                                                         idx1 = 2 * k
                                                                         idx2 = offa + (twon - idx1) Mod twon
                                                                         a(idx2) = a(offa + idx1)
                                                                         a(idx2 + 1) = -a(offa + idx1 + 1)
                                                                     Next

                                                                 End Sub)
                        Next
                        Try
                            Task.WaitAll(taskArray)
                        Finally

                        End Try
                    Else
                        Dim idx1, idx2 As Integer
                        For k As Integer = 0 To n / 2 - 1
                            idx1 = 2 * k
                            idx2 = offa + (twon - idx1) Mod twon
                            a(idx2) = a(offa + idx1)
                            a(idx2 + 1) = -a(offa + idx1 + 1)
                        Next
                    End If
                    a(offa + n) = -a(offa + 1)
                    a(offa + 1) = 0
                Case Plans.MIXED_RADIX
                    rfftf(a, offa)
                    Dim m As Integer
                    If n Mod 2 = 0 Then
                        m = n / 2
                    Else
                        m = (n + 1) / 2
                    End If
                    For k = 1 To m - 1
                        Dim idx1 = offa + twon - 2 * k
                        Dim idx2 = offa + 2 * k
                        a(idx1 + 1) = -a(idx2)
                        a(idx1) = a(idx2 - 1)
                    Next
                    For k = 1 To n - 1
                        Dim idx = offa + n - k
                        Dim tmp = a(idx + 1)
                        a(idx + 1) = a(idx)
                        a(idx) = tmp
                    Next
                    a(offa + 1) = 0
                Case Plans.BLUESTEIN
                    bluestein_real_full(a, offa, -1)
            End Select
        End Sub
#End Region

#Region "Real Inverse"
        ''' <summary>
        ''' Computes 1D inverse DFT of real data leaving the result in <code>a</code>
        ''' d The physical layout of the input data has to be as follows:&lt;br>
        ''' 
        ''' if n is even then
        ''' 
        ''' <pre>
        ''' a[2*k] = Re[k], 0&lt;=k&lt;n/2
        ''' a[2*k+1] = Im[k], 0&lt;k&lt;n/2
        ''' a[1] = Re[n/2]
        ''' </pre>
        ''' 
        ''' if n is odd then
        ''' 
        ''' <pre>
        ''' a[2*k] = Re[k], 0&lt;=k&lt;(n+1)/2
        ''' a[2*k+1] = Im[k], 0&lt;k&lt;(n-1)/2
        ''' a[1] = Im[(n-1)/2]
        ''' </pre>
        ''' 
        ''' This method computes only half of the elements of the real transformd The
        ''' other half satisfies the symmetry conditiond If you want the full real
        ''' inverse transform, use <code>realInverseFull</code>.
        ''' 
        ''' </summary>
        ''' <param name="a">data to transform</param>
        '''            
        ''' 
        ''' <param name="scale">if true then scaling is performed</param>   
        Public Sub RealInverse(a As Double(), scale As Boolean)
            RealInverse(a, 0, scale)
        End Sub

        ''' <summary>
        ''' Computes 1D inverse DFT of real data leaving the result in <code>a</code>
        ''' d The physical layout of the input data has to be as follows:&lt;br>
        ''' 
        ''' if n is even then
        ''' 
        ''' <pre>
        ''' a[offa+2*k] = Re[k], 0&lt;=k&lt;n/2
        ''' a[offa+2*k+1] = Im[k], 0&lt;k&lt;n/2
        ''' a[offa+1] = Re[n/2]
        ''' </pre>
        ''' 
        ''' if n is odd then
        ''' 
        ''' <pre>
        ''' a[offa+2*k] = Re[k], 0&lt;=k&lt;(n+1)/2
        ''' a[offa+2*k+1] = Im[k], 0&lt;k&lt;(n-1)/2
        ''' a[offa+1] = Im[(n-1)/2]
        ''' </pre>
        ''' 
        ''' This method computes only half of the elements of the real transformd The
        ''' other half satisfies the symmetry conditiond If you want the full real
        ''' inverse transform, use <code>realInverseFull</code>.
        ''' 
        ''' </summary>
        ''' <param name="a">
        '''            data to transform</param>
        ''' <param name="offa">
        '''            index of the first element in array <code>a</code></param>
        ''' <param name="isScale">
        '''            if true then scaling is performed  </param>
        Public Sub RealInverse(a As Double(), offa As Integer, isScale As Boolean)
            If n = 1 Then Return
            Select Case plan
                Case Plans.SPLIT_RADIX
                    a(offa + 1) = 0.5 * (a(offa) - a(offa + 1))
                    a(offa) -= a(offa + 1)
                    If n > 4 Then
                        rftfsub(n, a, offa, nc, w, nw)
                        cftbsub(n, a, offa, ip, nw, w)
                    ElseIf n = 4 Then
                        cftxc020(a, offa)
                    End If
                    If isScale Then
                        scale(n / 2, a, offa, False)
                    End If
                Case Plans.MIXED_RADIX
                    For k = 2 To n - 1
                        Dim idx = offa + k
                        Dim tmp = a(idx - 1)
                        a(idx - 1) = a(idx)
                        a(idx) = tmp
                    Next
                    rfftb(a, offa)
                    If isScale Then
                        scale(n, a, offa, False)
                    End If
                Case Plans.BLUESTEIN
                    bluestein_real_inverse(a, offa)
                    If isScale Then
                        scale(n, a, offa, False)
                    End If
            End Select

        End Sub

        ''' <summary>
        ''' Computes 1D inverse DFT of real data leaving the result in <code>a</code>
        ''' d This method computes the full real inverse transform, i.ed you will get
        ''' the same result as from <code>complexInverse</code> called with all
        ''' imaginary part equal 0d Because the result is stored in <code>a</code>,
        ''' the size of the input array must greater or equal 2*n, with only the
        ''' first n elements filled with real data.
        ''' 
        ''' </summary>
        ''' <param name="a"></param>
        '''            data to transform
        ''' <param name="isScale">
        '''            if true then scaling is performed</param>
        Public Sub RealInverseFull(a As Double(), isScale As Boolean)
            RealInverseFull(a, 0, isScale)
        End Sub

        ''' <summary>
        ''' Computes 1D inverse DFT of real data leaving the result in <code>a</code>
        ''' d This method computes the full real inverse transform, i.ed you will get
        ''' the same result as from <code>complexInverse</code> called with all
        ''' imaginary part equal 0d Because the result is stored in <code>a</code>,
        ''' the size of the input array must greater or equal 2*n, with only the
        ''' first n elements filled with real data.
        ''' 
        ''' </summary>
        ''' <param name="a"> data to transform</param>
        '''           
        ''' <param name="offa">index of the first element in array <code>a</code></param>
        '''            
        ''' <param name="isScale">if true then scaling is performed</param>
        '''            
        Public Sub RealInverseFull(a As Double(), offa As Integer, isScale As Boolean)
            Dim twon = 2 * n
            Select Case plan
                Case Plans.SPLIT_RADIX
                    RealInverse2(a, offa, isScale)
                    Dim nthreads As Integer = Process.GetCurrentProcess().Threads.Count
                    If nthreads > 1 AndAlso n / 2 > TransformCore.THREADS_BEGIN_N_1D_FFT_2THREADS Then
                        Dim taskArray = New Task(nthreads - 1) {}
                        Dim k As Integer = n / 2 / nthreads
                        For i = 0 To nthreads - 1
                            Dim firstIdx = i * k
                            Dim lastIdx As Integer = If(i = nthreads - 1, n / 2, firstIdx + k)
                            taskArray(i) = Task.Factory.StartNew(Sub()
                                                                     Dim idx1, idx2 As Integer
                                                                     For k = firstIdx To lastIdx - 1
                                                                         idx1 = 2 * k
                                                                         idx2 = offa + (twon - idx1) Mod twon
                                                                         a(idx2) = a(offa + idx1)
                                                                         a(idx2 + 1) = -a(offa + idx1 + 1)
                                                                     Next

                                                                 End Sub)
                        Next
                        Try
                            Task.WaitAll(taskArray)
                        Finally
                        End Try
                    Else
                        Dim idx1, idx2 As Integer
                        For k As Integer = 0 To n / 2 - 1
                            idx1 = 2 * k
                            idx2 = offa + (twon - idx1) Mod twon
                            a(idx2) = a(offa + idx1)
                            a(idx2 + 1) = -a(offa + idx1 + 1)
                        Next
                    End If
                    a(offa + n) = -a(offa + 1)
                    a(offa + 1) = 0
                Case Plans.MIXED_RADIX
                    rfftf(a, offa)
                    If isScale Then
                        scale(n, a, offa, False)
                    End If
                    Dim m As Integer
                    If n Mod 2 = 0 Then
                        m = n / 2
                    Else
                        m = (n + 1) / 2
                    End If
                    For k = 1 To m - 1
                        Dim idx1 = offa + 2 * k
                        Dim idx2 = offa + twon - 2 * k
                        a(idx1) = -a(idx1)
                        a(idx2 + 1) = -a(idx1)
                        a(idx2) = a(idx1 - 1)
                    Next
                    For k = 1 To n - 1
                        Dim idx = offa + n - k
                        Dim tmp = a(idx + 1)
                        a(idx + 1) = a(idx)
                        a(idx) = tmp
                    Next
                    a(offa + 1) = 0
                Case Plans.BLUESTEIN
                    bluestein_real_full(a, offa, 1)
                    If isScale Then
                        scale(n, a, offa, True)
                    End If
            End Select
        End Sub

        Public Sub RealInverse2(a As Double(), offa As Integer, isScale As Boolean)
            If n = 1 Then Return
            Select Case plan
                Case Plans.SPLIT_RADIX
                    Dim xi As Double

                    If n > 4 Then
                        cftfsub(n, a, offa, ip, nw, w)
                        rftbsub(n, a, offa, nc, w, nw)
                    ElseIf n = 4 Then
                        cftbsub(n, a, offa, ip, nw, w)
                    End If
                    xi = a(offa) - a(offa + 1)
                    a(offa) += a(offa + 1)
                    a(offa + 1) = xi
                    If isScale Then
                        scale(n, a, offa, False)
                    End If
                Case Plans.MIXED_RADIX
                    rfftf(a, offa)
                    For k = n - 1 To 2 Step -1
                        Dim idx = offa + k
                        Dim tmp = a(idx)
                        a(idx) = a(idx - 1)
                        a(idx - 1) = tmp
                    Next
                    If isScale Then
                        scale(n, a, offa, False)
                    End If
                    Dim m As Integer
                    If n Mod 2 = 0 Then
                        m = n / 2
                        For i = 1 To m - 1
                            Dim idx = offa + 2 * i + 1
                            a(idx) = -a(idx)
                        Next
                    Else
                        m = (n - 1) / 2
                        For i = 0 To m - 1
                            Dim idx = offa + 2 * i + 1
                            a(idx) = -a(idx)
                        Next
                    End If
                Case Plans.BLUESTEIN
                    bluestein_real_inverse2(a, offa)
                    If isScale Then
                        scale(n, a, offa, False)
                    End If
            End Select
        End Sub
#End Region

#Region "Private Methods"
        Private Shared Function GetReminder(n As Integer, factors As Integer()) As Integer
            Dim reminder = n

            If n <= 0 Then
                Throw New ArgumentException("n must be positive integer")
            End If

            Dim i = 0

            While i < factors.Length AndAlso reminder <> 1
                Dim factor = factors(i)
                While reminder Mod factor = 0
                    reminder /= factor
                End While

                i += 1
            End While
            Return reminder
        End Function

        ' -------- initializing routines -------- 

        ' ---------------------------------------------------------
        ' cffti: initialization of Complex FFT
        ' --------------------------------------------------------

        Private Sub cffti(n As Integer, offw As Integer)
            If n = 1 Then Return

            Dim twon = 2 * n
            Dim fourn = 4 * n
            Dim argh As Double
            Dim idot, i, j As Integer, ntry = 0
            Dim argld As Double
            Dim i1, k1, l1, l2, ib As Integer
            Dim fi As Double
            Dim ld, ii, nf, ip, nl, nq, nr As Integer
            Dim arg As Double
            Dim ido, ipm As Integer

            nl = n
            nf = 0
            j = 0

factorize_loop:
            While True
                j += 1
                If j <= 4 Then
                    ntry = factors(j - 1)
                Else
                    ntry += 2
                End If
                Do
                    nq = nl / ntry
                    nr = nl - ntry * nq
                    If nr <> 0 Then
                        GoTo factorize_loop
                    End If
                    nf += 1
                    wtable(offw + nf + 1 + fourn) = ntry
                    nl = nq
                    If ntry = 2 AndAlso nf <> 1 Then
                        For i = 2 To nf
                            ib = nf - i + 2
                            Dim idx = ib + fourn
                            wtable(offw + idx + 1) = wtable(offw + idx)
                        Next
                        wtable(offw + 2 + fourn) = 2
                    End If
                Loop While nl <> 1
                Exit While
            End While
            wtable(offw + fourn) = n
            wtable(offw + 1 + fourn) = nf
            argh = TWO_PI / n
            i = 1
            l1 = 1
            For k1 = 1 To nf
                ip = CInt(wtable(offw + k1 + 1 + fourn))
                ld = 0
                l2 = l1 * ip
                ido = n / l2
                idot = ido + ido + 2
                ipm = ip - 1
                For j = 1 To ipm
                    i1 = i
                    wtable(offw + i - 1 + twon) = 1
                    wtable(offw + i + twon) = 0
                    ld += l1
                    fi = 0
                    argld = ld * argh
                    For ii = 4 To idot Step 2
                        i += 2
                        fi += 1
                        arg = fi * argld
                        Dim idx = i + twon
                        wtable(offw + idx - 1) = System.Math.Cos(arg)
                        wtable(offw + idx) = System.Math.Sin(arg)
                    Next
                    If ip > 5 Then
                        Dim idx1 = i1 + twon
                        Dim idx2 = i + twon
                        wtable(offw + idx1 - 1) = wtable(offw + idx2 - 1)
                        wtable(offw + idx1) = wtable(offw + idx2)
                    End If
                Next
                l1 = l2
            Next

        End Sub

        Private Sub cffti()
            If n = 1 Then Return

            Dim twon = 2 * n
            Dim fourn = 4 * n
            Dim argh As Double
            Dim idot, i, j As Integer, ntry = 0
            Dim argld As Double
            Dim i1, k1, l1, l2, ib As Integer
            Dim fi As Double
            Dim ld, ii, nf, ip, nl, nq, nr As Integer
            Dim arg As Double
            Dim ido, ipm As Integer

            nl = n
            nf = 0
            j = 0

factorize_loop:
            While True
                j += 1
                If j <= 4 Then
                    ntry = factors(j - 1)
                Else
                    ntry += 2
                End If
                Do
                    nq = nl / ntry
                    nr = nl - ntry * nq
                    If nr <> 0 Then
                        GoTo factorize_loop
                    End If
                    nf += 1
                    wtable(nf + 1 + fourn) = ntry
                    nl = nq
                    If ntry = 2 AndAlso nf <> 1 Then
                        For i = 2 To nf
                            ib = nf - i + 2
                            Dim idx = ib + fourn
                            wtable(idx + 1) = wtable(idx)
                        Next
                        wtable(2 + fourn) = 2
                    End If
                Loop While nl <> 1
                Exit While
            End While
            wtable(fourn) = n
            wtable(1 + fourn) = nf
            argh = TWO_PI / n
            i = 1
            l1 = 1
            For k1 = 1 To nf
                ip = CInt(wtable(k1 + 1 + fourn))
                ld = 0
                l2 = l1 * ip
                ido = n / l2
                idot = ido + ido + 2
                ipm = ip - 1
                For j = 1 To ipm
                    i1 = i
                    wtable(i - 1 + twon) = 1
                    wtable(i + twon) = 0
                    ld += l1
                    fi = 0
                    argld = ld * argh
                    For ii = 4 To idot Step 2
                        i += 2
                        fi += 1
                        arg = fi * argld
                        Dim idx = i + twon
                        wtable(idx - 1) = System.Math.Cos(arg)
                        wtable(idx) = System.Math.Sin(arg)
                    Next
                    If ip > 5 Then
                        Dim idx1 = i1 + twon
                        Dim idx2 = i + twon
                        wtable(idx1 - 1) = wtable(idx2 - 1)
                        wtable(idx1) = wtable(idx2)
                    End If
                Next
                l1 = l2
            Next

        End Sub

        Private Sub rffti()

            If n = 1 Then Return
            Dim twon = 2 * n
            Dim argh As Double
            Dim i, j As Integer, ntry = 0
            Dim argld As Double
            Dim k1, l1, l2, ib As Integer
            Dim fi As Double
            Dim ld, ii, nf, ip, nl, ies, nq, nr As Integer
            Dim arg As Double
            Dim ido, ipm As Integer
            Dim nfm1 As Integer

            nl = n
            nf = 0
            j = 0

factorize_loop:
            While True
                Threading.Interlocked.Increment(j)
                If j <= 4 Then
                    ntry = factors(j - 1)
                Else
                    ntry += 2
                End If
                Do
                    nq = nl / ntry
                    nr = nl - ntry * nq
                    If nr <> 0 Then
                        GoTo factorize_loop
                    End If
                    Threading.Interlocked.Increment(nf)
                    wtable_r(nf + 1 + twon) = ntry

                    nl = nq
                    If ntry = 2 AndAlso nf <> 1 Then
                        For i = 2 To nf
                            ib = nf - i + 2
                            Dim idx = ib + twon
                            wtable_r(idx + 1) = wtable_r(idx)
                        Next
                        wtable_r(2 + twon) = 2
                    End If
                Loop While nl <> 1
                Exit While
            End While
            wtable_r(twon) = n
            wtable_r(1 + twon) = nf
            argh = TWO_PI / n
            ies = 0
            nfm1 = nf - 1
            l1 = 1
            If nfm1 = 0 Then Return
            For k1 = 1 To nfm1
                ip = CInt(wtable_r(k1 + 1 + twon))
                ld = 0
                l2 = l1 * ip
                ido = n / l2
                ipm = ip - 1
                j = 1

                While j <= ipm
                    ld += l1
                    i = ies
                    argld = ld * argh

                    fi = 0
                    For ii = 3 To ido Step 2
                        i += 2
                        fi += 1
                        arg = fi * argld
                        Dim idx = i + n
                        wtable_r(idx - 2) = System.Math.Cos(arg)
                        wtable_r(idx - 1) = System.Math.Sin(arg)
                    Next
                    ies += ido
                    Threading.Interlocked.Increment(j)
                End While
                l1 = l2
            Next
        End Sub

        Private Sub bluesteini()
            Dim k = 0
            Dim arg As Double
            Dim pi_n = PI / n
            bk1(0) = 1
            bk1(1) = 0
            For i = 1 To n - 1
                k += 2 * i - 1
                If k >= 2 * n Then k -= 2 * n
                arg = pi_n * k
                bk1(2 * i) = System.Math.Cos(arg)
                bk1(2 * i + 1) = System.Math.Sin(arg)
            Next
            Dim scale = 1.0 / nBluestein
            bk2(0) = bk1(0) * scale
            bk2(1) = bk1(1) * scale
            For i = 2 To 2 * n - 1 Step 2
                bk2(i) = bk1(i) * scale
                bk2(i + 1) = bk1(i + 1) * scale
                bk2(2 * nBluestein - i) = bk2(i)
                bk2(2 * nBluestein - i + 1) = bk2(i + 1)
            Next
            cftbsub(2 * nBluestein, bk2, 0, ip, nw, w)
        End Sub

        Private Sub makewt(nw As Integer)
            Dim j, nwh, nw0, nw1 As Integer
            Dim delta, wn4r, wk1r, wk1i, wk3r, wk3i As Double
            Dim delta2, deltaj, deltaj3 As Double

            ip(0) = nw
            ip(1) = 1
            If nw > 2 Then
                nwh = nw >> 1
                delta = 0.78539816339744828 / nwh
                delta2 = delta * 2
                wn4r = System.Math.Cos(delta * nwh)
                w(0) = 1
                w(1) = wn4r
                If nwh = 4 Then
                    w(2) = System.Math.Cos(delta2)
                    w(3) = System.Math.Sin(delta2)
                ElseIf nwh > 4 Then
                    makeipt(nw)
                    w(2) = 0.5 / System.Math.Cos(delta2)
                    w(3) = 0.5 / System.Math.Cos(delta * 6)
                    For j = 4 To nwh - 1 Step 4
                        deltaj = delta * j
                        deltaj3 = 3 * deltaj
                        w(j) = System.Math.Cos(deltaj)
                        w(j + 1) = System.Math.Sin(deltaj)
                        w(j + 2) = System.Math.Cos(deltaj3)
                        w(j + 3) = -System.Math.Sin(deltaj3)
                    Next
                End If
                nw0 = 0
                While nwh > 2
                    nw1 = nw0 + nwh
                    nwh >>= 1
                    w(nw1) = 1
                    w(nw1 + 1) = wn4r
                    If nwh = 4 Then
                        wk1r = w(nw0 + 4)
                        wk1i = w(nw0 + 5)
                        w(nw1 + 2) = wk1r
                        w(nw1 + 3) = wk1i
                    ElseIf nwh > 4 Then
                        wk1r = w(nw0 + 4)
                        wk3r = w(nw0 + 6)
                        w(nw1 + 2) = 0.5 / wk1r
                        w(nw1 + 3) = 0.5 / wk3r
                        For j = 4 To nwh - 1 Step 4
                            Dim idx1 = nw0 + 2 * j
                            Dim idx2 = nw1 + j
                            wk1r = w(idx1)
                            wk1i = w(idx1 + 1)
                            wk3r = w(idx1 + 2)
                            wk3i = w(idx1 + 3)
                            w(idx2) = wk1r
                            w(idx2 + 1) = wk1i
                            w(idx2 + 2) = wk3r
                            w(idx2 + 3) = wk3i
                        Next
                    End If
                    nw0 = nw1
                End While
            End If
        End Sub

        Private Sub makeipt(nw As Integer)
            Dim j, l, m, m2, p, q As Integer

            ip(2) = 0
            ip(3) = 16
            m = 2
            l = nw

            While l > 32
                m2 = m << 1
                q = m2 << 3
                For j = m To m2 - 1
                    p = ip(j) << 2
                    ip(m + j) = p
                    ip(m2 + j) = p + q
                Next
                m = m2
                l >>= 2
            End While
        End Sub

        Private Sub makect(nc As Integer, c As Double(), startc As Integer)
            Dim j, nch As Integer
            Dim delta, deltaj As Double

            ip(1) = nc
            If nc > 1 Then
                nch = nc >> 1
                delta = 0.78539816339744828 / nch
                c(startc) = System.Math.Cos(delta * nch)
                c(startc + nch) = 0.5 * c(startc)
                For j = 1 To nch - 1
                    deltaj = delta * j
                    c(startc + j) = 0.5 * System.Math.Cos(deltaj)
                    c(startc + nc - j) = 0.5 * System.Math.Sin(deltaj)
                Next
            End If
        End Sub

        Private Sub bluestein_complex(a As Double(), offa As Integer, isign As Integer)
            Dim ak = New Double(2 * nBluestein - 1) {}
            Dim nthreads = 1
            Dim threads As Integer = Process.GetCurrentProcess().Threads.Count
            If threads > 1 AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_2THREADS Then
                nthreads = 2
                If threads >= 4 AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_4THREADS Then
                    nthreads = 4
                End If
                Dim taskArray = New Task(nthreads - 1) {}
                Dim k As Integer = n / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, n, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               If isign > 0 Then
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       Dim idx3 = offa + idx1
                                                                       Dim idx4 = offa + idx2
                                                                       ak(idx1) = a(idx3) * bk1(idx1) - a(idx4) * bk1(idx2)
                                                                       ak(idx2) = a(idx3) * bk1(idx2) + a(idx4) * bk1(idx1)
                                                                   Next
                                                               Else
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       Dim idx3 = offa + idx1
                                                                       Dim idx4 = offa + idx2
                                                                       ak(idx1) = a(idx3) * bk1(idx1) + a(idx4) * bk1(idx2)
                                                                       ak(idx2) = -a(idx3) * bk1(idx2) + a(idx4) * bk1(idx1)
                                                                   Next
                                                               End If
                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try

                cftbsub(2 * nBluestein, ak, 0, ip, nw, w)

                k = nBluestein / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, nBluestein, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               If isign > 0 Then
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       Dim im = -ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                                                                       ak(idx1) = ak(idx1) * bk2(idx1) + ak(idx2) * bk2(idx2)
                                                                       ak(idx2) = im
                                                                   Next
                                                               Else
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       Dim im = ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                                                                       ak(idx1) = ak(idx1) * bk2(idx1) - ak(idx2) * bk2(idx2)
                                                                       ak(idx2) = im
                                                                   Next
                                                               End If
                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try

                cftfsub(2 * nBluestein, ak, 0, ip, nw, w)

                k = n / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, n, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               If isign > 0 Then
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       Dim idx3 = offa + idx1
                                                                       Dim idx4 = offa + idx2
                                                                       a(idx3) = bk1(idx1) * ak(idx1) - bk1(idx2) * ak(idx2)
                                                                       a(idx4) = bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                                                                   Next
                                                               Else
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       Dim idx3 = offa + idx1
                                                                       Dim idx4 = offa + idx2
                                                                       a(idx3) = bk1(idx1) * ak(idx1) + bk1(idx2) * ak(idx2)
                                                                       a(idx4) = -bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                                                                   Next
                                                               End If
                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try
            Else
                If isign > 0 Then
                    For i = 0 To n - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        Dim idx3 = offa + idx1
                        Dim idx4 = offa + idx2
                        ak(idx1) = a(idx3) * bk1(idx1) - a(idx4) * bk1(idx2)
                        ak(idx2) = a(idx3) * bk1(idx2) + a(idx4) * bk1(idx1)
                    Next
                Else
                    For i = 0 To n - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        Dim idx3 = offa + idx1
                        Dim idx4 = offa + idx2
                        ak(idx1) = a(idx3) * bk1(idx1) + a(idx4) * bk1(idx2)
                        ak(idx2) = -a(idx3) * bk1(idx2) + a(idx4) * bk1(idx1)
                    Next
                End If

                cftbsub(2 * nBluestein, ak, 0, ip, nw, w)

                If isign > 0 Then
                    For i = 0 To nBluestein - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        Dim im = -ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                        ak(idx1) = ak(idx1) * bk2(idx1) + ak(idx2) * bk2(idx2)
                        ak(idx2) = im
                    Next
                Else
                    For i = 0 To nBluestein - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        Dim im = ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                        ak(idx1) = ak(idx1) * bk2(idx1) - ak(idx2) * bk2(idx2)
                        ak(idx2) = im
                    Next
                End If

                cftfsub(2 * nBluestein, ak, 0, ip, nw, w)
                If isign > 0 Then
                    For i = 0 To n - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        Dim idx3 = offa + idx1
                        Dim idx4 = offa + idx2
                        a(idx3) = bk1(idx1) * ak(idx1) - bk1(idx2) * ak(idx2)
                        a(idx4) = bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                    Next
                Else
                    For i = 0 To n - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        Dim idx3 = offa + idx1
                        Dim idx4 = offa + idx2
                        a(idx3) = bk1(idx1) * ak(idx1) + bk1(idx2) * ak(idx2)
                        a(idx4) = -bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                    Next
                End If
            End If
        End Sub


        Private Sub bluestein_real_full(a As Double(), offa As Integer, isign As Integer)
            Dim ak = New Double(2 * nBluestein - 1) {}
            Dim nthreads = 1
            Dim threads As Integer = Process.GetCurrentProcess().Threads.Count
            If threads > 1 AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_2THREADS Then
                nthreads = 2
                If threads >= 4 AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_4THREADS Then
                    nthreads = 4
                End If
                Dim taskArray = New Task(nthreads - 1) {}
                Dim k As Integer = n / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, n, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               If isign > 0 Then
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       Dim idx3 = offa + i
                                                                       ak(idx1) = a(idx3) * bk1(idx1)
                                                                       ak(idx2) = a(idx3) * bk1(idx2)
                                                                   Next
                                                               Else
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       Dim idx3 = offa + i
                                                                       ak(idx1) = a(idx3) * bk1(idx1)
                                                                       ak(idx2) = -a(idx3) * bk1(idx2)
                                                                   Next
                                                               End If

                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try

                cftbsub(2 * nBluestein, ak, 0, ip, nw, w)

                k = nBluestein / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, nBluestein, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               If isign > 0 Then
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       Dim im = -ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                                                                       ak(idx1) = ak(idx1) * bk2(idx1) + ak(idx2) * bk2(idx2)
                                                                       ak(idx2) = im
                                                                   Next
                                                               Else
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       Dim im = ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                                                                       ak(idx1) = ak(idx1) * bk2(idx1) - ak(idx2) * bk2(idx2)
                                                                       ak(idx2) = im
                                                                   Next
                                                               End If
                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try

                cftfsub(2 * nBluestein, ak, 0, ip, nw, w)

                k = n / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, n, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               If isign > 0 Then
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       a(offa + idx1) = bk1(idx1) * ak(idx1) - bk1(idx2) * ak(idx2)
                                                                       a(offa + idx2) = bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                                                                   Next
                                                               Else
                                                                   For i = firstIdx To lastIdx - 1
                                                                       Dim idx1 = 2 * i
                                                                       Dim idx2 = idx1 + 1
                                                                       a(offa + idx1) = bk1(idx1) * ak(idx1) + bk1(idx2) * ak(idx2)
                                                                       a(offa + idx2) = -bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                                                                   Next
                                                               End If

                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try
            Else
                If isign > 0 Then
                    For i = 0 To n - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        Dim idx3 = offa + i
                        ak(idx1) = a(idx3) * bk1(idx1)
                        ak(idx2) = a(idx3) * bk1(idx2)
                    Next
                Else
                    For i = 0 To n - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        Dim idx3 = offa + i
                        ak(idx1) = a(idx3) * bk1(idx1)
                        ak(idx2) = -a(idx3) * bk1(idx2)
                    Next
                End If

                cftbsub(2 * nBluestein, ak, 0, ip, nw, w)

                If isign > 0 Then
                    For i = 0 To nBluestein - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        Dim im = -ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                        ak(idx1) = ak(idx1) * bk2(idx1) + ak(idx2) * bk2(idx2)
                        ak(idx2) = im
                    Next
                Else
                    For i = 0 To nBluestein - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        Dim im = ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                        ak(idx1) = ak(idx1) * bk2(idx1) - ak(idx2) * bk2(idx2)
                        ak(idx2) = im
                    Next
                End If

                cftfsub(2 * nBluestein, ak, 0, ip, nw, w)

                If isign > 0 Then
                    For i = 0 To n - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        a(offa + idx1) = bk1(idx1) * ak(idx1) - bk1(idx2) * ak(idx2)
                        a(offa + idx2) = bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                    Next
                Else
                    For i = 0 To n - 1
                        Dim idx1 = 2 * i
                        Dim idx2 = idx1 + 1
                        a(offa + idx1) = bk1(idx1) * ak(idx1) + bk1(idx2) * ak(idx2)
                        a(offa + idx2) = -bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                    Next
                End If
            End If
        End Sub

        Private Sub bluestein_real_forward(a As Double(), offa As Integer)
            Dim ak = New Double(2 * nBluestein - 1) {}
            Dim nthreads = 1
            Dim threads As Integer = Process.GetCurrentProcess().Threads.Count
            If threads > 1 AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_2THREADS Then
                nthreads = 2
                If threads >= 4 AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_4THREADS Then
                    nthreads = 4
                End If
                Dim taskArray = New Task(nthreads - 1) {}
                Dim k As Integer = n / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, n, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               For i = firstIdx To lastIdx - 1
                                                                   Dim idx1 = 2 * i
                                                                   Dim idx2 = idx1 + 1
                                                                   Dim idx3 = offa + i
                                                                   ak(idx1) = a(idx3) * bk1(idx1)
                                                                   ak(idx2) = -a(idx3) * bk1(idx2)
                                                               Next
                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try

                cftbsub(2 * nBluestein, ak, 0, ip, nw, w)

                k = nBluestein / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, nBluestein, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               For i = firstIdx To lastIdx - 1
                                                                   Dim idx1 = 2 * i
                                                                   Dim idx2 = idx1 + 1
                                                                   Dim im = ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                                                                   ak(idx1) = ak(idx1) * bk2(idx1) - ak(idx2) * bk2(idx2)
                                                                   ak(idx2) = im
                                                               Next
                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try
            Else

                For i = 0 To n - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    Dim idx3 = offa + i
                    ak(idx1) = a(idx3) * bk1(idx1)
                    ak(idx2) = -a(idx3) * bk1(idx2)
                Next

                cftbsub(2 * nBluestein, ak, 0, ip, nw, w)

                For i = 0 To nBluestein - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    Dim im = ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                    ak(idx1) = ak(idx1) * bk2(idx1) - ak(idx2) * bk2(idx2)
                    ak(idx2) = im
                Next
            End If

            cftfsub(2 * nBluestein, ak, 0, ip, nw, w)

            If n Mod 2 = 0 Then
                a(offa) = bk1(0) * ak(0) + bk1(1) * ak(1)
                a(offa + 1) = bk1(n) * ak(n) + bk1(n + 1) * ak(n + 1)
                For i As Integer = 1 To n / 2 - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    a(offa + idx1) = bk1(idx1) * ak(idx1) + bk1(idx2) * ak(idx2)
                    a(offa + idx2) = -bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                Next
            Else
                a(offa) = bk1(0) * ak(0) + bk1(1) * ak(1)
                a(offa + 1) = -bk1(n) * ak(n - 1) + bk1(n - 1) * ak(n)
                For i As Integer = 1 To (n - 1) / 2 - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    a(offa + idx1) = bk1(idx1) * ak(idx1) + bk1(idx2) * ak(idx2)
                    a(offa + idx2) = -bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                Next
                a(offa + n - 1) = bk1(n - 1) * ak(n - 1) + bk1(n) * ak(n)
            End If
        End Sub

        Private Sub bluestein_real_inverse(a As Double(), offa As Integer)
            Dim ak = New Double(2 * nBluestein - 1) {}
            If n Mod 2 = 0 Then
                ak(0) = a(offa) * bk1(0)
                ak(1) = a(offa) * bk1(1)

                For i As Integer = 1 To n / 2 - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    Dim idx3 = offa + idx1
                    Dim idx4 = offa + idx2
                    ak(idx1) = a(idx3) * bk1(idx1) - a(idx4) * bk1(idx2)
                    ak(idx2) = a(idx3) * bk1(idx2) + a(idx4) * bk1(idx1)
                Next

                ak(n) = a(offa + 1) * bk1(n)
                ak(n + 1) = a(offa + 1) * bk1(n + 1)

                For i = n / 2 + 1 To n - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    Dim idx3 = offa + 2 * n - idx1
                    Dim idx4 = idx3 + 1
                    ak(idx1) = a(idx3) * bk1(idx1) + a(idx4) * bk1(idx2)
                    ak(idx2) = a(idx3) * bk1(idx2) - a(idx4) * bk1(idx1)
                Next
            Else
                ak(0) = a(offa) * bk1(0)
                ak(1) = a(offa) * bk1(1)

                For i As Integer = 1 To (n - 1) / 2 - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    Dim idx3 = offa + idx1
                    Dim idx4 = offa + idx2
                    ak(idx1) = a(idx3) * bk1(idx1) - a(idx4) * bk1(idx2)
                    ak(idx2) = a(idx3) * bk1(idx2) + a(idx4) * bk1(idx1)
                Next

                ak(n - 1) = a(offa + n - 1) * bk1(n - 1) - a(offa + 1) * bk1(n)
                ak(n) = a(offa + n - 1) * bk1(n) + a(offa + 1) * bk1(n - 1)

                ak(n + 1) = a(offa + n - 1) * bk1(n + 1) + a(offa + 1) * bk1(n + 2)
                ak(n + 2) = a(offa + n - 1) * bk1(n + 2) - a(offa + 1) * bk1(n + 1)

                For i = (n - 1) / 2 + 2 To n - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    Dim idx3 = offa + 2 * n - idx1
                    Dim idx4 = idx3 + 1
                    ak(idx1) = a(idx3) * bk1(idx1) + a(idx4) * bk1(idx2)
                    ak(idx2) = a(idx3) * bk1(idx2) - a(idx4) * bk1(idx1)
                Next
            End If

            cftbsub(2 * nBluestein, ak, 0, ip, nw, w)

            Dim nthreads = 1
            Dim threads As Integer = Process.GetCurrentProcess().Threads.Count
            If threads > 1 AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_2THREADS Then
                nthreads = 2
                If threads >= 4 AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_4THREADS Then
                    nthreads = 4
                End If
                Dim taskArray = New Task(nthreads - 1) {}
                Dim k As Integer = nBluestein / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, nBluestein, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               For i = firstIdx To lastIdx - 1
                                                                   Dim idx1 = 2 * i
                                                                   Dim idx2 = idx1 + 1
                                                                   Dim im = -ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                                                                   ak(idx1) = ak(idx1) * bk2(idx1) + ak(idx2) * bk2(idx2)
                                                                   ak(idx2) = im
                                                               Next
                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try

                cftfsub(2 * nBluestein, ak, 0, ip, nw, w)

                k = n / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, n, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               For i = firstIdx To lastIdx - 1
                                                                   Dim idx1 = 2 * i
                                                                   Dim idx2 = idx1 + 1
                                                                   a(offa + i) = bk1(idx1) * ak(idx1) - bk1(idx2) * ak(idx2)
                                                               Next
                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try
            Else
                For i = 0 To nBluestein - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    Dim im = -ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                    ak(idx1) = ak(idx1) * bk2(idx1) + ak(idx2) * bk2(idx2)
                    ak(idx2) = im
                Next

                cftfsub(2 * nBluestein, ak, 0, ip, nw, w)

                For i = 0 To n - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    a(offa + i) = bk1(idx1) * ak(idx1) - bk1(idx2) * ak(idx2)
                Next
            End If
        End Sub

        Private Sub bluestein_real_inverse2(a As Double(), offa As Integer)
            Dim ak = New Double(2 * nBluestein - 1) {}
            Dim nthreads = 1
            Dim threads As Integer = Process.GetCurrentProcess().Threads.Count
            If threads > 1 AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_2THREADS Then
                nthreads = 2
                If threads >= 4 AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_4THREADS Then
                    nthreads = 4
                End If
                Dim taskArray = New Task(nthreads - 1) {}
                Dim k As Integer = n / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, n, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               For i = firstIdx To lastIdx - 1
                                                                   Dim idx1 = 2 * i
                                                                   Dim idx2 = idx1 + 1
                                                                   Dim idx3 = offa + i
                                                                   ak(idx1) = a(idx3) * bk1(idx1)
                                                                   ak(idx2) = a(idx3) * bk1(idx2)
                                                               Next

                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try

                cftbsub(2 * nBluestein, ak, 0, ip, nw, w)

                k = nBluestein / nthreads
                For idx As Integer = 0 To nthreads - 1
                    Dim firstIdx = idx * k
                    Dim lastIdx = If(idx = nthreads - 1, nBluestein, firstIdx + k)
                    taskArray(idx) = Task.Factory.StartNew(Sub()
                                                               For i = firstIdx To lastIdx - 1
                                                                   Dim idx1 = 2 * i
                                                                   Dim idx2 = idx1 + 1
                                                                   Dim im = -ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                                                                   ak(idx1) = ak(idx1) * bk2(idx1) + ak(idx2) * bk2(idx2)
                                                                   ak(idx2) = im
                                                               Next

                                                           End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try
            Else
                For i = 0 To n - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    Dim idx3 = offa + i
                    ak(idx1) = a(idx3) * bk1(idx1)
                    ak(idx2) = a(idx3) * bk1(idx2)
                Next

                cftbsub(2 * nBluestein, ak, 0, ip, nw, w)

                For i = 0 To nBluestein - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    Dim im = -ak(idx1) * bk2(idx2) + ak(idx2) * bk2(idx1)
                    ak(idx1) = ak(idx1) * bk2(idx1) + ak(idx2) * bk2(idx2)
                    ak(idx2) = im
                Next
            End If

            cftfsub(2 * nBluestein, ak, 0, ip, nw, w)

            If n Mod 2 = 0 Then
                a(offa) = bk1(0) * ak(0) - bk1(1) * ak(1)
                a(offa + 1) = bk1(n) * ak(n) - bk1(n + 1) * ak(n + 1)
                For i As Integer = 1 To n / 2 - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    a(offa + idx1) = bk1(idx1) * ak(idx1) - bk1(idx2) * ak(idx2)
                    a(offa + idx2) = bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                Next
            Else
                a(offa) = bk1(0) * ak(0) - bk1(1) * ak(1)
                a(offa + 1) = bk1(n) * ak(n - 1) + bk1(n - 1) * ak(n)
                For i As Integer = 1 To (n - 1) / 2 - 1
                    Dim idx1 = 2 * i
                    Dim idx2 = idx1 + 1
                    a(offa + idx1) = bk1(idx1) * ak(idx1) - bk1(idx2) * ak(idx2)
                    a(offa + idx2) = bk1(idx2) * ak(idx1) + bk1(idx1) * ak(idx2)
                Next
                a(offa + n - 1) = bk1(n - 1) * ak(n - 1) - bk1(n) * ak(n)
            End If
        End Sub

        ' ---------------------------------------------------------
        ' rfftf1: further processing Of Real forward FFT
        ' --------------------------------------------------------
        Private Sub rfftf(a As Double(), offa As Integer)
            If n = 1 Then Return
            Dim l1, l2, na, kh, nf, ip, iw, ido, idl1 As Integer

            Dim ch = New Double(n - 1) {}
            Dim twon = 2 * n
            nf = CInt(wtable_r(1 + twon))
            na = 1
            l2 = n
            iw = twon - 1
            Dim k1 = 1

            While k1 <= nf
                kh = nf - k1
                ip = CInt(wtable_r(kh + 2 + twon))
                l1 = l2 / ip
                ido = n / l2
                idl1 = ido * l1
                iw -= (ip - 1) * ido
                na = 1 - na
                Select Case ip
                    Case 2
                        If na = 0 Then
                            radf2(ido, l1, a, offa, ch, 0, iw)
                        Else
                            radf2(ido, l1, ch, 0, a, offa, iw)
                        End If
                    Case 3
                        If na = 0 Then
                            radf3(ido, l1, a, offa, ch, 0, iw)
                        Else
                            radf3(ido, l1, ch, 0, a, offa, iw)
                        End If
                    Case 4
                        If na = 0 Then
                            radf4(ido, l1, a, offa, ch, 0, iw)
                        Else
                            radf4(ido, l1, ch, 0, a, offa, iw)
                        End If
                    Case 5
                        If na = 0 Then
                            radf5(ido, l1, a, offa, ch, 0, iw)
                        Else
                            radf5(ido, l1, ch, 0, a, offa, iw)
                        End If

                    Case Else
                        If ido = 1 Then na = 1 - na
                        If na = 0 Then
                            radfg(ido, ip, l1, idl1, a, offa, ch, 0, iw)
                            na = 1
                        Else
                            radfg(ido, ip, l1, idl1, ch, 0, a, offa, iw)
                            na = 0
                        End If
                End Select
                l2 = l1
                Threading.Interlocked.Increment(k1)
            End While
            If na = 1 Then Return
            Array.Copy(ch, 0, a, offa, n)
        End Sub

        ' ---------------------------------------------------------
        ' rfftb1: further processing Of Real backward FFT
        ' --------------------------------------------------------
        Private Sub rfftb(a As Double(), offa As Integer)
            If n = 1 Then Return
            Dim l1, l2, na, nf, ip, iw, ido, idl1 As Integer

            Dim ch = New Double(n - 1) {}
            Dim twon = 2 * n
            nf = CInt(wtable_r(1 + twon))
            na = 0
            l1 = 1
            iw = n
            For k1 = 1 To nf
                ip = CInt(wtable_r(k1 + 1 + twon))
                l2 = ip * l1
                ido = n / l2
                idl1 = ido * l1
                Select Case ip
                    Case 2
                        If na = 0 Then
                            radb2(ido, l1, a, offa, ch, 0, iw)
                        Else
                            radb2(ido, l1, ch, 0, a, offa, iw)
                        End If
                        na = 1 - na
                    Case 3
                        If na = 0 Then
                            radb3(ido, l1, a, offa, ch, 0, iw)
                        Else
                            radb3(ido, l1, ch, 0, a, offa, iw)
                        End If
                        na = 1 - na
                    Case 4
                        If na = 0 Then
                            radb4(ido, l1, a, offa, ch, 0, iw)
                        Else
                            radb4(ido, l1, ch, 0, a, offa, iw)
                        End If
                        na = 1 - na
                    Case 5
                        If na = 0 Then
                            radb5(ido, l1, a, offa, ch, 0, iw)
                        Else
                            radb5(ido, l1, ch, 0, a, offa, iw)
                        End If
                        na = 1 - na
                    Case Else
                        If na = 0 Then
                            radbg(ido, ip, l1, idl1, a, offa, ch, 0, iw)
                        Else
                            radbg(ido, ip, l1, idl1, ch, 0, a, offa, iw)
                        End If
                        If ido = 1 Then na = 1 - na
                End Select
                l1 = l2
                iw += (ip - 1) * ido
            Next
            If na = 0 Then Return
            Array.Copy(ch, 0, a, offa, n)
        End Sub

        ' -------------------------------------------------
        ' radf2: Real FFT 's forward processing of factor 2
        ' -------------------------------------------------
        Private Sub radf2(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer)
            Dim i, ic, idx0, idx1, idx2, idx3, idx4 As Integer
            Dim t1i, t1r, w1r, w1i As Double
            Dim iw1 As Integer
            iw1 = offset
            idx0 = l1 * ido
            idx1 = 2 * ido
            For k = 0 To l1 - 1
                Dim oidx1 = out_off + k * idx1
                Dim oidx2 = oidx1 + idx1 - 1
                Dim iidx1 = in_off + k * ido
                Dim iidx2 = iidx1 + idx0

                Dim i1r = inp(iidx1)
                Dim i2r = inp(iidx2)

                outp(oidx1) = i1r + i2r
                outp(oidx2) = i1r - i2r
            Next
            If ido < 2 Then Return
            If ido <> 2 Then
                For k = 0 To l1 - 1
                    idx1 = k * ido
                    idx2 = 2 * idx1
                    idx3 = idx2 + ido
                    idx4 = idx1 + idx0
                    For i = 2 To ido - 1 Step 2
                        ic = ido - i
                        Dim widx1 = i - 1 + iw1
                        Dim oidx1 = out_off + i + idx2
                        Dim oidx2 = out_off + ic + idx3
                        Dim iidx1 = in_off + i + idx1
                        Dim iidx2 = in_off + i + idx4

                        Dim a1i = inp(iidx1 - 1)
                        Dim a1r = inp(iidx1)
                        Dim a2i = inp(iidx2 - 1)
                        Dim a2r = inp(iidx2)

                        w1r = wtable_r(widx1 - 1)
                        w1i = wtable_r(widx1)

                        t1r = w1r * a2i + w1i * a2r
                        t1i = w1r * a2r - w1i * a2i

                        outp(oidx1) = a1r + t1i
                        outp(oidx1 - 1) = a1i + t1r

                        outp(oidx2) = t1i - a1r
                        outp(oidx2 - 1) = a1i - t1r
                    Next
                Next
                If ido Mod 2 = 1 Then Return
            End If
            idx2 = 2 * idx1
            For k = 0 To l1 - 1
                idx1 = k * ido
                Dim oidx1 = out_off + idx2 + ido
                Dim iidx1 = in_off + ido - 1 + idx1

                outp(oidx1) = -inp(iidx1 + idx0)
                outp(oidx1 - 1) = inp(iidx1)
            Next
        End Sub

        ' -------------------------------------------------
        ' radb2: Real FFT 's backward processing of factor 2
        ' -------------------------------------------------
        Private Sub radb2(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer)
            Dim i, ic As Integer
            Dim t1i, t1r, w1r, w1i As Double
            Dim iw1 = offset

            Dim idx0 = l1 * ido
            For k = 0 To l1 - 1
                Dim idx1 = k * ido
                Dim idx2 = 2 * idx1
                Dim idx3 = idx2 + ido
                Dim oidx1 = out_off + idx1
                Dim iidx1 = in_off + idx2
                Dim iidx2 = in_off + ido - 1 + idx3
                Dim i1r = inp(iidx1)
                Dim i2r = inp(iidx2)
                outp(oidx1) = i1r + i2r
                outp(oidx1 + idx0) = i1r - i2r
            Next
            If ido < 2 Then Return
            If ido <> 2 Then
                Dim k = 0

                While k < l1
                    Dim idx1 = k * ido
                    Dim idx2 = 2 * idx1
                    Dim idx3 = idx2 + ido
                    Dim idx4 = idx1 + idx0
                    For i = 2 To ido - 1 Step 2
                        ic = ido - i
                        Dim idx5 = i - 1 + iw1
                        Dim idx6 = out_off + i
                        Dim idx7 = in_off + i
                        Dim idx8 = in_off + ic
                        w1r = wtable_r(idx5 - 1)
                        w1i = wtable_r(idx5)
                        Dim iidx1 = idx7 + idx2
                        Dim iidx2 = idx8 + idx3
                        Dim oidx1 = idx6 + idx1
                        Dim oidx2 = idx6 + idx4
                        t1r = inp(iidx1 - 1) - inp(iidx2 - 1)
                        t1i = inp(iidx1) + inp(iidx2)
                        Dim i1i = inp(iidx1)
                        Dim i1r = inp(iidx1 - 1)
                        Dim i2i = inp(iidx2)
                        Dim i2r = inp(iidx2 - 1)

                        outp(oidx1 - 1) = i1r + i2r
                        outp(oidx1) = i1i - i2i
                        outp(oidx2 - 1) = w1r * t1r - w1i * t1i
                        outp(oidx2) = w1r * t1i + w1i * t1r
                    Next

                    Threading.Interlocked.Increment(k)
                End While
                If ido Mod 2 = 1 Then Return
            End If
            For k = 0 To l1 - 1
                Dim idx1 = k * ido
                Dim idx2 = 2 * idx1
                Dim oidx1 = out_off + ido - 1 + idx1
                Dim iidx1 = in_off + idx2 + ido
                outp(oidx1) = 2 * inp(iidx1 - 1)
                outp(oidx1 + idx0) = -2 * inp(iidx1)
            Next
        End Sub

        ' -------------------------------------------------
        ' radf3: Real FFT 's forward processing of factor 3 
        ' -------------------------------------------------
        Private Sub radf3(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer)
            Dim taur = -0.5
            Dim taui = 0.86602540378443871
            Dim i, ic As Integer
            Dim ci2, di2, di3, cr2, dr2, dr3, ti2, ti3, tr2, tr3, w1r, w2r, w1i, w2i As Double
            Dim iw1, iw2 As Integer
            iw1 = offset
            iw2 = iw1 + ido

            Dim idx0 = l1 * ido
            For k = 0 To l1 - 1
                Dim idx1 = k * ido
                Dim idx3 = 2 * idx0
                Dim idx4 = (3 * k + 1) * ido
                Dim iidx1 = in_off + idx1
                Dim iidx2 = iidx1 + idx0
                Dim iidx3 = iidx1 + idx3
                Dim i1r = inp(iidx1)
                Dim i2r = inp(iidx2)
                Dim i3r = inp(iidx3)
                cr2 = i2r + i3r
                outp(out_off + 3 * idx1) = i1r + cr2
                outp(out_off + idx4 + ido) = taui * (i3r - i2r)
                outp(out_off + ido - 1 + idx4) = i1r + taur * cr2
            Next
            If ido = 1 Then Return
            For k = 0 To l1 - 1
                Dim idx3 = k * ido
                Dim idx4 = 3 * idx3
                Dim idx5 = idx3 + idx0
                Dim idx6 = idx5 + idx0
                Dim idx7 = idx4 + ido
                Dim idx8 = idx7 + ido
                For i = 2 To ido - 1 Step 2
                    ic = ido - i
                    Dim widx1 = i - 1 + iw1
                    Dim widx2 = i - 1 + iw2

                    w1r = wtable_r(widx1 - 1)
                    w1i = wtable_r(widx1)
                    w2r = wtable_r(widx2 - 1)
                    w2i = wtable_r(widx2)

                    Dim idx9 = in_off + i
                    Dim idx10 = out_off + i
                    Dim idx11 = out_off + ic
                    Dim iidx1 = idx9 + idx3
                    Dim iidx2 = idx9 + idx5
                    Dim iidx3 = idx9 + idx6

                    Dim i1i = inp(iidx1 - 1)
                    Dim i1r = inp(iidx1)
                    Dim i2i = inp(iidx2 - 1)
                    Dim i2r = inp(iidx2)
                    Dim i3i = inp(iidx3 - 1)
                    Dim i3r = inp(iidx3)

                    dr2 = w1r * i2i + w1i * i2r
                    di2 = w1r * i2r - w1i * i2i
                    dr3 = w2r * i3i + w2i * i3r
                    di3 = w2r * i3r - w2i * i3i
                    cr2 = dr2 + dr3
                    ci2 = di2 + di3
                    tr2 = i1i + taur * cr2
                    ti2 = i1r + taur * ci2
                    tr3 = taui * (di2 - di3)
                    ti3 = taui * (dr3 - dr2)

                    Dim oidx1 = idx10 + idx4
                    Dim oidx2 = idx11 + idx7
                    Dim oidx3 = idx10 + idx8

                    outp(oidx1 - 1) = i1i + cr2
                    outp(oidx1) = i1r + ci2
                    outp(oidx2 - 1) = tr2 - tr3
                    outp(oidx2) = ti3 - ti2
                    outp(oidx3 - 1) = tr2 + tr3
                    outp(oidx3) = ti2 + ti3
                Next
            Next
        End Sub

        ' -------------------------------------------------
        ' radb3: Real FFT 's backward processing of factor 3
        ' -------------------------------------------------
        Private Sub radb3(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer)
            Dim taur = -0.5
            Dim taui = 0.86602540378443871
            Dim i, ic As Integer
            Dim ci2, ci3, di2, di3, cr2, cr3, dr2, dr3, ti2, tr2, w1r, w2r, w1i, w2i As Double
            Dim iw1, iw2 As Integer
            iw1 = offset
            iw2 = iw1 + ido

            For k = 0 To l1 - 1
                Dim idx1 = k * ido
                Dim iidx1 = in_off + 3 * idx1
                Dim iidx2 = iidx1 + 2 * ido
                Dim i1i = inp(iidx1)

                tr2 = 2 * inp(iidx2 - 1)
                cr2 = i1i + taur * tr2
                ci3 = 2 * taui * inp(iidx2)

                outp(out_off + idx1) = i1i + tr2
                outp(out_off + (k + l1) * ido) = cr2 - ci3
                outp(out_off + (k + 2 * l1) * ido) = cr2 + ci3
            Next
            If ido = 1 Then Return
            Dim idx0 = l1 * ido
            For k = 0 To l1 - 1
                Dim idx1 = k * ido
                Dim idx2 = 3 * idx1
                Dim idx3 = idx2 + ido
                Dim idx4 = idx3 + ido
                Dim idx5 = idx1 + idx0
                Dim idx6 = idx5 + idx0
                For i = 2 To ido - 1 Step 2
                    ic = ido - i
                    Dim idx7 = in_off + i
                    Dim idx8 = in_off + ic
                    Dim idx9 = out_off + i
                    Dim iidx1 = idx7 + idx2
                    Dim iidx2 = idx7 + idx4
                    Dim iidx3 = idx8 + idx3

                    Dim i1i = inp(iidx1 - 1)
                    Dim i1r = inp(iidx1)
                    Dim i2i = inp(iidx2 - 1)
                    Dim i2r = inp(iidx2)
                    Dim i3i = inp(iidx3 - 1)
                    Dim i3r = inp(iidx3)

                    tr2 = i2i + i3i
                    cr2 = i1i + taur * tr2
                    ti2 = i2r - i3r
                    ci2 = i1r + taur * ti2
                    cr3 = taui * (i2i - i3i)
                    ci3 = taui * (i2r + i3r)
                    dr2 = cr2 - ci3
                    dr3 = cr2 + ci3
                    di2 = ci2 + cr3
                    di3 = ci2 - cr3

                    Dim widx1 = i - 1 + iw1
                    Dim widx2 = i - 1 + iw2

                    w1r = wtable_r(widx1 - 1)
                    w1i = wtable_r(widx1)
                    w2r = wtable_r(widx2 - 1)
                    w2i = wtable_r(widx2)

                    Dim oidx1 = idx9 + idx1
                    Dim oidx2 = idx9 + idx5
                    Dim oidx3 = idx9 + idx6

                    outp(oidx1 - 1) = i1i + tr2
                    outp(oidx1) = i1r + ti2
                    outp(oidx2 - 1) = w1r * dr2 - w1i * di2
                    outp(oidx2) = w1r * di2 + w1i * dr2
                    outp(oidx3 - 1) = w2r * dr3 - w2i * di3
                    outp(oidx3) = w2r * di3 + w2i * dr3
                Next
            Next
        End Sub

        ' -------------------------------------------------
        ' radf4: Real FFT 's forward processing of factor 4
        ' -------------------------------------------------
        Private Sub radf4(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer)
            Dim hsqt2 = 0.70710678118654757
            Dim i, ic As Integer
            Dim ci2, ci3, ci4, cr2, cr3, cr4, ti1, ti2, ti3, ti4, tr1, tr2, tr3, tr4, w1r, w1i, w2r, w2i, w3r, w3i As Double
            Dim iw1, iw2, iw3 As Integer
            iw1 = offset
            iw2 = offset + ido
            iw3 = iw2 + ido
            Dim idx0 = l1 * ido
            For k = 0 To l1 - 1
                Dim idx1 = k * ido
                Dim idx2 = 4 * idx1
                Dim idx3 = idx1 + idx0
                Dim idx4 = idx3 + idx0
                Dim idx5 = idx4 + idx0
                Dim idx6 = idx2 + ido
                Dim i1r = inp(in_off + idx1)
                Dim i2r = inp(in_off + idx3)
                Dim i3r = inp(in_off + idx4)
                Dim i4r = inp(in_off + idx5)

                tr1 = i2r + i4r
                tr2 = i1r + i3r

                Dim oidx1 = out_off + idx2
                Dim oidx2 = out_off + idx6 + ido

                outp(oidx1) = tr1 + tr2
                outp(oidx2 - 1 + ido + ido) = tr2 - tr1
                outp(oidx2 - 1) = i1r - i3r
                outp(oidx2) = i4r - i2r
            Next
            If ido < 2 Then Return
            If ido <> 2 Then
                For k = 0 To l1 - 1
                    Dim idx1 = k * ido
                    Dim idx2 = idx1 + idx0
                    Dim idx3 = idx2 + idx0
                    Dim idx4 = idx3 + idx0
                    Dim idx5 = 4 * idx1
                    Dim idx6 = idx5 + ido
                    Dim idx7 = idx6 + ido
                    Dim idx8 = idx7 + ido
                    For i = 2 To ido - 1 Step 2
                        ic = ido - i
                        Dim widx1 = i - 1 + iw1
                        Dim widx2 = i - 1 + iw2
                        Dim widx3 = i - 1 + iw3
                        w1r = wtable_r(widx1 - 1)
                        w1i = wtable_r(widx1)
                        w2r = wtable_r(widx2 - 1)
                        w2i = wtable_r(widx2)
                        w3r = wtable_r(widx3 - 1)
                        w3i = wtable_r(widx3)

                        Dim idx9 = in_off + i
                        Dim idx10 = out_off + i
                        Dim idx11 = out_off + ic
                        Dim iidx1 = idx9 + idx1
                        Dim iidx2 = idx9 + idx2
                        Dim iidx3 = idx9 + idx3
                        Dim iidx4 = idx9 + idx4

                        Dim i1i = inp(iidx1 - 1)
                        Dim i1r = inp(iidx1)
                        Dim i2i = inp(iidx2 - 1)
                        Dim i2r = inp(iidx2)
                        Dim i3i = inp(iidx3 - 1)
                        Dim i3r = inp(iidx3)
                        Dim i4i = inp(iidx4 - 1)
                        Dim i4r = inp(iidx4)

                        cr2 = w1r * i2i + w1i * i2r
                        ci2 = w1r * i2r - w1i * i2i
                        cr3 = w2r * i3i + w2i * i3r
                        ci3 = w2r * i3r - w2i * i3i
                        cr4 = w3r * i4i + w3i * i4r
                        ci4 = w3r * i4r - w3i * i4i
                        tr1 = cr2 + cr4
                        tr4 = cr4 - cr2
                        ti1 = ci2 + ci4
                        ti4 = ci2 - ci4
                        ti2 = i1r + ci3
                        ti3 = i1r - ci3
                        tr2 = i1i + cr3
                        tr3 = i1i - cr3

                        Dim oidx1 = idx10 + idx5
                        Dim oidx2 = idx11 + idx6
                        Dim oidx3 = idx10 + idx7
                        Dim oidx4 = idx11 + idx8

                        outp(oidx1 - 1) = tr1 + tr2
                        outp(oidx4 - 1) = tr2 - tr1
                        outp(oidx1) = ti1 + ti2
                        outp(oidx4) = ti1 - ti2
                        outp(oidx3 - 1) = ti4 + tr3
                        outp(oidx2 - 1) = tr3 - ti4
                        outp(oidx3) = tr4 + ti3
                        outp(oidx2) = tr4 - ti3
                    Next
                Next
                If ido Mod 2 = 1 Then Return
            End If
            For k = 0 To l1 - 1
                Dim idx1 = k * ido
                Dim idx2 = 4 * idx1
                Dim idx3 = idx1 + idx0
                Dim idx4 = idx3 + idx0
                Dim idx5 = idx4 + idx0
                Dim idx6 = idx2 + ido
                Dim idx7 = idx6 + ido
                Dim idx8 = idx7 + ido
                Dim idx9 = in_off + ido
                Dim idx10 = out_off + ido

                Dim i1i = inp(idx9 - 1 + idx1)
                Dim i2i = inp(idx9 - 1 + idx3)
                Dim i3i = inp(idx9 - 1 + idx4)
                Dim i4i = inp(idx9 - 1 + idx5)

                ti1 = -hsqt2 * (i2i + i4i)
                tr1 = hsqt2 * (i2i - i4i)

                outp(idx10 - 1 + idx2) = tr1 + i1i
                outp(idx10 - 1 + idx7) = i1i - tr1
                outp(out_off + idx6) = ti1 - i3i
                outp(out_off + idx8) = ti1 + i3i
            Next
        End Sub

        ' -------------------------------------------------
        ' radb4: Real FFT 's backward processing of factor 4
        ' -------------------------------------------------
        Private Sub radb4(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer)
            Dim sqrt2 = 1.4142135623730951
            Dim i, ic As Integer
            Dim ci2, ci3, ci4, cr2, cr3, cr4 As Double
            Dim ti1, ti2, ti3, ti4, tr1, tr2, tr3, tr4, w1r, w1i, w2r, w2i, w3r, w3i As Double
            Dim iw1, iw2, iw3 As Integer
            iw1 = offset
            iw2 = iw1 + ido
            iw3 = iw2 + ido

            Dim idx0 = l1 * ido
            For k = 0 To l1 - 1
                Dim idx1 = k * ido
                Dim idx2 = 4 * idx1
                Dim idx3 = idx1 + idx0
                Dim idx4 = idx3 + idx0
                Dim idx5 = idx4 + idx0
                Dim idx6 = idx2 + ido
                Dim idx7 = idx6 + ido
                Dim idx8 = idx7 + ido

                Dim i1r = inp(in_off + idx2)
                Dim i2r = inp(in_off + idx7)
                Dim i3r = inp(in_off + ido - 1 + idx8)
                Dim i4r = inp(in_off + ido - 1 + idx6)

                tr1 = i1r - i3r
                tr2 = i1r + i3r
                tr3 = i4r + i4r
                tr4 = i2r + i2r

                outp(out_off + idx1) = tr2 + tr3
                outp(out_off + idx3) = tr1 - tr4
                outp(out_off + idx4) = tr2 - tr3
                outp(out_off + idx5) = tr1 + tr4
            Next
            If ido < 2 Then Return
            If ido <> 2 Then
                Dim k = 0

                While k < l1
                    Dim idx1 = k * ido
                    Dim idx2 = idx1 + idx0
                    Dim idx3 = idx2 + idx0
                    Dim idx4 = idx3 + idx0
                    Dim idx5 = 4 * idx1
                    Dim idx6 = idx5 + ido
                    Dim idx7 = idx6 + ido
                    Dim idx8 = idx7 + ido
                    For i = 2 To ido - 1 Step 2
                        ic = ido - i
                        Dim widx1 = i - 1 + iw1
                        Dim widx2 = i - 1 + iw2
                        Dim widx3 = i - 1 + iw3
                        w1r = wtable_r(widx1 - 1)
                        w1i = wtable_r(widx1)
                        w2r = wtable_r(widx2 - 1)
                        w2i = wtable_r(widx2)
                        w3r = wtable_r(widx3 - 1)
                        w3i = wtable_r(widx3)

                        Dim idx12 = in_off + i
                        Dim idx13 = in_off + ic
                        Dim idx14 = out_off + i

                        Dim iidx1 = idx12 + idx5
                        Dim iidx2 = idx13 + idx6
                        Dim iidx3 = idx12 + idx7
                        Dim iidx4 = idx13 + idx8

                        Dim i1i = inp(iidx1 - 1)
                        Dim i1r = inp(iidx1)
                        Dim i2i = inp(iidx2 - 1)
                        Dim i2r = inp(iidx2)
                        Dim i3i = inp(iidx3 - 1)
                        Dim i3r = inp(iidx3)
                        Dim i4i = inp(iidx4 - 1)
                        Dim i4r = inp(iidx4)

                        ti1 = i1r + i4r
                        ti2 = i1r - i4r
                        ti3 = i3r - i2r
                        tr4 = i3r + i2r
                        tr1 = i1i - i4i
                        tr2 = i1i + i4i
                        ti4 = i3i - i2i
                        tr3 = i3i + i2i
                        cr3 = tr2 - tr3
                        ci3 = ti2 - ti3
                        cr2 = tr1 - tr4
                        cr4 = tr1 + tr4
                        ci2 = ti1 + ti4
                        ci4 = ti1 - ti4

                        Dim oidx1 = idx14 + idx1
                        Dim oidx2 = idx14 + idx2
                        Dim oidx3 = idx14 + idx3
                        Dim oidx4 = idx14 + idx4

                        outp(oidx1 - 1) = tr2 + tr3
                        outp(oidx1) = ti2 + ti3
                        outp(oidx2 - 1) = w1r * cr2 - w1i * ci2
                        outp(oidx2) = w1r * ci2 + w1i * cr2
                        outp(oidx3 - 1) = w2r * cr3 - w2i * ci3
                        outp(oidx3) = w2r * ci3 + w2i * cr3
                        outp(oidx4 - 1) = w3r * cr4 - w3i * ci4
                        outp(oidx4) = w3r * ci4 + w3i * cr4
                    Next

                    Threading.Interlocked.Increment(k)
                End While
                If ido Mod 2 = 1 Then Return
            End If
            For k = 0 To l1 - 1
                Dim idx1 = k * ido
                Dim idx2 = 4 * idx1
                Dim idx3 = idx1 + idx0
                Dim idx4 = idx3 + idx0
                Dim idx5 = idx4 + idx0
                Dim idx6 = idx2 + ido
                Dim idx7 = idx6 + ido
                Dim idx8 = idx7 + ido
                Dim idx9 = in_off + ido
                Dim idx10 = out_off + ido

                Dim i1r = inp(idx9 - 1 + idx2)
                Dim i2r = inp(idx9 - 1 + idx7)
                Dim i3r = inp(in_off + idx6)
                Dim i4r = inp(in_off + idx8)

                ti1 = i3r + i4r
                ti2 = i4r - i3r
                tr1 = i1r - i2r
                tr2 = i1r + i2r

                outp(idx10 - 1 + idx1) = tr2 + tr2
                outp(idx10 - 1 + idx3) = sqrt2 * (tr1 - ti1)
                outp(idx10 - 1 + idx4) = ti2 + ti2
                outp(idx10 - 1 + idx5) = -sqrt2 * (tr1 + ti1)
            Next
        End Sub

        ' -------------------------------------------------
        ' radf5: Real FFT 's forward processing of factor 5
        ' -------------------------------------------------
        Private Sub radf5(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer)
            Dim tr11 = 0.30901699437494745
            Dim ti11 = 0.95105651629515353
            Dim tr12 = -0.80901699437494734
            Dim ti12 = 0.58778525229247325
            Dim i, ic As Integer
            Dim ci2, di2, ci4, ci5, di3, di4, di5, ci3, cr2, cr3, dr2, dr3, dr4, dr5, cr5, cr4, ti2, ti3, ti5, ti4, tr2, tr3, tr4, tr5, w1r, w1i, w2r, w2i, w3r, w3i, w4r, w4i As Double
            Dim iw1, iw2, iw3, iw4 As Integer
            iw1 = offset
            iw2 = iw1 + ido
            iw3 = iw2 + ido
            iw4 = iw3 + ido

            Dim idx0 = l1 * ido
            For k1 As Integer = 0 To l1 - 1
                Dim idx1 = k1 * ido
                Dim idx2 = 5 * idx1
                Dim idx3 = idx2 + ido
                Dim idx4 = idx3 + ido
                Dim idx5 = idx4 + ido
                Dim idx6 = idx5 + ido
                Dim idx7 = idx1 + idx0
                Dim idx8 = idx7 + idx0
                Dim idx9 = idx8 + idx0
                Dim idx10 = idx9 + idx0
                Dim idx11 = out_off + ido - 1

                Dim i1r = inp(in_off + idx1)
                Dim i2r = inp(in_off + idx7)
                Dim i3r = inp(in_off + idx8)
                Dim i4r = inp(in_off + idx9)
                Dim i5r = inp(in_off + idx10)

                cr2 = i5r + i2r
                ci5 = i5r - i2r
                cr3 = i4r + i3r
                ci4 = i4r - i3r

                outp(out_off + idx2) = i1r + cr2 + cr3
                outp(idx11 + idx3) = i1r + tr11 * cr2 + tr12 * cr3
                outp(out_off + idx4) = ti11 * ci5 + ti12 * ci4
                outp(idx11 + idx5) = i1r + tr12 * cr2 + tr11 * cr3
                outp(out_off + idx6) = ti12 * ci5 - ti11 * ci4
            Next
            If ido = 1 Then Return
            Dim k = 0

            While k < l1
                Dim idx1 = k * ido
                Dim idx2 = 5 * idx1
                Dim idx3 = idx2 + ido
                Dim idx4 = idx3 + ido
                Dim idx5 = idx4 + ido
                Dim idx6 = idx5 + ido
                Dim idx7 = idx1 + idx0
                Dim idx8 = idx7 + idx0
                Dim idx9 = idx8 + idx0
                Dim idx10 = idx9 + idx0
                For i = 2 To ido - 1 Step 2
                    Dim widx1 = i - 1 + iw1
                    Dim widx2 = i - 1 + iw2
                    Dim widx3 = i - 1 + iw3
                    Dim widx4 = i - 1 + iw4
                    w1r = wtable_r(widx1 - 1)
                    w1i = wtable_r(widx1)
                    w2r = wtable_r(widx2 - 1)
                    w2i = wtable_r(widx2)
                    w3r = wtable_r(widx3 - 1)
                    w3i = wtable_r(widx3)
                    w4r = wtable_r(widx4 - 1)
                    w4i = wtable_r(widx4)

                    ic = ido - i
                    Dim idx15 = in_off + i
                    Dim idx16 = out_off + i
                    Dim idx17 = out_off + ic

                    Dim iidx1 = idx15 + idx1
                    Dim iidx2 = idx15 + idx7
                    Dim iidx3 = idx15 + idx8
                    Dim iidx4 = idx15 + idx9
                    Dim iidx5 = idx15 + idx10

                    Dim i1i = inp(iidx1 - 1)
                    Dim i1r = inp(iidx1)
                    Dim i2i = inp(iidx2 - 1)
                    Dim i2r = inp(iidx2)
                    Dim i3i = inp(iidx3 - 1)
                    Dim i3r = inp(iidx3)
                    Dim i4i = inp(iidx4 - 1)
                    Dim i4r = inp(iidx4)
                    Dim i5i = inp(iidx5 - 1)
                    Dim i5r = inp(iidx5)

                    dr2 = w1r * i2i + w1i * i2r
                    di2 = w1r * i2r - w1i * i2i
                    dr3 = w2r * i3i + w2i * i3r
                    di3 = w2r * i3r - w2i * i3i
                    dr4 = w3r * i4i + w3i * i4r
                    di4 = w3r * i4r - w3i * i4i
                    dr5 = w4r * i5i + w4i * i5r
                    di5 = w4r * i5r - w4i * i5i

                    cr2 = dr2 + dr5
                    ci5 = dr5 - dr2
                    cr5 = di2 - di5
                    ci2 = di2 + di5
                    cr3 = dr3 + dr4
                    ci4 = dr4 - dr3
                    cr4 = di3 - di4
                    ci3 = di3 + di4

                    tr2 = i1i + tr11 * cr2 + tr12 * cr3
                    ti2 = i1r + tr11 * ci2 + tr12 * ci3
                    tr3 = i1i + tr12 * cr2 + tr11 * cr3
                    ti3 = i1r + tr12 * ci2 + tr11 * ci3
                    tr5 = ti11 * cr5 + ti12 * cr4
                    ti5 = ti11 * ci5 + ti12 * ci4
                    tr4 = ti12 * cr5 - ti11 * cr4
                    ti4 = ti12 * ci5 - ti11 * ci4

                    Dim oidx1 = idx16 + idx2
                    Dim oidx2 = idx17 + idx3
                    Dim oidx3 = idx16 + idx4
                    Dim oidx4 = idx17 + idx5
                    Dim oidx5 = idx16 + idx6

                    outp(oidx1 - 1) = i1i + cr2 + cr3
                    outp(oidx1) = i1r + ci2 + ci3
                    outp(oidx3 - 1) = tr2 + tr5
                    outp(oidx2 - 1) = tr2 - tr5
                    outp(oidx3) = ti2 + ti5
                    outp(oidx2) = ti5 - ti2
                    outp(oidx5 - 1) = tr3 + tr4
                    outp(oidx4 - 1) = tr3 - tr4
                    outp(oidx5) = ti3 + ti4
                    outp(oidx4) = ti4 - ti3
                Next

                Threading.Interlocked.Increment(k)
            End While
        End Sub

        ' -------------------------------------------------
        ' radb5: Real FFT 's backward processing of factor 5
        ' -------------------------------------------------
        Private Sub radb5(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer)
            Dim tr11 = 0.30901699437494745
            Dim ti11 = 0.95105651629515353
            Dim tr12 = -0.80901699437494734
            Dim ti12 = 0.58778525229247325
            Dim i, ic As Integer
            Dim ci2, ci3, ci4, ci5, di3, di4, di5, di2, cr2, cr3, cr5, cr4, ti2, ti3, ti4, ti5, dr3, dr4, dr5, dr2, tr2, tr3, tr4, tr5, w1r, w1i, w2r, w2i, w3r, w3i, w4r, w4i As Double
            Dim iw1, iw2, iw3, iw4 As Integer
            iw1 = offset
            iw2 = iw1 + ido
            iw3 = iw2 + ido
            iw4 = iw3 + ido

            Dim idx0 = l1 * ido
            For k1 = 0 To l1 - 1
                Dim idx1 = k1 * ido
                Dim idx2 = 5 * idx1
                Dim idx3 = idx2 + ido
                Dim idx4 = idx3 + ido
                Dim idx5 = idx4 + ido
                Dim idx6 = idx5 + ido
                Dim idx7 = idx1 + idx0
                Dim idx8 = idx7 + idx0
                Dim idx9 = idx8 + idx0
                Dim idx10 = idx9 + idx0
                Dim idx11 = in_off + ido - 1

                Dim i1r = inp(in_off + idx2)

                ti5 = 2 * inp(in_off + idx4)
                ti4 = 2 * inp(in_off + idx6)
                tr2 = 2 * inp(idx11 + idx3)
                tr3 = 2 * inp(idx11 + idx5)
                cr2 = i1r + tr11 * tr2 + tr12 * tr3
                cr3 = i1r + tr12 * tr2 + tr11 * tr3
                ci5 = ti11 * ti5 + ti12 * ti4
                ci4 = ti12 * ti5 - ti11 * ti4

                outp(out_off + idx1) = i1r + tr2 + tr3
                outp(out_off + idx7) = cr2 - ci5
                outp(out_off + idx8) = cr3 - ci4
                outp(out_off + idx9) = cr3 + ci4
                outp(out_off + idx10) = cr2 + ci5
            Next
            If ido = 1 Then Return
            Dim k = 0

            While k < l1
                Dim idx1 = k * ido
                Dim idx2 = 5 * idx1
                Dim idx3 = idx2 + ido
                Dim idx4 = idx3 + ido
                Dim idx5 = idx4 + ido
                Dim idx6 = idx5 + ido
                Dim idx7 = idx1 + idx0
                Dim idx8 = idx7 + idx0
                Dim idx9 = idx8 + idx0
                Dim idx10 = idx9 + idx0
                For i = 2 To ido - 1 Step 2
                    ic = ido - i
                    Dim widx1 = i - 1 + iw1
                    Dim widx2 = i - 1 + iw2
                    Dim widx3 = i - 1 + iw3
                    Dim widx4 = i - 1 + iw4
                    w1r = wtable_r(widx1 - 1)
                    w1i = wtable_r(widx1)
                    w2r = wtable_r(widx2 - 1)
                    w2i = wtable_r(widx2)
                    w3r = wtable_r(widx3 - 1)
                    w3i = wtable_r(widx3)
                    w4r = wtable_r(widx4 - 1)
                    w4i = wtable_r(widx4)

                    Dim idx15 = in_off + i
                    Dim idx16 = in_off + ic
                    Dim idx17 = out_off + i

                    Dim iidx1 = idx15 + idx2
                    Dim iidx2 = idx16 + idx3
                    Dim iidx3 = idx15 + idx4
                    Dim iidx4 = idx16 + idx5
                    Dim iidx5 = idx15 + idx6

                    Dim i1i = inp(iidx1 - 1)
                    Dim i1r = inp(iidx1)
                    Dim i2i = inp(iidx2 - 1)
                    Dim i2r = inp(iidx2)
                    Dim i3i = inp(iidx3 - 1)
                    Dim i3r = inp(iidx3)
                    Dim i4i = inp(iidx4 - 1)
                    Dim i4r = inp(iidx4)
                    Dim i5i = inp(iidx5 - 1)
                    Dim i5r = inp(iidx5)

                    ti5 = i3r + i2r
                    ti2 = i3r - i2r
                    ti4 = i5r + i4r
                    ti3 = i5r - i4r
                    tr5 = i3i - i2i
                    tr2 = i3i + i2i
                    tr4 = i5i - i4i
                    tr3 = i5i + i4i

                    cr2 = i1i + tr11 * tr2 + tr12 * tr3
                    ci2 = i1r + tr11 * ti2 + tr12 * ti3
                    cr3 = i1i + tr12 * tr2 + tr11 * tr3
                    ci3 = i1r + tr12 * ti2 + tr11 * ti3
                    cr5 = ti11 * tr5 + ti12 * tr4
                    ci5 = ti11 * ti5 + ti12 * ti4
                    cr4 = ti12 * tr5 - ti11 * tr4
                    ci4 = ti12 * ti5 - ti11 * ti4
                    dr3 = cr3 - ci4
                    dr4 = cr3 + ci4
                    di3 = ci3 + cr4
                    di4 = ci3 - cr4
                    dr5 = cr2 + ci5
                    dr2 = cr2 - ci5
                    di5 = ci2 - cr5
                    di2 = ci2 + cr5

                    Dim oidx1 = idx17 + idx1
                    Dim oidx2 = idx17 + idx7
                    Dim oidx3 = idx17 + idx8
                    Dim oidx4 = idx17 + idx9
                    Dim oidx5 = idx17 + idx10

                    outp(oidx1 - 1) = i1i + tr2 + tr3
                    outp(oidx1) = i1r + ti2 + ti3
                    outp(oidx2 - 1) = w1r * dr2 - w1i * di2
                    outp(oidx2) = w1r * di2 + w1i * dr2
                    outp(oidx3 - 1) = w2r * dr3 - w2i * di3
                    outp(oidx3) = w2r * di3 + w2i * dr3
                    outp(oidx4 - 1) = w3r * dr4 - w3i * di4
                    outp(oidx4) = w3r * di4 + w3i * dr4
                    outp(oidx5 - 1) = w4r * dr5 - w4i * di5
                    outp(oidx5) = w4r * di5 + w4i * dr5
                Next

                Threading.Interlocked.Increment(k)
            End While
        End Sub

        ' ---------------------------------------------------------
        ' radfg: Real FFT 's forward processing of general factor
        ' --------------------------------------------------------
        Private Sub radfg(ido As Integer, ip As Integer, l1 As Integer, idl1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer)
            Dim idij, ipph, j2, ic, jc, lc, ies, nbd As Integer
            Dim dc2, ai1, ai2, ar1, ar2, ds2, dcp, arg, dsp, ar1h, ar2h, w1r, w1i As Double
            Dim iw1 = offset

            arg = TWO_PI / ip
            dcp = System.Math.Cos(arg)
            dsp = System.Math.Sin(arg)
            ipph = (ip + 1) / 2
            nbd = (ido - 1) / 2
            If ido <> 1 Then
                For ik = 0 To idl1 - 1
                    outp(out_off + ik) = inp(in_off + ik)
                Next
                For j = 1 To ip - 1
                    Dim idx1 = j * l1 * ido
                    For k = 0 To l1 - 1
                        Dim idx2 = k * ido + idx1
                        outp(out_off + idx2) = inp(in_off + idx2)
                    Next
                Next
                If nbd <= l1 Then
                    ies = -ido
                    For j = 1 To ip - 1
                        ies += ido
                        idij = ies - 1
                        Dim idx1 = j * l1 * ido
                        For i = 2 To ido - 1 Step 2
                            idij += 2
                            Dim idx2 = idij + iw1
                            Dim idx4 = in_off + i
                            Dim idx5 = out_off + i
                            w1r = wtable_r(idx2 - 1)
                            w1i = wtable_r(idx2)
                            For k = 0 To l1 - 1
                                Dim idx3 = k * ido + idx1
                                Dim oidx1 = idx5 + idx3
                                Dim iidx1 = idx4 + idx3
                                Dim i1i = inp(iidx1 - 1)
                                Dim i1r = inp(iidx1)

                                outp(oidx1 - 1) = w1r * i1i + w1i * i1r
                                outp(oidx1) = w1r * i1r - w1i * i1i
                            Next
                        Next
                    Next
                Else
                    ies = -ido
                    For j = 1 To ip - 1
                        ies += ido
                        Dim idx1 = j * l1 * ido
                        For k = 0 To l1 - 1
                            idij = ies - 1
                            Dim idx3 = k * ido + idx1
                            For i = 2 To ido - 1 Step 2
                                idij += 2
                                Dim idx2 = idij + iw1
                                w1r = wtable_r(idx2 - 1)
                                w1i = wtable_r(idx2)
                                Dim oidx1 = out_off + i + idx3
                                Dim iidx1 = in_off + i + idx3
                                Dim i1i = inp(iidx1 - 1)
                                Dim i1r = inp(iidx1)

                                outp(oidx1 - 1) = w1r * i1i + w1i * i1r
                                outp(oidx1) = w1r * i1r - w1i * i1i
                            Next
                        Next
                    Next
                End If
                If nbd >= l1 Then
                    For j = 1 To ipph - 1
                        jc = ip - j
                        Dim idx1 = j * l1 * ido
                        Dim idx2 = jc * l1 * ido
                        For k = 0 To l1 - 1
                            Dim idx3 = k * ido + idx1
                            Dim idx4 = k * ido + idx2
                            For i = 2 To ido - 1 Step 2
                                Dim idx5 = in_off + i
                                Dim idx6 = out_off + i
                                Dim iidx1 = idx5 + idx3
                                Dim iidx2 = idx5 + idx4
                                Dim oidx1 = idx6 + idx3
                                Dim oidx2 = idx6 + idx4
                                Dim o1i = outp(oidx1 - 1)
                                Dim o1r = outp(oidx1)
                                Dim o2i = outp(oidx2 - 1)
                                Dim o2r = outp(oidx2)

                                inp(iidx1 - 1) = o1i + o2i
                                inp(iidx1) = o1r + o2r

                                inp(iidx2 - 1) = o1r - o2r
                                inp(iidx2) = o2i - o1i
                            Next
                        Next
                    Next
                Else
                    For j = 1 To ipph - 1
                        jc = ip - j
                        Dim idx1 = j * l1 * ido
                        Dim idx2 = jc * l1 * ido
                        For i = 2 To ido - 1 Step 2
                            Dim idx5 = in_off + i
                            Dim idx6 = out_off + i
                            For k = 0 To l1 - 1
                                Dim idx3 = k * ido + idx1
                                Dim idx4 = k * ido + idx2
                                Dim iidx1 = idx5 + idx3
                                Dim iidx2 = idx5 + idx4
                                Dim oidx1 = idx6 + idx3
                                Dim oidx2 = idx6 + idx4
                                Dim o1i = outp(oidx1 - 1)
                                Dim o1r = outp(oidx1)
                                Dim o2i = outp(oidx2 - 1)
                                Dim o2r = outp(oidx2)

                                inp(iidx1 - 1) = o1i + o2i
                                inp(iidx1) = o1r + o2r
                                inp(iidx2 - 1) = o1r - o2r
                                inp(iidx2) = o2i - o1i
                            Next
                        Next
                    Next
                End If
            Else
                Array.Copy(outp, out_off, inp, in_off, idl1)
            End If
            For j = 1 To ipph - 1
                jc = ip - j
                Dim idx1 = j * l1 * ido
                Dim idx2 = jc * l1 * ido
                For k = 0 To l1 - 1
                    Dim idx3 = k * ido + idx1
                    Dim idx4 = k * ido + idx2
                    Dim oidx1 = out_off + idx3
                    Dim oidx2 = out_off + idx4
                    Dim o1r = outp(oidx1)
                    Dim o2r = outp(oidx2)

                    inp(in_off + idx3) = o1r + o2r
                    inp(in_off + idx4) = o2r - o1r
                Next
            Next

            ar1 = 1
            ai1 = 0
            Dim idx0 = (ip - 1) * idl1
            For l = 1 To ipph - 1
                lc = ip - l
                ar1h = dcp * ar1 - dsp * ai1
                ai1 = dcp * ai1 + dsp * ar1
                ar1 = ar1h
                Dim idx1 = l * idl1
                Dim idx2 = lc * idl1
                For ik = 0 To idl1 - 1
                    Dim idx3 = out_off + ik
                    Dim idx4 = in_off + ik
                    outp(idx3 + idx1) = inp(idx4) + ar1 * inp(idx4 + idl1)
                    outp(idx3 + idx2) = ai1 * inp(idx4 + idx0)
                Next
                dc2 = ar1
                ds2 = ai1
                ar2 = ar1
                ai2 = ai1
                For j = 2 To ipph - 1
                    jc = ip - j
                    ar2h = dc2 * ar2 - ds2 * ai2
                    ai2 = dc2 * ai2 + ds2 * ar2
                    ar2 = ar2h
                    Dim idx3 = j * idl1
                    Dim idx4 = jc * idl1
                    For ik = 0 To idl1 - 1
                        Dim idx5 = out_off + ik
                        Dim idx6 = in_off + ik
                        outp(idx5 + idx1) += ar2 * inp(idx6 + idx3)
                        outp(idx5 + idx2) += ai2 * inp(idx6 + idx4)
                    Next
                Next
            Next
            For j = 1 To ipph - 1
                Dim idx1 = j * idl1
                For ik = 0 To idl1 - 1
                    outp(out_off + ik) += inp(in_off + ik + idx1)
                Next
            Next

            If ido >= l1 Then
                For k = 0 To l1 - 1
                    Dim idx1 = k * ido
                    Dim idx2 = idx1 * ip
                    For i = 0 To ido - 1
                        inp(in_off + i + idx2) = outp(out_off + i + idx1)
                    Next
                Next
            Else
                For i = 0 To ido - 1
                    For k = 0 To l1 - 1
                        Dim idx1 = k * ido
                        inp(in_off + i + idx1 * ip) = outp(out_off + i + idx1)
                    Next
                Next
            End If
            Dim idx01 = ip * ido
            For j = 1 To ipph - 1
                jc = ip - j
                j2 = 2 * j
                Dim idx1 = j * l1 * ido
                Dim idx2 = jc * l1 * ido
                Dim idx3 = j2 * ido
                For k = 0 To l1 - 1
                    Dim idx4 = k * ido
                    Dim idx5 = idx4 + idx1
                    Dim idx6 = idx4 + idx2
                    Dim idx7 = k * idx01
                    inp(in_off + ido - 1 + idx3 - ido + idx7) = outp(out_off + idx5)
                    inp(in_off + idx3 + idx7) = outp(out_off + idx6)
                Next
            Next
            If ido = 1 Then Return
            If nbd >= l1 Then
                For j = 1 To ipph - 1
                    jc = ip - j
                    j2 = 2 * j
                    Dim idx1 = j * l1 * ido
                    Dim idx2 = jc * l1 * ido
                    Dim idx3 = j2 * ido
                    For k = 0 To l1 - 1
                        Dim idx4 = k * idx01
                        Dim idx5 = k * ido
                        For i = 2 To ido - 1 Step 2
                            ic = ido - i
                            Dim idx6 = in_off + i
                            Dim idx7 = in_off + ic
                            Dim idx8 = out_off + i
                            Dim iidx1 = idx6 + idx3 + idx4
                            Dim iidx2 = idx7 + idx3 - ido + idx4
                            Dim oidx1 = idx8 + idx5 + idx1
                            Dim oidx2 = idx8 + idx5 + idx2
                            Dim o1i = outp(oidx1 - 1)
                            Dim o1r = outp(oidx1)
                            Dim o2i = outp(oidx2 - 1)
                            Dim o2r = outp(oidx2)

                            inp(iidx1 - 1) = o1i + o2i
                            inp(iidx2 - 1) = o1i - o2i
                            inp(iidx1) = o1r + o2r
                            inp(iidx2) = o2r - o1r
                        Next
                    Next
                Next
            Else
                For j = 1 To ipph - 1
                    jc = ip - j
                    j2 = 2 * j
                    Dim idx1 = j * l1 * ido
                    Dim idx2 = jc * l1 * ido
                    Dim idx3 = j2 * ido
                    For i = 2 To ido - 1 Step 2
                        ic = ido - i
                        Dim idx6 = in_off + i
                        Dim idx7 = in_off + ic
                        Dim idx8 = out_off + i
                        For k = 0 To l1 - 1
                            Dim idx4 = k * idx01
                            Dim idx5 = k * ido
                            Dim iidx1 = idx6 + idx3 + idx4
                            Dim iidx2 = idx7 + idx3 - ido + idx4
                            Dim oidx1 = idx8 + idx5 + idx1
                            Dim oidx2 = idx8 + idx5 + idx2
                            Dim o1i = outp(oidx1 - 1)
                            Dim o1r = outp(oidx1)
                            Dim o2i = outp(oidx2 - 1)
                            Dim o2r = outp(oidx2)

                            inp(iidx1 - 1) = o1i + o2i
                            inp(iidx2 - 1) = o1i - o2i
                            inp(iidx1) = o1r + o2r
                            inp(iidx2) = o2r - o1r
                        Next
                    Next
                Next
            End If
        End Sub

        ' ---------------------------------------------------------
        ' radbg: Real FFT 's backward processing of general factor
        ' --------------------------------------------------------
        Private Sub radbg(ido As Integer, ip As Integer, l1 As Integer, idl1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer)
            Dim idij, ipph, j2, ic, jc, lc, ies As Integer
            Dim dc2, ai1, ai2, ar1, ar2, ds2, w1r, w1i As Double
            Dim nbd As Integer
            Dim dcp, arg, dsp, ar1h, ar2h As Double
            Dim iw1 = offset

            arg = TWO_PI / ip
            dcp = System.Math.Cos(arg)
            dsp = System.Math.Sin(arg)
            nbd = (ido - 1) / 2
            ipph = (ip + 1) / 2
            Dim idx0 = ip * ido
            If ido >= l1 Then
                For k = 0 To l1 - 1
                    Dim idx1 = k * ido
                    Dim idx2 = k * idx0
                    For i = 0 To ido - 1
                        outp(out_off + i + idx1) = inp(in_off + i + idx2)
                    Next
                Next
            Else
                For i = 0 To ido - 1
                    Dim idx1 = out_off + i
                    Dim idx2 = in_off + i
                    For k = 0 To l1 - 1
                        outp(idx1 + k * ido) = inp(idx2 + k * idx0)
                    Next
                Next
            End If
            Dim iidx0 = in_off + ido - 1
            For j = 1 To ipph - 1
                jc = ip - j
                j2 = 2 * j
                Dim idx1 = j * l1 * ido
                Dim idx2 = jc * l1 * ido
                Dim idx3 = j2 * ido
                For k = 0 To l1 - 1
                    Dim idx4 = k * ido
                    Dim idx5 = idx4 * ip
                    Dim iidx1 = iidx0 + idx3 + idx5 - ido
                    Dim iidx2 = in_off + idx3 + idx5
                    Dim i1r = inp(iidx1)
                    Dim i2r = inp(iidx2)

                    outp(out_off + idx4 + idx1) = i1r + i1r
                    outp(out_off + idx4 + idx2) = i2r + i2r
                Next
            Next

            If ido <> 1 Then
                If nbd >= l1 Then
                    For j = 1 To ipph - 1
                        jc = ip - j
                        Dim idx1 = j * l1 * ido
                        Dim idx2 = jc * l1 * ido
                        Dim idx3 = 2 * j * ido
                        For k = 0 To l1 - 1
                            Dim idx4 = k * ido + idx1
                            Dim idx5 = k * ido + idx2
                            Dim idx6 = k * ip * ido + idx3
                            For i = 2 To ido - 1 Step 2
                                ic = ido - i
                                Dim idx7 = out_off + i
                                Dim idx8 = in_off + ic
                                Dim idx9 = in_off + i
                                Dim oidx1 = idx7 + idx4
                                Dim oidx2 = idx7 + idx5
                                Dim iidx1 = idx9 + idx6
                                Dim iidx2 = idx8 + idx6 - ido
                                Dim a1i = inp(iidx1 - 1)
                                Dim a1r = inp(iidx1)
                                Dim a2i = inp(iidx2 - 1)
                                Dim a2r = inp(iidx2)

                                outp(oidx1 - 1) = a1i + a2i
                                outp(oidx2 - 1) = a1i - a2i
                                outp(oidx1) = a1r - a2r
                                outp(oidx2) = a1r + a2r
                            Next
                        Next
                    Next
                Else
                    For j = 1 To ipph - 1
                        jc = ip - j
                        Dim idx1 = j * l1 * ido
                        Dim idx2 = jc * l1 * ido
                        Dim idx3 = 2 * j * ido
                        For i = 2 To ido - 1 Step 2
                            ic = ido - i
                            Dim idx7 = out_off + i
                            Dim idx8 = in_off + ic
                            Dim idx9 = in_off + i
                            For k = 0 To l1 - 1
                                Dim idx4 = k * ido + idx1
                                Dim idx5 = k * ido + idx2
                                Dim idx6 = k * ip * ido + idx3
                                Dim oidx1 = idx7 + idx4
                                Dim oidx2 = idx7 + idx5
                                Dim iidx1 = idx9 + idx6
                                Dim iidx2 = idx8 + idx6 - ido
                                Dim a1i = inp(iidx1 - 1)
                                Dim a1r = inp(iidx1)
                                Dim a2i = inp(iidx2 - 1)
                                Dim a2r = inp(iidx2)

                                outp(oidx1 - 1) = a1i + a2i
                                outp(oidx2 - 1) = a1i - a2i
                                outp(oidx1) = a1r - a2r
                                outp(oidx2) = a1r + a2r
                            Next
                        Next
                    Next
                End If
            End If

            ar1 = 1
            ai1 = 0
            Dim idx01 = (ip - 1) * idl1
            For l = 1 To ipph - 1
                lc = ip - l
                ar1h = dcp * ar1 - dsp * ai1
                ai1 = dcp * ai1 + dsp * ar1
                ar1 = ar1h
                Dim idx1 = l * idl1
                Dim idx2 = lc * idl1
                For ik = 0 To idl1 - 1
                    Dim idx3 = in_off + ik
                    Dim idx4 = out_off + ik
                    inp(idx3 + idx1) = outp(idx4) + ar1 * outp(idx4 + idl1)
                    inp(idx3 + idx2) = ai1 * outp(idx4 + idx01)
                Next
                dc2 = ar1
                ds2 = ai1
                ar2 = ar1
                ai2 = ai1
                For j = 2 To ipph - 1
                    jc = ip - j
                    ar2h = dc2 * ar2 - ds2 * ai2
                    ai2 = dc2 * ai2 + ds2 * ar2
                    ar2 = ar2h
                    Dim idx5 = j * idl1
                    Dim idx6 = jc * idl1
                    For ik = 0 To idl1 - 1
                        Dim idx7 = in_off + ik
                        Dim idx8 = out_off + ik
                        inp(idx7 + idx1) += ar2 * outp(idx8 + idx5)
                        inp(idx7 + idx2) += ai2 * outp(idx8 + idx6)
                    Next
                Next
            Next
            For j = 1 To ipph - 1
                Dim idx1 = j * idl1
                For ik = 0 To idl1 - 1
                    Dim idx2 = out_off + ik
                    outp(idx2) += outp(idx2 + idx1)
                Next
            Next
            For j = 1 To ipph - 1
                jc = ip - j
                Dim idx1 = j * l1 * ido
                Dim idx2 = jc * l1 * ido
                For k = 0 To l1 - 1
                    Dim idx3 = k * ido
                    Dim oidx1 = out_off + idx3
                    Dim iidx1 = in_off + idx3 + idx1
                    Dim iidx2 = in_off + idx3 + idx2
                    Dim i1r = inp(iidx1)
                    Dim i2r = inp(iidx2)

                    outp(oidx1 + idx1) = i1r - i2r
                    outp(oidx1 + idx2) = i1r + i2r
                Next
            Next

            If ido = 1 Then Return
            If nbd >= l1 Then
                For j = 1 To ipph - 1
                    jc = ip - j
                    Dim idx1 = j * l1 * ido
                    Dim idx2 = jc * l1 * ido
                    For k = 0 To l1 - 1
                        Dim idx3 = k * ido
                        For i = 2 To ido - 1 Step 2
                            Dim idx4 = out_off + i
                            Dim idx5 = in_off + i
                            Dim oidx1 = idx4 + idx3 + idx1
                            Dim oidx2 = idx4 + idx3 + idx2
                            Dim iidx1 = idx5 + idx3 + idx1
                            Dim iidx2 = idx5 + idx3 + idx2
                            Dim i1i = inp(iidx1 - 1)
                            Dim i1r = inp(iidx1)
                            Dim i2i = inp(iidx2 - 1)
                            Dim i2r = inp(iidx2)

                            outp(oidx1 - 1) = i1i - i2r
                            outp(oidx2 - 1) = i1i + i2r
                            outp(oidx1) = i1r + i2i
                            outp(oidx2) = i1r - i2i
                        Next
                    Next
                Next
            Else
                For j = 1 To ipph - 1
                    jc = ip - j
                    Dim idx1 = j * l1 * ido
                    Dim idx2 = jc * l1 * ido
                    For i = 2 To ido - 1 Step 2
                        Dim idx4 = out_off + i
                        Dim idx5 = in_off + i
                        For k = 0 To l1 - 1
                            Dim idx3 = k * ido
                            Dim oidx1 = idx4 + idx3 + idx1
                            Dim oidx2 = idx4 + idx3 + idx2
                            Dim iidx1 = idx5 + idx3 + idx1
                            Dim iidx2 = idx5 + idx3 + idx2
                            Dim i1i = inp(iidx1 - 1)
                            Dim i1r = inp(iidx1)
                            Dim i2i = inp(iidx2 - 1)
                            Dim i2r = inp(iidx2)

                            outp(oidx1 - 1) = i1i - i2r
                            outp(oidx2 - 1) = i1i + i2r
                            outp(oidx1) = i1r + i2i
                            outp(oidx2) = i1r - i2i
                        Next
                    Next
                Next
            End If
            Array.Copy(outp, out_off, inp, in_off, idl1)
            For j = 1 To ip - 1
                Dim idx1 = j * l1 * ido
                For k = 0 To l1 - 1
                    Dim idx2 = k * ido + idx1
                    inp(in_off + idx2) = outp(out_off + idx2)
                Next
            Next
            If nbd <= l1 Then
                ies = -ido
                For j = 1 To ip - 1
                    ies += ido
                    idij = ies - 1
                    Dim idx1 = j * l1 * ido
                    For i = 2 To ido - 1 Step 2
                        idij += 2
                        Dim idx2 = idij + iw1
                        w1r = wtable_r(idx2 - 1)
                        w1i = wtable_r(idx2)
                        Dim idx4 = in_off + i
                        Dim idx5 = out_off + i
                        For k = 0 To l1 - 1
                            Dim idx3 = k * ido + idx1
                            Dim iidx1 = idx4 + idx3
                            Dim oidx1 = idx5 + idx3
                            Dim o1i = outp(oidx1 - 1)
                            Dim o1r = outp(oidx1)

                            inp(iidx1 - 1) = w1r * o1i - w1i * o1r
                            inp(iidx1) = w1r * o1r + w1i * o1i
                        Next
                    Next
                Next
            Else
                ies = -ido
                For j = 1 To ip - 1
                    ies += ido
                    Dim idx1 = j * l1 * ido
                    For k = 0 To l1 - 1
                        idij = ies - 1
                        Dim idx3 = k * ido + idx1
                        For i = 2 To ido - 1 Step 2
                            idij += 2
                            Dim idx2 = idij + iw1
                            w1r = wtable_r(idx2 - 1)
                            w1i = wtable_r(idx2)
                            Dim idx4 = in_off + i
                            Dim idx5 = out_off + i
                            Dim iidx1 = idx4 + idx3
                            Dim oidx1 = idx5 + idx3
                            Dim o1i = outp(oidx1 - 1)
                            Dim o1r = outp(oidx1)

                            inp(iidx1 - 1) = w1r * o1i - w1i * o1r
                            inp(iidx1) = w1r * o1r + w1i * o1i

                        Next
                    Next
                Next
            End If
        End Sub

        ' ---------------------------------------------------------
        ' cfftf1: further processing Of Complex forward FFT
        ' --------------------------------------------------------
        Private Sub cfftf(a As Double(), offa As Integer, isign As Integer)
            Dim idot As Integer
            Dim l1, l2 As Integer
            Dim na, nf, ip, iw, ido, idl1 As Integer
            Dim nac = New Integer(0) {}
            Dim twon = 2 * n

            Dim iw1, iw2 As Integer
            Dim ch = New Double(twon - 1) {}

            iw1 = twon
            iw2 = 4 * n
            nac(0) = 0
            nf = CInt(wtable(1 + iw2))
            na = 0
            l1 = 1
            iw = iw1
            For k1 = 2 To nf + 1
                ip = CInt(wtable(k1 + iw2))
                l2 = ip * l1
                ido = n / l2
                idot = ido + ido
                idl1 = idot * l1
                Select Case ip
                    Case 4
                        If na = 0 Then
                            passf4(idot, l1, a, offa, ch, 0, iw, isign)
                        Else
                            passf4(idot, l1, ch, 0, a, offa, iw, isign)
                        End If
                        na = 1 - na
                    Case 2
                        If na = 0 Then
                            passf2(idot, l1, a, offa, ch, 0, iw, isign)
                        Else
                            passf2(idot, l1, ch, 0, a, offa, iw, isign)
                        End If
                        na = 1 - na
                    Case 3
                        If na = 0 Then
                            passf3(idot, l1, a, offa, ch, 0, iw, isign)
                        Else
                            passf3(idot, l1, ch, 0, a, offa, iw, isign)
                        End If
                        na = 1 - na
                    Case 5
                        If na = 0 Then
                            passf5(idot, l1, a, offa, ch, 0, iw, isign)
                        Else
                            passf5(idot, l1, ch, 0, a, offa, iw, isign)
                        End If
                        na = 1 - na
                    Case Else
                        If na = 0 Then
                            passfg(nac, idot, ip, l1, idl1, a, offa, ch, 0, iw, isign)
                        Else
                            passfg(nac, idot, ip, l1, idl1, ch, 0, a, offa, iw, isign)
                        End If
                        If nac(0) <> 0 Then na = 1 - na
                End Select
                l1 = l2
                iw += (ip - 1) * idot
            Next
            If na = 0 Then Return
            Array.Copy(ch, 0, a, offa, twon)

        End Sub

        ' ----------------------------------------------------------------------
        ' passf2: Complex FFT 's forward/backward processing of factor 2;
        ' isign Is +1 for backward And -1 for forward transforms
        ' ----------------------------------------------------------------------

        Private Sub passf2(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer, isign As Integer)
            Dim t1i, t1r As Double
            Dim iw1 As Integer
            iw1 = offset
            Dim idx = ido * l1
            If ido <= 2 Then
                For k = 0 To l1 - 1
                    Dim idx0 = k * ido
                    Dim iidx1 = in_off + 2 * idx0
                    Dim iidx2 = iidx1 + ido
                    Dim a1r = inp(iidx1)
                    Dim a1i = inp(iidx1 + 1)
                    Dim a2r = inp(iidx2)
                    Dim a2i = inp(iidx2 + 1)

                    Dim oidx1 = out_off + idx0
                    Dim oidx2 = oidx1 + idx
                    outp(oidx1) = a1r + a2r
                    outp(oidx1 + 1) = a1i + a2i
                    outp(oidx2) = a1r - a2r
                    outp(oidx2 + 1) = a1i - a2i
                Next
            Else
                For k = 0 To l1 - 1
                    For i = 0 To ido - 1 - 1 Step 2
                        Dim idx0 = k * ido
                        Dim iidx1 = in_off + i + 2 * idx0
                        Dim iidx2 = iidx1 + ido
                        Dim i1r = inp(iidx1)
                        Dim i1i = inp(iidx1 + 1)
                        Dim i2r = inp(iidx2)
                        Dim i2i = inp(iidx2 + 1)

                        Dim widx1 = i + iw1
                        Dim w1r = wtable(widx1)
                        Dim w1i = isign * wtable(widx1 + 1)

                        t1r = i1r - i2r
                        t1i = i1i - i2i

                        Dim oidx1 = out_off + i + idx0
                        Dim oidx2 = oidx1 + idx
                        outp(oidx1) = i1r + i2r
                        outp(oidx1 + 1) = i1i + i2i
                        outp(oidx2) = w1r * t1r - w1i * t1i
                        outp(oidx2 + 1) = w1r * t1i + w1i * t1r
                    Next
                Next
            End If
        End Sub

        ' ----------------------------------------------------------------------
        ' passf3: Complex FFT 's forward/backward processing of factor 3;
        ' isign Is +1 for backward And -1 for forward transforms
        ' ----------------------------------------------------------------------
        Private Sub passf3(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer, isign As Integer)
            Dim taur = -0.5
            Dim taui = 0.86602540378443871
            Dim ci2, ci3, di2, di3, cr2, cr3, dr2, dr3, ti2, tr2 As Double
            Dim iw1, iw2 As Integer

            iw1 = offset
            iw2 = iw1 + ido

            Dim idxt = l1 * ido

            If ido = 2 Then
                For k = 1 To l1
                    Dim iidx1 = in_off + (3 * k - 2) * ido
                    Dim iidx2 = iidx1 + ido
                    Dim iidx3 = iidx1 - ido
                    Dim i1r = inp(iidx1)
                    Dim i1i = inp(iidx1 + 1)
                    Dim i2r = inp(iidx2)
                    Dim i2i = inp(iidx2 + 1)
                    Dim i3r = inp(iidx3)
                    Dim i3i = inp(iidx3 + 1)

                    tr2 = i1r + i2r
                    cr2 = i3r + taur * tr2
                    ti2 = i1i + i2i
                    ci2 = i3i + taur * ti2
                    cr3 = isign * taui * (i1r - i2r)
                    ci3 = isign * taui * (i1i - i2i)

                    Dim oidx1 = out_off + (k - 1) * ido
                    Dim oidx2 = oidx1 + idxt
                    Dim oidx3 = oidx2 + idxt
                    outp(oidx1) = inp(iidx3) + tr2
                    outp(oidx1 + 1) = i3i + ti2
                    outp(oidx2) = cr2 - ci3
                    outp(oidx2 + 1) = ci2 + cr3
                    outp(oidx3) = cr2 + ci3
                    outp(oidx3 + 1) = ci2 - cr3
                Next
            Else
                For k = 1 To l1
                    Dim idx1 = in_off + (3 * k - 2) * ido
                    Dim idx2 = out_off + (k - 1) * ido
                    For i = 0 To ido - 1 - 1 Step 2
                        Dim iidx1 = i + idx1
                        Dim iidx2 = iidx1 + ido
                        Dim iidx3 = iidx1 - ido
                        Dim a1r = inp(iidx1)
                        Dim a1i = inp(iidx1 + 1)
                        Dim a2r = inp(iidx2)
                        Dim a2i = inp(iidx2 + 1)
                        Dim a3r = inp(iidx3)
                        Dim a3i = inp(iidx3 + 1)

                        tr2 = a1r + a2r
                        cr2 = a3r + taur * tr2
                        ti2 = a1i + a2i
                        ci2 = a3i + taur * ti2
                        cr3 = isign * taui * (a1r - a2r)
                        ci3 = isign * taui * (a1i - a2i)
                        dr2 = cr2 - ci3
                        dr3 = cr2 + ci3
                        di2 = ci2 + cr3
                        di3 = ci2 - cr3

                        Dim widx1 = i + iw1
                        Dim widx2 = i + iw2
                        Dim w1r = wtable(widx1)
                        Dim w1i = isign * wtable(widx1 + 1)
                        Dim w2r = wtable(widx2)
                        Dim w2i = isign * wtable(widx2 + 1)

                        Dim oidx1 = i + idx2
                        Dim oidx2 = oidx1 + idxt
                        Dim oidx3 = oidx2 + idxt
                        outp(oidx1) = a3r + tr2
                        outp(oidx1 + 1) = a3i + ti2
                        outp(oidx2) = w1r * dr2 - w1i * di2
                        outp(oidx2 + 1) = w1r * di2 + w1i * dr2
                        outp(oidx3) = w2r * dr3 - w2i * di3
                        outp(oidx3 + 1) = w2r * di3 + w2i * dr3
                    Next
                Next
            End If
        End Sub

        ' ----------------------------------------------------------------------
        ' passf4: Complex FFT 's forward/backward processing of factor 4;
        ' isign Is +1 for backward And -1 for forward transforms
        ' ----------------------------------------------------------------------
        Private Sub passf4(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer, isign As Integer)
            Dim ci2, ci3, ci4, cr2, cr3, cr4, ti1, ti2, ti3, ti4, tr1, tr2, tr3, tr4 As Double
            Dim iw1, iw2, iw3 As Integer
            iw1 = offset
            iw2 = iw1 + ido
            iw3 = iw2 + ido

            Dim idx0 = l1 * ido
            If ido = 2 Then
                For k = 0 To l1 - 1
                    Dim idxt1 = k * ido
                    Dim iidx1 = in_off + 4 * idxt1 + 1
                    Dim iidx2 = iidx1 + ido
                    Dim iidx3 = iidx2 + ido
                    Dim iidx4 = iidx3 + ido

                    Dim i1i = inp(iidx1 - 1)
                    Dim i1r = inp(iidx1)
                    Dim i2i = inp(iidx2 - 1)
                    Dim i2r = inp(iidx2)
                    Dim i3i = inp(iidx3 - 1)
                    Dim i3r = inp(iidx3)
                    Dim i4i = inp(iidx4 - 1)
                    Dim i4r = inp(iidx4)

                    ti1 = i1r - i3r
                    ti2 = i1r + i3r
                    tr4 = i4r - i2r
                    ti3 = i2r + i4r
                    tr1 = i1i - i3i
                    tr2 = i1i + i3i
                    ti4 = i2i - i4i
                    tr3 = i2i + i4i

                    Dim oidx1 = out_off + idxt1
                    Dim oidx2 = oidx1 + idx0
                    Dim oidx3 = oidx2 + idx0
                    Dim oidx4 = oidx3 + idx0
                    outp(oidx1) = tr2 + tr3
                    outp(oidx1 + 1) = ti2 + ti3
                    outp(oidx2) = tr1 + isign * tr4
                    outp(oidx2 + 1) = ti1 + isign * ti4
                    outp(oidx3) = tr2 - tr3
                    outp(oidx3 + 1) = ti2 - ti3
                    outp(oidx4) = tr1 - isign * tr4
                    outp(oidx4 + 1) = ti1 - isign * ti4
                Next
            Else
                For k = 0 To l1 - 1
                    Dim idx1 = k * ido
                    Dim idx2 = in_off + 1 + 4 * idx1
                    For i = 0 To ido - 1 - 1 Step 2
                        Dim iidx1 = i + idx2
                        Dim iidx2 = iidx1 + ido
                        Dim iidx3 = iidx2 + ido
                        Dim iidx4 = iidx3 + ido
                        Dim i1i = inp(iidx1 - 1)
                        Dim i1r = inp(iidx1)
                        Dim i2i = inp(iidx2 - 1)
                        Dim i2r = inp(iidx2)
                        Dim i3i = inp(iidx3 - 1)
                        Dim i3r = inp(iidx3)
                        Dim i4i = inp(iidx4 - 1)
                        Dim i4r = inp(iidx4)

                        ti1 = i1r - i3r
                        ti2 = i1r + i3r
                        ti3 = i2r + i4r
                        tr4 = i4r - i2r
                        tr1 = i1i - i3i
                        tr2 = i1i + i3i
                        ti4 = i2i - i4i
                        tr3 = i2i + i4i
                        cr3 = tr2 - tr3
                        ci3 = ti2 - ti3
                        cr2 = tr1 + isign * tr4
                        cr4 = tr1 - isign * tr4
                        ci2 = ti1 + isign * ti4
                        ci4 = ti1 - isign * ti4

                        Dim widx1 = i + iw1
                        Dim widx2 = i + iw2
                        Dim widx3 = i + iw3
                        Dim w1r = wtable(widx1)
                        Dim w1i = isign * wtable(widx1 + 1)
                        Dim w2r = wtable(widx2)
                        Dim w2i = isign * wtable(widx2 + 1)
                        Dim w3r = wtable(widx3)
                        Dim w3i = isign * wtable(widx3 + 1)

                        Dim oidx1 = out_off + i + idx1
                        Dim oidx2 = oidx1 + idx0
                        Dim oidx3 = oidx2 + idx0
                        Dim oidx4 = oidx3 + idx0
                        outp(oidx1) = tr2 + tr3
                        outp(oidx1 + 1) = ti2 + ti3
                        outp(oidx2) = w1r * cr2 - w1i * ci2
                        outp(oidx2 + 1) = w1r * ci2 + w1i * cr2
                        outp(oidx3) = w2r * cr3 - w2i * ci3
                        outp(oidx3 + 1) = w2r * ci3 + w2i * cr3
                        outp(oidx4) = w3r * cr4 - w3i * ci4
                        outp(oidx4 + 1) = w3r * ci4 + w3i * cr4
                    Next
                Next
            End If
        End Sub

        ' ----------------------------------------------------------------------
        ' passf5: Complex FFT 's forward/backward processing of factor 5;
        ' isign Is +1 for backward And -1 for forward transforms
        ' ----------------------------------------------------------------------
        ' isign==-1 for forward transform and+1 for backward transform 
        Private Sub passf5(ido As Integer, l1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer, isign As Integer)
            Dim tr11 = 0.30901699437494745
            Dim ti11 = 0.95105651629515353
            Dim tr12 = -0.80901699437494734
            Dim ti12 = 0.58778525229247325
            Dim ci2, ci3, ci4, ci5, di3, di4, di5, di2, cr2, cr3, cr5, cr4, ti2, ti3, ti4, ti5, dr3, dr4, dr5, dr2, tr2, tr3, tr4, tr5 As Double
            Dim iw1, iw2, iw3, iw4 As Integer

            iw1 = offset
            iw2 = iw1 + ido
            iw3 = iw2 + ido
            iw4 = iw3 + ido

            Dim idx0 = l1 * ido

            If ido = 2 Then
                Dim k = 1

                While k <= l1
                    Dim iidx1 = in_off + (5 * k - 4) * ido + 1
                    Dim iidx2 = iidx1 + ido
                    Dim iidx3 = iidx1 - ido
                    Dim iidx4 = iidx2 + ido
                    Dim iidx5 = iidx4 + ido

                    Dim i1i = inp(iidx1 - 1)
                    Dim i1r = inp(iidx1)
                    Dim i2i = inp(iidx2 - 1)
                    Dim i2r = inp(iidx2)
                    Dim i3i = inp(iidx3 - 1)
                    Dim i3r = inp(iidx3)
                    Dim i4i = inp(iidx4 - 1)
                    Dim i4r = inp(iidx4)
                    Dim i5i = inp(iidx5 - 1)
                    Dim i5r = inp(iidx5)

                    ti5 = i1r - i5r
                    ti2 = i1r + i5r
                    ti4 = i2r - i4r
                    ti3 = i2r + i4r
                    tr5 = i1i - i5i
                    tr2 = i1i + i5i
                    tr4 = i2i - i4i
                    tr3 = i2i + i4i
                    cr2 = i3i + tr11 * tr2 + tr12 * tr3
                    ci2 = i3r + tr11 * ti2 + tr12 * ti3
                    cr3 = i3i + tr12 * tr2 + tr11 * tr3
                    ci3 = i3r + tr12 * ti2 + tr11 * ti3
                    cr5 = isign * (ti11 * tr5 + ti12 * tr4)
                    ci5 = isign * (ti11 * ti5 + ti12 * ti4)
                    cr4 = isign * (ti12 * tr5 - ti11 * tr4)
                    ci4 = isign * (ti12 * ti5 - ti11 * ti4)

                    Dim oidx1 = out_off + (k - 1) * ido
                    Dim oidx2 = oidx1 + idx0
                    Dim oidx3 = oidx2 + idx0
                    Dim oidx4 = oidx3 + idx0
                    Dim oidx5 = oidx4 + idx0
                    outp(oidx1) = i3i + tr2 + tr3
                    outp(oidx1 + 1) = i3r + ti2 + ti3
                    outp(oidx2) = cr2 - ci5
                    outp(oidx2 + 1) = ci2 + cr5
                    outp(oidx3) = cr3 - ci4
                    outp(oidx3 + 1) = ci3 + cr4
                    outp(oidx4) = cr3 + ci4
                    outp(oidx4 + 1) = ci3 - cr4
                    outp(oidx5) = cr2 + ci5
                    outp(oidx5 + 1) = ci2 - cr5
                    Threading.Interlocked.Increment(k)
                End While
            Else
                For k = 1 To l1
                    Dim idx1 = in_off + 1 + (k * 5 - 4) * ido
                    Dim idx2 = out_off + (k - 1) * ido
                    For i = 0 To ido - 1 - 1 Step 2
                        Dim iidx1 = i + idx1
                        Dim iidx2 = iidx1 + ido
                        Dim iidx3 = iidx1 - ido
                        Dim iidx4 = iidx2 + ido
                        Dim iidx5 = iidx4 + ido
                        Dim i1i = inp(iidx1 - 1)
                        Dim i1r = inp(iidx1)
                        Dim i2i = inp(iidx2 - 1)
                        Dim i2r = inp(iidx2)
                        Dim i3i = inp(iidx3 - 1)
                        Dim i3r = inp(iidx3)
                        Dim i4i = inp(iidx4 - 1)
                        Dim i4r = inp(iidx4)
                        Dim i5i = inp(iidx5 - 1)
                        Dim i5r = inp(iidx5)

                        ti5 = i1r - i5r
                        ti2 = i1r + i5r
                        ti4 = i2r - i4r
                        ti3 = i2r + i4r
                        tr5 = i1i - i5i
                        tr2 = i1i + i5i
                        tr4 = i2i - i4i
                        tr3 = i2i + i4i
                        cr2 = i3i + tr11 * tr2 + tr12 * tr3
                        ci2 = i3r + tr11 * ti2 + tr12 * ti3
                        cr3 = i3i + tr12 * tr2 + tr11 * tr3
                        ci3 = i3r + tr12 * ti2 + tr11 * ti3
                        cr5 = isign * (ti11 * tr5 + ti12 * tr4)
                        ci5 = isign * (ti11 * ti5 + ti12 * ti4)
                        cr4 = isign * (ti12 * tr5 - ti11 * tr4)
                        ci4 = isign * (ti12 * ti5 - ti11 * ti4)
                        dr3 = cr3 - ci4
                        dr4 = cr3 + ci4
                        di3 = ci3 + cr4
                        di4 = ci3 - cr4
                        dr5 = cr2 + ci5
                        dr2 = cr2 - ci5
                        di5 = ci2 - cr5
                        di2 = ci2 + cr5

                        Dim widx1 = i + iw1
                        Dim widx2 = i + iw2
                        Dim widx3 = i + iw3
                        Dim widx4 = i + iw4
                        Dim w1r = wtable(widx1)
                        Dim w1i = isign * wtable(widx1 + 1)
                        Dim w2r = wtable(widx2)
                        Dim w2i = isign * wtable(widx2 + 1)
                        Dim w3r = wtable(widx3)
                        Dim w3i = isign * wtable(widx3 + 1)
                        Dim w4r = wtable(widx4)
                        Dim w4i = isign * wtable(widx4 + 1)

                        Dim oidx1 = i + idx2
                        Dim oidx2 = oidx1 + idx0
                        Dim oidx3 = oidx2 + idx0
                        Dim oidx4 = oidx3 + idx0
                        Dim oidx5 = oidx4 + idx0
                        outp(oidx1) = i3i + tr2 + tr3
                        outp(oidx1 + 1) = i3r + ti2 + ti3
                        outp(oidx2) = w1r * dr2 - w1i * di2
                        outp(oidx2 + 1) = w1r * di2 + w1i * dr2
                        outp(oidx3) = w2r * dr3 - w2i * di3
                        outp(oidx3 + 1) = w2r * di3 + w2i * dr3
                        outp(oidx4) = w3r * dr4 - w3i * di4
                        outp(oidx4 + 1) = w3r * di4 + w3i * dr4
                        outp(oidx5) = w4r * dr5 - w4i * di5
                        outp(oidx5 + 1) = w4r * di5 + w4i * dr5
                    Next
                Next
            End If
        End Sub

        ' ----------------------------------------------------------------------
        ' passfg: Complex FFT 's forward/backward processing of general factor;
        ' isign Is +1 for backward And -1 for forward transforms
        ' ----------------------------------------------------------------------
        Private Sub passfg(nac As Integer(), ido As Integer, ip As Integer, l1 As Integer, idl1 As Integer, inp As Double(), in_off As Integer, outp As Double(), out_off As Integer, offset As Integer, isign As Integer)
            Dim idij, idlj, idot, ipph, l, jc, lc, idj, idl, inc, idp As Integer
            Dim w1r, w1i, w2i, w2r As Double
            Dim iw1 As Integer

            iw1 = offset
            idot = ido / 2
            ipph = (ip + 1) / 2
            idp = ip * ido
            If ido >= l1 Then
                For j = 1 To ipph - 1
                    jc = ip - j
                    Dim idx1 = j * ido
                    Dim idx2 = jc * ido
                    For k = 0 To l1 - 1
                        Dim idx3 = k * ido
                        Dim idx4 = idx3 + idx1 * l1
                        Dim idx5 = idx3 + idx2 * l1
                        Dim idx6 = idx3 * ip
                        For i = 0 To ido - 1
                            Dim oidx1 = out_off + i
                            Dim i1r = inp(in_off + i + idx1 + idx6)
                            Dim i2r = inp(in_off + i + idx2 + idx6)
                            outp(oidx1 + idx4) = i1r + i2r
                            outp(oidx1 + idx5) = i1r - i2r
                        Next
                    Next
                Next
                For k = 0 To l1 - 1
                    Dim idxt1 = k * ido
                    Dim idxt2 = idxt1 * ip
                    For i = 0 To ido - 1
                        outp(out_off + i + idxt1) = inp(in_off + i + idxt2)
                    Next
                Next
            Else
                For j = 1 To ipph - 1
                    jc = ip - j
                    Dim idxt1 = j * l1 * ido
                    Dim idxt2 = jc * l1 * ido
                    Dim idxt3 = j * ido
                    Dim idxt4 = jc * ido
                    For i = 0 To ido - 1
                        For k = 0 To l1 - 1
                            Dim idx1 = k * ido
                            Dim idx2 = idx1 * ip
                            Dim idx3 = out_off + i
                            Dim idx4 = in_off + i
                            Dim i1r = inp(idx4 + idxt3 + idx2)
                            Dim i2r = inp(idx4 + idxt4 + idx2)
                            outp(idx3 + idx1 + idxt1) = i1r + i2r
                            outp(idx3 + idx1 + idxt2) = i1r - i2r
                        Next
                    Next
                Next
                For i = 0 To ido - 1
                    For k = 0 To l1 - 1
                        Dim idx1 = k * ido
                        outp(out_off + i + idx1) = inp(in_off + i + idx1 * ip)
                    Next
                Next
            End If

            idl = 2 - ido
            inc = 0
            Dim idxt0 = (ip - 1) * idl1
            For l = 1 To ipph - 1
                lc = ip - l
                idl += ido
                Dim idxt1 = l * idl1
                Dim idxt2 = lc * idl1
                Dim idxt3 = idl + iw1
                w1r = wtable(idxt3 - 2)
                w1i = isign * wtable(idxt3 - 1)
                For ik = 0 To idl1 - 1
                    Dim idx1 = in_off + ik
                    Dim idx2 = out_off + ik
                    inp(idx1 + idxt1) = outp(idx2) + w1r * outp(idx2 + idl1)
                    inp(idx1 + idxt2) = w1i * outp(idx2 + idxt0)
                Next
                idlj = idl
                inc += ido
                For j = 2 To ipph - 1
                    jc = ip - j
                    idlj += inc
                    If idlj > idp Then idlj -= idp
                    Dim idxt4 = idlj + iw1
                    w2r = wtable(idxt4 - 2)
                    w2i = isign * wtable(idxt4 - 1)
                    Dim idxt5 = j * idl1
                    Dim idxt6 = jc * idl1
                    For ik = 0 To idl1 - 1
                        Dim idx1 = in_off + ik
                        Dim idx2 = out_off + ik
                        inp(idx1 + idxt1) += w2r * outp(idx2 + idxt5)
                        inp(idx1 + idxt2) += w2i * outp(idx2 + idxt6)
                    Next
                Next
            Next
            For j = 1 To ipph - 1
                Dim idxt1 = j * idl1
                For ik = 0 To idl1 - 1
                    Dim idx1 = out_off + ik
                    outp(idx1) += outp(idx1 + idxt1)
                Next
            Next
            For j = 1 To ipph - 1
                jc = ip - j
                Dim idx1 = j * idl1
                Dim idx2 = jc * idl1
                For ik = 1 To idl1 - 1 Step 2
                    Dim idx3 = out_off + ik
                    Dim idx4 = in_off + ik
                    Dim iidx1 = idx4 + idx1
                    Dim iidx2 = idx4 + idx2
                    Dim i1i = inp(iidx1 - 1)
                    Dim i1r = inp(iidx1)
                    Dim i2i = inp(iidx2 - 1)
                    Dim i2r = inp(iidx2)

                    Dim oidx1 = idx3 + idx1
                    Dim oidx2 = idx3 + idx2
                    outp(oidx1 - 1) = i1i - i2r
                    outp(oidx2 - 1) = i1i + i2r
                    outp(oidx1) = i1r + i2i
                    outp(oidx2) = i1r - i2i
                Next
            Next
            nac(0) = 1
            If ido = 2 Then Return
            nac(0) = 0
            Array.Copy(outp, out_off, inp, in_off, idl1)
            Dim idx0 = l1 * ido
            For j = 1 To ip - 1
                Dim idx1 = j * idx0
                For k = 0 To l1 - 1
                    Dim idx2 = k * ido
                    Dim oidx1 = out_off + idx2 + idx1
                    Dim iidx1 = in_off + idx2 + idx1
                    inp(iidx1) = outp(oidx1)
                    inp(iidx1 + 1) = outp(oidx1 + 1)
                Next
            Next
            If idot <= l1 Then
                idij = 0
                For j = 1 To ip - 1
                    idij += 2
                    Dim idx1 = j * l1 * ido
                    For i = 3 To ido - 1 Step 2
                        idij += 2
                        Dim idx2 = idij + iw1 - 1
                        w1r = wtable(idx2 - 1)
                        w1i = isign * wtable(idx2)
                        Dim idx3 = in_off + i
                        Dim idx4 = out_off + i
                        For k = 0 To l1 - 1
                            Dim idx5 = k * ido + idx1
                            Dim iidx1 = idx3 + idx5
                            Dim oidx1 = idx4 + idx5
                            Dim o1i = outp(oidx1 - 1)
                            Dim o1r = outp(oidx1)
                            inp(iidx1 - 1) = w1r * o1i - w1i * o1r
                            inp(iidx1) = w1r * o1r + w1i * o1i
                        Next
                    Next
                Next
            Else
                idj = 2 - ido
                For j = 1 To ip - 1
                    idj += ido
                    Dim idx1 = j * l1 * ido
                    For k = 0 To l1 - 1
                        idij = idj
                        Dim idx3 = k * ido + idx1
                        For i = 3 To ido - 1 Step 2
                            idij += 2
                            Dim idx2 = idij - 1 + iw1
                            w1r = wtable(idx2 - 1)
                            w1i = isign * wtable(idx2)
                            Dim iidx1 = in_off + i + idx3
                            Dim oidx1 = out_off + i + idx3
                            Dim o1i = outp(oidx1 - 1)
                            Dim o1r = outp(oidx1)
                            inp(iidx1 - 1) = w1r * o1i - w1i * o1r
                            inp(iidx1) = w1r * o1r + w1i * o1i
                        Next
                    Next
                Next
            End If
        End Sub

        Private Sub cftfsub(n As Integer, a As Double(), offa As Integer, ip As Integer(), nw As Integer, w As Double())
            If n > 8 Then
                If n > 32 Then
                    cftf1st(n, a, offa, w, nw - (n >> 2))
                    If (Process.GetCurrentProcess().Threads.Count > 1) AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_2THREADS Then
                        cftrec4_th(n, a, offa, nw, w)
                    ElseIf n > 512 Then
                        cftrec4(n, a, offa, nw, w)
                    ElseIf n > 128 Then
                        cftleaf(n, 1, a, offa, nw, w)
                    Else
                        cftfx41(n, a, offa, nw, w)
                    End If
                    bitrv2(n, ip, a, offa)
                ElseIf n = 32 Then
                    cftf161(a, offa, w, nw - 8)
                    bitrv216(a, offa)
                Else
                    cftf081(a, offa, w, 0)
                    bitrv208(a, offa)
                End If
            ElseIf n = 8 Then
                cftf040(a, offa)
            ElseIf n = 4 Then
                cftxb020(a, offa)
            End If
        End Sub

        Private Sub cftbsub(n As Integer, a As Double(), offa As Integer, ip As Integer(), nw As Integer, w As Double())
            If n > 8 Then
                If n > 32 Then
                    cftb1st(n, a, offa, w, nw - (n >> 2))
                    If (Process.GetCurrentProcess().Threads.Count > 1) AndAlso n > TransformCore.THREADS_BEGIN_N_1D_FFT_2THREADS Then
                        cftrec4_th(n, a, offa, nw, w)
                    ElseIf n > 512 Then
                        cftrec4(n, a, offa, nw, w)
                    ElseIf n > 128 Then
                        cftleaf(n, 1, a, offa, nw, w)
                    Else
                        cftfx41(n, a, offa, nw, w)
                    End If
                    bitrv2conj(n, ip, a, offa)
                ElseIf n = 32 Then
                    cftf161(a, offa, w, nw - 8)
                    bitrv216neg(a, offa)
                Else
                    cftf081(a, offa, w, 0)
                    bitrv208neg(a, offa)
                End If
            ElseIf n = 8 Then
                cftb040(a, offa)
            ElseIf n = 4 Then
                cftxb020(a, offa)
            End If
        End Sub

        Private Sub bitrv2(n As Integer, ip As Integer(), a As Double(), offa As Integer)
            Dim j1, k1, l, m, nh, nm As Integer
            Dim xr, xi, yr, yi As Double
            Dim idx0, idx1, idx2 As Integer

            m = 1
            l = n >> 2

            While l > 8
                m <<= 1
                l >>= 2
            End While
            nh = n >> 1
            nm = 4 * m
            If l = 8 Then
                For k = 0 To m - 1
                    idx0 = 4 * k
                    For j = 0 To k - 1
                        j1 = 4 * j + 2 * ip(m + k)
                        k1 = idx0 + 2 * ip(m + j)
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 -= nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nh
                        k1 += 2
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 += nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += 2
                        k1 += nh
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 -= nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nh
                        k1 -= 2
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 += nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                    Next
                    k1 = idx0 + 2 * ip(m + k)
                    j1 = k1 + 2
                    k1 += nh
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = a(idx1 + 1)
                    yr = a(idx2)
                    yi = a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    j1 += nm
                    k1 += 2 * nm
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = a(idx1 + 1)
                    yr = a(idx2)
                    yi = a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    j1 += nm
                    k1 -= nm
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = a(idx1 + 1)
                    yr = a(idx2)
                    yi = a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    j1 -= 2
                    k1 -= nh
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = a(idx1 + 1)
                    yr = a(idx2)
                    yi = a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    j1 += nh + 2
                    k1 += nh + 2
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = a(idx1 + 1)
                    yr = a(idx2)
                    yi = a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    j1 -= nh - nm
                    k1 += 2 * nm - 2
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = a(idx1 + 1)
                    yr = a(idx2)
                    yi = a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                Next
            Else
                For k = 0 To m - 1
                    idx0 = 4 * k
                    For j = 0 To k - 1
                        j1 = 4 * j + ip(m + k)
                        k1 = idx0 + ip(m + j)
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nh
                        k1 += 2
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += 2
                        k1 += nh
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nh
                        k1 -= 2
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = a(idx1 + 1)
                        yr = a(idx2)
                        yi = a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                    Next
                    k1 = idx0 + ip(m + k)
                    j1 = k1 + 2
                    k1 += nh
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = a(idx1 + 1)
                    yr = a(idx2)
                    yi = a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    j1 += nm
                    k1 += nm
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = a(idx1 + 1)
                    yr = a(idx2)
                    yi = a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                Next
            End If
        End Sub

        Private Sub bitrv2conj(n As Integer, ip As Integer(), a As Double(), offa As Integer)
            Dim j1, k1, l, m, nh, nm As Integer
            Dim xr, xi, yr, yi As Double
            Dim idx0, idx1, idx2 As Integer

            m = 1
            l = n >> 2

            While l > 8
                m <<= 1
                l >>= 2
            End While
            nh = n >> 1
            nm = 4 * m
            If l = 8 Then
                For k = 0 To m - 1
                    idx0 = 4 * k
                    For j = 0 To k - 1
                        j1 = 4 * j + 2 * ip(m + k)
                        k1 = idx0 + 2 * ip(m + j)
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 -= nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nh
                        k1 += 2
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 += nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += 2
                        k1 += nh
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 -= nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nh
                        k1 -= 2
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 += nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= 2 * nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                    Next
                    k1 = idx0 + 2 * ip(m + k)
                    j1 = k1 + 2
                    k1 += nh
                    idx1 = offa + j1
                    idx2 = offa + k1
                    a(idx1 - 1) = -a(idx1 - 1)
                    xr = a(idx1)
                    xi = -a(idx1 + 1)
                    yr = a(idx2)
                    yi = -a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    a(idx2 + 3) = -a(idx2 + 3)
                    j1 += nm
                    k1 += 2 * nm
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = -a(idx1 + 1)
                    yr = a(idx2)
                    yi = -a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    j1 += nm
                    k1 -= nm
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = -a(idx1 + 1)
                    yr = a(idx2)
                    yi = -a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    j1 -= 2
                    k1 -= nh
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = -a(idx1 + 1)
                    yr = a(idx2)
                    yi = -a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    j1 += nh + 2
                    k1 += nh + 2
                    idx1 = offa + j1
                    idx2 = offa + k1
                    xr = a(idx1)
                    xi = -a(idx1 + 1)
                    yr = a(idx2)
                    yi = -a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    j1 -= nh - nm
                    k1 += 2 * nm - 2
                    idx1 = offa + j1
                    idx2 = offa + k1
                    a(idx1 - 1) = -a(idx1 - 1)
                    xr = a(idx1)
                    xi = -a(idx1 + 1)
                    yr = a(idx2)
                    yi = -a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    a(idx2 + 3) = -a(idx2 + 3)
                Next
            Else
                For k = 0 To m - 1
                    idx0 = 4 * k
                    For j = 0 To k - 1
                        j1 = 4 * j + ip(m + k)
                        k1 = idx0 + ip(m + j)
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nh
                        k1 += 2
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += 2
                        k1 += nh
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 += nm
                        k1 += nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nh
                        k1 -= 2
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                        j1 -= nm
                        k1 -= nm
                        idx1 = offa + j1
                        idx2 = offa + k1
                        xr = a(idx1)
                        xi = -a(idx1 + 1)
                        yr = a(idx2)
                        yi = -a(idx2 + 1)
                        a(idx1) = yr
                        a(idx1 + 1) = yi
                        a(idx2) = xr
                        a(idx2 + 1) = xi
                    Next
                    k1 = idx0 + ip(m + k)
                    j1 = k1 + 2
                    k1 += nh
                    idx1 = offa + j1
                    idx2 = offa + k1
                    a(idx1 - 1) = -a(idx1 - 1)
                    xr = a(idx1)
                    xi = -a(idx1 + 1)
                    yr = a(idx2)
                    yi = -a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    a(idx2 + 3) = -a(idx2 + 3)
                    j1 += nm
                    k1 += nm
                    idx1 = offa + j1
                    idx2 = offa + k1
                    a(idx1 - 1) = -a(idx1 - 1)
                    xr = a(idx1)
                    xi = -a(idx1 + 1)
                    yr = a(idx2)
                    yi = -a(idx2 + 1)
                    a(idx1) = yr
                    a(idx1 + 1) = yi
                    a(idx2) = xr
                    a(idx2 + 1) = xi
                    a(idx2 + 3) = -a(idx2 + 3)
                Next
            End If
        End Sub

        Private Sub bitrv216(a As Double(), offa As Integer)
            Dim x1r, x1i, x2r, x2i, x3r, x3i, x4r, x4i, x5r, x5i, x7r, x7i, x8r, x8i, x10r, x10i, x11r, x11i, x12r, x12i, x13r, x13i, x14r, x14i As Double

            x1r = a(offa + 2)
            x1i = a(offa + 3)
            x2r = a(offa + 4)
            x2i = a(offa + 5)
            x3r = a(offa + 6)
            x3i = a(offa + 7)
            x4r = a(offa + 8)
            x4i = a(offa + 9)
            x5r = a(offa + 10)
            x5i = a(offa + 11)
            x7r = a(offa + 14)
            x7i = a(offa + 15)
            x8r = a(offa + 16)
            x8i = a(offa + 17)
            x10r = a(offa + 20)
            x10i = a(offa + 21)
            x11r = a(offa + 22)
            x11i = a(offa + 23)
            x12r = a(offa + 24)
            x12i = a(offa + 25)
            x13r = a(offa + 26)
            x13i = a(offa + 27)
            x14r = a(offa + 28)
            x14i = a(offa + 29)
            a(offa + 2) = x8r
            a(offa + 3) = x8i
            a(offa + 4) = x4r
            a(offa + 5) = x4i
            a(offa + 6) = x12r
            a(offa + 7) = x12i
            a(offa + 8) = x2r
            a(offa + 9) = x2i
            a(offa + 10) = x10r
            a(offa + 11) = x10i
            a(offa + 14) = x14r
            a(offa + 15) = x14i
            a(offa + 16) = x1r
            a(offa + 17) = x1i
            a(offa + 20) = x5r
            a(offa + 21) = x5i
            a(offa + 22) = x13r
            a(offa + 23) = x13i
            a(offa + 24) = x3r
            a(offa + 25) = x3i
            a(offa + 26) = x11r
            a(offa + 27) = x11i
            a(offa + 28) = x7r
            a(offa + 29) = x7i
        End Sub

        Private Sub bitrv216neg(a As Double(), offa As Integer)
            Dim x1r, x1i, x2r, x2i, x3r, x3i, x4r, x4i, x5r, x5i, x6r, x6i, x7r, x7i, x8r, x8i, x9r, x9i, x10r, x10i, x11r, x11i, x12r, x12i, x13r, x13i, x14r, x14i, x15r, x15i As Double

            x1r = a(offa + 2)
            x1i = a(offa + 3)
            x2r = a(offa + 4)
            x2i = a(offa + 5)
            x3r = a(offa + 6)
            x3i = a(offa + 7)
            x4r = a(offa + 8)
            x4i = a(offa + 9)
            x5r = a(offa + 10)
            x5i = a(offa + 11)
            x6r = a(offa + 12)
            x6i = a(offa + 13)
            x7r = a(offa + 14)
            x7i = a(offa + 15)
            x8r = a(offa + 16)
            x8i = a(offa + 17)
            x9r = a(offa + 18)
            x9i = a(offa + 19)
            x10r = a(offa + 20)
            x10i = a(offa + 21)
            x11r = a(offa + 22)
            x11i = a(offa + 23)
            x12r = a(offa + 24)
            x12i = a(offa + 25)
            x13r = a(offa + 26)
            x13i = a(offa + 27)
            x14r = a(offa + 28)
            x14i = a(offa + 29)
            x15r = a(offa + 30)
            x15i = a(offa + 31)
            a(offa + 2) = x15r
            a(offa + 3) = x15i
            a(offa + 4) = x7r
            a(offa + 5) = x7i
            a(offa + 6) = x11r
            a(offa + 7) = x11i
            a(offa + 8) = x3r
            a(offa + 9) = x3i
            a(offa + 10) = x13r
            a(offa + 11) = x13i
            a(offa + 12) = x5r
            a(offa + 13) = x5i
            a(offa + 14) = x9r
            a(offa + 15) = x9i
            a(offa + 16) = x1r
            a(offa + 17) = x1i
            a(offa + 18) = x14r
            a(offa + 19) = x14i
            a(offa + 20) = x6r
            a(offa + 21) = x6i
            a(offa + 22) = x10r
            a(offa + 23) = x10i
            a(offa + 24) = x2r
            a(offa + 25) = x2i
            a(offa + 26) = x12r
            a(offa + 27) = x12i
            a(offa + 28) = x4r
            a(offa + 29) = x4i
            a(offa + 30) = x8r
            a(offa + 31) = x8i
        End Sub

        Private Sub bitrv208(a As Double(), offa As Integer)
            Dim x1r, x1i, x3r, x3i, x4r, x4i, x6r, x6i As Double

            x1r = a(offa + 2)
            x1i = a(offa + 3)
            x3r = a(offa + 6)
            x3i = a(offa + 7)
            x4r = a(offa + 8)
            x4i = a(offa + 9)
            x6r = a(offa + 12)
            x6i = a(offa + 13)
            a(offa + 2) = x4r
            a(offa + 3) = x4i
            a(offa + 6) = x6r
            a(offa + 7) = x6i
            a(offa + 8) = x1r
            a(offa + 9) = x1i
            a(offa + 12) = x3r
            a(offa + 13) = x3i
        End Sub

        Private Sub bitrv208neg(a As Double(), offa As Integer)
            Dim x1r, x1i, x2r, x2i, x3r, x3i, x4r, x4i, x5r, x5i, x6r, x6i, x7r, x7i As Double

            x1r = a(offa + 2)
            x1i = a(offa + 3)
            x2r = a(offa + 4)
            x2i = a(offa + 5)
            x3r = a(offa + 6)
            x3i = a(offa + 7)
            x4r = a(offa + 8)
            x4i = a(offa + 9)
            x5r = a(offa + 10)
            x5i = a(offa + 11)
            x6r = a(offa + 12)
            x6i = a(offa + 13)
            x7r = a(offa + 14)
            x7i = a(offa + 15)
            a(offa + 2) = x7r
            a(offa + 3) = x7i
            a(offa + 4) = x3r
            a(offa + 5) = x3i
            a(offa + 6) = x5r
            a(offa + 7) = x5i
            a(offa + 8) = x1r
            a(offa + 9) = x1i
            a(offa + 10) = x6r
            a(offa + 11) = x6i
            a(offa + 12) = x2r
            a(offa + 13) = x2i
            a(offa + 14) = x4r
            a(offa + 15) = x4i
        End Sub

        Private Sub cftf1st(n As Integer, a As Double(), offa As Integer, w As Double(), startw As Integer)
            Dim j0, j1, j2, j3, k, m, mh As Integer
            Dim wn4r, csc1, csc3, wk1r, wk1i, wk3r, wk3i, wd1r, wd1i, wd3r, wd3i As Double
            Dim x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i, y0r, y0i, y1r, y1i, y2r, y2i, y3r, y3i As Double
            Dim idx0, idx1, idx2, idx3, idx4, idx5 As Integer
            mh = n >> 3
            m = 2 * mh
            j1 = m
            j2 = j1 + m
            j3 = j2 + m
            idx1 = offa + j1
            idx2 = offa + j2
            idx3 = offa + j3
            x0r = a(offa) + a(idx2)
            x0i = a(offa + 1) + a(idx2 + 1)
            x1r = a(offa) - a(idx2)
            x1i = a(offa + 1) - a(idx2 + 1)
            x2r = a(idx1) + a(idx3)
            x2i = a(idx1 + 1) + a(idx3 + 1)
            x3r = a(idx1) - a(idx3)
            x3i = a(idx1 + 1) - a(idx3 + 1)
            a(offa) = x0r + x2r
            a(offa + 1) = x0i + x2i
            a(idx1) = x0r - x2r
            a(idx1 + 1) = x0i - x2i
            a(idx2) = x1r - x3i
            a(idx2 + 1) = x1i + x3r
            a(idx3) = x1r + x3i
            a(idx3 + 1) = x1i - x3r
            wn4r = w(startw + 1)
            csc1 = w(startw + 2)
            csc3 = w(startw + 3)
            wd1r = 1
            wd1i = 0
            wd3r = 1
            wd3i = 0
            k = 0
            For j = 2 To mh - 2 - 1 Step 4
                k += 4
                idx4 = startw + k
                wk1r = csc1 * (wd1r + w(idx4))
                wk1i = csc1 * (wd1i + w(idx4 + 1))
                wk3r = csc3 * (wd3r + w(idx4 + 2))
                wk3i = csc3 * (wd3i + w(idx4 + 3))
                wd1r = w(idx4)
                wd1i = w(idx4 + 1)
                wd3r = w(idx4 + 2)
                wd3i = w(idx4 + 3)
                j1 = j + m
                j2 = j1 + m
                j3 = j2 + m
                idx1 = offa + j1
                idx2 = offa + j2
                idx3 = offa + j3
                idx5 = offa + j
                x0r = a(idx5) + a(idx2)
                x0i = a(idx5 + 1) + a(idx2 + 1)
                x1r = a(idx5) - a(idx2)
                x1i = a(idx5 + 1) - a(idx2 + 1)
                y0r = a(idx5 + 2) + a(idx2 + 2)
                y0i = a(idx5 + 3) + a(idx2 + 3)
                y1r = a(idx5 + 2) - a(idx2 + 2)
                y1i = a(idx5 + 3) - a(idx2 + 3)
                x2r = a(idx1) + a(idx3)
                x2i = a(idx1 + 1) + a(idx3 + 1)
                x3r = a(idx1) - a(idx3)
                x3i = a(idx1 + 1) - a(idx3 + 1)
                y2r = a(idx1 + 2) + a(idx3 + 2)
                y2i = a(idx1 + 3) + a(idx3 + 3)
                y3r = a(idx1 + 2) - a(idx3 + 2)
                y3i = a(idx1 + 3) - a(idx3 + 3)
                a(idx5) = x0r + x2r
                a(idx5 + 1) = x0i + x2i
                a(idx5 + 2) = y0r + y2r
                a(idx5 + 3) = y0i + y2i
                a(idx1) = x0r - x2r
                a(idx1 + 1) = x0i - x2i
                a(idx1 + 2) = y0r - y2r
                a(idx1 + 3) = y0i - y2i
                x0r = x1r - x3i
                x0i = x1i + x3r
                a(idx2) = wk1r * x0r - wk1i * x0i
                a(idx2 + 1) = wk1r * x0i + wk1i * x0r
                x0r = y1r - y3i
                x0i = y1i + y3r
                a(idx2 + 2) = wd1r * x0r - wd1i * x0i
                a(idx2 + 3) = wd1r * x0i + wd1i * x0r
                x0r = x1r + x3i
                x0i = x1i - x3r
                a(idx3) = wk3r * x0r + wk3i * x0i
                a(idx3 + 1) = wk3r * x0i - wk3i * x0r
                x0r = y1r + y3i
                x0i = y1i - y3r
                a(idx3 + 2) = wd3r * x0r + wd3i * x0i
                a(idx3 + 3) = wd3r * x0i - wd3i * x0r
                j0 = m - j
                j1 = j0 + m
                j2 = j1 + m
                j3 = j2 + m
                idx0 = offa + j0
                idx1 = offa + j1
                idx2 = offa + j2
                idx3 = offa + j3
                x0r = a(idx0) + a(idx2)
                x0i = a(idx0 + 1) + a(idx2 + 1)
                x1r = a(idx0) - a(idx2)
                x1i = a(idx0 + 1) - a(idx2 + 1)
                y0r = a(idx0 - 2) + a(idx2 - 2)
                y0i = a(idx0 - 1) + a(idx2 - 1)
                y1r = a(idx0 - 2) - a(idx2 - 2)
                y1i = a(idx0 - 1) - a(idx2 - 1)
                x2r = a(idx1) + a(idx3)
                x2i = a(idx1 + 1) + a(idx3 + 1)
                x3r = a(idx1) - a(idx3)
                x3i = a(idx1 + 1) - a(idx3 + 1)
                y2r = a(idx1 - 2) + a(idx3 - 2)
                y2i = a(idx1 - 1) + a(idx3 - 1)
                y3r = a(idx1 - 2) - a(idx3 - 2)
                y3i = a(idx1 - 1) - a(idx3 - 1)
                a(idx0) = x0r + x2r
                a(idx0 + 1) = x0i + x2i
                a(idx0 - 2) = y0r + y2r
                a(idx0 - 1) = y0i + y2i
                a(idx1) = x0r - x2r
                a(idx1 + 1) = x0i - x2i
                a(idx1 - 2) = y0r - y2r
                a(idx1 - 1) = y0i - y2i
                x0r = x1r - x3i
                x0i = x1i + x3r
                a(idx2) = wk1i * x0r - wk1r * x0i
                a(idx2 + 1) = wk1i * x0i + wk1r * x0r
                x0r = y1r - y3i
                x0i = y1i + y3r
                a(idx2 - 2) = wd1i * x0r - wd1r * x0i
                a(idx2 - 1) = wd1i * x0i + wd1r * x0r
                x0r = x1r + x3i
                x0i = x1i - x3r
                a(idx3) = wk3i * x0r + wk3r * x0i
                a(idx3 + 1) = wk3i * x0i - wk3r * x0r
                x0r = y1r + y3i
                x0i = y1i - y3r
                a(offa + j3 - 2) = wd3i * x0r + wd3r * x0i
                a(offa + j3 - 1) = wd3i * x0i - wd3r * x0r
            Next
            wk1r = csc1 * (wd1r + wn4r)
            wk1i = csc1 * (wd1i + wn4r)
            wk3r = csc3 * (wd3r - wn4r)
            wk3i = csc3 * (wd3i - wn4r)
            j0 = mh
            j1 = j0 + m
            j2 = j1 + m
            j3 = j2 + m
            idx0 = offa + j0
            idx1 = offa + j1
            idx2 = offa + j2
            idx3 = offa + j3
            x0r = a(idx0 - 2) + a(idx2 - 2)
            x0i = a(idx0 - 1) + a(idx2 - 1)
            x1r = a(idx0 - 2) - a(idx2 - 2)
            x1i = a(idx0 - 1) - a(idx2 - 1)
            x2r = a(idx1 - 2) + a(idx3 - 2)
            x2i = a(idx1 - 1) + a(idx3 - 1)
            x3r = a(idx1 - 2) - a(idx3 - 2)
            x3i = a(idx1 - 1) - a(idx3 - 1)
            a(idx0 - 2) = x0r + x2r
            a(idx0 - 1) = x0i + x2i
            a(idx1 - 2) = x0r - x2r
            a(idx1 - 1) = x0i - x2i
            x0r = x1r - x3i
            x0i = x1i + x3r
            a(idx2 - 2) = wk1r * x0r - wk1i * x0i
            a(idx2 - 1) = wk1r * x0i + wk1i * x0r
            x0r = x1r + x3i
            x0i = x1i - x3r
            a(idx3 - 2) = wk3r * x0r + wk3i * x0i
            a(idx3 - 1) = wk3r * x0i - wk3i * x0r
            x0r = a(idx0) + a(idx2)
            x0i = a(idx0 + 1) + a(idx2 + 1)
            x1r = a(idx0) - a(idx2)
            x1i = a(idx0 + 1) - a(idx2 + 1)
            x2r = a(idx1) + a(idx3)
            x2i = a(idx1 + 1) + a(idx3 + 1)
            x3r = a(idx1) - a(idx3)
            x3i = a(idx1 + 1) - a(idx3 + 1)
            a(idx0) = x0r + x2r
            a(idx0 + 1) = x0i + x2i
            a(idx1) = x0r - x2r
            a(idx1 + 1) = x0i - x2i
            x0r = x1r - x3i
            x0i = x1i + x3r
            a(idx2) = wn4r * (x0r - x0i)
            a(idx2 + 1) = wn4r * (x0i + x0r)
            x0r = x1r + x3i
            x0i = x1i - x3r
            a(idx3) = -wn4r * (x0r + x0i)
            a(idx3 + 1) = -wn4r * (x0i - x0r)
            x0r = a(idx0 + 2) + a(idx2 + 2)
            x0i = a(idx0 + 3) + a(idx2 + 3)
            x1r = a(idx0 + 2) - a(idx2 + 2)
            x1i = a(idx0 + 3) - a(idx2 + 3)
            x2r = a(idx1 + 2) + a(idx3 + 2)
            x2i = a(idx1 + 3) + a(idx3 + 3)
            x3r = a(idx1 + 2) - a(idx3 + 2)
            x3i = a(idx1 + 3) - a(idx3 + 3)
            a(idx0 + 2) = x0r + x2r
            a(idx0 + 3) = x0i + x2i
            a(idx1 + 2) = x0r - x2r
            a(idx1 + 3) = x0i - x2i
            x0r = x1r - x3i
            x0i = x1i + x3r
            a(idx2 + 2) = wk1i * x0r - wk1r * x0i
            a(idx2 + 3) = wk1i * x0i + wk1r * x0r
            x0r = x1r + x3i
            x0i = x1i - x3r
            a(idx3 + 2) = wk3i * x0r + wk3r * x0i
            a(idx3 + 3) = wk3i * x0i - wk3r * x0r
        End Sub

        Private Sub cftb1st(n As Integer, a As Double(), offa As Integer, w As Double(), startw As Integer)
            Dim j0, j1, j2, j3, k, m, mh As Integer
            Dim wn4r, csc1, csc3, wk1r, wk1i, wk3r, wk3i, wd1r, wd1i, wd3r, wd3i As Double
            Dim x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i, y0r, y0i, y1r, y1i, y2r, y2i, y3r, y3i As Double
            Dim idx0, idx1, idx2, idx3, idx4, idx5 As Integer
            mh = n >> 3
            m = 2 * mh
            j1 = m
            j2 = j1 + m
            j3 = j2 + m
            idx1 = offa + j1
            idx2 = offa + j2
            idx3 = offa + j3

            x0r = a(offa) + a(idx2)
            x0i = -a(offa + 1) - a(idx2 + 1)
            x1r = a(offa) - a(idx2)
            x1i = -a(offa + 1) + a(idx2 + 1)
            x2r = a(idx1) + a(idx3)
            x2i = a(idx1 + 1) + a(idx3 + 1)
            x3r = a(idx1) - a(idx3)
            x3i = a(idx1 + 1) - a(idx3 + 1)
            a(offa) = x0r + x2r
            a(offa + 1) = x0i - x2i
            a(idx1) = x0r - x2r
            a(idx1 + 1) = x0i + x2i
            a(idx2) = x1r + x3i
            a(idx2 + 1) = x1i + x3r
            a(idx3) = x1r - x3i
            a(idx3 + 1) = x1i - x3r
            wn4r = w(startw + 1)
            csc1 = w(startw + 2)
            csc3 = w(startw + 3)
            wd1r = 1
            wd1i = 0
            wd3r = 1
            wd3i = 0
            k = 0
            For j = 2 To mh - 2 - 1 Step 4
                k += 4
                idx4 = startw + k
                wk1r = csc1 * (wd1r + w(idx4))
                wk1i = csc1 * (wd1i + w(idx4 + 1))
                wk3r = csc3 * (wd3r + w(idx4 + 2))
                wk3i = csc3 * (wd3i + w(idx4 + 3))
                wd1r = w(idx4)
                wd1i = w(idx4 + 1)
                wd3r = w(idx4 + 2)
                wd3i = w(idx4 + 3)
                j1 = j + m
                j2 = j1 + m
                j3 = j2 + m
                idx1 = offa + j1
                idx2 = offa + j2
                idx3 = offa + j3
                idx5 = offa + j
                x0r = a(idx5) + a(idx2)
                x0i = -a(idx5 + 1) - a(idx2 + 1)
                x1r = a(idx5) - a(offa + j2)
                x1i = -a(idx5 + 1) + a(idx2 + 1)
                y0r = a(idx5 + 2) + a(idx2 + 2)
                y0i = -a(idx5 + 3) - a(idx2 + 3)
                y1r = a(idx5 + 2) - a(idx2 + 2)
                y1i = -a(idx5 + 3) + a(idx2 + 3)
                x2r = a(idx1) + a(idx3)
                x2i = a(idx1 + 1) + a(idx3 + 1)
                x3r = a(idx1) - a(idx3)
                x3i = a(idx1 + 1) - a(idx3 + 1)
                y2r = a(idx1 + 2) + a(idx3 + 2)
                y2i = a(idx1 + 3) + a(idx3 + 3)
                y3r = a(idx1 + 2) - a(idx3 + 2)
                y3i = a(idx1 + 3) - a(idx3 + 3)
                a(idx5) = x0r + x2r
                a(idx5 + 1) = x0i - x2i
                a(idx5 + 2) = y0r + y2r
                a(idx5 + 3) = y0i - y2i
                a(idx1) = x0r - x2r
                a(idx1 + 1) = x0i + x2i
                a(idx1 + 2) = y0r - y2r
                a(idx1 + 3) = y0i + y2i
                x0r = x1r + x3i
                x0i = x1i + x3r
                a(idx2) = wk1r * x0r - wk1i * x0i
                a(idx2 + 1) = wk1r * x0i + wk1i * x0r
                x0r = y1r + y3i
                x0i = y1i + y3r
                a(idx2 + 2) = wd1r * x0r - wd1i * x0i
                a(idx2 + 3) = wd1r * x0i + wd1i * x0r
                x0r = x1r - x3i
                x0i = x1i - x3r
                a(idx3) = wk3r * x0r + wk3i * x0i
                a(idx3 + 1) = wk3r * x0i - wk3i * x0r
                x0r = y1r - y3i
                x0i = y1i - y3r
                a(idx3 + 2) = wd3r * x0r + wd3i * x0i
                a(idx3 + 3) = wd3r * x0i - wd3i * x0r
                j0 = m - j
                j1 = j0 + m
                j2 = j1 + m
                j3 = j2 + m
                idx0 = offa + j0
                idx1 = offa + j1
                idx2 = offa + j2
                idx3 = offa + j3
                x0r = a(idx0) + a(idx2)
                x0i = -a(idx0 + 1) - a(idx2 + 1)
                x1r = a(idx0) - a(idx2)
                x1i = -a(idx0 + 1) + a(idx2 + 1)
                y0r = a(idx0 - 2) + a(idx2 - 2)
                y0i = -a(idx0 - 1) - a(idx2 - 1)
                y1r = a(idx0 - 2) - a(idx2 - 2)
                y1i = -a(idx0 - 1) + a(idx2 - 1)
                x2r = a(idx1) + a(idx3)
                x2i = a(idx1 + 1) + a(idx3 + 1)
                x3r = a(idx1) - a(idx3)
                x3i = a(idx1 + 1) - a(idx3 + 1)
                y2r = a(idx1 - 2) + a(idx3 - 2)
                y2i = a(idx1 - 1) + a(idx3 - 1)
                y3r = a(idx1 - 2) - a(idx3 - 2)
                y3i = a(idx1 - 1) - a(idx3 - 1)
                a(idx0) = x0r + x2r
                a(idx0 + 1) = x0i - x2i
                a(idx0 - 2) = y0r + y2r
                a(idx0 - 1) = y0i - y2i
                a(idx1) = x0r - x2r
                a(idx1 + 1) = x0i + x2i
                a(idx1 - 2) = y0r - y2r
                a(idx1 - 1) = y0i + y2i
                x0r = x1r + x3i
                x0i = x1i + x3r
                a(idx2) = wk1i * x0r - wk1r * x0i
                a(idx2 + 1) = wk1i * x0i + wk1r * x0r
                x0r = y1r + y3i
                x0i = y1i + y3r
                a(idx2 - 2) = wd1i * x0r - wd1r * x0i
                a(idx2 - 1) = wd1i * x0i + wd1r * x0r
                x0r = x1r - x3i
                x0i = x1i - x3r
                a(idx3) = wk3i * x0r + wk3r * x0i
                a(idx3 + 1) = wk3i * x0i - wk3r * x0r
                x0r = y1r - y3i
                x0i = y1i - y3r
                a(idx3 - 2) = wd3i * x0r + wd3r * x0i
                a(idx3 - 1) = wd3i * x0i - wd3r * x0r
            Next
            wk1r = csc1 * (wd1r + wn4r)
            wk1i = csc1 * (wd1i + wn4r)
            wk3r = csc3 * (wd3r - wn4r)
            wk3i = csc3 * (wd3i - wn4r)
            j0 = mh
            j1 = j0 + m
            j2 = j1 + m
            j3 = j2 + m
            idx0 = offa + j0
            idx1 = offa + j1
            idx2 = offa + j2
            idx3 = offa + j3
            x0r = a(idx0 - 2) + a(idx2 - 2)
            x0i = -a(idx0 - 1) - a(idx2 - 1)
            x1r = a(idx0 - 2) - a(idx2 - 2)
            x1i = -a(idx0 - 1) + a(idx2 - 1)
            x2r = a(idx1 - 2) + a(idx3 - 2)
            x2i = a(idx1 - 1) + a(idx3 - 1)
            x3r = a(idx1 - 2) - a(idx3 - 2)
            x3i = a(idx1 - 1) - a(idx3 - 1)
            a(idx0 - 2) = x0r + x2r
            a(idx0 - 1) = x0i - x2i
            a(idx1 - 2) = x0r - x2r
            a(idx1 - 1) = x0i + x2i
            x0r = x1r + x3i
            x0i = x1i + x3r
            a(idx2 - 2) = wk1r * x0r - wk1i * x0i
            a(idx2 - 1) = wk1r * x0i + wk1i * x0r
            x0r = x1r - x3i
            x0i = x1i - x3r
            a(idx3 - 2) = wk3r * x0r + wk3i * x0i
            a(idx3 - 1) = wk3r * x0i - wk3i * x0r
            x0r = a(idx0) + a(idx2)
            x0i = -a(idx0 + 1) - a(idx2 + 1)
            x1r = a(idx0) - a(idx2)
            x1i = -a(idx0 + 1) + a(idx2 + 1)
            x2r = a(idx1) + a(idx3)
            x2i = a(idx1 + 1) + a(idx3 + 1)
            x3r = a(idx1) - a(idx3)
            x3i = a(idx1 + 1) - a(idx3 + 1)
            a(idx0) = x0r + x2r
            a(idx0 + 1) = x0i - x2i
            a(idx1) = x0r - x2r
            a(idx1 + 1) = x0i + x2i
            x0r = x1r + x3i
            x0i = x1i + x3r
            a(idx2) = wn4r * (x0r - x0i)
            a(idx2 + 1) = wn4r * (x0i + x0r)
            x0r = x1r - x3i
            x0i = x1i - x3r
            a(idx3) = -wn4r * (x0r + x0i)
            a(idx3 + 1) = -wn4r * (x0i - x0r)
            x0r = a(idx0 + 2) + a(idx2 + 2)
            x0i = -a(idx0 + 3) - a(idx2 + 3)
            x1r = a(idx0 + 2) - a(idx2 + 2)
            x1i = -a(idx0 + 3) + a(idx2 + 3)
            x2r = a(idx1 + 2) + a(idx3 + 2)
            x2i = a(idx1 + 3) + a(idx3 + 3)
            x3r = a(idx1 + 2) - a(idx3 + 2)
            x3i = a(idx1 + 3) - a(idx3 + 3)
            a(idx0 + 2) = x0r + x2r
            a(idx0 + 3) = x0i - x2i
            a(idx1 + 2) = x0r - x2r
            a(idx1 + 3) = x0i + x2i
            x0r = x1r + x3i
            x0i = x1i + x3r
            a(idx2 + 2) = wk1i * x0r - wk1r * x0i
            a(idx2 + 3) = wk1i * x0i + wk1r * x0r
            x0r = x1r - x3i
            x0i = x1i - x3r
            a(idx3 + 2) = wk3i * x0r + wk3r * x0i
            a(idx3 + 3) = wk3i * x0i - wk3r * x0r
        End Sub

        Private Sub cftrec4_th(n As Integer, a As Double(), offa As Integer, nw As Integer, w As Double())
            Dim i As Integer
            Dim idiv4, m1, nthreads As Integer
            'int idx = 0;
            nthreads = 2
            idiv4 = 0
            m1 = n >> 1
            If n > TransformCore.THREADS_BEGIN_N_1D_FFT_4THREADS Then
                nthreads = 4
                idiv4 = 1
                m1 >>= 1
            End If
            Dim taskArray = New Task(nthreads - 1) {}
            Dim mf = m1
            For i = 0 To nthreads - 1
                Dim firstIdx = offa + i * m1
                If i <> idiv4 Then
                    taskArray(i) = Task.Factory.StartNew(Sub()
                                                             Dim isplt, j, k, m As Integer
                                                             Dim idx1 = firstIdx + mf
                                                             m = n
                                                             While m > 512
                                                                 m >>= 2
                                                                 cftmdl1(m, a, idx1 - m, w, nw - (m >> 1))
                                                             End While
                                                             cftleaf(m, 1, a, idx1 - m, nw, w)
                                                             k = 0
                                                             Dim idx2 = firstIdx - m
                                                             j = mf - m

                                                             While j > 0
                                                                 k += 1
                                                                 isplt = cfttree(m, j, k, a, firstIdx, nw, w)
                                                                 cftleaf(m, isplt, a, idx2 + j, nw, w)
                                                                 j -= m
                                                             End While

                                                         End Sub)
                Else
                    taskArray(i) = Task.Factory.StartNew(Sub()
                                                             Dim isplt, j, k, m As Integer
                                                             Dim idx1 = firstIdx + mf
                                                             k = 1
                                                             m = n
                                                             While m > 512
                                                                 m >>= 2
                                                                 k <<= 2
                                                                 cftmdl2(m, a, idx1 - m, w, nw - m)
                                                             End While
                                                             cftleaf(m, 0, a, idx1 - m, nw, w)
                                                             k >>= 1
                                                             Dim idx2 = firstIdx - m
                                                             j = mf - m

                                                             While j > 0
                                                                 k += 1
                                                                 isplt = cfttree(m, j, k, a, firstIdx, nw, w)
                                                                 cftleaf(m, isplt, a, idx2 + j, nw, w)
                                                                 j -= m
                                                             End While

                                                         End Sub)
                End If
            Next
            Try
                Task.WaitAll(taskArray)
            Finally

            End Try
        End Sub

        Private Sub cftrec4(n As Integer, a As Double(), offa As Integer, nw As Integer, w As Double())
            Dim isplt, j, k, m As Integer

            m = n
            Dim idx1 = offa + n
            While m > 512
                m >>= 2
                cftmdl1(m, a, idx1 - m, w, nw - (m >> 1))
            End While
            cftleaf(m, 1, a, idx1 - m, nw, w)
            k = 0
            Dim idx2 = offa - m
            j = n - m

            While j > 0
                k += 1
                isplt = cfttree(m, j, k, a, offa, nw, w)
                cftleaf(m, isplt, a, idx2 + j, nw, w)
                j -= m
            End While
        End Sub

        Private Function cfttree(n As Integer, j As Integer, k As Integer, a As Double(), offa As Integer, nw As Integer, w As Double()) As Integer
            Dim i, isplt, m As Integer
            Dim idx1 = offa - n
            If (k And 3) <> 0 Then
                isplt = k And 1
                If isplt <> 0 Then
                    cftmdl1(n, a, idx1 + j, w, nw - (n >> 1))
                Else
                    cftmdl2(n, a, idx1 + j, w, nw - n)
                End If
            Else
                m = n
                i = k

                While (i And 3) = 0
                    m <<= 2
                    i >>= 2
                End While
                isplt = i And 1
                Dim idx2 = offa + j
                If isplt <> 0 Then
                    While m > 128
                        cftmdl1(m, a, idx2 - m, w, nw - (m >> 1))
                        m >>= 2
                    End While
                Else
                    While m > 128
                        cftmdl2(m, a, idx2 - m, w, nw - m)
                        m >>= 2
                    End While
                End If
            End If
            Return isplt
        End Function

        Private Sub cftleaf(n As Integer, isplt As Integer, a As Double(), offa As Integer, nw As Integer, w As Double())
            If n = 512 Then
                cftmdl1(128, a, offa, w, nw - 64)
                cftf161(a, offa, w, nw - 8)
                cftf162(a, offa + 32, w, nw - 32)
                cftf161(a, offa + 64, w, nw - 8)
                cftf161(a, offa + 96, w, nw - 8)
                cftmdl2(128, a, offa + 128, w, nw - 128)
                cftf161(a, offa + 128, w, nw - 8)
                cftf162(a, offa + 160, w, nw - 32)
                cftf161(a, offa + 192, w, nw - 8)
                cftf162(a, offa + 224, w, nw - 32)
                cftmdl1(128, a, offa + 256, w, nw - 64)
                cftf161(a, offa + 256, w, nw - 8)
                cftf162(a, offa + 288, w, nw - 32)
                cftf161(a, offa + 320, w, nw - 8)
                cftf161(a, offa + 352, w, nw - 8)
                If isplt <> 0 Then
                    cftmdl1(128, a, offa + 384, w, nw - 64)
                    cftf161(a, offa + 480, w, nw - 8)
                Else
                    cftmdl2(128, a, offa + 384, w, nw - 128)
                    cftf162(a, offa + 480, w, nw - 32)
                End If
                cftf161(a, offa + 384, w, nw - 8)
                cftf162(a, offa + 416, w, nw - 32)
                cftf161(a, offa + 448, w, nw - 8)
            Else
                cftmdl1(64, a, offa, w, nw - 32)
                cftf081(a, offa, w, nw - 8)
                cftf082(a, offa + 16, w, nw - 8)
                cftf081(a, offa + 32, w, nw - 8)
                cftf081(a, offa + 48, w, nw - 8)
                cftmdl2(64, a, offa + 64, w, nw - 64)
                cftf081(a, offa + 64, w, nw - 8)
                cftf082(a, offa + 80, w, nw - 8)
                cftf081(a, offa + 96, w, nw - 8)
                cftf082(a, offa + 112, w, nw - 8)
                cftmdl1(64, a, offa + 128, w, nw - 32)
                cftf081(a, offa + 128, w, nw - 8)
                cftf082(a, offa + 144, w, nw - 8)
                cftf081(a, offa + 160, w, nw - 8)
                cftf081(a, offa + 176, w, nw - 8)
                If isplt <> 0 Then
                    cftmdl1(64, a, offa + 192, w, nw - 32)
                    cftf081(a, offa + 240, w, nw - 8)
                Else
                    cftmdl2(64, a, offa + 192, w, nw - 64)
                    cftf082(a, offa + 240, w, nw - 8)
                End If
                cftf081(a, offa + 192, w, nw - 8)
                cftf082(a, offa + 208, w, nw - 8)
                cftf081(a, offa + 224, w, nw - 8)
            End If
        End Sub

        Private Sub cftmdl1(n As Integer, a As Double(), offa As Integer, w As Double(), startw As Integer)
            Dim j0, j1, j2, j3, k, m, mh As Integer
            Dim wn4r, wk1r, wk1i, wk3r, wk3i As Double
            Dim x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i As Double
            Dim idx0, idx1, idx2, idx3, idx4, idx5 As Integer

            mh = n >> 3
            m = 2 * mh
            j1 = m
            j2 = j1 + m
            j3 = j2 + m
            idx1 = offa + j1
            idx2 = offa + j2
            idx3 = offa + j3
            x0r = a(offa) + a(idx2)
            x0i = a(offa + 1) + a(idx2 + 1)
            x1r = a(offa) - a(idx2)
            x1i = a(offa + 1) - a(idx2 + 1)
            x2r = a(idx1) + a(idx3)
            x2i = a(idx1 + 1) + a(idx3 + 1)
            x3r = a(idx1) - a(idx3)
            x3i = a(idx1 + 1) - a(idx3 + 1)
            a(offa) = x0r + x2r
            a(offa + 1) = x0i + x2i
            a(idx1) = x0r - x2r
            a(idx1 + 1) = x0i - x2i
            a(idx2) = x1r - x3i
            a(idx2 + 1) = x1i + x3r
            a(idx3) = x1r + x3i
            a(idx3 + 1) = x1i - x3r
            wn4r = w(startw + 1)
            k = 0
            For j = 2 To mh - 1 Step 2
                k += 4
                idx4 = startw + k
                wk1r = w(idx4)
                wk1i = w(idx4 + 1)
                wk3r = w(idx4 + 2)
                wk3i = w(idx4 + 3)
                j1 = j + m
                j2 = j1 + m
                j3 = j2 + m
                idx1 = offa + j1
                idx2 = offa + j2
                idx3 = offa + j3
                idx5 = offa + j
                x0r = a(idx5) + a(idx2)
                x0i = a(idx5 + 1) + a(idx2 + 1)
                x1r = a(idx5) - a(idx2)
                x1i = a(idx5 + 1) - a(idx2 + 1)
                x2r = a(idx1) + a(idx3)
                x2i = a(idx1 + 1) + a(idx3 + 1)
                x3r = a(idx1) - a(idx3)
                x3i = a(idx1 + 1) - a(idx3 + 1)
                a(idx5) = x0r + x2r
                a(idx5 + 1) = x0i + x2i
                a(idx1) = x0r - x2r
                a(idx1 + 1) = x0i - x2i
                x0r = x1r - x3i
                x0i = x1i + x3r
                a(idx2) = wk1r * x0r - wk1i * x0i
                a(idx2 + 1) = wk1r * x0i + wk1i * x0r
                x0r = x1r + x3i
                x0i = x1i - x3r
                a(idx3) = wk3r * x0r + wk3i * x0i
                a(idx3 + 1) = wk3r * x0i - wk3i * x0r
                j0 = m - j
                j1 = j0 + m
                j2 = j1 + m
                j3 = j2 + m
                idx0 = offa + j0
                idx1 = offa + j1
                idx2 = offa + j2
                idx3 = offa + j3
                x0r = a(idx0) + a(idx2)
                x0i = a(idx0 + 1) + a(idx2 + 1)
                x1r = a(idx0) - a(idx2)
                x1i = a(idx0 + 1) - a(idx2 + 1)
                x2r = a(idx1) + a(idx3)
                x2i = a(idx1 + 1) + a(idx3 + 1)
                x3r = a(idx1) - a(idx3)
                x3i = a(idx1 + 1) - a(idx3 + 1)
                a(idx0) = x0r + x2r
                a(idx0 + 1) = x0i + x2i
                a(idx1) = x0r - x2r
                a(idx1 + 1) = x0i - x2i
                x0r = x1r - x3i
                x0i = x1i + x3r
                a(idx2) = wk1i * x0r - wk1r * x0i
                a(idx2 + 1) = wk1i * x0i + wk1r * x0r
                x0r = x1r + x3i
                x0i = x1i - x3r
                a(idx3) = wk3i * x0r + wk3r * x0i
                a(idx3 + 1) = wk3i * x0i - wk3r * x0r
            Next
            j0 = mh
            j1 = j0 + m
            j2 = j1 + m
            j3 = j2 + m
            idx0 = offa + j0
            idx1 = offa + j1
            idx2 = offa + j2
            idx3 = offa + j3
            x0r = a(idx0) + a(idx2)
            x0i = a(idx0 + 1) + a(idx2 + 1)
            x1r = a(idx0) - a(idx2)
            x1i = a(idx0 + 1) - a(idx2 + 1)
            x2r = a(idx1) + a(idx3)
            x2i = a(idx1 + 1) + a(idx3 + 1)
            x3r = a(idx1) - a(idx3)
            x3i = a(idx1 + 1) - a(idx3 + 1)
            a(idx0) = x0r + x2r
            a(idx0 + 1) = x0i + x2i
            a(idx1) = x0r - x2r
            a(idx1 + 1) = x0i - x2i
            x0r = x1r - x3i
            x0i = x1i + x3r
            a(idx2) = wn4r * (x0r - x0i)
            a(idx2 + 1) = wn4r * (x0i + x0r)
            x0r = x1r + x3i
            x0i = x1i - x3r
            a(idx3) = -wn4r * (x0r + x0i)
            a(idx3 + 1) = -wn4r * (x0i - x0r)
        End Sub

        Private Sub cftmdl2(n As Integer, a As Double(), offa As Integer, w As Double(), startw As Integer)
            Dim j0, j1, j2, j3, k, kr, m, mh As Integer
            Dim wn4r, wk1r, wk1i, wk3r, wk3i, wd1r, wd1i, wd3r, wd3i As Double
            Dim x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i, y0r, y0i, y2r, y2i As Double
            Dim idx0, idx1, idx2, idx3, idx4, idx5, idx6 As Integer

            mh = n >> 3
            m = 2 * mh
            wn4r = w(startw + 1)
            j1 = m
            j2 = j1 + m
            j3 = j2 + m
            idx1 = offa + j1
            idx2 = offa + j2
            idx3 = offa + j3
            x0r = a(offa) - a(idx2 + 1)
            x0i = a(offa + 1) + a(idx2)
            x1r = a(offa) + a(idx2 + 1)
            x1i = a(offa + 1) - a(idx2)
            x2r = a(idx1) - a(idx3 + 1)
            x2i = a(idx1 + 1) + a(idx3)
            x3r = a(idx1) + a(idx3 + 1)
            x3i = a(idx1 + 1) - a(idx3)
            y0r = wn4r * (x2r - x2i)
            y0i = wn4r * (x2i + x2r)
            a(offa) = x0r + y0r
            a(offa + 1) = x0i + y0i
            a(idx1) = x0r - y0r
            a(idx1 + 1) = x0i - y0i
            y0r = wn4r * (x3r - x3i)
            y0i = wn4r * (x3i + x3r)
            a(idx2) = x1r - y0i
            a(idx2 + 1) = x1i + y0r
            a(idx3) = x1r + y0i
            a(idx3 + 1) = x1i - y0r
            k = 0
            kr = 2 * m
            For j = 2 To mh - 1 Step 2
                k += 4
                idx4 = startw + k
                wk1r = w(idx4)
                wk1i = w(idx4 + 1)
                wk3r = w(idx4 + 2)
                wk3i = w(idx4 + 3)
                kr -= 4
                idx5 = startw + kr
                wd1i = w(idx5)
                wd1r = w(idx5 + 1)
                wd3i = w(idx5 + 2)
                wd3r = w(idx5 + 3)
                j1 = j + m
                j2 = j1 + m
                j3 = j2 + m
                idx1 = offa + j1
                idx2 = offa + j2
                idx3 = offa + j3
                idx6 = offa + j
                x0r = a(idx6) - a(idx2 + 1)
                x0i = a(idx6 + 1) + a(idx2)
                x1r = a(idx6) + a(idx2 + 1)
                x1i = a(idx6 + 1) - a(idx2)
                x2r = a(idx1) - a(idx3 + 1)
                x2i = a(idx1 + 1) + a(idx3)
                x3r = a(idx1) + a(idx3 + 1)
                x3i = a(idx1 + 1) - a(idx3)
                y0r = wk1r * x0r - wk1i * x0i
                y0i = wk1r * x0i + wk1i * x0r
                y2r = wd1r * x2r - wd1i * x2i
                y2i = wd1r * x2i + wd1i * x2r
                a(idx6) = y0r + y2r
                a(idx6 + 1) = y0i + y2i
                a(idx1) = y0r - y2r
                a(idx1 + 1) = y0i - y2i
                y0r = wk3r * x1r + wk3i * x1i
                y0i = wk3r * x1i - wk3i * x1r
                y2r = wd3r * x3r + wd3i * x3i
                y2i = wd3r * x3i - wd3i * x3r
                a(idx2) = y0r + y2r
                a(idx2 + 1) = y0i + y2i
                a(idx3) = y0r - y2r
                a(idx3 + 1) = y0i - y2i
                j0 = m - j
                j1 = j0 + m
                j2 = j1 + m
                j3 = j2 + m
                idx0 = offa + j0
                idx1 = offa + j1
                idx2 = offa + j2
                idx3 = offa + j3
                x0r = a(idx0) - a(idx2 + 1)
                x0i = a(idx0 + 1) + a(idx2)
                x1r = a(idx0) + a(idx2 + 1)
                x1i = a(idx0 + 1) - a(idx2)
                x2r = a(idx1) - a(idx3 + 1)
                x2i = a(idx1 + 1) + a(idx3)
                x3r = a(idx1) + a(idx3 + 1)
                x3i = a(idx1 + 1) - a(idx3)
                y0r = wd1i * x0r - wd1r * x0i
                y0i = wd1i * x0i + wd1r * x0r
                y2r = wk1i * x2r - wk1r * x2i
                y2i = wk1i * x2i + wk1r * x2r
                a(idx0) = y0r + y2r
                a(idx0 + 1) = y0i + y2i
                a(idx1) = y0r - y2r
                a(idx1 + 1) = y0i - y2i
                y0r = wd3i * x1r + wd3r * x1i
                y0i = wd3i * x1i - wd3r * x1r
                y2r = wk3i * x3r + wk3r * x3i
                y2i = wk3i * x3i - wk3r * x3r
                a(idx2) = y0r + y2r
                a(idx2 + 1) = y0i + y2i
                a(idx3) = y0r - y2r
                a(idx3 + 1) = y0i - y2i
            Next
            wk1r = w(startw + m)
            wk1i = w(startw + m + 1)
            j0 = mh
            j1 = j0 + m
            j2 = j1 + m
            j3 = j2 + m
            idx0 = offa + j0
            idx1 = offa + j1
            idx2 = offa + j2
            idx3 = offa + j3
            x0r = a(idx0) - a(idx2 + 1)
            x0i = a(idx0 + 1) + a(idx2)
            x1r = a(idx0) + a(idx2 + 1)
            x1i = a(idx0 + 1) - a(idx2)
            x2r = a(idx1) - a(idx3 + 1)
            x2i = a(idx1 + 1) + a(idx3)
            x3r = a(idx1) + a(idx3 + 1)
            x3i = a(idx1 + 1) - a(idx3)
            y0r = wk1r * x0r - wk1i * x0i
            y0i = wk1r * x0i + wk1i * x0r
            y2r = wk1i * x2r - wk1r * x2i
            y2i = wk1i * x2i + wk1r * x2r
            a(idx0) = y0r + y2r
            a(idx0 + 1) = y0i + y2i
            a(idx1) = y0r - y2r
            a(idx1 + 1) = y0i - y2i
            y0r = wk1i * x1r - wk1r * x1i
            y0i = wk1i * x1i + wk1r * x1r
            y2r = wk1r * x3r - wk1i * x3i
            y2i = wk1r * x3i + wk1i * x3r
            a(idx2) = y0r - y2r
            a(idx2 + 1) = y0i - y2i
            a(idx3) = y0r + y2r
            a(idx3 + 1) = y0i + y2i
        End Sub

        Private Sub cftfx41(n As Integer, a As Double(), offa As Integer, nw As Integer, w As Double())
            If n = 128 Then
                cftf161(a, offa, w, nw - 8)
                cftf162(a, offa + 32, w, nw - 32)
                cftf161(a, offa + 64, w, nw - 8)
                cftf161(a, offa + 96, w, nw - 8)
            Else
                cftf081(a, offa, w, nw - 8)
                cftf082(a, offa + 16, w, nw - 8)
                cftf081(a, offa + 32, w, nw - 8)
                cftf081(a, offa + 48, w, nw - 8)
            End If
        End Sub

        Private Sub cftf161(a As Double(), offa As Integer, w As Double(), startw As Integer)
            Dim wn4r, wk1r, wk1i, x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i, y0r, y0i, y1r, y1i, y2r, y2i, y3r, y3i, y4r, y4i, y5r, y5i, y6r, y6i, y7r, y7i, y8r, y8i, y9r, y9i, y10r, y10i, y11r, y11i, y12r, y12i, y13r, y13i, y14r, y14i, y15r, y15i As Double

            wn4r = w(startw + 1)
            wk1r = w(startw + 2)
            wk1i = w(startw + 3)

            x0r = a(offa) + a(offa + 16)
            x0i = a(offa + 1) + a(offa + 17)
            x1r = a(offa) - a(offa + 16)
            x1i = a(offa + 1) - a(offa + 17)
            x2r = a(offa + 8) + a(offa + 24)
            x2i = a(offa + 9) + a(offa + 25)
            x3r = a(offa + 8) - a(offa + 24)
            x3i = a(offa + 9) - a(offa + 25)
            y0r = x0r + x2r
            y0i = x0i + x2i
            y4r = x0r - x2r
            y4i = x0i - x2i
            y8r = x1r - x3i
            y8i = x1i + x3r
            y12r = x1r + x3i
            y12i = x1i - x3r
            x0r = a(offa + 2) + a(offa + 18)
            x0i = a(offa + 3) + a(offa + 19)
            x1r = a(offa + 2) - a(offa + 18)
            x1i = a(offa + 3) - a(offa + 19)
            x2r = a(offa + 10) + a(offa + 26)
            x2i = a(offa + 11) + a(offa + 27)
            x3r = a(offa + 10) - a(offa + 26)
            x3i = a(offa + 11) - a(offa + 27)
            y1r = x0r + x2r
            y1i = x0i + x2i
            y5r = x0r - x2r
            y5i = x0i - x2i
            x0r = x1r - x3i
            x0i = x1i + x3r
            y9r = wk1r * x0r - wk1i * x0i
            y9i = wk1r * x0i + wk1i * x0r
            x0r = x1r + x3i
            x0i = x1i - x3r
            y13r = wk1i * x0r - wk1r * x0i
            y13i = wk1i * x0i + wk1r * x0r
            x0r = a(offa + 4) + a(offa + 20)
            x0i = a(offa + 5) + a(offa + 21)
            x1r = a(offa + 4) - a(offa + 20)
            x1i = a(offa + 5) - a(offa + 21)
            x2r = a(offa + 12) + a(offa + 28)
            x2i = a(offa + 13) + a(offa + 29)
            x3r = a(offa + 12) - a(offa + 28)
            x3i = a(offa + 13) - a(offa + 29)
            y2r = x0r + x2r
            y2i = x0i + x2i
            y6r = x0r - x2r
            y6i = x0i - x2i
            x0r = x1r - x3i
            x0i = x1i + x3r
            y10r = wn4r * (x0r - x0i)
            y10i = wn4r * (x0i + x0r)
            x0r = x1r + x3i
            x0i = x1i - x3r
            y14r = wn4r * (x0r + x0i)
            y14i = wn4r * (x0i - x0r)
            x0r = a(offa + 6) + a(offa + 22)
            x0i = a(offa + 7) + a(offa + 23)
            x1r = a(offa + 6) - a(offa + 22)
            x1i = a(offa + 7) - a(offa + 23)
            x2r = a(offa + 14) + a(offa + 30)
            x2i = a(offa + 15) + a(offa + 31)
            x3r = a(offa + 14) - a(offa + 30)
            x3i = a(offa + 15) - a(offa + 31)
            y3r = x0r + x2r
            y3i = x0i + x2i
            y7r = x0r - x2r
            y7i = x0i - x2i
            x0r = x1r - x3i
            x0i = x1i + x3r
            y11r = wk1i * x0r - wk1r * x0i
            y11i = wk1i * x0i + wk1r * x0r
            x0r = x1r + x3i
            x0i = x1i - x3r
            y15r = wk1r * x0r - wk1i * x0i
            y15i = wk1r * x0i + wk1i * x0r
            x0r = y12r - y14r
            x0i = y12i - y14i
            x1r = y12r + y14r
            x1i = y12i + y14i
            x2r = y13r - y15r
            x2i = y13i - y15i
            x3r = y13r + y15r
            x3i = y13i + y15i
            a(offa + 24) = x0r + x2r
            a(offa + 25) = x0i + x2i
            a(offa + 26) = x0r - x2r
            a(offa + 27) = x0i - x2i
            a(offa + 28) = x1r - x3i
            a(offa + 29) = x1i + x3r
            a(offa + 30) = x1r + x3i
            a(offa + 31) = x1i - x3r
            x0r = y8r + y10r
            x0i = y8i + y10i
            x1r = y8r - y10r
            x1i = y8i - y10i
            x2r = y9r + y11r
            x2i = y9i + y11i
            x3r = y9r - y11r
            x3i = y9i - y11i
            a(offa + 16) = x0r + x2r
            a(offa + 17) = x0i + x2i
            a(offa + 18) = x0r - x2r
            a(offa + 19) = x0i - x2i
            a(offa + 20) = x1r - x3i
            a(offa + 21) = x1i + x3r
            a(offa + 22) = x1r + x3i
            a(offa + 23) = x1i - x3r
            x0r = y5r - y7i
            x0i = y5i + y7r
            x2r = wn4r * (x0r - x0i)
            x2i = wn4r * (x0i + x0r)
            x0r = y5r + y7i
            x0i = y5i - y7r
            x3r = wn4r * (x0r - x0i)
            x3i = wn4r * (x0i + x0r)
            x0r = y4r - y6i
            x0i = y4i + y6r
            x1r = y4r + y6i
            x1i = y4i - y6r
            a(offa + 8) = x0r + x2r
            a(offa + 9) = x0i + x2i
            a(offa + 10) = x0r - x2r
            a(offa + 11) = x0i - x2i
            a(offa + 12) = x1r - x3i
            a(offa + 13) = x1i + x3r
            a(offa + 14) = x1r + x3i
            a(offa + 15) = x1i - x3r
            x0r = y0r + y2r
            x0i = y0i + y2i
            x1r = y0r - y2r
            x1i = y0i - y2i
            x2r = y1r + y3r
            x2i = y1i + y3i
            x3r = y1r - y3r
            x3i = y1i - y3i
            a(offa) = x0r + x2r
            a(offa + 1) = x0i + x2i
            a(offa + 2) = x0r - x2r
            a(offa + 3) = x0i - x2i
            a(offa + 4) = x1r - x3i
            a(offa + 5) = x1i + x3r
            a(offa + 6) = x1r + x3i
            a(offa + 7) = x1i - x3r
        End Sub

        Private Sub cftf162(a As Double(), offa As Integer, w As Double(), startw As Integer)
            Dim wn4r, wk1r, wk1i, wk2r, wk2i, wk3r, wk3i, x0r, x0i, x1r, x1i, x2r, x2i, y0r, y0i, y1r, y1i, y2r, y2i, y3r, y3i, y4r, y4i, y5r, y5i, y6r, y6i, y7r, y7i, y8r, y8i, y9r, y9i, y10r, y10i, y11r, y11i, y12r, y12i, y13r, y13i, y14r, y14i, y15r, y15i As Double

            wn4r = w(startw + 1)
            wk1r = w(startw + 4)
            wk1i = w(startw + 5)
            wk3r = w(startw + 6)
            wk3i = -w(startw + 7)
            wk2r = w(startw + 8)
            wk2i = w(startw + 9)
            x1r = a(offa) - a(offa + 17)
            x1i = a(offa + 1) + a(offa + 16)
            x0r = a(offa + 8) - a(offa + 25)
            x0i = a(offa + 9) + a(offa + 24)
            x2r = wn4r * (x0r - x0i)
            x2i = wn4r * (x0i + x0r)
            y0r = x1r + x2r
            y0i = x1i + x2i
            y4r = x1r - x2r
            y4i = x1i - x2i
            x1r = a(offa) + a(offa + 17)
            x1i = a(offa + 1) - a(offa + 16)
            x0r = a(offa + 8) + a(offa + 25)
            x0i = a(offa + 9) - a(offa + 24)
            x2r = wn4r * (x0r - x0i)
            x2i = wn4r * (x0i + x0r)
            y8r = x1r - x2i
            y8i = x1i + x2r
            y12r = x1r + x2i
            y12i = x1i - x2r
            x0r = a(offa + 2) - a(offa + 19)
            x0i = a(offa + 3) + a(offa + 18)
            x1r = wk1r * x0r - wk1i * x0i
            x1i = wk1r * x0i + wk1i * x0r
            x0r = a(offa + 10) - a(offa + 27)
            x0i = a(offa + 11) + a(offa + 26)
            x2r = wk3i * x0r - wk3r * x0i
            x2i = wk3i * x0i + wk3r * x0r
            y1r = x1r + x2r
            y1i = x1i + x2i
            y5r = x1r - x2r
            y5i = x1i - x2i
            x0r = a(offa + 2) + a(offa + 19)
            x0i = a(offa + 3) - a(offa + 18)
            x1r = wk3r * x0r - wk3i * x0i
            x1i = wk3r * x0i + wk3i * x0r
            x0r = a(offa + 10) + a(offa + 27)
            x0i = a(offa + 11) - a(offa + 26)
            x2r = wk1r * x0r + wk1i * x0i
            x2i = wk1r * x0i - wk1i * x0r
            y9r = x1r - x2r
            y9i = x1i - x2i
            y13r = x1r + x2r
            y13i = x1i + x2i
            x0r = a(offa + 4) - a(offa + 21)
            x0i = a(offa + 5) + a(offa + 20)
            x1r = wk2r * x0r - wk2i * x0i
            x1i = wk2r * x0i + wk2i * x0r
            x0r = a(offa + 12) - a(offa + 29)
            x0i = a(offa + 13) + a(offa + 28)
            x2r = wk2i * x0r - wk2r * x0i
            x2i = wk2i * x0i + wk2r * x0r
            y2r = x1r + x2r
            y2i = x1i + x2i
            y6r = x1r - x2r
            y6i = x1i - x2i
            x0r = a(offa + 4) + a(offa + 21)
            x0i = a(offa + 5) - a(offa + 20)
            x1r = wk2i * x0r - wk2r * x0i
            x1i = wk2i * x0i + wk2r * x0r
            x0r = a(offa + 12) + a(offa + 29)
            x0i = a(offa + 13) - a(offa + 28)
            x2r = wk2r * x0r - wk2i * x0i
            x2i = wk2r * x0i + wk2i * x0r
            y10r = x1r - x2r
            y10i = x1i - x2i
            y14r = x1r + x2r
            y14i = x1i + x2i
            x0r = a(offa + 6) - a(offa + 23)
            x0i = a(offa + 7) + a(offa + 22)
            x1r = wk3r * x0r - wk3i * x0i
            x1i = wk3r * x0i + wk3i * x0r
            x0r = a(offa + 14) - a(offa + 31)
            x0i = a(offa + 15) + a(offa + 30)
            x2r = wk1i * x0r - wk1r * x0i
            x2i = wk1i * x0i + wk1r * x0r
            y3r = x1r + x2r
            y3i = x1i + x2i
            y7r = x1r - x2r
            y7i = x1i - x2i
            x0r = a(offa + 6) + a(offa + 23)
            x0i = a(offa + 7) - a(offa + 22)
            x1r = wk1i * x0r + wk1r * x0i
            x1i = wk1i * x0i - wk1r * x0r
            x0r = a(offa + 14) + a(offa + 31)
            x0i = a(offa + 15) - a(offa + 30)
            x2r = wk3i * x0r - wk3r * x0i
            x2i = wk3i * x0i + wk3r * x0r
            y11r = x1r + x2r
            y11i = x1i + x2i
            y15r = x1r - x2r
            y15i = x1i - x2i
            x1r = y0r + y2r
            x1i = y0i + y2i
            x2r = y1r + y3r
            x2i = y1i + y3i
            a(offa) = x1r + x2r
            a(offa + 1) = x1i + x2i
            a(offa + 2) = x1r - x2r
            a(offa + 3) = x1i - x2i
            x1r = y0r - y2r
            x1i = y0i - y2i
            x2r = y1r - y3r
            x2i = y1i - y3i
            a(offa + 4) = x1r - x2i
            a(offa + 5) = x1i + x2r
            a(offa + 6) = x1r + x2i
            a(offa + 7) = x1i - x2r
            x1r = y4r - y6i
            x1i = y4i + y6r
            x0r = y5r - y7i
            x0i = y5i + y7r
            x2r = wn4r * (x0r - x0i)
            x2i = wn4r * (x0i + x0r)
            a(offa + 8) = x1r + x2r
            a(offa + 9) = x1i + x2i
            a(offa + 10) = x1r - x2r
            a(offa + 11) = x1i - x2i
            x1r = y4r + y6i
            x1i = y4i - y6r
            x0r = y5r + y7i
            x0i = y5i - y7r
            x2r = wn4r * (x0r - x0i)
            x2i = wn4r * (x0i + x0r)
            a(offa + 12) = x1r - x2i
            a(offa + 13) = x1i + x2r
            a(offa + 14) = x1r + x2i
            a(offa + 15) = x1i - x2r
            x1r = y8r + y10r
            x1i = y8i + y10i
            x2r = y9r - y11r
            x2i = y9i - y11i
            a(offa + 16) = x1r + x2r
            a(offa + 17) = x1i + x2i
            a(offa + 18) = x1r - x2r
            a(offa + 19) = x1i - x2i
            x1r = y8r - y10r
            x1i = y8i - y10i
            x2r = y9r + y11r
            x2i = y9i + y11i
            a(offa + 20) = x1r - x2i
            a(offa + 21) = x1i + x2r
            a(offa + 22) = x1r + x2i
            a(offa + 23) = x1i - x2r
            x1r = y12r - y14i
            x1i = y12i + y14r
            x0r = y13r + y15i
            x0i = y13i - y15r
            x2r = wn4r * (x0r - x0i)
            x2i = wn4r * (x0i + x0r)
            a(offa + 24) = x1r + x2r
            a(offa + 25) = x1i + x2i
            a(offa + 26) = x1r - x2r
            a(offa + 27) = x1i - x2i
            x1r = y12r + y14i
            x1i = y12i - y14r
            x0r = y13r - y15i
            x0i = y13i + y15r
            x2r = wn4r * (x0r - x0i)
            x2i = wn4r * (x0i + x0r)
            a(offa + 28) = x1r - x2i
            a(offa + 29) = x1i + x2r
            a(offa + 30) = x1r + x2i
            a(offa + 31) = x1i - x2r
        End Sub

        Private Sub cftf081(a As Double(), offa As Integer, w As Double(), startw As Integer)
            Dim wn4r, x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i, y0r, y0i, y1r, y1i, y2r, y2i, y3r, y3i, y4r, y4i, y5r, y5i, y6r, y6i, y7r, y7i As Double

            wn4r = w(startw + 1)
            x0r = a(offa) + a(offa + 8)
            x0i = a(offa + 1) + a(offa + 9)
            x1r = a(offa) - a(offa + 8)
            x1i = a(offa + 1) - a(offa + 9)
            x2r = a(offa + 4) + a(offa + 12)
            x2i = a(offa + 5) + a(offa + 13)
            x3r = a(offa + 4) - a(offa + 12)
            x3i = a(offa + 5) - a(offa + 13)
            y0r = x0r + x2r
            y0i = x0i + x2i
            y2r = x0r - x2r
            y2i = x0i - x2i
            y1r = x1r - x3i
            y1i = x1i + x3r
            y3r = x1r + x3i
            y3i = x1i - x3r
            x0r = a(offa + 2) + a(offa + 10)
            x0i = a(offa + 3) + a(offa + 11)
            x1r = a(offa + 2) - a(offa + 10)
            x1i = a(offa + 3) - a(offa + 11)
            x2r = a(offa + 6) + a(offa + 14)
            x2i = a(offa + 7) + a(offa + 15)
            x3r = a(offa + 6) - a(offa + 14)
            x3i = a(offa + 7) - a(offa + 15)
            y4r = x0r + x2r
            y4i = x0i + x2i
            y6r = x0r - x2r
            y6i = x0i - x2i
            x0r = x1r - x3i
            x0i = x1i + x3r
            x2r = x1r + x3i
            x2i = x1i - x3r
            y5r = wn4r * (x0r - x0i)
            y5i = wn4r * (x0r + x0i)
            y7r = wn4r * (x2r - x2i)
            y7i = wn4r * (x2r + x2i)
            a(offa + 8) = y1r + y5r
            a(offa + 9) = y1i + y5i
            a(offa + 10) = y1r - y5r
            a(offa + 11) = y1i - y5i
            a(offa + 12) = y3r - y7i
            a(offa + 13) = y3i + y7r
            a(offa + 14) = y3r + y7i
            a(offa + 15) = y3i - y7r
            a(offa) = y0r + y4r
            a(offa + 1) = y0i + y4i
            a(offa + 2) = y0r - y4r
            a(offa + 3) = y0i - y4i
            a(offa + 4) = y2r - y6i
            a(offa + 5) = y2i + y6r
            a(offa + 6) = y2r + y6i
            a(offa + 7) = y2i - y6r
        End Sub

        Private Sub cftf082(a As Double(), offa As Integer, w As Double(), startw As Integer)
            Dim wn4r, wk1r, wk1i, x0r, x0i, x1r, x1i, y0r, y0i, y1r, y1i, y2r, y2i, y3r, y3i, y4r, y4i, y5r, y5i, y6r, y6i, y7r, y7i As Double

            wn4r = w(startw + 1)
            wk1r = w(startw + 2)
            wk1i = w(startw + 3)
            y0r = a(offa) - a(offa + 9)
            y0i = a(offa + 1) + a(offa + 8)
            y1r = a(offa) + a(offa + 9)
            y1i = a(offa + 1) - a(offa + 8)
            x0r = a(offa + 4) - a(offa + 13)
            x0i = a(offa + 5) + a(offa + 12)
            y2r = wn4r * (x0r - x0i)
            y2i = wn4r * (x0i + x0r)
            x0r = a(offa + 4) + a(offa + 13)
            x0i = a(offa + 5) - a(offa + 12)
            y3r = wn4r * (x0r - x0i)
            y3i = wn4r * (x0i + x0r)
            x0r = a(offa + 2) - a(offa + 11)
            x0i = a(offa + 3) + a(offa + 10)
            y4r = wk1r * x0r - wk1i * x0i
            y4i = wk1r * x0i + wk1i * x0r
            x0r = a(offa + 2) + a(offa + 11)
            x0i = a(offa + 3) - a(offa + 10)
            y5r = wk1i * x0r - wk1r * x0i
            y5i = wk1i * x0i + wk1r * x0r
            x0r = a(offa + 6) - a(offa + 15)
            x0i = a(offa + 7) + a(offa + 14)
            y6r = wk1i * x0r - wk1r * x0i
            y6i = wk1i * x0i + wk1r * x0r
            x0r = a(offa + 6) + a(offa + 15)
            x0i = a(offa + 7) - a(offa + 14)
            y7r = wk1r * x0r - wk1i * x0i
            y7i = wk1r * x0i + wk1i * x0r
            x0r = y0r + y2r
            x0i = y0i + y2i
            x1r = y4r + y6r
            x1i = y4i + y6i
            a(offa) = x0r + x1r
            a(offa + 1) = x0i + x1i
            a(offa + 2) = x0r - x1r
            a(offa + 3) = x0i - x1i
            x0r = y0r - y2r
            x0i = y0i - y2i
            x1r = y4r - y6r
            x1i = y4i - y6i
            a(offa + 4) = x0r - x1i
            a(offa + 5) = x0i + x1r
            a(offa + 6) = x0r + x1i
            a(offa + 7) = x0i - x1r
            x0r = y1r - y3i
            x0i = y1i + y3r
            x1r = y5r - y7r
            x1i = y5i - y7i
            a(offa + 8) = x0r + x1r
            a(offa + 9) = x0i + x1i
            a(offa + 10) = x0r - x1r
            a(offa + 11) = x0i - x1i
            x0r = y1r + y3i
            x0i = y1i - y3r
            x1r = y5r + y7r
            x1i = y5i + y7i
            a(offa + 12) = x0r - x1i
            a(offa + 13) = x0i + x1r
            a(offa + 14) = x0r + x1i
            a(offa + 15) = x0i - x1r
        End Sub

        Private Sub cftf040(a As Double(), offa As Integer)
            Dim x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i As Double

            x0r = a(offa) + a(offa + 4)
            x0i = a(offa + 1) + a(offa + 5)
            x1r = a(offa) - a(offa + 4)
            x1i = a(offa + 1) - a(offa + 5)
            x2r = a(offa + 2) + a(offa + 6)
            x2i = a(offa + 3) + a(offa + 7)
            x3r = a(offa + 2) - a(offa + 6)
            x3i = a(offa + 3) - a(offa + 7)
            a(offa) = x0r + x2r
            a(offa + 1) = x0i + x2i
            a(offa + 2) = x1r - x3i
            a(offa + 3) = x1i + x3r
            a(offa + 4) = x0r - x2r
            a(offa + 5) = x0i - x2i
            a(offa + 6) = x1r + x3i
            a(offa + 7) = x1i - x3r
        End Sub

        Private Sub cftb040(a As Double(), offa As Integer)
            Dim x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i As Double

            x0r = a(offa) + a(offa + 4)
            x0i = a(offa + 1) + a(offa + 5)
            x1r = a(offa) - a(offa + 4)
            x1i = a(offa + 1) - a(offa + 5)
            x2r = a(offa + 2) + a(offa + 6)
            x2i = a(offa + 3) + a(offa + 7)
            x3r = a(offa + 2) - a(offa + 6)
            x3i = a(offa + 3) - a(offa + 7)
            a(offa) = x0r + x2r
            a(offa + 1) = x0i + x2i
            a(offa + 2) = x1r + x3i
            a(offa + 3) = x1i - x3r
            a(offa + 4) = x0r - x2r
            a(offa + 5) = x0i - x2i
            a(offa + 6) = x1r - x3i
            a(offa + 7) = x1i + x3r
        End Sub

        Private Sub cftx020(a As Double(), offa As Integer)
            Dim x0r, x0i As Double
            x0r = a(offa) - a(offa + 2)
            x0i = -a(offa + 1) + a(offa + 3)
            a(offa) += a(offa + 2)
            a(offa + 1) += a(offa + 3)
            a(offa + 2) = x0r
            a(offa + 3) = x0i
        End Sub

        Private Sub cftxb020(a As Double(), offa As Integer)
            Dim x0r, x0i As Double

            x0r = a(offa) - a(offa + 2)
            x0i = a(offa + 1) - a(offa + 3)
            a(offa) += a(offa + 2)
            a(offa + 1) += a(offa + 3)
            a(offa + 2) = x0r
            a(offa + 3) = x0i
        End Sub

        Private Sub cftxc020(a As Double(), offa As Integer)
            Dim x0r, x0i As Double
            x0r = a(offa) - a(offa + 2)
            x0i = a(offa + 1) + a(offa + 3)
            a(offa) += a(offa + 2)
            a(offa + 1) -= a(offa + 3)
            a(offa + 2) = x0r
            a(offa + 3) = x0i
        End Sub

        Private Sub rftfsub(n As Integer, a As Double(), offa As Integer, nc As Integer, c As Double(), startc As Integer)
            Dim k, kk, ks, m As Integer
            Dim wkr, wki, xr, xi, yr, yi As Double
            Dim idx1, idx2 As Integer

            m = n >> 1
            ks = 2 * nc / m
            kk = 0
            For j = 2 To m - 1 Step 2
                k = n - j
                kk += ks
                wkr = 0.5 - c(startc + nc - kk)
                wki = c(startc + kk)
                idx1 = offa + j
                idx2 = offa + k
                xr = a(idx1) - a(idx2)
                xi = a(idx1 + 1) + a(idx2 + 1)
                yr = wkr * xr - wki * xi
                yi = wkr * xi + wki * xr
                a(idx1) -= yr
                a(idx1 + 1) = yi - a(idx1 + 1)
                a(idx2) += yr
                a(idx2 + 1) = yi - a(idx2 + 1)
            Next
            a(offa + m + 1) = -a(offa + m + 1)
        End Sub

        Private Sub rftbsub(n As Integer, a As Double(), offa As Integer, nc As Integer, c As Double(), startc As Integer)
            Dim k, kk, ks, m As Integer
            Dim wkr, wki, xr, xi, yr, yi As Double
            Dim idx1, idx2 As Integer

            m = n >> 1
            ks = 2 * nc / m
            kk = 0
            For j = 2 To m - 1 Step 2
                k = n - j
                kk += ks
                wkr = 0.5 - c(startc + nc - kk)
                wki = c(startc + kk)
                idx1 = offa + j
                idx2 = offa + k
                xr = a(idx1) - a(idx2)
                xi = a(idx1 + 1) + a(idx2 + 1)
                yr = wkr * xr - wki * xi
                yi = wkr * xi + wki * xr
                a(idx1) -= yr
                a(idx1 + 1) -= yi
                a(idx2) += yr
                a(idx2 + 1) -= yi
            Next
        End Sub

        Private Sub scale(m As Double, a As Double(), offa As Integer, complex As Boolean)
            Dim norm = 1.0 / m
            Dim n2 As Integer
            If complex Then
                n2 = 2 * n
            Else
                n2 = n
            End If
            Dim nthreads As Integer = Process.GetCurrentProcess().Threads.Count
            If nthreads > 1 AndAlso n2 >= TransformCore.THREADS_BEGIN_N_1D_FFT_2THREADS Then
                Dim k As Integer = n2 / nthreads
                Dim taskArray = New Task(nthreads - 1) {}
                For i = 0 To nthreads - 1
                    Dim firstIdx = offa + i * k
                    Dim lastIdx = If(i = nthreads - 1, offa + n2, firstIdx + k)
                    taskArray(i) = Task.Factory.StartNew(Sub()
                                                             For idx = firstIdx To lastIdx - 1
                                                                 a(idx) *= norm
                                                             Next

                                                         End Sub)
                Next
                Try
                    Task.WaitAll(taskArray)
                Finally

                End Try
            Else
                For i = offa To offa + n2 - 1
                    a(i) *= norm
                Next

            End If
        End Sub
#End Region
    End Class
End Namespace
