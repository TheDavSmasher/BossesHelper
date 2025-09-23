import sys
# pylint: disable=W0401,W0614
from class_defs import *
from regex_defs import *

TAB = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
DOCS_FILE = "Bosser-Helper-‐-Lua-Helper-Functions"
REPO_PATH: str
LUA_PATH: str


def build_meta_file(lines: list[str], meta_ranges: list[LineRange]):
    meta_lines: list[str] = []

    for meta_range in meta_ranges:
        meta_lines.extend(meta_range.form_range(lines))
        meta_lines.append("\n")

    return meta_lines


def parse_function(func_name: str, lines_subset: list[str]):
    """
    Parses the subset of lines to extract the function's
    documentation comments, parameters, and return values.
    """
    doc_lines: list[str] = []
    params: list[FunctionParam] = []
    returns: list[FunctionType] = []

    for idx, line in enumerate(lines_subset):
        match line:
            case _ if (match := COMMENT_P.match(line)):
                doc_lines.append(match.group(1))

            case _ if (match := PARAM_P.match(line)):
                param_name, param_type, param_desc = match.groups()
                default = ""

                if opt := param_name.endswith('?'):
                    param_name = param_name[:-1]

                    if default_m := DEFAULT_P.match(lines_subset[idx + 1]):
                        default: str = default_m.group(1)

                params.append(FunctionParam(param_type, param_name, param_desc, opt, default))

            case _ if (match := RETURN_P.match(line)):
                returns.append(FunctionType(*match.groups()))

    return Function(func_name, '\n'.join(doc_lines), params, returns)


def get_annotations(lines: list[str], i: int):
    j = i
    while j > 0 and lines[j - 1].startswith('---'):
        j -= 1

    return lines[j:i + 1], j


def parse_lua_file():
    """
    Parses a Lua file to extract function names,
    parameters, return values, and documentation comments.
    """
    all_funcs: list[Function] = []
    all_regions: list[Region] = []
    all_fields: list[FieldName] = []
    all_meta_ranges: list[LineRange] = []

    current_region: Region | None = None

    with open(LUA_PATH, 'r', encoding='utf-8') as file:
        orig_lines: list[str] = file.readlines()
        lines: list[str] = list(map(str.strip, orig_lines))

    inside_func = False

    for i, line in enumerate(lines):
        if inside_func:
            if FUNC_END_P.match(orig_lines[i]):
                inside_func = False
            continue

        match line:
            case _ if (match := REGION_P.match(line)):
                if not any(_ in (region_name := match.group(1)) for _ in ("Import", "Local")):
                    current_region = Region(region_name)

            case _ if REGION_END_P.match(line):
                if current_region is not None:
                    all_regions.append(current_region)
                    current_region = None

            case _ if CLASS_P.match(line):
                all_meta_ranges.append(LocalRange(i, CLASS_F_P))

            case _ if MODULE_P.match(line):
                all_meta_ranges.append(LocalRange(i, MODULE_F_P))

            case _ if (match := FUNC_P.match(line)):
                annotations, start_idx = get_annotations(lines, i)
                new_func = parse_function(match.group(1), annotations)
                current_region.add(new_func)
                all_funcs.append(new_func)
                all_fields.append(FieldName(new_func.name))
                all_meta_ranges.append(FuncRange(start_idx, i))
                inside_func = True

            case _ if LOCAL_FUNC_P.match(line):
                inside_func = True

            case _ if (match := FIELD_P.match(line)):
                all_fields.append(FieldName(match.group(1)))
                all_meta_ranges.append(FieldRange(i))

    meta_lines = build_meta_file(orig_lines, all_meta_ranges)

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

        sidebar += f"\n## {name_link(region.name, DOCS_FILE)}\n\n"

        for func in region.functions:
            docs += f"\n### {func.full_name}\n\n{TAB}{func.description}\n"

            sidebar += f"- {name_link(func.full_name, DOCS_FILE)}\n"

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

    regions, files = parse_lua_file()
    markdown, layout = generate_markdown_documentation(regions, files)
    save_markdown_to_file(markdown, f'docs/{DOCS_FILE}.md', "Documentation")
    save_markdown_to_file(layout, 'docs/_Sidebar.md', "Layout")
