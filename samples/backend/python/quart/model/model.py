from dataclasses import dataclass
from enum import Enum
from typing import Any, Dict, List, Optional

from .model_utils import rename, serializable


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
    role: AIChatRole
    content: str
    context: Optional[Dict[str, Any]] = None
    files: Optional[List[AIChatFile]] = None


@serializable
@dataclass
class AIChatMessageDelta:
    role: Optional[AIChatRole] = None
    content: Optional[str] = None
    context: Optional[Dict[str, Any]] = None


@serializable
@dataclass
@rename("session_state", "sessionState")
class AIChatCompletion:
    message: AIChatMessage
    session_state: Optional[Any] = None
    context: Optional[Dict[str, Any]] = None


@serializable
@dataclass
@rename("session_state", "sessionState")
class AIChatCompletionDelta:
    delta: AIChatMessageDelta
    session_state: Optional[Any] = None
    context: Optional[Dict[str, Any]] = None


@serializable
@dataclass
@rename("session_state", "sessionState")
class AIChatCompletionOptions:
    context: Optional[Dict[str, Any]] = None
    session_state: Optional[Any] = None


@serializable
@dataclass
class AIChatError:
    code: str
    message: str


@serializable
@dataclass
class AIChatErrorResponse:
    error: AIChatError


@serializable
@dataclass
@rename("session_state", "sessionState")
class AIChatRequest:
    messages: List[AIChatMessage]
    session_state: Optional[Any] = None
    context: Optional[bytes] = None
