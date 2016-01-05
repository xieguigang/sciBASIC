Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Runtime.InteropServices
Imports System.Threading

Namespace Microsoft.VisualBasic
    <StandardModule> _
    Public NotInheritable Class Financial
        ' Methods
        Public Shared Function DDB(Cost As Double, Salvage As Double, Life As Double, Period As Double, Optional Factor As Double = 2) As Double
            If (((Factor <= 0) OrElse (Salvage < 0)) OrElse ((Period <= 0) OrElse (Period > Life))) Then
                Dim args As String() = New String() {"Factor"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If (Cost > 0) Then
                Dim num2 As Double
                Dim num4 As Double
                If (Life < 2) Then
                    Return (Cost - Salvage)
                End If
                If ((Life = 2) AndAlso (Period > 1)) Then
                    Return 0
                End If
                If ((Life = 2) AndAlso (Period <= 1)) Then
                    Return (Cost - Salvage)
                End If
                If (Period <= 1) Then
                    num2 = ((Cost * Factor) / Life)
                    num4 = (Cost - Salvage)
                    If (num2 > num4) Then
                        Return num4
                    End If
                    Return num2
                End If
                num4 = ((Life - Factor) / Life)
                Dim y As Double = (Period - 1)
                num2 = (((Factor * Cost) / Life) * Math.Pow(num4, y))
                Dim num3 As Double = (((Cost * (1 - Math.Pow(num4, Period))) - Cost) + Salvage)
                If (num3 > 0) Then
                    num2 = (num2 - num3)
                End If
                If (num2 >= 0) Then
                    Return num2
                End If
            End If
            Return 0
        End Function

        Public Shared Function FV(Rate As Double, NPer As Double, Pmt As Double, Optional PV As Double = 0, Optional Due As DueDate = 0) As Double
            Return Financial.FV_Internal(Rate, NPer, Pmt, PV, Due)
        End Function

        Private Shared Function FV_Internal(Rate As Double, NPer As Double, Pmt As Double, Optional PV As Double = 0, Optional Due As DueDate = 0) As Double
            Dim num2 As Double
            If (Rate = 0) Then
                Return (-PV - (Pmt * NPer))
            End If
            If (Due <> DueDate.EndOfPeriod) Then
                num2 = (1 + Rate)
            Else
                num2 = 1
            End If
            Dim num3 As Double = Math.Pow((1 + Rate), NPer)
            Return ((-PV * num3) - (((Pmt / Rate) * num2) * (num3 - 1)))
        End Function

        Public Shared Function IPmt(Rate As Double, Per As Double, NPer As Double, PV As Double, Optional FV As Double = 0, Optional Due As DueDate = 0) As Double
            Dim num3 As Double
            If (Due <> DueDate.EndOfPeriod) Then
                num3 = 2
            Else
                num3 = 1
            End If
            If ((Per <= 0) OrElse (Per >= (NPer + 1))) Then
                Dim args As String() = New String() {"Per"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If ((Due <> DueDate.EndOfPeriod) AndAlso (Per = 1)) Then
                Return 0
            End If
            Dim pmt As Double = Financial.PMT_Internal(Rate, NPer, PV, FV, Due)
            If (Due <> DueDate.EndOfPeriod) Then
                PV = (PV + pmt)
            End If
            Return (Financial.FV_Internal(Rate, (Per - num3), pmt, PV, DueDate.EndOfPeriod) * Rate)
        End Function

        Public Shared Function IRR(ByRef ValueArray As Double(), Optional Guess As Double = 0.1) As Double
            Dim num2 As Double
            Dim num4 As Double
            Dim num9 As Integer
            Dim upperBound As Integer
            Try
                upperBound = ValueArray.GetUpperBound(0)
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception6 As Exception
                Dim args As String() = New String() {"ValueArray"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End Try
            Dim num10 As Integer = (upperBound + 1)
            If (guess <= -1) Then
                Dim textArray2 As String() = New String() {"Guess"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2))
            End If
            If (num10 <= 1) Then
                Dim textArray3 As String() = New String() {"ValueArray"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray3))
            End If
            If (ValueArray(0) > 0) Then
                num2 = ValueArray(0)
            Else
                num2 = -ValueArray(0)
            End If
            Dim num12 As Integer = upperBound
            num9 = 0
            Do While (num9 <= num12)
                If (ValueArray(num9) > num2) Then
                    num2 = ValueArray(num9)
                ElseIf (-ValueArray(num9) > num2) Then
                    num2 = -ValueArray(num9)
                End If
                num9 += 1
            Loop
            Dim num7 As Double = ((num2 * 0.0000001) * 0.01)
            Dim guess As Double = guess
            Dim num5 As Double = Financial.OptPV2(ValueArray, guess)
            If (num5 > 0) Then
                num4 = (guess + 0.00001)
            Else
                num4 = (guess - 0.00001)
            End If
            If (num4 <= -1) Then
                Dim textArray4 As String() = New String() {"Rate"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray4))
            End If
            Dim num6 As Double = Financial.OptPV2(ValueArray, num4)
            num9 = 0
            Do While True
                Dim num8 As Double
                If (num6 = num5) Then
                    If (num4 > guess) Then
                        guess = (guess - 0.00001)
                    Else
                        guess = (guess + 0.00001)
                    End If
                    num5 = Financial.OptPV2(ValueArray, guess)
                    If (num6 = num5) Then
                        Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
                    End If
                End If
                guess = (num4 - (((num4 - guess) * num6) / (num6 - num5)))
                If (guess <= -1) Then
                    guess = ((num4 - 1) * 0.5)
                End If
                num5 = Financial.OptPV2(ValueArray, guess)
                If (guess > num4) Then
                    num2 = (guess - num4)
                Else
                    num2 = (num4 - guess)
                End If
                If (num5 > 0) Then
                    num8 = num5
                Else
                    num8 = -num5
                End If
                If ((num8 < num7) AndAlso (num2 < 0.0000001)) Then
                    Return guess
                End If
                num2 = num5
                num5 = num6
                num6 = num2
                num2 = guess
                guess = num4
                num4 = num2
                num9 += 1
                If (num9 > &H27) Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
                End If
            Loop
        End Function

        Private Shared Function LDoNPV(Rate As Double, ByRef ValueArray As Double(), iWNType As Integer) As Double
            Dim flag As Boolean = (iWNType < 0)
            Dim flag2 As Boolean = (iWNType > 0)
            Dim num2 As Double = 1
            Dim num3 As Double = 0
            Dim upperBound As Integer = ValueArray.GetUpperBound(0)
            Dim i As Integer = 0
            Do While (i <= upperBound)
                Dim num4 As Double = ValueArray(i)
                num2 = (num2 + (num2 * Rate))
                If ((Not flag OrElse (num4 <= 0)) AndAlso (Not flag2 OrElse (num4 >= 0))) Then
                    num3 = (num3 + (num4 / num2))
                End If
                i += 1
            Loop
            Return num3
        End Function

        Private Shared Function LEvalRate(Rate As Double, NPer As Double, Pmt As Double, PV As Double, dFv As Double, Due As DueDate) As Double
            Dim num3 As Double
            If (Rate = 0) Then
                Return ((PV + (Pmt * NPer)) + dFv)
            End If
            Dim num2 As Double = Math.Pow((Rate + 1), NPer)
            If (Due <> DueDate.EndOfPeriod) Then
                num3 = (1 + Rate)
            Else
                num3 = 1
            End If
            Return (((PV * num2) + (((Pmt * num3) * (num2 - 1)) / Rate)) + dFv)
        End Function

        Public Shared Function MIRR(ByRef ValueArray As Double(), FinanceRate As Double, ReinvestRate As Double) As Double
            If (ValueArray.Rank <> 1) Then
                Dim args As String() = New String() {"ValueArray"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_RankEQOne1", args))
            End If
            Dim num7 As Integer = 0
            Dim num6 As Integer = ((ValueArray.GetUpperBound(0) - num7) + 1)
            If (FinanceRate = -1) Then
                Dim textArray2 As String() = New String() {"FinanceRate"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2))
            End If
            If (ReinvestRate = -1) Then
                Dim textArray3 As String() = New String() {"ReinvestRate"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray3))
            End If
            If (num6 <= 1) Then
                Dim textArray4 As String() = New String() {"ValueArray"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray4))
            End If
            Dim num3 As Double = Financial.LDoNPV(FinanceRate, ValueArray, -1)
            If (num3 = 0) Then
                Throw New DivideByZeroException(Utils.GetResourceString("Financial_CalcDivByZero"))
            End If
            Dim num2 As Double = Financial.LDoNPV(ReinvestRate, ValueArray, 1)
            Dim x As Double = (ReinvestRate + 1)
            Dim y As Double = num6
            Dim num1 As Double = ((-num2 * Math.Pow(x, y)) / (num3 * (FinanceRate + 1)))
            If (num1 < 0) Then
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
            End If
            x = (1 / (num6 - 1))
            Return (Math.Pow(num1, x) - 1)
        End Function

        Public Shared Function NPer(Rate As Double, Pmt As Double, PV As Double, Optional FV As Double = 0, Optional Due As DueDate = 0) As Double
            Dim num2 As Double
            If (Rate <= -1) Then
                Dim args As String() = New String() {"Rate"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If (Rate = 0) Then
                If (Pmt = 0) Then
                    Dim textArray2 As String() = New String() {"Pmt"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2))
                End If
                Return (-(PV + FV) / Pmt)
            End If
            If (Due <> DueDate.EndOfPeriod) Then
                num2 = ((Pmt * (1 + Rate)) / Rate)
            Else
                num2 = (Pmt / Rate)
            End If
            Dim d As Double = (-FV + num2)
            Dim num4 As Double = (PV + num2)
            If ((d < 0) AndAlso (num4 < 0)) Then
                d = (-1 * d)
                num4 = (-1 * num4)
            ElseIf ((d <= 0) OrElse (num4 <= 0)) Then
                Throw New ArgumentException(Utils.GetResourceString("Financial_CannotCalculateNPer"))
            End If
            Dim num5 As Double = (Rate + 1)
            Return ((Math.Log(d) - Math.Log(num4)) / Math.Log(num5))
        End Function

        Public Shared Function NPV(Rate As Double, ByRef ValueArray As Double()) As Double
            If (ValueArray Is Nothing) Then
                Dim args As String() = New String() {"ValueArray"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", args))
            End If
            If (ValueArray.Rank <> 1) Then
                Dim textArray2 As String() = New String() {"ValueArray"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_RankEQOne1", textArray2))
            End If
            Dim num3 As Integer = 0
            Dim num2 As Integer = ((ValueArray.GetUpperBound(0) - num3) + 1)
            If (Rate = -1) Then
                Dim textArray3 As String() = New String() {"Rate"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray3))
            End If
            If (num2 < 1) Then
                Dim textArray4 As String() = New String() {"ValueArray"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray4))
            End If
            Return Financial.LDoNPV(Rate, ValueArray, 0)
        End Function

        Private Shared Function OptPV2(ByRef ValueArray As Double(), Optional Guess As Double = 0.1) As Double
            Dim index As Integer = 0
            Dim upperBound As Integer = ValueArray.GetUpperBound(0)
            Dim num4 As Double = 0
            Dim num5 As Double = (1 + Guess)
            Do While ((index <= upperBound) AndAlso (ValueArray(index) = 0))
                index += 1
            Loop
            Dim num6 As Integer = index
            Dim i As Integer = upperBound
            Do While (i >= num6)
                num4 = (num4 / num5)
                num4 = (num4 + ValueArray(i))
                i = (i + -1)
            Loop
            Return num4
        End Function

        Public Shared Function Pmt(Rate As Double, NPer As Double, PV As Double, Optional FV As Double = 0, Optional Due As DueDate = 0) As Double
            Return Financial.PMT_Internal(Rate, NPer, PV, FV, Due)
        End Function

        Private Shared Function PMT_Internal(Rate As Double, NPer As Double, PV As Double, Optional FV As Double = 0, Optional Due As DueDate = 0) As Double
            Dim num2 As Double
            If (NPer = 0) Then
                Dim args As String() = New String() {"NPer"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If (Rate = 0) Then
                Return ((-FV - PV) / NPer)
            End If
            If (Due <> DueDate.EndOfPeriod) Then
                num2 = (1 + Rate)
            Else
                num2 = 1
            End If
            Dim num3 As Double = Math.Pow((Rate + 1), NPer)
            Return (((-FV - (PV * num3)) / (num2 * (num3 - 1))) * Rate)
        End Function

        Public Shared Function PPmt(Rate As Double, Per As Double, NPer As Double, PV As Double, Optional FV As Double = 0, Optional Due As DueDate = 0) As Double
            If ((Per > 0) AndAlso (Per < (NPer + 1))) Then
                Return (Financial.PMT_Internal(Rate, NPer, PV, FV, Due) - Financial.IPmt(Rate, Per, NPer, PV, FV, Due))
            End If
            Dim args As String() = New String() {"Per"}
            Throw New ArgumentException(Utils.GetResourceString("PPMT_PerGT0AndLTNPer", args))
        End Function

        Public Shared Function PV(Rate As Double, NPer As Double, Pmt As Double, Optional FV As Double = 0, Optional Due As DueDate = 0) As Double
            Dim num2 As Double
            If (Rate = 0) Then
                Return (-FV - (Pmt * NPer))
            End If
            If (Due <> DueDate.EndOfPeriod) Then
                num2 = (1 + Rate)
            Else
                num2 = 1
            End If
            Dim num3 As Double = Math.Pow((1 + Rate), NPer)
            Return (-(FV + ((Pmt * num2) * ((num3 - 1) / Rate))) / num3)
        End Function

        Public Shared Function Rate(NPer As Double, Pmt As Double, PV As Double, Optional FV As Double = 0, Optional Due As DueDate = 0, Optional Guess As Double = 0.1) As Double
            Dim num3 As Double
            If (NPer <= 0) Then
                Throw New ArgumentException(Utils.GetResourceString("Rate_NPerMustBeGTZero"))
            End If
            Dim rate As Double = Guess
            Dim num4 As Double = Financial.LEvalRate(rate, NPer, Pmt, PV, FV, Due)
            If (num4 > 0) Then
                num3 = (rate / 2)
            Else
                num3 = (rate * 2)
            End If
            Dim num5 As Double = Financial.LEvalRate(num3, NPer, Pmt, PV, FV, Due)
            Dim num6 As Integer = 0
            Do While True
                If (num5 = num4) Then
                    If (num3 > rate) Then
                        rate = (rate - 0.00001)
                    Else
                        rate = (rate - -0.00001)
                    End If
                    num4 = Financial.LEvalRate(rate, NPer, Pmt, PV, FV, Due)
                    If (num5 = num4) Then
                        Throw New ArgumentException(Utils.GetResourceString("Financial_CalcDivByZero"))
                    End If
                End If
                rate = (num3 - (((num3 - rate) * num5) / (num5 - num4)))
                num4 = Financial.LEvalRate(rate, NPer, Pmt, PV, FV, Due)
                If (Math.Abs(num4) < 0.0000001) Then
                    Return rate
                End If
                num4 = num5
                num5 = num4
                rate = num3
                num3 = rate
                num6 += 1
                If (num6 > &H27) Then
                    Throw New ArgumentException(Utils.GetResourceString("Financial_CannotCalculateRate"))
                End If
            Loop
        End Function

        Public Shared Function SLN(Cost As Double, Salvage As Double, Life As Double) As Double
            If (Life = 0) Then
                Throw New ArgumentException(Utils.GetResourceString("Financial_LifeNEZero"))
            End If
            Return ((Cost - Salvage) / Life)
        End Function

        Public Shared Function SYD(Cost As Double, Salvage As Double, Life As Double, Period As Double) As Double
            If (Salvage < 0) Then
                Dim args As String() = New String() {"Salvage"}
                Throw New ArgumentException(Utils.GetResourceString("Financial_ArgGEZero1", args))
            End If
            If (Period > Life) Then
                Throw New ArgumentException(Utils.GetResourceString("Financial_PeriodLELife"))
            End If
            If (Period <= 0) Then
                Dim textArray2 As String() = New String() {"Period"}
                Throw New ArgumentException(Utils.GetResourceString("Financial_ArgGTZero1", textArray2))
            End If
            Return ((((Cost - Salvage) / (Life * (Life + 1))) * ((Life + 1) - Period)) * 2)
        End Function


        ' Fields
        Private Const cnL_IT_EPSILON As Double = 1E-07
        Private Const cnL_IT_STEP As Double = 1E-05
    End Class
End Namespace

