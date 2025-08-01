#Region "Microsoft.VisualBasic::e1ccd6abd0347058a0a3f8e1dd9e1c7d, Data_science\Mathematica\Math\Math\Numerics\BigDecimal.vb"

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

    '   Total Lines: 647
    '    Code Lines: 531 (82.07%)
    ' Comment Lines: 61 (9.43%)
    '    - Xml Docs: 18.03%
    ' 
    '   Blank Lines: 55 (8.50%)
    '     File Size: 26.84 KB


    '     Structure BigDecimal
    ' 
    '         Properties: IntegralLength, IsEven, IsOne, IsPowerOfTwo, IsZero
    '                     MantissaLength, Sign
    ' 
    '         Constructor: (+15 Overloads) Sub New
    ' 
    '         Function: CompareTo, Div, Phi, Pi, Pow
    '                   Pow10, (+2 Overloads) PowN10, PythagorasConst, Sqrt, Tau
    '                   ToByteArray, (+2 Overloads) ToString
    ' 
    '         Sub: Parse
    ' 
    '         Operators: -, *, /, ^, +
    '                    <, <=, <>, =, >
    '                    >=, (+2 Overloads) Mod
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports std = System.Math

Namespace Numerics

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' http://tizzyt-archive.blogspot.com/2015/03/quick-and-dirty-net-bigdecimal.html
    ''' </remarks>
    ''' 
    <FrameworkConfig(BigDecimal.BigDecimalPrecisionEnvironmentConfigName)>
    Public Structure BigDecimal

        Private ILen, MLen As Integer
        Private value As BigInteger

        Public Shared ReadOnly Zero As New BigDecimal(0)
        Public Shared ReadOnly ZeroPointOne As New BigDecimal(0.1)
        Public Shared ReadOnly ZeroPointFive As New BigDecimal(0.5)
        Public Shared ReadOnly One As New BigDecimal(1)
        Public Shared ReadOnly Two As New BigDecimal(2)
        Public Shared ReadOnly ThreePointTwo As New BigDecimal(3.2)
        Public Shared ReadOnly Four As New BigDecimal(4)
        Public Shared ReadOnly Five As New BigDecimal(5)
        Public Shared ReadOnly TwentyFive As New BigDecimal(25)
        Public Shared ReadOnly TwoHundredThirtyNine As New BigDecimal(239)
        Public Shared ReadOnly FiftySevenThousandOneHundredTwentyOne As New BigDecimal(57121)

#Region "Properties"
        'Property for number of digits in BigDecimal's integral part
        Public ReadOnly Property IntegralLength() As Integer
            Get
                Return ILen
            End Get
        End Property

        'Property for number of digits in BigDecimal's mantissa part
        Public ReadOnly Property MantissaLength() As Integer
            Get
                Return ILen
            End Get
        End Property

        'Property whether BigDecimal is one
        Public ReadOnly Property IsOne() As Boolean
            Get
                If ILen = 1 AndAlso MLen = 0 Then Return value.IsOne Else Return False
            End Get
        End Property

        'Property whether BigDecimal is zero
        Public ReadOnly Property IsZero() As Boolean
            Get
                Return value.IsZero
            End Get
        End Property

        'Property of the BigDecimal's sign (negative, zero, positive)
        Public ReadOnly Property Sign() As Integer
            Get
                Return value.Sign
            End Get
        End Property

        'Property of whether BigDecimal is a power of two
        Public ReadOnly Property IsPowerOfTwo() As Boolean
            Get
                If MLen = 0 AndAlso value.IsPowerOfTwo _
                Then Return True _
                Else Return False
            End Get
        End Property

        'Property of whether BigDecimal is Even
        Public ReadOnly Property IsEven As Boolean
            Get
                If MLen = 0 AndAlso value.IsEven _
                Then Return True _
                Else Return False
            End Get
        End Property
#End Region

        Private Shared ReadOnly defaultPrecision As Integer = 1000

        Friend Const BigDecimalPrecisionEnvironmentConfigName$ = "big_decimal.precision"

        Shared Sub New()
            defaultPrecision = App.GetVariable(BigDecimalPrecisionEnvironmentConfigName).ParseInteger
        End Sub

