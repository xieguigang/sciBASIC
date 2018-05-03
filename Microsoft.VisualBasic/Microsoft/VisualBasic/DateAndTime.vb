Imports System.Globalization
Imports System.Runtime.InteropServices
Imports System.Security.Permissions
Imports System.Threading
Imports Microsoft.VisualBasic.CompilerServices

Namespace Microsoft.VisualBasic
    <StandardModule> _
    Public NotInheritable Class DateAndTime
        ' Methods
        Public Shared Function DateAdd(Interval As DateInterval, Number As Double, DateValue As DateTime) As DateTime
            Dim years As Integer = CInt(Math.Round(Conversion.Fix(Number)))
            Select Case Interval
                Case DateInterval.Year
                    Return DateAndTime.CurrentCalendar.AddYears(DateValue, years)
                Case DateInterval.Quarter
                    Return DateValue.AddMonths((years * 3))
                Case DateInterval.Month
                    Return DateAndTime.CurrentCalendar.AddMonths(DateValue, years)
                Case DateInterval.DayOfYear, DateInterval.Day, DateInterval.Weekday
                    Return DateValue.AddDays(CDbl(years))
                Case DateInterval.WeekOfYear
                    Return DateValue.AddDays((years * 7))
                Case DateInterval.Hour
                    Return DateValue.AddHours(CDbl(years))
                Case DateInterval.Minute
                    Return DateValue.AddMinutes(CDbl(years))
                Case DateInterval.Second
                    Return DateValue.AddSeconds(CDbl(years))
            End Select
            Dim args As String() = New String() {"Interval"}
            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
        End Function

        Public Shared Function DateAdd(Interval As String, Number As Double, DateValue As Object) As DateTime
            Dim time As DateTime
            Try
                time = Conversions.ToDate(DateValue)
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception6 As Exception
                Dim args As String() = New String() {"DateValue"}
                Throw New InvalidCastException(Utils.GetResourceString("Argument_InvalidDateValue1", args))
            End Try
            Return DateAndTime.DateAdd(DateAndTime.DateIntervalFromString(Interval), Number, time)
        End Function

        Public Shared Function DateDiff(Interval As DateInterval, Date1 As DateTime, Date2 As DateTime, Optional DayOfWeek As FirstDayOfWeek = 1, Optional WeekOfYear As FirstWeekOfYear = 1) As Long
            Dim currentCalendar As Calendar
            Dim span As TimeSpan = Date2.Subtract(Date1)
            Select Case Interval
                Case DateInterval.Year
                    currentCalendar = DateAndTime.CurrentCalendar
                    Return CLng((currentCalendar.GetYear(Date2) - currentCalendar.GetYear(Date1)))
                Case DateInterval.Quarter
                    currentCalendar = DateAndTime.CurrentCalendar
                    Return CLng(((((currentCalendar.GetYear(Date2) - currentCalendar.GetYear(Date1)) * 4) + ((currentCalendar.GetMonth(Date2) - 1) / 3)) - ((currentCalendar.GetMonth(Date1) - 1) / 3)))
                Case DateInterval.Month
                    currentCalendar = DateAndTime.CurrentCalendar
                    Return CLng(((((currentCalendar.GetYear(Date2) - currentCalendar.GetYear(Date1)) * 12) + currentCalendar.GetMonth(Date2)) - currentCalendar.GetMonth(Date1)))
                Case DateInterval.DayOfYear, DateInterval.Day
                    Return CLng(Math.Round(Conversion.Fix(span.TotalDays)))
                Case DateInterval.WeekOfYear
                    Date1 = Date1.AddDays(CDbl((0 - DateAndTime.GetDayOfWeek(Date1, DayOfWeek))))
                    Date2 = Date2.AddDays(CDbl((0 - DateAndTime.GetDayOfWeek(Date2, DayOfWeek))))
                    Return (CLng(Math.Round(Conversion.Fix(Date2.Subtract(Date1).TotalDays))) / 7)
                Case DateInterval.Weekday
                    Return (CLng(Math.Round(Conversion.Fix(span.TotalDays))) / 7)
                Case DateInterval.Hour
                    Return CLng(Math.Round(Conversion.Fix(span.TotalHours)))
                Case DateInterval.Minute
                    Return CLng(Math.Round(Conversion.Fix(span.TotalMinutes)))
                Case DateInterval.Second
                    Return CLng(Math.Round(Conversion.Fix(span.TotalSeconds)))
            End Select
            Dim args As String() = New String() {"Interval"}
            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
        End Function

        Public Shared Function DateDiff(Interval As String, Date1 As Object, Date2 As Object, Optional DayOfWeek As FirstDayOfWeek = 1, Optional WeekOfYear As FirstWeekOfYear = 1) As Long
            Dim time As DateTime
            Dim time2 As DateTime
            Try
                time = Conversions.ToDate(Date1)
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception9 As Exception
                Dim args As String() = New String() {"Date1"}
                Throw New InvalidCastException(Utils.GetResourceString("Argument_InvalidDateValue1", args))
            End Try
            Try
                time2 = Conversions.ToDate(Date2)
            Catch exception4 As StackOverflowException
                Throw exception4
            Catch exception5 As OutOfMemoryException
                Throw exception5
            Catch exception6 As ThreadAbortException
                Throw exception6
            Catch exception13 As Exception
                Dim textArray2 As String() = New String() {"Date2"}
                Throw New InvalidCastException(Utils.GetResourceString("Argument_InvalidDateValue1", textArray2))
            End Try
            Return DateAndTime.DateDiff(DateAndTime.DateIntervalFromString(Interval), time, time2, DayOfWeek, WeekOfYear)
        End Function

        Private Shared Function DateIntervalFromString(Interval As String) As DateInterval
            Dim textArray1 As String()
            If (Not Interval Is Nothing) Then
                Interval = Interval.ToUpperInvariant
            End If
            Dim s As String = Interval

            ' <PrivateImplementationDetails>
            Dim num As UInt32 = ComputeStringHash(s)
            If (num <= &HCD0C04F7) Then
                Select Case num
                Case &HC80BFD18
                    If (s = "M") Then
                        Return DateInterval.Month
                    End If
                    Exit Select
                Case &HCB0C01D1
                    If (s = "N") Then
                        Return DateInterval.Minute
                    End If
                    Exit Select
                Case &HCD0C04F7
                    If (s = "H") Then
                        Return DateInterval.Hour
                    End If
                    Exit Select
                Case &H29F7AF13
                    If (s = "WW") Then
                        Return DateInterval.WeekOfYear
                    End If
                    Exit Select
                Case &HC10BF213
                    If (s = "D") Then
                        Return DateInterval.Day
                    End If
                    Exit Select
            End Select
            ElseIf (num <= &HD40C0FFC) Then
            Select Case num
                Case &HD20C0CD6
                    If (s = "W") Then
                        Return DateInterval.Weekday
                    End If
                    Exit Select
                Case &HD40C0FFC
                    If (s = "Q") Then
                        Return DateInterval.Quarter
                    End If
                    Exit Select
            End Select
            Else
            Select Case num
                Case &HD60C1322
                    If (s = "S") Then
                        Return DateInterval.Second
                    End If
                    GoTo Label_0184
                Case &HDC0C1C94
                    If (s = "Y") Then
                        Return DateInterval.DayOfYear
                    End If
                    GoTo Label_0184
            End Select
            If ((num = &HE9668689) AndAlso (s = "YYYY")) Then
                Return DateInterval.Year
            End If
            End If
