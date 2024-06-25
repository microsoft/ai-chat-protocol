from dataclasses import dataclass, field, fields, is_dataclass
from typing import List, Optional, Dict, Any, Type, TypeVar, Union, get_args, get_origin, get_type_hints
from enum import Enum

T = TypeVar('T')

def rename(orignal_name: str, target_name: str):
    def wrapper(cls: Type[T]) -> Type[T]:
        renames = getattr(cls, "__renames__", {})
        renames[orignal_name] = target_name
        setattr(cls, "__renames__", renames)
        return cls
    return wrapper

def serializable(cls: Type[T]) -> Type[T]:
    if not is_dataclass(cls):
        raise TypeError("serializable decorator can only be used with dataclasses")

    def __to_dict_internal(obj):
        result = {}
        for f in fields(obj):
            value = getattr(obj, f.name)
            field_name = cls.__get_renamed_field_name(f.name)
            if isinstance(value, Enum):
                result[field_name] = value.value
            elif is_dataclass(value):
                result[field_name] = value.to_dict(value)
            elif isinstance(value, list):
                result[field_name] = [__to_dict_internal(v) for v in value]
            else:
                result[field_name] = value
        return result

    def to_dict(self) -> dict:
        return __to_dict_internal(self)

    def __get_value_internal__(value, field_type):
        origin = get_origin(field_type)
        args = get_args(field_type)
        if origin == dict:
            key_type, value_type = args
            return { __get_value_internal__(key, key_type): __get_value_internal__(value, value_type) for key, value in value.items() }
        elif origin == list:
            item_type = args[0]
            return [__get_value_internal__(item, item_type) for item in value]
        elif issubclass(field_type, Enum):
            return field_type(value)
        elif is_dataclass(field_type):
            return field_type.from_dict(value)
        return value

    @classmethod
    def __get_renamed_field_name(cls, name):
        return getattr(cls, "__renames__", {}).get(name, name)

    @classmethod
    def from_dict(cls, data: dict) -> T:
        field_types = get_type_hints(cls)
        updated_data = {}
        for field_name, field_type in field_types.items():
            serialized_name = cls.__get_renamed_field_name(field_name)
            if serialized_name in data and data[serialized_name] is not None:
                value = data[serialized_name]
                origin = get_origin(field_type)
                args = get_args(field_type)
                if origin == Union and type(None) in args:
                    optional_type = [t for t in args if t is not type(None)][0]
                    field_type = optional_type
                updated_data[field_name] = __get_value_internal__(value, field_type)
        print(updated_data)
        return cls(**updated_data)

    setattr(cls, "to_dict", to_dict)
    setattr(cls, "from_dict", from_dict)
    setattr(cls, "__get_renamed_field_name", __get_renamed_field_name)

    return cls

class AIChatRole(Enum):
    USER = "user"
    ASSISTANT = "assistant"
    SYSTEM = "system"

@serializable
@dataclass
@rename("content_type", "contentType")
class AIChatFile:
    content_type: str
    data: bytes

@serializable
@dataclass
class AIChatMessage:
    role: AIChatRole # = RestFieldDescriptor[AIChatRole](name="role", type=AIChatRole)
    content: str # = RestFieldDescriptor[str](name="content", type=str)
    context: Optional[Dict[str, Any]] = None # = RestFieldDescriptor[Optional[Dict[str, Any]]](name="context", type=Optional[Dict[str, Any]])
    files: Optional[List[AIChatFile]] = None #= RestFieldDescriptor[Optional[List[AIChatFile]]](name="files", type=Optional[List[AIChatFile]])


a = AIChatFile(content_type="image/png", data=b"image data")

# print(a.to_dict())
# ra = AIChatFile.from_dict(a.to_dict())
# print(ra)


m1 = AIChatMessage(role=AIChatRole.USER, content="Hello", context={"key": "value"}, files=[a])
print(m1.to_dict())
rm1 = AIChatMessage.from_dict(m1.to_dict())
print(rm1)

# m2 = AIChatMessage(role=AIChatRole.USER, content="Hello", context={"key": "value"})
# print(m2.to_dict())
# rm2 = AIChatMessage.from_dict(m2.to_dict())
# print(rm2)

# m3 = AIChatMessage(role=AIChatRole.USER, content="Hello")
# print(m3.to_dict())
# rm3 = AIChatMessage.from_dict(m3.to_dict())
# print(rm3)

# @dataclass
# class AIChatMessageDelta:
#     role: Optional[AIChatRole] = None
#     content: Optional[str] = None
#     context: Optional[Dict[str, Any]] = None

# @dataclass
# class AIChatCompletion:
#     message: AIChatMessage
#     sessionState: Optional[Any] = None
#     context: Optional[Dict[str, Any]] = None

# @dataclass
# class AIChatCompletionDelta:
#     delta: AIChatMessageDelta
#     sessionState: Optional[Any] = None
#     context: Optional[Dict[str, Any]] = None

# @dataclass
# class AIChatCompletionOptions:
#     context: Optional[Dict[str, Any]] = None
#     sessionState: Optional[Any] = None

# @dataclass
# class AIChatError:
#     code: str
#     message: str

# @dataclass
# class AIChatErrorResponse:
#     error: AIChatError

# @dataclass
# class AIChatRequest:
#     messages: List[AIChatMessage]
#     sessionState: Optional[Any] = None
#     context: Optional[bytes] = None