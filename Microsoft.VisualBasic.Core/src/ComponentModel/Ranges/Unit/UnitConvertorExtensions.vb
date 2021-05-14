Imports System.Runtime.CompilerServices

Namespace ComponentModel.Ranges.Unit

    <HideModuleName>
    Public Module UnitConvertorExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetUnitConvertor(Of T As Structure)() As UnitTag(Of T)()
            Return Enums(Of T)() _
                .Select(Function(e)
                            Dim size As Double = CDbl(CObj(e))
                            Return New UnitTag(Of T)(e, size)
                        End Function) _
                .OrderBy(Function(u) u.value) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function Base(Of T)(convertors As IEnumerable(Of UnitTag(Of T))) As UnitTag(Of T)
            Return convertors.Where(Function(u) u.value = 1.0#).FirstOrDefault
        End Function

        <Extension>
        Friend Function IndexOf(Of T)(convertors As UnitTag(Of T)(), target As T) As Integer
            For i As Integer = 0 To convertors.Length - 1
                If (convertors(i).unit.Equals(target)) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Unit(Of T As Structure)(value#, unitVal As T) As UnitValue(Of T)
            Return New UnitValue(Of T)(value, unitVal)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Unit(Of T As Structure)(value&, unitVal As T) As UnitValue(Of T)
            Return New UnitValue(Of T)(value, unitVal)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Unit(Of T As Structure)(value%, unitVal As T) As UnitValue(Of T)
            Return New UnitValue(Of T)(value, unitVal)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Unit(Of T As Structure)(value!, unitVal As T) As UnitValue(Of T)
            Return New UnitValue(Of T)(value, unitVal)
        End Function
    End Module

End Namespace