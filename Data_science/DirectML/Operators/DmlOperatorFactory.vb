' ------------------------------------------------------------------------
' 算子工厂的基础设施
'
' 所有工厂函数都遵循同一个形状：
'   Build(算子类型, 输入张量, 输出张量, 名称, 描述符构造闭包)
' 闭包内部用 builder 申请张量描述符与参数数组，Build 负责把
' "具体描述符" 包成 DML_OPERATOR_DESC 并交给 DmlOperatorDefinition 托管生命周期。
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Operators

    ''' <summary>算子工厂的公共入口。</summary>
    Public Module DmlOperatorFactory

        ''' <summary>
        ''' 构造一个算子定义。
        ''' </summary>
        ''' <param name="type">算子类型。</param>
        ''' <param name="inputs">输入张量。</param>
        ''' <param name="outputs">输出张量。</param>
        ''' <param name="name">调试名称。</param>
        ''' <param name="buildDesc">返回具体算子描述符指针的闭包。</param>
        Public Function Build(type As DML_OPERATOR_TYPE,
                              inputs As DmlTensor(),
                              outputs As DmlTensor(),
                              name As String,
                              buildDesc As Func(Of DmlOperatorBuilder, IntPtr)) As DmlOperatorDefinition

            Dim builder As New DmlOperatorBuilder()

            Try
                Dim descPtr As IntPtr = buildDesc(builder)
                Dim desc As IntPtr = builder.OperatorDesc(type, descPtr)

                Return New DmlOperatorDefinition(type, desc, inputs, outputs, builder, name)
            Catch
                builder.Dispose()
                Throw
            End Try
        End Function

        ''' <summary>取得张量描述符指针的简写。</summary>
        Public Function Desc(tensor As DmlTensor, builder As DmlOperatorBuilder) As IntPtr
            If tensor Is Nothing Then
                Return IntPtr.Zero
            End If

            Return tensor.WriteDescriptor(builder)
        End Function

        ''' <summary>把布尔值转成原生 BOOL。</summary>
        Public Function BoolToInt(value As Boolean) As Integer
            Return If(value, 1, 0)
        End Function

    End Module

End Namespace
