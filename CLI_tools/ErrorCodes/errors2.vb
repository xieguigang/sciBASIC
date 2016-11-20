#Region "Microsoft.VisualBasic::124dfe046de77fb6976900779877617f, ..\sciBASIC#\CLI_tools\ErrorCodes\errors2.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Public Enum ErrorCodes As Long
'''<summary>
''' Indicates not all privileges
''' referenced are assigned to the
''' caller. This allows, for
''' example, all privileges to be
''' disabled without having to
''' know exactly which privileges
''' are assigned.
'''</summary>
ERROR_NOT_ALL_ASSIGNED = 1300L

'''<summary>
''' Some of the information to be
''' mapped has not been
''' translated.
'''</summary>
ERROR_SOME_NOT_MAPPED = 1301L

'''<summary>
''' No system quota limits are
''' specifically set for this
''' account.
'''</summary>
ERROR_NO_QUOTAS_FOR_ACCOUNT = 1302L

'''<summary>
''' A user session key was
''' requested for a local RPC
''' connection. The session key
''' returned is a constant value
''' and not unique to this
''' connection.
'''</summary>
ERROR_LOCAL_USER_SESSION_KEY = 1303L

'''<summary>
''' The Windows NT password is too
''' complex to be converted to a
''' Windows-networking password.
''' The Windows-networking
''' password returned is a NULL
''' string.
'''</summary>
ERROR_NULL_LM_PASSWORD = 1304L

'''<summary>
''' Indicates an encountered or
''' specified revision number is
''' not one known by the service.
''' The service may not be aware
''' of a more recent revision.
'''</summary>
ERROR_UNKNOWN_REVISION = 1305L

'''<summary>
''' Indicates two revision levels
''' are incompatible.
'''</summary>
ERROR_REVISION_MISMATCH = 1306L

'''<summary>
''' Indicates a particular
''' Security ID cannot be assigned
''' as the owner of an object.
'''</summary>
ERROR_INVALID_OWNER = 1307L

'''<summary>
''' Indicates a particular
''' Security ID cannot be assigned
''' as the primary group of an
''' object.
'''</summary>
ERROR_INVALID_PRIMARY_GROUP = 1308L

'''<summary>
''' An attempt was made to operate
''' on an impersonation token by a
''' thread was not currently
''' impersonating a client.
'''</summary>
ERROR_NO_IMPERSONATION_TOKEN = 1309L

'''<summary>
''' A mandatory group cannot be
''' disabled.
'''</summary>
ERROR_CANT_DISABLE_MANDATORY = 1310L

'''<summary>
''' There are currently no logon
''' servers available to service
''' the logon request.
'''</summary>
ERROR_NO_LOGON_SERVERS = 1311L

'''<summary>
''' A specified logon session does
''' not exist. It may already have
''' been terminated.
'''</summary>
ERROR_NO_SUCH_LOGON_SESSION = 1312L

'''<summary>
''' A specified privilege does not
''' exist.
'''</summary>
ERROR_NO_SUCH_PRIVILEGE = 1313L

'''<summary>
''' A required privilege is not
''' held by the client.
'''</summary>
ERROR_PRIVILEGE_NOT_HELD = 1314L

'''<summary>
''' The name provided is not a
''' properly formed account name.
'''</summary>
ERROR_INVALID_ACCOUNT_NAME = 1315L

'''<summary>
''' The specified user already
''' exists.
'''</summary>
ERROR_USER_EXISTS = 1316L

'''<summary>
''' The specified user does not
''' exist.
'''</summary>
ERROR_NO_SUCH_USER = 1317L

'''<summary>
''' The specified group already
''' exists.
''' 
'''</summary>
ERROR_GROUP_EXISTS = 1318L

'''<summary>
''' The specified group does not
''' exist.
'''</summary>
ERROR_NO_SUCH_GROUP = 1319L

'''<summary>
''' The specified user account is
''' already in the specified group
''' account. Also used to indicate
''' a group can not be deleted
''' because it contains a member.
'''</summary>
ERROR_MEMBER_IN_GROUP = 1320L

'''<summary>
''' The specified user account is
''' not a member of the specified
''' group account.
'''</summary>
ERROR_MEMBER_NOT_IN_GROUP = 1321L

'''<summary>
''' Indicates the requested
''' operation would disable or
''' delete the last remaining
''' administration account. This
''' is not allowed to prevent
''' creating a situation where the
''' system will not be
''' administrable.
'''</summary>
ERROR_LAST_ADMIN = 1322L

