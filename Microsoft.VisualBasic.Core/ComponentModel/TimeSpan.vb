#Region "Microsoft.VisualBasic::cdf4edfa1d484a1bfe9c77f0aab04e58, Microsoft.VisualBasic.Core\ComponentModel\TimeSpan.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Structure TimeInterval
    ' 
    '         Properties: Days, Hours, Miliseconds, Minutes, Seconds
    '                     Ticks, TimeSpan
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization


Namespace ComponentModel

    <XmlTypeAttribute("Interval", Namespace:="Microsoft.VisualBasic/Numerical_DataStruct")>
    Public Structure TimeInterval

        <XmlAttribute("dd")> Public Property Days As Integer
        <XmlAttribute("min")> Public Property Minutes As Integer
        <XmlAttribute("hr")> Public Property Hours As Integer
        <XmlAttribute("ss")> Public Property Seconds As Integer
        <XmlAttribute("ms")> Public Property Miliseconds As Integer

        Sub New(TimeSpan As TimeSpan)
            Days = TimeSpan.Days
            Minutes = TimeSpan.Minutes
            Hours = TimeSpan.Hours
            Seconds = TimeSpan.Seconds
            Miliseconds = TimeSpan.Milliseconds
        End Sub

        Public ReadOnly Property TimeSpan As TimeSpan
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New TimeSpan(Days, Hours, Minutes, Seconds, Miliseconds)
            End Get
        End Property

        ''' <summary>
        ''' (dd hh:mm:ss) 输出可以被MySQL数据库所识别的字符串格式
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Return String.Format("{0} {1}:{2}:{3}", Days, Hours, Minutes, Seconds)
        End Function

        Public ReadOnly Property Ticks As Long
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return TimeSpan.Ticks
            End Get
        End Property

        Public Shared Widening Operator CType(TimeSpan As TimeSpan) As TimeInterval
            Return New TimeInterval(TimeSpan)
        End Operator
    End Structure
End Namespace
