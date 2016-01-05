Namespace Framework.DynamicCode

    Friend Module DynamicInvoke

        Public Function GetMethod(Type As System.Reflection.TypeInfo, Name As String) As System.Reflection.MethodInfo
            Dim LQuery = From Method In Type.GetMethods Where String.Equals(Method.Name, Name) Select Method '
            Return LQuery.First
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Assembly"></param>
        ''' <param name="TypeId">将要查找的目标对象</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function [GetType](Assembly As System.Reflection.Assembly, TypeId As String) As System.Reflection.TypeInfo()
            Dim LQuery = From Type As System.Reflection.TypeInfo In Assembly.DefinedTypes Where String.Equals(TypeId, Type.Name) Select Type '
            Return LQuery.ToArray
        End Function
    End Module
End Namespace