#Region "Unsigned Integer Constructors"
        'Construct a BigDecimal number from Byte
        Sub New(Num As Byte)
            value = New BigInteger(Num)
            ILen = Num.ToString.Length
            MLen = 0
        End Sub

        'Construct a BigDecimal number from UShort
        Sub New(Num As UShort)
            value = New BigInteger(Num)
            ILen = Num.ToString.Length
            MLen = 0
        End Sub

        'Construct a BigDecimal number from UInteger
        Sub New(Num As UInteger)
            value = New BigInteger(Num)
            ILen = Num.ToString.Length
            MLen = 0
        End Sub

        'Construct a BigDecimal number from ULong
        Sub New(Num As ULong)
            value = New BigInteger(Num)
            ILen = Num.ToString.Length
            MLen = 0
        End Sub
#End Region

#Region "Signed Integral Constructors"
        'Construct a BigDecimal number from Signed Byte
        Sub New(Num As SByte)
            Dim t As String = Num.ToString & "."
            value = New BigInteger(Num)
            If t.StartsWith("-") Then
                t = (t.Remove(0, 1) & ".").Trim("0"c)
                ILen = t.Length - 1
            Else
                ILen = t.Length - 1
            End If
            MLen = 0
        End Sub

        'Construct a BigDecimal number from Short
        Sub New(Num As Short)
            Dim t As String = Num.ToString & "."
            value = New BigInteger(Num)
            If t.StartsWith("-") Then
                t = (t.Remove(0, 1) & ".").Trim("0"c)
                ILen = t.Length - 1
            Else
                ILen = t.Length - 1
            End If
            MLen = 0
        End Sub

        'Construct a BigDecimal number from Integer
        Sub New(Num As Integer)
            Dim t As String = Num.ToString & "."
            value = New BigInteger(Num)
            If t.StartsWith("-") Then
                t = (t.Remove(0, 1) & ".").Trim("0"c)
                ILen = t.Length - 1
            Else
                ILen = t.Length - 1
            End If
            MLen = 0
        End Sub

        'Construct a BigDecimal number from Long
        Sub New(Num As Long)
            Dim t As String = Num.ToString & "."
            value = New BigInteger(Num)
            If t.StartsWith("-") Then
                t = (t.Remove(0, 1) & ".").Trim("0"c)
                ILen = t.Length - 1
            Else
                ILen = t.Length - 1
            End If
            MLen = 0
        End Sub

        'Construct a BigDecimal number from BigInteger
        Sub New(Num As BigInteger)
            Dim t As String = Num.ToString & "."
            value = Num
            If t.StartsWith("-") Then
                t = (t.Remove(0, 1) & ".").Trim("0"c)
                ILen = t.Length - 1
            Else
                ILen = t.Length - 1
            End If
            MLen = 0
        End Sub
#End Region

#Region "Decimal Constructors"
        'Construct a BigDecimal number from Decimal
        Sub New(Num As Decimal)
            Parse(CStr(Num))
        End Sub

        'Construct a BigDecimal number from Single
        Sub New(Num As Single)
            Parse(CStr(Num))
        End Sub

        'Construct a BigDecimal number from Double
        Sub New(Num As Double)
            Parse(CStr(Num))
        End Sub

        'Construct a BigDecimal number from Byte array
        Sub New(Num() As Byte)
            ILen = BitConverter.ToInt32({Num(0), Num(1), Num(2), Num(3)}, 0)
            MLen = BitConverter.ToInt32({Num(4), Num(5), Num(6), Num(7)}, 0)
            Dim VBytes(Num.Length - 9) As Byte
            Array.Copy(Num, 8, VBytes, 0, VBytes.Length)
            value = New BigInteger(VBytes)
        End Sub

        'Construct a BigDecimal number from String
        Sub New(Num As String)
            Parse(Num)
        End Sub
#End Region

#Region "Main Parser"
        'Parses and sets the attributes for BigDecimal
        Private Sub Parse(Num As String)
            Dim neg As Boolean = False
            Dim dec As Boolean = True
            Dim Str As String = ""
            For Each digit As Char In Num
                Select Case digit
                    Case "-"c : neg = True
                    Case "0"c To "9"c : Str &= digit
                    Case "."c : If dec Then Str &= "." : dec = False
                End Select
            Next
            If Str.Contains(".") Then
                Str = Str.Trim("0"c)
                ILen = Str.IndexOf(".")
                MLen = Str.Length - ILen - 1
                If Str.EndsWith(".") Then Str = Str.Substring(0, Str.Length - 1)
            Else
                Str = (Str & ".").Trim("0"c)
                Str = Str.Substring(0, Str.Length - 1)
                ILen = Str.Length
                MLen = 0
            End If
            If Str = "." OrElse Str = "" Then
                Num = "0"
            Else
                If Str.StartsWith(".") Then Num = "0" & Str Else Num = Str
                If neg Then Num = "-" & Str
            End If
            value = BigInteger.Parse(Num.Replace(".", String.Empty))
        End Sub
