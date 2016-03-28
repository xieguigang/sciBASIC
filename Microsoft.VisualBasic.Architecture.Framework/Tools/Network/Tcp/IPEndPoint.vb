Imports System.ComponentModel
Imports System.Xml.Serialization

Namespace Net

    ''' <summary>
    ''' The object of <see cref="System.Net.IPEndPoint"/> can not be Xml serialization.
    ''' (系统自带的<see cref="System.Net.IPEndPoint"></see>不能够进行Xml序列化)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class IPEndPoint

#Region "Public Property"

        <Browsable(True)>
        <Description("Guid value of this portal information on the server registry.")>
        <XmlAttribute> Public Property uid As String
        <Browsable(True)>
        <Description("IPAddress of the services instance.")>
        <XmlAttribute> Public Property IPAddress As String
        <Browsable(True)>
        <Description("Data port of the services instance.")>
        <XmlAttribute> Public Property Port As Integer
#End Region

        ''' <summary>
        ''' This parameterless constructor is required for the xml serialization.(XML序列化所需要的)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="IPAddress">IPAddress string using for create object using method <see cref="System.Net.IPAddress.Parse(String)"/></param>
        ''' <param name="Port"><see cref="System.Net.IPEndPoint.Port"/></param>
        Sub New(IPAddress As String, Port As Integer)
            Me.Port = Port
            Me.IPAddress = IPAddress
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="str">IPAddress:Port</param>
        ''' <remarks></remarks>
        Sub New(str As String)
            Dim Tokens As String() = str.Split(":"c)

            If Tokens.IsNullOrEmpty OrElse Tokens.Length < 2 Then
                Throw New DataException(str & " is not a valid IPEndPoint string value!")
            End If

            IPAddress = Tokens.First
            Port = CInt(Val(Tokens(1)))
        End Sub

        Sub New(ipEnd As System.Net.IPEndPoint)
            Call Me.New(ipEnd.ToString)
        End Sub

        ''' <summary>
        ''' http://IPAddress:&lt;Port>/
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"http://{IPAddress}:{Port}/"
        End Function

        ''' <summary>
        ''' Convert this networking end point DDM into the <see cref="System.Net.IPEndPoint"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function GetIPEndPoint() As System.Net.IPEndPoint
            Return New System.Net.IPEndPoint(System.Net.IPAddress.Parse(ipString:=IPAddress), Port)
        End Function

        Public Function GetValue() As String
            Return IPAddress & ":" & Port.ToString
        End Function

        ''' <summary>
        ''' 格式是否正确
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsValid As Boolean
            Get
                Return Port > 0 AndAlso System.Net.IPAddress.TryParse(IPAddress, Nothing)
            End Get
        End Property

        Public Shared Narrowing Operator CType(ep As IPEndPoint) As System.Net.IPEndPoint
            Return ep.GetIPEndPoint
        End Operator
    End Class
End Namespace