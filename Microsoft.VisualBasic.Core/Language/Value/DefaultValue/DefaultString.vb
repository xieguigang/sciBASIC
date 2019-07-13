#Region "Microsoft.VisualBasic::c0c300c7751c675905ed4b93e9c9d6f8, Microsoft.VisualBasic.Core\Language\Value\DefaultValue\DefaultString.vb"

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

    '     Structure DefaultString
    ' 
    '         Properties: DefaultValue, IsEmpty, IsTrue
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: assertIsNothing, LoadJson, LoadXml, ReadAllLines, ToString
    '         Operators: (+2 Overloads) IsFalse, (+2 Overloads) IsTrue, (+8 Overloads) Or
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON
Imports CLI = Microsoft.VisualBasic.CommandLine.CommandLine

Namespace Language.Default

    ''' <summary>
    ''' <see cref="CLI"/> optional value helper data model
    ''' </summary>
    Public Structure DefaultString : Implements IDefault(Of String)
        Implements IsEmpty

        ''' <summary>
        ''' The optional argument value that read from <see cref="CLI"/> 
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DefaultValue As String Implements IDefault(Of String).DefaultValue

        ''' <summary>
        ''' <see cref="DefaultValue"/> is empty?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean Implements IsEmpty.IsEmpty
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return DefaultValue.StringEmpty
            End Get
        End Property

        Public ReadOnly Property IsTrue As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return DefaultValue.ParseBoolean
            End Get
        End Property

        Sub New([string] As String)
            DefaultValue = [string]
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LoadXml(Of T)() As T
            Return DefaultValue.LoadXml(Of T)
        End Function

        ''' <summary>
        ''' 如果文件不存在或者文本内容为空，则函数返回空值
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LoadJson(Of T)() As T
            If DefaultValue.FileExists Then
                Return DefaultValue.ReadAllText.LoadJSON(Of T)
            ElseIf DefaultValue.StringEmpty Then
                Return Nothing
            Else
                Return DefaultValue.LoadJSON(Of T)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadAllLines() As String()
            Return DefaultValue.ReadAllLines
        End Function

        Public Overrides Function ToString() As String
            Return DefaultValue
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function assertIsNothing(s As String) As Boolean
            Return s Is Nothing OrElse String.IsNullOrEmpty(s)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(str As DefaultString) As Boolean
            Return str.DefaultValue.ParseBoolean
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(str As DefaultString) As Integer
            Return str.DefaultValue.ParseDouble
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(str As DefaultString) As Double
            Return str.DefaultValue.ParseDouble
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(str As DefaultString) As Long
            Return str.DefaultValue.ParseDouble
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(str As DefaultString) As Single
            Return str.DefaultValue.ParseDouble
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(str As DefaultString) As Short
            Return str.DefaultValue.ParseDouble
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(str As String) As DefaultString
            Return New DefaultString(str)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator IsTrue(str As DefaultString) As Boolean
            Return CType(str, Boolean)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator IsFalse(str As DefaultString) As Boolean
            Return False = CType(str, Boolean)
        End Operator

        ''' <summary>
        ''' If <paramref name="value"/> is empty then returns <paramref name="default"/>, 
        ''' else returns <paramref name="value"/> itself.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="default$"></param>
        ''' <returns></returns>
        Public Shared Operator Or(value As DefaultString, default$) As String
            If assertIsNothing(value.DefaultValue) Then
                Return [default]
            Else
                Return value.DefaultValue
            End If
        End Operator

        ''' <summary>
        ''' Get a <see cref="Integer"/> value or using default <see cref="Integer"/> value.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="x%"></param>
        ''' <returns></returns>
        Public Shared Operator Or(value As DefaultString, x%) As Integer
            Return CInt(value Or CDbl(x))
        End Operator

        Public Shared Operator Or(value As DefaultString, x#) As Double
            If assertIsNothing(value.DefaultValue) Then
                Return x
            Else
                Return Val(value.DefaultValue)
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Or(arg As DefaultString, [default] As [Default](Of String)) As String
            Return arg.DefaultValue Or [default]
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(value As DefaultString) As String
            Return value.DefaultValue
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator &(s1 As DefaultString, s2$) As String
            Return s1.DefaultValue & s2
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator &(s1$, s2 As DefaultString) As String
            Return s1 & s2.DefaultValue
        End Operator
    End Structure
End Namespace
