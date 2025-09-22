import re
import sys
from class_defs import Function, FunctionParam, FunctionType, Region

TAB = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
DOCS_FILE = "Bosser-Helper-‐-Lua-Helper-Functions"
REPO_PATH: str
LUA_PATH: str

region_p = re.compile(r'^--#region\s+(.*)')
end_p = re.compile(r'^--#endregion')
func_p = re.compile(r'^function\s+([\w.]+)\s*\(([^)]*)\)')

comment_p = re.compile(r'---\s*(?!@)(.*)')
param_p = re.compile(
    r'---\s*@param\s+([\w?.]+)\s+([\w?.|]+(?:<[^<>]+>)?(?:\([^)]*\))?(?:\[\])*)(?:\s*(.*))?$')
default_p = re.compile(r'---\s*@default\s+(.*)')
return_p = re.compile(
    r'---\s*@return\s+([\w?.|]+(?:<[^<>]+>)?(?:\[\])*)\s*([^#\s]*)(?:\s*(?:#\s*)?(.*))?$')

class_p = re.compile(r'---\s*@class\s+[\w.]+')
module_p = re.compile(r'---\s*@module\s+"[\w.]+"')
local_p = re.compile(r'^local\s+\w+')

def parse_function(func_name: str, lines_subset: list[str]):
    """
    Parses the subset of lines to extract the function's
    documentation comments, parameters, and return values.
    """
    doc_lines: list[str] = []
    params: list[FunctionParam] = []
    returns: list[FunctionType] = []

    idx = -1
    while (idx := idx + 1) < len(lines_subset):
        match (line := lines_subset[idx]):
            case _ if (comment_m := comment_p.match(line)):
                doc_lines.append(comment_m.group(1))

            case _ if (param_m := param_p.match(line)):
                param_name, param_type, param_desc = param_m.groups()
                default = ""

                if (opt := param_name.endswith('?')):
                    param_name = param_name[:-1]

                    if (default_m := default_p.match(lines_subset[idx + 1])):
                        default: str = default_m.group(1)
                        idx += 1

                params.append(FunctionParam(param_type, param_name, param_desc, opt, default))

            case _ if (return_m := return_p.match(line)):
                returns.append(FunctionType(*return_m.groups()))

    return Function(func_name, '\n'.join(doc_lines), params, returns)


def get_annotations(lines: list[str], i: int):
    j = i
    while j > 0 and lines[j - 1].startswith('---'):
        j -= 1

    return lines[j:i+1], j


def parse_lua_file():
    """
    Parses a Lua file to extract function names,
    parameters, return values, and documentation comments.
    """
    all_funcs: list[Function] = []
    all_regions: list[Region] = []
    all_ranges: list[range] = []

    current_region: Region | None = None

    with open(LUA_PATH, 'r', encoding='utf-8') as file:
        lines: list[str] = list(map(str.strip, file.readlines()))

    for i, line in enumerate(lines):
        match line:
            case _ if ((match := region_p.match(line)) and
                       'Import' not in (region_name := match.group(1))):
                current_region = Region(region_name)

            case _ if end_p.match(line) and current_region is not None:
                all_regions.append(current_region)
                current_region = None

            case _ if (match := func_p.match(line)):
                annotations, start_idx = get_annotations(lines, i)
                all_ranges.append(range(start_idx, i + 1))
                new_function = parse_function(match.group(1), annotations)
                current_region.add(new_function)
                all_funcs.append(new_function)

    return all_regions, all_funcs


def format_markdown_link(name: str):
    return re.sub(r'[^a-z0-9-]', '', name.replace(' ', '-').lower())


def name_link(name: str, pre_link: str = "", link: str = None):
    return f'[{name}]({pre_link}#{format_markdown_link(link or name)})'


def generate_markdown_documentation(region_list: list[Region], file_funcs: list[Function]):
    """
    Generates markdown documentation for a list of functions.
    """

    docs = ("This page contains all documentation for all Lua helper functions this mod" +
            " provides for all attacks, events, and setup files required for a boss.\n\n" +
            f"[Find the actual Lua file here]({REPO_PATH}/{LUA_PATH}).\n")

    sidebar = "[**Home**](Home)\n"

    for region in region_list:
        if "Original" in region.name:
            continue

        docs += f"\n## {region.name}\n"

        sidebar += (f"\n## {name_link(region.name, DOCS_FILE)}\n\n")

        for func in region.functions:
            docs += f"\n### {func.full_name}\n\n{TAB}{func.description}\n"

            sidebar += (f"- {name_link(func.full_name, DOCS_FILE)}\n")

            if func.params:
                for param in func.params:
                    docs += f"\n{TAB}`{param.name}` (`{param.type}`)"
                    if param.optional:
                        docs += (f" (default `{param.default}`)"
                                          if param.default else " (optional)")
                    docs += "  \n"

                    param_desc = param.description
                    if "helpers." in param_desc:
                        for function in [func for func in file_funcs if func.name in param_desc]:
                            param_desc = param_desc.replace(
                                function.name,
                                name_link(function.name, link=function.full_name),
                                )

                    docs += f"\n{TAB}{TAB}{param_desc}  \n" if param_desc else ''

            if func.returns:
                docs += f"\n{TAB}Returns:  \n"
                for ret in func.returns:
                    name = f'`{ret.name}` ' if ret.name else ''
                    desc = f': {ret.description}' if ret.description else ''
                    docs += f"\n{TAB}{TAB}{name}(`{ret.type}`){desc}\n"

            docs += "\n---\n"

    return docs, sidebar


def save_markdown_to_file(markdown_text, output_path, desc):
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(markdown_text)
    print(f"{desc} saved to {f}")


if __name__ == '__main__':
    if len(sys.argv) != 3:
        print("File path is required for script.")
        sys.exit(1)

    REPO_PATH = sys.argv[1]
    LUA_PATH = f'{sys.argv[2]}/helper_functions.lua'

    markdown, layout = generate_markdown_documentation(*parse_lua_file())
    save_markdown_to_file(markdown, f'docs/{DOCS_FILE}.md', "Documentation")
    save_markdown_to_file(layout, 'docs/_Sidebar.md', "Layout")
