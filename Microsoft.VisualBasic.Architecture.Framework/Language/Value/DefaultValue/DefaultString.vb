Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Perl
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language.Default

    Public Structure DefaultString : Implements IDefaultValue(Of String)

        Public ReadOnly Property DefaultValue As String Implements IDefaultValue(Of String).DefaultValue

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return DefaultValue.StringEmpty
            End Get
        End Property

        Sub New([string] As String)
            DefaultValue = [string]
        End Sub

        Public Function LoadXml(Of T)() As T
            Return DefaultValue.LoadXml(Of T)
        End Function

        Public Function LoadJson(Of T)() As T
            If DefaultValue.FileExists Then
                Return DefaultValue.ReadAllText.LoadObject(Of T)
            Else
                Return DefaultValue.LoadObject(Of T)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return DefaultValue
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function assertIsNothing(s As String) As Boolean
            Return s Is Nothing OrElse String.IsNullOrEmpty(s)
        End Function

        Public Shared Operator Or(value As DefaultString, default$) As String
            If assertIsNothing(value.DefaultValue) Then
                Return [default]
            Else
                Return value.DefaultValue
            End If
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