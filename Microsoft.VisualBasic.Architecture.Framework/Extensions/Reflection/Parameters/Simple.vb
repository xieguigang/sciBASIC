Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace Emit.Parameters

    Partial Module ParamLogUtility

        ''' <summary>
        ''' 这个方法要求传递进来的参数的顺序要和原函数的参数的顺序一致，故而不太推荐使用这个方法
        ''' </summary>
        ''' <param name="parameters"></param>
        Public Function AcquireOrder(ParamArray parameters As Object()) As Dictionary(Of Value)
            Dim invoke As MethodBase = New StackTrace().GetFrame(1).GetMethod()
            Dim trace As New NamedValue(Of MethodBase) With {
                .Name = invoke.Name,
                .Value = invoke
            }
            Dim methodParameters = invoke.GetParameters()
            Dim out As New Dictionary(Of Value)

            For Each aMethodParameter As ParameterInfo In methodParameters
                Dim value As Object =
                    parameters(aMethodParameter.Position)
                out += New Value With {
                    .Name = aMethodParameter.Name,
                    .Trace = trace,
                    .Type = aMethodParameter.ParameterType,
                    .value = value
                }
            Next

            Return out
        End Function
    End Module
End Namespace