Label_0184:
            textArray1 = New String() {"Interval"}
            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray1))
        End Function

        Public Shared Function DatePart(Interval As DateInterval, DateValue As DateTime, Optional FirstDayOfWeekValue As FirstDayOfWeek = 1, Optional FirstWeekOfYearValue As FirstWeekOfYear = 1) As Integer
            Dim calendarWeekRule As CalendarWeekRule
            Dim firstDayOfWeek As DayOfWeek
            Select Case Interval
                Case DateInterval.Year
                    Return DateAndTime.CurrentCalendar.GetYear(DateValue)
                Case DateInterval.Quarter
                    Return (((DateValue.Month - 1) / 3) + 1)
                Case DateInterval.Month
                    Return DateAndTime.CurrentCalendar.GetMonth(DateValue)
                Case DateInterval.DayOfYear
                    Return DateAndTime.CurrentCalendar.GetDayOfYear(DateValue)
                Case DateInterval.Day
                    Return DateAndTime.CurrentCalendar.GetDayOfMonth(DateValue)
                Case DateInterval.WeekOfYear
                    If (FirstDayOfWeekValue <> firstDayOfWeek.System) Then
                        firstDayOfWeek = DirectCast((FirstDayOfWeekValue - 1), DayOfWeek)
                        Exit Select
                    End If
                    firstDayOfWeek = Utils.GetCultureInfo.DateTimeFormat.FirstDayOfWeek
                    Exit Select
                Case DateInterval.Weekday
                    Return DateAndTime.Weekday(DateValue, FirstDayOfWeekValue)
                Case DateInterval.Hour
                    Return DateAndTime.CurrentCalendar.GetHour(DateValue)
                Case DateInterval.Minute
                    Return DateAndTime.CurrentCalendar.GetMinute(DateValue)
                Case DateInterval.Second
                    Return DateAndTime.CurrentCalendar.GetSecond(DateValue)
                Case Else
                    Dim args As String() = New String() {"Interval"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End Select
            Select Case FirstWeekOfYearValue
                Case FirstWeekOfYear.System
                    calendarWeekRule = Utils.GetCultureInfo.DateTimeFormat.CalendarWeekRule
                    Exit Select
                Case FirstWeekOfYear.Jan1
                    calendarWeekRule = CalendarWeekRule.FirstDay
                    Exit Select
                Case FirstWeekOfYear.FirstFourDays
                    calendarWeekRule = CalendarWeekRule.FirstFourDayWeek
                    Exit Select
                Case FirstWeekOfYear.FirstFullWeek
                    calendarWeekRule = CalendarWeekRule.FirstFullWeek
                    Exit Select
            End Select
            Return DateAndTime.CurrentCalendar.GetWeekOfYear(DateValue, calendarWeekRule, firstDayOfWeek)
        End Function

        Public Shared Function DatePart(Interval As String, DateValue As Object, Optional DayOfWeek As FirstDayOfWeek = 1, Optional WeekOfYear As FirstWeekOfYear = 1) As Integer
            Dim time As DateTime
            Try
                time = Conversions.ToDate(DateValue)
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception6 As Exception
                Dim args As String() = New String() {"DateValue"}
                Throw New InvalidCastException(Utils.GetResourceString("Argument_InvalidDateValue1", args))
            End Try
            Return DateAndTime.DatePart(DateAndTime.DateIntervalFromString(Interval), time, DayOfWeek, WeekOfYear)
        End Function

        Public Shared Function DateSerial(Year As Integer, Month As Integer, Day As Integer) As DateTime
            Dim time2 As DateTime
            Dim currentCalendar As Calendar = DateAndTime.CurrentCalendar
            If (Year < 0) Then
                Year = (currentCalendar.GetYear(DateTime.Today) + Year)
            ElseIf (Year < 100) Then
                Year = currentCalendar.ToFourDigitYear(Year)
            End If
            If (((TypeOf currentCalendar Is GregorianCalendar AndAlso (Month >= 1)) AndAlso ((Month <= 12) AndAlso (Day >= 1))) AndAlso (Day <= &H1C)) Then
                Return New DateTime(Year, Month, Day)
            End If
            Try
                time2 = currentCalendar.ToDateTime(Year, 1, 1, 0, 0, 0, 0)
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception12 As Exception
                Dim args As String() = New String() {"Year"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args)), 5)
            End Try
            Try
                time2 = currentCalendar.AddMonths(time2, (Month - 1))
            Catch exception4 As StackOverflowException
                Throw exception4
            Catch exception5 As OutOfMemoryException
                Throw exception5
            Catch exception6 As ThreadAbortException
                Throw exception6
            Catch exception16 As Exception
                Dim textArray2 As String() = New String() {"Month"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2)), 5)
            End Try
            Try
                time2 = currentCalendar.AddDays(time2, (Day - 1))
            Catch exception7 As StackOverflowException
                Throw exception7
            Catch exception8 As OutOfMemoryException
                Throw exception8
            Catch exception9 As ThreadAbortException
                Throw exception9
            Catch exception20 As Exception
                Dim textArray3 As String() = New String() {"Day"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray3)), 5)
            End Try
            Return time2
        End Function

        Public Shared Function DateValue(StringDate As String) As DateTime
            Return Conversions.ToDate(StringDate).Date
        End Function

        Public Shared Function Day(DateValue As DateTime) As Integer
            Return DateAndTime.CurrentCalendar.GetDayOfMonth(DateValue)
        End Function

        Private Shared Function GetDayOfWeek(dt As DateTime, weekdayFirst As FirstDayOfWeek) As Integer
            If ((weekdayFirst < FirstDayOfWeek.System) OrElse (weekdayFirst > FirstDayOfWeek.Saturday)) Then
                Throw ExceptionUtils.VbMakeException(5)
            End If
            If (weekdayFirst = FirstDayOfWeek.System) Then
                weekdayFirst = DirectCast((Utils.GetDateTimeFormatInfo.FirstDayOfWeek + 1), FirstDayOfWeek)
            End If
            Return (CInt((((dt.DayOfWeek - DirectCast(CInt(weekdayFirst), DayOfWeek)) + 8) Mod (DayOfWeek.Saturday Or DayOfWeek.Monday))) + 1)
        End Function

        Public Shared Function Hour(TimeValue As DateTime) As Integer
            Return DateAndTime.CurrentCalendar.GetHour(TimeValue)
        End Function

        Private Shared Function IsDBCSCulture() As Boolean
            If (Marshal.SystemMaxDBCSCharSize = 1) Then
                Return False
            End If
            Return True
        End Function

        Public Shared Function Minute(TimeValue As DateTime) As Integer
            Return DateAndTime.CurrentCalendar.GetMinute(TimeValue)
        End Function

        Public Shared Function Month(DateValue As DateTime) As Integer
            Return DateAndTime.CurrentCalendar.GetMonth(DateValue)
        End Function

        Public Shared Function MonthName(Month As Integer, Optional Abbreviate As Boolean = False) As String
            Dim abbreviatedMonthName As String
            If ((Month < 1) OrElse (Month > 13)) Then
                Dim args As String() = New String() {"Month"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If Abbreviate Then
                abbreviatedMonthName = Utils.GetDateTimeFormatInfo.GetAbbreviatedMonthName(Month)
            Else
                abbreviatedMonthName = Utils.GetDateTimeFormatInfo.GetMonthName(Month)
            End If
            If (abbreviatedMonthName.Length = 0) Then
                Dim textArray2 As String() = New String() {"Month"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2))
            End If
            Return abbreviatedMonthName
        End Function

        Public Shared Function Second(TimeValue As DateTime) As Integer
            Return DateAndTime.CurrentCalendar.GetSecond(TimeValue)
        End Function

        Public Shared Function TimeSerial(Hour As Integer, Minute As Integer, Second As Integer) As DateTime
            Dim num As Integer = ((((Hour * 60) * 60) + (Minute * 60)) + Second)
            If (num < 0) Then
                num = (num + &H15180)
            End If
            Return New DateTime((num * &H989680))
        End Function

        Public Shared Function TimeValue(StringTime As String) As DateTime
            Return New DateTime((Conversions.ToDate(StringTime).Ticks Mod &HC92A69C000))
        End Function

        Public Shared Function Weekday(DateValue As DateTime, Optional DayOfWeek As FirstDayOfWeek = 1) As Integer
            If (DayOfWeek = FirstDayOfWeek.System) Then
                DayOfWeek = DirectCast((DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek + 1), FirstDayOfWeek)
            ElseIf ((DayOfWeek < FirstDayOfWeek.Sunday) OrElse (DayOfWeek > FirstDayOfWeek.Saturday)) Then
                Dim args As String() = New String() {"DayOfWeek"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Return (CInt(((((DateAndTime.CurrentCalendar.GetDayOfWeek(DateValue) + 1) - DirectCast(CInt(DayOfWeek), DayOfWeek)) + 7) Mod (DayOfWeek.Saturday Or DayOfWeek.Monday))) + 1)
        End Function

        Public Shared Function WeekdayName(Weekday As Integer, Optional Abbreviate As Boolean = False, Optional FirstDayOfWeekValue As FirstDayOfWeek = 0) As String
            Dim abbreviatedDayName As String
            If ((Weekday < 1) OrElse (Weekday > 7)) Then
                Dim args As String() = New String() {"Weekday"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If ((FirstDayOfWeekValue < FirstDayOfWeek.System) OrElse (FirstDayOfWeekValue > FirstDayOfWeek.Saturday)) Then
                Dim textArray2 As String() = New String() {"FirstDayOfWeekValue"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2))
            End If
            Dim format As DateTimeFormatInfo = DirectCast(Utils.GetCultureInfo.GetFormat(GetType(DateTimeFormatInfo)), DateTimeFormatInfo)
            If (FirstDayOfWeekValue = FirstDayOfWeek.System) Then
                FirstDayOfWeekValue = DirectCast((format.FirstDayOfWeek + 1), FirstDayOfWeek)
            End If
            Try
                If Abbreviate Then
                    abbreviatedDayName = format.GetAbbreviatedDayName(DirectCast((((Weekday + FirstDayOfWeekValue) - 2) Mod 7), DayOfWeek))
                Else
                    abbreviatedDayName = format.GetDayName(DirectCast((((Weekday + FirstDayOfWeekValue) - 2) Mod 7), DayOfWeek))
                End If
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception6 As Exception
                Dim textArray3 As String() = New String() {"Weekday"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray3))
            End Try
            If (abbreviatedDayName.Length = 0) Then
                Dim textArray4 As String() = New String() {"Weekday"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray4))
            End If
            Return abbreviatedDayName
        End Function

        Public Shared Function Year(DateValue As DateTime) As Integer
            Return DateAndTime.CurrentCalendar.GetYear(DateValue)
        End Function


        ' Properties
        Private Shared ReadOnly Property CurrentCalendar As Calendar
            Get
                Return Thread.CurrentThread.CurrentCulture.Calendar
            End Get
        End Property

        Public Shared Property DateString As String
            Get
                If DateAndTime.IsDBCSCulture Then
                    Return DateTime.Today.ToString("yyyy\-MM\-dd", Utils.GetInvariantCultureInfo)
                End If
                Return DateTime.Today.ToString("MM\-dd\-yyyy", Utils.GetInvariantCultureInfo)
            End Get
            <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
            Set(Value As String)
                Dim time As DateTime
                Try
                    Dim s As String = Utils.ToHalfwidthNumbers(Value, Utils.GetCultureInfo)
                    If DateAndTime.IsDBCSCulture Then
                        time = DateTime.ParseExact(s, DateAndTime.AcceptedDateFormatsDBCS, Utils.GetInvariantCultureInfo, DateTimeStyles.AllowWhiteSpaces)
                    Else
                        time = DateTime.ParseExact(s, DateAndTime.AcceptedDateFormatsSBCS, Utils.GetInvariantCultureInfo, DateTimeStyles.AllowWhiteSpaces)
                    End If
                Catch exception As StackOverflowException
                    Throw exception
                Catch exception2 As OutOfMemoryException
                    Throw exception2
                Catch exception3 As ThreadAbortException
                    Throw exception3
                Catch exception6 As Exception
                    Dim args As String() = New String() {Strings.Left(Value, &H20), "Date"}
                    Throw ExceptionUtils.VbMakeException(New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args)), 5)
                End Try
                Utils.SetDate(time)
            End Set
        End Property

        Public Shared ReadOnly Property Now As DateTime
            Get
                Return DateTime.Now
            End Get
        End Property

        Public Shared Property TimeOfDay As DateTime
            Get
                Dim ticks As Long = DateTime.Now.TimeOfDay.Ticks
                Return New DateTime((ticks - (ticks Mod &H989680)))
            End Get
            <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
            Set(Value As DateTime)
                Utils.SetTime(Value)
            End Set
        End Property

        Public Shared ReadOnly Property Timer As Double
            Get
                Return (CDbl((DateTime.Now.Ticks Mod &HC92A69C000)) / 10000000)
            End Get
        End Property

        Public Shared Property TimeString As String
            Get
                Dim time As New DateTime(DateTime.Now.TimeOfDay.Ticks)
                Return time.ToString("HH:mm:ss", Utils.GetInvariantCultureInfo)
            End Get
            <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
            Set(Value As String)
                Dim time As DateTime
                Try
                    time = DateType.FromString(Value, Utils.GetInvariantCultureInfo)
                Catch exception As StackOverflowException
                    Throw exception
                Catch exception2 As OutOfMemoryException
                    Throw exception2
                Catch exception3 As ThreadAbortException
                    Throw exception3
                Catch exception6 As Exception
                    Dim args As String() = New String() {Strings.Left(Value, &H20), "Date"}
                    Throw ExceptionUtils.VbMakeException(New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args)), 5)
                End Try
                Utils.SetTime(time)
            End Set
        End Property

        Public Shared Property Today As DateTime
            Get
                Return DateTime.Today
            End Get
            <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
            Set(Value As DateTime)
                Utils.SetDate(Value)
            End Set
        End Property


        ' Fields
        Private Shared AcceptedDateFormatsDBCS As String() = New String() { "yyyy-M-d", "y-M-d", "yyyy/M/d", "y/M/d" }
        Private Shared AcceptedDateFormatsSBCS As String() = New String() { "M-d-yyyy", "M-d-y", "M/d/yyyy", "M/d/y" }
    End Class
End Namespace

