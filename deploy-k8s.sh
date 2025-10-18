#!/bin/bash

# Kubernetes Deployment Script for Travel Portal
# This script deploys all services to Kubernetes cluster

set -e

echo "🚀 Deploying Travel Portal to Kubernetes..."

# Create namespace
kubectl create namespace travel-portal --dry-run=client -o yaml | kubectl apply -f -

# Create secrets
kubectl create secret generic travel-secrets \
  --from-literal=postgres-password=postgres \
  --from-literal=rabbitmq-password=guest \
  --from-literal=jwt-secret=your-super-secret-key-at-least-32-characters-long-change-in-production \
  --namespace=travel-portal \
  --dry-run=client -o yaml | kubectl apply -f -

# Deploy infrastructure services
echo "📦 Deploying infrastructure..."
kubectl apply -f k8s/infrastructure/ -n travel-portal

# Wait for infrastructure
echo "⏳ Waiting for infrastructure to be ready..."
kubectl wait --for=condition=ready pod -l app=postgres -n travel-portal --timeout=180s
kubectl wait --for=condition=ready pod -l app=rabbitmq -n travel-portal --timeout=180s
kubectl wait --for=condition=ready pod -l app=redis -n travel-portal --timeout=180s

# Deploy services
echo "🔧 Deploying microservices..."
kubectl apply -f k8s/services/ -n travel-portal

# Wait for services
echo "⏳ Waiting for services to be ready..."
kubectl wait --for=condition=ready pod -l app=identityservice -n travel-portal --timeout=300s
kubectl wait --for=condition=ready pod -l app=tenantservice -n travel-portal --timeout=300s

# Deploy API Gateway
echo "🌐 Deploying API Gateway..."
kubectl apply -f k8s/gateway/ -n travel-portal

echo "✅ Deployment complete!"
echo ""
echo "📊 Service Status:"
kubectl get pods -n travel-portal
echo ""
echo "🌍 External Services:"
kubectl get services -n travel-portal | grep LoadBalancer

echo ""
echo "🎉 Travel Portal is now running on Kubernetes!"
echo "Access the API Gateway: kubectl port-forward -n travel-portal svc/apigateway 8080:80"

