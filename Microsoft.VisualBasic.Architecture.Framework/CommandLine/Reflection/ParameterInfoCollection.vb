Imports System.Reflection
Imports System.Text

Namespace CommandLine.Reflection

    ''' <summary>
    ''' The help information for a specific command line parameter switch.(某一个指定的命令的开关的帮助信息)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ParameterInfoCollection

        Implements System.Collections.Generic.IEnumerable(Of KeyValuePair(Of String, Reflection.ParameterInfo))

        Dim _ParametersDict As Dictionary(Of String, Reflection.ParameterInfo) =
            New Dictionary(Of String, ParameterInfo)

        ''' <summary>
        ''' 本命令行对象中的包含有帮助信息的开关参数的数目
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As Integer
            Get
                Return _ParametersDict.Count
            End Get
        End Property

        ''' <summary>
        ''' Returns the parameter switch help information with the specific name value.(显示某一个指定名称的开关信息)
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Default Public ReadOnly Property Parameter(Name As String) As String
            Get
                Return _ParametersDict(Name).ToString
            End Get
        End Property

        ''' <summary>
        ''' Gets the usage example of this parameter switch.(获取本参数开关的帮助信息)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property GetExample() As String
            Get
                Dim RequiredSwitchs = (From switch In Me._ParametersDict.Values Where switch.Optional = False Select switch).ToArray
                Dim OptionalSwitchs = (From switch In Me._ParametersDict.Values Where switch.Optional Select switch).ToArray
                Dim sBuilder As StringBuilder = New StringBuilder(1024)
                For Each Switch As ParameterInfo In RequiredSwitchs
                    Call sBuilder.AppendFormat("{0} {1} ", Switch.Name, Switch.Example)
                Next
                For Each Switch As ParameterInfo In OptionalSwitchs
                    Call sBuilder.AppendFormat("[{0} {1}] ", Switch.Name, Switch.Example)
                Next

                Return sBuilder.ToString.Trim
            End Get
        End Property

        Public ReadOnly Property GetUsage() As String
            Get
                Dim requiredParameters = (From parameter In Me._ParametersDict.Values Where parameter.Optional = False Select parameter).ToArray
                Dim optionalParameters = (From parameter In Me._ParametersDict.Values Where parameter.Optional Select parameter).ToArray
                Dim sBuilder As StringBuilder = New StringBuilder(1024)
                For Each Switch As ParameterInfo In requiredParameters
                    Call sBuilder.AppendFormat("{0} {1} ", Switch.Name, Switch.Usage)
                Next
                For Each Switch As ParameterInfo In optionalParameters
                    Call sBuilder.AppendFormat("[{0} {1}] ", Switch.Name, Switch.Usage)
                Next

                Return sBuilder.ToString.Trim
            End Get
        End Property

        Public ReadOnly Property EmptyUsage As Boolean
            Get
                Dim LQuery = From switch In _ParametersDict.Values Where String.IsNullOrEmpty(switch.Usage) Select 1 '
                Return LQuery.Sum = _ParametersDict.Count
            End Get
        End Property

        Public ReadOnly Property EmptyExample As Boolean
            Get
                Dim LQuery = From switch In _ParametersDict.Values Where String.IsNullOrEmpty(switch.Example) Select 1 '
                Return LQuery.Sum = _ParametersDict.Count
            End Get
        End Property

        ''' <summary>
        ''' 显示所有的开关信息
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Dim sBuilder As StringBuilder = New StringBuilder(1024)
            For Each parameter As Reflection.ParameterInfo In _ParametersDict.Values
                Call sBuilder.AppendLine(parameter.ToString)
            Next
            Return sBuilder.ToString
        End Function

        Sub New(methodInfo As System.Reflection.MethodInfo)
            Dim switchInfo As System.Type = GetType(Reflection.ParameterInfo)
            Dim switchsObject = methodInfo.GetCustomAttributes(switchInfo, inherit:=False)
            Dim LQuery = From sw As Object In switchsObject
                         Let parameter As ParameterInfo = TryCast(sw, Reflection.ParameterInfo)
                         Select parameter
                         Order By parameter.Optional Ascending '

            For Each param As ParameterInfo In LQuery.ToArray
                Call _ParametersDict.Add(param.Name, param)
            Next
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, ParameterInfo)) _
            Implements IEnumerable(Of KeyValuePair(Of String, ParameterInfo)).GetEnumerator
            For Each obj In Me._ParametersDict
                Yield obj
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace