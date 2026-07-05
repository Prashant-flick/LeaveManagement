#!/bin/bash

# =====================================================================
# Leave Management System - Azure Infrastructure Setup Script
# =====================================================================
# This script provisions an Azure Container Registry (ACR), an Azure
# Kubernetes Service (AKS) cluster, links them together, and installs
# the NGINX Ingress Controller (API Gateway).
# =====================================================================

set -e

# Configuration variables
RESOURCE_GROUP="LeaveManagementIndiaRG"
LOCATION="centralindia" # Pune, India (closest data center to Delhi for lowest latency)
AKS_CLUSTER_NAME="leave-aks"
VM_SIZE="Standard_D2s_v3" # Allowed VM size with 10 vCPU quota in centralindia (2 vCPU, 8GB RAM, ~$0.096/hour)

# Use your existing globally unique ACR name
ACR_NAME="leaveacrprashant"

echo "========================================="
echo "Starting Azure Infrastructure Provisioning"
echo "ACR Name: ${ACR_NAME}"
echo "AKS Cluster: ${AKS_CLUSTER_NAME} (${VM_SIZE})"
echo "Location: ${LOCATION}"
echo "========================================="

# 1. Create Resource Group
echo -e "\n--> 1. Creating Azure Resource Group..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION"

# 2. Create Azure Container Registry
echo -e "\n--> 2. Creating Azure Container Registry (Basic Tier)..."
az acr create --resource-group "$RESOURCE_GROUP" --name "$ACR_NAME" --sku Basic

# 3. Create AKS Cluster & attach ACR
echo -e "\n--> 3. Creating single-node AKS Cluster (This can take 5-10 minutes)..."
az aks create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$AKS_CLUSTER_NAME" \
  --node-count 1 \
  --node-vm-size "$VM_SIZE" \
  --generate-ssh-keys \
  --attach-acr "$ACR_NAME"

# 4. Fetch AKS Cluster Credentials
echo -e "\n--> 4. Configuring local Kubernetes access (kubectl)..."
az aks get-credentials --resource-group "$RESOURCE_GROUP" --name "$AKS_CLUSTER_NAME" --overwrite-existing

# 5. Install NGINX Ingress Controller (API Gateway)
echo -e "\n--> 5. Installing NGINX Ingress Controller..."
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml

echo "========================================="
echo "Azure Infrastructure Setup Complete!"
echo "Next Steps:"
echo "1. Run: kubectl get service -w ingress-nginx-controller -n ingress-nginx"
echo "   (Wait until the EXTERNAL-IP is allocated. This is your API Gateway IP!)"
echo "2. Use this ACR name in your GitHub deployment workflow: ${ACR_NAME}"
echo "========================================="