#End Region

#Region "General Functions"
        'Does a comparison between the current BigInteger and another returning the first difference
        Public Function CompareTo(ByRef other As BigDecimal) As Integer
            If other.value.Sign = value.Sign Then 'Compares decimal sign
                If other.ILen = ILen Then 'Compares the integral size
                    If other.MLen = MLen Then 'Compares the mantissa size
                        If other.value = value Then 'Compares the actual value
                            Return 0 'the two are the same
                        Else
                            If other.value > value Then
                                Return -6 'Other decimal value is greater
                            Else
                                Return 6 'Other decimal value is smaller
                            End If
                        End If
                    Else
                        If other.MLen > MLen Then
                            Return -5 'Other decimal has a larger mantissa length
                        Else
                            Return 5 'Other decimal has a smaller mantissa length
                        End If
                    End If
                Else
                    If other.ILen > ILen Then
                        Return -4 'Other decimal has a larger integral length
                    Else
                        Return 4 'Other decimal has a smaller integral length
                    End If
                End If
            Else
                Select Case value.Sign
                    Case 0 'This decimal is zero
                        Select Case other.value.Sign
                            Case -1 'Other decimal is negative
                                Return 3
                            Case 1 'Other decimal is positive
                                Return -1
                        End Select
                    Case -1 'This decimal is negative
                        Select Case other.value.Sign
                            Case 0 'Other decimal is zero
                                Return -3
                            Case 1 'Other decimal is positive
                                Return -2
                        End Select
                    Case 1 'This decimal is positive
                        Select Case other.value.Sign
                            Case -1 'Other decimal is negative
                                Return 2
                            Case 0 'Other decimal is zero
                                Return 1
                        End Select
                End Select
            End If
            Return 7 'Shouldn't get this far, defaults to not same, Unknown error
        End Function

        'Represents BigDecimal number as string
        Public Overrides Function ToString() As String
            If value.Sign = 0 Then Return "0"
            Dim AsString As String = value.ToString
            If value.Sign = -1 Then
                AsString = AsString.Remove(0, 1).PadLeft(MLen, "0"c).Insert(ILen, ".")
                If ILen = 0 Then Return "-0" & AsString Else Return "-" & AsString.Trim("."c)
            End If
            AsString = AsString.PadLeft(MLen, "0"c).Insert(ILen, ".")
            If ILen = 0 Then Return "0" & AsString Else Return AsString.Trim("."c)
        End Function

        'Represents BigDecimal as string with a specified character for thousands delimiter
        Private Const ValDig As String = "-.0123456789" 'Valid characters for bigdecimal string
        Public Overloads Function ToString(Spacer As Char) As String
            If ValDig.Contains(Spacer) Then Throw New Exception("Invalid Character for use as spacer")
            Dim str As String = ToString()
            If Not Spacer = "" Then
                If value.Sign = -1 Then
                    If MLen = 0 Then
                        For i = str.Length - 3 To 2 Step -3 : str = str.Insert(i, Spacer) : Next
                    Else
                        For i = str.IndexOf(".") - 3 To 2 Step -3 : str = str.Insert(i, Spacer) : Next
                    End If
                Else
                    If MLen = 0 Then
                        For i = str.Length - 3 To 1 Step -3 : str = str.Insert(i, Spacer) : Next
                    Else
                        For i = str.IndexOf(".") - 3 To 1 Step -3 : str = str.Insert(i, Spacer) : Next
                    End If
                End If
            End If
            Return str
        End Function

        'Returns the current BigDecimal represented as an array of bytes
        Public Function ToByteArray() As Byte()
            Dim RBytes As New List(Of Byte)
            RBytes.AddRange(BitConverter.GetBytes(ILen))
            RBytes.AddRange(BitConverter.GetBytes(MLen))
            RBytes.AddRange(value.ToByteArray)
            Return RBytes.ToArray
        End Function
