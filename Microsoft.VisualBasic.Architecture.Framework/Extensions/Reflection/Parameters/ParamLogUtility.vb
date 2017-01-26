Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Web.Script.Serialization

Namespace Emit.Parameters

    ''' <summary>
    ''' Exception is a common issue in projects. To track this exception, we use error loggers 
    ''' which only log the exception detail and some other information if you want to. 
    ''' But hardly do we get any idea for which input set(parameters and its values) a 
    ''' particular method is throwing the error.
    ''' </summary>
    ''' <remarks>
    ''' https://www.codeproject.com/tips/795865/log-all-parameters-that-were-passed-to-some-method
    ''' </remarks>
    Public Module ParamLogUtility

        Public Function Acquire(ParamArray providedParameters As Expression(Of Func(Of Object))()) As Dictionary(Of String, Object)
            Dim _methodName As String
            Dim _paramaterLog As String

            Dim _serializer As JavaScriptSerializer
            Dim _methodParamaters As Dictionary(Of String, Type)
            Dim _providedParametars As List(Of Tuple(Of String, Type, Object))

            Try
                _serializer = New JavaScriptSerializer()
                Dim currentMethod = New StackTrace().GetFrame(1).GetMethod()

                'Set class and current method info

                _methodName = String.Format("Class = {0}, Method = {1}", currentMethod.DeclaringType.FullName, currentMethod.Name)

                'Get current methods paramaters

                _methodParamaters = New Dictionary(Of String, Type)()
                Call (From aParamater In currentMethod.GetParameters Select New With {
                     .Name = aParamater.Name,
                     .DataType = aParamater.ParameterType
                }).ToList().ForEach(Sub(obj) _methodParamaters.Add(obj.Name, obj.DataType))

                'Get provided methods paramaters

                _providedParametars = New List(Of Tuple(Of String, Type, Object))()
                For Each aExpression In providedParameters
                    Dim bodyType As Expression = aExpression.Body

                    If TypeOf bodyType Is MemberExpression Then
                        AddProvidedParamaterDetail(DirectCast(aExpression.Body, MemberExpression))
                    ElseIf TypeOf bodyType Is UnaryExpression Then
                        Dim unaryExpression As UnaryExpression = DirectCast(aExpression.Body, UnaryExpression)
                        AddProvidedParamaterDetail(DirectCast(unaryExpression.Operand, MemberExpression))
                    Else
                        Throw New Exception("Expression type unknown.")
                    End If
                Next

                'Process log for all method parameters


                ProcessLog()
            Catch exception As Exception
                Throw New Exception("Error in paramater log processing.", exception)
            End Try
        End Function

        Private Sub ProcessLog()
            Try
                For Each aMethodParamater In _methodParamaters
                    Dim aParameter = _providedParametars.Where(Function(obj) obj.Item1.Equals(aMethodParamater.Key) AndAlso obj.Item2 = aMethodParamater.Value).[Single]()
                    _paramaterLog += String.Format(" ""{0}"":{1},", aParameter.Item1, _serializer.Serialize(aParameter.Item3))
                Next

                _paramaterLog = If((_paramaterLog IsNot Nothing), _paramaterLog.Trim(" "c, ","c), String.Empty)
            Catch exception As Exception
                Throw New Exception("MathodParamater is not found in providedParameters.")
            End Try
        End Sub

        Private Sub AddProvidedParamaterDetail(memberExpression As MemberExpression)
            Dim constantExpression As ConstantExpression = DirectCast(memberExpression.Expression, ConstantExpression)
            Dim name = memberExpression.Member.Name
            Dim value = DirectCast(memberExpression.Member, FieldInfo).GetValue(constantExpression.Value)
            Dim type = value.[GetType]()
            name = name.Replace("$VB$Local_", "")
            _providedParametars.Add(New Tuple(Of String, Type, Object)(name, type, value))
        End Sub

        Public Function GetLog() As String
            Return String.Format("{0}({1})", _methodName, _paramaterLog)
        End Function
    End Module
End Namespace
