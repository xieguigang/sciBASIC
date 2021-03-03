Imports System

Namespace Xdr
    Friend Module ErrorStub
        Private Function StubDelegate(ex As Exception, method As String, targetType As Type, genDelegateType As Type) As [Delegate]
            Dim stubType = GetType(ErrorStubType(Of)).MakeGenericType(targetType)
            Dim stubInstance = Activator.CreateInstance(stubType, ex)
            Dim mi = stubType.GetMethod(method)
            Return [Delegate].CreateDelegate(genDelegateType.MakeGenericType(targetType), stubInstance, mi)
        End Function

        Friend Function ReadOneDelegate(t As Type, ex As Exception) As [Delegate]
            Return StubDelegate(ex, "ReadOne", t, GetType(ReadOneDelegate(Of)))
        End Function

        Friend Function ReadManyDelegate(t As Type, ex As Exception) As [Delegate]
            Return StubDelegate(ex, "ReadMany", t, GetType(ReadManyDelegate(Of)))
        End Function

        Friend Function WriteOneDelegate(t As Type, ex As Exception) As [Delegate]
            Return StubDelegate(ex, "WriteOne", t, GetType(WriteOneDelegate(Of)))
        End Function

        Friend Function WriteManyDelegate(t As Type, ex As Exception) As [Delegate]
            Return StubDelegate(ex, "WriteMany", t, GetType(WriteManyDelegate(Of)))
        End Function
    End Module
End Namespace