'''<summary>
''' When trying to update a
''' password, this return status
''' indicates the value provided
''' as the current password is
''' incorrect.
'''</summary>
ERROR_WRONG_PASSWORD = 1323L

'''<summary>
''' When trying to update a
''' password, this return status
''' indicates the value provided
''' for the new password contains
''' values not allowed in
''' passwords.
'''</summary>
ERROR_ILL_FORMED_PASSWORD = 1324L

'''<summary>
''' When trying to update a
''' password, this status
''' indicates that some password
''' update rule was violated. For
''' example, the password may not
''' meet length criteria.
'''</summary>
ERROR_PASSWORD_RESTRICTION = 1325L

'''<summary>
''' The attempted logon is
''' invalid. This is due to either
''' a bad user name or
''' authentication information.
'''</summary>
ERROR_LOGON_FAILURE = 1326L

'''<summary>
''' Indicates a referenced user
''' name and authentication
''' information are valid, but
''' some user account restriction
''' has prevented successful
''' authentication (such as time-
''' of-day restrictions).
'''</summary>
ERROR_ACCOUNT_RESTRICTION = 1327L

'''<summary>
''' The user account has time
''' restrictions and cannot be
''' logged onto at this time.
'''</summary>
ERROR_INVALID_LOGON_HOURS = 1328L

'''<summary>
''' The user account is restricted
''' and cannot be used to log on
''' from the source workstation.
'''</summary>
ERROR_INVALID_WORKSTATION = 1329L

'''<summary>
''' The user account's password
''' has expired.
'''</summary>
ERROR_PASSWORD_EXPIRED = 1330L

'''<summary>
''' The referenced account is
''' currently disabled and cannot
''' be logged on to.
'''</summary>
ERROR_ACCOUNT_DISABLED = 1331L

'''<summary>
''' None of the information to be
''' mapped has been translated.
'''</summary>
ERROR_NONE_MAPPED = 1332L

'''<summary>
''' The number of LUID requested
''' cannot be allocated with a
''' single allocation.
'''</summary>
ERROR_TOO_MANY_LUIDS_REQUESTED = 1333L

'''<summary>
''' Indicates there are no more
''' LUID to allocate.
'''</summary>
ERROR_LUIDS_EXHAUSTED = 1334L

'''<summary>
''' Indicates the sub-authority
''' value is invalid for the
''' particular use.
'''</summary>
ERROR_INVALID_SUB_AUTHORITY = 1335L

'''<summary>
''' Indicates the ACL structure is
''' not valid.
'''</summary>
ERROR_INVALID_ACL = 1336L

'''<summary>
''' Indicates the SID structure is
''' invalid.
'''</summary>
ERROR_INVALID_SID = 1337L

'''<summary>
''' Indicates the
''' SECURITY_DESCRIPTOR structure
''' is invalid.
'''</summary>
ERROR_INVALID_SECURITY_DESCR = 1338L

'''<summary>
''' Indicates that an attempt to
''' build either an inherited ACL
''' or ACE did not succeed. One of
''' the more probable causes is
''' the replacement of a CreatorId
''' with an SID that didn't fit
''' into the ACE or ACL.
'''</summary>
ERROR_BAD_INHERITANCE_ACL = 1340L

'''<summary>
''' The GUID allocation server is
''' already disabled at the
''' moment.
'''</summary>
ERROR_SERVER_DISABLED = 1341L

'''<summary>
''' The GUID allocation server is
''' already enabled at the moment.
'''</summary>
ERROR_SERVER_NOT_DISABLED = 1342L

'''<summary>
''' The value provided is an
''' invalid value for an
''' identifier authority.
'''</summary>
ERROR_INVALID_ID_AUTHORITY = 1343L

'''<summary>
''' When a block of memory is
''' allotted for future updates,
''' such as the memory allocated
''' to hold discretionary access
''' control and primary group
''' information, successive
''' updates may exceed the amount
''' of memory originally allotted.
''' Since quota may already have
''' been charged to several
''' processes that have handles of
''' the object, it is not
''' reasonable to alter the size
''' of the allocated memory.
''' Instead, a request that
''' requires more memory than has
''' been allotted must fail and
''' the
''' ERROR_ALLOTTED_SPACE_EXCEEDED
''' error returned.
'''</summary>
ERROR_ALLOTTED_SPACE_EXCEEDED = 1344L

'''<summary>
''' The specified attributes are
''' invalid, or incompatible with
''' the attributes for the group
''' as a whole.
'''</summary>
ERROR_INVALID_GROUP_ATTRIBUTES = 1345L

'''<summary>
''' A specified impersonation
''' level is invalid. Also used to
''' indicate a required
''' impersonation level was not
''' provided.
'''</summary>
ERROR_BAD_IMPERSONATION_LEVEL = 1346L

'''<summary>
''' An attempt was made to open an
''' anonymous level token.
''' Anonymous tokens cannot be
''' opened.
'''</summary>
ERROR_CANT_OPEN_ANONYMOUS = 1347L

'''<summary>
''' The requested validation
''' information class is invalid.
'''</summary>
ERROR_BAD_VALIDATION_CLASS = 1348L

'''<summary>
''' The type of token object is
''' inappropriate for its
''' attempted use.
'''</summary>
ERROR_BAD_TOKEN_TYPE = 1349L

'''<summary>
''' Indicates an attempt was made
''' to operate on the security of
''' an object that does not have
''' security associated with it.
'''</summary>
ERROR_NO_SECURITY_ON_OBJECT = 1350L

'''<summary>
''' Indicates a domain controller
''' could not be contacted or that
''' objects within the domain are
''' protected and necessary
''' information could not be
''' retrieved.
'''</summary>
ERROR_CANT_ACCESS_DOMAIN_INFO = 1351L

'''<summary>
''' Indicates the Sam Server was
''' in the wrong state to perform
''' the desired operation.
'''</summary>
ERROR_INVALID_SERVER_STATE = 1352L

'''<summary>
''' Indicates the domain is in the
''' wrong state to perform the
''' desired operation.
'''</summary>
ERROR_INVALID_DOMAIN_STATE = 1353L

'''<summary>
''' Indicates the requested
''' operation cannot be completed
''' with the domain in its present
''' role.
'''</summary>
ERROR_INVALID_DOMAIN_ROLE = 1354L

'''<summary>
''' The specified domain does not
''' exist.
'''</summary>
ERROR_NO_SUCH_DOMAIN = 1355L

'''<summary>
''' The specified domain already
''' exists.
'''</summary>
ERROR_DOMAIN_EXISTS = 1356L

'''<summary>
''' An attempt to exceed the limit
''' on the number of domains per
''' server for this release.
'''</summary>
ERROR_DOMAIN_LIMIT_EXCEEDED = 1357L

'''<summary>
''' This error indicates the
''' requested operation cannot be
''' completed due to a
''' catastrophic media failure or
''' on-disk data structure
''' corruption.
'''</summary>
ERROR_INTERNAL_DB_CORRUPTION = 1358L

'''<summary>
''' This error indicates the SAM
''' server has encounterred an
''' internal consistency error in
''' its database. This
''' catastrophic failure prevents
''' further operation of SAM.
'''</summary>
ERROR_INTERNAL_ERROR = 1359L

'''<summary>
''' Indicates generic access types
''' were contained in an access
''' mask that should already be
''' mapped to non-generic access
''' types.
'''</summary>
ERROR_GENERIC_NOT_MAPPED = 1360L

'''<summary>
''' Indicates a security
''' descriptor is not in the
''' required format (absolute or
''' self-relative).
'''</summary>
ERROR_BAD_DESCRIPTOR_FORMAT = 1361L

'''<summary>
''' The requested action is
''' restricted for use by logon
''' processes only. The calling
''' process has not registered as
''' a logon process.
'''</summary>
ERROR_NOT_LOGON_PROCESS = 1362L

'''<summary>
''' An attempt was made to start a
''' new session manager or LSA
''' logon session with an ID
''' already in use.
'''</summary>
ERROR_LOGON_SESSION_EXISTS = 1363L

'''<summary>
''' A specified authentication
''' package is unknown.
'''</summary>
ERROR_NO_SUCH_PACKAGE = 1364L

'''<summary>
''' The logon session is not in a
''' state consistent with the
''' requested operation.
'''</summary>
ERROR_BAD_LOGON_SESSION_STATE = 1365L

'''<summary>
''' The logon session ID is
''' already in use.
'''</summary>
ERROR_LOGON_SESSION_COLLISION = 1366L

'''<summary>
''' Indicates an invalid value has
''' been provided for LogonType
''' has been requested.
'''</summary>
ERROR_INVALID_LOGON_TYPE = 1367L

'''<summary>
''' Indicates that an attempt was
''' made to impersonate via a
''' named pipe was not yet read
''' from.
'''</summary>
ERROR_CANNOT_IMPERSONATE = 1368L

