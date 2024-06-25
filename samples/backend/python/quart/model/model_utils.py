from dataclasses import fields, is_dataclass
from typing import Type, TypeVar, Union, get_args, get_origin, get_type_hints
from enum import Enum

T = TypeVar("T")


def rename(original_name: str, target_name: str):
    def wrapper(cls: Type[T]) -> Type[T]:
        renames = getattr(cls, "__renames__", {})
        renames[original_name] = target_name
        setattr(cls, "__renames__", renames)
        return cls

    return wrapper


def serializable(cls: Type[T]) -> Type[T]:
    if not is_dataclass(cls):
        raise TypeError("serializable decorator can only be used with dataclasses")

    def __value_to_dict(value):
        if isinstance(value, Enum):
            return value.value
        elif is_dataclass(value):
            return value.to_dict()
        elif isinstance(value, list):
            return [__value_to_dict(v) for v in value]
        elif isinstance(value, dict):
            return {__value_to_dict(k): __value_to_dict(v) for k, v in value.items()}
        return value

    def to_dict(self) -> dict:
        result = {}
        for f in fields(self):
            value = getattr(self, f.name)
            field_name = cls.__get_renamed_field_name(f.name)
            result[field_name] = __value_to_dict(value)
        return result

    def __get_value_internal__(value, field_type):
        origin = get_origin(field_type)
        args = get_args(field_type)
        if origin == dict:
            key_type, value_type = args
            return {
                __get_value_internal__(key, key_type): __get_value_internal__(
                    value, value_type
                )
                for key, value in value.items()
            }
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
        return cls(**updated_data)

    setattr(cls, "to_dict", to_dict)
    setattr(cls, "from_dict", from_dict)
    setattr(cls, "__get_renamed_field_name", __get_renamed_field_name)

    return cls
