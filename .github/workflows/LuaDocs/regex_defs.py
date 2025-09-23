import re

LOCAL_R = r'^(local\s+\w+'
ANN_R = r'---\s*@'
TABLE = 'helpers'

TABLE_P = re.compile(fr'{TABLE}\.(\w+)\b')
FIELD_R = r'([\w.]+)\s*'

REGION_P = re.compile(r'^--#region\s+(.*)')
REGION_END_P = re.compile(r'^--#endregion')
MODULE_P = re.compile(fr'{ANN_R}module\s+"[\w.]+"')
MODULE_F_P = re.compile(fr'{LOCAL_R})\s+=\s+require')
FIELD_P = re.compile(fr'^{FIELD_R}=')
FUNC_P = re.compile(fr'^function\s+{FIELD_R}\(([^)]*)\)')
LOCAL_FUNC_P = re.compile(r'^local\s+function')
FUNC_END_P = re.compile(r'^end$')

TYPE_R = r'([\w?.|]+(?:<[^<>]+>)?(?:\([^)]*\))?(?:\[\])*)'

COMMENT_P = re.compile(r'---\s*(?![^@]*@)(.+)')
PARAM_P = re.compile(fr'{ANN_R}param\s+([\w?]+|\.\.\.)\s+{TYPE_R}(?:\s*(.*))?$')
DEFAULT_P = re.compile(fr'{ANN_R}default\s+(.*)')
RETURN_P = re.compile(fr'{ANN_R}return\s+{TYPE_R}\s*([^#\s]*)(?:\s*(?:#\s*)?(.*))?$')