'''<summary>
''' Indicates that the transaction
''' state of a registry sub-tree
''' is incompatible with the
''' requested operation. For
''' example, a request has been
''' made to start a new
''' transaction with one already
''' in progress, or a request to
''' apply a transaction when one
''' is not currently in progress.
''' This status value is returned
''' by the runtime library (RTL)
''' registry transaction package
''' (RXact).
'''</summary>
ERROR_RXACT_INVALID_STATE = 1369L

'''<summary>
''' Indicates an error occurred
''' during a registry transaction
''' commit. The database has been
''' left in an unknown state. The
''' state of the registry
''' transaction is left as
''' COMMITTING. This status value
''' is returned by the runtime
''' library (RTL) registry
''' transaction package (RXact).
'''</summary>
ERROR_RXACT_COMMIT_FAILURE = 1370L

'''<summary>
''' Indicates an operation was
''' attempted on a built-in
''' (special) SAM account that is
''' incompatible with built-in
''' accounts. For example, built-
''' in accounts cannot be renamed
''' or deleted.
'''</summary>
ERROR_SPECIAL_ACCOUNT = 1371L

'''<summary>
''' The requested operation cannot
''' be performed on the specified
''' group because it is a built-in
''' special group.
'''</summary>
ERROR_SPECIAL_GROUP = 1372L

'''<summary>
''' The requested operation cannot
''' be performed on the specified
''' user because it is a built-in
''' special user.
'''</summary>
ERROR_SPECIAL_USER = 1373L

'''<summary>
''' Indicates a member cannot be
''' removed from a group because
''' the group is currently the
''' member's primary group.
'''</summary>
ERROR_MEMBERS_PRIMARY_GROUP = 1374L

'''<summary>
''' An attempt was made to
''' establish a token for use as a
''' primary token but the token is
''' already in use. A token can
''' only be the primary token of
''' one process at a time.
'''</summary>
ERROR_TOKEN_ALREADY_IN_USE = 1375L

'''<summary>
''' The specified alias does not
''' exist.
'''</summary>
ERROR_NO_SUCH_ALIAS = 1376L

'''<summary>
''' The specified account name is
''' not a member of the alias.
'''</summary>
ERROR_MEMBER_NOT_IN_ALIAS = 1377L

'''<summary>
''' The specified account name is
''' not a member of the alias.
'''</summary>
ERROR_MEMBER_IN_ALIAS = 1378L

'''<summary>
''' The specified alias already
''' exists.
'''</summary>
ERROR_ALIAS_EXISTS = 1379L

'''<summary>
''' A requested type of logon,
''' such as Interactive, Network,
''' or Service, is not granted by
''' the target system's local
''' security policy. The system
''' administrator can grant the
''' required form of logon.
'''</summary>
ERROR_LOGON_NOT_GRANTED = 1380L

'''<summary>
''' The maximum number of secrets
''' that can be stored in a single
''' system was exceeded.
'''</summary>
ERROR_TOO_MANY_SECRETS = 1381L

'''<summary>
''' The length of a secret exceeds
''' the maximum length allowed.
'''</summary>
ERROR_SECRET_TOO_LONG = 1382L

'''<summary>
''' The Local Security Authority
''' (LSA) database contains in
''' internal inconsistency.
'''</summary>
ERROR_INTERNAL_DB_ERROR = 1383L

'''<summary>
''' During a logon attempt, the
''' user's security context
''' accumulated too many security
''' IDs. Remove the user from some
''' groups or aliases to reduce
''' the number of security ids to
''' incorporate into the security
''' context.
'''</summary>
ERROR_TOO_MANY_CONTEXT_IDS = 1384L

'''<summary>
''' A user has requested a type of
''' logon, such as interactive or
''' network, that was not granted.
''' An administrator has control
''' over who may logon
''' interactively and through the
''' network.
'''</summary>
ERROR_LOGON_TYPE_NOT_GRANTED = 1385L

'''<summary>
''' An attempt was made to change
''' a user password in the
''' security account manager
''' without providing the
''' necessary NT cross-encrypted
''' password.
'''</summary>
ERROR_NT_CROSS_ENCRYPTION_REQUIRED = 1386L

'''<summary>
''' A new member cannot be added
''' to an alias because the member
''' does not exist.
'''</summary>
ERROR_NO_SUCH_MEMBER = 1387L

'''<summary>
''' A new member could not be
''' added to an alias because the
''' member has the wrong account
''' type.
'''</summary>
ERROR_INVALID_MEMBER = 1388L

'''<summary>
''' Too many SIDs specified.
'''</summary>
ERROR_TOO_MANY_SIDS = 1389L

'''<summary>
''' An attempt was made to change
''' a user password in the
''' security account manager
''' without providing the required
''' LM cross-encrypted password.
'''</summary>
ERROR_LM_CROSS_ENCRYPTION_REQUIRED = 1390L

'''<summary>
''' Indicates an ACL contains no
''' inheritable components.
'''</summary>
ERROR_NO_INHERITANCE = 1391L

'''<summary>
''' The file or directory is
''' damaged and nonreadable.
'''</summary>
ERROR_FILE_CORRUPT = 1392L

'''<summary>
''' The disk structure is damaged
''' and nonreadable.
'''</summary>
ERROR_DISK_CORRUPT = 1393L

'''<summary>
''' There is no user session key
''' for the specified logon
''' session.
'''</summary>
ERROR_NO_USER_SESSION_KEY = 1394L

'''<summary>
''' The service being accessed is
''' licensed for a particular
''' number of connections. No more
''' connections can be made to the
''' service at this time because
''' there are already as many
''' connections as the service can
''' accept.
'''</summary>
ERROR_LICENSE_QUOTA_EXCEEDED = 1395L

'''<summary>
''' The window handle invalid.
'''</summary>
ERROR_INVALID_WINDOW_HANDLE = 1400L

'''<summary>
''' The menu handle is invalid.
'''</summary>
ERROR_INVALID_MENU_HANDLE = 1401L

'''<summary>
''' The cursor handle is invalid.
'''</summary>
ERROR_INVALID_CURSOR_HANDLE = 1402L

'''<summary>
''' Invalid accelerator-table
''' handle.
'''</summary>
ERROR_INVALID_ACCEL_HANDLE = 1403L

'''<summary>
''' The hook handle is invalid.
'''</summary>
ERROR_INVALID_HOOK_HANDLE = 1404L

'''<summary>
''' The DeferWindowPos handle is
''' invalid.
'''</summary>
ERROR_INVALID_DWP_HANDLE = 1405L

'''<summary>
''' CreateWindow failed, creating
''' top-level window with WS_CHILD
''' style.
'''</summary>
ERROR_TLW_WITH_WSCHILD = 1406L

'''<summary>
''' Cannot find window class.
'''</summary>
ERROR_CANNOT_FIND_WND_CLASS = 1407L

'''<summary>
''' Invalid window, belongs to
''' other thread.
'''</summary>
ERROR_WINDOW_OF_OTHER_THREAD = 1408L

'''<summary>
''' Hotkey is already registered.
'''</summary>
ERROR_HOTKEY_ALREADY_REGISTERED = 1409L

