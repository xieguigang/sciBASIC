Imports System.IO

Namespace Numpy

    ''' <summary>
    ''' numpy ndarray 在 pickle 流中的占位对象。
    ''' </summary>
    ''' <remarks>
    ''' numpy 通过 <c>numpy.core.multiarray._reconstruct(ndarray, (0,), b'b')</c> 构造一个空数组，
    ''' 再以 <c>__setstate__((version, shape, dtype, is_fortran, data))</c> 填充真实的形状与数据。
    ''' 数据在 <see cref="NumpyPickleFactory.ApplyState"/> 阶段才可用，因此这里先返回占位对象，
    ''' 随后把占位对象整体替换为真正的 <see cref="NpyArray"/>。
    ''' </remarks>
    Friend Class NpyArrayPlaceholder
    End Class

    ''' <summary>
    ''' <see cref="Pickle.IPickleObjectFactory"/> 的 numpy 实现：把 pickle 流中的
    ''' <c>numpy.ndarray</c> / <c>numpy.dtype</c> / <c>numpy.core.multiarray.scalar</c> 还原为
    ''' <see cref="NpyArray"/>、<see cref="NpyDType"/> 与 .NET 标量。
    ''' </summary>
    ''' <remarks>
    ''' 典型用法由 <see cref="NpyFile.Read(String)"/> 内部完成；直接使用 pickle 反序列化器时：
    ''' 
    ''' <code>
    ''' Dim obj = Pickle.MinimalPickleUnpickler.UnpickleFromBytes(bytes, New NumpyPickleFactory())
    ''' </code>
    ''' </remarks>
    Public Class NumpyPickleFactory
        Implements Pickle.IPickleObjectFactory

        ''' <summary>
        ''' 识别 numpy 全局符号并构造对应的 .NET 对象。
        ''' </summary>
        ''' <param name="moduleName">Python 模块名</param>
        ''' <param name="className">Python 类/函数名</param>
        ''' <param name="args">构造参数</param>
        ''' <returns>占位数组 / dtype / 标量；非 numpy 符号返回 <c>Nothing</c></returns>
        Public Function Construct(moduleName As String, className As String, args As Object()) As Object Implements Pickle.IPickleObjectFactory.Construct
            Dim fullName As String = moduleName & "." & className

            Select Case fullName
                Case "numpy.core.multiarray._reconstruct", "numpy.ndarray", "numpy.core.ndarray"
                    Return New NpyArrayPlaceholder()

                Case "numpy.dtype", "numpy.core.multiarray.dtype"
                    If args Is Nothing OrElse args.Length = 0 Then
                        Return Nothing
                    Else
                        Return NpyDType.FromLiteral(args(0))
                    End If

                Case "numpy.core.multiarray.scalar"
                    Return DecodeScalar(args)

                Case Else
                    Return Nothing
            End Select
        End Function

        ''' <summary>
        ''' 用 numpy 的 <c>__setstate__</c> 状态还原数组或补全 dtype。
        ''' </summary>
        ''' <param name="obj">占位数组或者 dtype 对象</param>
        ''' <param name="state">状态元组</param>
        ''' <returns>是否已处理</returns>
        Public Function ApplyState(ByRef obj As Object, state As Object) As Boolean Implements Pickle.IPickleObjectFactory.ApplyState
            Dim tuple As Object() = ToTupleItems(state)

            If tuple Is Nothing Then
                Return False
            End If

            If TypeOf obj Is NpyArrayPlaceholder Then
                obj = RestoreArray(tuple)
                Return True
            ElseIf TypeOf obj Is NpyDType Then
                Call ApplyDTypeState(DirectCast(obj, NpyDType), tuple)
                Return True
            Else
                Return False
            End If
        End Function

        Private Function RestoreArray(state As Object()) As NpyArray
            If state.Length < 5 Then
                Throw New InvalidDataException($"invalid numpy ndarray pickle state: expected 5 items but {state.Length} were found!")
            End If

            Dim dtype As NpyDType = TryCast(state(2), NpyDType)

            If dtype Is Nothing Then
                Throw New InvalidDataException("invalid numpy ndarray pickle state: the dtype item is missing!")
            End If

            Dim result As New NpyArray With {
                .DType = dtype,
                .Shape = ToShape(state(1)),
                .FortranOrder = CBool(state(3))
            }

            If dtype.Kind = NpyTypeKind.PyObject Then
                result.Objects = ToObjects(state(4))
            Else
                Dim raw As Byte() = TryCast(state(4), Byte())

                If raw Is Nothing Then
                    Dim text As String = TryCast(state(4), String)

                    If text Is Nothing Then
                        Throw New InvalidDataException("invalid numpy ndarray pickle state: the data item is neither bytes nor string!")
                    End If

                    raw = System.Text.Encoding.Latin1.GetBytes(text)
                End If

                result.RawData = raw
            End If

            Return result
        End Function

        Private Sub ApplyDTypeState(dtype As NpyDType, state As Object())
            Dim endianText As String = TryCast(state(0), String)

            If String.IsNullOrEmpty(endianText) Then
                Return
            End If

            Select Case endianText(0)
                Case "<"c
                    dtype.Endian = NpyEndian.LittleEndian
                Case ">"c
                    dtype.Endian = NpyEndian.BigEndian
                Case "|"c
                    dtype.Endian = NpyEndian.NotApplicable
                Case Else
                    dtype.Endian = NpyEndian.Native
            End Select
        End Sub

        ''' <summary>
        ''' 把 pickle 的元组对象（<see cref="Pickle.PythonTuple"/> 或者 <c>Object()</c>）展开为对象数组。
        ''' </summary>
        Private Function ToTupleItems(raw As Object) As Object()
            If raw Is Nothing Then
                Return Nothing
            End If

            Dim pyTuple As Pickle.PythonTuple = TryCast(raw, Pickle.PythonTuple)

            If pyTuple IsNot Nothing Then
                Return pyTuple.Items
            Else
                Return TryCast(raw, Object())
            End If
        End Function

        Private Function ToShape(raw As Object) As Integer()
            Dim [tuple] As Object() = ToTupleItems(raw)

            If [tuple] Is Nothing Then
                Dim [value] As Long = CLng(raw)

                Return New Integer() {CInt([value])}
            End If

            Dim result As Integer() = New Integer([tuple].Length - 1) {}

            For i As Integer = 0 To [tuple].Length - 1
                result(i) = CInt(CLng([tuple](i)))
            Next

            Return result
        End Function

        Private Function ToObjects(raw As Object) As Object()
            Dim items As IList = TryCast(raw, IList)

            If items Is Nothing Then
                Dim buffer As Object() = TryCast(raw, Object())

                If buffer IsNot Nothing Then
                    Return buffer
                Else
                    Return New Object() {raw}
                End If
            End If

            Dim result As Object() = New Object(items.Count - 1) {}

            Call items.CopyTo(result, 0)

            Return result
        End Function

        Private Function DecodeScalar(args As Object()) As Object
            If args Is Nothing OrElse args.Length < 2 Then
                Return Nothing
            End If

            Dim dtype As NpyDType = TryCast(args(0), NpyDType)
            Dim raw As Byte() = TryCast(args(1), Byte())

            If dtype Is Nothing Then
                Return Nothing
            End If

            If raw Is Nothing Then
                Dim text As String = TryCast(args(1), String)

                If text Is Nothing Then
                    Return Nothing
                End If

                raw = System.Text.Encoding.Latin1.GetBytes(text)
            End If

            Dim scalar As New NpyArray With {
                .DType = dtype,
                .Shape = New Integer() {1},
                .RawData = raw
            }

            Return scalar.GetValue(0)
        End Function

    End Class

End Namespace
