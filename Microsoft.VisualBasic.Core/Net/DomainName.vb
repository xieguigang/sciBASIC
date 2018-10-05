Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Net

    Public Structure DomainName : Implements IKeyValuePairObject(Of String, String)

        Public Property Domain As String Implements IKeyValuePairObject(Of String, String).Key
        ''' <summary>
        ''' 顶级域名
        ''' </summary>
        ''' <returns></returns>
        Public Property TLD As String Implements IKeyValuePairObject(Of String, String).Value

        Public ReadOnly Property Invalid As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return (String.IsNullOrEmpty(Domain) OrElse String.IsNullOrEmpty(TLD))
            End Get
        End Property

        Sub New(url As String)
            Dim tokens As String() = TryParse(url).Split(CChar("."))
            Domain = tokens(0)
            TLD = tokens.Skip(1).JoinBy(".")
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Domain}.{TLD}"
        End Function
    End Structure
End Namespace