'''<summary>
''' Class already exists.
'''</summary>
ERROR_CLASS_ALREADY_EXISTS = 1410L

'''<summary>
''' Class does not exist.
'''</summary>
ERROR_CLASS_DOES_NOT_EXIST = 1411L

'''<summary>
''' Class still has open windows.
'''</summary>
ERROR_CLASS_HAS_WINDOWS = 1412L

'''<summary>
''' The index is invalid.
'''</summary>
ERROR_INVALID_INDEX = 1413L

'''<summary>
''' The icon handle is invalid.
'''</summary>
ERROR_INVALID_ICON_HANDLE = 1414L

'''<summary>
''' Using private DIALOG window
''' words.
'''</summary>
ERROR_PRIVATE_DIALOG_INDEX = 1415L

'''<summary>
''' List box ID not found.
'''</summary>
ERROR_LISTBOX_ID_NOT_FOUND = 1416L

'''<summary>
''' No wildcard characters found.
'''</summary>
ERROR_NO_WILDCARD_CHARACTERS = 1417L

'''<summary>
''' Thread doesn't have clipboard
''' open.
'''</summary>
ERROR_CLIPBOARD_NOT_OPEN = 1418L

'''<summary>
''' Hotkey is not registered.
'''</summary>
ERROR_HOTKEY_NOT_REGISTERED = 1419L

'''<summary>
''' The window is not a valid
''' dialog window.
'''</summary>
ERROR_WINDOW_NOT_DIALOG = 1420L

'''<summary>
''' Control ID not found.
'''</summary>
ERROR_CONTROL_ID_NOT_FOUND = 1421L

'''<summary>
''' Invalid Message, combo box
''' doesn't have an edit control.
'''</summary>
ERROR_INVALID_COMBOBOX_MESSAGE = 1422L

'''<summary>
''' The window is not a combo box.
'''</summary>
ERROR_WINDOW_NOT_COMBOBOX = 1423L

'''<summary>
''' Height must be less than 256.
'''</summary>
ERROR_INVALID_EDIT_HEIGHT = 1424L

'''<summary>
''' Invalid HDC passed to
''' ReleaseDC.
'''</summary>
ERROR_DC_NOT_FOUND = 1425L

'''<summary>
''' The hook filter type is
''' invalid.
'''</summary>
ERROR_INVALID_HOOK_FILTER = 1426L

'''<summary>
''' The filter proc is invalid.
'''</summary>
ERROR_INVALID_FILTER_PROC = 1427L

'''<summary>
''' Cannot set non-local hook
''' without an module handle.
'''</summary>
ERROR_HOOK_NEEDS_HMOD = 1428L

'''<summary>
''' This hook can only be set
''' globally.
'''</summary>
ERROR_GLOBAL_ONLY_HOOK = 1429L

'''<summary>
''' The journal hook is already
''' installed.
'''</summary>
ERROR_JOURNAL_HOOK_SET = 1430L

'''<summary>
''' Hook is not installed.
'''</summary>
ERROR_HOOK_NOT_INSTALLED = 1431L

'''<summary>
''' The message for single-
''' selection list box is invalid.
'''</summary>
ERROR_INVALID_LB_MESSAGE = 1432L

'''<summary>
''' LB_SETCOUNT sent to non-lazy
''' list box.
'''</summary>
ERROR_SETCOUNT_ON_BAD_LB = 1433L

'''<summary>
''' This list box doesn't support
''' tab stops.
'''</summary>
ERROR_LB_WITHOUT_TABSTOPS = 1434L

'''<summary>
''' Cannot destroy object created
''' by another thread.
'''</summary>
ERROR_DESTROY_OBJECT_OF_OTHER_THREAD = 1435L

'''<summary>
''' Child windows can't have
''' menus.
'''</summary>
ERROR_CHILD_WINDOW_MENU = 1436L

'''<summary>
''' Window does not have system
''' menu.
'''</summary>
ERROR_NO_SYSTEM_MENU = 1437L

'''<summary>
''' The message box style is
''' invalid.
'''</summary>
ERROR_INVALID_MSGBOX_STYLE = 1438L

'''<summary>
''' The SPI_* parameter is
''' invalid.
'''</summary>
ERROR_INVALID_SPI_VALUE = 1439L

'''<summary>
''' Screen already locked.
'''</summary>
ERROR_SCREEN_ALREADY_LOCKED = 1440L

'''<summary>
''' All DeferWindowPos HWNDs must
''' have same parent.
'''</summary>
ERROR_HWNDS_HAVE_DIFFERENT_PARENT = 1441L

'''<summary>
''' Window is not a child window.
'''</summary>
ERROR_NOT_CHILD_WINDOW = 1442L

'''<summary>
''' The GW_* command is invalid.
'''</summary>
ERROR_INVALID_GW_COMMAND = 1443L

'''<summary>
''' The thread ID is invalid.
'''</summary>
ERROR_INVALID_THREAD_ID = 1444L

'''<summary>
''' DefMDIChildProc called with a
''' non-MDI child window.
'''</summary>
ERROR_NON_MDICHILD_WINDOW = 1445L

'''<summary>
''' Pop-up menu already active.
'''</summary>
ERROR_POPUP_ALREADY_ACTIVE = 1446L

'''<summary>
''' Window does not have scroll
''' bars.
'''</summary>
ERROR_NO_SCROLLBARS = 1447L

'''<summary>
''' Scrollbar range greater than
''' 0x7FFF.
'''</summary>
ERROR_INVALID_SCROLLBAR_RANGE = 1448L

'''<summary>
''' The ShowWindow command is
''' invalid.
'''</summary>
ERROR_INVALID_SHOWWIN_COMMAND = 1449L

'''<summary>
''' Insufficient system resources
''' exist to complete the
''' requested service.
'''</summary>
ERROR_NO_SYSTEM_RESOURCES = 1450L

'''<summary>
''' Insufficient system resources
''' exist to complete the
''' requested service.
'''</summary>
ERROR_NONPAGED_SYSTEM_RESOURCES = 1451L

'''<summary>
''' Insufficient system resources
''' exist to complete the
''' requested service.
'''</summary>
ERROR_PAGED_SYSTEM_RESOURCES = 1452L

'''<summary>
''' Insufficient quota to complete
''' the requested service.
'''</summary>
ERROR_WORKING_SET_QUOTA = 1453L

'''<summary>
''' Insufficient quota to complete
''' the requested service.
'''</summary>
ERROR_PAGEFILE_QUOTA = 1454L

'''<summary>
''' The paging file is too small
''' for this operation to
''' complete.
'''</summary>
ERROR_COMMITMENT_LIMIT = 1455L

