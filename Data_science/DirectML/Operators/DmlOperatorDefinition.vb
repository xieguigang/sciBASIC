' ------------------------------------------------------------------------
' 算子定义：一个 DML_OPERATOR_DESC + 它的输入/输出张量
'
' 生命周期：
'   Build  -> 持有非托管描述符内存
'   CreateOperator(DescPtr) -> DML 复制描述符内容
'   Dispose -> 释放描述符内存（编译完成后即可释放）
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Operators

    ''' <summary>
    ''' 一个待编译的 DirectML 算子：算子类型、原生描述符指针与输入/输出张量。
    ''' </summary>
    Public NotInheritable Class DmlOperatorDefinition
        Implements IDisposable

        Private ReadOnly _builder As DmlOperatorBuilder
        Private _disposed As Boolean

        ''' <summary>创建算子定义。</summary>
        ''' <param name="type">算子类型。</param>
        ''' <param name="descPtr">指向 <c>DML_OPERATOR_DESC</c> 的指针。</param>
        ''' <param name="inputs">输入张量（顺序与 DML 描述符一致）。</param>
        ''' <param name="outputs">输出张量。</param>
        ''' <param name="builder">持有描述符内存的构建器。</param>
        ''' <param name="opName">调试名称（通常是 ONNX 节点名）。</param>
        Friend Sub New(type As DML_OPERATOR_TYPE,
                       descPtr As IntPtr,
                       inputs As DmlTensor(),
                       outputs As DmlTensor(),
                       builder As DmlOperatorBuilder,
                       Optional opName As String = Nothing)

            Me.Type = type
            Me.DescPtr = descPtr
            Me.Inputs = If(inputs, New DmlTensor() {})
            Me.Outputs = If(outputs, New DmlTensor() {})
            Me.OperatorName = opName
            Me._builder = builder
        End Sub

        ''' <summary>算子类型。</summary>
        Public ReadOnly Property Type As DML_OPERATOR_TYPE

        ''' <summary>指向 <c>DML_OPERATOR_DESC</c> 的指针。</summary>
        Public ReadOnly Property DescPtr As IntPtr

        ''' <summary>输入张量。</summary>
        Public ReadOnly Property Inputs As DmlTensor()

        ''' <summary>输出张量。</summary>
        Public ReadOnly Property Outputs As DmlTensor()

        ''' <summary>调试名称。</summary>
        Public ReadOnly Property OperatorName As String

        ''' <summary>释放描述符内存。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then
                Return
            End If

            _disposed = True

            If _builder IsNot Nothing Then
                _builder.Dispose()
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Type}({If(OperatorName, "")})"
        End Function

    End Class

End Namespace
