#!/bin/bash

file_env() {
	local var="$1"
	local fileVar="${var}_FILE"
	local def="${2:-}"
	if [ "${!var:-}" ] && [ "${!fileVar:-}" ]; then
		mysql_error "Both $var and $fileVar are set (but are exclusive)"
	fi
	local val="$def"
	if [ "${!var:-}" ]; then
		val="${!var}"
	elif [ "${!fileVar:-}" ]; then
		val="$(< "${!fileVar}")"
	fi
	export "$var"="$val"
	unset "$fileVar"
}

target=$1

file_env SAMPLE_CHAT_SERVICE_AZURE_OPENAI_KEY
file_env SAMPLE_CHAT_SERVICE_AZURE_OPENAI_ENDPOINT
file_env SAMPLE_CHAT_SERVICE_AZURE_OPENAI_DEPLOYMENT
file_env SAMPLE_CHAT_SERVICE_MAAS_KEY
file_env SAMPLE_CHAT_SERVICE_MAAS_ENDPOINT
file_env SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_KEY
file_env SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_ENDPOINT
file_env SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_DEPLOYMENT

dotnet $target
