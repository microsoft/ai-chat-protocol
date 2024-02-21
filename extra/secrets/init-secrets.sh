#!/bin/bash

echo "Initializing secrets..."
echo "- Azure OpenAI"
read -p "  Please enter your key: " AZURE_OPENAI_KEY
read -p "  Please enter your endpoint: " AZURE_OPENAI_ENDPOINT
read -p "  Please enter your deployment: " AZURE_OPENAI_DEPLOYMENT
echo ""
echo "- Azure Model as a Service"
read -p "  Please enter your key: " AZURE_MAAS_KEY
read -p "  Please enter your endpoint: " AZURE_MAAS_ENDPOINT
echo ""
echo "- Azure MaaP LLama2"
read -p "  Please enter your key: " AZURE_MAAP_LLAMA2_KEY
read -p "  Please enter your endpoint: " AZURE_MAAP_LLAMA2_ENDPOINT
read -p "  Please enter your deployment: " AZURE_MAAP_LLAMA2_DEPLOYMENT

SECRETS_DIR=$(dirname $(readlink -f $0))
echo "$AZURE_OPENAI_KEY" > $SECRETS_DIR/azure_openai_key
echo "$AZURE_OPENAI_ENDPOINT" > $SECRETS_DIR/azure_openai_endpoint
echo "$AZURE_OPENAI_DEPLOYMENT" > $SECRETS_DIR/azure_openai_deployment
echo "$AZURE_MAAS_KEY" > $SECRETS_DIR/maas_key
echo "$AZURE_MAAS_ENDPOINT" > $SECRETS_DIR/maas_endpoint
echo "$AZURE_MAAP_LLAMA2_KEY" > $SECRETS_DIR/llama2_maap_key
echo "$AZURE_MAAP_LLAMA2_ENDPOINT" > $SECRETS_DIR/llama2_maap_endpoint
echo "$AZURE_MAAP_LLAMA2_DEPLOYMENT" > $SECRETS_DIR/llama2_maap_deployment
