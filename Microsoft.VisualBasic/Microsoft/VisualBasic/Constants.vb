Imports Microsoft.VisualBasic.CompilerServices
Imports System

Namespace Microsoft.VisualBasic
    <StandardModule, DynamicallyInvokableAttribute> _
    Public NotInheritable Class Constants
        ' Fields
        Public Const vbAbort As MsgBoxResult = MsgBoxResult.Abort
        Public Const vbAbortRetryIgnore As MsgBoxStyle = MsgBoxStyle.AbortRetryIgnore
        Public Const vbApplicationModal As MsgBoxStyle = MsgBoxStyle.ApplicationModal
        Public Const vbArchive As FileAttribute = FileAttribute.Archive
        Public Const vbArray As VariantType = VariantType.Array
        <DynamicallyInvokableAttribute> _
        Public Const vbBack As String = ChrW(8)
        Public Const vbBinaryCompare As CompareMethod = CompareMethod.Binary
        Public Const vbBoolean As VariantType = VariantType.Boolean
        Public Const vbByte As VariantType = VariantType.Byte
        Public Const vbCancel As MsgBoxResult = MsgBoxResult.Cancel
        <DynamicallyInvokableAttribute> _
        Public Const vbCr As String = ChrW(13)
        Public Const vbCritical As MsgBoxStyle = MsgBoxStyle.Critical
        <DynamicallyInvokableAttribute> _
        Public Const vbCrLf As String = ChrW(13) & ChrW(10)
        Public Const vbCurrency As VariantType = VariantType.Currency
        Public Const vbDate As VariantType = VariantType.Date
        Public Const vbDecimal As VariantType = VariantType.Decimal
        Public Const vbDefaultButton1 As MsgBoxStyle = MsgBoxStyle.ApplicationModal
        Public Const vbDefaultButton2 As MsgBoxStyle = MsgBoxStyle.DefaultButton2
        Public Const vbDefaultButton3 As MsgBoxStyle = MsgBoxStyle.DefaultButton3
        Public Const vbDirectory As FileAttribute = FileAttribute.Directory
        Public Const vbDouble As VariantType = VariantType.Double
        Public Const vbEmpty As VariantType = VariantType.Empty
        Public Const vbExclamation As MsgBoxStyle = MsgBoxStyle.Exclamation
        Public Const vbFalse As TriState = TriState.False
        Public Const vbFirstFourDays As FirstWeekOfYear = FirstWeekOfYear.FirstFourDays
        Public Const vbFirstFullWeek As FirstWeekOfYear = FirstWeekOfYear.FirstFullWeek
        Public Const vbFirstJan1 As FirstWeekOfYear = FirstWeekOfYear.Jan1
        <DynamicallyInvokableAttribute> _
        Public Const vbFormFeed As String = ChrW(12)
        Public Const vbFriday As FirstDayOfWeek = FirstDayOfWeek.Friday
        Public Const vbGeneralDate As DateFormat = DateFormat.GeneralDate
        Public Const vbGet As CallType = CallType.Get
        Public Const vbHidden As FileAttribute = FileAttribute.Hidden
        Public Const vbHide As AppWinStyle = AppWinStyle.Hide
        Public Const vbHiragana As VbStrConv = VbStrConv.Hiragana
        Public Const vbIgnore As MsgBoxResult = MsgBoxResult.Ignore
        Public Const vbInformation As MsgBoxStyle = MsgBoxStyle.Information
        Public Const vbInteger As VariantType = VariantType.Integer
        Public Const vbKatakana As VbStrConv = VbStrConv.Katakana
        Public Const vbLet As CallType = CallType.Let
        <DynamicallyInvokableAttribute> _
        Public Const vbLf As String = ChrW(10)
        Public Const vbLinguisticCasing As VbStrConv = VbStrConv.LinguisticCasing
        Public Const vbLong As VariantType = VariantType.Long
        Public Const vbLongDate As DateFormat = DateFormat.LongDate
        Public Const vbLongTime As DateFormat = DateFormat.LongTime
        Public Const vbLowerCase As VbStrConv = VbStrConv.Lowercase
        Public Const vbMaximizedFocus As AppWinStyle = AppWinStyle.MaximizedFocus
        Public Const vbMethod As CallType = CallType.Method
        Public Const vbMinimizedFocus As AppWinStyle = AppWinStyle.MinimizedFocus
        Public Const vbMinimizedNoFocus As AppWinStyle = AppWinStyle.MinimizedNoFocus
        Public Const vbMonday As FirstDayOfWeek = FirstDayOfWeek.Monday
        Public Const vbMsgBoxHelp As MsgBoxStyle = MsgBoxStyle.MsgBoxHelp
        Public Const vbMsgBoxRight As MsgBoxStyle = MsgBoxStyle.MsgBoxRight
        Public Const vbMsgBoxRtlReading As MsgBoxStyle = MsgBoxStyle.MsgBoxRtlReading
        Public Const vbMsgBoxSetForeground As MsgBoxStyle = MsgBoxStyle.MsgBoxSetForeground
        Public Const vbNarrow As VbStrConv = VbStrConv.Narrow
        <DynamicallyInvokableAttribute> _
        Public Const vbNewLine As String = ChrW(13) & ChrW(10)
        Public Const vbNo As MsgBoxResult = MsgBoxResult.No
        Public Const vbNormal As FileAttribute = FileAttribute.Normal
        Public Const vbNormalFocus As AppWinStyle = AppWinStyle.NormalFocus
        Public Const vbNormalNoFocus As AppWinStyle = AppWinStyle.NormalNoFocus
        Public Const vbNull As VariantType = VariantType.Null
        <DynamicallyInvokableAttribute> _
        Public Const vbNullChar As String = ChrW(0)
        <DynamicallyInvokableAttribute> _
        Public Const vbNullString As String = Nothing
        Public Const vbObject As VariantType = VariantType.Object
        Public Const vbObjectError As Integer = -2147221504
        Public Const vbOK As MsgBoxResult = MsgBoxResult.Ok
        Public Const vbOKCancel As MsgBoxStyle = MsgBoxStyle.OkCancel
        Public Const vbOKOnly As MsgBoxStyle = MsgBoxStyle.ApplicationModal
        Public Const vbProperCase As VbStrConv = VbStrConv.ProperCase
        Public Const vbQuestion As MsgBoxStyle = MsgBoxStyle.Question
        Public Const vbReadOnly As FileAttribute = FileAttribute.ReadOnly
        Public Const vbRetry As MsgBoxResult = MsgBoxResult.Retry
        Public Const vbRetryCancel As MsgBoxStyle = MsgBoxStyle.RetryCancel
        Public Const vbSaturday As FirstDayOfWeek = FirstDayOfWeek.Saturday
        Public Const vbSet As CallType = CallType.Set
        Public Const vbShortDate As DateFormat = DateFormat.ShortDate
        Public Const vbShortTime As DateFormat = DateFormat.ShortTime
        Public Const vbSimplifiedChinese As VbStrConv = VbStrConv.SimplifiedChinese
        Public Const vbSingle As VariantType = VariantType.Single
        Public Const vbString As VariantType = VariantType.String
        Public Const vbSunday As FirstDayOfWeek = FirstDayOfWeek.Sunday
        Public Const vbSystem As FileAttribute = FileAttribute.System
        Public Const vbSystemModal As MsgBoxStyle = MsgBoxStyle.SystemModal
        <DynamicallyInvokableAttribute> _
        Public Const vbTab As String = ChrW(9)
        Public Const vbTextCompare As CompareMethod = CompareMethod.Text
        Public Const vbThursday As FirstDayOfWeek = FirstDayOfWeek.Thursday
        Public Const vbTraditionalChinese As VbStrConv = VbStrConv.TraditionalChinese
        Public Const vbTrue As TriState = TriState.True
        Public Const vbTuesday As FirstDayOfWeek = FirstDayOfWeek.Tuesday
        Public Const vbUpperCase As VbStrConv = VbStrConv.Uppercase
        Public Const vbUseDefault As TriState = TriState.UseDefault
        Public Const vbUserDefinedType As VariantType = VariantType.UserDefinedType
        Public Const vbUseSystem As FirstWeekOfYear = FirstWeekOfYear.System
        Public Const vbUseSystemDayOfWeek As FirstDayOfWeek = FirstDayOfWeek.System
        Public Const vbVariant As VariantType = VariantType.Variant
        <DynamicallyInvokableAttribute> _
        Public Const vbVerticalTab As String = ChrW(11)
        Public Const vbVolume As FileAttribute = FileAttribute.Volume
        Public Const vbWednesday As FirstDayOfWeek = FirstDayOfWeek.Wednesday
        Public Const vbWide As VbStrConv = VbStrConv.Wide
        Public Const vbYes As MsgBoxResult = MsgBoxResult.Yes
        Public Const vbYesNo As MsgBoxStyle = MsgBoxStyle.YesNo
        Public Const vbYesNoCancel As MsgBoxStyle = MsgBoxStyle.YesNoCancel
    End Class
End Namespace

