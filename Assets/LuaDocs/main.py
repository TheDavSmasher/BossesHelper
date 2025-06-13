# This is a sample Python script.

# Press Shift+F10 to execute it or replace it with your code.
# Press Double Shift to search everywhere for classes, files, tool windows, actions, and settings.

import re

TAB = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"

all_functions = []


def parse_lua_file(lua_path):
    """
    Parses a Lua file to extract function names, parameters, return values, and documentation comments.
    """
    regions = []

    current_region = None

    with open(lua_path, 'r') as file:
        lines = file.readlines()

    region_pattern = re.compile(r'--#region\s+(.*)')
    end_pattern = re.compile(r'--#endregion+(.*)')
    func_pattern = re.compile(r'function\s+([a-zA-Z0-9_]+(?:\.[a-zA-Z0-9_]+)*)\s*\(([^)]*)\)')
    param_pattern = re.compile(r'---\s*@param\s+([a-zA-Z0-9_?.]+)\s+([a-zA-Z0-9_?.|]+(?:\([^)]*\))?)\s+(.*)')
    default_pattern = re.compile(r'---\s*@default\s+(.*)')
    return_pattern = re.compile(r'---\s*@return\s+([a-zA-Z0-9_|]+)\s+([a-zA-Z0-9_]+)\s+(.*)')

    skipping = False

    # Iterate over the lines of the Lua file
    for i, line in enumerate(lines):
        line = line.strip()

        region_match = region_pattern.match(line)
        if region_match:
            if region_match.group(1).startswith('Original'):
                skipping = True
            if not (region_match.group(1).startswith('Type') or region_match.group(1).__contains__('Helper')):
                current_region = {
                    'region': region_match.group(1),
                    'functions': []
                }
            continue

        end_match = end_pattern.match(line)
        if end_match:
            skipping = False
            if current_region is not None:
                regions.append(current_region)
                current_region = None
            continue

        if skipping:
            continue

        # Match function definitions (including object methods)
        func_match = func_pattern.match(line)
        if func_match:
            # Initialize function data
            function_name = func_match.group(1)

            # Prepare for backward pass to find documentation
            doc_lines = []  # Temporary list to store description lines
            params = []  # Temporary list to store parameters
            returns = []  # Temporary list to store return values
            optional_params = 0

            # Backtrack to find the first --- line for the function's documentation
            j = i - 1
            while j >= 0 and lines[j].startswith('---'):
                j -= 1  # Only update j during backtracking, no data collection yet

            # Now, start moving forward from the current value of j (after backtracking)
            # Loop until we reach the function's position (i)
            while j < i:
                j += 1  # Move forward

                line = lines[j].strip()

                # Collect documentation lines (description)
                if line.startswith('---') and not line[3:].strip().startswith('@'):
                    doc_line = line[3:].strip()  # Remove "---" prefix
                    doc_lines.append(doc_line)
                    continue

                # Collect @param annotations
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
                        {'name': param_name, 'type': param_type, 'optional': optional, 'default': param_default,
                         'description': param_desc})
                    continue

                # Collect @return annotations
                return_match = return_pattern.match(line)
                if return_match:
                    return_type = return_match.group(1)
                    return_name = return_match.group(2)
                    return_desc = return_match.group(3)
                    returns.append({'name': return_name, 'type': return_type, 'description': return_desc})

            # Get the function's signature
            first = True
            function_sig = "("
            for param in params:
                optional = param['optional']
                if optional:
                    function_sig += '['
                if not first:
                    function_sig += ", "
                function_sig += param['name']
                if optional and len(param['default']) > 0:
                    function_sig += '=' + param['default']
                first = False
            for i in range(optional_params):
                function_sig += ']'
            function_sig += ')'

            # Add the function to the list with its description, parameters, and return values
            current_region['functions'].append({
                'name': function_name,
                'signature': function_sig,
                'full_name': function_name + ' ' + function_sig,
                'params': params,
                'returns': returns,
                'description': '\n'.join(doc_lines)  # Join description lines
            })

            all_functions.append({'name': function_name, 'full_name': function_name + ' ' + function_sig})

    return regions


def format_markdown_link(name):
    return re.sub(r'[^a-z0-9-]', '', name.replace(' ', '-').lower())


def generate_markdown_documentation(regions):
    """
    Generates markdown documentation for a list of functions.
    """
    markdown_text = "# [Bosses Helper](README.md): Lua Helper Functions\n"

    layout_markdown = "# [Bosses Helper](README.md): [Lua Helper Functions](boss_helper_functions.md#bosses-helper-lua-helper-functions) Layout\n"

    markdown_text += "\n## [Document Layout](boss_helper_functions_layout.md#bosses-helper-lua-helper-functions-layout)\n\n[Find the actual Lua file here](Assets/LuaBossHelper/helper_functions.lua).\n"

    for reg in regions:
        region_name = reg['region']
        markdown_text += f"\n## {region_name}\n"

        layout_markdown += f"\n## [{region_name}](boss_helper_functions.md#{format_markdown_link(region_name)})\n\n"

        for func in reg['functions']:
            full_name = func['full_name']

            layout_markdown += f"- [{full_name}](boss_helper_functions.md#{format_markdown_link(full_name)})\n"

            markdown_text += f"\n### {full_name}\n\n{TAB}{func['description']}\n"

            if func['params']:
                for param in func['params']:
                    markdown_text += f"\n{TAB}`{param['name']}` (`{param['type']}`)"
                    if param['optional']:
                        if param['default']:
                            markdown_text += f" (default `{param['default']}`)"
                        else:
                            markdown_text += f" (optional)"
                    markdown_text += f"  \n\n"

                    param_description = param['description']

                    if param_description.__contains__("helpers."):
                        for function in all_functions:
                            func_name = function['name']
                            if param_description.__contains__(func_name):
                                param_description = param_description.replace(function['name'],
                                                                              f"[{function['name']}](#{format_markdown_link(function['full_name'])})")
                                break

                    markdown_text += f"{TAB}{TAB}{param_description}  \n"

            if func['returns']:
                markdown_text += f"\n{TAB}Returns:  \n"
                for ret in func['returns']:
                    markdown_text += f"\n{TAB}{TAB}`{ret['name']}` (`{ret['type']}`): {ret['description']}\n"

            markdown_text += "\n---\n"

    return markdown_text, layout_markdown


def save_markdown_to_file(markdown_text, output_path):
    with open(output_path, 'w') as f:
        f.write(markdown_text)


# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    lua_file_path = "../LuaBossHelper/helper_functions.lua"
    output_file_path = "../../boss_helper_functions.md"
    layout_file_path = "../../boss_helper_functions_layout.md"

    functions = parse_lua_file(lua_file_path)
    markdown, layout = generate_markdown_documentation(functions)
    save_markdown_to_file(markdown, output_file_path)
    save_markdown_to_file(layout, layout_file_path)

    print(f"Documentation saved to {output_file_path}")

# See PyCharm help at https://www.jetbrains.com/help/pycharm/
