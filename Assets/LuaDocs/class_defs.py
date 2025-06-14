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
    signature: str
    description: str
    params: list[FunctionParam]
    returns: list[FunctionType]

    @property
    def full_name(self):
        return self.name + ' ' + self.signature


@dataclass
class Region:
    name: str
    functions: list[Function] = field(default_factory=list)