'''<summary>
''' A menu item was not found.
'''</summary>
ERROR_MENU_ITEM_NOT_FOUND = 1456L

'''<summary>
''' Invalid keyboard layout
''' handle.
'''</summary>
ERROR_INVALID_KEYBOARD_HANDLE = 1457L

'''<summary>
''' Hook type not allowed.
'''</summary>
ERROR_HOOK_TYPE_NOT_ALLOWED = 1458L

'''<summary>
''' One of the Eventlog logfiles
''' is damaged.
'''</summary>
ERROR_EVENTLOG_FILE_CORRUPT = 1500L

'''<summary>
''' No event log file could be
''' opened, so the event logging
''' service did not start.
'''</summary>
ERROR_EVENTLOG_CANT_START = 1501L

'''<summary>
''' The event log file is full.
'''</summary>
ERROR_LOG_FILE_FULL = 1502L

'''<summary>
''' The event log file has changed
''' between reads.
'''</summary>
ERROR_EVENTLOG_FILE_CHANGED = 1503L

'''<summary>
''' The string binding is invalid.
'''</summary>
RPC_S_INVALID_STRING_BINDING = 1700L

'''<summary>
''' The binding handle is the
''' incorrect type.
'''</summary>
RPC_S_WRONG_KIND_OF_BINDING = 1701L

'''<summary>
''' The binding handle is invalid.
'''</summary>
RPC_S_INVALID_BINDING = 1702L

'''<summary>
''' The RPC protocol sequence is
''' not supported.
'''</summary>
RPC_S_PROTSEQ_NOT_SUPPORTED = 1703L

'''<summary>
''' The RPC protocol sequence is
''' invalid.
'''</summary>
RPC_S_INVALID_RPC_PROTSEQ = 1704L

'''<summary>
''' The string UUID is invalid.
'''</summary>
RPC_S_INVALID_STRING_UUID = 1705L

'''<summary>
''' The endpoint format is
''' invalid.
'''</summary>
RPC_S_INVALID_ENDPOINT_FORMAT = 1706L

'''<summary>
''' The network address is
''' invalid.
'''</summary>
RPC_S_INVALID_NET_ADDR = 1707L

'''<summary>
''' No endpoint was found.
'''</summary>
RPC_S_NO_ENDPOINT_FOUND = 1708L

'''<summary>
''' The timeout value is invalid.
'''</summary>
RPC_S_INVALID_TIMEOUT = 1709L

'''<summary>
''' The object UUID was not found.
'''</summary>
RPC_S_OBJECT_NOT_FOUND = 1710L

'''<summary>
''' The object UUID already
''' registered.
'''</summary>
RPC_S_ALREADY_REGISTERED = 1711L

'''<summary>
''' The type UUID is already
''' registered.
'''</summary>
RPC_S_TYPE_ALREADY_REGISTERED = 1712L

'''<summary>
''' The server is already
''' listening.
'''</summary>
RPC_S_ALREADY_LISTENING = 1713L

'''<summary>
''' No protocol sequences were
''' registered.
'''</summary>
RPC_S_NO_PROTSEQS_REGISTERED = 1714L

'''<summary>
''' The server is not listening.
'''</summary>
RPC_S_NOT_LISTENING = 1715L

'''<summary>
''' The manager type is unknown.
'''</summary>
RPC_S_UNKNOWN_MGR_TYPE = 1716L

'''<summary>
''' The interface is unknown.
'''</summary>
RPC_S_UNKNOWN_IF = 1717L

'''<summary>
''' There are no bindings.
'''</summary>
RPC_S_NO_BINDINGS = 1718L

'''<summary>
''' There are no protocol
''' sequences.
'''</summary>
RPC_S_NO_PROTSEQS = 1719L

'''<summary>
''' The endpoint cannot be
''' created.
'''</summary>
RPC_S_CANT_CREATE_ENDPOINT = 1720L

'''<summary>
''' Not enough resources are
''' available to complete this
''' operation.
'''</summary>
RPC_S_OUT_OF_RESOURCES = 1721L

'''<summary>
''' The server is unavailable.
'''</summary>
RPC_S_SERVER_UNAVAILABLE = 1722L

'''<summary>
''' The server is too busy to
''' complete this operation.
'''</summary>
RPC_S_SERVER_TOO_BUSY = 1723L

'''<summary>
''' The network options are
''' invalid.
'''</summary>
RPC_S_INVALID_NETWORK_OPTIONS = 1724L

'''<summary>
''' There is not a remote
''' procedure call active in this
''' thread.
'''</summary>
RPC_S_NO_CALL_ACTIVE = 1725L

'''<summary>
''' The remote procedure call
''' failed.
'''</summary>
RPC_S_CALL_FAILED = 1726L

'''<summary>
''' The remote procedure call
''' failed and did not execute.
'''</summary>
RPC_S_CALL_FAILED_DNE = 1727L

'''<summary>
''' An RPC protocol error
''' occurred.
'''</summary>
RPC_S_PROTOCOL_ERROR = 1728L

'''<summary>
''' The transfer syntax is not
''' supported by the server.
'''</summary>
RPC_S_UNSUPPORTED_TRANS_SYN = 1730L

'''<summary>
''' The server has insufficient
''' memory to complete this
''' operation.
'''</summary>
RPC_S_SERVER_OUT_OF_MEMORY = 1731L

'''<summary>
''' The type UUID is not
''' supported.
'''</summary>
RPC_S_UNSUPPORTED_TYPE = 1732L

'''<summary>
''' The tag is invalid.
'''</summary>
RPC_S_INVALID_TAG = 1733L

'''<summary>
''' The array bounds are invalid.
'''</summary>
RPC_S_INVALID_BOUND = 1734L

'''<summary>
''' The binding does not contain
''' an entry name.
'''</summary>
RPC_S_NO_ENTRY_NAME = 1735L

'''<summary>
''' The name syntax is invalid.
'''</summary>
RPC_S_INVALID_NAME_SYNTAX = 1736L

'''<summary>
''' The name syntax is not
''' supported.
'''</summary>
RPC_S_UNSUPPORTED_NAME_SYNTAX = 1737L

'''<summary>
''' No network address is
''' available to use to construct
''' a UUID.
'''</summary>
RPC_S_UUID_NO_ADDRESS = 1739L

'''<summary>
''' The endpoint is a duplicate.
'''</summary>
RPC_S_DUPLICATE_ENDPOINT = 1740L

'''<summary>
''' The authentication type is
''' unknown.
'''</summary>
RPC_S_UNKNOWN_AUTHN_TYPE = 1741L

'''<summary>
''' The maximum number of calls is
''' too small.
'''</summary>
RPC_S_MAX_CALLS_TOO_SMALL = 1742L

'''<summary>
''' The string is too long.
'''</summary>
RPC_S_STRING_TOO_LONG = 1743L

