﻿Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.ListExtensions
Imports System.Math

' 
'  Copyright [2009] [Marcin Rzeźnicki]
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
' http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' 

Namespace Filters

    ''' <summary>
    ''' Savitzky-Golay filter implementation. For more information see
    ''' http://www.nrbook.com/a/bookcpdf/c14-8.pdf. This implementation,
    ''' however, does not use FFT
    ''' 
    ''' @author Marcin Rzeźnicki
    ''' 
    ''' </summary>
    Public Class SGFilter

        ''' <summary>
        ''' Computes Savitzky-Golay coefficients for given parameters
        ''' </summary>
        ''' <paramname="nl">
        '''            numer of past data points filter will use </param>
        ''' <paramname="nr">
        '''            number of future data points filter will use </param>
        ''' <paramname="degree">
        '''            order of smoothin polynomial </param>
        ''' <returns> Savitzky-Golay coefficients </returns>
        ''' <exceptioncref="IllegalArgumentException">
        '''             if {@code nl </> </> <degree/> </exception>
        Public Shared Function computeSGCoefficients(nl As Integer, nr As Integer, degree As Integer) As Double()
            If nl < 0 OrElse nr < 0 OrElse nl + nr < degree Then
                Throw New ArgumentException("Bad arguments")
            End If
            Dim matrix As NumericMatrix = New NumericMatrix(degree + 1, degree + 1)
            Dim a = matrix.Array
            Dim sum As Double
            For i = 0 To degree
                For j = 0 To degree
                    sum = If(i = 0 AndAlso j = 0, 1, 0)
                    For k = 1 To nr
                        sum += Pow(k, i + j)
                    Next
                    For k = 1 To nl
                        sum += Pow(-k, i + j)
                    Next
                    a(i)(j) = sum
                Next
            Next
            Dim b = New Double(degree + 1 - 1) {}
            b(0) = 1
            b = matrix.Solve(b)
            Dim coeffs = New Double(nl + nr + 1 - 1) {}
            For n = -nl To nr
                sum = b(0)
                For m = 1 To degree
                    sum += b(m) * Pow(n, m)
                Next
                coeffs(n + nl) = sum
            Next
            Return coeffs
        End Function

        Private Shared Sub convertDoubleArrayToFloat([in] As Double(), out As Single())
            For i = 0 To [in].Length - 1
                out(i) = CSng([in](i))
            Next
        End Sub

        Private Shared Sub convertFloatArrayToDouble([in] As Single(), out As Double())
            For i = 0 To [in].Length - 1
                out(i) = [in](i)
            Next
        End Sub

        Private ReadOnly dataFilters As List(Of DataFilter) = New List(Of DataFilter)()

        Private nlField As Integer

        Private nrField As Integer

        Private ReadOnly preprocessors As List(Of Preprocessor) = New List(Of Preprocessor)()

        ''' <summary>
        ''' Constructs Savitzky-Golay filter which uses specified numebr of
        ''' surrounding data points
        ''' </summary>
        ''' <paramname="nl">
        '''            numer of past data points filter will use </param>
        ''' <paramname="nr">
        '''            numer of future data points filter will use </param>
        ''' <exceptioncref="IllegalArgumentException">
        '''             of {@code nl </> </> </exception>
        Public Sub New(nl As Integer, nr As Integer)
            If nl < 0 OrElse nr < 0 Then
                Throw New ArgumentException("Bad arguments")
            End If
            nlField = nl
            nrField = nr
        End Sub

        ''' <summary>
        ''' Appends data filter
        ''' </summary>
        ''' <paramname="dataFilter">
        '''            dataFilter </param>
        ''' <seealsocref="DataFilter"/> </seealso>
        Public Overridable Sub appendDataFilter(dataFilter As DataFilter)
            dataFilters.Add(dataFilter)
        End Sub

        ''' <summary>
        ''' Appends data preprocessor
        ''' </summary>
        ''' <paramname="p">
        '''            preprocessor </param>
        ''' <seealsocref="Preprocessor"/> </seealso>
        Public Overridable Sub appendPreprocessor(p As Preprocessor)
            preprocessors.Add(p)
        End Sub

        ''' 
        ''' <returns> number of past data points that this filter uses </returns>
        Public Overridable Property Nl As Integer
            Get
                Return nlField
            End Get
            Set(value As Integer)
                If value < 0 Then
                    Throw New ArgumentException("nl < 0")
                End If
                nlField = value
            End Set
        End Property

        ''' 
        ''' <returns> number of future data points that this filter uses </returns>
        Public Overridable Property Nr As Integer
            Get
                Return nrField
            End Get
            Set(value As Integer)
                If value < 0 Then
                    Throw New ArgumentException("nr < 0")
                End If
                nrField = value
            End Set
        End Property

        ''' <summary>
        ''' Inserts data filter
        ''' </summary>
        ''' <paramname="dataFilter">
        '''            data filter </param>
        ''' <paramname="index">
        '''            where it should be placed in data filters queue </param>
        ''' <seealsocref="DataFilter"/> </seealso>
        Public Overridable Sub insertDataFilter(dataFilter As DataFilter, index As Integer)
            dataFilters.Insert(index, dataFilter)
        End Sub

        ''' <summary>
        ''' Inserts preprocessor
        ''' </summary>
        ''' <paramname="p">
        '''            preprocessor </param>
        ''' <paramname="index">
        '''            where it should be placed in preprocessors queue </param>
        ''' <seealsocref="Preprocessor"/> </seealso>
        Public Overridable Sub insertPreprocessor(p As Preprocessor, index As Integer)
            preprocessors.Insert(index, p)
        End Sub

        ''' <summary>
        ''' Removes data filter
        ''' </summary>
        ''' <paramname="dataFilter">
        '''            data filter to be removed </param>
        ''' <returns> {@code true} if data filter existed and was removed, {@code
        '''         false} otherwise </returns>
        Public Overridable Function removeDataFilter(dataFilter As DataFilter) As Boolean
            Return dataFilters.Remove(dataFilter)
        End Function

        ''' <summary>
        ''' Removes data filter
        ''' </summary>
        ''' <paramname="index">
        '''            which data filter to remove </param>
        ''' <returns> removed data filter </returns>
        Public Overridable Function removeDataFilter(index As Integer) As DataFilter
            Return dataFilters.PopAt(index)
        End Function

        ''' <summary>
        ''' Removes preprocessor
        ''' </summary>
        ''' <paramname="index">
        '''            which preprocessor to remove </param>
        ''' <returns> removed preprocessor </returns>
        Public Overridable Function removePreprocessor(index As Integer) As Preprocessor
            Return preprocessors.PopAt(index)
        End Function

        ''' <summary>
        ''' Removes preprocessor
        ''' </summary>
        ''' <paramname="p">
        '''            preprocessor to be removed </param>
        ''' <returns> {@code true} if preprocessor existed and was removed, {@code
        '''         false} otherwise </returns>
        Public Overridable Function removePreprocessor(p As Preprocessor) As Boolean
            Return preprocessors.Remove(p)
        End Function



        ''' <summary>
        ''' Smooths data by using Savitzky-Golay filter. This method will use 0 for
        ''' any element beyond {@code data} which will be needed for computation (you
        ''' may want to use some <seealsocref="Preprocessor"/>)
        ''' </summary>
        ''' <paramname="data">
        '''            data for filter </param>
        ''' <paramname="coeffs">
        '''            filter coefficients </param>
        ''' <returns> filtered data </returns>
        ''' <exceptioncref="NullPointerException">
        '''             when any array passed as parameter is null </exception>
        Public Overridable Function smooth(data As Double(), coeffs As Double()) As Double()
            Return smooth(data, 0, data.Length, coeffs)
        End Function

        ''' <summary>
        ''' Smooths data by using Savitzky-Golay filter. Smoothing uses {@code
        ''' leftPad} and/or {@code rightPad} if you want to augment data on
        ''' boundaries to achieve smoother results for your purpose. If you do not
        ''' need this feature you may pass empty arrays (filter will use 0s in this
        ''' place, so you may want to use appropriate preprocessor)
        ''' </summary>
        ''' <paramname="data">
        '''            data for filter </param>
        ''' <paramname="leftPad">
        '''            left padding </param>
        ''' <paramname="rightPad">
        '''            right padding </param>
        ''' <paramname="coeffs">
        '''            filter coefficients </param>
        ''' <returns> filtered data </returns>
        ''' <exceptioncref="NullPointerException">
        '''             when any array passed as parameter is null </exception>
        Public Overridable Function smooth(data As Double(), leftPad As Double(), rightPad As Double(), coeffs As Double()) As Double()
            Return smooth(data, leftPad, rightPad, 0, New Double()() {coeffs})
        End Function

        ''' <summary>
        ''' Smooths data by using Savitzky-Golay filter. Smoothing uses {@code
        ''' leftPad} and/or {@code rightPad} if you want to augment data on
        ''' boundaries to achieve smoother results for your purpose. If you do not
        ''' need this feature you may pass empty arrays (filter will use 0s in this
        ''' place, so you may want to use appropriate preprocessor). If you want to
        ''' use different (probably non-symmetrical) filter near both ends of
        ''' (padded) data, you will be using {@code bias} and {@code coeffs}. {@code
        ''' bias} essentially means
        ''' "how many points of pad should be left out when smoothing". Filters
        ''' taking this condition into consideration are passed in {@code coeffs}.
        ''' <tt>coeffs[0]</tt> is used for unbiased data (that is, for
        ''' <tt>data[bias]..data[data.length-bias-1]</tt>). Its length has to be
        ''' <tt>nr + nl + 1</tt>. Filters from range
        ''' <tt>coeffs[coeffs.length - 1]</tt> to
        ''' <tt>coeffs[coeffs.length - bias]</tt> are used for smoothing first
        ''' {@code bias} points (that is, from <tt>data[0]</tt> to
        ''' <tt>data[bias]</tt>) correspondingly. Filters from range
        ''' <tt>coeffs[1]</tt> to <tt>coeffs[bias]</tt> are used for smoothing last
        ''' {@code bias} points (that is, for
        ''' <tt>data[data.length-bias]..data[data.length-1]</tt>). For example, if
        ''' you use 5 past points and 5 future points for smoothing, but have only 3
        ''' meaningful padding points - you would use {@code bias} equal to 2 and
        ''' would pass in {@code coeffs} param filters taking 5-5 points (for regular
        ''' smoothing), 5-4, 5-3 (for rightmost range of data) and 3-5, 4-5 (for
        ''' leftmost range). If you do not wish to use pads completely for
        ''' symmetrical filter then you should pass <tt>bias = nl = nr</tt>
        ''' </summary>
        ''' <paramname="data">
        '''            data for filter </param>
        ''' <paramname="leftPad">
        '''            left padding </param>
        ''' <paramname="rightPad">
        '''            right padding </param>
        ''' <paramname="bias">
        '''            how many points of pad should be left out when smoothing </param>
        ''' <paramname="coeffs">
        '''            array of filter coefficients </param>
        ''' <returns> filtered data </returns>
        ''' <exceptioncref="IllegalArgumentException">
        '''             when <tt>bias </></tt> or <tt>bias > min(nr, nl)</tt> </exception>
        ''' <exceptioncref="IndexOutOfBoundsException">
        '''             when {@code coeffs} has less than <tt>2*bias + 1</tt>
        '''             elements </exception>
        ''' <exceptioncref="NullPointerException">
        '''             when any array passed as parameter is null </exception>
        Public Overridable Function smooth(data As Double(), leftPad As Double(), rightPad As Double(), bias As Integer, coeffs As Double()()) As Double()
            If bias < 0 OrElse bias > nrField OrElse bias > nlField Then
                Throw New ArgumentException("bias < 0 or bias > nr or bias > nl")
            End If
            For Each dataFilter In dataFilters
                data = dataFilter.filter(data)
            Next
            Dim dataLength = data.Length
            If dataLength = 0 Then
                Return data
            End If
            Dim n = dataLength + nlField + nrField
            Dim dataCopy = New Double(n - 1) {}
            ' copy left pad reversed
            Dim leftPadOffset = nlField - leftPad.Length
            If leftPadOffset >= 0 Then
                For i = 0 To leftPad.Length - 1
                    dataCopy(leftPadOffset + i) = leftPad(i)
                Next
            Else
                For i = 0 To nlField - 1
                    dataCopy(i) = leftPad(i - leftPadOffset)
                Next
            End If
            ' copy actual data
            For i = 0 To dataLength - 1
                dataCopy(i + nlField) = data(i)
            Next
            ' copy right pad
            Dim rightPadOffset = nrField - rightPad.Length
            If rightPadOffset >= 0 Then
                For i = 0 To rightPad.Length - 1
                    dataCopy(i + dataLength + nlField) = rightPad(i)
                Next
            Else
                For i = 0 To nrField - 1
                    dataCopy(i + dataLength + nlField) = rightPad(i)
                Next
            End If
            For Each p In preprocessors
                p.apply(dataCopy)
            Next
            ' convolution (with savitzky-golay coefficients)
            Dim sdata = New Double(dataLength - 1) {}
            Dim sg As Double()
            For b = bias To 1 Step -1
                sg = coeffs(coeffs.Length - b)
                Dim x = nlField + bias - b
                Dim sum As Double = 0
                For i = -nlField + b To nrField
                    sum += dataCopy(x + i) * sg(nlField - b + i)
                Next
                sdata(x - nlField) = sum
            Next
            sg = coeffs(0)
            For X = nlField + bias To n - nrField - bias - 1
                Dim sum As Double = 0
                For i = -nlField To nrField
                    sum += dataCopy(X() + i) * sg(nlField + i)
                Next
                sdata(X() - nlField) = sum
            Next
            For b = 1 To bias
                sg = coeffs(b)
                Dim x = n - nrField - bias + (b - 1)
                Dim sum As Double = 0
                For i = -nlField To nrField - b
                    sum += dataCopy(x + i) * sg(nlField + i)
                Next
                sdata(x - nlField) = sum
            Next
            Return sdata
        End Function

        ''' <summary>
        ''' Runs filter on data from {@code from} (including) to {@code to}
        ''' (excluding). Data beyond range spanned by {@code from} and {@code to}
        ''' will be used for padding
        ''' </summary>
        ''' <paramname="data">
        '''            data for filter </param>
        ''' <paramname="from">
        '''            inedx of the first element of data </param>
        ''' <paramname="to">
        '''            index of the first element omitted </param>
        ''' <paramname="coeffs">
        '''            filter coefficients </param>
        ''' <returns> filtered data </returns>
        ''' <exceptioncref="ArrayIndexOutOfBoundsException">
        '''             if <tt>to > data.length</tt> </exception>
        ''' <exceptioncref="IllegalArgumentException">
        '''             if <tt>from </></tt> or <tt>to > data.length</tt> </exception>
        ''' <exceptioncref="NullPointerException">
        '''             if {@code data} is null or {@code coeffs} is null </exception>
        Public Overridable Function smooth(data As Double(), from As Integer, [to] As Integer, coeffs As Double()) As Double()
            Return smooth(data, from, [to], 0, New Double()() {coeffs})
        End Function

        ''' <summary>
        ''' Runs filter on data from {@code from} (including) to {@code to}
        ''' (excluding). Data beyond range spanned by {@code from} and {@code to}
        ''' will be used for padding. See
        ''' <seealsocref="smooth(,,,Integer,)"/> for usage
        ''' of {@code bias}
        ''' </summary>
        ''' <paramname="data">
        '''            data for filter </param>
        ''' <paramname="from">
        '''            inedx of the first element of data </param>
        ''' <paramname="to">
        '''            index of the first element omitted </param>
        ''' <paramname="bias">
        '''            how many points of pad should be left out when smoothing </param>
        ''' <paramname="coeffs">
        '''            filter coefficients </param>
        ''' <returns> filtered data </returns>
        ''' <exceptioncref="ArrayIndexOutOfBoundsException">
        '''             if <tt>to > data.length</tt> or when {@code coeffs} has less
        '''             than <tt>2*bias + 1</tt> elements </exception>
        ''' <exceptioncref="IllegalArgumentException">
        '''             if <tt>from </></tt> or <tt>to > data.length</tt> or
        '''             <tt>from > to</tt> or when <tt>bias </></tt> or
        '''             <tt>bias > min(nr, nl)</tt> </exception>
        ''' <exceptioncref="NullPointerException">
        '''             if {@code data} is null or {@code coeffs} is null </exception>
        Public Overridable Function smooth(data As Double(), from As Integer, [to] As Integer, bias As Integer, coeffs As Double()()) As Double()
            Dim leftPad = copyOfRange(data, 0, from)
            Dim rightPad = copyOfRange(data, [to], data.Length)
            Dim dataCopy = copyOfRange(data, from, [to])
            Return smooth(dataCopy, leftPad, rightPad, bias, coeffs)
        End Function

        ''' <summary>
        ''' See <seealsocref="smooth(,)"/>. This method converts {@code
        ''' data} to double for computation and then converts it back to float
        ''' </summary>
        ''' <paramname="data">
        '''            data for filter </param>
        ''' <paramname="coeffs">
        '''            filter coefficients </param>
        ''' <returns> filtered data </returns>
        ''' <exceptioncref="NullPointerException">
        '''             when any array passed as parameter is null </exception>
        Public Overridable Function smooth(data As Single(), coeffs As Double()) As Single()
            Return smooth(data, 0, data.Length, coeffs)
        End Function

        ''' <summary>
        ''' See <seealsocref="smooth(,,,)"/>. This method
        ''' converts {@code data} {@code leftPad} and {@code rightPad} to double for
        ''' computation and then converts back to float
        ''' </summary>
        ''' <paramname="data">
        '''            data for filter </param>
        ''' <paramname="leftPad">
        '''            left padding </param>
        ''' <paramname="rightPad">
        '''            right padding </param>
        ''' <paramname="coeffs">
        '''            filter coefficients </param>
        ''' <returns> filtered data </returns>
        ''' <exceptioncref="NullPointerException">
        '''             when any array passed as parameter is null </exception>
        Public Overridable Function smooth(data As Single(), leftPad As Single(), rightPad As Single(), coeffs As Double()) As Single()
            Return smooth(data, leftPad, rightPad, 0, New Double()() {coeffs})
        End Function

        ''' <summary>
        ''' See <seealsocref="smooth(,,,Integer,)"/>. This
        ''' method converts {@code data} {@code leftPad} and {@code rightPad} to
        ''' double for computation and then converts back to float
        ''' </summary>
        ''' <paramname="data">
        '''            data for filter </param>
        ''' <paramname="leftPad">
        '''            left padding </param>
        ''' <paramname="rightPad">
        '''            right padding </param>
        ''' <paramname="bias">
        '''            how many points of pad should be left out when smoothing </param>
        ''' <paramname="coeffs">
        '''            array of filter coefficients </param>
        ''' <returns> filtered data </returns>
        ''' <exceptioncref="IllegalArgumentException">
        '''             when <tt>bias </></tt> or <tt>bias > min(nr, nl)</tt> </exception>
        ''' <exceptioncref="IndexOutOfBoundsException">
        '''             when {@code coeffs} has less than <tt>2*bias + 1</tt>
        '''             elements </exception>
        ''' <exceptioncref="NullPointerException">
        '''             when any array passed as parameter is null </exception>
        Public Overridable Function smooth(data As Single(), leftPad As Single(), rightPad As Single(), bias As Integer, coeffs As Double()()) As Single()
            Dim dataAsDouble = New Double(data.Length - 1) {}
            Dim leftPadAsDouble = New Double(leftPad.Length - 1) {}
            Dim rightPadAsDouble = New Double(rightPad.Length - 1) {}
            convertFloatArrayToDouble(data, dataAsDouble)
            convertFloatArrayToDouble(leftPad, leftPadAsDouble)
            convertFloatArrayToDouble(rightPad, rightPadAsDouble)
            Dim results = smooth(dataAsDouble, leftPadAsDouble, rightPadAsDouble, bias, coeffs)
            Dim resultsAsFloat = New Single(results.Length - 1) {}
            convertDoubleArrayToFloat(results, resultsAsFloat)
            Return resultsAsFloat
        End Function

        ''' <summary>
        ''' See <seealsocref="smooth(,Integer,Integer,)"/>. This method converts
        ''' {@code data} to double for computation and then converts it back to float
        ''' </summary>
        ''' <paramname="data">
        '''            data for filter </param>
        ''' <paramname="from">
        '''            inedx of the first element of data </param>
        ''' <paramname="to">
        '''            index of the first element omitted </param>
        ''' <paramname="coeffs">
        '''            filter coefficients </param>
        ''' <returns> filtered data </returns>
        ''' <exceptioncref="ArrayIndexOutOfBoundsException">
        '''             if <tt>to > data.length</tt> </exception>
        ''' <exceptioncref="IllegalArgumentException">
        '''             if <tt>from </></tt> or <tt>to > data.length</tt> </exception>
        ''' <exceptioncref="NullPointerException">
        '''             if {@code data} is null or {@code coeffs} is null </exception>
        Public Overridable Function smooth(data As Single(), from As Integer, [to] As Integer, coeffs As Double()) As Single()
            Return smooth(data, from, [to], 0, New Double()() {coeffs})
        End Function

        ''' <summary>
        ''' See <seealsocref="smooth(,Integer,Integer,Integer,)"/>. This method
        ''' converts {@code data} to double for computation and then converts it back
        ''' to float
        ''' </summary>
        ''' <paramname="data">
        '''            data for filter </param>
        ''' <paramname="from">
        '''            inedx of the first element of data </param>
        ''' <paramname="to">
        '''            index of the first element omitted </param>
        ''' <paramname="bias">
        '''            how many points of pad should be left out when smoothing </param>
        ''' <paramname="coeffs">
        '''            filter coefficients </param>
        ''' <returns> filtered data </returns>
        ''' <exceptioncref="ArrayIndexOutOfBoundsException">
        '''             if <tt>to > data.length</tt> or when {@code coeffs} has less
        '''             than <tt>2*bias + 1</tt> elements </exception>
        ''' <exceptioncref="IllegalArgumentException">
        '''             if <tt>from </></tt> or <tt>to > data.length</tt> or
        '''             <tt>from > to</tt> or when <tt>bias </></tt> or
        '''             <tt>bias > min(nr, nl)</tt> </exception>
        ''' <exceptioncref="NullPointerException">
        '''             if {@code data} is null or {@code coeffs} is null </exception>
        Public Overridable Function smooth(data As Single(), from As Integer, [to] As Integer, bias As Integer, coeffs As Double()()) As Single()
            Dim leftPad = copyOfRange(data, 0, from)
            Dim rightPad = copyOfRange(data, [to], data.Length)
            Dim dataCopy = copyOfRange(data, from, [to])
            Return smooth(dataCopy, leftPad, rightPad, bias, coeffs)
        End Function
    End Class

End Namespace
