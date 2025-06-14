from dataclasses import dataclass, field


@dataclass
class FunctionType:
    type: str
    name: str
    description: str


@dataclass
class FunctionParam(FunctionType):
    optional: bool
    default: str


@dataclass
class Function:
    name: str
    full_name: str
    description: str
    params: list[FunctionParam]
    returns: list[FunctionType]

    def __init__(self, name: str, description: str, params: list[FunctionParam], returns: list[FunctionType]):
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
