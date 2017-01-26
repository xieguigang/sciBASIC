Imports System.Reflection
Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.Language

Namespace Emit.Parameters

    Partial Module ParamLogUtility

        ''' <summary>
        ''' 这个方法要求传递进来的参数的顺序要和原函数的参数的顺序一致，故而不太推荐使用这个方法
        ''' </summary>
        ''' <param name="parameters"></param>
        Public Function AcquireOrder(ParamArray parameters As Object()) As Dictionary(Of Value)
            Dim _methodName As String
            Dim _paramaterLog As String

            Dim _serializer As JavaScriptSerializer
            Dim _methodParameters As List(Of ParameterInfo)
            Dim _parameters As ArrayList
            _serializer = New JavaScriptSerializer()
            _methodParameters = New List(Of ParameterInfo)()
            _parameters = New ArrayList()

            Dim currentMethod = New StackTrace().GetFrame(1).GetMethod()
            _methodName = String.Format("Class = {0}, Method = {1}", currentMethod.DeclaringType.FullName, currentMethod.Name)
            Dim methodParameters = currentMethod.GetParameters()

            For Each aMethodParameter In methodParameters
                Dim aParameters = parameters(aMethodParameter.Position)
                _paramaterLog += String.Format(" ""{0}"":{1},", aMethodParameter.Name, _serializer.Serialize(aParameters))
            Next
            _paramaterLog = _paramaterLog.Trim(" "c, ","c)
        End Function
    End Module
End Namespace
