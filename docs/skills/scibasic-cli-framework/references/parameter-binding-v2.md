# v2 Parameter Binding Algorithm

Source: `g:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\CommandLine\Reflection\EntryPoints\Delegate.vb` (class `APIDelegate`), plus supporting files.

Getting this wrong causes parameters to silently keep defaults or throw at invocation. Follow the algorithm exactly.

## Internal State

- `_NumberOfParameters`, `_metaData As Binding(Of ExportApiModel, MethodInfo)`, `__funcInvoker As Func(Of Object(), Integer)` (:70-72).
- Constructor (:110-115) sets `_NumberOfParameters = attribute.Target.GetParameters.Length` (:114).
- Note (:169-171): if the target is a `Function`, the exit code is its return value; if a `Sub`, exit code is always 0.

## `Execute` Main Flow (:175-231)

1. `Dim callParameters(_NumberOfParameters - 1) As Object` (:176); `pars = _metaData.Target.GetParameters` (:177).
2. For each parameter i (:179-228):
   - `type = pars(i).ParameterType`, `name = pars(i).Name`, `parAlias = pars(i).GetCustomAttribute(Of Parameter)` (:180-182).
   - `name = MatchArgumentName(args, name, parAlias)` (:184) → returns the **original CLI key (with prefix)** or `Nothing`.
3. **No match** (:186-208):
   - If `type Is GetType(CommandLine)` → inject `args` (:189-196). The 20240418 comment: this check must precede `IsOptional`, else an unmatched optional `CommandLine` parameter defaults to `Nothing` and the object is lost.
   - Else if `pars(i).IsOptional` → use `pars(i).DefaultValue` (:197-198).
   - Else missing required argument (:200-207): display name is `"--" & pars(i).Name` (no alias) or `parAlias.Alias` (with alias); throws `MissingMemberException($"missing the required argument({name}) inside your commandline input!")`.
4. **Match found** (:209-227):
   - **Boolean special case** (:212-219): `value = args.GetValue(Of Boolean)(name, True)`. The 20261110 comment: a bare switch like `--no-passive` is parsed into `BoolFlags` and exposed by `GetEnumerator()` with placeholder `"true"`, but the indexer `args(name)` returns empty string; `CTypeDynamic("")` yields `Nothing` which CLR-casts to `False`, silently ignoring the switch. Boolean must use `GetValue` to consult `BoolFlags`.
   - **Other types** (:221): `val_str = args(name)` then `value = Scripting.CTypeDynamic(val_str, type)` (:223).
   - Write `callParameters(i)` (:226).
5. `Return __funcInvoker.Invoke(callParameters)` (:230).

## `MatchArgumentName` (:240-260)

```vbnet
Private Shared Function MatchArgumentName(args As CommandLine, name As String, [alias] As Parameter) As String
    If Not [alias] Is Nothing Then
        name = [alias].Alias.TrimParamPrefix
    End If
    Dim matched As String = FindMatch(args.Keys, name)
    If matched Is Nothing AndAlso args.BoolFlags IsNot Nothing AndAlso args.BoolFlags.Length > 0 Then
        matched = FindMatch(args.BoolFlags, name)
    End If
    Return matched
End Function
```

- If a `Parameter` alias attribute exists, use `alias.Alias.TrimParamPrefix` as the name (:241-243).
- 20261105 rule (:245-248): CLI names may be `--aa-bb`, but `-` is illegal in VB identifiers; source uses `aa_bb`. Replace `-` with `_` when comparing.
- 20261110 rule (:250-252): a bare switch is in `BoolFlags` and absent from `CommandLine.Keys`, so both collections must be scanned or the parameter is treated as not supplied and keeps its default.
- Scan `args.Keys` first (:253); only if no match and `BoolFlags` is non-empty, scan `args.BoolFlags` (:255-257).

## `FindMatch` (:262-274)

```vbnet
If cli_names.IsNullOrEmpty Then Return Nothing
For Each cli_name As String In cli_names
    If cli_name.TrimParamPrefix.Replace("-", "_").TextEquals(name) Then
        Return cli_name
    End If
Next
Return Nothing
```

Core comparison (:268):

1. `TrimParamPrefix` on the **CLI-side name** (strips `--`, `-`, `\`, `/`).
2. Replace `-` with `_`.
3. `TextEquals` (case-insensitive) against the VB parameter name.
4. On a hit, return the **un-normalized original CLI key** (with prefix); the indexer resolves the prefix later.

## Supporting Details

- `Parameter` attribute (`g:\...\src\Scripting\MetaData\Parameter.vb:69`): `<AttributeUsage(Parameter, AllowMultiple:=False, Inherited:=True)>` (:68). `Alias` getter falls back to `ParameterInfo.Name` when `_alias` is empty (:90-92). Constructor `Sub New([Alias] As String, Optional MyDescription As String = "")` (:115-118).
- `TrimParamPrefix` (`g:\...\src\CommandLine\CLITools.vb:355-363`): `--` → `Mid(,3)`; `-`, `\`, `/` → `Mid(,2)`; else unchanged.
- `CommandLine.Keys` (`CommandLine.vb:156-161`): only `arguments.Select(Name).ToArray`, **excludes BoolFlags**.
- `CommandLine.GetValue(Of T)` (`CommandLine.vb:627-646`): absent → Boolean + `HavebFlag(name)` returns `True`, otherwise default; present → `InputHandler.CTypeDynamic(str, GetType(T))` (:641).
- Indexer `Item(paramName)` (`CommandLine.vb:235-247`): matches `a.Name.TextEquals(paramName)` or `a.Name.TrimNamePrefix.TextEquals(paramName)`, returns `DefaultString` (never null).
- `CTypeDynamic(expression$, target As Type)` (`g:\...\src\Scripting\InputHandler.vb:197-232`): `String` returns directly; empty or `"null"` → `Nothing`; custom converters `_CasterString` (:204-207); else `Conversion.CTypeDynamic(expression, target)` (:221), exceptions logged and return `Nothing` (:220-231).

## Practical Checklist for v2 Parameters

- Name VB parameters to match the CLI tokens after prefix-strip and `-`→`_` (e.g. `--no-passive` → `no_passive`).
- Declare booleans as `Boolean` (with default) so the `BoolFlags` path is used.
- Give optional parameters explicit defaults; use nullable value types when "not supplied" must differ from zero.
- Use `<Parameter("--alias")>` when CLI name and VB identifier cannot be aligned.
- Type a parameter as `CommandLine` to receive the whole parsed object inside a v2 subcommand.
- Remember missing required parameters fail at invocation, not at startup.