#End Region

#Region "Basic Operators"
        'Does addition on two BigDecimal numbers
        Public Shared Operator +(Num1 As BigDecimal, Num2 As BigDecimal) As BigDecimal
            Dim L As Integer = Num1.MLen
            Dim D As Integer = 0
            If Num1.MLen > Num2.MLen Then
                D = Num1.MLen - Num2.MLen
                Num2.value = Pow10(Num2, D)
            Else
                L = Num2.MLen
                D = Num2.MLen - Num1.MLen
                Num1.value = Pow10(Num1, D)
            End If
            Return PowN10(BigInteger.Add(Num1.value, Num2.value).ToString, L)
        End Operator

        'Does subtraction on two BigDecimal numbers
        Public Shared Operator -(Num1 As BigDecimal, Num2 As BigDecimal) As BigDecimal
            Dim L As Integer = Num1.MLen
            Dim D As Integer = 0
            If Num1.MLen > Num2.MLen Then
                D = Num1.MLen - Num2.MLen
                Num2.value = Pow10(Num2, D)
            Else
                L = Num2.MLen
                D = Num2.MLen - Num1.MLen
                Num1.value = Pow10(Num1, D)
            End If
            Return PowN10(BigInteger.Subtract(Num1.value, Num2.value).ToString, L)
        End Operator

        'Does multiplication on two BigDecimal numbers
        Public Shared Operator *(Num1 As BigDecimal, Num2 As BigDecimal) As BigDecimal
            Dim D As Integer = 0
            If Num1.MLen >= Num2.MLen _
            Then D = Num1.MLen - Num2.MLen _
            Else D = Num2.MLen - Num1.MLen
            Dim O As Integer = Num1.MLen + Num2.MLen + D * 2
            Num1.value = Pow10(Num1, D)
            Num2.value = Pow10(Num2, D)
            Return PowN10(BigInteger.Multiply(Num1.value, Num2.value).ToString, O)
        End Operator

        ''' <summary>
        ''' Does division on two BigDecimal numbers
        ''' </summary>
        ''' <param name="Num1"></param>
        ''' <param name="Num2"></param>
        ''' <returns></returns>
        Public Shared Operator /(Num1 As BigDecimal, Num2 As BigDecimal) As BigDecimal
            Return Div(Num1, Num2, Precision:=defaultPrecision)
        End Operator

        'Does division on two BigDecimal numbers with specified precision (no rounding)
        Public Shared Function Div(Num1 As BigDecimal, Num2 As BigDecimal,
                                  Optional Precision As Integer = -1) As BigDecimal
            If Num2.value.IsZero Then Throw New DivideByZeroException
            Dim P As Integer = (Num1.MLen * Num2.MLen) + Num1.ILen + Num2.ILen + 1
            Dim D As Integer = std.Abs(Num1.MLen - Num2.MLen)
            If Precision > -1 Then P = Precision
            If Num1.MLen >= Num2.MLen Then
                Num1.value = Pow10(Num1, P)
                Num2.value = Pow10(Num2, D)
            Else
                Num1.value = Pow10(Num1, D + P)
            End If
            Return PowN10(BigInteger.Divide(Num1.value, Num2.value).ToString, P)
        End Function

        'Raise BigDecimal to the power of an integer
        Public Shared Operator ^(Num1 As BigDecimal, Num2 As Integer) As BigDecimal
            Return Pow(Num1, Num2)
        End Operator

        'Raise BigDecimal to power of integer with specified precision for negatives(no rounding)
        Public Shared Function Pow(Num1 As BigDecimal, Num2 As Integer,
                               Optional Precision As Integer = -1) As BigDecimal
            If Num2 = 0 Then Return One
            If Num2 < 0 Then
                Num2 = std.Abs(Num2)
                Dim L As Integer = Num1.MLen * Num2
                Dim R As BigDecimal = PowN10(BigInteger.Pow(Num1.value, Num2).ToString, L)
                If Precision = -1 Then Precision = R.ILen + R.MLen
                Return Div(One, R, Precision)
            Else
                Dim L As Integer = Num1.MLen * Num2
                Dim R As BigInteger = BigInteger.Pow(Num1.value, Num2)
                Return PowN10(R.ToString, L)
            End If
        End Function

        'Does Modular arithmetic on two BigDecimal numbers
        Public Shared Operator Mod(Num1 As BigDecimal, Num2 As BigDecimal) As BigDecimal
            If Num2.ILen > Num1.ILen Then Return Num1
            If Num2.value.IsZero Then Throw New DivideByZeroException
            Dim L As Integer = Num1.MLen
            Dim D As Integer = std.Abs(Num1.MLen - Num2.MLen)
            If Num1.MLen >= Num2.MLen Then
                Num2.value = Pow10(Num2, D)
            Else
                L = Num2.MLen
                Num1.value = Pow10(Num1, D)
            End If
            Return PowN10(New BigDecimal(BigInteger.ModPow(Num1.value, 1, Num2.value)), L)
            'Return PowN10(BigInteger.ModPow(Num1.value, 1, Num2.value).ToString, L)
        End Operator

        'Does equal comparison on two BigDecimal numbers
        Public Shared Operator =(Num1 As BigDecimal, Num2 As BigDecimal) As Boolean
            If Num1.value.Sign = Num2.value.Sign _
            AndAlso Num1.ILen = Num2.ILen _
            AndAlso Num1.MLen = Num2.MLen _
            AndAlso Num1.value = Num2.value _
            Then Return True _
            Else Return False
        End Operator

        'Does greater than less than comparison on two BigDecimal numbers
        Public Shared Operator <>(Num1 As BigDecimal, Num2 As BigDecimal) As Boolean
            If Num1.value.Sign = Num2.value.Sign _
            AndAlso Num1.ILen = Num2.ILen _
            AndAlso Num1.MLen = Num2.MLen _
            AndAlso Num1.value = Num2.value _
            Then Return False _
            Else Return True
        End Operator

        'Does greater than comparison on two BigDecimal numbers
        Public Shared Operator >(Num1 As BigDecimal, Num2 As BigDecimal) As Boolean
            If Num1.Sign > Num2.Sign OrElse Num1.ILen > Num1.ILen Then Return True
            Dim L As Integer = Num1.MLen
            Dim D As Integer = 0
            If Num1.MLen > Num2.MLen Then
                D = Num1.MLen - Num2.MLen
                Num2.value = Pow10(Num2, D)
            Else
                L = Num2.MLen
                D = Num2.MLen - Num1.MLen
                Num1.value = Pow10(Num1, D)
            End If
            If BigInteger.Subtract(Num1.value, Num2.value) > 0 Then Return True
            Return False
        End Operator

        'Does less than comparison on two BigDecimal numbers
        Public Shared Operator <(Num1 As BigDecimal, Num2 As BigDecimal) As Boolean
            If Num1.Sign > Num2.Sign OrElse Num1.ILen > Num1.ILen Then Return True
            If Num1.MLen = Num2.MLen Then
                Return Num1.value < Num2.value
            Else
                Dim L As Integer = Num1.MLen
                Dim D As Integer = 0
                If Num1.MLen > Num2.MLen Then
                    D = Num1.MLen - Num2.MLen
                    Num2.value = Pow10(Num2, D)
                Else
                    L = Num2.MLen
                    D = Num2.MLen - Num1.MLen
                    Num1.value = Pow10(Num1, D)
                End If
                If BigInteger.Subtract(Num1.value, Num2.value) < 0 Then Return True
            End If
            Return False
        End Operator

        'Does greater than or equal to comparison on two BigDecimal numbers
        Public Shared Operator >=(Num1 As BigDecimal, Num2 As BigDecimal) As Boolean
            If Num1.Sign > Num2.Sign OrElse Num1.ILen > Num1.ILen Then Return True
            If Num1.MLen = Num2.MLen Then
                Return Num1.value >= Num2.value
            Else
                Dim L As Integer = Num1.MLen
                Dim D As Integer = 0
                If Num1.MLen > Num2.MLen Then
                    D = Num1.MLen - Num2.MLen
                    Num2.value = Pow10(Num2, D)
                Else
                    L = Num2.MLen
                    D = Num2.MLen - Num1.MLen
                    Num1.value = Pow10(Num1, D)
                End If
                If BigInteger.Subtract(Num1.value, Num2.value) >= 0 Then Return True
            End If
            Return False
        End Operator

        'Does less than or equal to comparison on two BigDecimal numbers
        Public Shared Operator <=(Num1 As BigDecimal, Num2 As BigDecimal) As Boolean
            If Num1.Sign > Num2.Sign OrElse Num1.ILen > Num1.ILen Then Return True
            If Num1.MLen = Num2.MLen Then
                Return Num1.value <= Num2.value
            Else
                Dim L As Integer = Num1.MLen
                Dim D As Integer = 0
                If Num1.MLen > Num2.MLen Then
                    D = Num1.MLen - Num2.MLen
                    Num2.value = Pow10(Num2, D)
                Else
                    L = Num2.MLen
                    D = Num2.MLen - Num1.MLen
                    Num1.value = Pow10(Num1, D)
                End If
                If BigInteger.Subtract(Num1.value, Num2.value) <= 0 Then Return True
            End If
            Return False
        End Operator