'''<summary>
''' The RPC protocol sequence was
''' not found.
'''</summary>
RPC_S_PROTSEQ_NOT_FOUND = 1744L

'''<summary>
''' The procedure number is out of
''' range.
'''</summary>
RPC_S_PROCNUM_OUT_OF_RANGE = 1745L

'''<summary>
''' The binding does not contain
''' any authentication
''' information.
'''</summary>
RPC_S_BINDING_HAS_NO_AUTH = 1746L

'''<summary>
''' The authentication service is
''' unknown.
'''</summary>
RPC_S_UNKNOWN_AUTHN_SERVICE = 1747L

'''<summary>
''' The authentication level is
''' unknown.
'''</summary>
RPC_S_UNKNOWN_AUTHN_LEVEL = 1748L

'''<summary>
''' The security context is
''' invalid.
'''</summary>
RPC_S_INVALID_AUTH_IDENTITY = 1749L

'''<summary>
''' The authorization service is
''' unknown.
'''</summary>
RPC_S_UNKNOWN_AUTHZ_SERVICE = 1750L

'''<summary>
''' The entry is invalid.
'''</summary>
EPT_S_INVALID_ENTRY = 1751L

'''<summary>
''' The operation cannot be
''' performed.
'''</summary>
EPT_S_CANT_PERFORM_OP = 1752L

'''<summary>
''' There are no more endpoints
''' available from the endpoint
''' mapper.
'''</summary>
EPT_S_NOT_REGISTERED = 1753L

'''<summary>
''' The entry name is incomplete.
'''</summary>
RPC_S_INCOMPLETE_NAME = 1755L

'''<summary>
''' The version option is invalid.
'''</summary>
RPC_S_INVALID_VERS_OPTION = 1756L

'''<summary>
''' There are no more members.
'''</summary>
RPC_S_NO_MORE_MEMBERS = 1757L

'''<summary>
''' There is nothing to unexport.
'''</summary>
RPC_S_NOT_ALL_OBJS_UNEXPORTED = 1758L

'''<summary>
''' The interface was not found.
'''</summary>
RPC_S_INTERFACE_NOT_FOUND = 1759L

'''<summary>
''' The entry already exists.
'''</summary>
RPC_S_ENTRY_ALREADY_EXISTS = 1760L

'''<summary>
''' The entry is not found.
'''</summary>
RPC_S_ENTRY_NOT_FOUND = 1761L

'''<summary>
''' The name service is
''' unavailable.
'''</summary>
RPC_S_NAME_SERVICE_UNAVAILABLE = 1762L

'''<summary>
''' The requested operation is not
''' supported.
'''</summary>
RPC_S_CANNOT_SUPPORT = 1764L

'''<summary>
''' No security context is
''' available to allow
''' impersonation.
'''</summary>
RPC_S_NO_CONTEXT_AVAILABLE = 1765L

'''<summary>
''' An internal error occurred in
''' RPC.
'''</summary>
RPC_S_INTERNAL_ERROR = 1766L

'''<summary>
''' The server attempted an
''' integer divide by zero.
'''</summary>
RPC_S_ZERO_DIVIDE = 1767L

'''<summary>
''' An addressing error occurred
''' in the server.
'''</summary>
RPC_S_ADDRESS_ERROR = 1768L

'''<summary>
''' A floating point operation at
''' the server caused a divide by
''' zero.
'''</summary>
RPC_S_FP_DIV_ZERO = 1769L

'''<summary>
''' A floating point underflow
''' occurred at the server.
'''</summary>
RPC_S_FP_UNDERFLOW = 1770L

'''<summary>
''' A floating point overflow
''' occurred at the server.
'''</summary>
RPC_S_FP_OVERFLOW = 1771L

'''<summary>
''' The list of servers available
''' for auto_handle binding was
''' exhausted.
'''</summary>
RPC_X_NO_MORE_ENTRIES = 1772L

'''<summary>
''' The file designated by
''' DCERPCCHARTRANS cannot be
''' opened.
'''</summary>
RPC_X_SS_CHAR_TRANS_OPEN_FAIL = 1773L

'''<summary>
''' The file containing the
''' character translation table
''' has fewer than 512 bytes.
'''</summary>
RPC_X_SS_CHAR_TRANS_SHORT_FILE = 1774L

'''<summary>
''' A null context handle is
''' passed as an [in] parameter.
'''</summary>
RPC_X_SS_IN_NULL_CONTEXT = 1775L

'''<summary>
''' The context handle does not
''' match any known context
''' handles.
'''</summary>
RPC_X_SS_CONTEXT_MISMATCH = 1776L

'''<summary>
''' The context handle changed
''' during a call.
'''</summary>
RPC_X_SS_CONTEXT_DAMAGED = 1777L

'''<summary>
''' The binding handles passed to
''' a remote procedure call do not
''' match.
'''</summary>
RPC_X_SS_HANDLES_MISMATCH = 1778L

'''<summary>
''' The stub is unable to get the
''' call handle.
'''</summary>
RPC_X_SS_CANNOT_GET_CALL_HANDLE = 1779L

'''<summary>
''' A null reference pointer was
''' passed to the stub.
'''</summary>
RPC_X_NULL_REF_POINTER = 1780L

'''<summary>
''' The enumeration value is out
''' of range.
'''</summary>
RPC_X_ENUM_VALUE_OUT_OF_RANGE = 1781L

'''<summary>
''' The byte count is too small.
'''</summary>
RPC_X_BYTE_COUNT_TOO_SMALL = 1782L

'''<summary>
''' The stub received bad data.
'''</summary>
RPC_X_BAD_STUB_DATA = 1783L

'''<summary>
''' The supplied user buffer is
''' invalid for the requested
''' operation.
'''</summary>
ERROR_INVALID_USER_BUFFER = 1784L

'''<summary>
''' The disk media is not
''' recognized. It may not be
''' formatted.
'''</summary>
ERROR_UNRECOGNIZED_MEDIA = 1785L

'''<summary>
''' The workstation does not have
''' a trust secret.
'''</summary>
ERROR_NO_TRUST_LSA_SECRET = 1786L

'''<summary>
''' The domain controller does not
''' have an account for this
''' workstation.
'''</summary>
ERROR_NO_TRUST_SAM_ACCOUNT = 1787L

'''<summary>
''' The trust relationship between
''' the primary domain and the
''' trusted domain failed.
'''</summary>
ERROR_TRUSTED_DOMAIN_FAILURE = 1788L

'''<summary>
''' The trust relationship between
''' this workstation and the
''' primary domain failed.
'''</summary>
ERROR_TRUSTED_RELATIONSHIP_FAILURE = 1789L

