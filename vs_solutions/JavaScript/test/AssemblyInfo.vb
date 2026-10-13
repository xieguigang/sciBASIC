Imports Xunit

' the engine binds console output through a per-thread static sink; running test
' classes in parallel would still race on the interpreter's global environment,
' so the suite runs serially and deterministically
<Assembly: CollectionBehavior(DisableTestParallelization:=True)>
