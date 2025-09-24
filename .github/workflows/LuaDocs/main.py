import sys
# pylint: disable=W0401,W0614
from class_defs import *
from regex_defs import *

TAB = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
DOCS_FILE = "Bosser-Helper-‐-Lua-Helper-Functions"
REPO_PATH: str
LUA_PATH: str


def build_meta_file(lines: list[str], list_ranges: list[LineRange], field_list: list[FieldName]):
    _meta_lines = (DocList("---@meta HelperFunctions")
                   .append_s("---@class HelperFunctions")
                   .append(f"{TABLE} = {{}}")
                   )

    _meta_lines.set_sep('')
    for meta_range in list_ranges:
        _meta_lines.append_s(meta_range.form_range(lines))

    _meta_lines.append_s().set_sep('\n')
    for helper_field in field_list:
        _meta_lines.append(helper_field.name)

    return _meta_lines


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


def parse_lua_file(orig_lines: list[str]):
    """
    Parses a Lua file to extract function names,
    parameters, return values, and documentation comments.
    """
    all_funcs: list[Function] = []
    all_regions: list[Region] = []
    all_fields: list[FieldName] = []
    all_meta_ranges: list[LineRange] = []

    current_region: Region | None = None

    lines: list[str] = list(map(str.strip, orig_lines))

    inside_func = False
    inside_table = False

    for i, line in enumerate(lines):
        if inside_func:
            if FUNC_END_P.match(orig_lines[i]):
                inside_func = False
            continue

        if inside_table:
            if TABLE_END_P.match(line):
                inside_table = False
            continue

        match line:
            case _ if (match := REGION_P.match(line)):
                if not any(_ in (region_name := match.group(1)) for _ in ("Import", "Local")):
                    current_region = Region(region_name)

            case _ if REGION_END_P.match(line):
                if current_region is not None:
                    all_regions.append(current_region)
                    current_region = None

            case _ if MODULE_P.match(line):
                all_meta_ranges.append(ModuleRange(i))

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

            case _ if LOCAL_TABLE_P.match(line) and '}' not in line:
                inside_table = True

            case _ if (match := FIELD_P.match(line)):
                all_fields.append(FieldName(match.group(1)))
                all_meta_ranges.append(FieldRange(i))

    return all_regions, all_funcs, all_meta_ranges, all_fields


def format_markdown_link(name: str):
    return re.sub(r'[^a-z0-9-]', '', name.replace(' ', '-').lower())


def name_link(name: str, pre_link: str = "", link: str = None):
    return f'[{name}]({pre_link}#{format_markdown_link(link or name)})'


def generate_markdown_documentation(region_list: list[Region], file_funcs: list[Function]):
    """
    Generates markdown documentation for a list of functions.
    """

    docs = (DocList(("This page contains all documentation for all Lua helper functions this mod" +
                     " provides for all attacks, events, and setup files required for a boss."))
            .append_s(f"[Find the actual Lua file here]({REPO_PATH}/{LUA_PATH})."))

    sidebar = DocList("[**Home**](Home)")

    for region in region_list:
        if "Original" in region.name:
            continue

        docs.append_s(f"## {region.name}")

        sidebar.append_s(f"## {name_link(region.name, DOCS_FILE)}\n")

        for func in region.functions:
            docs.append_s(f"### {func.full_name}\n\n{TAB}{func.description}")

            sidebar.append(f"- {name_link(func.full_name, DOCS_FILE)}")

            if func.params:
                for param in func.params:
                    desc = f"{TAB}`{param.name}` (`{param.type}`)"
                    if param.optional:
                        desc += (f" (default `{param.default}`)"
                                 if param.default else " (optional)")
                    docs.append_s(desc + "  ")

                    param_desc = param.description
                    if "helpers." in param_desc:
                        for function in [func for func in file_funcs if func.name in param_desc]:
                            param_desc = param_desc.replace(
                                function.name,
                                name_link(function.name, link=function.full_name),
                            )

                    if param_desc:
                        docs.append_s(f"{TAB}{TAB}{param_desc}  ")

            if func.returns:
                docs.append_s(f"{TAB}Returns:  ")
                for ret in func.returns:
                    name = f'`{ret.name}` ' if ret.name else ''
                    desc = f': {ret.description}' if ret.description else ''
                    docs.append_s(f"{TAB}{TAB}{name}(`{ret.type}`){desc}")

            docs.append_s("---")

    return docs, sidebar


def save_lines_to_file(lines, output_path, desc):
    with open(output_path, 'w', encoding='utf-8') as f:
        f.writelines(lines)
    print(f"{desc} saved to {f}")


if __name__ == '__main__':
    if len(sys.argv) != 3:
        print("File path is required for script.")
        sys.exit(1)

    REPO_PATH = sys.argv[1]
    LUA_PATH = f'{sys.argv[2]}/helper_functions.lua'

    with open(LUA_PATH, 'r', encoding='utf-8') as file:
        file_lines: list[str] = file.readlines()

    regions, files, meta_ranges, class_fields = parse_lua_file(file_lines)
    meta_lines = build_meta_file(file_lines, meta_ranges, class_fields)
    markdown, layout = generate_markdown_documentation(regions, files)

    save_lines_to_file(markdown.as_list(), f'docs/{DOCS_FILE}.md', "Documentation")
    save_lines_to_file(layout.as_list(), 'docs/_Sidebar.md', "Layout")
    save_lines_to_file(meta_lines.as_list(), 'meta/helper_functions_meta.lua', "Meta")
