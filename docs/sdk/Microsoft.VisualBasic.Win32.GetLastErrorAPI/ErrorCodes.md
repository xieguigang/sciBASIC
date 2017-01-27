# ErrorCodes
_namespace: [Microsoft.VisualBasic.Win32.GetLastErrorAPI](./index.md)_

This article lists the error codes you may encounter in Windows NT. For the remaining error codes, 
 please see the following article(s) in the Microsoft Knowledge Base:
 
 + [155011](https://support.microsoft.com/en-us/kb/155011) Error Codes in Windows NT Part 1 of 2
 + [155012](https://support.microsoft.com/EN-US/kb/155012) Error Codes in Windows NT Part 2 of 2
 (@``M:Microsoft.VisualBasic.Win32.GetLastErrorAPI.GetLastError``的返回值的含义)




### Properties

#### EPT_S_CANT_CREATE
The endpoint mapper database
 could not be created.
#### EPT_S_CANT_PERFORM_OP
The operation cannot be
 performed.
#### EPT_S_INVALID_ENTRY
The entry is invalid.
#### EPT_S_NOT_REGISTERED
There are no more endpoints
 available from the endpoint
 mapper.
#### ERROR_ACCESS_DENIED
Access is denied.
#### ERROR_ACCOUNT_DISABLED
The referenced account is
 currently disabled and cannot
 be logged on to.
#### ERROR_ACCOUNT_EXPIRED
The user's account has
 expired.
#### ERROR_ACCOUNT_LOCKED_OUT
The referenced account is
 currently locked out and may
 not be logged on to.
#### ERROR_ACCOUNT_RESTRICTION
Indicates a referenced user
 name and authentication
 information are valid, but
 some user account restriction
 has prevented successful
 authentication (such as time-
 of-day restrictions).
#### ERROR_ACTIVE_CONNECTIONS
Active connections still
 exist.
#### ERROR_ADAP_HDW_ERR
A network adapter hardware
 error occurred.
#### ERROR_ADDRESS_ALREADY_ASSOCIATED
The network transport endpoint
 already has an address
 associated with it.
#### ERROR_ADDRESS_NOT_ASSOCIATED
An address has not yet been
 associated with the network
 endpoint.
#### ERROR_ALIAS_EXISTS
The specified alias already
 exists.
#### ERROR_ALLOTTED_SPACE_EXCEEDED
When a block of memory is
 allotted for future updates,
 such as the memory allocated
 to hold discretionary access
 control and primary group
 information, successive
 updates may exceed the amount
 of memory originally allotted.
 Since quota may already have
 been charged to several
 processes that have handles of
 the object, it is not
 reasonable to alter the size
 of the allocated memory.
 Instead, a request that
 requires more memory than has
 been allotted must fail and
 the
 ERROR_ALLOTTED_SPACE_EXCEEDED
 error returned.
#### ERROR_ALREADY_ASSIGNED
The local device name is
 already in use.
#### ERROR_ALREADY_EXISTS
Attempt to create file that
 already exists.
#### ERROR_ALREADY_INITIALIZED
An attempt was made to perform
 an initialization operation
 when initialization has
 already been completed.
#### ERROR_ALREADY_REGISTERED
The service is already
 registered.
#### ERROR_ALREADY_RUNNING_LKG
The system is currently
 running with the last-known-
 good configuration.
#### ERROR_ALREADY_WAITING
The specified Printer handle
 is already being waited on.
#### ERROR_APP_WRONG_OS
The specified program is not a
 Windows or MS-DOS program.
#### ERROR_ARENA_TRASHED
The storage control blocks
 were destroyed.
#### ERROR_ARITHMETIC_OVERFLOW
Arithmetic result exceeded 32-
 bits.
#### ERROR_ATOMIC_LOCKS_NOT_SUPPORTED
The file system does not
 support atomic changing of the
 lock type.
#### ERROR_AUTODATASEG_EXCEEDS_64k
The operating system cannot
 run this application program.
#### ERROR_BAD_ARGUMENTS
The argument string passed to
 DosExecPgm is incorrect.
#### ERROR_BAD_COMMAND
The device does not recognize
 the command.
#### ERROR_BAD_DESCRIPTOR_FORMAT
Indicates a security
 descriptor is not in the
 required format (absolute or
 self-relative).
#### ERROR_BAD_DEV_TYPE
The network resource type is
 incorrect.
#### ERROR_BAD_DEVICE
The specified device name is
 invalid.
#### ERROR_BAD_DRIVER
The specified driver is
 invalid.
#### ERROR_BAD_DRIVER_LEVEL
The system does not support
 the requested command.
#### ERROR_BAD_ENVIRONMENT
The environment is incorrect.
#### ERROR_BAD_EXE_FORMAT
%1 is not a valid Windows-
 based application.
#### ERROR_BAD_FORMAT
An attempt was made to load a
 program with an incorrect
 format.
#### ERROR_BAD_IMPERSONATION_LEVEL
A specified impersonation
 level is invalid. Also used to
 indicate a required
 impersonation level was not
 provided.
#### ERROR_BAD_INHERITANCE_ACL
Indicates that an attempt to
 build either an inherited ACL
 or ACE did not succeed. One of
 the more probable causes is
 the replacement of a CreatorId
 with an SID that didn't fit
 into the ACE or ACL.
#### ERROR_BAD_LENGTH
The program issued a command
 but the command length is
 incorrect.
#### ERROR_BAD_LOGON_SESSION_STATE
The logon session is not in a
 state consistent with the
 requested operation.
#### ERROR_BAD_NET_NAME
The network name cannot be
 found.
#### ERROR_BAD_NET_RESP
The specified server cannot
 perform the requested
 operation.
#### ERROR_BAD_NETPATH
The network path was not
 found.
#### ERROR_BAD_PATHNAME
The specified path name is
 invalid.
#### ERROR_BAD_PIPE
The pipe state is invalid.
#### ERROR_BAD_PROFILE
The network connection profile
 is damaged.
#### ERROR_BAD_PROVIDER
The specified network provider
 name is invalid.
#### ERROR_BAD_REM_ADAP
The remote adapter is not
 compatible.
#### ERROR_BAD_THREADID_ADDR
The address for the thread ID
 is incorrect.
#### ERROR_BAD_TOKEN_TYPE
The type of token object is
 inappropriate for its
 attempted use.
#### ERROR_BAD_UNIT
The system cannot find the
 specified device.
#### ERROR_BAD_USERNAME
The specified user name is
 invalid.
#### ERROR_BAD_VALIDATION_CLASS
The requested validation
 information class is invalid.
#### ERROR_BADDB
The configuration registry
 database is damaged.
#### ERROR_BADKEY
The configuration registry key
 is invalid.
#### ERROR_BEGINNING_OF_MEDIA
The beginning of the tape or
 partition was encountered.
#### ERROR_BOOT_ALREADY_ACCEPTED
The current boot has already
 been accepted for use as the
 last-known-good control set.
#### ERROR_BROKEN_PIPE
The pipe was ended.
#### ERROR_BUFFER_OVERFLOW
The file name is too long.
#### ERROR_BUS_RESET
The I/O bus was reset.
#### ERROR_BUSY
The requested resource is in
 use.
#### ERROR_BUSY_DRIVE
The system cannot perform a
 JOIN or SUBST at this time.
#### ERROR_CALL_NOT_IMPLEMENTED
The Application Program
 Interface (API) entered will
 only work in Windows/NT mode.
#### ERROR_CAN_NOT_COMPLETE
Cannot complete function for
 some reason.
#### ERROR_CAN_NOT_DEL_LOCAL_WINS
The local WINS can not be
 deleted.
#### ERROR_CANCEL_VIOLATION
A lock request was not
 outstanding for the supplied
 cancel region.
#### ERROR_CANCELLED
The operation was cancelled by
 the user.
#### ERROR_CANNOT_COPY
The Copy API cannot be used.
#### ERROR_CANNOT_FIND_WND_CLASS
Cannot find window class.
#### ERROR_CANNOT_IMPERSONATE
Indicates that an attempt was
 made to impersonate via a
 named pipe was not yet read
 from.
#### ERROR_CANNOT_MAKE
The directory or file cannot
 be created.
#### ERROR_CANNOT_OPEN_PROFILE
Unable to open the network
 connection profile.
#### ERROR_CANT_ACCESS_DOMAIN_INFO
Indicates a domain controller
 could not be contacted or that
 objects within the domain are
 protected and necessary
 information could not be
 retrieved.
#### ERROR_CANT_DISABLE_MANDATORY
A mandatory group cannot be
 disabled.
#### ERROR_CANT_OPEN_ANONYMOUS
An attempt was made to open an
 anonymous level token.
 Anonymous tokens cannot be
 opened.
#### ERROR_CANTOPEN
The configuration registry key
 cannot be opened.
#### ERROR_CANTREAD
The configuration registry key
 cannot be read.
#### ERROR_CANTWRITE
The configuration registry key
 cannot be written.
#### ERROR_CHILD_MUST_BE_VOLATILE
An attempt was made to create
 a stable subkey under a
 volatile parent key.
#### ERROR_CHILD_NOT_COMPLETE
The %1 application cannot be
 run in Windows mode.
#### ERROR_CHILD_WINDOW_MENU
Child windows can't have
 menus.
#### ERROR_CIRCULAR_DEPENDENCY
Circular service dependency
 was specified.
#### ERROR_CLASS_ALREADY_EXISTS
Class already exists.
#### ERROR_CLASS_DOES_NOT_EXIST
Class does not exist.
#### ERROR_CLASS_HAS_WINDOWS
Class still has open windows.
#### ERROR_CLIPBOARD_NOT_OPEN
Thread doesn't have clipboard
 open.
#### ERROR_CLIPPING_NOT_SUPPORTED
The requested clipping
 operation is not supported.
#### ERROR_COMMITMENT_LIMIT
The paging file is too small
 for this operation to
 complete.
#### ERROR_CONNECTION_ABORTED
The network connection was
 aborted by the local system.
#### ERROR_CONNECTION_ACTIVE
An invalid operation was
 attempted on an active network
 connection.
#### ERROR_CONNECTION_COUNT_LIMIT
A connection to the server
 could not be made because the
 limit on the number of
 concurrent connections for
 this account has been reached.
#### ERROR_CONNECTION_INVALID
An operation was attempted on
 a non-existent network
 connection.
#### ERROR_CONNECTION_REFUSED
The remote system refused the
 network connection.
#### ERROR_CONNECTION_UNAVAIL
The device is not currently
 connected but is a remembered
 connection.
#### ERROR_CONTINUE
Return that wants caller to
 continue with work in
 progress.
#### ERROR_CONTROL_ID_NOT_FOUND
Control ID not found.
#### ERROR_COUNTER_TIMEOUT
A serial I/O operation
 completed because the time-out
 period expired. (The
 IOCTL_SERIAL_XOFF_COUNTER did
 not reach zero.)
#### ERROR_CRC
Data error (cyclic redundancy
 check).
#### ERROR_CURRENT_DIRECTORY
The directory cannot be
 removed.
#### ERROR_DATABASE_DOES_NOT_EXIST
The database specified does
 not exist.
#### ERROR_DC_NOT_FOUND
Invalid HDC passed to
 ReleaseDC.
#### ERROR_DDE_FAIL
An error occurred in sending
 the command to the
 application.
#### ERROR_DEPENDENT_SERVICES_RUNNING
A stop control has been sent
 to a service which other
 running services are dependent
 on.
#### ERROR_DESTROY_OBJECT_OF_OTHER_THREAD
Cannot destroy object created
 by another thread.
#### ERROR_DEV_NOT_EXIST
The specified network resource
 is no longer available.
#### ERROR_DEVICE_ALREADY_REMEMBERED
An attempt was made to
 remember a device that was
 previously remembered.
#### ERROR_DEVICE_IN_USE
The device is in use by an
 active process and cannot be
 disconnected.
#### ERROR_DEVICE_NOT_PARTITIONED
Tape partition information
 could not be found when
 loading a tape.
#### ERROR_DIFFERENT_SERVICE_ACCOUNT
The account specified for this
 service is different from the
 account specified for other
 services running in the same
 process.
#### ERROR_DIR_NOT_EMPTY
The directory is not empty.
#### ERROR_DIR_NOT_ROOT
The directory is not a
 subdirectory of the root
 directory.
#### ERROR_DIRECT_ACCESS_HANDLE
Attempt to use a file handle
 to an open disk partition for
 an operation other than raw
 disk I/O.
#### ERROR_DIRECTORY
The directory name is invalid.
#### ERROR_DISCARDED
The segment is already
 discarded and cannot be
 locked.
#### ERROR_DISK_CHANGE
Program stopped because
 alternate disk was not
 inserted.
#### ERROR_DISK_CORRUPT
The disk structure is damaged
 and nonreadable.
#### ERROR_DISK_FULL
There is not enough space on
 the disk.
#### ERROR_DISK_OPERATION_FAILED
While accessing the hard disk,
 a disk operation failed even
 after retries.
#### ERROR_DISK_RECALIBRATE_FAILED
While accessing the hard disk,
 a recalibrate operation
 failed, even after retries.
#### ERROR_DISK_RESET_FAILED
While accessing the hard disk,
 a disk controller reset was
 needed, but even that failed.
#### ERROR_DLL_INIT_FAILED
A DLL initialization routine
 failed.
#### ERROR_DLL_NOT_FOUND
One of the library files
 needed to run this application
 cannot be found.
#### ERROR_DOMAIN_CONTROLLER_NOT_FOUND
Could not find the domain
 controller for this domain.
#### ERROR_DOMAIN_EXISTS
The specified domain already
 exists.
#### ERROR_DOMAIN_LIMIT_EXCEEDED
An attempt to exceed the limit
 on the number of domains per
 server for this release.
#### ERROR_DOMAIN_TRUST_INCONSISTENT
The name or security ID (SID)
 of the domain specified is
 inconsistent with the trust
 information for that domain.
#### ERROR_DRIVE_LOCKED
The disk is in use or locked
 by another process.
#### ERROR_DUP_DOMAINNAME
The workgroup or domain name
 is already in use by another
 computer on the network.
#### ERROR_DUP_NAME
A duplicate name exists on the
 network.
#### ERROR_DUPLICATE_SERVICE_NAME
The name is already in use as
 either a service name or a
 service display name.
#### ERROR_DYNLINK_FROM_INVALID_RING
The operating system cannot
 run this application program.
#### ERROR_EA_ACCESS_DENIED
Access to the EA is denied.
#### ERROR_EA_FILE_CORRUPT
The EA file on the mounted
 file system is damaged.
#### ERROR_EA_LIST_INCONSISTENT
The EAs are inconsistent.
#### ERROR_EA_TABLE_FULL
The EA table in the EA file on
 the mounted file system is
 full.
#### ERROR_EAS_DIDNT_FIT
The EAs did not fit in the
 buffer.
#### ERROR_EAS_NOT_SUPPORTED
The mounted file system does
 not support extended
 attributes.
#### ERROR_END_OF_MEDIA
The physical end of the tape
 has been reached.
#### ERROR_ENVVAR_NOT_FOUND
The system could not find the
 environment option entered.
#### ERROR_EOM_OVERFLOW
Physical end of tape
 encountered.
#### ERROR_EVENTLOG_CANT_START
No event log file could be
 opened, so the event logging
 service did not start.
#### ERROR_EVENTLOG_FILE_CHANGED
The event log file has changed
 between reads.
#### ERROR_EVENTLOG_FILE_CORRUPT
One of the Eventlog logfiles
 is damaged.
#### ERROR_EXCEPTION_IN_SERVICE
An exception occurred in the
 service when handling the
 control request.
#### ERROR_EXCL_SEM_ALREADY_OWNED
The exclusive semaphore is
 owned by another process.
#### ERROR_EXE_MARKED_INVALID
The operating system cannot
 run %1.
#### ERROR_EXTENDED_ERROR
An extended error has
 occurred.
#### ERROR_FAIL_I24
Fail on INT 24.
#### ERROR_FAILED_SERVICE_CONTROLLER_CONNECT
The service process could
 not connect to the service
 controller.
#### ERROR_FILE_CORRUPT
The file or directory is
 damaged and nonreadable.
#### ERROR_FILE_EXISTS
The file exists.
#### ERROR_FILE_INVALID
The volume for a file was
 externally altered and the
 opened file is no longer
 valid.
#### ERROR_FILE_NOT_FOUND
The system cannot find the
 file specified.
#### ERROR_FILEMARK_DETECTED
A tape access reached a
 filemark.
#### ERROR_FILENAME_EXCED_RANGE
The file name or extension is
 too long.
#### ERROR_FLOPPY_BAD_REGISTERS
The floppy disk controller
 returned inconsistent results
 in its registers.
#### ERROR_FLOPPY_ID_MARK_NOT_FOUND
No ID address mark was found
 on the floppy disk.
#### ERROR_FLOPPY_UNKNOWN_ERROR
The floppy disk controller
 reported an error that is not
 recognized by the floppy disk
 driver.
#### ERROR_FLOPPY_WRONG_CYLINDER
Mismatch between the floppy
 disk sector ID field and the
 floppy disk controller track
 address.
#### ERROR_FULL_BACKUP
The backup failed. Check the
 directory that you are backing
 the database to.
#### ERROR_FULLSCREEN_MODE
The requested operation cannot
 be performed in full-screen
 mode.
#### ERROR_GEN_FAILURE
A device attached to the
 system is not functioning.
#### ERROR_GENERIC_NOT_MAPPED
Indicates generic access types
 were contained in an access
 mask that should already be
 mapped to non-generic access
 types.
#### ERROR_GLOBAL_ONLY_HOOK
This hook can only be set
 globally.
#### ERROR_GRACEFUL_DISCONNECT
The network connection was
 gracefully closed.
#### ERROR_GROUP_EXISTS
The specified group already
 exists.
#### ERROR_HANDLE_DISK_FULL
The disk is full.
#### ERROR_HANDLE_EOF
Reached End Of File.
#### ERROR_HOOK_NEEDS_HMOD
Cannot set non-local hook
 without an module handle.
#### ERROR_HOOK_NOT_INSTALLED
Hook is not installed.
#### ERROR_HOOK_TYPE_NOT_ALLOWED
Hook type not allowed.
#### ERROR_HOST_UNREACHABLE
The remote system is not
 reachable by the transport.
#### ERROR_HOTKEY_ALREADY_REGISTERED
Hotkey is already registered.
#### ERROR_HOTKEY_NOT_REGISTERED
Hotkey is not registered.
#### ERROR_HWNDS_HAVE_DIFFERENT_PARENT
All DeferWindowPos HWNDs must
 have same parent.
#### ERROR_ILL_FORMED_PASSWORD
When trying to update a
 password, this return status
 indicates the value provided
 for the new password contains
 values not allowed in
 passwords.
#### ERROR_INC_BACKUP
The backup failed. Was a full
 backup done before?
#### ERROR_INCORRECT_ADDRESS
The network address could not
 be used for the operation
 requested.
#### ERROR_INFLOOP_IN_RELOC_CHAIN
The operating system cannot
 run %1.
#### ERROR_INSUFFICIENT_BUFFER
The data area passed to a
 system call is too small.
#### ERROR_INTERNAL_DB_CORRUPTION
This error indicates the
 requested operation cannot be
 completed due to a
 catastrophic media failure or
 on-disk data structure
 corruption.
#### ERROR_INTERNAL_DB_ERROR
The Local Security Authority
 (LSA) database contains in
 internal inconsistency.
#### ERROR_INTERNAL_ERROR
This error indicates the SAM
 server has encounterred an
 internal consistency error in
 its database. This
 catastrophic failure prevents
 further operation of SAM.
#### ERROR_INVALID_ACCEL_HANDLE
Invalid accelerator-table
 handle.
#### ERROR_INVALID_ACCESS
The access code is invalid.
#### ERROR_INVALID_ACCOUNT_NAME
The name provided is not a
 properly formed account name.
#### ERROR_INVALID_ACL
Indicates the ACL structure is
 not valid.
#### ERROR_INVALID_ADDRESS
Attempt to access invalid
 address.
#### ERROR_INVALID_AT_INTERRUPT_TIME
Cannot request exclusive
 semaphores at interrupt time.
#### ERROR_INVALID_BLOCK
The storage control block
 address is invalid.
#### ERROR_INVALID_BLOCK_LENGTH
When accessing a new tape of a
 multivolume partition, the
 current block size is
 incorrect.
#### ERROR_INVALID_CATEGORY
The IOCTL call made by the
 application program is
 incorrect.
#### ERROR_INVALID_COMBOBOX_MESSAGE
Invalid Message, combo box
 doesn't have an edit control.
#### ERROR_INVALID_COMPUTERNAME
The format of the specified
 computer name is invalid.
#### ERROR_INVALID_CURSOR_HANDLE
The cursor handle is invalid.
#### ERROR_INVALID_DATA
The data is invalid.
#### ERROR_INVALID_DATATYPE
The specified datatype is
 invalid.
#### ERROR_INVALID_DLL
One of the library files
 needed to run this application
 is damaged.
#### ERROR_INVALID_DOMAIN_ROLE
Indicates the requested
 operation cannot be completed
 with the domain in its present
 role.
#### ERROR_INVALID_DOMAIN_STATE
Indicates the domain is in the
 wrong state to perform the
 desired operation.
#### ERROR_INVALID_DOMAINNAME
The format of the specified
 domain name is invalid.
#### ERROR_INVALID_DRIVE
The system cannot find the
 specified drive.
#### ERROR_INVALID_DWP_HANDLE
The DeferWindowPos handle is
 invalid.
#### ERROR_INVALID_EA_HANDLE
The specified EA handle is
 invalid.
#### ERROR_INVALID_EA_NAME
The specified EA name is
 invalid.
#### ERROR_INVALID_EDIT_HEIGHT
Height must be less than 256.
#### ERROR_INVALID_ENVIRONMENT
The Environment specified is
 invalid.
#### ERROR_INVALID_EVENT_COUNT
The number of specified
 semaphore events is incorrect.
#### ERROR_INVALID_EVENTNAME
The format of the specified
 event name is invalid.
#### ERROR_INVALID_EXE_SIGNATURE
%1 cannot be run in Windows/NT
 mode.
#### ERROR_INVALID_FILTER_PROC
The filter proc is invalid.
#### ERROR_INVALID_FLAG_NUMBER
The flag passed is incorrect.
#### ERROR_INVALID_FLAGS
The flags are invalid.
#### ERROR_INVALID_FORM_NAME
The specified Form name is
 invalid.
#### ERROR_INVALID_FORM_SIZE
The specified Form size is
 invalid.
#### ERROR_INVALID_FUNCTION
The function is incorrect.
#### ERROR_INVALID_GROUP_ATTRIBUTES
The specified attributes are
 invalid, or incompatible with
 the attributes for the group
 as a whole.
#### ERROR_INVALID_GROUPNAME
The format of the specified
 group name is invalid.
#### ERROR_INVALID_GW_COMMAND
The GW_* command is invalid.
#### ERROR_INVALID_HANDLE
The internal file identifier
 is incorrect.
#### ERROR_INVALID_HOOK_FILTER
The hook filter type is
 invalid.
#### ERROR_INVALID_HOOK_HANDLE
The hook handle is invalid.
#### ERROR_INVALID_ICON_HANDLE
The icon handle is invalid.
#### ERROR_INVALID_ID_AUTHORITY
The value provided is an
 invalid value for an
 identifier authority.
#### ERROR_INVALID_INDEX
The index is invalid.
#### ERROR_INVALID_KEYBOARD_HANDLE
Invalid keyboard layout
 handle.
#### ERROR_INVALID_LB_MESSAGE
The message for single-
 selection list box is invalid.
#### ERROR_INVALID_LEVEL
The system call level is
 incorrect.
#### ERROR_INVALID_LIST_FORMAT
The list is not correct.
#### ERROR_INVALID_LOGON_HOURS
The user account has time
 restrictions and cannot be
 logged onto at this time.
#### ERROR_INVALID_LOGON_TYPE
Indicates an invalid value has
 been provided for LogonType
 has been requested.
#### ERROR_INVALID_MEMBER
A new member could not be
 added to an alias because the
 member has the wrong account
 type.
#### ERROR_INVALID_MENU_HANDLE
The menu handle is invalid.
#### ERROR_INVALID_MESSAGE
Window can't handle sent
 message.
#### ERROR_INVALID_MESSAGEDEST
The format of the specified
 message destination is
 invalid.
#### ERROR_INVALID_MESSAGENAME
The format of the specified
 message name is invalid.
#### ERROR_INVALID_MINALLOCSIZE
The operating system cannot
 run %1.
#### ERROR_INVALID_MODULETYPE
The operating system cannot
 run %1.
#### ERROR_INVALID_MSGBOX_STYLE
The message box style is
 invalid.
#### ERROR_INVALID_NAME
The file name, directory name,
 or volume label is
 syntactically incorrect.
#### ERROR_INVALID_NETNAME
The format of the specified
 network name is invalid.
#### ERROR_INVALID_ORDINAL
The operating system cannot
 run %1.
#### ERROR_INVALID_OWNER
Indicates a particular
 Security ID cannot be assigned
 as the owner of an object.
#### ERROR_INVALID_PARAMETER
The parameter is incorrect.
#### ERROR_INVALID_PASSWORD
The specified network password
 is incorrect.
#### ERROR_INVALID_PASSWORDNAME
The format of the specified
 password is invalid.
#### ERROR_INVALID_PIXEL_FORMAT
The pixel format is invalid.
#### ERROR_INVALID_PRIMARY_GROUP
Indicates a particular
 Security ID cannot be assigned
 as the primary group of an
 object.
#### ERROR_INVALID_PRINT_MONITOR
The specified print monitor
 does not have the required
 functions.
#### ERROR_INVALID_PRINTER_COMMAND
The printer command is
 invalid.
#### ERROR_INVALID_PRINTER_NAME
The printer name is invalid.
#### ERROR_INVALID_PRINTER_STATE
The state of the Printer is
 invalid.
#### ERROR_INVALID_PRIORITY
The specified priority is
 invalid.
#### ERROR_INVALID_SCROLLBAR_RANGE
Scrollbar range greater than
 0x7FFF.
#### ERROR_INVALID_SECURITY_DESCR
Indicates the
 SECURITY_DESCRIPTOR structure
 is invalid.
#### ERROR_INVALID_SEGDPL
The operating system cannot
 run %1.
#### ERROR_INVALID_SEGMENT_NUMBER
The system detected a segment
 number that is incorrect.
#### ERROR_INVALID_SEPARATOR_FILE
The specified separator file
 is invalid.
#### ERROR_INVALID_SERVER_STATE
Indicates the Sam Server was
 in the wrong state to perform
 the desired operation.
#### ERROR_INVALID_SERVICE_ACCOUNT
The account name is invalid or
 does not exist.
#### ERROR_INVALID_SERVICE_CONTROL
The requested control is not
 valid for this service.
#### ERROR_INVALID_SERVICE_LOCK
The specified service database
 lock is invalid.
#### ERROR_INVALID_SERVICENAME
The format of the specified
 service name is invalid.
#### ERROR_INVALID_SHARENAME
The format of the specified
 share name is invalid.
#### ERROR_INVALID_SHOWWIN_COMMAND
The ShowWindow command is
 invalid.
#### ERROR_INVALID_SID
Indicates the SID structure is
 invalid.
#### ERROR_INVALID_SIGNAL_NUMBER
The signal being posted is
 incorrect.
#### ERROR_INVALID_SPI_VALUE
The SPI_* parameter is
 invalid.
#### ERROR_INVALID_STACKSEG
The operating system cannot
 run %1.
#### ERROR_INVALID_STARTING_CODESEG
The operating system cannot
 run %1.
#### ERROR_INVALID_SUB_AUTHORITY
Indicates the sub-authority
 value is invalid for the
 particular use.
#### ERROR_INVALID_TARGET_HANDLE
The target internal file
 identifier is incorrect.
#### ERROR_INVALID_THREAD_ID
The thread ID is invalid.
#### ERROR_INVALID_TIME
The specified time is invalid.
#### ERROR_INVALID_USER_BUFFER
The supplied user buffer is
 invalid for the requested
 operation.
#### ERROR_INVALID_VERIFY_SWITCH
The verify-on-write switch
 parameter value is incorrect.
#### ERROR_INVALID_WINDOW_HANDLE
The window handle invalid.
#### ERROR_INVALID_WINDOW_STYLE
The window style or class
 attribute is invalid for this
 operation.
#### ERROR_INVALID_WORKSTATION
The user account is restricted
 and cannot be used to log on
 from the source workstation.
#### ERROR_IO_DEVICE
The request could not be
 performed because of an I/O
 device error.
#### ERROR_IO_INCOMPLETE
Overlapped IO event not in
 signaled state.
#### ERROR_IO_PENDING
Overlapped IO operation in
 progress.
#### ERROR_IOPL_NOT_ENABLED
The operating system is not
 presently configured to run
 this application.
#### ERROR_IRQ_BUSY
Unable to open a device that
 was sharing an interrupt
 request (IRQ) with other
 devices. At least one other
 device that uses that IRQ was
 already opened.
#### ERROR_IS_JOIN_PATH
Not enough resources are
 available to process this
 command.
#### ERROR_IS_JOIN_TARGET
A JOIN or SUBST command cannot
 be used for a drive that
 contains previously joined
 drives.
#### ERROR_IS_JOINED
An attempt was made to use a
 JOIN or SUBST command on a
 drive that is already joined.
#### ERROR_IS_SUBST_PATH
The path specified is being
 used in a substitute.
#### ERROR_IS_SUBST_TARGET
An attempt was made to join or
 substitute a drive for which a
 directory on the drive is the
 target of a previous
 substitute.
#### ERROR_IS_SUBSTED
An attempt was made to use a
 JOIN or SUBST command on a
 drive already substituted.
#### ERROR_ITERATED_DATA_EXCEEDS_64k
The operating system cannot
 run %1.
#### ERROR_JOIN_TO_JOIN
The system tried to join a
 drive to a directory on a
 joined drive.
#### ERROR_JOIN_TO_SUBST
The system tried to join a
 drive to a directory on a
 substituted drive.
#### ERROR_JOURNAL_HOOK_SET
The journal hook is already
 installed.
#### ERROR_KEY_DELETED
Illegal operation attempted on
 a registry key that has been
 marked for deletion.
#### ERROR_KEY_HAS_CHILDREN
An attempt was made to create
 a symbolic link in a registry
 key that already has subkeys
 or values.
#### ERROR_LABEL_TOO_LONG
The volume label entered
 exceeds the 11 character
 limit. The first 11 characters
 were written to disk. Any
 characters that exceeded the
 11 character limit were
 automatically deleted.
#### ERROR_LAST_ADMIN
Indicates the requested
 operation would disable or
 delete the last remaining
 administration account. This
 is not allowed to prevent
 creating a situation where the
 system will not be
 administrable.
#### ERROR_LB_WITHOUT_TABSTOPS
This list box doesn't support
 tab stops.
#### ERROR_LICENSE_QUOTA_EXCEEDED
The service being accessed is
 licensed for a particular
 number of connections. No more
 connections can be made to the
 service at this time because
 there are already as many
 connections as the service can
 accept.
#### ERROR_LISTBOX_ID_NOT_FOUND
List box ID not found.
#### ERROR_LM_CROSS_ENCRYPTION_REQUIRED
An attempt was made to change
 a user password in the
 security account manager
 without providing the required
 LM cross-encrypted password.
#### ERROR_LOCAL_USER_SESSION_KEY
A user session key was
 requested for a local RPC
 connection. The session key
 returned is a constant value
 and not unique to this
 connection.
#### ERROR_LOCK_FAILED
Attempt to lock a region of a
 file failed.
#### ERROR_LOCK_VIOLATION
The process cannot access the
 file because another process
 has locked a portion of the
 file.
#### ERROR_LOCKED
The segment is locked and
 cannot be reallocated.
#### ERROR_LOG_FILE_FULL
The event log file is full.
#### ERROR_LOGIN_TIME_RESTRICTION
Attempting to login during an
 unauthorized time of day for
 this account.
#### ERROR_LOGIN_WKSTA_RESTRICTION
The account is not authorized
 to login from this station.
#### ERROR_LOGON_FAILURE
The attempted logon is
 invalid. This is due to either
 a bad user name or
 authentication information.
#### ERROR_LOGON_NOT_GRANTED
A requested type of logon,
 such as Interactive, Network,
 or Service, is not granted by
 the target system's local
 security policy. The system
 administrator can grant the
 required form of logon.
#### ERROR_LOGON_SESSION_COLLISION
The logon session ID is
 already in use.
#### ERROR_LOGON_SESSION_EXISTS
An attempt was made to start a
 new session manager or LSA
 logon session with an ID
 already in use.
#### ERROR_LOGON_TYPE_NOT_GRANTED
A user has requested a type of
 logon, such as interactive or
 network, that was not granted.
 An administrator has control
 over who may logon
 interactively and through the
 network.
#### ERROR_LUIDS_EXHAUSTED
Indicates there are no more
 LUID to allocate.
#### ERROR_MAPPED_ALIGNMENT
The base address or the file
 offset specified does not have
 the proper alignment.
#### ERROR_MAX_THRDS_REACHED
No more threads can be created
 in the system.
#### ERROR_MEDIA_CHANGED
Media in drive may have
 changed.
#### ERROR_MEMBER_IN_ALIAS
The specified account name is
 not a member of the alias.
#### ERROR_MEMBER_IN_GROUP
The specified user account is
 already in the specified group
 account. Also used to indicate
 a group can not be deleted
 because it contains a member.
#### ERROR_MEMBER_NOT_IN_ALIAS
The specified account name is
 not a member of the alias.
#### ERROR_MEMBER_NOT_IN_GROUP
The specified user account is
 not a member of the specified
 group account.
#### ERROR_MEMBERS_PRIMARY_GROUP
Indicates a member cannot be
 removed from a group because
 the group is currently the
 member's primary group.
#### ERROR_MENU_ITEM_NOT_FOUND
A menu item was not found.
#### ERROR_META_EXPANSION_TOO_LONG
The global filename characters
 * or ? are entered
 incorrectly, or too many
 global filename characters are
 specified.
#### ERROR_METAFILE_NOT_SUPPORTED
The requested metafile
 operation is not supported.
#### ERROR_MOD_NOT_FOUND
The specified module cannot be
 found.
#### ERROR_MORE_DATA
More data is available.
#### ERROR_MORE_WRITES
A serial I/O operation was
 completed by another write to
 the serial port. (The
 IOCTL_SERIAL_XOFF_COUNTER
 reached zero.)
#### ERROR_MR_MID_NOT_FOUND
The system cannot find message
 for message number 0x%1 in
 message file for %2.
#### ERROR_NEGATIVE_SEEK
An attempt was made to move
 the file pointer before the
 beginning of the file.
#### ERROR_NESTING_NOT_ALLOWED
Can't nest calls to
 LoadModule.
#### ERROR_NET_WRITE_FAULT
A write fault occurred on the
 network.
#### ERROR_NETLOGON_NOT_STARTED
An attempt was made to logon,
 but the network logon service
 was not started.
#### ERROR_NETNAME_DELETED
The specified network name is
 no longer available.
#### ERROR_NETWORK_ACCESS_DENIED
Network access is denied.
#### ERROR_NETWORK_BUSY
The network is busy.
#### ERROR_NETWORK_UNREACHABLE
The remote network is not
 reachable by the transport.
#### ERROR_NO_ASSOCIATION
No application is associated
 with the specified file for
 this operation.
#### ERROR_NO_BROWSER_SERVERS_FOUND
The list of servers for this
 workgroup is not currently
 available.
#### ERROR_NO_DATA
Pipe close in progress.
#### ERROR_NO_DATA_DETECTED
During a tape access, the end
 of the data marker was
 reached.
#### ERROR_NO_IMPERSONATION_TOKEN
An attempt was made to operate
 on an impersonation token by a
 thread was not currently
 impersonating a client.
#### ERROR_NO_INHERITANCE
Indicates an ACL contains no
 inheritable components.
#### ERROR_NO_LOG_SPACE
System could not allocate
 required space in a registry
 log.
#### ERROR_NO_LOGON_SERVERS
There are currently no logon
 servers available to service
 the logon request.
#### ERROR_NO_MEDIA_IN_DRIVE
Tape query failed because of
 no media in drive.
#### ERROR_NO_MORE_DEVICES
No more local devices.
#### ERROR_NO_MORE_FILES
There are no more files.
#### ERROR_NO_MORE_ITEMS
No more data is available.
#### ERROR_NO_MORE_SEARCH_HANDLES
No more internal file
 identifiers available.
#### ERROR_NO_NET_OR_BAD_PATH
No network provider accepted
 the given network path.
#### ERROR_NO_NETWORK
The network is not present or
 not started.
#### ERROR_NO_NETWORK2
The network is not present or
 not started.
#### ERROR_NO_PROC_SLOTS
The system cannot start
 another process at this time.
#### ERROR_NO_QUOTAS_FOR_ACCOUNT
No system quota limits are
 specifically set for this
 account.
#### ERROR_NO_SCROLLBARS
Window does not have scroll
 bars.
#### ERROR_NO_SECURITY_ON_OBJECT
Indicates an attempt was made
 to operate on the security of
 an object that does not have
 security associated with it.
#### ERROR_NO_SHUTDOWN_IN_PROGRESS
An attempt to abort the
 shutdown of the system failed
 because no shutdown was in
 progress.
#### ERROR_NO_SIGNAL_SENT
No process in the command
 subtree has a signal handler.
#### ERROR_NO_SPOOL_SPACE
Space to store the file
 waiting to be printed is not
 available on the server.
#### ERROR_NO_SUCH_ALIAS
The specified alias does not
 exist.
#### ERROR_NO_SUCH_DOMAIN
The specified domain does not
 exist.
#### ERROR_NO_SUCH_GROUP
The specified group does not
 exist.
#### ERROR_NO_SUCH_LOGON_SESSION
A specified logon session does
 not exist. It may already have
 been terminated.
#### ERROR_NO_SUCH_MEMBER
A new member cannot be added
 to an alias because the member
 does not exist.
#### ERROR_NO_SUCH_PACKAGE
A specified authentication
 package is unknown.
#### ERROR_NO_SUCH_PRIVILEGE
A specified privilege does not
 exist.
#### ERROR_NO_SUCH_USER
The specified user does not
 exist.
#### ERROR_NO_SYSTEM_MENU
Window does not have system
 menu.
#### ERROR_NO_SYSTEM_RESOURCES
Insufficient system resources
 exist to complete the
 requested service.
#### ERROR_NO_TOKEN
An attempt was made to
 reference a token that does
 not exist.
#### ERROR_NO_TRUST_LSA_SECRET
The workstation does not have
 a trust secret.
#### ERROR_NO_TRUST_SAM_ACCOUNT
The domain controller does not
 have an account for this
 workstation.
#### ERROR_NO_UNICODE_TRANSLATION
No mapping for the Unicode
 character exists in the target
 multi-byte code page.
#### ERROR_NO_USER_SESSION_KEY
There is no user session key
 for the specified logon
 session.
#### ERROR_NO_VOLUME_LABEL
The disk has no volume label.
#### ERROR_NO_WILDCARD_CHARACTERS
No wildcard characters found.
#### ERROR_NOACCESS
Invalid access to memory
 location.
#### ERROR_NOLOGON_INTERDOMAIN_TRUST_ACCOUNT
The account used is an
 interdomain trust account. Use
 your normal user account or
 remote user account to access
 this server.
#### ERROR_NOLOGON_SERVER_TRUST_ACCOUNT
The account used is an server
 trust account. Use your normal
 user account or remote user
 account to access this server.
#### ERROR_NOLOGON_WORKSTATION_TRUST_ACCOUNT
The account used is a
 workstation trust account. Use
 your normal user account or
 remote user account to access
 this server.
#### ERROR_NON_MDICHILD_WINDOW
DefMDIChildProc called with a
 non-MDI child window.
#### ERROR_NONE_MAPPED
None of the information to be
 mapped has been translated.
#### ERROR_NONPAGED_SYSTEM_RESOURCES
Insufficient system resources
 exist to complete the
 requested service.
#### ERROR_NOT_ALL_ASSIGNED
Indicates not all privileges
 referenced are assigned to the
 caller. This allows, for
 example, all privileges to be
 disabled without having to
 know exactly which privileges
 are assigned.
#### ERROR_NOT_AUTHENTICATED
The operation being requested
 was not performed because the
 user has not been
 authenticated.
#### ERROR_NOT_CHILD_WINDOW
Window is not a child window.
#### ERROR_NOT_CONNECTED
This network connection does
 not exist.
#### ERROR_NOT_CONTAINER
Cannot enumerate a non-
 container.
#### ERROR_NOT_DOS_DISK
The specified disk cannot be
 accessed.
#### ERROR_NOT_ENOUGH_MEMORY
Not enough storage is
 available to process this
 command.
#### ERROR_NOT_ENOUGH_QUOTA
Not enough quota is available
 to process this command.
#### ERROR_NOT_ENOUGH_SERVER_MEMORY
Not enough server storage is
 available to process this
 command.
#### ERROR_NOT_JOINED
The system attempted to delete
 the JOIN of a drive not
 previously joined.
#### ERROR_NOT_LOCKED
The segment is already
 unlocked.
#### ERROR_NOT_LOGGED_ON
The operation being requested
 was not performed because the
 user has not logged on to the
 network.
#### ERROR_NOT_LOGON_PROCESS
The requested action is
 restricted for use by logon
 processes only. The calling
 process has not registered as
 a logon process.
#### ERROR_NOT_OWNER
Attempt to release mutex not
 owned by caller.
#### ERROR_NOT_READY
The drive is not ready.
#### ERROR_NOT_REGISTRY_FILE
The system attempted to load
 or restore a file into the
 registry, and the specified
 file is not in the format of a
 registry file.
#### ERROR_NOT_SAME_DEVICE
The system cannot move the
 file to a different disk
 drive.
#### ERROR_NOT_SUBSTED
The system attempted to delete
 the substitution of a drive
 not previously substituted.
#### ERROR_NOT_SUPPORTED
The network request is not
 supported.
#### ERROR_NOTIFY_ENUM_DIR
This indicates that a notify
 change request is being
 completed and the information
 is not being returned in the
 caller's buffer. The caller
 now needs to enumerate the
 files to find the changes.
#### ERROR_NT_CROSS_ENCRYPTION_REQUIRED
An attempt was made to change
 a user password in the
 security account manager
 without providing the
 necessary NT cross-encrypted
 password.
#### ERROR_NULL_LM_PASSWORD
The Windows NT password is too
 complex to be converted to a
 Windows-networking password.
 The Windows-networking
 password returned is a NULL
 string.
#### ERROR_OLD_WIN_VERSION
The specified program requires
 a newer version of Windows.
#### ERROR_OPEN_FAILED
The system cannot open the
 specified device or file.
#### ERROR_OPEN_FILES
There are open files or
 requests pending on this
 connection.
#### ERROR_OPERATION_ABORTED
The I/O operation was aborted
 due to either thread exit or
 application request.
#### ERROR_OUT_OF_PAPER
The printer is out of paper.
#### ERROR_OUT_OF_STRUCTURES
Storage to process this
 request is not available.
#### ERROR_OUTOFMEMORY
Not enough storage is
 available to complete this
 operation.
#### ERROR_PAGED_SYSTEM_RESOURCES
Insufficient system resources
 exist to complete the
 requested service.
#### ERROR_PAGEFILE_QUOTA
Insufficient quota to complete
 the requested service.
#### ERROR_PARTIAL_COPY
Only part of a
 Read/WriteProcessMemory
 request was completed.
#### ERROR_PARTITION_FAILURE
Tape could not be partitioned.
#### ERROR_PASSWORD_EXPIRED
The user account's password
 has expired.
#### ERROR_PASSWORD_MUST_CHANGE
The user must change his
 password before he logs on the
 first time.
#### ERROR_PASSWORD_RESTRICTION
When trying to update a
 password, this status
 indicates that some password
 update rule was violated. For
 example, the password may not
 meet length criteria.
#### ERROR_PATH_BUSY
The specified path cannot be
 used at this time.
#### ERROR_PATH_NOT_FOUND
The system cannot find the
 specified path.
#### ERROR_PIPE_BUSY
All pipe instances busy.
#### ERROR_PIPE_CONNECTED
There is a process on other
 end of the pipe.
#### ERROR_PIPE_LISTENING
Waiting for a process to open
 the other end of the pipe.
#### ERROR_PIPE_NOT_CONNECTED
No process on other end of
 pipe.
#### ERROR_POPUP_ALREADY_ACTIVE
Pop-up menu already active.
#### ERROR_PORT_UNREACHABLE
No service is operating at the
 destination network endpoint
 on the remote system.
#### ERROR_POSSIBLE_DEADLOCK
A potential deadlock condition
 has been detected.
#### ERROR_PRINT_CANCELLED
File waiting to be printed was
 deleted.
#### ERROR_PRINT_MONITOR_ALREADY_INSTALLED
The specified print monitor
 has already been installed.
#### ERROR_PRINT_MONITOR_IN_USE
The specified print monitor is
 currently in use.
#### ERROR_PRINT_PROCESSOR_ALREADY_INSTALLED
The specified print
 processor has already been
 installed.
#### ERROR_PRINTER_ALREADY_EXISTS
The printer already exists.
#### ERROR_PRINTER_DELETED
The specified Printer has been
 deleted.
#### ERROR_PRINTER_DRIVER_ALREADY_INSTALLED
The specified printer driver
 is already installed.
#### ERROR_PRINTER_DRIVER_IN_USE
The specified printer driver
 is currently in use.
#### ERROR_PRINTER_HAS_JOBS_QUEUED
The requested operation is not
 allowed when there are jobs
 queued to the printer.
#### ERROR_PRINTQ_FULL
The printer queue is full.
#### ERROR_PRIVATE_DIALOG_INDEX
Using private DIALOG window
 words.
#### ERROR_PRIVILEGE_NOT_HELD
A required privilege is not
 held by the client.
#### ERROR_PROC_NOT_FOUND
The specified procedure could
 not be found.
#### ERROR_PROCESS_ABORTED
The process terminated
 unexpectedly.
#### ERROR_PROTOCOL_UNREACHABLE
The remote system does not
 support the transport
 protocol.
#### ERROR_READ_FAULT
The system cannot read from
 the specified device.
#### ERROR_REC_NON_EXISTENT
The name does not exist in the
 WINS database.
#### ERROR_REDIR_PAUSED
The specified printer or disk
 device has been paused.
#### ERROR_REDIRECTOR_HAS_OPEN_HANDLES
The redirector is in use and
 cannot be unloaded.
#### ERROR_REGISTRY_CORRUPT
The registry is damaged. The
 structure of one of the files
 that contains registry data is
 damaged, or the system's in
 memory image of the file is
 damaged, or the file could not
 be recovered because its
 alternate copy or log was
 absent or damaged.
#### ERROR_REGISTRY_IO_FAILED
The registry initiated an I/O
 operation that had an
 unrecoverable failure. The
 registry could not read in, or
 write out, or flush, one of
 the files that contain the
 system's image of the
 registry.
#### ERROR_REGISTRY_RECOVERED
One of the files containing
 the system's registry data had
 to be recovered by use of a
 log or alternate copy. The
 recovery succeeded.
#### ERROR_RELOC_CHAIN_XEEDS_SEGLIM
The operating system cannot
 run %1.
#### ERROR_REM_NOT_LIST
The remote computer is not
 available.
#### ERROR_REMOTE_SESSION_LIMIT_EXCEEDED
An attempt was made to
 establish a session to a LAN
 Manager server, but there are
 already too many sessions
 established to that server.
#### ERROR_REQ_NOT_ACCEP
The network request was not
 accepted.
#### ERROR_REQUEST_ABORTED
The request was aborted.
#### ERROR_RESOURCE_DATA_NOT_FOUND
The specified image file did
 not contain a resource
 section.
#### ERROR_RESOURCE_LANG_NOT_FOUND
The specified resource
 language ID cannot be found in
 the image file.
#### ERROR_RESOURCE_NAME_NOT_FOUND
The specified resource name
 can not be found in the image
 file.
#### ERROR_RESOURCE_TYPE_NOT_FOUND
The specified resource type
 can not be found in the image
 file.
#### ERROR_RETRY
The operation could not be
 completed. A retry should be
 performed.
#### ERROR_REVISION_MISMATCH
Indicates two revision levels
 are incompatible.
#### ERROR_RING2_STACK_IN_USE
The ring 2 stack is in use.
#### ERROR_RING2SEG_MUST_BE_MOVABLE
The code segment cannot be
 greater than or equal to 64KB.
#### ERROR_RMODE_APP
The specified program was
 written for an older version
 of Windows.
#### ERROR_RPL_NOT_ALLOWED
Replication with a non-
 configured partner is not
 allowed.
#### ERROR_RXACT_COMMIT_FAILURE
Indicates an error occurred
 during a registry transaction
 commit. The database has been
 left in an unknown state. The
 state of the registry
 transaction is left as
 COMMITTING. This status value
 is returned by the runtime
 library (RTL) registry
 transaction package (RXact).
#### ERROR_RXACT_INVALID_STATE
Indicates that the transaction
 state of a registry sub-tree
 is incompatible with the
 requested operation. For
 example, a request has been
 made to start a new
 transaction with one already
 in progress, or a request to
 apply a transaction when one
 is not currently in progress.
 This status value is returned
 by the runtime library (RTL)
 registry transaction package
 (RXact).
#### ERROR_SAME_DRIVE
The system cannot join or
 substitute a drive to or for a
 directory on the same drive.
#### ERROR_SCREEN_ALREADY_LOCKED
Screen already locked.
#### ERROR_SECRET_TOO_LONG
The length of a secret exceeds
 the maximum length allowed.
#### ERROR_SECTOR_NOT_FOUND
The drive cannot find the
 requested sector.
#### ERROR_SEEK
The drive cannot locate a
 specific area or track on the
 disk.
#### ERROR_SEEK_ON_DEVICE
The file pointer cannot be set
 on the specified device or
 file.
#### ERROR_SEM_IS_SET
The semaphore is set and
 cannot be closed.
#### ERROR_SEM_NOT_FOUND
The specified system semaphore
 name was not found.
#### ERROR_SEM_OWNER_DIED
The previous ownership of this
 semaphore has ended.
#### ERROR_SEM_TIMEOUT
The semaphore timeout period
 has expired.
#### ERROR_SEM_USER_LIMIT
Insert the disk for drive 1.
#### ERROR_SERIAL_NO_DEVICE
No serial device was
 successfully initialized. The
 serial driver will unload.
#### ERROR_SERVER_DISABLED
The GUID allocation server is
 already disabled at the
 moment.
#### ERROR_SERVER_HAS_OPEN_HANDLES
The server is in use and
 cannot be unloaded.
#### ERROR_SERVER_NOT_DISABLED
The GUID allocation server is
 already enabled at the moment.
#### ERROR_SERVICE_ALREADY_RUNNING
An instance of the service is
 already running.
#### ERROR_SERVICE_CANNOT_ACCEPT_CTRL
The service cannot accept
 control messages at this time.
#### ERROR_SERVICE_DATABASE_LOCKED
The service database is
 locked.
#### ERROR_SERVICE_DEPENDENCY_DELETED
The dependency service does
 not exist or has been marked
 for deletion.
#### ERROR_SERVICE_DEPENDENCY_FAIL
The dependency service or
 group failed to start.
#### ERROR_SERVICE_DISABLED
The specified service is
 disabled and cannot be
 started.
#### ERROR_SERVICE_DOES_NOT_EXIST
The specified service does not
 exist as an installed service.
#### ERROR_SERVICE_EXISTS
The specified service already
 exists.
#### ERROR_SERVICE_LOGON_FAILED
The service did not start due
 to a logon failure.
#### ERROR_SERVICE_MARKED_FOR_DELETE
The specified service has been
 marked for deletion.
#### ERROR_SERVICE_NEVER_STARTED
No attempts to start the
 service have been made since
 the last boot.
#### ERROR_SERVICE_NO_THREAD
A thread could not be created
 for the service.
#### ERROR_SERVICE_NOT_ACTIVE
The service has not been
 started.
#### ERROR_SERVICE_NOT_FOUND
The specified service does not
 exist.
#### ERROR_SERVICE_REQUEST_TIMEOUT
The service did not respond to
 the start or control request
 in a timely fashion.
#### ERROR_SERVICE_SPECIFIC_ERROR
The service has returned a
 service-specific error code.
#### ERROR_SERVICE_START_HANG
After starting, the service
 hung in a start-pending state.
#### ERROR_SESSION_CREDENTIAL_CONFLICT
The credentials supplied
 conflict with an existing set
 of credentials.
#### ERROR_SET_POWER_STATE_FAILED
The system BIOS failed an
 attempt to change the system
 power state.
#### ERROR_SET_POWER_STATE_VETOED
An attempt to change the
 system power state was vetoed
 by another application or
 driver.
#### ERROR_SETCOUNT_ON_BAD_LB
LB_SETCOUNT sent to non-lazy
 list box.
#### ERROR_SETMARK_DETECTED
A tape access reached a
 setmark.
#### ERROR_SHARING_BUFFER_EXCEEDED
Too many files opened for
 sharing.
#### ERROR_SHARING_PAUSED
The remote server is paused or
 is in the process of being
 started.
#### ERROR_SHARING_VIOLATION
The process cannot access the
 file because it is being used
 by another process.
#### ERROR_SHUTDOWN_IN_PROGRESS
A system shutdown is in
 progress.
#### ERROR_SIGNAL_PENDING
A signal is already pending.
#### ERROR_SIGNAL_REFUSED
The recipient process has
 refused the signal.
#### ERROR_SINGLE_INSTANCE_APP
Cannot start more than one
 instance of the specified
 program.
#### ERROR_SOME_NOT_MAPPED
Some of the information to be
 mapped has not been
 translated.
#### ERROR_SPECIAL_ACCOUNT
Indicates an operation was
 attempted on a built-in
 (special) SAM account that is
 incompatible with built-in
 accounts. For example, built-
 in accounts cannot be renamed
 or deleted.
#### ERROR_SPECIAL_GROUP
The requested operation cannot
 be performed on the specified
 group because it is a built-in
 special group.
#### ERROR_SPECIAL_USER
The requested operation cannot
 be performed on the specified
 user because it is a built-in
 special user.
#### ERROR_SPL_NO_ADDJOB
An AddJob call was not issued.
#### ERROR_SPL_NO_STARTDOC
A StartDocPrinter call was not
 issued.
#### ERROR_SPOOL_FILE_NOT_FOUND
The spool file was not found.
#### ERROR_STACK_OVERFLOW
Recursion too deep, stack
 overflowed.
#### ERROR_STATIC_INIT
The importation from the file
 failed.
#### ERROR_SUBST_TO_JOIN
The system attempted to SUBST
 a drive to a directory on a
 joined drive.
#### ERROR_SUBST_TO_SUBST
The system attempted to
 substitute a drive to a
 directory on a substituted
 drive.
#### ERROR_SUCCESS
The operation was successfully
 completed.
#### ERROR_SUCCESS_REBOOT_REQUIRED
The requested operation is
 successful. Changes will not
 be effective until the system
 is rebooted.
#### ERROR_SUCCESS_RESTART_REQUIRED
The requested operation is
 successful. Changes will not
 be effective until the service
 is restarted.
#### ERROR_SWAPERROR
Error accessing paging file.
#### ERROR_SYSTEM_TRACE
System trace information not
 specified in your CONFIG.SYS
 file, or tracing is not
 allowed.
#### ERROR_THREAD_1_INACTIVE
The signal handler cannot be
 set.
#### ERROR_TLW_WITH_WSCHILD
CreateWindow failed, creating
 top-level window with WS_CHILD
 style.
#### ERROR_TOKEN_ALREADY_IN_USE
An attempt was made to
 establish a token for use as a
 primary token but the token is
 already in use. A token can
 only be the primary token of
 one process at a time.
#### ERROR_TOO_MANY_CMDS
The network BIOS command limit
 has been reached.
#### ERROR_TOO_MANY_CONTEXT_IDS
During a logon attempt, the
 user's security context
 accumulated too many security
 IDs. Remove the user from some
 groups or aliases to reduce
 the number of security ids to
 incorporate into the security
 context.
#### ERROR_TOO_MANY_LINKS
An attempt was made to create
 more links on a file than the
 file system supports.
#### ERROR_TOO_MANY_LUIDS_REQUESTED
The number of LUID requested
 cannot be allocated with a
 single allocation.
#### ERROR_TOO_MANY_MODULES
Too many dynamic link modules
 are attached to this program
 or dynamic link module.
#### ERROR_TOO_MANY_MUXWAITERS
Too many semaphores are
 already set.
#### ERROR_TOO_MANY_NAMES
The name limit for the local
 computer network adapter card
 exceeded.
#### ERROR_TOO_MANY_OPEN_FILES
The system cannot open the
 file.
#### ERROR_TOO_MANY_POSTS
Too many posts made to a
 semaphore.
#### ERROR_TOO_MANY_SECRETS
The maximum number of secrets
 that can be stored in a single
 system was exceeded.
#### ERROR_TOO_MANY_SEM_REQUESTS
The semaphore cannot be set
 again.
#### ERROR_TOO_MANY_SEMAPHORES
Cannot create another system
 semaphore.
#### ERROR_TOO_MANY_SESS
The network BIOS session limit
 exceeded.
#### ERROR_TOO_MANY_SIDS
Too many SIDs specified.
#### ERROR_TOO_MANY_TCBS
Cannot create another thread.
#### ERROR_TRANSFORM_NOT_SUPPORTED
The requested transformation
 operation is not supported.
#### ERROR_TRUST_FAILURE
The network logon failed.
#### ERROR_TRUSTED_DOMAIN_FAILURE
The trust relationship between
 the primary domain and the
 trusted domain failed.
#### ERROR_TRUSTED_RELATIONSHIP_FAILURE
The trust relationship between
 this workstation and the
 primary domain failed.
#### ERROR_UNABLE_TO_LOCK_MEDIA
Attempt to lock the eject
 media mechanism failed.
#### ERROR_UNABLE_TO_UNLOAD_MEDIA
Unload media failed.
#### ERROR_UNEXP_NET_ERR
An unexpected network error
 occurred.
#### ERROR_UNKNOWN_PORT
The specified port is unknown.
#### ERROR_UNKNOWN_PRINT_MONITOR
The specified print monitor is
 unknown.
#### ERROR_UNKNOWN_PRINTER_DRIVER
The printer driver is unknown.
#### ERROR_UNKNOWN_PRINTPROCESSOR
The print processor is
 unknown.
#### ERROR_UNKNOWN_REVISION
Indicates an encountered or
 specified revision number is
 not one known by the service.
 The service may not be aware
 of a more recent revision.
#### ERROR_UNRECOGNIZED_MEDIA
The disk media is not
 recognized. It may not be
 formatted.
#### ERROR_UNRECOGNIZED_VOLUME
The volume does not contain a
 recognized file system. Make
 sure that all required file
 system drivers are loaded and
 the volume is not damaged.
#### ERROR_USER_EXISTS
The specified user already
 exists.
#### ERROR_USER_MAPPED_FILE
The requested operation cannot
 be performed on a file with a
 user mapped section open.
#### ERROR_VC_DISCONNECTED
The session was canceled.
#### ERROR_WAIT_NO_CHILDREN
There are no child processes
 to wait for.
#### ERROR_WINDOW_NOT_COMBOBOX
The window is not a combo box.
#### ERROR_WINDOW_NOT_DIALOG
The window is not a valid
 dialog window.
#### ERROR_WINDOW_OF_OTHER_THREAD
Invalid window, belongs to
 other thread.
#### ERROR_WINS_INTERNAL
WINS encountered an error
 while processing the command.
#### ERROR_WORKING_SET_QUOTA
Insufficient quota to complete
 the requested service.
#### ERROR_WRITE_FAULT
The system cannot write to the
 specified device.
#### ERROR_WRITE_PROTECT
The media is write protected.
#### ERROR_WRONG_DISK
The wrong disk is in the
 drive. Insert %2 (Volume
 Serial Number: %3) into drive
 %1.
#### ERROR_WRONG_PASSWORD
When trying to update a
 password, this return status
 indicates the value provided
 as the current password is
 incorrect.
#### LZERROR_BADINHANDLE
Invalid input handle.
#### LZERROR_BADOUTHANDLE
Invalid output handle.
#### LZERROR_BADVALUE
Input parameter out of
 acceptable range.
#### LZERROR_GLOBALLOC
Insufficient memory for LZFile
 structure.
#### LZERROR_GLOBLOCK
Bad global handle.
#### LZERROR_READ
Corrupt compressed file
 format.
#### LZERROR_UNKNOWNALG
Compression algorithm not
 recognized.
#### LZERROR_WRITE
Out of space for output file.
#### NO_ERROR
No error.
#### OR_INVALID_OID
The object specified was not
 found.
#### OR_INVALID_OXID
The object exporter specified
 was not found.
#### OR_INVALID_SET
The object resolver set
 specified was not found.
#### RPC_S_ADDRESS_ERROR
An addressing error occurred
 in the server.
#### RPC_S_ALREADY_LISTENING
The server is already
 listening.
#### RPC_S_ALREADY_REGISTERED
The object UUID already
 registered.
#### RPC_S_BINDING_HAS_NO_AUTH
The binding does not contain
 any authentication
 information.
#### RPC_S_BINDING_INCOMPLETE
The binding handle does not
 contain all required
 information.
#### RPC_S_CALL_CANCELLED
The server was altered while
 processing this call.
#### RPC_S_CALL_FAILED
The remote procedure call
 failed.
#### RPC_S_CALL_FAILED_DNE
The remote procedure call
 failed and did not execute.
#### RPC_S_CALL_IN_PROGRESS
A remote procedure call is
 already in progress for this
 thread.
#### RPC_S_CANNOT_SUPPORT
The requested operation is not
 supported.
#### RPC_S_CANT_CREATE_ENDPOINT
The endpoint cannot be
 created.
#### RPC_S_COMM_FAILURE
Communications failure.
#### RPC_S_DUPLICATE_ENDPOINT
The endpoint is a duplicate.
#### RPC_S_ENTRY_ALREADY_EXISTS
The entry already exists.
#### RPC_S_ENTRY_NOT_FOUND
The entry is not found.
#### RPC_S_FP_DIV_ZERO
A floating point operation at
 the server caused a divide by
 zero.
#### RPC_S_FP_OVERFLOW
A floating point overflow
 occurred at the server.
#### RPC_S_FP_UNDERFLOW
A floating point underflow
 occurred at the server.
#### RPC_S_GROUP_MEMBER_NOT_FOUND
The group member was not
 found.
#### RPC_S_INCOMPLETE_NAME
The entry name is incomplete.
#### RPC_S_INTERFACE_NOT_FOUND
The interface was not found.
#### RPC_S_INTERNAL_ERROR
An internal error occurred in
 RPC.
#### RPC_S_INVALID_AUTH_IDENTITY
The security context is
 invalid.
#### RPC_S_INVALID_BINDING
The binding handle is invalid.
#### RPC_S_INVALID_BOUND
The array bounds are invalid.
#### RPC_S_INVALID_ENDPOINT_FORMAT
The endpoint format is
 invalid.
#### RPC_S_INVALID_NAME_SYNTAX
The name syntax is invalid.
#### RPC_S_INVALID_NET_ADDR
The network address is
 invalid.
#### RPC_S_INVALID_NETWORK_OPTIONS
The network options are
 invalid.
#### RPC_S_INVALID_OBJECT
The object universal unique
 identifier (UUID) is the nil
 UUID.
#### RPC_S_INVALID_RPC_PROTSEQ
The RPC protocol sequence is
 invalid.
#### RPC_S_INVALID_STRING_BINDING
The string binding is invalid.
#### RPC_S_INVALID_STRING_UUID
The string UUID is invalid.
#### RPC_S_INVALID_TAG
The tag is invalid.
#### RPC_S_INVALID_TIMEOUT
The timeout value is invalid.
#### RPC_S_INVALID_VERS_OPTION
The version option is invalid.
#### RPC_S_MAX_CALLS_TOO_SMALL
The maximum number of calls is
 too small.
#### RPC_S_NAME_SERVICE_UNAVAILABLE
The name service is
 unavailable.
#### RPC_S_NO_BINDINGS
There are no bindings.
#### RPC_S_NO_CALL_ACTIVE
There is not a remote
 procedure call active in this
 thread.
#### RPC_S_NO_CONTEXT_AVAILABLE
No security context is
 available to allow
 impersonation.
#### RPC_S_NO_ENDPOINT_FOUND
No endpoint was found.
#### RPC_S_NO_ENTRY_NAME
The binding does not contain
 an entry name.
#### RPC_S_NO_INTERFACES

#### RPC_S_NO_MORE_BINDINGS
There are no more bindings.
#### RPC_S_NO_MORE_MEMBERS
There are no more members.
#### RPC_S_NO_PRINC_NAME
No principal name registered.
#### RPC_S_NO_PROTSEQS
There are no protocol
 sequences.
#### RPC_S_NO_PROTSEQS_REGISTERED
No protocol sequences were
 registered.
#### RPC_S_NOT_ALL_OBJS_UNEXPORTED
There is nothing to unexport.
#### RPC_S_NOT_CANCELLED
Thread is not cancelled.
#### RPC_S_NOT_LISTENING
The server is not listening.
#### RPC_S_NOT_RPC_ERROR
The error specified is not a
 valid Windows RPC error code.
#### RPC_S_OBJECT_NOT_FOUND
The object UUID was not found.
#### RPC_S_OUT_OF_RESOURCES
Not enough resources are
 available to complete this
 operation.
#### RPC_S_PROCNUM_OUT_OF_RANGE
The procedure number is out of
 range.
#### RPC_S_PROTOCOL_ERROR
An RPC protocol error
 occurred.
#### RPC_S_PROTSEQ_NOT_FOUND
The RPC protocol sequence was
 not found.
#### RPC_S_PROTSEQ_NOT_SUPPORTED
The RPC protocol sequence is
 not supported.
#### RPC_S_SEC_PKG_ERROR
A security package specific
 error occurred.
#### RPC_S_SEND_INCOMPLETE
Some data remains to be sent
 in the request buffer.
#### RPC_S_SERVER_OUT_OF_MEMORY
The server has insufficient
 memory to complete this
 operation.
#### RPC_S_SERVER_TOO_BUSY
The server is too busy to
 complete this operation.
#### RPC_S_SERVER_UNAVAILABLE
The server is unavailable.
#### RPC_S_STRING_TOO_LONG
The string is too long.
#### RPC_S_TYPE_ALREADY_REGISTERED
The type UUID is already
 registered.
#### RPC_S_UNKNOWN_AUTHN_LEVEL
The authentication level is
 unknown.
#### RPC_S_UNKNOWN_AUTHN_SERVICE
The authentication service is
 unknown.
#### RPC_S_UNKNOWN_AUTHN_TYPE
The authentication type is
 unknown.
#### RPC_S_UNKNOWN_AUTHZ_SERVICE
The authorization service is
 unknown.
#### RPC_S_UNKNOWN_IF
The interface is unknown.
#### RPC_S_UNKNOWN_MGR_TYPE
The manager type is unknown.
#### RPC_S_UNSUPPORTED_AUTHN_LEVEL
The requested authentication
 level is not supported.
#### RPC_S_UNSUPPORTED_NAME_SYNTAX
The name syntax is not
 supported.
#### RPC_S_UNSUPPORTED_TRANS_SYN
The transfer syntax is not
 supported by the server.
#### RPC_S_UNSUPPORTED_TYPE
The type UUID is not
 supported.
#### RPC_S_UUID_LOCAL_ONLY
A UUID that is valid only on
 this computer has been
 allocated.
#### RPC_S_UUID_NO_ADDRESS
No network address is
 available to use to construct
 a UUID.
#### RPC_S_WRONG_KIND_OF_BINDING
The binding handle is the
 incorrect type.
#### RPC_S_ZERO_DIVIDE
The server attempted an
 integer divide by zero.
#### RPC_X_BAD_STUB_DATA
The stub received bad data.
#### RPC_X_BYTE_COUNT_TOO_SMALL
The byte count is too small.
#### RPC_X_ENUM_VALUE_OUT_OF_RANGE
The enumeration value is out
 of range.
#### RPC_X_INVALID_ES_ACTION
Invalid operation on the
 encoding/decoding handle.
#### RPC_X_INVALID_PIPE_OBJECT
The idl pipe object is invalid
 or corrupted.
#### RPC_X_INVALID_PIPE_OPERATION
The operation is invalid for a
 given idl pipe object.
#### RPC_X_NO_MORE_ENTRIES
The list of servers available
 for auto_handle binding was
 exhausted.
#### RPC_X_NULL_REF_POINTER
A null reference pointer was
 passed to the stub.
#### RPC_X_SS_CANNOT_GET_CALL_HANDLE
The stub is unable to get the
 call handle.
#### RPC_X_SS_CHAR_TRANS_OPEN_FAIL
The file designated by
 DCERPCCHARTRANS cannot be
 opened.
#### RPC_X_SS_CHAR_TRANS_SHORT_FILE
The file containing the
 character translation table
 has fewer than 512 bytes.
#### RPC_X_SS_CONTEXT_DAMAGED
The context handle changed
 during a call.
#### RPC_X_SS_CONTEXT_MISMATCH
The context handle does not
 match any known context
 handles.
#### RPC_X_SS_HANDLES_MISMATCH
The binding handles passed to
 a remote procedure call do not
 match.
#### RPC_X_SS_IN_NULL_CONTEXT
A null context handle is
 passed as an [in] parameter.
#### RPC_X_WRONG_ES_VERSION
Incompatible version of the
 serializing package.
#### RPC_X_WRONG_PIPE_VERSION
The idl pipe version is not
 supported.
#### RPC_X_WRONG_STUB_VERSION
Incompatible version of the
 RPC stub.
