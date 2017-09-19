Namespace Drawing2D.Colors

' lighter(term, percentage)
' darker(term, percentage)
' alpha(term, percentage)
' reverse(term)
' skip(term, n)
' take(term, n)

Public Structure DesignerExpression

Dim Term$
Dim Modification As NamedValue(Of String)


Public Overrides Function ToString() As String 

If Modification.StringEmpty() Then
 Return Term
 Else
 With Modification
 If .Value.StringEmpty Then
  Return  $"{.Name} ( {Term} )"
 Else
  Return  $"{.Name} ( {Term}, {.Value} )"
 End If

 End With
End If

End Function

Delegate Function Apply(colors As Color(), value$) As Color()


Friend Shared Readonly actions As New Dictionary(Of String, Apply) From {
	{"lighter", new Apply(AddressOf lighter)},
	{"darker", new Apply(AddressOf darker)},
	{"alpha", new Apply(AddressOf alpha)},
	{"reverse", new Apply(AddressOf reverse)},
	{"skip", new Apply(AddressOf skip)},
	{"take", new Apply(AddressOf take)}
}

Public Function Modify(colors As Color()) As Color()


End Function

private shared function lighter(colors As Color(), value$) As Color()
End function

private shared  function darker(colors As Color(), value$) As color()

End function 

private shared  function alpha (colors as color(), value$) as color()
return colors.alpha(255 * val(value))
end function

private shared  function reverse(colors as color(), value$)  as color()
return colors.reverse.toarray
end function

private  shared function skip (colors as  color(), value$)  as  color()
return colors.skip(cint(val(value))).toarray
end function 

private  shared function take(colors  as color(), value$) as color()

return colors.Take(cint(val(value))).toarray
end function 

End Structure

End Namespace