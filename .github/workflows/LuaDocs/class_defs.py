from abc import ABC, abstractmethod
from dataclasses import dataclass, field
import re
from regex_defs import MODULE_F_P, CLASS_F_P, TABLE_P


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

    @property
    def range(self):
        return range(self._start, self._end + 1)

    def reformat(self, lines: list[str]):
        lines[self._end] = self._format_last(lines[self._end])

    @abstractmethod
    def _format_last(self, last: str) -> str:
        pass

@dataclass(init=False)
class LocalRange(LineRange):
    def __init__(self, start: int):
        super().__init__(start, start + 1)

    def _format_last(self, last):
        return self.parser.match(last).group(1)

    @property
    @abstractmethod
    def parser(self) -> re.Pattern:
        pass


@dataclass(init=False)
class ModuleRange(LocalRange):
    @property
    def parser(self):
        return MODULE_F_P


@dataclass(init=False)
class ClassRange(LocalRange):
    @property
    def parser(self):
        return CLASS_F_P


@dataclass
class FuncRange(LineRange):
    def _format_last(self, last):
        return last + ' end'


@dataclass(init=False)
class FieldRange(LineRange):
    def __init__(self, start: int):
        super().__init__(start, start)

    def _format_last(self, last):
        return last
# endregion