'''<summary>
''' The network logon failed.
'''</summary>
ERROR_TRUST_FAILURE = 1790L

'''<summary>
''' A remote procedure call is
''' already in progress for this
''' thread.
'''</summary>
RPC_S_CALL_IN_PROGRESS = 1791L

'''<summary>
''' An attempt was made to logon,
''' but the network logon service
''' was not started.
'''</summary>
ERROR_NETLOGON_NOT_STARTED = 1792L

'''<summary>
''' The user's account has
''' expired.
'''</summary>
ERROR_ACCOUNT_EXPIRED = 1793L

'''<summary>
''' The redirector is in use and
''' cannot be unloaded.
'''</summary>
ERROR_REDIRECTOR_HAS_OPEN_HANDLES = 1794L

'''<summary>
''' The specified printer driver
''' is already installed.
'''</summary>
ERROR_PRINTER_DRIVER_ALREADY_INSTALLED = 1795L

'''<summary>
''' The specified port is unknown.
'''</summary>
ERROR_UNKNOWN_PORT = 1796L

'''<summary>
''' The printer driver is unknown.
'''</summary>
ERROR_UNKNOWN_PRINTER_DRIVER = 1797L

'''<summary>
''' The print processor is
''' unknown.
'''</summary>
ERROR_UNKNOWN_PRINTPROCESSOR = 1798L

'''<summary>
''' The specified separator file
''' is invalid.
'''</summary>
ERROR_INVALID_SEPARATOR_FILE = 1799L

'''<summary>
''' The specified priority is
''' invalid.
'''</summary>
ERROR_INVALID_PRIORITY = 1800L

'''<summary>
''' The printer name is invalid.
'''</summary>
ERROR_INVALID_PRINTER_NAME = 1801L

'''<summary>
''' The printer already exists.
'''</summary>
ERROR_PRINTER_ALREADY_EXISTS = 1802L

'''<summary>
''' The printer command is
''' invalid.
'''</summary>
ERROR_INVALID_PRINTER_COMMAND = 1803L

'''<summary>
''' The specified datatype is
''' invalid.
'''</summary>
ERROR_INVALID_DATATYPE = 1804L

'''<summary>
''' The Environment specified is
''' invalid.
'''</summary>
ERROR_INVALID_ENVIRONMENT = 1805L

'''<summary>
''' There are no more bindings.
'''</summary>
RPC_S_NO_MORE_BINDINGS = 1806L

'''<summary>
''' The account used is an
''' interdomain trust account. Use
''' your normal user account or
''' remote user account to access
''' this server.
'''</summary>
ERROR_NOLOGON_INTERDOMAIN_TRUST_ACCOUNT = 1807L

'''<summary>
''' The account used is a
''' workstation trust account. Use
''' your normal user account or
''' remote user account to access
''' this server.
'''</summary>
ERROR_NOLOGON_WORKSTATION_TRUST_ACCOUNT = 1808L

'''<summary>
''' The account used is an server
''' trust account. Use your normal
''' user account or remote user
''' account to access this server.
'''</summary>
ERROR_NOLOGON_SERVER_TRUST_ACCOUNT = 1809L

'''<summary>
''' The name or security ID (SID)
''' of the domain specified is
''' inconsistent with the trust
''' information for that domain.
'''</summary>
ERROR_DOMAIN_TRUST_INCONSISTENT = 1810L

'''<summary>
''' The server is in use and
''' cannot be unloaded.
'''</summary>
ERROR_SERVER_HAS_OPEN_HANDLES = 1811L

'''<summary>
''' The specified image file did
''' not contain a resource
''' section.
'''</summary>
ERROR_RESOURCE_DATA_NOT_FOUND = 1812L

'''<summary>
''' The specified resource type
''' can not be found in the image
''' file.
'''</summary>
ERROR_RESOURCE_TYPE_NOT_FOUND = 1813L

'''<summary>
''' The specified resource name
''' can not be found in the image
''' file.
'''</summary>
ERROR_RESOURCE_NAME_NOT_FOUND = 1814L

'''<summary>
''' The specified resource
''' language ID cannot be found in
''' the image file.
'''</summary>
ERROR_RESOURCE_LANG_NOT_FOUND = 1815L

'''<summary>
''' Not enough quota is available
''' to process this command.
'''</summary>
ERROR_NOT_ENOUGH_QUOTA = 1816L

'''<summary>
''' 
'''</summary>
RPC_S_NO_INTERFACES = 1817L

'''<summary>
''' The server was altered while
''' processing this call.
'''</summary>
RPC_S_CALL_CANCELLED = 1818L

'''<summary>
''' The binding handle does not
''' contain all required
''' information.
'''</summary>
RPC_S_BINDING_INCOMPLETE = 1819L

'''<summary>
''' Communications failure.
'''</summary>
RPC_S_COMM_FAILURE = 1820L

'''<summary>
''' The requested authentication
''' level is not supported.
'''</summary>
RPC_S_UNSUPPORTED_AUTHN_LEVEL = 1821L

'''<summary>
''' No principal name registered.
'''</summary>
RPC_S_NO_PRINC_NAME = 1822L

'''<summary>
''' The error specified is not a
''' valid Windows RPC error code.
'''</summary>
RPC_S_NOT_RPC_ERROR = 1823L

'''<summary>
''' A UUID that is valid only on
''' this computer has been
''' allocated.
'''</summary>
RPC_S_UUID_LOCAL_ONLY = 1824L

'''<summary>
''' A security package specific
''' error occurred.
'''</summary>
RPC_S_SEC_PKG_ERROR = 1825L

'''<summary>
''' Thread is not cancelled.
'''</summary>
RPC_S_NOT_CANCELLED = 1826L

'''<summary>
''' Invalid operation on the
''' encoding/decoding handle.
'''</summary>
RPC_X_INVALID_ES_ACTION = 1827L

'''<summary>
''' Incompatible version of the
''' serializing package.
'''</summary>
RPC_X_WRONG_ES_VERSION = 1828L

'''<summary>
''' Incompatible version of the
''' RPC stub.
'''</summary>
RPC_X_WRONG_STUB_VERSION = 1829L

'''<summary>
''' The idl pipe object is invalid
''' or corrupted.
'''</summary>
RPC_X_INVALID_PIPE_OBJECT = 1830L

'''<summary>
''' The operation is invalid for a
''' given idl pipe object.
'''</summary>
RPC_X_INVALID_PIPE_OPERATION = 1831L

'''<summary>
''' The idl pipe version is not
''' supported.
'''</summary>
RPC_X_WRONG_PIPE_VERSION = 1832L

'''<summary>
''' The group member was not
''' found.
'''</summary>
RPC_S_GROUP_MEMBER_NOT_FOUND = 1898L

'''<summary>
''' The endpoint mapper database
''' could not be created.
'''</summary>
EPT_S_CANT_CREATE = 1899L

