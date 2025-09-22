import re

REGION_P = re.compile(r'^--#region\s+(.*)')
END_P = re.compile(r'^--#endregion')
FUNC_P = re.compile(r'^function\s+([\w.]+)\s*\(([^)]*)\)')

COMMENT_P = re.compile(r'---\s*(?!@)(.*)')
PARAM_P = re.compile(
    r'---\s*@param\s+([\w?.]+)\s+([\w?.|]+(?:<[^<>]+>)?(?:\([^)]*\))?(?:\[\])*)(?:\s*(.*))?$')
DEFAULT_P = re.compile(r'---\s*@default\s+(.*)')
RETURN_P = re.compile(
    r'---\s*@return\s+([\w?.|]+(?:<[^<>]+>)?(?:\[\])*)\s*([^#\s]*)(?:\s*(?:#\s*)?(.*))?$')

CLASS_P = re.compile(r'---\s*@class\s+[\w.]+')
MODULE_P = re.compile(r'---\s*@module\s+"[\w.]+"')
FIELD_P = re.compile(r'^helpers\.(.+)\s+=')
