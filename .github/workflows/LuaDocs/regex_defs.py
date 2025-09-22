import re

LOCAL_R = r'^(local\s+\w+'
ANN_R = r'---\s*@'
TABLE = 'helpers'
TABLE_F = TABLE + r'\.'

REGION_P = re.compile(r'^--#region\s+(.*)')
END_P = re.compile(r'^--#endregion')
CLASS_P = re.compile(ANN_R + r'class\s+[\w.]+')
CLASS_F_P = re.compile(LOCAL_R + r'\s+=\s+{})')
MODULE_P = re.compile(ANN_R + r'module\s+"[\w.]+"')
MODULE_F_P = re.compile(LOCAL_R + r')\s+=\s+require')
FIELD_P = re.compile(r'^([\w.]+)\s+=')
FUNC_P = re.compile(r'^function\s+([\w.]+)\s*\(([^)]*)\)')

TYPE_R = r'\s+([\w?.|]+(?:<[^<>]+>)?(?:\([^)]*\))?(?:\[\])*)'

COMMENT_P = re.compile(r'---\s*(?!@)(.*)')
PARAM_P = re.compile(
    ANN_R + r'param\s+([\w?]+|\.\.\.)' + TYPE_R + r'(?:\s*(.*))?$')
DEFAULT_P = re.compile(ANN_R + r'default\s+(.*)')
RETURN_P = re.compile(
    ANN_R + r'return' + TYPE_R + r'\s*([^#\s]*)(?:\s*(?:#\s*)?(.*))?$')