'''<summary>
''' The object universal unique
''' identifier (UUID) is the nil
''' UUID.
'''</summary>
RPC_S_INVALID_OBJECT = 1900L

'''<summary>
''' The specified time is invalid.
'''</summary>
ERROR_INVALID_TIME = 1901L

'''<summary>
''' The specified Form name is
''' invalid.
'''</summary>
ERROR_INVALID_FORM_NAME = 1902L

'''<summary>
''' The specified Form size is
''' invalid.
'''</summary>
ERROR_INVALID_FORM_SIZE = 1903L

'''<summary>
''' The specified Printer handle
''' is already being waited on.
'''</summary>
ERROR_ALREADY_WAITING = 1904L

'''<summary>
''' The specified Printer has been
''' deleted.
'''</summary>
ERROR_PRINTER_DELETED = 1905L

'''<summary>
''' The state of the Printer is
''' invalid.
'''</summary>
ERROR_INVALID_PRINTER_STATE = 1906L

'''<summary>
''' The user must change his
''' password before he logs on the
''' first time.
'''</summary>
ERROR_PASSWORD_MUST_CHANGE = 1907L

'''<summary>
''' Could not find the domain
''' controller for this domain.
'''</summary>
ERROR_DOMAIN_CONTROLLER_NOT_FOUND = 1908L

'''<summary>
''' The referenced account is
''' currently locked out and may
''' not be logged on to.
'''</summary>
ERROR_ACCOUNT_LOCKED_OUT = 1909L

'''<summary>
''' The object exporter specified
''' was not found.
'''</summary>
OR_INVALID_OXID = 1910L

'''<summary>
''' The object specified was not
''' found.
'''</summary>
OR_INVALID_OID = 1911L

'''<summary>
''' The object resolver set
''' specified was not found.
'''</summary>
OR_INVALID_SET = 1912L

'''<summary>
''' Some data remains to be sent
''' in the request buffer.
'''</summary>
RPC_S_SEND_INCOMPLETE = 1913L

'''<summary>
''' The pixel format is invalid.
'''</summary>
ERROR_INVALID_PIXEL_FORMAT = 2000L

'''<summary>
''' The specified driver is
''' invalid.
'''</summary>
ERROR_BAD_DRIVER = 2001L

'''<summary>
''' The window style or class
''' attribute is invalid for this
''' operation.
'''</summary>
ERROR_INVALID_WINDOW_STYLE = 2002L

'''<summary>
''' The requested metafile
''' operation is not supported.
'''</summary>
ERROR_METAFILE_NOT_SUPPORTED = 2003L

'''<summary>
''' The requested transformation
''' operation is not supported.
'''</summary>
ERROR_TRANSFORM_NOT_SUPPORTED = 2004L

'''<summary>
''' The requested clipping
''' operation is not supported.
'''</summary>
ERROR_CLIPPING_NOT_SUPPORTED = 2005L

'''<summary>
''' The network is not present or
''' not started.
'''</summary>
ERROR_NO_NETWORK = 2138L

'''<summary>
''' The specified user name is
''' invalid.
'''</summary>
ERROR_BAD_USERNAME = 2202L

'''<summary>
''' This network connection does
''' not exist.
'''</summary>
ERROR_NOT_CONNECTED = 2250L

'''<summary>
''' There are open files or
''' requests pending on this
''' connection.
'''</summary>
ERROR_OPEN_FILES = 2401L

'''<summary>
''' Active connections still
''' exist.
'''</summary>
ERROR_ACTIVE_CONNECTIONS = 2402L

'''<summary>
''' The device is in use by an
''' active process and cannot be
''' disconnected.
'''</summary>
ERROR_DEVICE_IN_USE = 2404L

'''<summary>
''' The specified print monitor is
''' unknown.
'''</summary>
ERROR_UNKNOWN_PRINT_MONITOR = 3000L

'''<summary>
''' The specified printer driver
''' is currently in use.
'''</summary>
ERROR_PRINTER_DRIVER_IN_USE = 3001L

'''<summary>
''' The spool file was not found.
'''</summary>
ERROR_SPOOL_FILE_NOT_FOUND = 3002L

'''<summary>
''' A StartDocPrinter call was not
''' issued.
'''</summary>
ERROR_SPL_NO_STARTDOC = 3003L

'''<summary>
''' An AddJob call was not issued.
'''</summary>
ERROR_SPL_NO_ADDJOB = 3004L

'''<summary>
''' The specified print
''' processor has already been
''' installed.
'''</summary>
ERROR_PRINT_PROCESSOR_ALREADY_INSTALLED = 3005L

'''<summary>
''' The specified print monitor
''' has already been installed.
'''</summary>
ERROR_PRINT_MONITOR_ALREADY_INSTALLED = 3006L

'''<summary>
''' The specified print monitor
''' does not have the required
''' functions.
'''</summary>
ERROR_INVALID_PRINT_MONITOR = 3007L

'''<summary>
''' The specified print monitor is
''' currently in use.
'''</summary>
ERROR_PRINT_MONITOR_IN_USE = 3008L

'''<summary>
''' The requested operation is not
''' allowed when there are jobs
''' queued to the printer.
'''</summary>
ERROR_PRINTER_HAS_JOBS_QUEUED = 3009L

'''<summary>
''' The requested operation is
''' successful. Changes will not
''' be effective until the system
''' is rebooted.
'''</summary>
ERROR_SUCCESS_REBOOT_REQUIRED = 3010L

'''<summary>
''' The requested operation is
''' successful. Changes will not
''' be effective until the service
''' is restarted.
'''</summary>
ERROR_SUCCESS_RESTART_REQUIRED = 3011L

'''<summary>
''' WINS encountered an error
''' while processing the command.
'''</summary>
ERROR_WINS_INTERNAL = 4000L

'''<summary>
''' The local WINS can not be
''' deleted.
'''</summary>
ERROR_CAN_NOT_DEL_LOCAL_WINS = 4001L

'''<summary>
''' The importation from the file
''' failed.
'''</summary>
ERROR_STATIC_INIT = 4002L

'''<summary>
''' The backup failed. Was a full
''' backup done before?
'''</summary>
ERROR_INC_BACKUP = 4003L

'''<summary>
''' The backup failed. Check the
''' directory that you are backing
''' the database to.
'''</summary>
ERROR_FULL_BACKUP = 4004L

'''<summary>
''' The name does not exist in the
''' WINS database.
'''</summary>
ERROR_REC_NON_EXISTENT = 4005L

'''<summary>
''' Replication with a non-
''' configured partner is not
''' allowed.
'''</summary>
ERROR_RPL_NOT_ALLOWED = 4006L

'''<summary>
''' The list of servers for this
''' workgroup is not currently
''' available.
'''</summary>
ERROR_NO_BROWSER_SERVERS_FOUND = 6118L

End Enum