#End Region

#Region "Misc"
        'Multiplies a BigDecimal number by 10 raised to a specified exponent
        Private Shared Function Pow10(value As BigDecimal, exponent As Integer) As BigInteger
            If value.value = 0 OrElse exponent = 0 Then Return value.value
            Return BigInteger.Multiply(value.value, BigInteger.Pow(10, exponent))
        End Function

        'Moves around the decimal value to mimic a BigDecimal multiplied by 10 to a negative power
        Private Shared Function PowN10(value As String, exponent As Integer) As BigDecimal
            If value.StartsWith("-") Then
                value = value.Remove(0, 1).PadLeft(exponent, "0"c)
                Return New BigDecimal("-" & value.Insert(value.Length - exponent, "."))
            End If
            value = value.PadLeft(exponent, "0"c)
            Return New BigDecimal(value.Insert(value.Length - exponent, "."))
        End Function

        'Raises a bigdecimal to a negative integer power
        Private Shared Function PowN10(Num As BigDecimal, exponent As Integer) As BigDecimal
            If exponent = 0 Then Return One
            Dim EXP As BigDecimal = ZeroPointOne
            For i = 1 To std.Abs(exponent) : EXP *= ZeroPointOne : Next
            Return Num * EXP
        End Function

        'Finds the square root of a bigdecimal number given the precision, no decimals if not specified
        Public Shared Function Sqrt(Num As BigDecimal,
                            Optional Precision As Integer = 0) As BigDecimal
            Dim ourGuess As BigDecimal = Div(Num, Two, Precision)
            While True
                Dim result As BigDecimal = Div(Num, ourGuess, Precision)
                Dim average As BigDecimal = Div((ourGuess + result), Two, Precision)
                If average = ourGuess Then Return average Else ourGuess = average
            End While
            Return Zero
        End Function

        'Returns PI to the specified decimal place
        Public Shared Function Pi(Precision As Integer) As BigDecimal
            Precision += 4 'Increase precision internally to account for rounding
            Dim answer As BigDecimal = Zero
            Dim term5 As BigDecimal = ThreePointTwo
            Dim term239 As BigDecimal = Div(Four, TwoHundredThirtyNine, Precision)
            Dim term5m As BigDecimal
            Dim term239m As BigDecimal
            Dim n5 As Integer = 0
            Dim n239 As Integer = 0
            While True
                term5m = Div(term5, New BigDecimal(n5 * 2 + 1), Precision)
                If n5 Mod 2 = 0 Then answer += term5m Else answer -= term5m
                term5 = Div(term5, TwentyFive, Precision)
                n5 += 1
                If term5m.IsZero Then Exit While
            End While
            While True
                term239m = Div(term239, New BigDecimal(n239 * 2 + 1), Precision)
                If n239 Mod 2 = 0 Then answer -= term239m Else answer += term239m
                term239 = Div(term239, FiftySevenThousandOneHundredTwentyOne, Precision)
                n239 += 1
                If term239m.IsZero Then Exit While
            End While
            Return Div(answer, One, Precision - 4)
        End Function

        'Returns Phi to the specified decimal place
        Public Shared Function Phi(Precision As Integer) As BigDecimal
            Return (One + Sqrt(Five, Precision)) * ZeroPointFive
        End Function

        'Returns Tau to the specified decimal place
        Public Shared Function Tau(Precision As Integer) As BigDecimal
            Return Div(Pi(Precision + 1) * Two, One, Precision)
        End Function

        'Returns Pythagoras' constant to the specified decimal place
        Public Shared Function PythagorasConst(Precision As Integer) As BigDecimal
            Return Sqrt(Two, Precision)
        End Function
#End Region
    End Structure
End Namespace
