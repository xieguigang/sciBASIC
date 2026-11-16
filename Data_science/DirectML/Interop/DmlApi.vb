' ------------------------------------------------------------------------
' DirectML 原生互操作：DMLCreateDevice 导出与 IDML* 接口的 vtable 槽位
'
' 槽位号来自 Windows SDK DirectML.h 的接口声明顺序（已逐项核对）：
'   IDMLObject      3 GetPrivateData, 4 SetPrivateData, 5 SetPrivateDataInterface, 6 SetName
'   IDMLDeviceChild 7 GetDevice
'   IDMLDispatchable 8 GetBindingProperties
'   IDMLOperatorInitializer 9 Reset
'   IDMLBindingTable 8 BindInputs, 9 BindOutputs, 10 BindTemporaryResource,
'                    11 BindPersistentResource, 12 Reset
'   IDMLCommandRecorder 8 RecordDispatch
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>DirectML 导出函数与 Vtbl 槽位。</summary>
    Public Module DmlApi

        ' ---- IDMLDevice ----
        Public Const SlotCheckFeatureSupport As Integer = 5
        Public Const SlotCreateOperator As Integer = 6
        Public Const SlotCompileOperator As Integer = 7
        Public Const SlotCreateOperatorInitializer As Integer = 8
        Public Const SlotCreateCommandRecorder As Integer = 9
        Public Const SlotCreateBindingTable As Integer = 10
        Public Const SlotGetDeviceRemovedReason As Integer = 13

        ' ---- IDMLDispatchable ----
        Public Const SlotGetBindingProperties As Integer = 8

        ' ---- IDMLOperatorInitializer ----
        Public Const SlotInitializerReset As Integer = 9

        ' ---- IDMLBindingTable ----
        Public Const SlotBindInputs As Integer = 8
        Public Const SlotBindOutputs As Integer = 9
        Public Const SlotBindTemporaryResource As Integer = 10
        Public Const SlotBindPersistentResource As Integer = 11
        Public Const SlotBindingTableReset As Integer = 12

        ' ---- IDMLCommandRecorder ----
        Public Const SlotRecordDispatch As Integer = 8

        ''' <summary>DMLCreateDevice</summary>
        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DMLCreateDeviceFn(d3d12Device As IntPtr,
                                                   flags As DML_CREATE_DEVICE_FLAGS,
                                                   ByRef riid As Guid,
                                                   ByRef ppv As IntPtr) As Integer

        ''' <summary>DMLCreateDevice1（可指定最低特性级别）</summary>
        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DMLCreateDevice1Fn(d3d12Device As IntPtr,
                                                    flags As DML_CREATE_DEVICE_FLAGS,
                                                    minimumFeatureLevel As DML_FEATURE_LEVEL,
                                                    ByRef riid As Guid,
                                                    ByRef ppv As IntPtr) As Integer

        ' ---- IDMLDevice ----

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DmlCheckFeatureSupportFn(device As IntPtr, feature As DML_FEATURE,
                                                          queryDataSize As UInteger, queryData As IntPtr,
                                                          supportDataSize As UInteger, supportData As IntPtr) As Integer

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DmlCreateOperatorFn(device As IntPtr, desc As IntPtr,
                                                     ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DmlCompileOperatorFn(device As IntPtr, op As IntPtr, flags As DML_EXECUTION_FLAGS,
                                                      ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DmlCreateOperatorInitializerFn(device As IntPtr, operatorCount As UInteger,
                                                                operators As IntPtr,
                                                                ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DmlCreateCommandRecorderFn(device As IntPtr, ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DmlCreateBindingTableFn(device As IntPtr, desc As IntPtr,
                                                         ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DmlGetDeviceRemovedReasonFn(device As IntPtr) As Integer

        ' ---- IDMLDispatchable ----

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DmlGetBindingPropertiesFn(dispatchable As IntPtr) As DML_BINDING_PROPERTIES

        ' ---- IDMLOperatorInitializer ----

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DmlInitializerResetFn(initializer As IntPtr, operatorCount As UInteger, operators As IntPtr) As Integer

        ' ---- IDMLBindingTable ----

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Sub DmlBindArrayFn(bindingTable As IntPtr, bindingCount As UInteger, bindings As IntPtr)

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Sub DmlBindResourceFn(bindingTable As IntPtr, binding As IntPtr)

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function DmlBindingTableResetFn(bindingTable As IntPtr, desc As IntPtr) As Integer

        ' ---- IDMLCommandRecorder ----

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Sub DmlRecordDispatchFn(recorder As IntPtr, commandList As IntPtr,
                                                dispatchable As IntPtr, bindings As IntPtr)

    End Module

End Namespace
