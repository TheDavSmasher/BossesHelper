import re
import sys
from class_defs import Function, FunctionParam, FunctionType, Region

TAB = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
DOCS_FILE = "Bosser-Helper-‐-Lua-Helper-Functions"
REPO_PATH: str
LUA_PATH: str


def parse_lua_file():
    """
    Parses a Lua file to extract function names,
    parameters, return values, and documentation comments.
    """
    all_funcs: list[Function] = []
    all_regions: list[Region] = []

    current_region: Region | None = None

    with open(LUA_PATH, 'r', encoding='utf-8') as file:
        lines: list[str] = list(map(str.strip, file.readlines()))

    region_pattern = re.compile(r'^--#region\s+(.*)')
    end_pattern = re.compile(r'^--#endregion$')
    func_pattern = re.compile(r'^function\s+([a-zA-Z0-9_]+(?:\.[a-zA-Z0-9_]+)*)\s*\(([^)]*)\)')

    comment_pattern = re.compile(r'(?:---\s?)+((?<!@)[^@]*)$')
    param_pattern = re.compile(
        r'---\s*@param\s+([a-zA-Z0-9_?.]+)\s+([a-zA-Z0-9_?.|]+(?:\([^)]*\))?)\s+(.*)')
    default_pattern = re.compile(r'---\s*@default\s+(.*)')
    return_pattern = re.compile(r'---\s*@return\s+([a-zA-Z0-9_|]+)\s+([a-zA-Z0-9_]+)\s+(.*)')

    for i, line in enumerate(lines):
        if (region_match := region_pattern.match(line)):
            if 'Import' not in (region_name := region_match.group(1)):
                current_region = Region(region_name)
            continue

        if end_pattern.match(line):
            if current_region is not None:
                all_regions.append(current_region)
                current_region = None
            continue

        if (func_match := func_pattern.match(line)) is None:
            continue

        doc_lines: list[str] = []
        params: list[FunctionParam] = []
        returns: list[FunctionType] = []

        j = i - 1
        while j >= 0 and lines[j].startswith('---'):
            j -= 1

        while j < i:
            j += 1
            line = lines[j]

            if (comment_match := comment_pattern.match(line)):
                doc_lines.append(comment_match.group(1))
                continue

            if (param_match := param_pattern.match(line)):
                param_name, param_type, param_desc = param_match.groups()

                if (opt := param_name.endswith('?')):
                    param_name = param_name[:-1]

                    if (default_match := default_pattern.match(lines[j + 1])):
                        param_default: str = default_match.group(1)
                        j += 1

                params.append(
                    FunctionParam(param_type, param_name, param_desc, opt, param_default or ""))
                continue

            if (return_match := return_pattern.match(line)):
                returns.append(
                    FunctionType(*return_match.groups()))

        new_function = Function(func_match.group(1), '\n'.join(doc_lines), params, returns)
        current_region.functions.append(new_function)

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
                    docs += "  \n\n"

                    param_desc = param.description
                    if "helpers." in param_desc:
                        for function in [func for func in file_funcs if func.name in param_desc]:
                            param_desc = param_desc.replace(
                                function.name,
                                name_link(function.name, link=function.full_name),
                                )

                    docs += f"{TAB}{TAB}{param_desc}  \n"

            if func.returns:
                docs += f"\n{TAB}Returns:  \n"
                for ret in func.returns:
                    docs += f"\n{TAB}{TAB}`{ret.name}` (`{ret.type}`): {ret.description}\n"

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
