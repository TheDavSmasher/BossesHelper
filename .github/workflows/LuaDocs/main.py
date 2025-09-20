import re
from class_defs import Function, FunctionParam, FunctionType, Region
import sys

TAB = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"


def parse_lua_file(lua_path):
    """
    Parses a Lua file to extract function names,
    parameters, return values, and documentation comments.
    """
    all_funcs: list[Function] = []
    all_regions: list[Region] = []

    current_region: Region | None = None

    with open(lua_path, 'r', encoding='utf-8') as file:
        lines: list[str] = list(map(str.strip, file.readlines()))

    region_pattern = re.compile(r'--#region\s+(.*)')
    end_pattern = re.compile(r'--#endregion+(.*)')
    func_pattern = re.compile(r'function\s+([a-zA-Z0-9_]+(?:\.[a-zA-Z0-9_]+)*)\s*\(([^)]*)\)')
    comment_pattern = re.compile(r'(?:---\s?)+((?<!@)[^@]*)$')
    param_pattern = re.compile(
        r'---\s*@param\s+([a-zA-Z0-9_?.]+)\s+([a-zA-Z0-9_?.|]+(?:\([^)]*\))?)\s+(.*)')
    default_pattern = re.compile(r'---\s*@default\s+(.*)')
    return_pattern = re.compile(r'---\s*@return\s+([a-zA-Z0-9_|]+)\s+([a-zA-Z0-9_]+)\s+(.*)')

    skipping = False

    for i, line in enumerate(lines):
        region_match = region_pattern.match(line)
        if region_match:
            region_name = region_match.group(1)
            if region_name.startswith('Original'):
                skipping = True
            if not ('Import' in region_name or 'Helper' in region_name):
                current_region = Region(region_name)
            continue

        if end_pattern.match(line):
            skipping = False
            if current_region is not None:
                all_regions.append(current_region)
                current_region = None
            continue

        if skipping:
            continue

        func_match = func_pattern.match(line)
        if func_match is None:
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

            comment_match = comment_pattern.match(line)
            if comment_match:
                doc_lines.append(comment_match.group(1))
                continue

            param_match = param_pattern.match(line)
            if param_match:
                param_name, param_type, param_desc = param_match.groups()
                param_default = ""
                optional = param_name.endswith('?')
                if optional:
                    param_name = param_name[:-1]

                    default_match = default_pattern.match(lines[j + 1])
                    if default_match:
                        param_default = default_match.group(1)
                        j += 1

                params.append(
                    FunctionParam(param_type, param_name, param_desc, optional, param_default))
                continue

            return_match = return_pattern.match(line)
            if return_match:
                returns.append(
                    FunctionType(*return_match.groups()))

        new_function = Function(func_match.group(1), '\n'.join(doc_lines), params, returns)
        current_region.functions.append(new_function)

        all_funcs.append(new_function)

    return all_regions, all_funcs


def format_markdown_link(name):
    return re.sub(r'[^a-z0-9-]', '', name.replace(' ', '-').lower())


def generate_markdown_documentation(region_list: list[Region], file_funcs: list[Function]):
    """
    Generates markdown documentation for a list of functions.
    """
    markdown_text = ("# [Bosses Helper](README.md): Lua Helper Functions\n\n" +
                     "## [Document Layout](boss_helper_functions_layout.md#bosses-helper-lua-helper-functions-layout)\n\n" +
                     "[Find the actual Lua file here](Assets/LuaBossHelper/helper_functions.lua).\n")

    layout_markdown = ("# [Bosses Helper](README.md): [Lua Helper Functions]"
                       + "(boss_helper_functions.md#bosses-helper-lua-helper-functions) Layout\n")

    for region in region_list:
        markdown_text += f"\n## {region.name}\n"

        layout_markdown += (f"\n## [{region.name}](boss_helper_functions.md"
                            + f"#{format_markdown_link(region.name)})\n\n")

        for func in region.functions:
            markdown_text += f"\n### {func.full_name}\n\n{TAB}{func.description}\n"

            layout_markdown += (f"- [{func.full_name}](boss_helper_functions.md"
                                + f"#{format_markdown_link(func.full_name)})\n")

            if func.params:
                for param in func.params:
                    markdown_text += f"\n{TAB}`{param.name}` (`{param.type}`)"
                    if param.optional:
                        markdown_text += (f" (default `{param.default}`)"
                                          if param.default else " (optional)")
                    markdown_text += "  \n\n"

                    param_desc = param.description
                    if "helpers." in param_desc:
                        for function in [func for func in file_funcs if func.name in param_desc]:
                            param_desc = param_desc.replace(function.name,
                                                            f"[{function.name}](#{format_markdown_link(function.full_name)})")

                    markdown_text += f"{TAB}{TAB}{param_desc}  \n"

            if func.returns:
                markdown_text += f"\n{TAB}Returns:  \n"
                for ret in func.returns:
                    markdown_text += f"\n{TAB}{TAB}`{ret.name}` (`{ret.type}`): {ret.description}\n"

            markdown_text += "\n---\n"

    return markdown_text, layout_markdown


def save_markdown_to_file(markdown_text, output_path, desc):
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(markdown_text)
    print(f"{desc} saved to {f}")


if __name__ == '__main__':
    if len(sys.argv) != 2:
        print("File path is required for script.")
        sys.exit(1)

    LUA_PATH = f'{sys.argv[1]}/helper_functions.md'
    markdown, layout = generate_markdown_documentation(*parse_lua_file(LUA_PATH))
    save_markdown_to_file(markdown, 'docs/Boss-Lua-Helper-Functions.md', "Documentation")
    save_markdown_to_file(layout, 'docs/_Sidebar.md', "Layout")
