from abc import ABC, abstractmethod
from dataclasses import dataclass, field
import re
from regex_defs import TABLE_P


# region Function Parsing
@dataclass
class FunctionType:
    type: str
    name: str
    description: str


@dataclass
class FunctionParam(FunctionType):
    optional: bool
    default: str


@dataclass(init=False)
class Function:
    name: str
    full_name: str
    description: str
    params: list[FunctionParam]
    returns: list[FunctionType]

    def __init__(self, name: str, description: str,
                 params: list[FunctionParam], returns: list[FunctionType]):
        self.name = name
        self.description = description
        self.params = params
        self.returns = returns

        first = True
        optional_params = 0
        self.full_name = self.name + " ("
        for param in self.params:
            if param.optional:
                optional_params += 1
                self.full_name += '['
            if not first:
                self.full_name += ", "
            self.full_name += param.name
            if param.optional and len(param.default) > 0:
                self.full_name += '=' + param.default
            first = False
        for _ in range(optional_params):
            self.full_name += ']'
        self.full_name += ')'


@dataclass
class Region:
    name: str
    functions: list[Function] = field(default_factory=list)

    def add(self, func: Function):
        self.functions.append(func)


# endregion

# region Meta Parsing
@dataclass(init=False)
class FieldName:
    full_name: str
    name: str

    def __init__(self, full_name: str):
        self.full_name = full_name
        self.name = TABLE_P.match(full_name).group(1)


@dataclass
class LineRange(ABC):
    _start: int
    _end: int

    def form_range(self, lines: list[str]):
        return lines[self._start: self._end] + [self._format_last(lines[self._end])]

    @abstractmethod
    def _format_last(self, last: str) -> str:
        pass


@dataclass(init=False)
class LocalRange(LineRange):
    _last_parser: re.Pattern

    def __init__(self, start: int, parse: re.Pattern):
        super().__init__(start, start + 1)
        self._last_parser = parse

    def _format_last(self, last):
        return self._last_parser.match(last).group(1) + '\n'


@dataclass
class FuncRange(LineRange):
    def _format_last(self, last):
        return last.rstrip() + ' end\n'


@dataclass(init=False)
class FieldRange(LineRange):
    def __init__(self, start: int):
        super().__init__(start, start)

    def _format_last(self, last):
        return last


# endregion

# region Others
@dataclass(init=False)
class DocList:
    _docs: list[str]
    _sep: str

    def __init__(self, docs: str | list[str] = None, sep: str = '\n'):
        self._sep = sep
        self._docs = []
        if docs:
            self.append(docs)

    def append(self, docs: str | list[str]):
        if isinstance(docs, list):
            for doc in docs:
                self._append(doc)
        else:
            self._append(docs)
        return self

    def _append(self, doc: str):
        self._docs.append(doc + self._sep)

    def append_s(self, docs: str | list[str] = None):
        self._docs.append('\n')
        if docs:
            self.append(docs)
        return self

    def as_list(self):
        return self._docs
# endregion
