import re
from class_defs import *

TAB = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"


def parse_lua_file(lua_path):
    """
    Parses a Lua file to extract function names, parameters, return values, and documentation comments.
    """
    all_funcs: list[Function] = []
    all_regions: list[Region] = []

    current_region: Region | None = None

    with open(lua_path, 'r') as file:
        lines: list[str] = list(map(str.strip, file.readlines()))

    region_pattern = re.compile(r'--#region\s+(.*)')
    end_pattern = re.compile(r'--#endregion+(.*)')
    func_pattern = re.compile(r'function\s+([a-zA-Z0-9_]+(?:\.[a-zA-Z0-9_]+)*)\s*\(([^)]*)\)')
    comment_pattern = re.compile(r'(?:---\s?)+((?<!@)[^@]*)$')
    param_pattern = re.compile(r'---\s*@param\s+([a-zA-Z0-9_?.]+)\s+([a-zA-Z0-9_?.|]+(?:\([^)]*\))?)\s+(.*)')
    default_pattern = re.compile(r'---\s*@default\s+(.*)')
    return_pattern = re.compile(r'---\s*@return\s+([a-zA-Z0-9_|]+)\s+([a-zA-Z0-9_]+)\s+(.*)')

    skipping = False

    for i, line in enumerate(lines):
        region_match = region_pattern.match(line)
        if region_match:
            region_name = region_match.group(1)
            if region_name.startswith('Original'):
                skipping = True
            if not (region_name.startswith('Type') or region_name.__contains__('Helper')):
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
        optional_params = 0

        j = i - 1
        while j >= 0 and lines[j].startswith('---'):
            j -= 1

        while j < i:
            j += 1
            line = lines[j]

            comment_match = comment_pattern.match(line)
            if comment_match:
                doc_line = comment_match.group(1)
                doc_lines.append(doc_line)
                continue

            param_match = param_pattern.match(line)
            if param_match:
                param_name = param_match.group(1)
                param_type = param_match.group(2)
                param_desc = param_match.group(3)
                param_default = ""
                optional = param_name.endswith('?')
                if optional:
                    optional_params += 1
                    param_name = param_name[:-1]

                    default_match = default_pattern.match(lines[j + 1].strip())
                    if default_match:
                        param_default = default_match.group(1)
                        j += 1

                params.append(
                    FunctionParam(param_name, param_type, param_desc, optional, param_default))
                continue

            return_match = return_pattern.match(line)
            if return_match:
                return_type = return_match.group(1)
                return_name = return_match.group(2)
                return_desc = return_match.group(3)
                returns.append(
                    FunctionType(return_name, return_type, return_desc))

        first = True
        function_sig = "("
        for param in params:
            optional = param.optional
            if optional:
                function_sig += '['
            if not first:
                function_sig += ", "
            function_sig += param.name
            if optional and len(param.default) > 0:
                function_sig += '=' + param.default
            first = False
        for _ in range(optional_params):
            function_sig += ']'
        function_sig += ')'

        new_function = Function(func_match.group(1), function_sig, '\n'.join(doc_lines), params, returns)
        current_region.functions.append(new_function)

        all_funcs.append(new_function)

    return all_regions, all_funcs


def format_markdown_link(name):
    return re.sub(r'[^a-z0-9-]', '', name.replace(' ', '-').lower())


def generate_markdown_documentation(region_list: list[Region]):
    """
    Generates markdown documentation for a list of functions.
    """
    markdown_text = "# [Bosses Helper](README.md): Lua Helper Functions\n"

    layout_markdown = "# [Bosses Helper](README.md): [Lua Helper Functions](boss_helper_functions.md#bosses-helper-lua-helper-functions) Layout\n"

    markdown_text += "\n## [Document Layout](boss_helper_functions_layout.md#bosses-helper-lua-helper-functions-layout)\n\n[Find the actual Lua file here](Assets/LuaBossHelper/helper_functions.lua).\n"

    for reg in region_list:
        region_name = reg.name
        markdown_text += f"\n## {region_name}\n"

        layout_markdown += f"\n## [{region_name}](boss_helper_functions.md#{format_markdown_link(region_name)})\n\n"

        for func in reg.functions:
            full_name = func.full_name

            layout_markdown += f"- [{full_name}](boss_helper_functions.md#{format_markdown_link(full_name)})\n"

            markdown_text += f"\n### {full_name}\n\n{TAB}{func.description}\n"

            if func.params:
                for param in func.params:
                    markdown_text += f"\n{TAB}`{param.name}` (`{param.type}`)"
                    if param.optional:
                        if param.default:
                            markdown_text += f" (default `{param.default}`)"
                        else:
                            markdown_text += f" (optional)"
                    markdown_text += f"  \n\n"

                    param_description = param.description

                    if param_description.__contains__("helpers."):
                        for function in all_functions:
                            func_name = function.name
                            if param_description.__contains__(func_name):
                                param_description = param_description.replace(function.name,
                                                                              f"[{function.name}](#{format_markdown_link(function.full_name)})")
                                break

                    markdown_text += f"{TAB}{TAB}{param_description}  \n"

            if func.returns:
                markdown_text += f"\n{TAB}Returns:  \n"
                for ret in func.returns:
                    markdown_text += f"\n{TAB}{TAB}`{ret.name}` (`{ret.type}`): {ret.description}\n"

            markdown_text += "\n---\n"

    return markdown_text, layout_markdown


def save_markdown_to_file(markdown_text, output_path):
    with open(output_path, 'w') as f:
        f.write(markdown_text)


if __name__ == '__main__':
    lua_file_path = "../LuaBossHelper/helper_functions.lua"
    output_file_path = "../../boss_helper_functions.md"
    layout_file_path = "../../boss_helper_functions_layout.md"

    regions, all_functions = parse_lua_file(lua_file_path)
    markdown, layout = generate_markdown_documentation(regions)
    save_markdown_to_file(markdown, output_file_path)
    save_markdown_to_file(layout, layout_file_path)

    print(f"Documentation saved to {output_file_path}")
