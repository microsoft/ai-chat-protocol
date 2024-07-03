from enum import Enum
from typing import Any, Optional

from pydantic import BaseModel, ConfigDict, Field
from pydantic.alias_generators import to_camel


class ChatModel(BaseModel):
    model_config = ConfigDict(
        alias_generator=to_camel,
        populate_by_name=True,
        from_attributes=True,
    )


class AIChatRole(str, Enum):
    USER = "user"
    ASSISTANT = "assistant"
    SYSTEM = "system"


class AIChatFile(ChatModel):
    content_type: str = Field(serialization_alias="contentType")
    data: bytes


class AIChatMessage(ChatModel):
    role: AIChatRole
    content: str
    context: Optional[dict[str, Any]] = None
    files: Optional[list[AIChatFile]] = None


class AIChatMessageDelta(ChatModel):
    role: Optional[AIChatRole] = None
    content: Optional[str] = None
    context: Optional[dict[str, Any]] = None


class AIChatCompletion(ChatModel):
    message: AIChatMessage
    session_state: Optional[Any] = Field(serialization_alias="sessionState", default=None)
    context: Optional[dict[str, Any]] = None


class AIChatCompletionDelta(ChatModel):
    delta: AIChatMessageDelta
    session_state: Optional[Any] = Field(serialization_alias="sessionState", default=None)
    context: Optional[dict[str, Any]] = None


class AIChatCompletionOptions(ChatModel):
    context: Optional[dict[str, Any]] = None
    session_state: Optional[Any] = Field(serialization_alias="sessionState", default=None)


class AIChatError(ChatModel):
    code: str
    message: str


class AIChatErrorResponse(ChatModel):
    error: AIChatError


class AIChatRequest(ChatModel):
    messages: list[AIChatMessage]
    session_state: Optional[Any] = Field(serialization_alias="sessionState", default=None)
    context: Optional[bytes] = None
